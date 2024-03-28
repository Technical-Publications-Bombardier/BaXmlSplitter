using System.Diagnostics;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using ManualState = TechPubsDatabase.Models.CsdbContext.ManualState;

namespace TechPubsDatabase.Data;

/// <summary>
///     Build the child-parent relationship between units-of-work.
/// </summary>
public class UowRelationshipBuilder(ManualContext context)
{
    /// <summary>
    /// Checks whether the unit-of-work <paramref name="uow"/> is within the publication state <paramref name="state"/>.
    /// </summary>
    /// <param name="uow">The unit-of-work as a row in the <c>OBJECTNEW</c> table.</param>
    /// <param name="state">The selected state.</param>
    /// <returns><c>True</c> if the unit-of-work <paramref name="uow"/> is within the publication state <paramref name="state"/>, <c>False</c> otherwise.</returns>
    private static bool UowHasPubState(ObjectNew uow, ManualState state)
    {
        return state switch
        {
            ManualState.Official => uow.CurrentState == 2,
            ManualState.WorkInProgress => true,
            ManualState.None => false,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state,
                "No units-of-work would be selected by this state")
        };
    }

    /// <summary>
    ///     Constructs an <see cref="XmlDocument"/> to represent the states for each unit-of-work in the manual.
    /// </summary>
    /// <param name="rootName">The name of the root node.</param>
    /// <param name="rootObjectId">The root object reference.</param>
    /// <param name="pubState">The desired publication status of the manual</param>
    /// <param name="token">The <see cref="CancellationToken"/> token</param>
    /// <returns>An <see cref="XmlDocument"/> to represent the states for each unit-of-work in the manual</returns>
    public async Task<XmlDocument> ConstructXml(string rootName, long rootObjectId, ManualState pubState = ManualState.Official, CancellationToken token = default)
    {
        var xmlDoc = new XmlDocument();
        var rootNode = xmlDoc.CreateElement(rootName);
        xmlDoc.AppendChild(rootNode);

        await AppendChildrenAsync(rootNode, rootObjectId, pubState, token);

        return xmlDoc;
    }

    /// <summary>
    ///     Appends the children from <see cref="ObjectNew" /> by looking up the object ref.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <param name="parentObjectId">The parent object reference.</param>
    /// <param name="pubState">The desired publication status of the manual</param>
    /// <param name="token">The <see cref="CancellationToken"/> token</param>
    private async Task AppendChildrenAsync(XmlNode parentNode, long parentObjectId, ManualState pubState, CancellationToken token = default)
    {
        if (context.ObjectNew == null || pubState == ManualState.None) return;
        var children = await context.ObjectNew
            .Include(o => o.ObjectAttributes)
            .Include(o => o.State)
            .Where(o => o.ParentObjectId == parentObjectId && o.CurrentState != null && o.CurrentState != 3 /* Filter out OBSOLETE */)
            .ToListAsync(cancellationToken: token);
        children = children.Where(o => UowHasPubState(o, pubState)).ToList();
        Debug.WriteLineIf(children is { Count: > 0 }, $"There were no children for object ref {parentObjectId}",
            "Information");
        var childrenAtRevision = children.GroupBy(o => o.ObjectId).Select(group => new
        {
            ObjectId = group.Key,
            LatestRevision = group.OrderByDescending(o => o.ValidTime)
                .ThenByDescending(o => o.ObjectRef) // Use ObjectRef as a tie-breaker
                .FirstOrDefault()
        }).Where(g => g.LatestRevision != null);
        foreach (var latestRevision in childrenAtRevision)
        {
            var attributesAtRevision = latestRevision.LatestRevision;
            if (attributesAtRevision == null)
                continue;
            var groupObjectId = attributesAtRevision.ObjectId;
            var groupObjectName = attributesAtRevision.ObjectName;
            var groupCurrentState = attributesAtRevision.CurrentState;

            if (string.IsNullOrEmpty(groupObjectName) || parentNode.OwnerDocument?.CreateElement(groupObjectName) is not
                    { } childNode)
                continue;
            parentNode.AppendChild(childNode);

            // Add the state name and value
            if (groupCurrentState.HasValue)
            {
                var state = childNode.OwnerDocument.CreateAttribute(nameof(ObjectNew.CurrentState));
                state.Value = attributesAtRevision.CurrentState.ToString();
                childNode.Attributes.Append(state);
            }

            // Add the attributes
            foreach (var uowAttribute in attributesAtRevision.ObjectAttributes?.Where(oa =>
                         !string.IsNullOrEmpty(oa.AttributeName)) ?? [])
            {
                var attribute = childNode.OwnerDocument.CreateAttribute(uowAttribute.AttributeName!);
                attribute.Value = uowAttribute.AttributeValue;
                childNode.Attributes.Append(attribute);
            }

            if (groupObjectId != -1)
                await AppendChildrenAsync(childNode, parentObjectId: groupObjectId, pubState, token);
        }
    }
}
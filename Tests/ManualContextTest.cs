using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TechPubsDatabase.Data;
using TechPubsDatabase.Models;

namespace Tests;

public class ManualContextTest
{
    private static readonly IConfiguration Configuration =
        new ConfigurationBuilder().AddUserSecrets<ManualContextTest>().Build();

    private static readonly string? BaOraConnectionString = Configuration.GetConnectionString("BaOraConnectionString");

    // Arrange
    private static readonly DbContextOptions<ManualContext> InMemoryOptions =
        new DbContextOptionsBuilder<ManualContext>()
            .UseInMemoryDatabase("TestDatabase") // Use in-memory database for testing
            .Options;

    [Fact]
    public void ConnectionStringTest()
    {
        Assert.False(string.IsNullOrEmpty(BaOraConnectionString));
        Assert.Equal("BBB59842DBE8ABBB1F0B2C249C4D1EBC",
            Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(BaOraConnectionString))));
    }

    [Fact]
    public async Task AnchorTableTest()
    {
        // Insert seed data into the database using one instance of the context
        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            context.Anchor?.Add(new Anchor
            {
                AnchorRef = 1, ManualObjectRef = 1, SeqNo = 1, ObjectPath = "Test", ObjectRef = 1, Key = "Test",
                Chg = 'A',
                RevDate = "20010319"
            });
            await context.SaveChangesAsync();
        }

        // Act
        // Use a clean instance of the context to run the test
        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            if (context.Anchor != null)
            {
                var firstAnchor = await context.Anchor.FirstAsync();

                // Assert
                Assert.NotNull(firstAnchor);
                Assert.Equal(1, firstAnchor.AnchorRef);
            }
        }
    }

    [Fact]
    public async Task DocumentTableTest()
    {
        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            context.Document?.Add(new Document
            {
                DocumentRef = 1,
                ObjectPath = "Test",
                ObjectRef = 1
            });
            await context.SaveChangesAsync();
        }

        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            if (context.Document != null)
            {
                var firstDocument = await context.Document.FirstAsync();
                Assert.NotNull(firstDocument);
                Assert.Equal(1, firstDocument.DocumentRef);
                Assert.Equal("Test", firstDocument.ObjectPath);
                Assert.Equal(1, firstDocument.ObjectRef);
            }
        }
    }

    [Fact]
    public async Task ObjectAttributeTableTest()
    {
        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            context.ObjectAttribute?.Add(new ObjectAttribute
            {
                ObjectRef = 1,
                AttributeName = "TestAttribute",
                AttributeValue = "TestValue"
            });
            await context.SaveChangesAsync();
        }

        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            if (context.ObjectAttribute != null)
            {
                var firstObjectAttribute = await context.ObjectAttribute.FirstAsync();
                Assert.NotNull(firstObjectAttribute);
                Assert.Equal(1, firstObjectAttribute.ObjectRef);
                Assert.Equal("TestAttribute", firstObjectAttribute.AttributeName);
                Assert.Equal("TestValue", firstObjectAttribute.AttributeValue);
            }
        }
    }

    [Fact]
    public async Task ObjectNewTableTest()
    {
        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            context.ObjectNew?.Add(new ObjectNew
            {
                ObjectRef = 1,
                ObjectId = 1,
                CreateTime = DateTime.Now,
                UserRef = 1,
                ModifyTime = DateTime.Now,
                ValidTime = DateTime.Now,
                ObsoleteTime = DateTime.Now,
                OfflineTime = DateTime.Now,
                ObjectType = false
            });
            await context.SaveChangesAsync();
        }

        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            if (context.ObjectNew != null)
            {
                var firstObjectNew = await context.ObjectNew.FirstAsync();
                Assert.NotNull(firstObjectNew);
                Assert.Equal(1, firstObjectNew.ObjectRef);
                Assert.Equal(1, firstObjectNew.ObjectId);
                // Add additional assertions for other properties as needed
            }
        }
    }

    [Fact]
    public async Task ConstructXml_WithValidRootObjectRef_ReturnsExpectedXmlDocument()
    {
        // Arrange
        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            Assert.NotNull(context);
            Assert.NotNull(context.ObjectNew);
            // Seed the database with necessary data
            context.ObjectNew.Add(new ObjectNew
            {
                ObjectRef = 1,
                ParentObjectId = null
                // Add other properties as needed
            });

            context.ObjectNew.Add(new ObjectNew
            {
                ObjectRef = 2,
                ParentObjectId = 1
                // Add other properties as needed
            });

            await context.SaveChangesAsync();
        }

        // Act
        await using (var context =
                     new ManualContext(InMemoryOptions, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            var builder = new UowRelationshipBuilder(context);
            var xmlDocument = await builder.ConstructXml("test", 1);

            // Assert
            Assert.NotNull(xmlDocument);
            Assert.Equal("test", xmlDocument.DocumentElement?.Name);
            // Add other assertions to verify the structure and content of the generated XML
        }
    }

    [Fact]
    public async Task ConstructXml_WithIfmGvx_ReturnsExpectedXmlDocument()
    {
        // Arrange
        const string docnbr = "IFM_GVXER";
        ManualMetadata? ifmGvxerMetadata;
        await using (var context = new ManualContext(CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            var cancellationSource = new CancellationTokenSource();
            Assert.NotNull(context);
            Assert.True(await context.Database.CanConnectAsync(cancellationSource.Token));
            // Get the manual state

            ifmGvxerMetadata = (await context.GetManualMetaDataAsync([docnbr], cancellationSource.Token)).FirstOrDefault();
            Assert.NotNull(ifmGvxerMetadata);
            foreach (var property in typeof(ManualMetadata).GetProperties())
                Assert.NotNull(property.GetValue(ifmGvxerMetadata));
        }

        // Act
        await using (var context = new ManualContext(CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            var builder = new UowRelationshipBuilder(context);
            var xmlDocument = await builder.ConstructXml(docnbr, ifmGvxerMetadata.ParentObjectId);

            // Assert
            Assert.NotNull(xmlDocument);
            var chapters = xmlDocument.SelectNodes("/IFM_GVXER/IFM/CHAPTER");
            Assert.NotNull(chapters);
            foreach (var chapter in chapters.Cast<XmlNode>())
            {
                var cfmatr = chapter.SelectNodes("CFMATR");
                Assert.NotNull(cfmatr);
                Assert.Single(cfmatr);
            }

            Assert.Equal(docnbr, xmlDocument.DocumentElement?.Name);
            xmlDocument.Save($"{docnbr}.xml");
        }
    }

    [Fact]
    public async Task ConstructXml_WithAmm_ReturnsExpectedXmlDocument()
    {
        // Arrange
        const string docnbr = "AMM";
        ManualMetadata? ammMetadata;
        var cancellationSource = new CancellationTokenSource();
        await using (var context = new ManualContext(CsdbContext.CsdbProgram.CTALPROD, BaOraConnectionString!))
        {
            Assert.NotNull(context);
            Assert.True(await context.Database.CanConnectAsync(cancellationSource.Token));
            // Get the manual state

            ammMetadata = (await context.GetManualMetaDataAsync([docnbr], cancellationSource.Token)).FirstOrDefault();
            Assert.NotNull(ammMetadata);
            foreach (var property in typeof(ManualMetadata).GetProperties()
                         .Where(p => p.Name != nameof(ManualMetadata.Manual)))
                Assert.NotNull(property.GetValue(ammMetadata));
        }

        // Act
        await using (var context = new ManualContext(CsdbContext.CsdbProgram.CTALPROD, BaOraConnectionString!))
        {
            var builder = new UowRelationshipBuilder(context);
            var xmlDocument =
                await builder.ConstructXml(docnbr, ammMetadata.ParentObjectId, token: cancellationSource.Token);

            // Assert
            Assert.NotNull(xmlDocument);
            var chapters = xmlDocument.SelectNodes("/AMM/AMM/CHAPTER");
            Assert.NotNull(chapters);
            foreach (var chapter in chapters.Cast<XmlNode>())
            {
                var cfmatr = chapter.SelectNodes("CFMATR");
                Assert.NotNull(cfmatr);
                Assert.Single(cfmatr);
            }

            Assert.Equal(docnbr, xmlDocument.DocumentElement?.Name);
            xmlDocument.Save($"{docnbr}.xml");
        }
    }

    [Fact]
    public async Task ConstructXml_WithAmmWip_ReturnsExpectedXmlDocument()
    {
        // Arrange
        const string docnbr = "AMM";
        ManualMetadata? ammMetadata;
        var cancellationSource = new CancellationTokenSource();
        await using (var context = new ManualContext(CsdbContext.CsdbProgram.CTALPROD, BaOraConnectionString!))
        {
            Assert.NotNull(context);
            Assert.True(await context.Database.CanConnectAsync(cancellationSource.Token));
            // Get the manual state

            ammMetadata = (await context.GetManualMetaDataAsync([docnbr], cancellationSource.Token)).FirstOrDefault();
            Assert.NotNull(ammMetadata);
            foreach (var property in typeof(ManualMetadata).GetProperties()
                         .Where(p => p.Name != nameof(ManualMetadata.Manual)))
                Assert.NotNull(property.GetValue(ammMetadata));
        }

        // Act
        await using (var context = new ManualContext(CsdbContext.CsdbProgram.CTALPROD, BaOraConnectionString!))
        {
            var builder = new UowRelationshipBuilder(context);
            var xmlDocument = await builder.ConstructXml(docnbr, ammMetadata.ParentObjectId,
                CsdbContext.ManualState.WorkInProgress, cancellationSource.Token);

            // Assert
            Assert.NotNull(xmlDocument);
            var chapters = xmlDocument.SelectNodes("/AMM/AMM/CHAPTER");
            Assert.NotNull(chapters);
            foreach (var chapter in chapters.Cast<XmlNode>())
            {
                var cfmatr = chapter.SelectNodes("CFMATR");
                Assert.NotNull(cfmatr);
                Assert.Single(cfmatr);
            }

            Assert.Equal(docnbr, xmlDocument.DocumentElement?.Name);
            xmlDocument.Save($"{docnbr}-{nameof(CsdbContext.ManualState.WorkInProgress)}.xml");
        }
    }
}
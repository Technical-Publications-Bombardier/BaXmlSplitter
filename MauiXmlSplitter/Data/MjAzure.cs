using System.Text.Json.Serialization;

namespace MauiXmlSplitter.Data;

/// <summary>
/// The Azure Application Insights details
/// </summary>
internal record AzureResource
{
    /// <summary>
    /// Gets the universal resource identifier relative to https://management.azure.com/.
    /// </summary>
    /// <value>
    /// The Azure universal resource identifier.
    /// </value>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the resource name.
    /// </summary>
    /// <value>
    /// The Azure resource name.
    /// </value>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the resource type.
    /// </summary>
    /// <value>
    /// The Azure resource type.
    /// </value>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Gets the resource location.
    /// </summary>
    /// <value>
    /// The Azure resource location.
    /// </value>
    [JsonPropertyName("location")]
    public required string Location { get; init; }

    /// <summary>
    /// Gets the resource tags.
    /// </summary>
    /// <value>
    /// The Azure resource tags.
    /// </value>
    [JsonPropertyName("tags")]
    public required Dictionary<string, string> Tags { get; init; }

    /// <summary>
    /// Gets the kind of the resource.
    /// </summary>
    /// <value>
    /// The kind of the Azure resource.
    /// </value>
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    /// <summary>
    /// Gets the resource etag.
    /// </summary>
    /// <value>
    /// The Azure resource etag.
    /// </value>
    [JsonPropertyName("etag")]
    public required string Etag { get; init; }

    /// <summary>
    /// Gets the properties.
    /// </summary>
    /// <value>
    /// The properties.
    /// </value>
    [JsonPropertyName("properties")]
    public required ResourceProperties Properties { get; init; }
}

/// <summary>
/// Azure Application Insights properties.
/// </summary>
internal record ResourceProperties
{
    /// <summary>
    /// Gets the human-readable remote application identifier.
    /// </summary>
    /// <value>
    /// The application identifier.
    /// </value>
    [JsonPropertyName("ApplicationId")]
    public required string ApplicationId { get; init; }

    /// <summary>
    /// Gets the machine-readable remote application identifier.
    /// </summary>
    /// <value>
    /// The application identifier as <see cref="Guid"/>.
    /// </value>
    [JsonPropertyName("AppId")]
    public Guid AppId { get; init; }

    /// <summary>
    /// Gets the type of the remote application.
    /// </summary>
    /// <value>
    /// The type of the application.
    /// </value>
    [JsonPropertyName("Application_Type")]
    public required string ApplicationType { get; init; }

    /// <summary>
    /// Gets the type of the flow.
    /// </summary>
    /// <value>
    /// The type of the flow.
    /// </value>
    [JsonPropertyName("Flow_Type")]
    public required string FlowType { get; init; }

    /// <summary>
    /// Gets the request source.
    /// </summary>
    /// <value>
    /// The request source.
    /// </value>
    [JsonPropertyName("Request_Source")]
    public required string RequestSource { get; init; }

    /// <summary>
    /// Gets the instrumentation key.
    /// </summary>
    /// <value>
    /// The instrumentation key.
    /// </value>
    [JsonPropertyName("InstrumentationKey")]
    public Guid InstrumentationKey { get; init; }

    /// <summary>
    /// Gets the connection string.
    /// </summary>
    /// <value>
    /// The connection string.
    /// </value>
    [JsonPropertyName("ConnectionString")]
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the resource creation date.
    /// </summary>
    /// <value>
    /// The creation date.
    /// </value>
    [JsonPropertyName("CreationDate")]
    public DateTime CreationDate { get; init; }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    /// <value>
    /// The tenant identifier.
    /// </value>
    [JsonPropertyName("TenantId")]
    public Guid TenantId { get; init; }

    /// <summary>
    /// Gets the provisioning state.
    /// </summary>
    /// <value>
    /// The provisioning state.
    /// </value>
    [JsonPropertyName("provisioningState")]
    public required string ProvisioningState { get; init; }

    /// <summary>
    /// Gets the sampling percentage.
    /// </summary>
    /// <value>
    /// The sampling percentage.
    /// </value>
    [JsonPropertyName("SamplingPercentage")]
    public double? SamplingPercentage { get; init; }

    /// <summary>
    /// Gets the retention duration in days.
    /// </summary>
    /// <value>
    /// The retention duration in days.
    /// </value>
    [JsonPropertyName("RetentionInDays")]
    public int RetentionInDays { get; init; }

    /// <summary>
    /// Gets the workspace resource identifier relative to https://management.azure.com/.
    /// </summary>
    /// <value>
    /// The workspace resource identifier.
    /// </value>
    [JsonPropertyName("WorkspaceResourceId")]
    public required string WorkspaceResourceId { get; init; }

    /// <summary>
    /// Gets the ingestion mode.
    /// </summary>
    /// <value>
    /// The ingestion mode.
    /// </value>
    [JsonPropertyName("IngestionMode")]
    public required string IngestionMode { get; init; }

    /// <summary>
    /// Gets or sets the public network access for ingestion.
    /// </summary>
    /// <value>
    /// The public network access for ingestion.
    /// </value>
    [JsonPropertyName("publicNetworkAccessForIngestion")]
    public required string PublicNetworkAccessForIngestion { get; init; }

    /// <summary>
    /// Gets or sets the public network access for query.
    /// </summary>
    /// <value>
    /// The public network access for query.
    /// </value>
    [JsonPropertyName("publicNetworkAccessForQuery")]
    public required string PublicNetworkAccessForQuery { get; init; }

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    /// <value>
    /// The version.
    /// </value>
    [JsonPropertyName("Ver")]
    public required string Version { get; init; }
}
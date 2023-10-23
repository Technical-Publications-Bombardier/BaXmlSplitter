using Microsoft.EntityFrameworkCore;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaXmlSplitter.Resources;
using Google.Protobuf.WellKnownTypes;
using CsdbProgram = BaXmlSplitter.XmlSplitterHelpers.CsdbProgram;

namespace BaXmlSplitter
{
    internal class Remote
    {
        /// <summary>
        /// The Azure Application Insights properties
        /// </summary>
        internal static readonly AzureResource? ApplicationInsights = JsonSerializer.Deserialize<AzureResource>(Properties.Resources.ApplicationInsights);
        internal class ManualContext(CsdbProgram program) : DbContext
        {
            private readonly string baOraConnectionString = Settings.Default.BaOraConnectionString;
            private readonly string serviceName = ServiceNameTable[program];
            private static readonly Dictionary<CsdbProgram, string> ServiceNameTable = new() {
                { CsdbProgram.B_IFM , "BIFM" },
                { CsdbProgram.CTALPROD , "CTALPROD" },
                { CsdbProgram.CH604PROD, "CH604PRD" },
                { CsdbProgram.LJ4045PROD, "LJ4045P" },
                { CsdbProgram.GXPROD, "GXPROD" }
            };

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                _ = optionsBuilder.UseOracle($"{baOraConnectionString}/{serviceName}");
                base.OnConfiguring(optionsBuilder);
            }
        }
        class UnitOfWork
        {
        }

        internal class HashiCorpContext(string clientSecret) : AuthenticatorBase(string.Empty)
        {
            private DateTime Expiration { get; set; } = DateTime.MinValue;

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
            public string Id { get; init; }

            /// <summary>
            /// Gets the resource name.
            /// </summary>
            /// <value>
            /// The Azure resource name.
            /// </value>
            [JsonPropertyName("name")]
            public string Name { get; init; }

            /// <summary>
            /// Gets the resource type.
            /// </summary>
            /// <value>
            /// The Azure resource type.
            /// </value>
            [JsonPropertyName("type")]
            public string Type { get; init; }

            /// <summary>
            /// Gets the resource location.
            /// </summary>
            /// <value>
            /// The Azure resource location.
            /// </value>
            [JsonPropertyName("location")]
            public string Location { get; init; }

            /// <summary>
            /// Gets the resource tags.
            /// </summary>
            /// <value>
            /// The Azure resource tags.
            /// </value>
            [JsonPropertyName("tags")]
            public Dictionary<string, string> Tags { get; init; }

            /// <summary>
            /// Gets the kind of the resource.
            /// </summary>
            /// <value>
            /// The kind of the Azure resource.
            /// </value>
            [JsonPropertyName("kind")]
            public string Kind { get; init; }

            /// <summary>
            /// Gets the resource etag.
            /// </summary>
            /// <value>
            /// The Azure resource etag.
            /// </value>
            [JsonPropertyName("etag")]
            public string Etag { get; init; }

            /// <summary>
            /// Gets the properties.
            /// </summary>
            /// <value>
            /// The properties.
            /// </value>
            [JsonPropertyName("properties")]
            public ResourceProperties Properties { get; init; }
                }
                var options = new RestClientOptions("https://auth.hashicorp.com");
                using var client = new RestClient(options);
                var request = new RestRequest("oauth/token").AddParameter("audience", "https://api.hashicorp.cloud").AddParameter("grant_type", "client_credentials").AddParameter("client_id", hashiCorpIdentity.ClientId).AddParameter("client_secret", clientSecret);
                return await client.PostAsync<TokenResponse>(request).ConfigureAwait(false);
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
            public string ApplicationId { get; init; }

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
            public string ApplicationType { get; init; }

                /// <summary>
            /// Gets the type of the flow.
                /// </summary>
            /// <value>
            /// The type of the flow.
            /// </value>
            [JsonPropertyName("Flow_Type")]
            public string FlowType { get; init; }

            /// <summary>
            /// Gets the request source.
            /// </summary>
            /// <value>
            /// The request source.
            /// </value>
            [JsonPropertyName("Request_Source")]
            public string RequestSource { get; init; }

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
            public string ConnectionString { get; init; }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            [JsonPropertyName("Name")]
            public string Name { get; init; }

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
            public string ProvisioningState { get; init; }

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
            public string WorkspaceResourceId { get; init; }

            /// <summary>
            /// Gets the ingestion mode.
            /// </summary>
            /// <value>
            /// The ingestion mode.
            /// </value>
            [JsonPropertyName("IngestionMode")]
            public string IngestionMode { get; init; }

            /// <summary>
            /// Gets or sets the public network access for ingestion.
            /// </summary>
            /// <value>
            /// The public network access for ingestion.
            /// </value>
            [JsonPropertyName("publicNetworkAccessForIngestion")]
            public string PublicNetworkAccessForIngestion { get; init; }

            /// <summary>
            /// Gets or sets the public network access for query.
            /// </summary>
            /// <value>
            /// The public network access for query.
            /// </value>
            [JsonPropertyName("publicNetworkAccessForQuery")]
            public string PublicNetworkAccessForQuery { get; init; }

            /// <summary>
            /// Gets or sets the version.
            /// </summary>
            /// <value>
            /// The version.
            /// </value>
            [JsonPropertyName("Ver")]
            public string Version { get; init; }
        }

        class UnitOfWork
                {
                }
        /// <summary>
        /// HashiCorp <see cref="RestSharp"/> client for retrieving secrets from HashiCorp Cloud Platform Vault Secrets.
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        /// <seealso href="https://developer.hashicorp.com/hcp/api-docs/vault-secrets"/>
        internal class HashiCorpClient : IDisposable
        {
            /// <summary>
            /// The HashiCorp identity
            /// </summary>
            private readonly HashiCorpIdentity identity = JsonSerializer.Deserialize<HashiCorpIdentity>(utf8Json: Properties.Resources.HashiCorpIdentity) ?? throw new ArgumentNullException();
            /// <summary>
            /// The internal <see cref="RestClient"/> client for receiving the HashiCorp secrets
            /// </summary>
            private readonly RestClient hashiCorpClient;

                /// <summary>
                /// Initializes a new instance of the <see cref="TokenAcquisitionException" /> class.
                /// </summary>
                /// <param name="specificMessage">The specific message.</param>
                /// <param name="innerException">The inner exception.</param>
                public TokenAcquisitionException(string specificMessage, Exception innerException) : base($"{BaseMessage}: {specificMessage}", innerException)
                {
                }
            }

            /// <summary>
            /// Deserializes the <see href="https://developer.hashicorp.com/hcp/api-docs/vault-secrets#OpenAppSecret"><c>OpenAppSecret</c></see>.
            /// </summary>
            public record OpenAppSecret
            {
                /// <summary>
                /// Gets the name.
                /// </summary>
                /// <value>
                /// The name.
                /// </value>
                public required string Name { get; init; }
                /// <summary>
                /// Gets the version.
                /// </summary>
                /// <value>
                /// The version.
                /// </value>
                public required VersionInfo Version { get; init; }
                /// <summary>
                /// Gets the created at.
                /// </summary>
                /// <value>
                /// The created at.
                /// </value>
                public required string CreatedAt { get; init; }
                /// <summary>
                /// Gets the latest version.
                /// </summary>
                /// <value>
                /// The latest version.
                /// </value>
                public required string LatestVersion { get; init; }
                /// <summary>
                /// Gets the created by.
                /// </summary>
                /// <value>
                /// The created by.
                /// </value>
                public required CreatedByInfo CreatedBy { get; init; }
                /// <summary>
                /// Gets the synchronize status.
                /// </summary>
                /// <value>
                /// The synchronize status.
                /// </value>
                public required object SyncStatus { get; init; }

                /// <summary>
                /// Secret value with version information
                /// </summary>
                public record VersionInfo
                {
                    /// <summary>
                    /// Gets the version.
                    /// </summary>
                    /// <value>
                    /// The version.
                    /// </value>
                    public required string Version { get; init; }
                    /// <summary>
                    /// Gets the type.
                    /// </summary>
                    /// <value>
                    /// The type.
                    /// </value>
                    public required string Type { get; init; }
                    /// <summary>
                    /// Gets the created at.
                    /// </summary>
                    /// <value>
                    /// The created at.
                    /// </value>
                    public required string CreatedAt { get; init; }
                    /// <summary>
                    /// Gets the value.
                    /// </summary>
                    /// <value>
                    /// The value.
                    /// </value>
                    public required string Value { get; init; }
                    /// <summary>
                    /// Gets the created by.
                    /// </summary>
                    /// <value>
                    /// The created by.
                    /// </value>
                    public required CreatedByInfo CreatedBy { get; init; }
                }

                /// <summary>
                /// 
                /// </summary>
                public record CreatedByInfo
                {
                    /// <summary>
                    /// Gets the name.
                    /// </summary>
                    /// <value>
                    /// The name.
                    /// </value>
                    public required string Name { get; init; }
                    /// <summary>
                    /// Gets the type.
                    /// </summary>
                    /// <value>
                    /// The type.
                    /// </value>
                    public required string Type { get; init; }
                    /// <summary>
                    /// Gets the email.
                    /// </summary>
                    /// <value>
                    /// The email.
                    /// </value>
                    public required string Email { get; init; }
                }
            }


            /// <summary>
            /// The HashiCorp authentication token used to validate the session. The auth is valid for 3600 seconds, or 1 hour.
            /// </summary>
            private record TokenResponse
            {

                /// <summary>
                /// Gets the access token.
                /// </summary>
                /// <value>
                /// The access token.
                /// </value>
                [JsonPropertyName("access_token")]
                public required string AccessToken { get; init; }
                /// <summary>
                /// Gets the expires in.
                /// </summary>
                /// <value>
                /// The expires in.
                /// </value>
                [JsonPropertyName("expires_in")]
                public long ExpiresIn { get; init; }
                /// <summary>
                /// Gets the type of the token.
                /// </summary>
                /// <value>
                /// The type of the token.
                /// </value>
                [JsonPropertyName("token_type")]
                public required string TokenType { get; init; }

                /// <summary>
                /// Gets the expiration date.
                /// </summary>
                /// <param name="baseDateTime">The base date time.</param>
                /// <param name="toleranceInMinutes">The tolerance in minutes.</param>
                /// <returns>The expiration <see cref="DateTime"/> of the token.</returns>
                public DateTime GetExpirationDate(DateTime baseDateTime, int toleranceInMinutes = 1)
                {
                    return baseDateTime.AddSeconds(ExpiresIn).AddMinutes(-toleranceInMinutes);
                }
            }
            /// <summary>
            /// HashiCorp identity structure.
            /// </summary>
            private record HashiCorpIdentity
            {
                /// <summary>
                /// Gets the client identifier.
                /// </summary>
                /// <value>
                /// The client identifier.
                /// </value>
                [JsonPropertyName("client_id")]
                public required string ClientId { get; init; }
                /// <summary>
                /// Gets the analytics Javascript anonymous identifier.
                /// </summary>
                /// <value>
                /// The anonymous analytics id.
                /// </value>
                /// <seealso href="https://github.com/hashicorp/web-platform-packages/blob/main/packages/analytics/analytics-js-helpers.ts#L33">HashiCorp web platform packages <c>getSegmentId()</c></seealso>
                [JsonPropertyName("ajs_aid")]
                public required string AnalyticsJsAnonymousId { get; init; }
                /// <summary>
                /// Gets the user identifier.
                /// </summary>
                /// <value>
                /// The user identifier.
                /// </value>
                [JsonPropertyName("user_id")]
                public required string UserId { get; init; }
                /// <summary>
                /// Gets the organization identifier.
                /// </summary>
                /// <value>
                /// The organization identifier.
                /// </value>
                [JsonPropertyName("org_id")]
                public required string OrgId { get; init; }
                /// <summary>
                /// Gets the project identifier.
                /// </summary>
                /// <value>
                /// The project identifier.
                /// </value>
                [JsonPropertyName("proj_id")]
                public required string ProjectId { get; init; }
                /// <summary>
                /// Gets the name of the application.
                /// </summary>
                /// <value>
                /// The name of the application.
                /// </value>
                [JsonPropertyName("app_name")]
                public required string ApplicationName { get; init; }
            }

            /// <summary>
            /// Default error response from HashiCorp endpoints.
            /// </summary>
            /// <param name="Code"></param>
            public record ErrorResponse(int Code, string Message, Any[] Details);
        }
    }
}

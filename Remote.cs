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

            protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
            {
                // ReSharper disable once InvertIf
                if (string.IsNullOrEmpty(Token) || DateTime.UtcNow >= Expiration)
                {
                    if (await GetToken().ConfigureAwait(false) is not { } tokenResponse)
                    {
                        throw new TokenAcquisitionException("Did not receive HashiCorp auth token");
                    }
                    Token = $"{tokenResponse.TokenType} {tokenResponse.AccessToken}";
                    Expiration = tokenResponse.GetExpirationDate(DateTime.UtcNow);
                }
                return new HeaderParameter(KnownHeaders.Authorization, $"Bearer {Token}");
            }

            /// <summary>
            /// Gets the token.
            /// </summary>
            /// <returns>HashiCorp <see cref="TokenResponse"/> token.</returns>
            private async Task<TokenResponse?> GetToken()
            {
                if (JsonSerializer.Deserialize<HashiCorpIdentity>(utf8Json: Properties.Resources.HashiCorpIdentity) is not { } hashiCorpIdentity)
                {
                    return null;
                }
                var options = new RestClientOptions("https://auth.hashicorp.com");
                using var client = new RestClient(options);
                var request = new RestRequest("oauth/token").AddParameter("audience", "https://api.hashicorp.cloud").AddParameter("grant_type", "client_credentials").AddParameter("client_id", hashiCorpIdentity.ClientId).AddParameter("client_secret", clientSecret);
                return await client.PostAsync<TokenResponse>(request).ConfigureAwait(false);
            }
            /// <summary>
            /// Exception to indicate failed token acquisition.
            /// </summary>
            /// <seealso cref="Exception" />
            public class TokenAcquisitionException : Exception
            {
                /// <summary>
                /// The base message
                /// </summary>
                private const string BaseMessage = "Failed to get token";
                /// <summary>
                /// Initializes a new instance of the <see cref="TokenAcquisitionException" /> class.
                /// </summary>
                public TokenAcquisitionException() : base($"{BaseMessage}.")
                {

                }

                /// <summary>
                /// Initializes a new instance of the <see cref="TokenAcquisitionException" /> class.
                /// </summary>
                /// <param name="specificMessage">The specific message.</param>
                public TokenAcquisitionException(string specificMessage) : base($"{BaseMessage}: {specificMessage}")
                {
                }

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

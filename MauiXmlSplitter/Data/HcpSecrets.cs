using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
using RestSharp;
using RestSharp.Authenticators;
using static BaXmlSplitter.Remote.HashiCorpClient;

namespace BaXmlSplitter;

internal partial class Remote
{
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
        private readonly HashiCorpIdentity identity;

        /// <summary>
        /// The internal <see cref="RestClient"/> client for receiving the HashiCorp secrets
        /// </summary>
        private readonly RestClient hashiCorpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashiCorpClient"/> class.
        /// </summary>
        /// <param name="identity">The <see cref="HashiCorpIdentity"/> identity.</param>
        /// <param name="clientSecret">The client secret.</param>
        internal HashiCorpClient(HashiCorpIdentity identity, string clientSecret)
        {
            this.identity = identity;
            var options =
                new RestClientOptions(
                    $"https://api.cloud.hashicorp.com/secrets/2023-06-13/organizations/{identity.OrganizationId}/projects/{identity.ProjectId}/apps/{identity.ApplicationName}")
                {
                    Authenticator = new HashiCorpContext(clientSecret, identity)
                };
            hashiCorpClient = new RestClient(options);
        }

        /// <summary>
        /// Deserializes the <see href="https://developer.hashicorp.com/hcp/api-docs/vault-secrets#OpenAppSecret"><c>OpenAppSecret</c></see>.
        /// </summary>
        public record OpenAppSecretResponse
        {
            /// <summary>
            /// Gets the secret.
            /// </summary>
            /// <value>
            /// The secret.
            /// </value>
            [JsonPropertyName("secret")]
            public required OpenAppSecret Secret { get; init; }
        }

        /// <summary>
        /// Represents a <see href="https://developer.hashicorp.com/hcp/api-docs/vault-secrets#OpenAppSecret"><c>OpenAppSecret</c></see>.
        /// </summary>
        public record OpenAppSecret
        {
            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            [JsonPropertyName("name")]
            public string Name { get; init; }

            /// <summary>
            /// Gets the version.
            /// </summary>
            /// <value>
            /// The version.
            /// </value>
            [JsonPropertyName("version")]
            public VersionInfo Version { get; init; }

            /// <summary>
            /// Gets the created at.
            /// </summary>
            /// <value>
            /// The created at.
            /// </value>
            [JsonPropertyName("created_at")]
            public string CreatedAt { get; init; }

            /// <summary>
            /// Gets the latest version.
            /// </summary>
            /// <value>
            /// The latest version.
            /// </value>
            [JsonPropertyName("latest_version")]
            public string LatestVersion { get; init; }

            /// <summary>
            /// Gets the created by.
            /// </summary>
            /// <value>
            /// The created by.
            /// </value>
            [JsonPropertyName("created_by")]
            public CreatedByInfo CreatedBy { get; init; }

            /// <summary>
            /// Gets the synchronize status.
            /// </summary>
            /// <value>
            /// The synchronize status.
            /// </value>
            [JsonPropertyName("sync_status")]
            public object SyncStatus { get; init; }

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
                [JsonPropertyName("version")]
                public string Version { get; init; }

                /// <summary>
                /// Gets the type.
                /// </summary>
                /// <value>
                /// The type.
                /// </value>
                [JsonPropertyName("type")]
                public string Type { get; init; }

                /// <summary>
                /// Gets the created at.
                /// </summary>
                /// <value>
                /// The created at.
                /// </value>
                [JsonPropertyName("created_at")]
                public string CreatedAt { get; init; }

                /// <summary>
                /// Gets the value.
                /// </summary>
                /// <value>
                /// The value.
                /// </value>
                [JsonPropertyName("value")]
                public string Value { get; init; }

                /// <summary>
                /// Gets the created by.
                /// </summary>
                /// <value>
                /// The created by.
                /// </value>
                [JsonPropertyName("created_by")]
                public CreatedByInfo CreatedBy { get; init; }
            }

            /// <summary>
            /// Information about the creator of the secret.
            /// </summary>
            public record CreatedByInfo
            {
                /// <summary>
                /// Gets the name.
                /// </summary>
                /// <value>
                /// The name.
                /// </value>
                [JsonPropertyName("name")]
                public string Name { get; init; }

                /// <summary>
                /// Gets the type.
                /// </summary>
                /// <value>
                /// The type.
                /// </value>
                [JsonPropertyName("type")]
                public string Type { get; init; }

                /// <summary>
                /// Gets the email.
                /// </summary>
                /// <value>
                /// The email.
                /// </value>
                [JsonPropertyName("email")]
                public string Email { get; init; }
            }
        }


        /// <summary>
        /// Gets the secret.
        /// </summary>
        /// <param name="secretName">Name of the secret.</param>
        /// <returns>The <see cref="OpenAppSecret"/> secret</returns>
        /// <exception cref="HashiCorpContext.TokenAcquisitionException">Failed to acquire <see cref="HashiCorpContext"/> bearer token.</exception>
        public async Task<OpenAppSecret> GetSecret(string secretName)
        {
            var secret = new OpenAppSecret();
            if (await hashiCorpClient.GetJsonAsync<OpenAppSecretResponse>($"open/{secretName}").ConfigureAwait(false) is
                { } response)
            {
                secret = response.Secret;
            }

            return secret;
        }

        public void Dispose()
        {
            hashiCorpClient.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Default error response from HashiCorp endpoints.
        /// </summary>
        /// <param name="Code"></param>
        public record ErrorResponse(int Code, string Message, Any[] Details);

        /// <summary>
        /// HashiCorp identity (non-secret regions only).
        /// </summary>
        internal record HashiCorpIdentity
        {
            /// <summary>
            /// Gets the client identifier.
            /// </summary>
            /// <value>
            /// The client identifier.
            /// </value>
            [JsonPropertyName("client_id")]
            public string ClientId { get; init; }

            /// <summary>
            /// Gets the analytics Javascript anonymous identifier.
            /// </summary>
            /// <value>
            /// The anonymous analytics id.
            /// </value>
            /// <seealso href="https://github.com/hashicorp/web-platform-packages/blob/main/packages/analytics/analytics-js-helpers.ts#L33">HashiCorp web platform packages <c>getSegmentId()</c></seealso>
            [JsonPropertyName("ajs_aid")]
            public string AnalyticsJsAnonymousId { get; init; }

            /// <summary>
            /// Gets the user identifier.
            /// </summary>
            /// <value>
            /// The user identifier.
            /// </value>
            [JsonPropertyName("user_id")]
            public string UserId { get; init; }

            /// <summary>
            /// Gets the organization identifier.
            /// </summary>
            /// <value>
            /// The organization identifier.
            /// </value>
            [JsonPropertyName("org_id")]
            public string OrganizationId { get; init; }

            /// <summary>
            /// Gets the project identifier.
            /// </summary>
            /// <value>
            /// The project identifier.
            /// </value>
            [JsonPropertyName("proj_id")]
            public string ProjectId { get; init; }

            /// <summary>
            /// Gets the name of the application.
            /// </summary>
            /// <value>
            /// The name of the application.
            /// </value>
            [JsonPropertyName("app_name")]
            public string ApplicationName { get; init; }
        }

        internal class HashiCorpContext(string clientSecret, HashiCorpIdentity identity)
            : AuthenticatorBase(string.Empty)
        {
            /// <summary>
            /// Gets the token expiration.
            /// </summary>
            /// <value>
            /// The expiration.
            /// </value>
            private DateTime Expiration { get; set; } = DateTime.Now;

            /// <summary>
            /// Gets the authentication parameter.
            /// </summary>
            /// <param name="accessToken">The access token.</param>
            /// <returns></returns>
            /// <exception cref="TokenAcquisitionException">Did not receive HashiCorp auth token</exception>
            protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
            {
                var tokenType = "Bearer";
                // ReSharper disable once InvertIf
                if (string.IsNullOrEmpty(Token) || DateTime.UtcNow >= Expiration)
                {
                    if (await GetToken().ConfigureAwait(false) is not { } tokenResponse)
                    {
                        throw new TokenAcquisitionException("Did not receive HashiCorp auth token");
                    }

                    tokenType = tokenResponse.TokenType;
                    Token = tokenResponse.AccessToken;
                    Expiration = tokenResponse.GetExpirationDate(DateTime.UtcNow);
                }

                return new HeaderParameter(KnownHeaders.Authorization, $"{tokenType} {Token}");
            }

            /// <summary>
            /// Gets the token.
            /// </summary>
            /// <returns>HashiCorp <see cref="TokenResponse"/> token.</returns>
            private async Task<TokenResponse?> GetToken()
            {
                var options = new RestClientOptions("https://auth.hashicorp.com");
                using var client = new RestClient(options);
                var request = new RestRequest("oauth/token").AddParameter("audience", "https://api.hashicorp.cloud")
                    .AddParameter("grant_type", "client_credentials").AddParameter("client_id", identity.ClientId)
                    .AddParameter("client_secret", clientSecret);
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
                public TokenAcquisitionException(string specificMessage, Exception innerException) : base(
                    $"{BaseMessage}: {specificMessage}", innerException)
                {
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
                public string AccessToken { get; init; }

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
                public string TokenType { get; init; }

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
        }
    }
}
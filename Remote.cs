using Microsoft.EntityFrameworkCore;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaXmlSplitter.Resources;
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

            private abstract record TokenResponse
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
            private abstract record HashiCorpIdentity
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
                /// Gets the analytics aid.
                /// </summary>
                /// <value>
                /// The analytics id.
                /// </value>
                [JsonPropertyName("ajs_aid")]
                public required string AnalyticsId { get; init; }
                /// <summary>
                /// Gets the user identifier.
                /// </summary>
                /// <value>
                /// The user identifier.
                /// </value>
                [JsonPropertyName("user_id")]
                public required string UserId { get; init; }
                /// <summary>
                /// Gets the org identifier.
                /// </summary>
                /// <value>
                /// The org identifier.
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
        }
    }
}

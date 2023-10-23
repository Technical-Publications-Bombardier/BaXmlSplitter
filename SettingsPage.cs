using System.Configuration;
using BaXmlSplitter.Resources;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Versioning;

namespace BaXmlSplitter
{
    [SupportedOSPlatform("windows")]
    public partial class SettingsPage : Form
    {
        private readonly Remote.HashiCorpClient hcpClient;
        /// <summary>
        /// Exception to indicate that the secret was incorrect.
        /// </summary>
        /// <seealso cref="ArgumentException" />
        private class SecretSettingsException : ArgumentException
        {
            /// <summary>
            /// The base message
            /// </summary>
            private const string BaseMessage = "Secret is invalid";
            /// <summary>
            /// Initializes a new instance of the <see cref="SecretSettingsException"/> class.
            /// </summary>
            public SecretSettingsException() : base($"{BaseMessage}.")
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SecretSettingsException"/> class.
            /// </summary>
            /// <param name="specificMessage">The specific message.</param>
            public SecretSettingsException(string specificMessage) : base($"{BaseMessage}: {specificMessage}")
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SecretSettingsException"/> class.
            /// </summary>
            /// <param name="specificMessage">The specific message.</param>
            /// <param name="innerException">The inner exception.</param>
            public SecretSettingsException(string specificMessage, Exception innerException) : base($"{BaseMessage}: {specificMessage}", innerException)
            {
            }
        }
        /// <summary>
        /// The entropy
        /// </summary>
        private readonly byte[] entropy = [156, 79, 60, 110, 120, 20, 61, 44, 196, 135, 175, 132, 88, 23, 119, 173, 252, 147, 148, 172, 149, 30, 52, 81, 57, 6, 247, 19, 213, 159, 162, 158, .. Convert.FromBase64String(Properties.Resources.Entropy)];
        /// <summary>
        /// The HashiCorp client secret (encrypted bytes encoded as base-64 string)
        /// </summary>
        /// <remarks>The stored secret is encrypted to remain secure at rest. The client should only store the value from this private variable, and use the value of the <see cref="HashiCorpClientSecret"/> at runtime.</remarks>
        private string hashiCorpClientSecret = string.Empty;
        /// <summary>
        /// Gets the HashiCorp client secret.
        /// </summary>
        /// <value>
        /// The HashiCorp client secret.
        /// </value>
        /// <exception cref="SecretSettingsException">
        /// Incorrect length. HashiCorp client secret must be 64 characters.
        /// or
        /// Hash mismatch. HashiCorp client secret content appears to have been corrupted.
        /// </exception>
        internal string HashiCorpClientSecret
        {
            get => GetSecretSetting(hashiCorpClientSecret, nameof(HashiCorpClientSecret));
            set => SetSecretSetting(nameof(HashiCorpClientSecret), Properties.Resources.HashiCorpClientSecretExpectedHash, value, out hashiCorpClientSecret);
        }

        /// <summary>
        /// Sets the encrypted secret from a plaintext <param name="inputValue" />. This setter is
        /// designed to ensure that all sensitive data is secure at rest.
        /// </summary>
        /// <param name="property">The name of the <see cref="Settings"/> property to set.</param>
        /// <param name="expectedHash">The expected <see cref="SHA256"/> hash.</param>
        /// <param name="field">The field to set.</param>
        /// <param name="inputValue">The input from which to read the value.</param>
        /// <returns></returns>
        /// <exception cref="SecretSettingsException">
        /// Hash mismatch indicates <param name="property" /> content has been corrupted.
        /// </exception>
        private void SetSecretSetting(string property, string expectedHash, string inputValue, out string field)
        {
            if (string.IsNullOrEmpty(inputValue))
            {
                field = string.Empty;
                return;
            }
            // check that the SHA-256 hash is as expected
            if (SHA256.HashData(Encoding.UTF8.GetBytes(inputValue)) is not { } hashBytes ||
                BitConverter.ToString(hashBytes)
                        .Replace("-",
                            string.Empty,
                            StringComparison.InvariantCulture) is not { } hashStr ||
                !string.Equals(hashStr,
                    expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecretSettingsException($"Hash mismatch. {property} content appears to have been corrupted");
            }
            // encrypt the hashicorp client secret
            var plainTextBytes = Encoding.UTF8.GetBytes(inputValue);
            var cipherTextBytes = ProtectedData.Protect(plainTextBytes, entropy, DataProtectionScope.CurrentUser);
            field = Settings.Default.HashiCorpClientSecret = Convert.ToBase64String(cipherTextBytes);
        }

        /// <summary>
        /// Gets the secret setting.
        /// </summary>
        /// <param name="secret">The secret.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        /// <exception cref="SettingsPropertyNotFoundException">propertyName</exception>
        private string GetSecretSetting(string? secret, string propertyName)
        {
            string? fromSettings;
            try
            {
                // Get a specific property by name
                if (Settings.Default.Properties[propertyName] is not { } property)
                {
                    throw new SettingsPropertyNotFoundException(propertyName);
                }
                fromSettings = Settings.Default[property.Name].ToString();
            }
            catch (SettingsPropertyNotFoundException)
            {
                // Handle the case when the property does not exist
                return string.Empty;
            }
            catch (ConfigurationErrorsException)
            {
                // Handle the case when the configuration file is invalid
                return string.Empty;
            }
            var secretValue = secret ?? fromSettings;
            if (string.IsNullOrEmpty(secretValue))
            {
                return string.Empty;
            }

            byte[] cipherTextBytes;

            try
            {
                cipherTextBytes = Convert.FromBase64String(secretValue);
            }
            catch (FormatException)
            {
                Debug.WriteLine("Retrieving secret failed on base-64 decoding. This suggests the settings were corrupted.", "Error");
                return string.Empty;
            }
            var plainTextBytes = ProtectedData.Unprotect(cipherTextBytes, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainTextBytes);
        }

        /// <summary>
        /// The Azure application secret
        /// </summary>
        private string azureApplicationSecret = string.Empty;
        /// <summary>
        /// Gets the Azure application secret.
        /// </summary>
        /// <value>
        /// The Azure application secret.
        /// </value>
        internal string AzureApplicationSecret
        {
            get => GetSecretSetting(azureApplicationSecret, nameof(AzureApplicationSecret));
            set => SetSecretSetting(nameof(AzureApplicationSecret), Properties.Resources.AzureApplicationSecretExpectedHash, value, out azureApplicationSecret);
        }

        /// <summary>
        /// The Bombardier Oracle connection string
        /// </summary>
        private string baOraConnectionString = string.Empty;
        /// <summary>
        /// Gets the Bombardier Oracle connection string.
        /// </summary>
        /// <value>
        /// The Bombardier Oracle connection string.
        /// </value>
        internal string BaOraConnectionString
        {
            get => GetSecretSetting(baOraConnectionString, nameof(BaOraConnectionString));
            set => SetSecretSetting(nameof(BaOraConnectionString), Properties.Resources.BaOraConnectionStringExpectedHash, value, out baOraConnectionString);
        }

        /// <summary>
        /// The first Azure storage account key.
        /// </summary>
        private string storageAccountKeyOne = string.Empty;
        /// <summary>
        /// Gets the Azure storage account key number one.
        /// </summary>
        /// <value>
        /// The Azure storage account key number one.
        /// </value>
        internal string StorageAccountKeyOne
        {
            get => GetSecretSetting(storageAccountKeyOne, nameof(StorageAccountKeyOne));
            set => SetSecretSetting(nameof(StorageAccountKeyOne), Properties.Resources.StorageAccountKeyOneExpectedHash, value, out storageAccountKeyOne);
        }

        /// <summary>
        /// The second Azure storage account key.
        /// </summary>
        private string storageAccountKeyTwo = string.Empty;
        /// <summary>
        /// Gets the Azure storage account key number two.
        /// </summary>
        /// <value>
        /// The Azure storage account key number two.
        /// </value>
        internal string StorageAccountKeyTwo
        {
            get => GetSecretSetting(storageAccountKeyTwo, nameof(StorageAccountKeyTwo));
            set => SetSecretSetting(nameof(StorageAccountKeyTwo), Properties.Resources.StorageAccountKeyTwoExpectedHash, value, out storageAccountKeyTwo);
        }

        private readonly Dictionary<string, Action<string>> secrets;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage" /> class.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();


            // Collection of secrets to iterate over
            secrets = new Dictionary<string, Action<string>>
            {
                { nameof(AzureApplicationSecret), secretValue => AzureApplicationSecret = secretValue },
                { nameof(BaOraConnectionString), secretValue => BaOraConnectionString = secretValue },
                { nameof(StorageAccountKeyOne), secretValue => StorageAccountKeyOne = secretValue },
                { nameof(StorageAccountKeyTwo), secretValue => StorageAccountKeyTwo = secretValue },
            };

            // Get the HashiCorp Client Secret from the settings, if possible
            HashiCorpClientSecret = GetSecretSetting(null, nameof(HashiCorpClientSecret));

            // prepare a HashiCorp client to query for HashiCorp Cloud Platform Vault Secrets
            hcpClient = new Remote.HashiCorpClient(HashiCorpClientSecret);
        }


        private void LoadSecrets(object sender, EventArgs e)
        {
            _ = LoadSecretsAsync(sender, e);
        }
        private async Task LoadSecretsAsync(object sender, EventArgs e)
        {
            foreach (var name in secrets.Keys)
            {
                // set the secret using the settings with fallback to the HashiCorp client
                var secret = GetSecretSetting(null, name);
                if (string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(hashiCorpClientSecret))
                {
                    Remote.HashiCorpClient.OpenAppSecret result;
                    try
                    {
                        result = await hcpClient.GetSecret(name).ConfigureAwait(true);
                    }
                    catch (HttpRequestException exception)
                    {
                        Debug.WriteLine(exception);
                        throw;
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                        throw;
                    }
                    if (result is { Version.Value: { } value })
                        // set the secret using the HashiCorp client
                        secrets[name](value);
                }
                else
                {
                    // set the secret using the settings
                    secrets[name](secret);
                }
            }
            this.Invoke((MethodInvoker)delegate
            {
                // Set secret TextBox values from the properties
                HashiCorpClientSecretTextBox.Text = HashiCorpClientSecret;
                AzureApplicationSecretTextBox.Text = AzureApplicationSecret;
                BaOraConnectionStringTextBox.Text = BaOraConnectionString;
                AzureStorageKeyOneTextBox.Text = StorageAccountKeyOne;
                AzureStorageKeyTwoTextBox.Text = StorageAccountKeyTwo;

                // Hide the arrows if HashiCorp client secret is set
                HideArrowsIfClientSecretIsSet();
            });
        }

        /// <summary>
        /// The HashiCorp client secret text field handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void HashiCorpClientSecretHandler(object sender, EventArgs e)
        {
            try
            {
                HashiCorpClientSecret = HashiCorpClientSecretTextBox.Text;
                Settings.Default.HashiCorpClientSecret = hashiCorpClientSecret;
            }
            catch (SecretSettingsException ex)
            {
                XmlSplitterHelpers.ShowWarningBox($"{ex.Message}. Please check the HashiCorp client secret and re-enter.", "Invalid HashiCorp Client Secret");
            }
            HideArrowsIfClientSecretIsSet();
        }

        /// <summary>
        /// Hides the arrow instructions if the <see cref="HashiCorpClientSecret"/> is set.
        /// </summary>
        /// <returns></returns>
        private void HideArrowsIfClientSecretIsSet()
        {

            if (!string.IsNullOrEmpty(HashiCorpClientSecret))
            {
                secretsGroup.BackgroundImage = null;
                arrowsLabel.Hide();
            }
            else
            {
                arrowsLabel.Show();
                secretsGroup.BackgroundImage = Properties.Resources.arrows;
            }
        }

        /// <summary>
        /// Handles the click event of the OkSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void OkSettings_Click(object sender, EventArgs e)
        {
            // apply settings and close the form
            ApplySettings_Click(sender, e);
            Close();
        }

        /// <summary>
        /// Handles the Click event of the ApplySettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void ApplySettings_Click(object sender, EventArgs e)
        {
            HashiCorpClientSecretHandler(sender, e);
            Settings.Default.Save();
        }

        /// <summary>
        /// Handles the Click event of the CancelSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void CancelSettings_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the <see cref="showHashiCorpClientSecretCheckbox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void ShowHashiCorpClientSecret_Click(object sender, EventArgs e)
        {
            HashiCorpClientSecretTextBox.UseSystemPasswordChar = !HashiCorpClientSecretTextBox.UseSystemPasswordChar;
        }

        /// <summary>
        /// Handles the Click event of the <see cref="showAzureApplicationSecretCheckbox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void ShowAzureApplicationSecret_Click(object sender, EventArgs e)
        {
            AzureApplicationSecretTextBox.UseSystemPasswordChar = !AzureApplicationSecretTextBox.UseSystemPasswordChar;
        }

        /// <summary>
        /// Handles the Click event of the <see cref="showBaOraConnectionStringCheckbox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void ShowBaOraConnectionString_Click(object sender, EventArgs e)
        {
            BaOraConnectionStringTextBox.UseSystemPasswordChar = !BaOraConnectionStringTextBox.UseSystemPasswordChar;
        }

        /// <summary>
        /// Handles the Click event of the <see cref="showAzureStorageAccountKeyOneCheckbox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void ShowAzureStorageAccountKeyOne_Click(object sender, EventArgs e)
        {
            AzureStorageKeyOneTextBox.UseSystemPasswordChar = !AzureStorageKeyOneTextBox.UseSystemPasswordChar;
        }

        /// <summary>
        /// Handles the Click event of the <see cref="showAzureStorageAccountKeyTwoCheckbox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void ShowAzureStorageAccountKeyTwo_Click(object sender, EventArgs e)
        {
            AzureStorageKeyTwoTextBox.UseSystemPasswordChar = !AzureStorageKeyTwoTextBox.UseSystemPasswordChar;
        }

    }
}

using BaXmlSplitter.Resources;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace BaXmlSplitter
{
    public partial class SettingsPage : Form
    {
        /// <summary>
        /// Exception to indicate that the secret was incorrect.
        /// </summary>
        /// <seealso cref="System.ArgumentException" />
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
        private string hashiCorpClientSecret;
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
            get => GetSecretSetting(hashiCorpClientSecret, "HashiCorpClientSecret");
            set => SetSecretSetting("HashiCorpClientSecret", Properties.Resources.HashiCorpClientSecretExpectedHash, value, out hashiCorpClientSecret);
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
            // check that the SHA-256 hash is as expected
            if (SHA256.HashData(Encoding.UTF8.GetBytes(inputValue)) is not { } hash || string.Equals(hash.ToString(),
                    expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecretSettingsException($"Hash mismatch. {property} content appears to have been corrupted.");
            }
            // encrypt the hashicorp client secret
            var plainTextBytes = Encoding.UTF8.GetBytes(inputValue);
            var cipherTextBytes = ProtectedData.Protect(plainTextBytes, entropy, DataProtectionScope.CurrentUser);
            field = Settings.Default.HashiCorpClientSecret = Convert.ToBase64String(cipherTextBytes);
        }

        private string GetSecretSetting(string secret, string property)
        {
            dynamic? fromSettings;
            try
            {
                fromSettings = typeof(Settings).GetProperty(property)?.GetValue(this);
            }
            catch (TargetException)
            {
                return string.Empty;
            }
            if (fromSettings is null || string.IsNullOrEmpty(fromSettings.ToString()))
            {
                return string.Empty;
            }
            var cipherTextBytes = Convert.FromBase64String(secret);
            var plainTextBytes = ProtectedData.Unprotect(cipherTextBytes, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainTextBytes);
        }

        /// <summary>
        /// The Azure application secret
        /// </summary>
        private string azureSecret;
        /// <summary>
        /// Gets the Azure application secret.
        /// </summary>
        /// <value>
        /// The Azure application secret.
        /// </value>
        internal string AzureSecret
        {
            get => GetSecretSetting(azureSecret, "AzureSecret");
            set => SetSecretSetting("AzureSecret", Properties.Resources.AzureSecretExpectedHash, value, out azureSecret);
        }

        /// <summary>
        /// The Bombardier Oracle connection string
        /// </summary>
        private string baOraConnectionString;
        /// <summary>
        /// Gets the Bombardier Oracle connection string.
        /// </summary>
        /// <value>
        /// The Bombardier Oracle connection string.
        /// </value>
        internal string BaOraConnectionString
        {
            get => GetSecretSetting(baOraConnectionString, "BaOraConnectionString");
            set => SetSecretSetting("BaOraConnectionString", Properties.Resources.BaOraConnectionStringExpectedHash, value, out baOraConnectionString);
        }

        /// <summary>
        /// The first Azure storage account key.
        /// </summary>
        private string storageAccountKeyOne;
        /// <summary>
        /// Gets the Azure storage account key number one.
        /// </summary>
        /// <value>
        /// The Azure storage account key number one.
        /// </value>
        internal string StorageAccountKeyOne
        {
            get => GetSecretSetting(storageAccountKeyOne, "StorageAccountKeyOne");
            set => SetSecretSetting("StorageAccountKeyOne", Properties.Resources.StorageAccountKeyOneExpectedHash, value, out storageAccountKeyOne);
        }

        /// <summary>
        /// The second Azure storage account key.
        /// </summary>
        private string storageAccountKeyTwo;
        /// <summary>
        /// Gets the Azure storage account key number two.
        /// </summary>
        /// <value>
        /// The Azure storage account key number two.
        /// </value>
        internal string StorageAccountKeyTwo
        {
            get => GetSecretSetting(storageAccountKeyTwo, "StorageAccountKeyTwo");
            set => SetSecretSetting("StorageAccountKeyTwo", Properties.Resources.StorageAccountKeyTwoExpectedHash, value, out storageAccountKeyTwo);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            hashiCorpClientSecret = Settings.Default.HashiCorpClientSecret;
            hashiCorpClientSecretTextBox.Text = HashiCorpClientSecret;
        }

        /// <summary>
        /// The HashiCorp client secret text field handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void HashiCorpClientSecretHandler(object sender, EventArgs e)
        {
            try
            {
                HashiCorpClientSecret = hashiCorpClientSecretTextBox.Text;
                Settings.Default.HashiCorpClientSecret = hashiCorpClientSecret;
            }
            catch (SecretSettingsException ex)
            {
                XmlSplitterHelpers.ShowWarningBox($"{ex.Message}. Please check the HashiCorp client secret and re-enter.", "Invalid HashiCorp Client Secret");
            }
        }

        /// <summary>
        /// Handles the click event of the OkSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
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
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
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
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void CancelSettings_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the ShowHashiCorpClientSecret control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void ShowHashiCorpClientSecret_Click(object sender, EventArgs e)
        {
            hashiCorpClientSecretTextBox.UseSystemPasswordChar = !hashiCorpClientSecretTextBox.UseSystemPasswordChar;

        }

    }
}

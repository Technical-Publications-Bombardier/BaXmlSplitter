using BaXmlSplitter.Resources;
using System.Globalization;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace BaXmlSplitter
{
    public partial class SettingsPage : Form
    {
        /// <summary>
        /// Exception to indicate that the HashiCorp Client Secret was incorrect.
        /// </summary>
        /// <seealso cref="System.ArgumentException" />
        private class HashiCorpClientSecretException : ArgumentException
        {
            /// <summary>
            /// The base message
            /// </summary>
            private const string BaseMessage = "HashiCorp client secret is invalid";
            /// <summary>
            /// Initializes a new instance of the <see cref="HashiCorpClientSecretException"/> class.
            /// </summary>
            public HashiCorpClientSecretException() : base($"{BaseMessage}.")
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="HashiCorpClientSecretException"/> class.
            /// </summary>
            /// <param name="specificMessage">The specific message.</param>
            public HashiCorpClientSecretException(string specificMessage) : base($"{BaseMessage}: {specificMessage}")
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="HashiCorpClientSecretException"/> class.
            /// </summary>
            /// <param name="specificMessage">The specific message.</param>
            /// <param name="innerException">The inner exception.</param>
            public HashiCorpClientSecretException(string specificMessage, Exception innerException) : base($"{BaseMessage}: {specificMessage}", innerException)
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
        /// <exception cref="HashiCorpClientSecretException">
        /// Incorrect length. HashiCorp client secret must be 64 characters.
        /// or
        /// Hash mismatch. HashiCorp client secret content appears to have been corrupted.
        /// </exception>
        internal string HashiCorpClientSecret
        {
            get
            {
                if (string.IsNullOrEmpty(Settings.Default.HashiCorpClientSecret))
                {
                    return string.Empty;
                }
                var cipherTextBytes = Convert.FromBase64String(hashiCorpClientSecret);
                var plainTextBytes = ProtectedData.Unprotect(cipherTextBytes, entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainTextBytes);
            }
            set
            {
                if (value.Length != 64)
                {
                    throw new HashiCorpClientSecretException($"Incorrect length. HashiCorp client secret must be 64 characters (not {value.Length}).");
                }
                // check that the hashicorp client secret SHA-256 hash is 2D0E8...3A1DF8C
                if (SHA256.HashData(Encoding.UTF8.GetBytes(value)) is not { } hash || string.Equals(hash.ToString(),
                        Properties.Resources.HashiCorpClientSecretExpectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    throw new HashiCorpClientSecretException("Hash mismatch. HashiCorp client secret content appears to have been corrupted.");
                }
                // encrypt the hashicorp client secret
                var plainTextBytes = Encoding.UTF8.GetBytes(value);
                var cipherTextBytes = ProtectedData.Protect(plainTextBytes, entropy, DataProtectionScope.CurrentUser);
                hashiCorpClientSecret = Settings.Default.HashiCorpClientSecret = Convert.ToBase64String(cipherTextBytes);
            }
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
            catch (HashiCorpClientSecretException ex)
            {
                XmlSplitterHelpers.ShowWarningBox($"{ex.Message}. Please check the HashiCorp client secret and re-enter.", "Invalid HashiCorp Client Secret");
            }
        }

        /// <summary>
        /// Handles the Click event of the OkSettings control.
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

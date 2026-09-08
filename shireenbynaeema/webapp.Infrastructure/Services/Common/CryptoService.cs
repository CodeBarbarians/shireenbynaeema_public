namespace Infrastructure
{
    using System.Security.Cryptography;
    using System.Text;

    using Application;
    using Domain;

    /// <summary>
    /// Provides cryptographic services for encrypting and decrypting strings using AES-256-GCM. Implements the
    /// ICryptoService interface to enable secure data protection within the application.
    /// </summary>
    /// <remarks>The CryptoService retrieves its encryption key from the application configuration via the
    /// IAppSettingsConfig interface. Ensure that the configuration contains a valid EncryptionKey property to avoid
    /// cryptographic errors. This class is intended for use in scenarios where sensitive string data must be securely
    /// encrypted and decrypted. The service is not thread-safe; create separate instances if used concurrently in
    /// multi-threaded environments.</remarks>
    public class CryptoService : ICryptoService
    {
        // 32 bytes key for AES-256
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private readonly byte[] key;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoService"/> class using the specified application settings.
        /// configuration.
        /// </summary>
        /// <remarks>The encryption key is retrieved from the provided configuration and encoded as a
        /// UTF-8 byte array. Ensure that the EncryptionKey property is properly set in the configuration to avoid
        /// cryptographic errors.</remarks>
        /// <param name="config">The application settings configuration that provides the encryption key used for cryptographic operations.
        /// Cannot be null and must contain a valid EncryptionKey property.</param>
        public CryptoService(IAppSettingsConfig config)
        {
            this.key = Encoding.UTF8.GetBytes(config.EncryptionKey);
        }

        /// <summary>
        /// Encrypts the specified plain text string using AES-GCM and returns the encrypted data as a Base64-encoded
        /// string.
        /// </summary>
        /// <remarks>The returned string includes all necessary components for decryption: the nonce,
        /// authentication tag, and ciphertext. The encryption uses AES-GCM with a predefined key and tag size. This
        /// method is not intended for encrypting large data; it is optimized for short strings such as passwords or
        /// tokens.</remarks>
        /// <param name="plainText">The plain text string to encrypt. If null or empty, the original value is returned unmodified.</param>
        /// <returns>A Base64-encoded string containing the encrypted data, including the nonce and authentication tag. Returns
        /// the original value if the input is null or empty.</returns>
        public string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using var aes = new AesGcm(this.key, TagSize);

            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            byte[] result = new byte[NonceSize + TagSize + ciphertext.Length];

            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts the specified Base64-encoded ciphertext string using AES-GCM and returns the original plaintext.
        /// </summary>
        /// <remarks>If the input is not a valid encrypted string or decryption fails, the method returns
        /// the input value unchanged. The method expects the ciphertext to be formatted with the nonce, authentication
        /// tag, and encrypted data concatenated and Base64-encoded.</remarks>
        /// <param name="cipherText">The Base64-encoded string representing the encrypted data to decrypt. Must contain the nonce, authentication
        /// tag, and ciphertext in the expected format.</param>
        /// <returns>A string containing the decrypted plaintext. If decryption fails, returns the original ciphertext string.</returns>
        public string DecryptString(string cipherText)
        {
            try
            {
                byte[] buffer = Convert.FromBase64String(cipherText);

                byte[] nonce = new byte[NonceSize];
                byte[] tag = new byte[TagSize];
                byte[] ciphertext = new byte[buffer.Length - NonceSize - TagSize];

                Buffer.BlockCopy(buffer, 0, nonce, 0, NonceSize);
                Buffer.BlockCopy(buffer, NonceSize, tag, 0, TagSize);
                Buffer.BlockCopy(buffer, NonceSize + TagSize, ciphertext, 0, ciphertext.Length);

                byte[] plaintext = new byte[ciphertext.Length];

                using var aes = new AesGcm(this.key, TagSize);

                aes.Decrypt(nonce, ciphertext, tag, plaintext);

                return Encoding.UTF8.GetString(plaintext);
            }
            catch
            {
                return cipherText;
            }
        }
    }
}
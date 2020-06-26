using System;
using System.Security.Cryptography;
using Xunit;

namespace Encryption.Tests
{
    public class EncryptionKeysFixture : IDisposable
    {
        public byte[] SymmetricKey { get; private set; } = new byte[256 / 8];
        public RSA RSAKeyPair { get; private set; } = RSA.Create();

        public EncryptionKeysFixture()
        {
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(SymmetricKey); // Fill symmetricKey with random bytes.
        }

        public void Dispose()
        {
        }
    }


    [CollectionDefinition("EncryptionKeys")]
    public class EncryptionKeysCollection : ICollectionFixture<EncryptionKeysFixture>
    {
    }
}

﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Configuration.Test
{
    public class CryptoHashProviderTest
    {
        [Fact]
        public void DefaultCryptoHashProviderUsesSHA512()
        {
            // Arrange
            byte[] testBytes = Encoding.UTF8.GetBytes("There is no butter knife");
            string expectedHash = "xy/brd+/mxheBbyBL7i8Oyy62P2ZRteaIkfc4yA8ncH1MYkbDo+XwBcZsOBY2YeaOucrdLJj5odPvozD430w2g==";
            CryptoHashProvider hashProvider = new CryptoHashProvider();

            // Act
            byte[] actualHash = hashProvider.CalculateHash(testBytes);

            // Assert
            Assert.Equal(actualHash, Convert.FromBase64String(expectedHash));
        }

        [Fact]
        public void DefaultCryptoHashProviderUsesSHA512Stream()
        {
            // Arrange
            byte[] testBytes = Encoding.UTF8.GetBytes("There is no butter knife");
            string expectedHash = "xy/brd+/mxheBbyBL7i8Oyy62P2ZRteaIkfc4yA8ncH1MYkbDo+XwBcZsOBY2YeaOucrdLJj5odPvozD430w2g==";
            CryptoHashProvider hashProvider = new CryptoHashProvider();
            MemoryStream stream = new MemoryStream(testBytes);

            // Act
            byte[] actualHash = hashProvider.CalculateHash(stream);

            // Assert
            Assert.Equal(actualHash, Convert.FromBase64String(expectedHash));
        }

        [Theory]
        [InlineData("md5")]
        [InlineData("MD5")]
        [InlineData("SHA1")]
        [InlineData("SHA2561")]
        public void CryptoHashProviderThrowsIfHashAlgorithmIsNotSHA512orSHA256(string hashAlgorithm)
        {
            // Act and Assert
            Exception ex = Record.Exception(() => new CryptoHashProvider(hashAlgorithm));
            Assert.NotNull(ex);
            ArgumentException tex = Assert.IsAssignableFrom<ArgumentException>(ex);
            Assert.Equal("hashAlgorithm", tex.ParamName);
            var lines = ex.Message.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, lines.Length);
            Assert.Equal(String.Format("Hash algorithm '{0}' is unsupported. Supported algorithms include: SHA512 and SHA256.", hashAlgorithm), lines[0]);
            Assert.True(lines[1].EndsWith("hashAlgorithm"));
        }

        [Theory]
        [InlineData("sha512", "xy/brd+/mxheBbyBL7i8Oyy62P2ZRteaIkfc4yA8ncH1MYkbDo+XwBcZsOBY2YeaOucrdLJj5odPvozD430w2g==")]
        [InlineData("SHA512", "xy/brd+/mxheBbyBL7i8Oyy62P2ZRteaIkfc4yA8ncH1MYkbDo+XwBcZsOBY2YeaOucrdLJj5odPvozD430w2g==")]
        [InlineData("sha256", "F7qs6AZmrGdFSsAc/EpRjjIgkhlW8M92djz8ySt48EM=")]
        public void CryptoHashProviderAllowsSHA512orSHA256(string hashAlgorithm, string expectedHash)
        {
            // Arrange
            byte[] testBytes = Encoding.UTF8.GetBytes("There is no butter knife");
            var hashProvider = new CryptoHashProvider(hashAlgorithm);

            // Act
            string result = Convert.ToBase64String(hashProvider.CalculateHash(testBytes));

            // Assert
            Assert.Equal(expectedHash, result);
        }


        [Theory]
        [InlineData("sha512", "xy/brd+/mxheBbyBL7i8Oyy62P2ZRteaIkfc4yA8ncH1MYkbDo+XwBcZsOBY2YeaOucrdLJj5odPvozD430w2g==")]
        [InlineData("SHA512", "xy/brd+/mxheBbyBL7i8Oyy62P2ZRteaIkfc4yA8ncH1MYkbDo+XwBcZsOBY2YeaOucrdLJj5odPvozD430w2g==")]
        [InlineData("sha256", "F7qs6AZmrGdFSsAc/EpRjjIgkhlW8M92djz8ySt48EM=")]
        public void CryptoHashProviderAllowsSHA512orSHA256Stream(string hashAlgorithm, string expectedHash)
        {
            // Arrange
            byte[] testBytes = Encoding.UTF8.GetBytes("There is no butter knife");
            var hashProvider = new CryptoHashProvider(hashAlgorithm);
            MemoryStream stream = new MemoryStream(testBytes);

            // Act
            string result = Convert.ToBase64String(hashProvider.CalculateHash(stream));

            // Assert
            Assert.Equal(expectedHash, result);
        }

        [Fact]
        public void CryptoHashProviderReturnsTrueIfHashAreEqual()
        {
            // Arrange
            byte[] testBytes = Encoding.UTF8.GetBytes("There is no butter knife");
            string expectedHash = "xy/brd+/mxheBbyBL7i8Oyy62P2ZRteaIkfc4yA8ncH1MYkbDo+XwBcZsOBY2YeaOucrdLJj5odPvozD430w2g==";
            CryptoHashProvider hashProvider = new CryptoHashProvider();

            // Act
            bool result = hashProvider.VerifyHash(testBytes, Convert.FromBase64String(expectedHash));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CryptoHashProviderReturnsFalseIfHashValuesAreUnequal()
        {
            // Arrange
            byte[] testBytes = Encoding.UTF8.GetBytes("There is no butter knife");
            byte[] badBytes = Encoding.UTF8.GetBytes("this is a bad input");
            CryptoHashProvider hashProvider = new CryptoHashProvider();


            // Act
            byte[] testHash = hashProvider.CalculateHash(testBytes);
            byte[] badHash = hashProvider.CalculateHash(badBytes);
            bool result = hashProvider.VerifyHash(testHash, badHash);

            // Assert
            Assert.False(result);
        }

        // Ensures this issue is fixed: http://nuget.codeplex.com/workitem/1489
        [Fact]
        public void CryptoHashProviderIsThreadSafe()
        {
            // Arrange
            byte[] testBytes = Encoding.UTF8.GetBytes("There is no butter knife");
            string expectedHash = "xy/brd+/mxheBbyBL7i8Oyy62P2ZRteaIkfc4yA8ncH1MYkbDo+XwBcZsOBY2YeaOucrdLJj5odPvozD430w2g==";
            CryptoHashProvider hashProvider = new CryptoHashProvider();

            Parallel.For(0, 10000, ignored =>
            {
                // Act
                byte[] actualHash = hashProvider.CalculateHash(testBytes);

                // Assert
                Assert.Equal(actualHash, Convert.FromBase64String(expectedHash));
            });
        }
    }
}
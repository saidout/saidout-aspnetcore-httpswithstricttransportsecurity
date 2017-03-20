using System;
using Microsoft.AspNetCore.Builder;
using NSubstitute;
using NUnit.Framework;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.Tests
{

    public class HttpsWithStrictTransportSecurityExtension_Tests
    {
        private const HttpsMode ValidHttpsMode = HttpsMode.Strict;
        private const int ValidHstsMaxAge = 10000;
        private const bool HstsIncludeSubDomains = true;
        private const int ValidRedirectPort = 1000;


        [Test]
        public void UseHttpsWithHsts_ValidValues_DoesNotReturnNull()
        {
            var appStub = Substitute.For<IApplicationBuilder>();

            var actual = appStub.UseHttpsWithHsts(ValidHttpsMode, ValidHstsMaxAge, HstsIncludeSubDomains, ValidRedirectPort);

            Assert.That(actual, Is.Not.Null);
        }


        [TestCase(int.MinValue)]
        [TestCase(-5000000)]
        [TestCase(-1)]
        public void UseHttpsWithHsts_HstsMaxAgeInvalid_ThrowsArgumentOutOfRangeException(int invalidHstsMaxAge)
        {
            var appStub = Substitute.For<IApplicationBuilder>();

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => appStub.UseHttpsWithHsts(ValidHttpsMode, invalidHstsMaxAge, HstsIncludeSubDomains, ValidRedirectPort));
            Assert.That(ex.ParamName, Is.EqualTo("hstsMaxAgeInSeconds"), nameof(ex.ParamName));
            Assert.That(ex.Message, Does.Contain($"{invalidHstsMaxAge}"), nameof(ex.Message));
        }


        [TestCase(0)]
        [TestCase(50000000)]
        [TestCase(int.MaxValue)]
        public void UseHttpsWithHsts_HstsMaxAgeValid_DoesNotThrow(int validHstsMaxAge)
        {
            var appStub = Substitute.For<IApplicationBuilder>();

            Assert.DoesNotThrow(() => appStub.UseHttpsWithHsts(ValidHttpsMode, validHstsMaxAge, HstsIncludeSubDomains, ValidRedirectPort));
        }


        [TestCase(int.MinValue)]
        [TestCase(-2)]
        [TestCase(65536)]
        [TestCase(int.MaxValue)]
        public void UseHttpsWithHsts_RedirectPortInvalid_ThrowsArgumentOutOfRangeException(int invalidRedirectPort)
        {
            var appStub = Substitute.For<IApplicationBuilder>();

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => appStub.UseHttpsWithHsts(ValidHttpsMode, ValidHstsMaxAge, HstsIncludeSubDomains, invalidRedirectPort));
            Assert.That(ex.ParamName, Is.EqualTo("httpsRedirectPort"), nameof(ex.ParamName));
            Assert.That(ex.Message, Does.Contain($"{invalidRedirectPort}"), nameof(ex.Message));
        }


        [TestCase(-1)]    // Indicates that no port should be added to redirect url
        [TestCase(0)]
        [TestCase(32000)]
        [TestCase(65535)]
        public void UseHttpsWithHsts_RedirectPortValid_DoesNotThrow(int validRedirectPort)
        {
            var appStub = Substitute.For<IApplicationBuilder>();

            Assert.DoesNotThrow(() => appStub.UseHttpsWithHsts(ValidHttpsMode, ValidHstsMaxAge, HstsIncludeSubDomains, validRedirectPort));
        }
    }
}
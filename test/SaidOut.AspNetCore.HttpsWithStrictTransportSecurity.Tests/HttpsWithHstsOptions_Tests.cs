using System;
using NUnit.Framework;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.Tests
{

    public class HttpsWithHstsOptions_Tests
    {

        [TestCase(HttpsMode.Strict, 10, true, 100)]
        [TestCase(HttpsMode.AllowedRedirectForGet, 100, false, 1000)]
        public void Constructor_ValidValues_PropertiesAreSet(HttpsMode validMode, int validHstsMaxAge, bool hstsIncludeSubDomains,  int validRedirectPort)
        {
            var sut = MakeHttpsWithHstsOptions(validMode, validHstsMaxAge, hstsIncludeSubDomains, validRedirectPort);

            Assert.Multiple(() =>
            {
                Assert.That(sut.HttpsMode, Is.EqualTo(validMode), nameof(sut.HttpsMode));
                Assert.That(sut.HstsMaxAgeInSeconds, Is.EqualTo(validHstsMaxAge), nameof(sut.HstsMaxAgeInSeconds));
                Assert.That(sut.HstsIncludeSubDomains, Is.EqualTo(hstsIncludeSubDomains), nameof(sut.HstsIncludeSubDomains));
                Assert.That(sut.HttpsRedirectPort, Is.EqualTo(validRedirectPort), nameof(sut.HttpsRedirectPort));
            });
        }


        [TestCase(-50)]
        [TestCase(50)]
        public void Constructor_HttpsModeIsNotDefined_ThrowsArgumentException(HttpsMode httpsModeNotDefined)
        {
            var ex = Assert.Throws<ArgumentException>(() => MakeHttpsWithHstsOptions(httpsModeNotDefined));
            Assert.That(ex.ParamName, Is.EqualTo("httpsMode"), nameof(ex.ParamName));
            Assert.That(ex.Message, Does.Contain($"{httpsModeNotDefined}"), nameof(ex.Message));
        }


        [TestCase(HttpsMode.Strict)]
        [TestCase(HttpsMode.AllowedRedirectForGet)]
        public void Constructor_HttpsModeIsDefined_DoesNotThrow(HttpsMode httpsMode)
        {
            Assert.DoesNotThrow(() => MakeHttpsWithHstsOptions(httpsMode));
        }


        [TestCase(int.MinValue)]
        [TestCase(-5000000)]
        [TestCase(-1)]
        public void Constructor_HstsMaxAgeInvalid_ThrowsArgumentOutOfRangeException(int invalidHstsMaxAge)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => MakeHttpsWithHstsOptions(hstsMaxAge: invalidHstsMaxAge));
            Assert.That(ex.ParamName, Is.EqualTo("hstsMaxAgeInSeconds"), nameof(ex.ParamName));
            Assert.That(ex.Message, Does.Contain($"{invalidHstsMaxAge}"), nameof(ex.Message));
        }


        [TestCase(0)]
        [TestCase(50000000)]
        [TestCase(int.MaxValue)]
        public void Constructor_HstsMaxAgeValid_DoesNotThrow(int validHstsMaxAge)
        {
            Assert.DoesNotThrow(() => MakeHttpsWithHstsOptions(hstsMaxAge: validHstsMaxAge));
        }


        [TestCase(int.MinValue)]
        [TestCase(-2)]
        [TestCase(65536)]
        [TestCase(int.MaxValue)]
        public void Constructor_RedirectPortInvalid_ThrowsArgumentOutOfRangeException(int invalidRedirectPort)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => MakeHttpsWithHstsOptions(redirectPort: invalidRedirectPort));
            Assert.That(ex.ParamName, Is.EqualTo("httpsRedirectPort"), nameof(ex.ParamName));
            Assert.That(ex.Message, Does.Contain($"{invalidRedirectPort}"), nameof(ex.Message));
        }


        [TestCase(-1)]    // Indicates that no port should be added to redirect URI
        [TestCase(0)]
        [TestCase(32000)]
        [TestCase(65535)]
        public void Constructor_RedirectPortValid_DoesNotThrow(int validRedirectPort)
        {
            Assert.DoesNotThrow(() => MakeHttpsWithHstsOptions(redirectPort: validRedirectPort));
        }


        private HttpsWithHstsOptions MakeHttpsWithHstsOptions(HttpsMode httpsMode = HttpsMode.Strict,
            int hstsMaxAge = 10000,
            bool hstsIncludeSubDomains = true,
            int redirectPort = 1000)
        {
            return new HttpsWithHstsOptions(httpsMode, hstsMaxAge, hstsIncludeSubDomains, redirectPort);
        }
    }
}
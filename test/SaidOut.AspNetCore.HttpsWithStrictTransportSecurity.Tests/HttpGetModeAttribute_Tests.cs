using System;
using NUnit.Framework;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.Tests
{
    public class HttpGetModeAttribute_Tests
    {

        [TestCase(HttpGetMode.ReturnForbidden)]
        [TestCase(HttpGetMode.RedirectToHttps)]
        public void Constructor_ValidValues_PropertiesAreSet(HttpGetMode validMode)
        {
            var sut = MakeHttpGetModeAttribute(validMode);

            Assert.Multiple(() =>
            {
                Assert.That(sut.Mode, Is.EqualTo(validMode), nameof(sut.Mode));
            });
        }


        [TestCase(-50)]
        [TestCase(50)]
        public void Constructor_HttpGetModeIsNotDefined_ThrowsArgumentException(HttpGetMode httpGetModeNotDefined)
        {
            var ex = Assert.Throws<ArgumentException>(() => MakeHttpGetModeAttribute(httpGetModeNotDefined));
            Assert.That(ex.ParamName, Is.EqualTo("mode"), nameof(ex.ParamName));
            Assert.That(ex.Message, Does.Contain($"{httpGetModeNotDefined}"), nameof(ex.Message));
        }


        [TestCase(HttpGetMode.ReturnForbidden)]
        [TestCase(HttpGetMode.RedirectToHttps)]
        public void Constructor_HttpGetModeIsDefined_DoesNotThrow(HttpGetMode httpGetMode)
        {
            Assert.DoesNotThrow(() => MakeHttpGetModeAttribute(httpGetMode));
        }


        private HttpGetModeAttribute MakeHttpGetModeAttribute(HttpGetMode httpGetMode = HttpGetMode.ReturnForbidden)
        {
            return new HttpGetModeAttribute(httpGetMode);
        }
    }
}
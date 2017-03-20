using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NUnit.Framework;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity.Tests
{

    public class HttpsWithStrictTransportSecurityMiddleware_Tests
    {
        private const string HstsHeader = "Strict-Transport-Security";
        private const HttpsMode ValidHttpsMode = HttpsMode.Strict;
        private const int ValidHstsMaxAge = 10000;
        private const bool HstsIncludeSubDomains = true;
        private const int ValidRedirectPort = 1000;
        private const string TestHttpUrl = "http://test.site.net/go";

        public enum CasStubBehavior
        {
            ReturnNull,
            StubReturnNull,
            StubReturnHttpGetModeForbidden,
            StubReturnHttpGetModeRedirect
        }


        [TestCase(0 ,true, "max-age=0; includeSubDomains", CasStubBehavior.ReturnNull)]
        [TestCase(0, false, "max-age=0", CasStubBehavior.ReturnNull)]
        [TestCase(40000000, true, "max-age=40000000; includeSubDomains", CasStubBehavior.ReturnNull)]
        [TestCase(40000000, false, "max-age=40000000", CasStubBehavior.ReturnNull)]
        [TestCase(6000000, true, "max-age=6000000; includeSubDomains", CasStubBehavior.StubReturnNull)]
        [TestCase(6000000, true, "max-age=6000000; includeSubDomains", CasStubBehavior.StubReturnHttpGetModeForbidden)]
        [TestCase(6000000, true, "max-age=6000000; includeSubDomains", CasStubBehavior.StubReturnHttpGetModeRedirect)]
        [TestCase(6000000, false, "max-age=6000000", CasStubBehavior.StubReturnNull)]
        [TestCase(6000000, false, "max-age=6000000", CasStubBehavior.StubReturnHttpGetModeForbidden)]
        [TestCase(6000000, false, "max-age=6000000", CasStubBehavior.StubReturnHttpGetModeRedirect)]
        public void Invoke_SchemeIsHttps_StrictTransportSecurityHeaderIsAddedToResponse(int hstsMaxAge, bool hstsIncludeSubDomains, string expectedHeaderValue, CasStubBehavior casBehavior)
        {
            var nextStub = Substitute.For<RequestDelegate>();
            var httpsOption = new HttpsWithHstsOptions(ValidHttpsMode, hstsMaxAge, hstsIncludeSubDomains, ValidRedirectPort);
            var contextMock = MakeContextFake(true);
            var controllerAttributeSupportStub = MakeControllerAttributeSupportFake(casBehavior, contextMock);
            var sut = new HttpsWithStrictTransportSecurityMiddleware(nextStub, httpsOption, controllerAttributeSupportStub);

            sut.Invoke(contextMock).Wait();

            contextMock.Response.Headers.Received(1).Add(HstsHeader, expectedHeaderValue);
            contextMock.Response.Headers.DidNotReceive().Add(Arg.Any<KeyValuePair<string, StringValues>>());
        }


        // test that it is not set when scheme is not https
        [TestCase(0, true, CasStubBehavior.ReturnNull)]
        [TestCase(0, false, CasStubBehavior.ReturnNull)]
        [TestCase(0, true, CasStubBehavior.StubReturnNull)]
        [TestCase(0, true, CasStubBehavior.StubReturnHttpGetModeForbidden)]
        [TestCase(0, true, CasStubBehavior.StubReturnHttpGetModeRedirect)]
        public void Invoke_SchemeIsNotHttps_StrictTransportSecurityHeaderIsNotAddedToResponse(int hstsMaxAge, bool hstsIncludeSubDomains, CasStubBehavior casBehavior)
        {
            var nextStub = Substitute.For<RequestDelegate>();
            var httpsOption = new HttpsWithHstsOptions(ValidHttpsMode, hstsMaxAge, hstsIncludeSubDomains, ValidRedirectPort);
            var contextMock = MakeContextFake(false);
            var controllerAttributeSupportStub = MakeControllerAttributeSupportFake(casBehavior, contextMock);
            var sut = new HttpsWithStrictTransportSecurityMiddleware(nextStub, httpsOption, controllerAttributeSupportStub);

            sut.Invoke(contextMock).Wait();

            contextMock.Response.Headers.DidNotReceive().Add(Arg.Any<string>(), Arg.Any<StringValues>());
            contextMock.Response.Headers.DidNotReceive().Add(Arg.Any<KeyValuePair<string, StringValues>>());
        }


        [TestCase(CasStubBehavior.ReturnNull)]
        [TestCase(CasStubBehavior.StubReturnNull)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect)]
        public void Invoke_SchemeIsHttps_NextDelegateIsInvoked(CasStubBehavior casBehavior)
        {
            var nextMock = Substitute.For<RequestDelegate>();
            var httpsOption = new HttpsWithHstsOptions(ValidHttpsMode, ValidHstsMaxAge, HstsIncludeSubDomains, ValidRedirectPort);            
            var contextStub = MakeContextFake(true);
            var controllerAttributeSupportStub = MakeControllerAttributeSupportFake(casBehavior, contextStub);
            var sut = new HttpsWithStrictTransportSecurityMiddleware(nextMock, httpsOption, controllerAttributeSupportStub);

            sut.Invoke(contextStub).Wait();

            nextMock.Received(1).Invoke(contextStub);
        }


        [TestCase(CasStubBehavior.ReturnNull)]
        [TestCase(CasStubBehavior.StubReturnNull)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect)]
        public void Invoke_SchemeIsNotHttps_NextDelegateIsNotInvoked(CasStubBehavior casBehavior)
        {
            var nextMock = Substitute.For<RequestDelegate>();
            var httpsOption = new HttpsWithHstsOptions(ValidHttpsMode, ValidHstsMaxAge, HstsIncludeSubDomains, ValidRedirectPort);
            var contextStub = MakeContextFake(false);
            var controllerAttributeSupportStub = MakeControllerAttributeSupportFake(casBehavior, contextStub);
            var sut = new HttpsWithStrictTransportSecurityMiddleware(nextMock, httpsOption, controllerAttributeSupportStub);

            sut.Invoke(contextStub).Wait();

            nextMock.DidNotReceive().Invoke(Arg.Any<HttpContext>());
        }


        [TestCase(CasStubBehavior.ReturnNull, "GET")]
        [TestCase(CasStubBehavior.StubReturnNull, "GET")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "GET")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "GET")]
        [TestCase(CasStubBehavior.ReturnNull, "HEAD")]
        [TestCase(CasStubBehavior.StubReturnNull, "HEAD")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "HEAD")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "HEAD")]
        [TestCase(CasStubBehavior.ReturnNull, "POST")]
        [TestCase(CasStubBehavior.StubReturnNull, "POST")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "POST")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "POST")]
        [TestCase(CasStubBehavior.ReturnNull, "PUT")]
        [TestCase(CasStubBehavior.StubReturnNull, "PUT")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "PUT")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "PUT")]
        [TestCase(CasStubBehavior.ReturnNull, "DELETE")]
        [TestCase(CasStubBehavior.StubReturnNull, "DELETE")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "DELETE")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "DELETE")]
        [TestCase(CasStubBehavior.ReturnNull, "CONNECT")]
        [TestCase(CasStubBehavior.StubReturnNull, "CONNECT")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "CONNECT")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "CONNECT")]
        [TestCase(CasStubBehavior.ReturnNull, "OPTIONS")]
        [TestCase(CasStubBehavior.StubReturnNull, "OPTIONS")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "OPTIONS")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "OPTIONS")]
        [TestCase(CasStubBehavior.ReturnNull, "TRACE")]
        [TestCase(CasStubBehavior.StubReturnNull, "TRACE")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "TRACE")]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "TRACE")]
        public void Invoke_HttpsModeStrictAndSchemeIsHttp_ResponseStatusCodeSetToForbidden(CasStubBehavior casBehavior, string httpMethod)
        {
            var nextStub = Substitute.For<RequestDelegate>();
            var httpsOption = new HttpsWithHstsOptions(HttpsMode.Strict, ValidHstsMaxAge, HstsIncludeSubDomains, ValidRedirectPort);
            var contextMock = MakeContextFake(false, httpMethod, TestHttpUrl);
            var controllerAttributeSupportStub = MakeControllerAttributeSupportFake(casBehavior, contextMock);
            var sut = new HttpsWithStrictTransportSecurityMiddleware(nextStub, httpsOption, controllerAttributeSupportStub);

            sut.Invoke(contextMock).Wait();

            contextMock.Response.Received(1).StatusCode = (int)HttpStatusCode.Forbidden;
        }


        [TestCase(CasStubBehavior.ReturnNull, "GET", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnNull, "GET", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "GET", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "GET", HttpStatusCode.MovedPermanently)]
        [TestCase(CasStubBehavior.ReturnNull, "HEAD", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnNull, "HEAD", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "HEAD", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "HEAD", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.ReturnNull, "POST", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnNull, "POST", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "POST", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "POST", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.ReturnNull, "PUT", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnNull, "PUT", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "PUT", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "PUT", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.ReturnNull, "DELETE", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnNull, "DELETE", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "DELETE", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "DELETE", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.ReturnNull, "CONNECT", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnNull, "CONNECT", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "CONNECT", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "CONNECT", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.ReturnNull, "OPTIONS", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnNull, "OPTIONS", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "OPTIONS", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "OPTIONS", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.ReturnNull, "TRACE", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnNull, "TRACE", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeForbidden, "TRACE", HttpStatusCode.Forbidden)]
        [TestCase(CasStubBehavior.StubReturnHttpGetModeRedirect, "TRACE", HttpStatusCode.Forbidden)]
        public void Invoke_HttpsModeAllowedRedirectForGetAndSchemeIsHttp_ResponseStatusCodeSetToExpectedStatusCode(CasStubBehavior casBehavior, string httpMethod, int expectedStatusCode)
        {
            var nextStub = Substitute.For<RequestDelegate>();
            var httpsOption = new HttpsWithHstsOptions(HttpsMode.AllowedRedirectForGet, ValidHstsMaxAge, HstsIncludeSubDomains, ValidRedirectPort);
            var contextMock = MakeContextFake(false, httpMethod, TestHttpUrl);
            var controllerAttributeSupportStub = MakeControllerAttributeSupportFake(casBehavior, contextMock);
            var sut = new HttpsWithStrictTransportSecurityMiddleware(nextStub, httpsOption, controllerAttributeSupportStub);

            sut.Invoke(contextMock).Wait();

            contextMock.Response.Received(1).StatusCode = expectedStatusCode;
        }


        [TestCase("http://test.net/go", -1, "https://test.net/go")]
        [TestCase("http://test.net/go", 500, "https://test.net:500/go")]
        [TestCase("http://test.net/go?set=5", -1, "https://test.net/go?set=5")]
        [TestCase("http://test.net/go?set=5", 600, "https://test.net:600/go?set=5")]
        [TestCase("http://test.net:1000/go?set=5", 600, "https://test.net:600/go?set=5")]
        public void Invoke_HttpsModeAllowedRedirectForGetAndHttpGetModeIsRedirectAndSchemeIsHttp_ResponseHeaderLocationIsSet(string url, int redirectPort, string expectedLocationHeaderValue)
        {
            var nextStub = Substitute.For<RequestDelegate>();
            var httpsOption = new HttpsWithHstsOptions(HttpsMode.AllowedRedirectForGet, ValidHstsMaxAge, HstsIncludeSubDomains, redirectPort);
            var contextMock = MakeContextFake(false, "GET", url);
            var controllerAttributeSupportStub = MakeControllerAttributeSupportFake(CasStubBehavior.StubReturnHttpGetModeRedirect, contextMock);
            var sut = new HttpsWithStrictTransportSecurityMiddleware(nextStub, httpsOption, controllerAttributeSupportStub);

            sut.Invoke(contextMock).Wait();

            contextMock.Response.Headers.Received(1).Add("Location", expectedLocationHeaderValue);
            contextMock.Response.Headers.DidNotReceive().Add(Arg.Any<KeyValuePair<string, StringValues>>());
        }


        [TestCase("ftp://test.net/go")]
        [TestCase("svn://test.net:12/go")]
        [TestCase("ssh://test.net:50/go")]
        public void Invoke_HttpsModeAllowedRedirectForGetAndHttpGetModeIsRedirectAndSchemeIsNotHttp_ResponseStatusCodeSetToForbiddenAndNextIsNotInvoked(string url)
        {
            var nextStub = Substitute.For<RequestDelegate>();
            var httpsOption = new HttpsWithHstsOptions(HttpsMode.AllowedRedirectForGet, ValidHstsMaxAge, HstsIncludeSubDomains, ValidRedirectPort);
            var contextMock = MakeContextFake(false, "GET", url);
            var controllerAttributeSupportStub = MakeControllerAttributeSupportFake(CasStubBehavior.StubReturnHttpGetModeRedirect, contextMock);
            var sut = new HttpsWithStrictTransportSecurityMiddleware(nextStub, httpsOption, controllerAttributeSupportStub);

            sut.Invoke(contextMock).Wait();

            contextMock.Response.Received(1).StatusCode = (int)HttpStatusCode.Forbidden;
        }


        [Test]
        public void Constructor_NextIsNull_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HttpsWithStrictTransportSecurityMiddleware(null, new HttpsWithHstsOptions(ValidHttpsMode, ValidHstsMaxAge, HstsIncludeSubDomains, ValidRedirectPort), null));
            Assert.That(ex.ParamName, Is.EqualTo("next"), nameof(ex.ParamName));
        }


        [Test]
        public void Constructor_HttpsOptionsIsNull_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new HttpsWithStrictTransportSecurityMiddleware(context => Task.CompletedTask, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("httpsOptions"), nameof(ex.ParamName));
        }


        private HttpContext MakeContextFake(bool isHttps, string httpMethod = "GET", string requestUrl = TestHttpUrl)
        {
            var contextFake = Substitute.For<HttpContext>();
            contextFake.Request.IsHttps.Returns(isHttps);
            contextFake.Request.Method.Returns(httpMethod);
            var requestUri = new Uri(requestUrl);
            contextFake.Request.Scheme.Returns(requestUri.Scheme);
            contextFake.Request.Host.Returns(requestUri.Port == 80 ? new HostString(requestUri.Host) : new HostString(requestUri.Host, requestUri.Port));
            contextFake.Request.Path.Returns(new PathString(requestUri.AbsolutePath));
            contextFake.Request.QueryString.Returns(new QueryString(requestUri.Query));

            var streamFake = Substitute.For<Stream>();
            streamFake.WriteAsync(null, 0, 0).ReturnsForAnyArgs(Task.CompletedTask);
            contextFake.Response.Body.Returns(streamFake);

            return contextFake;
        }


        private IControllerAttributeSupport MakeControllerAttributeSupportFake(CasStubBehavior stubBehavior, HttpContext context)
        {
            if (stubBehavior == CasStubBehavior.ReturnNull)
                return null;

            var fake = Substitute.For<IControllerAttributeSupport>();
            fake.GetActionAttributeAsync<HttpGetModeAttribute>(context).Returns(Task.FromResult(
                stubBehavior == CasStubBehavior.StubReturnHttpGetModeForbidden
                    ? new HttpGetModeAttribute(HttpGetMode.ReturnForbidden)
                    : stubBehavior == CasStubBehavior.StubReturnHttpGetModeRedirect
                        ? new HttpGetModeAttribute(HttpGetMode.RedirectToHttps)
                        : null));
            return fake;
        }
    }
}
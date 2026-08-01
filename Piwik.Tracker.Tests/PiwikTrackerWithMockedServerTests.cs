using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Piwik.Tracker.Tests
{
    [TestFixture]
    internal class PiwikTrackerWithMockedServerTests
    {
        private PiwikTracker _sut = null!;
        private Mock<HttpMessageHandler> _mockHttpHandler = null!;
        private HttpClient _mockHttpClient = null!;
        private const string UA = "Firefox";
        private const string PiwikBaseUrl = "http://127.0.0.1:1122/piwik.php";
        private const int SiteId = 1;

        private static readonly NameValueCollection DefaultRequestParameter = new NameValueCollection
        {
            { "idsite",SiteId.ToString()},
            { "rec","1"},
            { "apiv","1"},
            { "url","http://unknown" },
        };

        private static readonly string[] DefaultRequestParameterKeysToRemoveFromComparison =
        {
            "r", // random value
            "_idts" // _createTs from cookie
        };

        [SetUp]
        public void SetUpTest()
        {
            _mockHttpHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _mockHttpClient = new HttpClient(_mockHttpHandler.Object);
            _sut = new PiwikTracker(SiteId, PiwikBaseUrl, _mockHttpClient);
        }

        [Test]
        [TestCase("myPage")]
        [TestCase("myPage/?Ü&")]
        public void DoTrackPageView_Test(string documentTitle)
        {
            // Arrange
            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("")
                })
                .Verifiable();

            // Act
            var actual = _sut.DoTrackPageView(documentTitle);

            // Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        [TestCase("myCategory", "myAction", "myName", "myValue")]
        [TestCase("myCategory", "myAction", "myName", "")]
        [TestCase("myCategory", "myAction", "", "myValue")]
        public void DoTrackEvent_Test(string category, string action, string name, string value)
        {
            // Arrange
            var expectedUri = new Uri($"{PiwikBaseUrl}?idsite={SiteId}&rec=1&apiv=1&url=http://unknown&e_c={HttpUtility.UrlEncode(category)}&e_a={HttpUtility.UrlEncode(action)}&_idvc=0&_id={_sut.GetVisitorId()}");
            if (!string.IsNullOrEmpty(name))
            {
                expectedUri = new Uri(expectedUri + $"&e_n={HttpUtility.UrlEncode(name)}");
            }
            if (!string.IsNullOrEmpty(value))
            {
                expectedUri = new Uri(expectedUri + $"&e_v={HttpUtility.UrlEncode(value)}");
            }

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("")
                })
                .Verifiable();

            // Act
            var actual = _sut.DoTrackEvent(category, action, name, value);

            // Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        [TestCase("myCn", "mycp", "myct")]
        [TestCase("myCn", null, "myct")]
        [TestCase("myCn", "mycp", null)]
        public void DoTrackContentImpression_Test(string contentName, string? contentPiece, string? contentTarget)
        {
            // Arrange
            var expectedUri = new Uri($"{PiwikBaseUrl}?idsite={SiteId}&rec=1&apiv=1&url=http://unknown&c_n={HttpUtility.UrlEncode(contentName)}&_idvc=0&_id={_sut.GetVisitorId()}");
            if (!string.IsNullOrEmpty(contentPiece))
            {
                expectedUri = new Uri(expectedUri + $"&c_p={HttpUtility.UrlEncode(contentPiece)}");
            }
            if (!string.IsNullOrEmpty(contentTarget))
            {
                expectedUri = new Uri(expectedUri + $"&c_t={HttpUtility.UrlEncode(contentTarget)}");
            }

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("")
                })
                .Verifiable();

            // Act
            var actual = _sut.DoTrackContentImpression(contentName, contentPiece, contentTarget);

            // Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
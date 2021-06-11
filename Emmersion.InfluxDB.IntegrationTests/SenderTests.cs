using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Emmersion.InfluxDB.IntegrationTests
{
    [TestFixture]
    internal class With_a_base_sender_test : With_a_service_located_ClassUnderTest<Sender>
    {
        protected string ExecuteInfluxQuery(string query, string payload)
        {
            var httpClient = GetInstance<IHttpClient>();
            var settings = GetInstance<Settings>();
            var uri = $"{settings.InfluxSettings.Hostname}:{settings.InfluxSettings.Port}/query?db={settings.InfluxSettings.DbName}&q={query}";
            var message = new HttpRequestMessage(HttpMethod.Get, uri) {Content = new StringContent(payload, Encoding.UTF8)};

            return httpClient.Execute(message).Item2;
        }
    }

    [TestFixture]
    internal class when_sending_payload_over_http : With_a_base_sender_test
    {
        private static string retrieved;
        private static string testFieldValue;
        private static string testMeasurement;
        private static string payload;

        [SetUp]
        public void SetUp()
        {
            retrieved = string.Empty;
            testFieldValue = Guid.NewGuid().ToString();
            testMeasurement = $"test_measurement{Guid.NewGuid():N}";
            payload = $@"{testMeasurement} field_1=""{testFieldValue}""";

            ClassUnderTest.SendPayload(payload);

            Thread.Sleep(millisecondsTimeout: 1000);

            retrieved = ExecuteInfluxQuery($"SELECT * FROM {testMeasurement} WHERE time >= now() - 60s", payload);
        }

        [TearDown]
        public void TearDown()
        {
            retrieved = ExecuteInfluxQuery($"DROP MEASUREMENT {testMeasurement}", payload);
        }

        [Test]
        public void retrieved_should_include_unique_payload()
        {
            Assert.That(retrieved.Contains(testFieldValue));
        }
    }

    [TestFixture]
    internal class when_sending_multi_line_payload_over_http : With_a_base_sender_test
    {
        private static string retrieved;
        private static string testFieldValue1;
        private static string testFieldValue2;
        private static string testMeasurement;
        private static string payload;

        [SetUp]
        public void SetUp()
        {
            retrieved = string.Empty;
            testFieldValue1 = Guid.NewGuid().ToString();
            testFieldValue2 = Guid.NewGuid().ToString();
            testMeasurement = $"test_measurement{Guid.NewGuid():N}";
            payload = $@"{testMeasurement} field_1=""{testFieldValue1}""{Environment.NewLine}{testMeasurement} field_2=""{testFieldValue2}""";

            ClassUnderTest.SendPayload(payload);

            Thread.Sleep(millisecondsTimeout: 1000);

            retrieved = ExecuteInfluxQuery($"SELECT * FROM {testMeasurement} WHERE time >= now() - 60s", payload);
        }

        [TearDown]
        public void TearDown()
        {
            retrieved = ExecuteInfluxQuery($"DROP MEASUREMENT {testMeasurement}", payload);
        }

        [Test]
        public void retrieved_should_include_first_field_value()
        {
            Assert.That(retrieved.Contains(testFieldValue1));
        }

        [Test]
        public void retrieved_should_include_second_field_value()
        {
            Assert.That(retrieved.Contains(testFieldValue2));
        }
    }

    [TestFixture]
    internal class when_sending_payload_over_http_with_bad_settings
    {
        [Test]
        public void should_throw_an_exception()
        {
            var classUnderTest = new Sender(new HttpClient(), new InfluxSettings {Hostname = "", Port = 0, DbName = ""});

            var exception = Assert.Throws<AggregateException>(() => classUnderTest.SendPayload(string.Empty));
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
        }
    }

    [TestFixture]
    internal class when_sending_payload_over_http_returns_unexpected_status_code : With_a_base_sender_test
    {
        [Test]
        public void should_throw_an_exception()
        {
            Assert.Throws<Exception>(() => ClassUnderTest.SendPayload("this-payload-will-return-400-status-code"));
        }
    }
}
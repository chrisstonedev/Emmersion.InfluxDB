using System;
using System.Threading;
using EL.Http;
using NUnit.Framework;

namespace EL.InfluxDB.IntegrationTests
{
    [TestFixture]
    public class when_sending_payload_over_http
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var classUnderTest = new Sender(new IntegrationSettings(), new HttpClient());
            classUnderTest.SendPayload(payload);

            Thread.Sleep(1000);

            var influxTestClient = new InfluxTestClient();
            retrieved = influxTestClient.Query($"SELECT * FROM {testMeasurement} WHERE time >= now() - 60s");
        }

        [Test]
        public void retrieved_should_include_unique_payload() => Assert.That(retrieved.Contains(testFieldValue));

        static string retrieved = string.Empty;
        static string testFieldValue = Guid.NewGuid().ToString();
        static string testMeasurement = $"test_measurement{Guid.NewGuid():N}";
        static string payload = $@"{testMeasurement} field_1=""{testFieldValue}""";

        [OneTimeTearDown]
        public void TearDown()
        {
            var influxTestClient = new InfluxTestClient();
            influxTestClient.Command($"DROP MEASUREMENT {testMeasurement}");
        }
    }

    [TestFixture]
    public class when_sending_multi_line_payload_over_http
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var classUnderTest = new Sender(new IntegrationSettings(), new HttpClient());
            classUnderTest.SendPayload(payload);

            Thread.Sleep(1000);

            var influxTestClient = new InfluxTestClient();
            retrieved = influxTestClient.Query($"SELECT * FROM {testMeasurement} WHERE time >= now() - 60s");
        }

        [Test]
        public void retrieved_should_include_first_field_value() => Assert.That(retrieved.Contains(testFieldValue1));
        public void retrieved_should_include_second_field_value() => Assert.That(retrieved.Contains(testFieldValue2));

        static string retrieved = string.Empty;
        static string testFieldValue1 = Guid.NewGuid().ToString();
        static string testFieldValue2 = Guid.NewGuid().ToString();
        static string testMeasurement = $"test_measurement{Guid.NewGuid():N}";
        static string payload = $@"{testMeasurement} field_1=""{testFieldValue1}""{Environment.NewLine}{testMeasurement} field_2=""{testFieldValue2}""";

        [OneTimeTearDown]
        public void TearDown()
        {
            var influxTestClient = new InfluxTestClient();
            influxTestClient.Command($"DROP MEASUREMENT {testMeasurement}");
        }
    }

    [TestFixture]
    public class when_sending_payload_over_http_with_bad_settings
    {
        [Test]
        public void should_throw_an_exception()
        {
            var classUnderTest = new Sender(new InfluxSettings("", 0, ""), new HttpClient());

            var exception = Assert.Throws<AggregateException>(() => classUnderTest.SendPayload(string.Empty));
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
        }
    }

    [TestFixture]
    public class when_sending_payload_over_http_returns_unexpected_status_code
    {
        [Test]
        public void should_throw_an_exception()
        {
            var httpSettings = new IntegrationSettings();
            var classUnderTest = new Sender(httpSettings, new HttpClient());

            Assert.Throws<Exception>(() => classUnderTest.SendPayload("this-payload-will-return-400-status-code"));
        }
    }
}
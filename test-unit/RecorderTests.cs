using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EL.InfluxDB.UnitTests
{
    public class RecorderTests
    {
        private readonly InfluxSettings settings = new InfluxSettings("http://localhost", 6000, "test-database");
        private InfluxRecorder classUnderTest;
        private MockSender mockSender;

        [SetUp]
        public void Setup()
        {
            mockSender = new MockSender();
            classUnderTest = new InfluxRecorder(mockSender, settings, new StubLogger());
        }

        [TearDown]
        public void TearDown()
        {
            classUnderTest.Dispose();
        }

        [Test]
        public void when_sending_a_single_point()
        {
            var timestamp = DateTimeOffset.Parse("2018-08-21T01:02:03Z");
            classUnderTest.Record(new InfluxPoint("test-measurement", new[] {new Field("count", 1)}, timestamp));
            Assert.That(mockSender.SentPayloads.Count, Is.EqualTo(expected: 0));

            SleepOneBatchInterval();
            Assert.That(mockSender.SentPayloads.Count, Is.EqualTo(expected: 1));
            Assert.That(mockSender.SentPayloads[0], Is.EqualTo("test-measurement count=1 1534813323000000000"));
        }

        [Test]
        public void when_sending_many_points_from_multiple_threads()
        {
            var pointsToRecordPerTask = 5000;
            settings.MaxBatchSize = 10000;

            var tasks = new[]
            {
                Task.Factory.StartNew(() => RecordTaskPoints(1, pointsToRecordPerTask)),
                Task.Factory.StartNew(() => RecordTaskPoints(2, pointsToRecordPerTask)),
                Task.Factory.StartNew(() => RecordTaskPoints(3, pointsToRecordPerTask))
            };

            Task.WaitAll(tasks);
            SleepOneBatchInterval();

            Assert.That(mockSender.SentPayloads.Count > 1);

            var allSentData = string.Join("\n", mockSender.SentPayloads);
            for (var task = 1; task <= 3; task++)
            {
                for (var count = 0; count < pointsToRecordPerTask; count++)
                {
                    var expected = $"task-{task}-measurement count={count}";
                    Assert.That(allSentData.Contains(expected), Is.True, $"Expected to find {expected}");
                }
            }
        }

        [Test]
        public void when_sending_more_points_than_the_batch_size()
        {
            settings.MaxBatchSize = 5;
            for (var i = 0; i < 11; i++)
            {
                classUnderTest.Record(new InfluxPoint("test-measurement", new[] {new Field("count", i)}));
            }

            Assert.That(mockSender.SentPayloads.Count, Is.EqualTo(expected: 0));

            SleepOneBatchInterval();
            Assert.That(mockSender.SentPayloads.Count, Is.EqualTo(expected: 3));
        }

        private void RecordTaskPoints(int taskNumber, int count)
        {
            for (var i = 0; i < count; i++)
            {
                classUnderTest.Record(new InfluxPoint($"task-{taskNumber}-measurement", new[] {new Field("count", i)}));
            }
        }

        private void SleepOneBatchInterval()
        {
            Thread.Sleep(settings.BatchIntervalInSeconds * 1000 + 250);
        }
    }

    internal class MockSender : ISender
    {
        public List<string> SentPayloads = new List<string>();

        public void SendPayload(string payload)
        {
            SentPayloads.Add(payload);
        }
    }

    internal class StubLogger : IInfluxLogger
    {
        public void Error(string message, Exception exception)
        {
        }

        public void Debug(string message)
        {
        }
    }
}
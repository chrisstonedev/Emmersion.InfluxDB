using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace EL.InfluxDB.UnitTests
{
    internal class RecorderTests : With_an_automocked<InfluxRecorder>
    {
        private readonly InfluxSettings settings = new InfluxSettings("http://localhost", influxPort: 6000, "test-database") {BatchIntervalInSeconds = 0, MaxBatchSize = 5};

        [Test]
        public void when_sending_a_single_point()
        {
            var timestamp = DateTimeOffset.Parse("2018-08-21T01:02:03Z");
            ClassUnderTest.Record(new InfluxPoint("test-measurement", new[] {new Field("count", value: 1)}, timestamp));

            GetMock<ISender>().Verify(x=>x.SendPayload(IsAny<string>()), Times.Never);

            SleepOneBatchInterval();

            GetMock<ISender>().Verify(x=>x.SendPayload("test-measurement count=1 1534813323000000000"));
            GetMock<ISender>().Verify(x=>x.SendPayload(IsAny<string>()), Times.Exactly(1));
        }

        [Test]
        public void when_sending_many_points_from_multiple_threads()
        {
            var pointsToRecordPerTask = 5000;
            settings.MaxBatchSize = 10000;

            var tasks = new[]
            {
                Task.Factory.StartNew(() => RecordTaskPoints(taskNumber: 1, pointsToRecordPerTask)),
                Task.Factory.StartNew(() => RecordTaskPoints(taskNumber: 2, pointsToRecordPerTask)),
                Task.Factory.StartNew(() => RecordTaskPoints(taskNumber: 3, pointsToRecordPerTask))
            };

            Task.WaitAll(tasks);
            SleepOneBatchInterval();

            GetMock<ISender>().Verify(x=>x.SendPayload(IsAny<string>()), Times.Exactly(1));

            for (var task = 1; task <= 3; task++)
            {
                for (var count = 0; count < pointsToRecordPerTask; count++)
                {
                    var expected = $"task-{task}-measurement count={count}";
                    GetMock<ISender>().Verify(x=>x.SendPayload(expected));
                }
            }
        }

        [Test]
        public void when_sending_more_points_than_the_batch_size()
        {
            settings.MaxBatchSize = 5;
            for (var i = 0; i < 11; i++)
            {
                ClassUnderTest.Record(new InfluxPoint("test-measurement", new[] {new Field("count", i)}));
            }

            GetMock<ISender>().Verify(x=>x.SendPayload(IsAny<string>()), Times.Never);

            SleepOneBatchInterval();
            GetMock<ISender>().Verify(x=>x.SendPayload(IsAny<string>()), Times.Exactly(3));
        }

        private void RecordTaskPoints(int taskNumber, int count)
        {
            for (var i = 0; i < count; i++)
            {
                ClassUnderTest.Record(new InfluxPoint($"task-{taskNumber}-measurement", new[] {new Field("count", i)}));
            }
        }

        private void SleepOneBatchInterval()
        {
            Thread.Sleep(settings.BatchIntervalInSeconds * 1000 + 250);
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace EL.InfluxDB.UnitTests
{
    internal class RecorderTests : With_an_automocked<InfluxRecorder>
    {
        [Test]
        public void when_sending_a_single_point()
        {
            GetMock<IInfluxSettings>().Setup(x => x.BatchIntervalInSeconds).Returns(1);
            GetMock<IInfluxSettings>().Setup(x => x.MaxBatchSize).Returns(5);

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
            GetMock<IInfluxSettings>().Setup(x => x.BatchIntervalInSeconds).Returns(1);
            GetMock<IInfluxSettings>().Setup(x => x.MaxBatchSize).Returns(10000);

            var tasks = new[]
            {
                Task.Factory.StartNew(() => RecordTaskPoints(taskNumber: 1, pointsToRecordPerTask)),
                Task.Factory.StartNew(() => RecordTaskPoints(taskNumber: 2, pointsToRecordPerTask)),
                Task.Factory.StartNew(() => RecordTaskPoints(taskNumber: 3, pointsToRecordPerTask))
            };

            Task.WaitAll(tasks);
            SleepOneBatchInterval();

            GetMock<ISender>().Verify(x=>x.SendPayload(IsAny<string>()), Times.AtLeastOnce);

            for (var task = 1; task <= 3; task++)
            {
                for (var count = 0; count < pointsToRecordPerTask; count++)
                {
                    var expected = $"task-{task}-measurement count={count}";
                    GetMock<ISender>().Verify(x=>x.SendPayload(It.Is<string>(y=> y.Contains(expected))));
                }
            }
        }

        [Test]
        public void when_sending_more_points_than_the_batch_size()
        {
            GetMock<IInfluxSettings>().Setup(x => x.BatchIntervalInSeconds).Returns(1);
            GetMock<IInfluxSettings>().Setup(x => x.MaxBatchSize).Returns(5);
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
            Thread.Sleep(1250);
        }
    }
}
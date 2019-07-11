using System;
using NUnit.Framework;

namespace EL.InfluxDB.UnitTests
{
    public class DateTimeExtensionsTests
    {
        [Test]
        public void UnixTimeNanoseconds()
        {
            var now = DateTimeOffset.UtcNow;
            var unixMilli = now.ToUnixTimeMilliseconds();
            var unixNano = now.ToUnixTimeNanoseconds();
            Assert.That(unixNano / 1000000, Is.EqualTo(unixMilli));
        }
    }
}
using System;

namespace EL.InfluxDB.IntegrationTests
{
    public class InfluxLogger : IInfluxLogger
    {
        public void Error(string message, Exception exception)
        {
            Console.WriteLine($"ERROR! {message}: {exception.Message}");
        }

        public void Debug(string message)
        {
            Console.WriteLine($"DEBUG {message}");
        }
    }
}
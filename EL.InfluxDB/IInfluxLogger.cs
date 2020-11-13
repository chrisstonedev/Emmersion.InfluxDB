using System;

namespace EL.InfluxDB
{
    public interface IInfluxLogger
    {
        void Debug(string message);
        void Error(string message, Exception exception);
    }
}
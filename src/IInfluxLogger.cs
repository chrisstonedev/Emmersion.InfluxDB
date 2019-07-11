using System;

namespace EL.InfluxDB
{
    public interface IInfluxLogger
    {
        void Error(string message, Exception exception);
        void Debug(string message);
    }
}

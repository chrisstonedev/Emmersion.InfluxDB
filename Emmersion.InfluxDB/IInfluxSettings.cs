namespace Emmersion.InfluxDB
{
    public interface IInfluxSettings
    {
        /// <summary>
        ///     Typical value: http://localhost
        /// </summary>
        string Hostname { get; }

        /// <summary>
        ///     Typical value: 8086
        /// </summary>
        int Port { get; }

        /// <summary>
        ///     Typical value: _internal
        /// </summary>
        string DbName { get; }

        /// <summary>
        ///     Typical value: 1
        /// </summary>
        int BatchIntervalInSeconds { get; }

        /// <summary>
        ///     Typical value: 10000
        /// </summary>
        int MaxBatchSize { get; }
    }
}
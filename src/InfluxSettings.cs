namespace EL.InfluxDB
{
    public interface IInfluxSettings
    {
        string InfluxHostname { get; }
        int InfluxPort { get; }
        string InfluxDbName { get; }
        int BatchIntervalInSeconds { get; set; }
        int MaxBatchSize { get; set; }
        string ConnectionString { get; }
    }

    internal class InfluxSettings : IInfluxSettings
    {
        public InfluxSettings(string influxHostName, int influxPort, string influxDbName)
        {
            InfluxHostname = influxHostName;
            InfluxPort = influxPort;
            InfluxDbName = influxDbName;
        }

        public string InfluxHostname { get; }
        public int InfluxPort { get; }
        public string InfluxDbName { get; }
        public int BatchIntervalInSeconds { get; set; } = 1;
        public int MaxBatchSize { get; set; } = 10000;
        public string ConnectionString => $"{InfluxHostname}:{InfluxPort}/write?db={InfluxDbName}";
    }
}
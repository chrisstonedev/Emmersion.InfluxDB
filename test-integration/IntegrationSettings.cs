namespace EL.InfluxDB.IntegrationTests
{
    public class IntegrationSettings : InfluxSettings
    {
        private static readonly Settings settings = new Settings();

        public IntegrationSettings() : base(settings.GetRequiredSetting<string>("InfluxDB:Hostname"), 
            settings.GetRequiredSetting<int>("InfluxDB:Port"),
            settings.GetRequiredSetting<string>("InfluxDB:DbName"))
        {
        }
    }
}
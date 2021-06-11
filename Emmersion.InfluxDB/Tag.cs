namespace Emmersion.InfluxDB
{
    public class Tag
    {
        public Tag(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }
    }
}
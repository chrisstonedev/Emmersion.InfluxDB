namespace EL.InfluxDB
{
    public class Field
    {
        public Field(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public object Value { get; }
    }
}
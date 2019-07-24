using System;

namespace EL.InfluxDB
{
    public class InfluxPoint
    {
        
        public InfluxPoint(string measurementName, Field[] fields)
            : this(measurementName, fields, tags: null, DateTimeOffset.UtcNow)
        {
        }

        
        public InfluxPoint(string measurementName, Field[] fields, Tag[] tags)
            : this(measurementName, fields, tags, DateTimeOffset.UtcNow)
        {
        }

        
        public InfluxPoint(string measurementName, Field[] fields, DateTimeOffset timestamp)
            : this(measurementName, fields, tags: null, timestamp)
        {
        }

        
        public InfluxPoint(string measurementName, Field[] fields, Tag[] tags, DateTimeOffset timestamp)
        {
            MeasurementName = measurementName;
            Timestamp = timestamp;
            Fields = fields;
            Tags = tags;
        }

        public string MeasurementName { get; }
        public DateTimeOffset Timestamp { get; }
        public Field[] Fields { get; }
        public Tag[] Tags { get; }
    }
}
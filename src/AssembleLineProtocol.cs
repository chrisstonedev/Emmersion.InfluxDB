using System;
using System.Linq;

namespace EL.InfluxDB
{
    public class InfluxPoint
    {
        public string MeasurementName { get; }
        public DateTimeOffset Timestamp { get; }
        public Field[] Fields { get; }
        public Tag[] Tags { get; }

        public InfluxPoint(string measurementName, Field[] fields)
            : this(measurementName, fields, null, DateTimeOffset.UtcNow) { }

        public InfluxPoint(string measurementName, Field[] fields, Tag[] tags)
            : this(measurementName, fields, tags, DateTimeOffset.UtcNow) { }

        public InfluxPoint(string measurementName, Field[] fields, DateTimeOffset timestamp)
            : this(measurementName, fields, null, timestamp) { }

        public InfluxPoint(string measurementName, Field[] fields, Tag[] tags, DateTimeOffset timestamp)
        {
            MeasurementName = measurementName;
            Timestamp = timestamp;
            Fields = fields;
            Tags = tags;
        }
    }

    public class Tag
    {
        public string Key { get; }
        public string Value { get; }

        public Tag(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    public class Field
    {
        public string Key { get; }
        public object Value { get; }

        public Field(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    public class AssembleLineProtocol
    {
        public static string Assemble(InfluxPoint point)
        {
            var formattedFields = string.Join(",", point.Fields.Select(FormatOneField));
            var formattedTags = point.Tags != null ? string.Join("", point.Tags.Select(FormatOneTag)) : "";
            var formattedTimestamp = " " + point.Timestamp.ToUnixTimeNanoseconds();

            return $"{EscapeMeasurementName(point.MeasurementName)}{formattedTags} {formattedFields}{formattedTimestamp}";
        }

        static string FormatOneField(Field field)
        {
            var fieldValueStr = IsNativeInfluxDataType(field.Value)
                ? field.Value.ToString()
                : $"\"{EscapeFieldValue(field.Value.ToString())}\"";
            return $"{EscapeFieldKey(field.Key)}={fieldValueStr}";
        }

        static string FormatOneTag(Tag tag) => $",{EscapeTagKey(tag.Key)}={EscapeTagValue(tag.Value)}";

        static string EscapeMeasurementName(string s) => s.Replace(",", @"\,").Replace(" ", @"\ ");

        static string EscapeFieldKey(string s) => s.Replace(",", @"\,").Replace(" ", @"\ ").Replace("=", @"\=");

        static string EscapeFieldValue(string s) => s.Replace(@"""", @"\""");

        static string EscapeTagKey(string s) => EscapeFieldKey(s);

        static string EscapeTagValue(string s) => EscapeFieldKey(s);

        static bool IsNativeInfluxDataType(object value) => value is bool || IsNumeric(value);

        private static bool IsNumeric(object value)
        {
            return value is sbyte ||
                   value is short ||
                   value is int ||
                   value is long ||
                   value is byte ||
                   value is ushort ||
                   value is uint ||
                   value is ulong ||
                   value is float ||
                   value is double ||
                   value is decimal;
        }
    }

    public static class DateTimeExtensions
    {
        //TODO: make platform independent. Suspicion is that this will not work on non-Windows host.
        public static long ToUnixTimeNanoseconds(this DateTimeOffset timestamp)
        {
            var epochTicks = new DateTime(1970, 1, 1).Ticks;
            var ticks = timestamp.Ticks - epochTicks;
            return ticks * 100;
        }
    }
}
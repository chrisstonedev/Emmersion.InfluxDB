using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EL.InfluxDB
{
    internal class AssembleLineProtocol
    {
        
        public static string Assemble(InfluxPoint point)
        {
            var formattedFields = string.Join(",", point.Fields.Select(FormatOneField));
            var formattedTags = point.Tags != null ? string.Join("", point.Tags.Select(FormatOneTag)) : "";
            var formattedTimestamp = $" {point.Timestamp.ToUnixTimeNanoseconds()}";

            return $"{EscapeMeasurementName(point.MeasurementName)}{formattedTags} {formattedFields}{formattedTimestamp}";
        }

        private static string EscapeFieldKey(string s)
        {
            return s.Replace(",", @"\,").Replace(" ", @"\ ").Replace("=", @"\=");
        }

        private static string EscapeFieldValue(string s)
        {
            return s.Replace(@"""", @"\""");
        }

        private static string EscapeMeasurementName(string s)
        {
            return s.Replace(",", @"\,").Replace(" ", @"\ ");
        }

        private static string EscapeTagKey(string s)
        {
            return EscapeFieldKey(s);
        }

        private static string EscapeTagValue(string s)
        {
            return EscapeFieldKey(s);
        }

        private static string FormatOneField(Field field)
        {
            var fieldValueStr = IsNativeInfluxDataType(field.Value)
                ? field.Value.ToString()
                : $"\"{EscapeFieldValue(field.Value.ToString())}\"";
            return $"{EscapeFieldKey(field.Key)}={fieldValueStr}";
        }

        private static string FormatOneTag(Tag tag)
        {
            return $",{EscapeTagKey(tag.Key)}={EscapeTagValue(tag.Value)}";
        }

        private static bool IsNativeInfluxDataType(object value)
        {
            return value is bool || IsNumeric(value);
        }

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

    
    internal static class DateTimeExtensions
    {
        //TODO: make platform independent. Suspicion is that this will not work on non-Windows host.
        
        internal static long ToUnixTimeNanoseconds(this DateTimeOffset timestamp)
        {
            var epochTicks = new DateTime(year: 1970, month: 1, day: 1).Ticks;
            var ticks = timestamp.Ticks - epochTicks;
            return ticks * 100;
        }
    }
}
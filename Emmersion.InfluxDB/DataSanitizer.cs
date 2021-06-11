using System.Text.RegularExpressions;

namespace Emmersion.InfluxDB
{
    
    internal interface IDataSanitizer
    {
        string SanitizeUrl(string input);
    }

    
    internal class DataSanitizer : IDataSanitizer
    {
        public string SanitizeUrl(string input)
        {
            var sanitized = Regex.Replace(input, @"(?i)\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b", "{guid}");
            sanitized = Regex.Replace(sanitized, @"authenticate/.+?/truenorth", "authenticate/{id}/truenorth");
            sanitized = Regex.Replace(sanitized, @"search/.+", "search/");
            sanitized = Regex.Replace(sanitized, @"Users/lostpassword/.+", "Users/lostpassword/{id}");
            sanitized = Regex.Replace(sanitized, @"/\d+", "/{id}");

            return sanitized;
        }
    }
}
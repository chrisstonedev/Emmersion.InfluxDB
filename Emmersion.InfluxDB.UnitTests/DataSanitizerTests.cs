using NUnit.Framework;

namespace Emmersion.InfluxDB.UnitTests
{
    public class DataSanitizerTests
    {
        [TestCase("/api/Accounts/customdomains", "/api/Accounts/customdomains")]
        [TestCase("/api/Users/userObjectId=DE22B826-68FC-48E5-A1D2-0F2803ACB0E5", "/api/Users/userObjectId={guid}")]
        [TestCase("/api/adminsettings/GetSettingByCodeName/DE22B826-68FC-48E5-A1D2-0F2803ACB0E5/ScoreType", "/api/adminsettings/GetSettingByCodeName/{guid}/ScoreType")]
        [TestCase("/api/Users/authenticate/dave-1/truenorth", "/api/Users/authenticate/{id}/truenorth")]
        [TestCase("/api/Users/authenticate/davida/truenorth", "/api/Users/authenticate/{id}/truenorth")]
        [TestCase("/api/Users/authenticate/1234/truenorth", "/api/Users/authenticate/{id}/truenorth")]
        [TestCase("/api/settings/getBySettingId/37", "/api/settings/getBySettingId/{id}")]
        [TestCase("/api/Users/lostpassword/sarahtwald@gmail.com", "/api/Users/lostpassword/{id}")]
        [TestCase("/api/Users/lostpassword/yessica.garridocontreras@cl.nsg.com", "/api/Users/lostpassword/{id}")]
        public void SanitizeUrl_makes_url_safe_for_grafana(string input, string expected)
        {
            Assert.That(new DataSanitizer().SanitizeUrl(input), Is.EqualTo(expected));
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace LinkUs.Modules.Default.ClientInformation
{
    public class NetHelper
    {
        const string Url = "http://checkip.dyndns.org";
        public static readonly Regex FindPublicIpRegex = new Regex(@"<body>Current IP Address: (?<publicIp>.*)<\/body>");

        public static string GetPublicIp()
        {
            try {
                var webRequest = WebRequest.Create(Url);
                var webResponse = webRequest.GetResponse();
                using (var streamReader = new StreamReader(webResponse.GetResponseStream())) {
                    var plainTextResponse = streamReader.ReadToEnd().Trim();
                    var match = FindPublicIpRegex.Match(plainTextResponse);
                    return match.Groups["publicIp"].Value;
                }
            }
            catch (Exception) {
                return "--";
            }
        }
    }
}
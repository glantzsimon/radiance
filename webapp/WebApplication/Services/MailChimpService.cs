using K9.WebApplication.Config;
using System.IO;
using System.Net;

namespace K9.WebApplication.Services
{
    public class MailChimpService : IMailChimpService
    {
        private readonly MailChimpConfiguration _config;

        public MailChimpService(MailChimpConfiguration config)
        {
            _config = config;
        }

        public string Call(string method, string requestJson)
        {
            //var mc = new MailChimp.Net.MailChimpManager(_config.MailChimpApiKey);
            //var mc = new MailChimp.Net.Core.
            //var list = mc.Lists.GetAsync("sdf");

            var endpoint = $"https://us15.api.mailchimp.com/3.0/{method}";
            var wc = new WebClient();
            try
            {
                return wc.UploadString(endpoint, requestJson);
            }
            catch (WebException we)
            {
                using (var sr = new StreamReader(we.Response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
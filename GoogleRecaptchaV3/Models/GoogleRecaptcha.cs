using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using RestSharp;
using System.Net;

namespace GoogleRecaptchaV3.Models
{
    public class GoogleRecaptcha
    {
        private RecaptchaSettings _settings;
        private readonly ILogger<GoogleRecaptcha> _logger;

        public GoogleRecaptcha(IOptions<RecaptchaSettings> settings, ILogger<GoogleRecaptcha> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> VerifyreGoogleRecaptcha(string token)
        {
            GoogleRecaptchaData MyData = new GoogleRecaptchaData
            {
                response = token,
                secret = _settings.RecaptchaSecretKey
            };
            try
            {
                string apiUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={MyData.secret}&response={MyData.response}";
                
                using (HttpClient client = new HttpClient())
                {
                    var httpResult = await client.GetAsync(apiUrl);
                    if (httpResult.StatusCode != HttpStatusCode.OK)
                    {
                        return false;
                    }

                    var responseString = await httpResult.Content.ReadAsStringAsync();
                    var googleApiResponse = JsonConvert.DeserializeObject<GoogleResponse>(responseString);

                    return googleApiResponse.success && googleApiResponse.score >= 0.5;
                }               
            }
            catch (Exception e)
            {
                _logger.LogInformation($"{e.Message}");
                return false;
            }
        }
    }

    public class GoogleRecaptchaData
    {
        public string response { get; set; }
        public string secret { get; set; }
    }

    public class GoogleResponse
    {
        public bool success { get; set; }
        public double score { get; set; }
        public string action { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
    }
}

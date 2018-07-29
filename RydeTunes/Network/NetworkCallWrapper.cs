using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RydeTunes.Network
{
    class NetworkCallWrapper
    {
        public static async Task<T> ParseResponse<T>(HttpResponseMessage message, HttpStatusCode expectedStatusCode)
        {
            if (expectedStatusCode != message.StatusCode)
            {
                throw new Exception("Expected status code was not correct, instead found " + message.StatusCode + ":\n" + await message.Content.ReadAsStringAsync() + "\n" + message.RequestMessage.RequestUri);
            } else
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(await message.Content.ReadAsStringAsync());
            }
        }
    }
}

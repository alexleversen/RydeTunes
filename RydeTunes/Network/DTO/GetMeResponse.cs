using System;
using System.Collections.Generic;
using System.Text;

namespace RydeTunes.Network.DTO
{
    class GetMeResponse
    {
        public string birthdate { get; set; }
        public string country { get; set; }
        public string display_name { get; set; }
        public string email { get; set; }
        public ExternalUrls external_urls { get; set; }
        public Followers followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public List<Image> images { get; set; }
        public string product { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }
    
    public class Followers
    {
        public object href { get; set; }
        public int total { get; set; }
    }
    
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WeatherDisplay {
    class MainClass {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            new ObtainZipCode();

            Console.ReadKey();
        }
    }

    class ObtainZipCode {

        WeatherData wd;

        public ObtainZipCode() {
            wd = new WeatherData();
            wd.setZip(getZip());
            validateZip(wd.getZip());
            try {
                String rawJsonData = formatRawJsonData(downloadWebData(getZipCodeURL()));
                WeatherJson account = JsonConvert.DeserializeObject<WeatherJson>(rawJsonData);
            } catch (WebException webExec) {
              //  Console.WriteLine(webExec);
                Console.WriteLine("\nSorry, that zip code is not valid.\n");
            }

        }

        public String getZip() {
            Console.Write("Enter your zip code: ");
            return Console.ReadLine().Trim();
        }

        public String formatRawJsonData(String retreivedURLData) {
            String formattedJsonData = retreivedURLData.Replace("\"places\": [{", "");
            return formattedJsonData.Replace("}]", "");

        }

        private Boolean validateZip(String userZip) {
            char[] characterZip = userZip.ToCharArray();
            if (userZip.Length != 5) {
                return false;
            }
            for (int i = 0; i < userZip.Length; i++) {
                if (Char.IsLetter(characterZip[i]) == true) {
                    return false;
                }
            }
            return true;
        }

        private String getZipCodeURL() {
            return "https://api.zippopotam.us/us/" + wd.getZip();
        }

        private String downloadWebData(String userURL) {
            String responseString = string.Empty;
            using (var webClient = new WebClient()) {
                responseString = webClient.DownloadString(userURL);
            }
            //Console.WriteLine(responseString);
            return responseString;
        }

    }


    class WeatherData {
        private String zip;

        public String getZip() {
            return this.zip;
        }

        public void setZip(String userZip) {
            this.zip = userZip;
        }

    }

    class WeatherJson {
        [JsonProperty(PropertyName = "post code")]
        public String postCode { get; set; }
        public String country { get; set; }
        [JsonProperty(PropertyName = "country abbreviation")]
        public String countryAbbreviation { get; set; }
        [JsonProperty(PropertyName = "place name")]
        public String placeName { get; set; }
        public String longitude { get; set; }
        public String state { get; set; }
        [JsonProperty(PropertyName = "state abbreviation")]
        public String stateAbbreviation { get; set; }
        public String latitude { get; set; }
    }
}

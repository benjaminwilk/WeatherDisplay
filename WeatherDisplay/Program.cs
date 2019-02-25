using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WeatherDisplay {
    class MainClass {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            WeatherData wd = new WeatherData();
            new ObtainZipCode(wd);
            new ObtainICAO(wd);

            Console.ReadKey();
        }

        public static String downloadWebData(String userURL) {
            String responseString = string.Empty;
            using (var webClient = new WebClient()) {
                responseString = webClient.DownloadString(userURL);
            }
            //Console.WriteLine(responseString);
            return responseString;
        }
    }

    class ObtainZipCode {

        public ObtainZipCode(WeatherData wd) {
            wd.setZip(userInputZip());
            if (validateZip(wd.getZip()) == true) {
                try {
                    String rawJsonData = formatRawJsonData(MainClass.downloadWebData(getZipCodeURL(wd.getZip())));
                    WeatherJson account = JsonConvert.DeserializeObject<WeatherJson>(rawJsonData);
                    parseFormattedJsonData(account, wd);
                } catch (WebException webExec) {
                    Quitter(webExec);
                }
            } else {
                Quitter();
            }

        }

        public String userInputZip() {
            Console.Write("Enter your zip code: ");
            return Console.ReadLine();
        }

        private String formatRawJsonData(String rawURLData) {
            return rawURLData.Replace("\"places\": [{", "").Replace("}]", "");
            //return formattedJsonData.Replace("}]", "");
        }

        private Boolean validateZip(String userZip) {
            if (checkZipLength(userZip) == true || checkZipContents(userZip) == true) {
                return true;
            }
            return false;
        }

        private Boolean checkZipContents(String userZip) {
            char[] userZipToChar = userZip.ToCharArray();
            for (int i = 0; i < userZip.Length; i++) {
                if (Char.IsLetter(userZipToChar[i]) == true) {
                    return false;
                }
            }
            return true;
        }

        private void parseFormattedJsonData(WeatherJson weatherJson, WeatherData wd) {
            wd.setLFN(weatherJson.placeName, weatherJson.stateAbbreviation);
        }

        private Boolean checkZipLength(String userZip) {
            if (userZip.Length == 5) {
                return true;
            }
            return false;
        }

        private String getZipCodeURL(String passedZip) {
            return "https://api.zippopotam.us/us/" + passedZip;
        }

        private void Quitter(WebException webexec) {
            Console.WriteLine(webexec + "\n\n");
            Console.WriteLine("\nSorry, that zip code is not valid.\n");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

        private void Quitter() {
            Console.WriteLine("\nSorry, that zip code is not valid.\n");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

    }

    class ObtainICAO {

        public ObtainICAO(WeatherData wd) {
            data();
            //String rawAirportData = MainClass.downloadWebData(getAiportIcaoURL(wd.getZip()));
            //Console.WriteLine(rawAirportData);
        }

        private String getAiportIcaoURL(String userZip) {
            return "https://www.allplaces.us/afz.cgi?s=" +userZip + "&rad=30";
        }

        public void data() {
            String airportURL = @"https://www.travelmath.com/nearest-airport/48236";
            HtmlWeb webData = new HtmlWeb();
            var htmlDoc = webData.Load(airportURL);
            var node = htmlDoc.DocumentNode.SelectSingleNode(".//body/div[@id='wrapper']");
            var node1 = node.SelectSingleNode(".//div[@id='EchoTopic']");
            Console.WriteLine("Node Name: " + node1.OuterHtml);
        }
    }


    class WeatherData {
        private String zip;
        private String longFormName;
        private String ICAO;

        public String getZip() {
            return this.zip;
        }

        public void setZip(String userZip) {
            this.zip = userZip.Trim();
        }

        public String getIcao() {
            return this.ICAO;
        }

        public void setIcao(String userIcao) {
            this.ICAO = userIcao;
        }

        public String getLFN() {
            return this.longFormName;
        }

        public void setLFN(String userCity, String userState) {
            this.longFormName = userCity + ", " + userState;
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

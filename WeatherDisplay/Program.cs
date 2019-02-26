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
using System.Text.RegularExpressions;
using System.Collections;

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

        public static void Quitter(WebException webexec) {
            Console.WriteLine(webexec + "\n\n");
            Console.WriteLine("\nSorry, that zip code is not valid.\n");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

        public static void Quitter() {
            Console.WriteLine("\nSorry, that zip code is not valid.\n");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

    }

    class ObtainZipCode {

        public ObtainZipCode(WeatherData wd) {
            wd.SetZip(UserInputZip());
            if (ValidateZip(wd.GetZip()) == true) {
                try {
                    String rawJsonData = FormatRawJsonData(MainClass.downloadWebData(GetZipCodeURL(wd.GetZip())));
                    WeatherJson account = JsonConvert.DeserializeObject<WeatherJson>(rawJsonData);
                    ParseFormattedJsonData(account, wd);
                } catch (WebException webExec) {
                    MainClass.Quitter(webExec);
                }
            } else {
                MainClass.Quitter();
            }
        }

        public String UserInputZip() {
            Console.Write("Enter your zip code: ");
            return Console.ReadLine();
        }

        private String FormatRawJsonData(String rawURLData) {
            return rawURLData.Replace("\"places\": [{", "").Replace("}]", "");
        }

        private Boolean ValidateZip(String userZip) {
            if (CheckZipLength(userZip) == true || CheckZipContents(userZip) == true) {
                return true;
            }
            return false;
        }

        private Boolean CheckZipContents(String userZip) {
            char[] userZipToChar = userZip.ToCharArray();
            for (int i = 0; i < userZip.Length; i++) {
                if (Char.IsLetter(userZipToChar[i]) == true) {
                    return false;
                }
            }
            return true;
        }

        private void ParseFormattedJsonData(WeatherJson weatherJson, WeatherData wd) {
            wd.SetLFN(weatherJson.placeName, weatherJson.stateAbbreviation);
        }

        private Boolean CheckZipLength(String userZip) {
            if (userZip.Length == 5) {
                return true;
            }
            return false;
        }

        private String GetZipCodeURL(String passedZip) {
            return "https://api.zippopotam.us/us/" + passedZip;
        }

    }

    class ObtainICAO {
        public ObtainICAO(WeatherData wd) {
            String[] airportList = SplitAirportsByNewLine(ObtainRawAirportData(wd));
            DisplayICAOCodes(ObtainICAOCode(airportList));
            //String rawAirportData = MainClass.downloadWebData(getAiportIcaoURL(wd.getZip()));
            //Console.WriteLine(rawAirportData);
        }

        private String GetAiportIcaoURL(String userZip) {
            return "https://www.allplaces.us/afz.cgi?s=" +userZip + "&rad=30";
        }

        public String ObtainRawAirportData(WeatherData wd) {
            String airportURL = @"https://www.travelmath.com/nearest-airport/" + wd.GetZip();
            HtmlWeb webData = new HtmlWeb();
            var htmlDoc = webData.Load(airportURL);
            var node = htmlDoc.DocumentNode.SelectSingleNode(".//body/div[@id='wrapper']");
            var node1 = node.SelectSingleNode(".//div[@id='EchoTopic']");
            var node2 = node1.SelectSingleNode(".//div[@class='leftcolumn']/p");
            return node2.InnerText;
        }

        public String[] SplitAirportsByNewLine(String rawAirportData) {
            return rawAirportData.Split('\n');
        }

        public String[] ObtainICAOCode(String[] rawAirportList) {
            String[] icaoList = new string[20];
            foreach (var airportLine in rawAirportList) {
                int i = 0;
                icaoList[i] = Regex.Match(airportLine, @"\/(.*)\)").Groups[1].Value.Trim();
                i++;
            }
            Console.WriteLine("Item 1: " + icaoList[0]);
            return icaoList;
        }

        public void DisplayICAOCodes(String[] icaoList) {
            for (int p = 0; p < icaoList.Length; p++) {
                Console.WriteLine(icaoList[p]);
            }
        }
    }


    class WeatherData {
        private String zip;
        private String longFormName;
        private String ICAO;

        public String GetZip() {
            return this.zip;
        }

        public void SetZip(String userZip) {
            this.zip = userZip.Trim();
        }

        public String GetIcao() {
            return this.ICAO;
        }

        public void SetIcao(String userIcao) {
            this.ICAO = userIcao;
        }

        public String GetLFN() {
            return this.longFormName;
        }

        public void SetLFN(String userCity, String userState) {
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

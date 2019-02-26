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
using System.Globalization;

namespace WeatherDisplay {
    class MainClass {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            WeatherData wd = new WeatherData();
            new ObtainZipCode(wd);
            new ObtainICAO(wd);
            new ObtainUserDate(wd);
            new ObtainWeatherData(wd);
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
                    String downloadedJsonData = MainClass.downloadWebData(GetZipCodeURL(wd.GetZip()));
                    String rawJsonData = FormatRawJsonData(downloadedJsonData);
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
            //DisplayICAOCodes();
            parseForK(wd, ObtainICAOCode(airportList));
            //String rawAirportData = MainClass.downloadWebData(getAiportIcaoURL(wd.getZip()));
            //Console.WriteLine(rawAirportData);
        }

        private String GetAiportIcaoURL(WeatherData wd) {
            return @"https://www.travelmath.com/nearest-airport/" + wd.GetZip();
        }

        public String ObtainRawAirportData(WeatherData wd) {
            String airportURL = GetAiportIcaoURL(wd);
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

        public ArrayList ObtainICAOCode(String[] rawAirportList) {
            ArrayList icaoList = new ArrayList();
            foreach (var airportLine in rawAirportList) {
                icaoList.Add(Regex.Match(airportLine, @"\/(.*)\)").Groups[1].Value.Trim());
            }
            return icaoList;
        }

        public void DisplayICAOCodes(ArrayList icaoList) {
            for (int p = 0; p < icaoList.Count; p++) {
                Console.WriteLine(icaoList[p]);
            }
        }

        public void parseForK(WeatherData wd, ArrayList icaoList) {
            int iterationValue = -1;
            do {
                iterationValue++;
            } while (icaoList[iterationValue].ToString().Substring(0).ToUpper() == "K");
           // Console.WriteLine("SetICAO Value: " + "" + icaoList[iterationValue]);
            wd.SetIcao("" + icaoList[iterationValue]);
        }
    }

    class ObtainUserDate {
        public ObtainUserDate(WeatherData wd) {
            String formattedDate = formatDate(GetDate());
            wd.SetDate(formattedDate);
        }

        public String GetDate() {
            Console.Write("Enter the date you would like to view(mm/dd/yyyy): ");
            return Console.ReadLine();
        }

        private String formatDate(String userDate) {
            String dateFormat = "MM/dd/yyyy";
            String newDateFormat = "yyyy/MM/dd";
            DateTime parsedDate;
            DateTime.TryParseExact(userDate, dateFormat, null,
                                   DateTimeStyles.None, out parsedDate);
            return parsedDate.ToString(newDateFormat);
        }

    }

    class ObtainWeatherData {
        public ObtainWeatherData(WeatherData wd) {
            ParseWeatherData(DownloadWeatherData(wd));
        }

        private String getWeatherURL(WeatherData wd) {
            return @"http://api.wunderground.com/history/airport/" + wd.GetIcao() + "/" + wd.GetDate() + "/DailyHistory.html?reqdb.zip=" + wd.GetZip();
        }

        private String DownloadWeatherData(WeatherData wd) {
            String airportURL = getWeatherURL(wd);
            HtmlWeb webData = new HtmlWeb();
            var htmlDoc = webData.Load(airportURL);
            var node = htmlDoc.DocumentNode.SelectSingleNode(".//body/div[@id='content-wrap']/div[@id='inner-wrap']/section[@id='inner-content']/div[@class='mainWrapper']/div[@class='row collapse']/div[@class='column large-12']/div[@id='observations_details']");
            //Console.WriteLine(node.InnerText);
            return "" + node.InnerText;
        }

        private void ParseWeatherData(String rawWeatherData) {
            ArrayList dataWeather = new ArrayList();
            String[] splitWeatherData = (rawWeatherData.Split('\n'));
            for (int i = 0; i < splitWeatherData.Length; i++) {
                if (String.IsNullOrEmpty(splitWeatherData[i].Trim()) == false) {
                    dataWeather.Add(splitWeatherData[i].Trim());
                }
            }

            for (int p = 0; p < dataWeather.Count; p++) {
                Console.WriteLine("Weather data: <" + dataWeather[p] + ">");
            }

        }

    }

    class WeatherData {
        private String zip;
        private String longFormName;
        private String ICAO;
        private String date;
   //     private WeatherGraph wg;

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

        public void SetDate(String userDate) {
            this.date = userDate;
        }

        public String GetDate() {
            return this.date;
        }

   /*     public void initializeWeatherGraph(int row, int column) {
            this.wg = new WeatherGraph(row, column);
        }

        public String getWeatherGraph(int row, int column) {
            return this.wg.
        }*/

    }

 /*   class WeatherGraph {
        String[,] WeatherData;

        public WeatherGraph() {
            WeatherData = new String[50,50];
        }

        public WeatherGraph(int row, int column) {
            this.WeatherData = new ArrayList[row, column];
        }

        public void setWeatherGraphData(int row, int column, String dataToSet) {
            this.WeatherData[row][column].Add(dataToSet);
        }

        public String GetWeatherGraphPlot(int row, int column) {
            return this.WeatherData[row][column];
        }

    }*/

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

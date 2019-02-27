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
            Console.WriteLine("Hello World!"); // This is only in here to make sure the application is actually running.

            new ObtainUserInformation();

            Console.ReadKey();
        }

        public static string DownloadWebData(string userURL) { // Static function that downloads raw HTML data from chosen website.  
            string responseString = String.Empty;
            using (var webClient = new WebClient()) {
                responseString = webClient.DownloadString(userURL);
            }
            //Console.WriteLine(responseString);
            return responseString;
        }

        public static void Quitter(String webexec) { // Static function that drops the user; this function prints the web exception reason.
            Console.WriteLine(webexec + "\n\n");
            Console.WriteLine("\nSorry, that zip code is not valid.\n");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

        public static void Quitter() { // Static function that drops the user, but does not print a reason other than the zip code not being valid.  
            Console.WriteLine("\nSorry, that zip code is not valid.\n");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

    }

    class ObtainUserInformation {
        WeatherData wd = new WeatherData();

        public ObtainUserInformation() { // Standard constructor, without any passed arguments.
            string userGeneratedZip = UserZipInput();
            string parsedDate = DateFormatter(UserDateInput());
            if (ValidateZipInput(userGeneratedZip) != true || ValidDateInput(parsedDate) != true) {
                MainClass.Quitter();
            }
            this.wd.SetDate(parsedDate);
            this.wd.SetZip(userGeneratedZip);
            this.wd.SetLFN(VerifyUserZip());

            String[] airportList = DivideAirportsByNewLine(ObtainRawAirportData());
            this.wd.SetIcao(ParseForK(ObtainICAOCode(airportList)));

        }

        public ObtainUserInformation(String userKeyedZip) {
            this.wd.SetZip(userKeyedZip);
            this.wd.SetZip(this.wd.GetZip());
            this.wd.SetLFN(VerifyUserZip());
            String[] airportList = DivideAirportsByNewLine(ObtainRawAirportData());
            this.wd.SetIcao(ParseForK(ObtainICAOCode(airportList)));
        }

        public ObtainUserInformation(String userKeyedZip, String userKeyedICAO) {
            this.wd.SetZip(userKeyedZip);
            this.wd.SetZip(this.wd.GetZip());
            this.wd.SetLFN(VerifyUserZip());
            this.wd.SetIcao(userKeyedICAO);
        }

        public string UserZipInput() { 
            Console.Write("Enter your zip code: ");
            return Console.ReadLine();
        }

        private Boolean ValidateZipInput(string userZip) { // Mother function of CheckZipLength() and CheckZipContents().  If either of those functions come back false, a boolean false will be generated.
            if (CheckZipLength(userZip) == true || CheckZipForLetters(userZip) == true) {
                return true;
            }
            return false;
        }

        private Boolean CheckZipForLetters(string userZip) { // Function that checks the contents of the user input, if any of the input are letters, this will come back false.
            char[] userZipToChar = userZip.ToCharArray();
            for (int i = 0; i < userZip.Length; i++) {
                if (Char.IsLetter(userZipToChar[i]) == true) {
                    return false;
                }
            }
            return true;
        }

        private Boolean CheckZipLength(string userZip) { // Checks the length of the user input zip; if the input is not 5 characters, it will return false.
            if (userZip.Length == 5) {
                return true;
            }
            return false;
        }

        private Boolean ValidDateInput(string userDate) {
            return true;
        }

        private string VerifyUserZip() {
            try {
                string splitJsonData = FormatRawWebDataFromURL(MainClass.DownloadWebData(wd.GetZip()));
                WeatherJson account = JsonConvert.DeserializeObject<WeatherJson>(splitJsonData);
                return account.placeName + ", " + account.stateAbbreviation;
            } catch (Exception e) {
                MainClass.Quitter("" + e);
            }
            return "Failure";
        }

        private string FormatRawWebDataFromURL(string rawURLData) { // This function takes in the raw URL data downloaded from the static function above, and then removes the brackets.
            return rawURLData.Replace("\"places\": [{", "").Replace("}]", "");
        }

        public string  UserDateInput() {
            Console.Write("Enter the date you would like to view(mm/dd/yyyy): ");
            return Console.ReadLine();
        }

        private string DateFormatter(String userDate) {
            string dateFormat = "MM/dd/yyyy";
            string newDateFormat = "yyyy/MM/dd";
            DateTime parsedDate;
            try {
                DateTime.TryParseExact(userDate, dateFormat, null,
                                    DateTimeStyles.None, out parsedDate);
                return parsedDate.ToString(newDateFormat);
            } catch (WebException we) {
                MainClass.Quitter("" + we);
            }
            return "-1";
        }

        private static string GetZipCodeURL(string passedZip) { // Returns the zip code url.
            return "https://api.zippopotam.us/us/" + passedZip;
        }

        private static string GetZipCodeURL() { // Returns the zip code url, but without the user passed zip code.
            return "https://api.zippopotam.us/us/";
        }

        private String GetAiportIcaoURL() {
            return @"https://www.travelmath.com/nearest-airport/" + this.wd.GetZip();
        }

        public String ObtainRawAirportData() {
            String airportURL = GetAiportIcaoURL();
            HtmlWeb webData = new HtmlWeb();
            var htmlDoc = webData.Load(airportURL);
            var node = htmlDoc.DocumentNode.SelectSingleNode(".//body/div[@id='wrapper']/div[@id='EchoTopic']/div[@class='leftcolumn']/p");
            //var node1 = node.SelectSingleNode(".//div[@id='EchoTopic']");
            //var node2 = node1.SelectSingleNode(".//div[@class='leftcolumn']/p");
            return node.InnerText;
        }

        public String[] DivideAirportsByNewLine(String rawAirportData) {
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

        public string ParseForK(ArrayList icaoList) {
            int iterationValue = -1;
            do {
                iterationValue++;
            } while (icaoList[iterationValue].ToString().Substring(0).ToUpper() == "K");
            // Console.WriteLine("SetICAO Value: " + "" + icaoList[iterationValue]);
            return "" + icaoList[iterationValue];
        }


    }

   /* class ObtainICAO {
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
            var node = htmlDoc.DocumentNode.SelectSingleNode(".//body/div[@id='wrapper']/div[@id='EchoTopic']/div[@class='leftcolumn']/p");
            //var node1 = node.SelectSingleNode(".//div[@id='EchoTopic']");
            //var node2 = node1.SelectSingleNode(".//div[@class='leftcolumn']/p");
            return node.InnerText;
        }

        public String[] DivideAirportsByNewLine(String rawAirportData) {
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
    }*/

    class ObtainWeatherData {
        public ObtainWeatherData(WeatherData wd) {
            ArrayList parsedWeatherData = ParseWeatherData(DownloadWeatherData(wd));
         //   StoreWeatherData(wd, parsedWeatherData);
        }

        private string getWeatherURL(WeatherData wd) {
            return @"http://api.wunderground.com/history/airport/" + wd.GetIcao() + "/" + wd.GetDate() + "/DailyHistory.html?reqdb.zip=" + wd.GetZip();
        }

        private string DownloadWeatherData(WeatherData wd) {
            string airportURL = getWeatherURL(wd);
            HtmlWeb webData = new HtmlWeb();
            var htmlDoc = webData.Load(airportURL);
            var node = htmlDoc.DocumentNode.SelectSingleNode(".//body/div[@id='content-wrap']/div[@id='inner-wrap']/section[@id='inner-content']/div[@class='mainWrapper']/div[@class='row collapse']/div[@class='column large-12']/div[@id='observations_details']");
            //Console.WriteLine(node.InnerText);
            return "" + node.InnerText;
        }

        private ArrayList ParseWeatherData(string rawWeatherData) {
            ArrayList dataWeather = new ArrayList();
            string[] splitWeatherData = (rawWeatherData.Split('\n'));
            for (int i = 0; i < splitWeatherData.Length; i++) {
                if (string.IsNullOrEmpty(splitWeatherData[i].Trim()) == false) {
                    dataWeather.Add(splitWeatherData[i].Trim());
                }
            }
            return dataWeather;
            /*for (int p = 0; p < dataWeather.Count; p++) {
                Console.WriteLine("Weather data: <" + dataWeather[p] + ">");
            }*/

        }

    /*    private void StoreWeatherData(WeatherData wd, ArrayList parsedWeatherData) {
            for (int columnValue = 0; columnValue < wd.Get) {

            }
        }*/

    }

    class WeatherData {
        private string zip;
        private string longFormName;
        private string ICAO;
        private string date;
        private WeatherGraph wg;

        public string GetZip() {
            return this.zip;
        }

        public void SetZip(string userZip) {
            this.zip = userZip.Trim();
        }

        public string GetIcao() {
            return this.ICAO;
        }

        public void SetIcao(string userIcao) {
            this.ICAO = userIcao;
        }

        public string GetLFN() {
            return this.longFormName;
        }

        public void SetLFN(string userCity, string userState) {
            this.longFormName = userCity + ", " + userState;
        }

        public void SetLFN(string userJoinedName) {
            this.longFormName = userJoinedName;
        }

        public void SetDate(string userDate) {
            this.date = userDate;
        }

        public string GetDate() {
            return this.date;
        }

        public void InitializeWeatherGraph() {
            this.wg.InitializeWeatherGraph();
        }

        public void InitializeWeatherGraph(int row, int column) {
            this.wg.InitializeWeatherGraph(row, column);
        }

        public string GetWeatherGraph(int row, int column) {
            return this.wg.GetWeatherGraphPlot(row, column);
        }

        public string GetWeatherRow(int row) {
            return this.wg.GetWeatherGraphRow(row);
        }

    }

    class WeatherGraph {
        String[,] WeatherData;

        public WeatherGraph() {

        }

        public WeatherGraph(int row, int column) {

        }

        public void InitializeWeatherGraph() {
            this.WeatherData = new string[50, 50];
        }

        public void InitializeWeatherGraph(int row, int column) {
            this.WeatherData = new string[row, column];
        }

        public int GetWeatherDataLength() {
            return WeatherData.GetLength(0);
        }

        public int GetWeatherDataWidth() {
            return WeatherData.GetLength(1);
        }

        public void setWeatherGraphData(int row, int column, string dataToSet) {
            this.WeatherData[row, column] = dataToSet;
        }

        public String GetWeatherGraphPlot(int row, int column) {
            return this.WeatherData[row, column];
        }

        public string GetWeatherGraphRow(int row) {
            StringBuilder weatherRow = new StringBuilder();
            for (int columnValue = 0; columnValue < this.WeatherData.Length - 5; columnValue++) {
                weatherRow.Append(GetWeatherGraphPlot(row, columnValue) + "|");
            }
            Console.WriteLine("Weather row: " + weatherRow);
            return "" + weatherRow;
        }
    }

    class WeatherJson {
        [JsonProperty(PropertyName = "post code")]
        public string postCode { get; set; }
        public string country { get; set; }
        [JsonProperty(PropertyName = "country abbreviation")]
        public string countryAbbreviation { get; set; }
        [JsonProperty(PropertyName = "place name")]
        public string placeName { get; set; }
        public string longitude { get; set; }
        public string state { get; set; }
        [JsonProperty(PropertyName = "state abbreviation")]
        public string stateAbbreviation { get; set; }
        public string latitude { get; set; }
    }
}

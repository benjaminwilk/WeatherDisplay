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
using System.Windows.Forms;

namespace WeatherDisplay {
    class MainClass {
        static void Main(string[] args) {
            if (1 == 2) {
                Application.Run(new MainPane());
            } else {
                Console.WriteLine("Hello World!"); // This is only in here to make sure the application is actually running
                new ObtainUserInformation();
                Console.ReadKey();
            }  
        }

        public static string DownloadWebData(string userURL) { // Static function that downloads raw HTML data from chosen website.  
            string responseString = String.Empty;
            // Console.WriteLine("URL being downloaded from: " + userURL);  // For debugging purposes; shows what is being downloaded.
            try {
                using (var webClient = new WebClient()) {
                    responseString = webClient.DownloadString(userURL);
                    return responseString;
                }
            } catch (WebException we) {
                //Console.WriteLine(responseString);
                Quitter("" + we, "Unable to download response string.");
                return "-1";
            }
        }

        public static void Quitter(String stackTrace, String displayMessage) { // Static function that drops the user; this function prints the web exception reason.
            Console.WriteLine(stackTrace + "\n\n");
            Console.WriteLine("\nSorry, " + displayMessage + "\n");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

        public static void Quitter(String displayMessage) { // Static function that drops the user, but does not print a reason other than the zip code not being valid.  
            Console.WriteLine("\nSorry, " + displayMessage + ".\n");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

        public static void Quitter() { // Static function that drops the user, but does not print a reason other than the zip code not being valid.  
            Console.WriteLine("\nSorry, something has gone wrong.\n");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

    }

    class ObtainUserInformation {
        WeatherData wd = new WeatherData();
        ObtainWeatherData owd;

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

            this.owd = new ObtainWeatherData(this.wd);

        }

        public ObtainUserInformation(String userKeyedZip) {
            this.wd.SetZip(userKeyedZip);
            this.wd.SetLFN(VerifyUserZip());
            String[] airportList = DivideAirportsByNewLine(ObtainRawAirportData());
            this.wd.SetIcao(ParseForK(ObtainICAOCode(airportList)));
        }

        public ObtainUserInformation(String userKeyedZip, String userKeyedICAO) {
            this.wd.SetZip(userKeyedZip);
            this.wd.SetLFN(VerifyUserZip());
            this.wd.SetIcao(userKeyedICAO);
            this.owd = new ObtainWeatherData(this.wd);
        }

    /*    public ObtainUserInformation(String[] userArgData) {
            if (userArgData.Length == 2) {
                this.wd.SetZip(userArgData[0]);
                this.wd.SetLFN(VerifyUserZip());
                this.wd.SetIcao(userArgData[1]);

            }
            if(userArgData.Length == 1) {
                this.wd.SetZip(userArgData[0]);
                this.wd.SetLFN(VerifyUserZip());
                String[] airportList = DivideAirportsByNewLine(ObtainRawAirportData());
                this.wd.SetIcao(ParseForK(ObtainICAOCode(airportList)));
            }
            this.owd = new ObtainWeatherData(this.wd);
        }*/

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
                string splitJsonData = FormatRawWebDataFromURL(MainClass.DownloadWebData(GetZipCodeURL(wd.GetZip())));
                OnlineZipStructure account = JsonConvert.DeserializeObject<OnlineZipStructure>(splitJsonData);
                return account.placeName + ", " + account.stateAbbreviation;
            } catch (Exception e) {
                MainClass.Quitter("" + e, "Unable to deserialize zip code.");
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
                MainClass.Quitter("" + we, "Unable to format provided date.");
            }
            return "-1";
        }

        private static string GetZipCodeURL(string passedZip) { // Returns the zip code url.
            return "https://api.zippopotam.us/us/" + passedZip;
        }

        private static string GetZipCodeURL() { // Returns the zip code url, but without the user passed zip code.
            return "https://api.zippopotam.us/us/";
        }

        private string GetAiportIcaoURL() {
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

        private class OnlineZipStructure {
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
            string rawWeatherData = DownloadWeatherData(getWeatherURL(wd));
            ArrayList parsedWeatherData = ParseWeatherData(rawWeatherData);
            DefineRowsAndColumns(wd, parsedWeatherData);
          //  InsertHeaderData(parsedWeatherData);
        //    wd.InitializeWeatherGraph();
            StoreWeatherData(wd, parsedWeatherData);
            GetWeatherData(wd, 2, 7);
        }

        private string getWeatherURL(WeatherData wd) {
            return @"http://api.wunderground.com/history/airport/" + wd.GetIcao() + "/" + wd.GetDate() + "/DailyHistory.html?reqdb.zip=" + wd.GetZip();
        }

        private string DownloadWeatherData(string downloadWeatherURL) {
            string airportURL = downloadWeatherURL;
            HtmlWeb webData = new HtmlWeb();
            var htmlDoc = webData.Load(airportURL);
            var node = htmlDoc.DocumentNode.SelectSingleNode(".//body/div[@id='content-wrap']/div[@id='inner-wrap']/section[@id='inner-content']/div[@class='mainWrapper']/div[@class='row collapse']/div[@class='column large-12']/div[@id='observations_details']");
            //Console.WriteLine(node.InnerText);
            return "" + node.InnerText;
        }

        private void DefineRowsAndColumns(WeatherData wd, ArrayList parsedWeatherData) {
            int widthCount = 0;
            int columnCount = 0;
            do {
                widthCount++;
            } while (Char.IsDigit(Convert.ToChar(parsedWeatherData[widthCount].ToString()[0])) != true);

            for (int p = 0; p < parsedWeatherData.Count; p++) {
                Match m = Regex.Match("" + parsedWeatherData[p], "((1[0-2]|0?[1-9]):([0-5][0-9]) ([AP][M]))", RegexOptions.IgnoreCase);
                if (m.Success) {
                    columnCount++;
                }
        //        Console.WriteLine("Weather data: <" + parsedWeatherData[p] + ">");
            }
            wd.InitializeWeatherGraph(columnCount, widthCount);
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

        }

      /*  private void InsertHeaderData(ArrayList parsedWeatherData) {
            
        }*/

        private void StoreWeatherData(WeatherData wd, ArrayList parsedWeatherData) {
            int iterationValue = 0;
            Console.WriteLine("Data Length: " + wd.GetWeatherDataLength() + "\tData Width: " + wd.GetWeatherDataWidth());
            for (int columnValue = 0; columnValue < wd.GetWeatherDataLength() - 1; columnValue++) {
                for (int rowValue = 0; rowValue < wd.GetWeatherDataWidth(); rowValue++) {
                    wd.SetWeatherGraphPoint(columnValue, rowValue, parsedWeatherData[iterationValue].ToString());
                    iterationValue++;
                }
                Console.WriteLine();

            }
        }

        public void GetWeatherData(WeatherData wd, int userRow, int userColumn) {
            Console.WriteLine(wd.GetWeatherPoint(userRow, userColumn));
        }

    }

    class WeatherData {
        private string zip;
        private string longFormName;
        private string ICAO;
        private string date;
        private WeatherGrapher wg;

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
            this.wg = new WeatherGrapher();
        }

        public void InitializeWeatherGraph(int row, int column) {
            this.wg = new WeatherGrapher(row, column);
        }

        public string GetWeatherPoint(int row, int column) {
            return this.wg.GetWeatherGraphPoint(row, column);
        }

        public string GetWeatherRow(int row) {
            return this.wg.GetWeatherGraphRow(row);
        }

        public int GetWeatherDataLength() {
            return this.wg.GetWeatherDataLength();
        }
        
        public int GetWeatherDataWidth() {
            return this.wg.GetWeatherDataWidth();
        }

        public void SetWeatherGraphPoint(int userRow, int userColumn, string dataToSet) {
            this.wg.SetWeatherGraphPoint(userRow, userColumn, dataToSet);
        }

        public void GetWeatherWidthAndLength() {
            this.wg.GetWeatherWidthAndLength();
        }

    }

    class WeatherGrapher {
        String[,] WeatherGraph;

        public WeatherGrapher() {
            this.WeatherGraph = new string[50, 50];
        }

        public WeatherGrapher(int row, int column) {
            this.WeatherGraph = new string[row, column];
        }

        public int GetWeatherDataLength() {
            return this.WeatherGraph.GetLength(0);
        }

        public int GetWeatherDataWidth() {
            return this.WeatherGraph.GetLength(1);
        }

        public void GetWeatherWidthAndLength() {
            Console.WriteLine("Length: " + GetWeatherDataLength() + "\tWidth: " + GetWeatherDataWidth());
        }

        public void SetWeatherGraphPoint(int row, int column, string dataToSet) {
            this.WeatherGraph[row, column] = dataToSet;
        }

        public String GetWeatherGraphPoint(int row, int column) {
            return this.WeatherGraph[row, column];
        }

        public string GetWeatherGraphRow(int row) {
            StringBuilder weatherRow = new StringBuilder();
            for (int columnValue = 0; columnValue < this.WeatherGraph.Length - 5; columnValue++) {
                weatherRow.Append(GetWeatherGraphPoint(row, columnValue) + "|");
            }
            Console.WriteLine("Weather row: " + weatherRow);
            return "" + weatherRow;
        }
    }
}

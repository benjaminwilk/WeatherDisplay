using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeatherDisplay {
    public partial class MainPane : Form {

        string userZipBox;
        string userDateBox;

        public MainPane() {
            InitializeComponent();
            this.Load += this.MainPane_Load;
            //    MessageBox.Show("Yeet");
        }

        private void MainPane_Load(object sender, EventArgs e) {


        }

        private void SubmitButton_Click(object sender, EventArgs e) {
            WeatherData displayWD = new WeatherData();
            displayWD.SetZip(this.userZipBox = zipBox.Text);
            ObtainUserInformation oui = new ObtainUserInformation((displayWD.GetZip()), (displayWD.GetDate()), sender);
            displayWD.SetDate(oui.DateFormatter(this.userDateBox = dateBox.Text));
            if (oui.ValidateZipInput(displayWD.GetZip()) == false) {
                displayBox.AppendText("Sorry, that is an invalid zip code.");
            } else {
                string[] rawAirportData = oui.DivideAirportsByNewLine(oui.ObtainRawAirportData());
                displayWD.SetIcao(oui.ParseForK(oui.ObtainICAOCode(rawAirportData)));
                ObtainWeatherData owd = new ObtainWeatherData(displayWD, sender);
                // displayBox.AppendText("Width: " + displayWD.GetWeatherDataWidth() + "\tLength: " + displayWD.GetWeatherDataLength());
                for (int i = 0; i < displayWD.GetWeatherDataLength(); i++) {
                    displayBox.AppendText(owd.GetWeatherRow(displayWD, i));
                }
            }
            //displayBox.AppendText(owd.GetAllWeatherData(dd.SendWeatherData(), sender));
        }
        private void ResetButton_Click(object sender, EventArgs e) {
            dateBox.Text = "Enter Date (MM/DD/YYYY)";
            zipBox.Text = "Enter Zip Code";

            displayBox.Clear();
        }

        private void DateBox_textChanged(object sender, EventArgs e) {
            this.userDateBox = dateBox.Text;
        }

        private void ZipBox_textChanged(object sender, EventArgs e) {
            this.userZipBox = zipBox.Text;
        }

        private void DateBox_Click(object sender, EventArgs e) {
            dateBox.Text = "";
        }

        private void ZipBox_Click(object sender, EventArgs e) {
            zipBox.Text = "";
        }

        private void DateBox_Leave(object sender, EventArgs e) {
            if(dateBox.Text == "") {
                dateBox.Text = "Enter Date (MM/DD/YYYY)";
            }
        }

        private void ZipBox_Leave(object sender, EventArgs e) {
            if (zipBox.Text == "") {
                zipBox.Text = "Enter Zip Code";
            }
        }

        private void zipBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Tab)
                dateBox.Focus();
        }

        private void dateBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Tab)
                submitButton.Focus();
        }

        private void submitButton_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Tab)
                resetButton.Focus();
        }

        private void resetButton_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Tab)
                zipBox.Focus();
        }

    }

}


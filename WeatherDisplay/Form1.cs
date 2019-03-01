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
            DisplayData dd = new DisplayData(zipBox.Text, dateBox.Text);
            displayBox.AppendText(dd.getZip());
            ObtainWeatherData owd = new ObtainWeatherData(dd.SendWeatherData());
            displayBox.AppendText(owd.GetAllWeatherData(dd.SendWeatherData(), sender));
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
    }
}

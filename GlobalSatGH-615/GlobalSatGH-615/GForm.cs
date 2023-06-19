using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalSatGH_615 // // Globalsat GH600
{
    public partial class GForm : Form
    {
        public string LastFile = Path.Combine(GetCurrentDir(), "GlobalSatGH-615.lst");

        public GForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1_Click(sender, e);
            LoadLast();
        }

        private void LoadLast()
        {
            try { cport.Text = File.ReadAllText(LastFile); } catch { };
        }

        private void SaveLast()
        {
            try { File.WriteAllText(LastFile, cport.Text); } catch { };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cport.Items.Clear();
            foreach (string pn in SerialPort.GetPortNames())
                cport.Items.Add(pn);
            if (cport.Items.Count > 0)
                cport.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cport.Text)) return;
            SaveLast();
            log.AppendText("Get Short Info:\r\n");
            GH600 gh = new GH600(cport.Text);
            log.AppendText("  " + gh.GetShortInfo().Replace("\r\n", "\r\n  ") + "\r\n\r\n");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cport.Text)) return;
            SaveLast();
            log.AppendText("Get Full Info:\r\n");
            GH600 gh = new GH600(cport.Text);
            log.AppendText("  " + gh.GetFullInfo().Replace("\r\n", "\r\n  ") + "\r\n\r\n");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cport.Text)) return;
            SaveLast();
            log.AppendText("Get Waypoints:\r\n");
            GH600 gh = new GH600(cport.Text);
            log.AppendText("  " + gh.GetWaypoints(out _).Replace("\r\n", "\r\n  ") + "\r\n\r\n");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cport.Text)) return;
            SaveLast();
            log.AppendText("Save Waypoints:\r\n");
            GH600 gh = new GH600(cport.Text);
            gh.GetWaypoints(out List<Waypoint> wps);
            if (wps != null && wps.Count > 0 )
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".kml";
                sfd.FileName = "GlobalSatGH-615_Waypoints.kml";
                sfd.Filter = "Google Eartch KML (*.kml)|*.kml|GPX Exchange Format (*.gpx)|*.gpx|JSON Data Array (*.json)|*.json";
                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    bool ok = false;
                    string fExt = Path.GetExtension(sfd.FileName).ToLower();
                    if (fExt == ".kml") ok = Waypoint.SaveToKML(wps, sfd.FileName);
                    if (fExt == ".gpx") ok = Waypoint.SaveToGPX(wps, sfd.FileName);
                    if (fExt == ".json") ok = Waypoint.SaveToJSON(wps, sfd.FileName);
                    if(ok) log.AppendText($"  Saved {wps.Count} points to {sfd.FileName}\r\n\r\n");
                    else log.AppendText($" Unknown Error\r\n\r\n");
                };
                sfd.Dispose();
            };
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cport.Text)) return;
            SaveLast();

            if (MessageBox.Show($"Are you sure to erase waypoints in {cport.Text}?", "GlobalsatGH-615", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            log.AppendText("Erase Waypoints:\r\n");
            GH600 gh = new GH600(cport.Text);
            log.AppendText("  " + gh.EraseWaypoints().Replace("\r\n", "\r\n  ") + "\r\n\r\n");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cport.Text)) return;
            SaveLast();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All Supported Files (*.kml;*.gpx;*.json)|*.kml;*.gpx;*.json|Google Eartch KML (*.kml)|*.kml|GPX Exchange Format (*.gpx)|*.gpx|JSON Data Array (*.json)|*.json";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                bool ok = false;
                string fExt = Path.GetExtension(ofd.FileName).ToLower();
                List<Waypoint> wps = null;
                if (fExt == ".kml") ok = Waypoint.LoadFromKML(ofd.FileName, out wps);
                if (fExt == ".gpx") ok = Waypoint.LoadFromGPX(ofd.FileName, out wps);
                if (fExt == ".json") ok = Waypoint.LoadFromJSON(ofd.FileName, out wps);
                if (fExt == ".txt") ok = Waypoint.LoadFromJSON(ofd.FileName, out wps);
                if (ok && wps != null && wps.Count > 0)
                {
                    if (MessageBox.Show($"Write {wps.Count} Waypoints to {cport.Text}?", "GlobalsatGH-615", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
                    log.AppendText($"Write {wps.Count} Waypoints:\r\n");
                    GH600 gh = new GH600(cport.Text);
                    log.AppendText($"  " + (new GH600(cport.Text)).WriteWaypoints(wps).Replace("\r\n", "\r\n  ") + "\r\n\r\n");                    
                }
                else log.AppendText($"  Unknown Error\r\n\r\n");
            };            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            log.Clear();
        }
        public static string GetCurrentDir()
        {
            string fname = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            fname = fname.Replace("file:///", "");
            fname = fname.Replace("/", @"\");
            fname = fname.Substring(0, fname.LastIndexOf(@"\") + 1);
            return fname;
        }

    }
}

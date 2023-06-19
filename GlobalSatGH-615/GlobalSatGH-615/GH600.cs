using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace GlobalSatGH_615
{
    public enum WPTYPES
    {
        DOT = 0,
        HOUSE = 1,
        TRIANGLE = 2,
        TUNNEL = 3,
        CROSS = 4,
        FISH = 5,
        LIGHT = 6,
        CAR = 7,
        COMM = 8,
        REDCROSS = 9,
        TREE = 10,
        BUS = 11,
        COPCAR = 12,
        TREES = 13,
        RESTAURANT = 14,
        SEVEN = 15,
        PARKING = 16,
        REPAIRS = 17,
        MAIL = 18,
        DOLLAR = 19,
        GOVOFFICE = 20,
        CHURCH = 21,
        GROCERY = 22,
        HEART = 23,
        BOOK = 24,
        GAS = 25,
        GRILL = 26,
        LOOKOUT = 27,
        FLAG = 28,
        PLANE = 29,
        BIRD = 30,
        DAWN = 31,
        RESTROOM = 32,
        WTF = 33,
        MANTARAY = 34,
        INFORMATION = 35,
        BLAN = 36
    }

    public class Waypoint
    {
        public static CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;

        public string title;
        public double latitude;
        public double longitude;
        public ushort altitude;
        public byte type;
        public WPTYPES symbol { get { return (WPTYPES)this.type; } }

        public override string ToString()
        {
            return $"{{'title':'{title}', 'latitude':{latitude}, 'longitude':{longitude}, 'altitude':{altitude}, 'type':{type}, 'symbol':'{(WPTYPES)symbol}'}}";
        }

        public string ToString(bool replace_quotas)
        {
            string res = $"{{'title':'{title}', 'latitude':{latitude}, 'longitude':{longitude}, 'altitude':{altitude}, 'type':{type}, 'symbol':'{(WPTYPES)symbol}'}}";
            return replace_quotas ? res.Replace("'", "\"") : res;
        }

        public static bool SaveToGPX(List<Waypoint> waypoints, string filename)
        {
            if (waypoints == null || waypoints.Count == 0) return false;
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" creator=\"GlobalSatGH_615\" version=\"1.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\">");
                for (int x = 0; x < waypoints.Count; x++)
                {
                    sw.WriteLine("\t<wpt lat=\"" + waypoints[x].latitude.ToString(ci) + "\" lon=\"" + waypoints[x].longitude.ToString(ci) + "\">");
                    sw.WriteLine("\t\t<name>" + waypoints[x].title + "</name>");
                    sw.WriteLine("\t\t<desc><![CDATA[" + waypoints[x].symbol.ToString() + "]]></desc>");
                    sw.WriteLine("\t</wpt>");
                };
                sw.WriteLine("</gpx>");
                sw.Close();
                fs.Close();
                return true;
            }
            catch { };
            return false;
        }

        public static bool SaveToKML(List<Waypoint> waypoints, string filename)
        {
            if (waypoints == null || waypoints.Count == 0) return false;
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
                sw.WriteLine("<kml xmlns='http://www.opengis.net/kml/2.2'><Document>");
                sw.WriteLine("<name>GlobalSatGH-615 Waypoints</name>");
                sw.WriteLine("<createdby>GlobalSatGH_615</createdby>");
                sw.WriteLine("<Folder><name><![CDATA[GlobalSatGH-615 Waypoints (Точек: " + waypoints.Count.ToString() + ")]]></name>");
                for (int x = 0; x < waypoints.Count; x++)
                {
                    sw.WriteLine("<Placemark><name>" + waypoints[x].title + "</name>" +
                        "<description><![CDATA[" + waypoints[x].symbol.ToString() + "]]></description>" +
                        "<Point><coordinates>" + waypoints[x].longitude.ToString(ci) + "," + waypoints[x].latitude.ToString(ci) + "," + waypoints[x].altitude.ToString(ci) + "</coordinates></Point>\r\n</Placemark>");
                };
                sw.WriteLine("</Folder></Document></kml>");
                sw.Close();
                fs.Close();
                return true;
            }
            catch { };
            return false;
        }

        public static bool SaveToJSON(List<Waypoint> waypoints, string filename)
        {
            if (waypoints == null || waypoints.Count == 0) return false;
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("[");
                for (int x = 0; x < waypoints.Count; x++)
                {
                    sw.Write("  " + waypoints[x].ToString(true));
                    if (x < waypoints.Count - 1) sw.Write(",");
                    sw.WriteLine();
                };
                sw.WriteLine("]");
                sw.Close();
                fs.Close();
                return true;
            }
            catch { };
            return false;
        }

        public static bool LoadFromGPX(string filename, out List<Waypoint> waypoints)
        {
            waypoints = null;
            try
            {
                string txt = File.ReadAllText(filename);
                txt = RemoveXMLNamespaces(txt);
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(txt);
                XmlNodeList nodes = xdoc.SelectNodes("/gpx/wpt");
                waypoints = new List<Waypoint>();
                foreach (XmlNode node in nodes) 
                {
                    string lat = node.Attributes["lat"].Value;
                    string lon = node.Attributes["lon"].Value;
                    string nam = null;
                    string des = null;
                    foreach (XmlNode n in node.SelectNodes("name")) nam = n.InnerText;
                    foreach (XmlNode n in node.SelectNodes("desc")) des = n.InnerText;
                    Waypoint wp = new Waypoint() { title = nam };
                    double.TryParse(lat, NumberStyles.Float, ci, out wp.latitude);
                    double.TryParse(lon, NumberStyles.Float, ci, out wp.longitude);
                    if (byte.TryParse(des, NumberStyles.Integer, ci, out byte tp)) wp.type = tp;                    
                    if (Enum.TryParse<WPTYPES>(des, out WPTYPES ttp)) wp.type = (byte)ttp;
                    waypoints.Add(wp);
                };
                return true;
            }
            catch { return false; };
        }

        public static bool LoadFromKML(string filename, out List<Waypoint> waypoints)
        {
            waypoints = null;
            try
            {
                string txt = File.ReadAllText(filename);
                txt = RemoveXMLNamespaces(txt);
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(txt);
                waypoints = new List<Waypoint>();
                List<XmlNode> nodes = new List<XmlNode>();
                foreach (XmlNode node in xdoc.SelectNodes("/kml/Document/Placemark")) nodes.Add(node);
                foreach (XmlNode node in xdoc.SelectNodes("/kml/Document/Folder/Placemark")) nodes.Add(node);                
                foreach (XmlNode node in nodes)
                {

                    string nam = "";
                    foreach (XmlNode n in node.SelectNodes("name")) nam = n.InnerText;
                    Waypoint wp = new Waypoint() { title = nam };                    
                    foreach (XmlNode n in node.SelectNodes("Point/coordinates"))
                    {
                        string[] xyz = n.InnerText.Split(',');
                        double.TryParse(xyz[1], NumberStyles.Float, ci, out wp.latitude);
                        double.TryParse(xyz[0], NumberStyles.Float, ci, out wp.longitude);
                        ushort.TryParse(xyz[2], NumberStyles.Integer, ci, out wp.altitude);
                    };
                    string des = null;                    
                    foreach (XmlNode n in node.SelectNodes("description")) des = n.InnerText;                    
                    if (byte.TryParse(des, NumberStyles.Integer, ci, out byte tp)) wp.type = tp;
                    if (Enum.TryParse<WPTYPES>(des, out WPTYPES ttp)) wp.type = (byte)ttp;
                    waypoints.Add(wp);
                };
                return true;
            }
            catch { return false; };
        }

        public static bool LoadFromJSON(string filename, out List<Waypoint> waypoints)
        {
            waypoints = null;
            try
            {
                string txt = File.ReadAllText(filename);
                waypoints = JsonConvert.DeserializeObject<List<Waypoint>>(txt);
                return true;
            }
            catch { return false; };
        }

        public static string RemoveXMLNamespaces(string xml)
        {
            string outerXml = xml;
            { // "
                string xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"";
                MatchCollection matchCol = Regex.Matches(outerXml, xmlnsPattern);
                foreach (Match match in matchCol)
                    outerXml = outerXml.Replace(match.ToString(), "");
            };
            {// '
                string xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\'(?<url>[^\\\']*)\\\'";
                MatchCollection matchCol = Regex.Matches(outerXml, xmlnsPattern);
                foreach (Match match in matchCol)
                    outerXml = outerXml.Replace(match.ToString(), "");
            };
            {
                string xmlnsPattern = "<kml[^>]*?>";
                MatchCollection matchCol = Regex.Matches(outerXml, xmlnsPattern);
                foreach (Match match in matchCol)
                    outerXml = outerXml.Replace(match.ToString(), "<kml>");
            };
            {
                string xmlnsPattern = "<gpx[^>]*?>";
                MatchCollection matchCol = Regex.Matches(outerXml, xmlnsPattern);
                foreach (Match match in matchCol)
                    outerXml = outerXml.Replace(match.ToString(), "<gpx>");
            };
            return outerXml;
        }
    }

    public class GH600
    {
        private byte[] devShortInfo = new byte[] { 0x02, 0x00, 0x01, 0xBF, 0xBE };
        private byte[] devFullInfo = new byte[] { 0x02, 0x00, 0x01, 0x85, 0x84 };
        private byte[] devWaypoints = new byte[] { 0x02, 0x00, 0x01, 0x77, 0x76 };
        private byte[] delWaypoints = new byte[] { 0x02, 0x00, 0x03, 0x75, 0x00, 0x64, 0x12 };
        private const int READ_SLEEP = 2000;
        private const int WAYP_SIZE = 18;
        private int BUFFER_SIZE = ushort.MaxValue * 4;

        private SerialPort sp = null;

        public GH600(string port, int baudRate = 57600)
        {
            sp = new SerialPort(port.Trim(), baudRate);
            sp.ReadTimeout = 500;
            sp.WriteTimeout = 500;
        }

        public string GetShortInfo()
        {
            string res = "";            
            try
            {
                byte[] buff = WriteAndRead(devShortInfo);
                string msg = Encoding.ASCII.GetString(buff, 0, buff.Length);
                res = Encoding.ASCII.GetString(buff, 3, 7);
            }
            catch (Exception ex) { return ex.ToString(); };
            try { sp.Close(); } catch { };
            return res;
        }

        public string GetFullInfo()
        {
            string res = "";
            try
            {
                byte[] buff = WriteAndRead(devFullInfo);
                string msg = Encoding.ASCII.GetString(buff, 0, buff.Length);
                string device_name = System.Text.Encoding.ASCII.GetString(buff, 3, 9).Trim('\0');
                res += $"Device Name: {device_name}\r\n";
                string version = System.Text.Encoding.ASCII.GetString(buff, 25, 1).Trim('\0');
                res += $"Software Version: {version}\r\n";
                string firmware = System.Text.Encoding.ASCII.GetString(buff, 28, 16).Trim('\0');
                res += $"Firmware: {firmware}\r\n";
                string name = System.Text.Encoding.ASCII.GetString(buff, 45, 10).Trim('\0');
                res += $"User Name: {name}\r\n";
                int age = buff[57];
                res += $"User Age: {age}\r\n";
                int weight_pounds = GetUint16(buff, 58);
                int weight_kilos = GetUint16(buff, 60);
                res += $"User Wight: {weight_kilos}\r\n";
                int height_centimeters = GetUint16(buff, 64);
                res += $"User Height: {height_centimeters}\r\n";
                int waypoint_count = buff[66];
                res += $"Waypoints: {waypoint_count}\r\n";
                int trackpoint_count = buff[67];
                res += $"Trackpoints: {trackpoint_count}\r\n";
                int birth_year = GetUint16(buff, 69);
                int birth_month = buff[71];
                int birth_day = buff[72];
                DateTime birth = new DateTime(birth_year, birth_month + 1, birth_day);
                res += $"User Birth: {birth}\r\n";
            }
            catch (Exception ex) { return ex.ToString(); };
            try { sp.Close(); } catch { };
            return res;
        }

        public string GetWaypoints(out List<Waypoint> waypoints)
        {
            string res = "";
            waypoints = new List<Waypoint>();
            try
            {
                byte[] b = WriteAndRead(devWaypoints);
                byte[] buff = new byte[b.Length - 4];
                Array.Copy(b,3,buff,0,buff.Length);
                string msg = Encoding.ASCII.GetString(buff, 0, buff.Length);
                byte[] wp = new byte[WAYP_SIZE];
                res = $"Device Waypoints: {(int)(buff.Length/WAYP_SIZE) }\r\n";
                for (int i = 0; i < buff.Length; i += WAYP_SIZE)
                {
                    Array.Copy(buff, i, wp, 0, WAYP_SIZE);
                    string wpt = System.Text.Encoding.ASCII.GetString(wp, 0, WAYP_SIZE);
                    string title = System.Text.Encoding.ASCII.GetString(wp, 0, 6).Replace('\0', ' ').Trim(); // 3..8
                    byte typ = wp[7];
                    ushort alt = GetUint16(wp, 8);
                    double lat = GetInt32(wp, 10) / 1000000.0;
                    double lon = GetInt32(wp, 14) / 1000000.0;
                    Waypoint waypnt = new Waypoint() { title = title, altitude = alt, latitude = lat, longitude = lon, type = typ };
                    waypoints.Add(waypnt);
                    res += $"{waypnt}\r\n";
                };
            }
            catch (Exception ex) { return ex.ToString(); };
            try { sp.Close(); } catch { };
            return res;
        }

        public string EraseWaypoints()
        {
            string res = "";
            try
            {
                byte[] buff = WriteAndRead(delWaypoints);
                string msg = Encoding.ASCII.GetString(buff, 0, buff.Length);
                if (buff.Length == 4 && buff[0] == 0x75 && buff[1] == 0 && buff[2] == 0 && buff[3] == 0)
                    res = "Erased Ok";
                else
                    res = "Erased Failed";
            }
            catch (Exception ex) { return ex.ToString(); };
            try { sp.Close(); } catch { };
            return res;
        }

        public string WriteWaypoints(List<Waypoint> waypoints)
        {
            if (waypoints == null || waypoints.Count == 0) return "No Waypoints";

            Func<byte[], byte> checksum = (inc) => { byte ret = 0; for (int i = 0; i < inc.Length; i++) ret = (byte)(ret ^ inc[i]); return ret; };
            
            List<byte> wpshex = new List<byte>();
            for (int i = 0;i<waypoints.Count;i++)
            {
                string title = waypoints[i].title;
                if (string.IsNullOrEmpty(title)) title = i.ToString();
                if (title.Length > 6) title = title.Substring(0, 6);
                while (title.Length < 6) title += " ";

                wpshex.AddRange(Encoding.ASCII.GetBytes(title));
                wpshex.Add((byte)0);
                wpshex.Add((byte)waypoints[i].type);
                wpshex.AddRange(GetUint16((ushort)waypoints[i].altitude));
                wpshex.AddRange(GetInt32((int)(waypoints[i].latitude * 1000000.0)));
                wpshex.AddRange(GetInt32((int)(waypoints[i].longitude * 1000000.0)));                
            };

            byte[] waypointsConverted = wpshex.ToArray();
            byte[] numberOfWaypoints = GetUint16((ushort)waypoints.Count);
            byte[] payload = GetUint16((ushort)(3 + 18 * waypoints.Count));

            List<byte> toWrite = new List<byte>();
            List<byte> toChSum = new List<byte>();

            toWrite.Add(0x02);
            toWrite.AddRange(payload); toChSum.AddRange(payload);
            toWrite.Add(0x76); toChSum.Add(0x76);
            toWrite.AddRange(numberOfWaypoints); toChSum.AddRange(numberOfWaypoints);            
            toWrite.AddRange(waypointsConverted); toChSum.AddRange(waypointsConverted);
            byte chsum = checksum(toChSum.ToArray());
            toWrite.Add(chsum);
            
            // 02 payl 76 numb title        0  tp alt  lat      lon
            // 02 0015 76 0001 424B2D412020 00 0E 0000 03500BBA 023DAFAC D7 -- 25 -- Original
            // 02 0015 76 0001 424B2D412020 00 0E 0000 03500BBA 023DAFAC D7 -- 25 -- C#
            // string rtext = BitConverter.ToString(toWrite.ToArray()).Replace("-","");

            string res = "";
            try
            {
                byte[] buff = WriteAndRead(toWrite.ToArray());
                string msg = Encoding.ASCII.GetString(buff, 0, buff.Length);
                if (buff.Length >= 4 && buff[0] == 0x76 && buff[1] == 0x00 && buff[2] == 0x02 && buff[3] == 0x00)
                    res = "Writed Ok";
                else
                    res = "Writed Failed";
            }
            catch (Exception ex) { return ex.ToString(); };
            try { sp.Close(); } catch { };
            return res;
        }

        private byte[] WriteAndRead(byte[] data)
        {
            Exception err = null;
            try
            {
                sp.Open();
                sp.Write(data, 0, data.Length);
                System.Threading.Thread.Sleep(READ_SLEEP);
                byte[] buff = new byte[BUFFER_SIZE];
                int rdd = sp.Read(buff, 0, buff.Length);
                if(rdd < buff.Length) Array.Resize(ref buff, rdd);
                sp.Close();
                return buff;
            }
            catch (Exception ex) { err = ex; }
            try { sp.Close(); } catch { };
            throw err;
        }

        public static ushort GetUint16(byte[] buff, int pos)
        {
            byte[] b = new byte[2];
            Array.Copy(buff, pos, b, 0, 2);
            Array.Reverse(b);
            return BitConverter.ToUInt16(b, 0);
        }

        public static int GetInt32(byte[] buff, int pos)
        {
            //return (int)BitConverter.ToUInt32(buff, pos);
            byte[] b = new byte[4];
            Array.Copy(buff, pos, b, 0, 4);
            Array.Reverse(b);
            return BitConverter.ToInt32(b, 0);
        }

        public static byte[] GetUint16(ushort value)
        {
            byte[] b = BitConverter.GetBytes(value);
            Array.Reverse(b);
            return b;
        }

        public static byte[] GetInt32(int value)
        {
            byte[] b = BitConverter.GetBytes(value);
            Array.Reverse(b);
            return b;
        }
    }
}

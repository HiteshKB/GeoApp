using Newtonsoft.Json.Linq;
using System;
using System.Device.Location;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Navigation;

namespace SunRiSetApp
{
    public partial class MainWindow : Window
    {
        GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
        StringBuilder sbUrl = null;
        public MainWindow()
        {
            InitializeComponent();
            sbUrl = new StringBuilder(@"https://www.google.com/maps/@HKBLat,HKBLong");
            location.Navigate(sbUrl.ToString());
        }

        private void BtnGetData_Click(object sender, RoutedEventArgs e)
        {
            string ist = "India Standard Time";
            if (decimal.TryParse(txtLongi.Text, out decimal longi) && decimal.TryParse(txtLati.Text, out decimal lati))
            {
                string result = CallSunriseSunsetApi(lati, longi);
                var riset = JObject.Parse(result);
                string srise = riset["results"]["sunrise"].ToString();
                string sset = riset["results"]["sunset"].ToString();
                lblSunriseOut.Content = "Sunrise Time : " + ParseTimeFromUTC(srise, ist);
                lblSunsetOut.Content = "Sunset Time  : " + ParseTimeFromUTC(sset, ist);
            }
            else
            {
                lblSunriseOut.Content = "Invalid Input!!!";
            }
        }
        private string ParseTimeFromUTC(string time, string ist)
        {
            DateTime utcdate = DateTime.ParseExact(time, "h:mm:ss tt", CultureInfo.InvariantCulture);
            var tz = TimeZoneInfo.ConvertTimeFromUtc(utcdate, TimeZoneInfo.FindSystemTimeZoneById(ist));
            return tz.Hour > 12 ? tz.ToLongTimeString() + " PM" : tz.ToLongTimeString() + " AM";
        }
        private string CallSunriseSunsetApi(decimal lat, decimal lng)
        {
            using (var htClient = new HttpClient())
            {
                var response = htClient.GetAsync(@"https://api.sunrise-sunset.org/json?lat=" + lat + "&lng=" + lng).Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    lblSunriseOut.Content = "Unable to fetch data from API...";
                    return null;
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));
            GeoCoordinate cord = watcher.Position.Location;

            if (cord.IsUnknown != true)
            {
                txtLati.Text = cord.Latitude.ToString();
                txtLongi.Text = cord.Longitude.ToString();
                sbUrl.Replace("HKBLat", cord.Latitude.ToString()).Replace("HKBLong", cord.Longitude.ToString());
                location.Navigate(sbUrl.ToString());
            }
            else
                lblSunriseOut.Content = "Unknown latitude and longitude.";
        }
        private void Location_Navigated(object sender, NavigationEventArgs e)
        {
            txtUrl.Text = e.Uri.AbsoluteUri;
            if (e.Uri.Segments.Length > 2)
            {
                foreach (var seg in e.Uri.Segments)
                {
                    string cords = seg.ToString();
                    if (cords[0] == '@')
                    {
                        string[] latlng = cords.Substring(1).Split(',');
                        if (decimal.TryParse(latlng[0], out decimal lat) && decimal.TryParse(latlng[1], out decimal lng))
                        {
                            txtLati.Text = lat.ToString();
                            txtLongi.Text = lng.ToString();
                        }
                    }
                }
            }

        }
    }
}

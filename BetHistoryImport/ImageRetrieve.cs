using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using SportsDatabaseSqlite.Tables;
using SportsDatabaseSqlite;
using System.Windows.Media.Imaging;
using System.Security.Policy;
using System.Windows.Input;

namespace BetHistoryImport
{

    public partial class MainWindow : Window
    {
        private List<string> imageUrls;
        private int urlIndex = 0;
        private int playerIndex = 0;
        private string selectedName;
        private List<PlayerInfo> sels;

        private string GetHtmlCode(string name)
        {

            string url = "https://www.google.com/search?q=" + name.Replace(' ','+') + myGuiProperties.AditionalImageTextSearch.Replace(' ', '+') + "&tbm=isch";
            string data = "";

            var request = (HttpWebRequest)WebRequest.Create(url);
            //request.Accept = "text/html, application/xhtml+xml, */*";
            //request.UserAgent = ".Net Image Request";

            var response = (HttpWebResponse)request.GetResponse();

            using (Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return "";
                using (var sr = new StreamReader(dataStream))
                {
                    data = sr.ReadToEnd();
                }
            }
            return data;
        }

        private List<string> GetUrls(string html)
        {
            var urls = new List<string>();
            int ndx = html.IndexOf("<a href=", StringComparison.Ordinal);
            ndx = html.IndexOf("<img", ndx, StringComparison.Ordinal);

            while (ndx >= 0)
            {
                ndx = html.IndexOf("src=\"", ndx, StringComparison.Ordinal);
                ndx = ndx + 5;
                int ndx2 = html.IndexOf("\"", ndx, StringComparison.Ordinal);
                string url = html.Substring(ndx, ndx2 - ndx);
                if (url.Contains("http"))
                {
                    urls.Add(url);
                }
                ndx = html.IndexOf("<img", ndx, StringComparison.Ordinal);
            }
            return urls;
        }

        private byte[] GetImageByte(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            using (Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return null;
                using (var sr = new BinaryReader(dataStream))
                {
                    byte[] bytes = sr.ReadBytes(100000);

                    return bytes;
                }
            }

            return null;
        }

        public List<string> GetImageUrlsFromInternet(string name)
        {
            selectedName = name;
            string html = GetHtmlCode(name);
            List<string> urls = GetUrls(html);
            
            return urls;
        }

        private void UseImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImageFromInternet(imageUrls[urlIndex]);
        }

        private void RunImageBtn_Click(object sender, RoutedEventArgs e)
        {
            HideAllWindows();
            if (ImageGrid.Visibility == Visibility.Visible)
            {
                dataGrid.Visibility = Visibility.Visible;
                dataGrid.Focus();
                resTypeTxtBox.IsEnabled = true;
                evTypeTxtBox.IsEnabled = true;
            }
            else
            {
                ImageGrid.Visibility = Visibility.Visible;


                resTypeTxtBox.IsEnabled = false;
                evTypeTxtBox.IsEnabled = false;
                ImageGrid.Focus();
                

                using (var db = new SportsDatabaseModel())
                {
                    sels = db.playerInfo.Where(x => x.Image == null).ToList();
                }

                imageUrls = GetImageUrlsFromInternet(sels.First().Name);
                
                //var iimg = GetImageFromInternet(imageUrls[0]);
                SetImageInWindow();
            }

        }

        private void SetImageInWindow()
        {
            var src = new BitmapImage(new Uri(imageUrls[urlIndex]));

            myGuiProperties.Image1 = src;
        }

        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            urlIndex++;
            SetImageInWindow();
            if(urlIndex >= imageUrls.Count - 1)
            {
                nextImage.IsEnabled = false;
            }

        }

        public void SaveImageFromInternet(string url)
        {
            byte[] image = GetImageByte(url);
            using (var db = new SportsDatabaseModel())
            {
                db.playerInfo.First(x => x.Name == selectedName).Image = image;

                db.SaveChanges();
            }
        }

        private void ImageGrid_KeyDown(object sender, KeyEventArgs e)
        {
            savedLbl.Visibility = Visibility.Hidden;
            if (ImageGrid.Visibility != Visibility.Visible)
                return;
            if(e.Key == Key.Down)
            {
                playerIndex++;
                urlIndex = 0;
                imageUrls = GetImageUrlsFromInternet(sels[playerIndex].Name);
                SetImageInWindow();
            }
            else if (e.Key == Key.Up)
            {
                playerIndex--;
                urlIndex = 0;
                imageUrls = GetImageUrlsFromInternet(sels[playerIndex].Name);
                SetImageInWindow();

            }
            else if (e.Key == Key.Left)
            {
                if(urlIndex > 0)
                    urlIndex--;
                SetImageInWindow();
            }
            else if (e.Key == Key.Right)
            {
                urlIndex++;
                SetImageInWindow();
            }
            else if (e.Key == Key.W)
            {
                SaveImageFromInternet(imageUrls[urlIndex]);
                savedLbl.Visibility = Visibility.Visible;
            }
            playerGrid.Focus();

        }

    }
}

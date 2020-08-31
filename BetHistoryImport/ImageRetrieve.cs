using SportsDatabaseSqlite;
using SportsDatabaseSqlite.Tables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BetHistoryImport
{
    public partial class MainWindow : Window
    {
        private List<string> imageUrls;
        private int urlIndex = 0;
        private int playerIndex = 0;
        private string selectedName;
        private List<PlayerInfo> sels;
        private static string imagePathExtension = ".png";
        private List<IGrouping<string, OddsInfoMedian>> meds;

        private string GetHtmlCode(string name)
        {
            myGuiProperties.PlayerName1 = name;

            //string url = "https://www.google.com/search?q=" + name.Replace(' ','+') + myGuiProperties.AditionalImageTextSearch.Replace(' ', '+') + "&tbm=isch";
            string url = "https://www.ufc.com/athlete/" + name.Replace(' ', '-').Replace("'", "");
            string data = "";

            var request = (HttpWebRequest)WebRequest.Create(url);
            //request.Accept = "text/html, application/xhtml+xml, */*";
            //request.UserAgent = ".Net Image Request";
            try
            {
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
            catch
            {
                return "";
            }
        }

        private IDictionary<string, string> GetPlayerMetrics(string html)
        {
            var metricsDict = new Dictionary<string, string>();
            metricsDict.Add("SA", GetStringInHtml(html, "Striking accuracy ", "%"));
            metricsDict.Add("GA", GetStringInHtml(html, "Grappling accuracy ", "%"));
            metricsDict.Add("SSD", GetStringInHtml(html, "Sig. Str. Defense", "\n", -152, ">"));
            metricsDict.Add("SubAv", GetStringInHtml(html, "Submission avg", "\n", -100, ">"));
            metricsDict.Add("TAv", GetStringInHtml(html, "Takedown avg", "\n", -100, ">"));
            metricsDict.Add("StrAb", GetStringInHtml(html, "Sig. Str. Absorbed", "\n", -100, ">"));
            metricsDict.Add("StrL", GetStringInHtml(html, "Sig. Str. Landed", "\n", -100, ">"));
            metricsDict.Add("Reach", GetStringInHtml(html, ">Reach<", "<", 40, ">"));
            metricsDict.Add("LegReach", GetStringInHtml(html, ">Leg reach<", "<", 40, ">"));
            metricsDict.Add("Weight", GetStringInHtml(html, ">Weight<", "<", 40, ">"));
            metricsDict.Add("Height", GetStringInHtml(html, ">Height<", "<", 40, ">"));

            return metricsDict;
        }

        private static string GetStringInHtml(string html, string queryStartStr, string queryEndStr, double addToIndx = 0d, string secondQuery = "")
        {
            int ndx = html.IndexOf(queryStartStr, StringComparison.Ordinal) + queryStartStr.Length;
            if (ndx > 0 + queryStartStr.Length)
            {
                if (addToIndx != 0)
                {
                    if (addToIndx > 0)
                    {
                        ndx += (int)addToIndx;
                    }
                    else
                    {
                        ndx -= (int)(addToIndx * -1);
                    }
                }
                if (secondQuery != "")
                {
                    ndx = html.IndexOf(secondQuery, ndx, StringComparison.Ordinal) + secondQuery.Length;
                }
                int ndx2 = html.IndexOf(queryEndStr, ndx, StringComparison.Ordinal);
                return html.Substring(ndx, ndx2 - ndx);
            }
            else
            {
                return "";
            }
        }

        private List<string> GetUrls(string html)
        {
            var urls = new List<string>();

            //int ndx = html.IndexOf("<a href=", StringComparison.Ordinal);
            //ndx = html.IndexOf("<img", ndx, StringComparison.Ordinal);
            int ndx = html.IndexOf("c-bio__image", StringComparison.Ordinal);
            if (ndx < 0)
                return new List<string>();
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
                else if (url.Contains("styles"))
                {
                    urls.Add("https://dmxg5wxfqgb4u.cloudfront.net" + url);
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
            if (String.IsNullOrEmpty(html))
            {
                return new List<string>();
            }
            List<string> urls = GetUrls(html);

            return urls;
        }

        private void UseImage_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                ExportPlayerMetrics(0);
            });
        }

        private async void ExportPlayerMetrics(int iC)
        {
            int iCount = iC;
            //Itterate to next
            if (iCount > 10)
                return;
            await Task.Run(async () =>
            {
                
                using (var db = new SportsDatabaseModel())
                {
                    var evsList = new List<string>();
                    var playList = new List<PlayerInfo>();
                    var players = db.playerInfo.ToList();
                    meds = db.oddsInfoMed.GroupBy(x => x.MarketID).ToList();
                    foreach (var m in meds)
                    {
                        evsList = m.Select(x => x.SelectionName).ToList();
                        foreach (var ev in evsList)
                        {
                            var p = players.FirstOrDefault(x => x.Name == ev);
                            if (p != null)
                            {
                                playList.Add(p);
                            }
                        }
                        PlayerInfo best;
                        if (playList.Count >= 2)
                        {
                            switch (iCount)
                            {
                                case 0:
                                    best = playList.Aggregate((largest, next) => largest.Height > next.Height ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb && sb != null)
                                    {
                                        sb.SelectionBias = true;
                                    }
                                    break;

                                case 1:
                                    best = playList.Aggregate((largest, next) => largest.Weight > next.Weight ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb1 && sb1 != null)
                                    {
                                        sb1.SelectionBias = true;
                                    }
                                    break;

                                case 2:
                                    best = playList.Aggregate((largest, next) => largest.Reach > next.Reach ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb2 && sb2 != null)
                                    {
                                        sb2.SelectionBias = true;
                                    }
                                    break;

                                case 3:
                                    best = playList.Aggregate((largest, next) => largest.LegReach > next.LegReach ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb3 && sb3 != null)
                                    {
                                        sb3.SelectionBias = true;
                                    }
                                    break;

                                case 4:
                                    best = playList.Aggregate((largest, next) => largest.GrapplingAccuracy > next.GrapplingAccuracy ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb4 && sb4 != null)
                                    {
                                        sb4.SelectionBias = true;
                                    }
                                    break;

                                case 5:
                                    best = playList.Aggregate((largest, next) => largest.StrikingAccuracy > next.StrikingAccuracy ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb5 && sb5 != null)
                                    {
                                        sb5.SelectionBias = true;
                                    }
                                    break;

                                case 6:
                                    best = playList.Aggregate((largest, next) => largest.SigStrikeDef > next.SigStrikeDef ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb6 && sb6 != null)
                                    {
                                        sb6.SelectionBias = true;
                                    }
                                    break;

                                case 7:
                                    best = playList.Aggregate((largest, next) => largest.SigStrikesAbs > next.SigStrikesAbs ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb7 && sb7 != null)
                                    {
                                        sb7.SelectionBias = true;
                                    }
                                    break;

                                case 8:
                                    best = playList.Aggregate((largest, next) => largest.SigStrikesLand > next.SigStrikesLand ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb8 && sb8 != null)
                                    {
                                        sb8.SelectionBias = true;
                                    }
                                    break;

                                case 9:
                                    best = playList.Aggregate((largest, next) => largest.SubmissionAvg > next.SubmissionAvg ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb9 && sb9 != null)
                                    {
                                        sb9.SelectionBias = true;
                                    }
                                    break;

                                case 10:
                                    best = playList.Aggregate((largest, next) => largest.TakedownAvg > next.TakedownAvg ? largest : next);
                                    if (m.FirstOrDefault(x => x.SelectionName == best.Name) is var sb0 && sb0 != null)
                                    {
                                        sb0.SelectionBias = true;
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                        playList.Clear();
                    }
                    db.SaveChanges();
                    switch (iCount)
                    {
                        case 0:
                            await ExportFinalOddsToCSV("Height");
                            break;

                        case 1:
                            await ExportFinalOddsToCSV("Weight");
                            break;

                        case 2:
                            await ExportFinalOddsToCSV("Reach");
                            break;

                        case 3:
                            await ExportFinalOddsToCSV("LegReach");
                            break;

                        case 4:
                            await ExportFinalOddsToCSV("GrapplingAccuracy");
                            break;

                        case 5:
                            await ExportFinalOddsToCSV("StrikingAccuracy");
                            break;

                        case 6:
                            await ExportFinalOddsToCSV("SigStrikeDef");
                            break;

                        case 7:
                            await ExportFinalOddsToCSV("SigStrikesAbs");
                            break;

                        case 8:
                            await ExportFinalOddsToCSV("SigStrikesLand");
                            break;

                        case 9:
                            await ExportFinalOddsToCSV("SubmissionAvg");
                            break;

                        case 10:
                            await ExportFinalOddsToCSV("TakedownAvg");
                            break;

                        default:
                            break;
                    }
                    iCount++;
                    foreach (var o in db.oddsInfoMed.OrderBy(x => x.ID))
                    {
                        o.SelectionBias = false;
                    }
                    db.SaveChanges();
                }
            });
            iC = iC + 1;
            ExportPlayerMetrics(iC);
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
                meds = new List<IGrouping<string, OddsInfoMedian>>();
                using (var db = new SportsDatabaseModel())
                {
                    var players = db.playerInfo.Where(x => x.Gender == 2).Select(x => x.Name).ToList();
                    meds = db.oddsInfoMed.GroupBy(x => x.EventName).Where(x => x.Any(y => players.Contains(y.SelectionName))).OrderBy(p => p.Key).ToList();
                }
                playerIndex = 0;
                SetupImagesForBias(meds[playerIndex]);
                //ImageGrid.Visibility = Visibility.Visible;

                //resTypeTxtBox.IsEnabled = false;
                //evTypeTxtBox.IsEnabled = false;
                //ImageGrid.Focus();

                //using (var db = new SportsDatabaseModel())
                //{
                //    sels = db.playerInfo.Where(x=>String.IsNullOrEmpty(x.ImagePath)).ToList();
                //}

                //imageUrls = GetImageUrlsFromInternet(sels.First().Name);

                ////var iimg = GetImageFromInternet(imageUrls[0]);
                //SetImageInWindow();
            }
        }

        private void SetupImagesForBias(IGrouping<string, OddsInfoMedian> grouping, bool reversing = false)
        {
            PlayerInfo p1;
            PlayerInfo p2;
            if (grouping.Count() != 2)
                return;

            var g1 = grouping.First();
            var g2 = grouping.Skip(1).First();
            using (var db = new SportsDatabaseModel())
            {
                p1 = db.playerInfo.OrderBy(x => x.ID).FirstOrDefault(x => x.Name == g1.SelectionName);
                p2 = db.playerInfo.OrderBy(x => x.ID).FirstOrDefault(x => x.Name == g2.SelectionName);
            }
            if (p1 == null || p2 == null)
            {
                if (reversing)
                {
                    playerIndex--;
                }
                else
                {
                    playerIndex++;
                }
                SetupImagesForBias(meds[playerIndex], reversing);
            }
            else
            {
                if (LoadImageFromResource(p1.ImagePath) == null)
                {
                    imageUrls = GetImageUrlsFromInternet(p1.Name);
                    if (imageUrls?.Count > 0)
                    {
                        myGuiProperties.Image1 = SetImageInWindow(imageUrls[0]);
                    }
                }
                else
                {
                    myGuiProperties.Image1 = LoadImageFromResource(p1.ImagePath);
                }
                if (LoadImageFromResource(p2.ImagePath) == null)
                {
                    imageUrls = GetImageUrlsFromInternet(p2.Name);
                    if (imageUrls.Count > 0)
                    {
                        myGuiProperties.Image2 = SetImageInWindow(imageUrls[0]);
                    }
                }
                else
                {
                    myGuiProperties.Image2 = LoadImageFromResource(p2.ImagePath);
                }
                myGuiProperties.PlayerName1 = p1.Name;
                image1Lbl.Foreground = g1.SelectionBias ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.White;
                myGuiProperties.PlayerName2 = p2.Name;
                image2Lbl.Foreground = g2.SelectionBias ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.White;
            }
        }

        private ImageSource SetImageInWindow(string url)
        {
            if (imageUrls.Count > 0)
            {
                var src = new BitmapImage(new Uri(url));
                return src;
            }
            return null;
            //else
            //{
            //    im = LoadImageFromResource(sels[playerIndex].Name + imagePathExtension);
            //}
        }

        private Bitmap GetImageFromBytes(byte[] image)
        {
            if (image == null)
                return null;
            using (var ms = new MemoryStream(image))
            {
                ms.Write(image, 0, image.Length);
                ms.Seek(0, SeekOrigin.Begin);

                Bitmap bm = new Bitmap(ms);
                return bm;
            }
        }

        public BitmapSource ConvertToBitmap(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            nextImage.IsEnabled = false;
            Task.Run(async () =>
            {
                foreach (var n in sels.Select(x => x.Name))
                {
                    var d = GetPlayerMetrics(GetHtmlCode(n));
                    WritePlayerMetrics(d, n);
                }
                InvokeUI(() =>
                {
                    nextImage.IsEnabled = true;
                });
            });
        }

        public void SaveImageFromInternet(string url)
        {
            var src = new BitmapImage(new Uri(imageUrls[urlIndex]));
            SaveImageToResource(src, selectedName + imagePathExtension);
            using (var db = new SportsDatabaseModel())
            {
                db.playerInfo.First(x => x.Name == selectedName).ImagePath = selectedName + imagePathExtension;

                db.SaveChanges();
            }
        }

        private bool SaveImageToResource(BitmapImage src, string v)
        {
            if (src == null)
            {
                return false;
            }
            try
            {
                BitmapEncoder be = new PngBitmapEncoder();
                be.Frames.Add(BitmapFrame.Create(src));
                if (System.IO.Directory.Exists(currentDirectory + "\\Images\\" + v))
                {
                    System.IO.Directory.Delete(currentDirectory + "\\Images\\" + v);
                }
                using (var stream = new System.IO.FileStream(currentDirectory + "\\Images\\" + v, System.IO.FileMode.Create))
                {
                    be.Save(stream);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private BitmapImage LoadImageFromResource(string path)
        {
            var bit = new BitmapImage();
            if (System.IO.File.Exists(currentDirectory + "\\Images\\" + path))
            {
                using (var stream = new FileStream(currentDirectory + "\\Images\\" + path, FileMode.Open))
                {
                    bit.BeginInit();
                    bit.CacheOption = BitmapCacheOption.OnLoad;
                    bit.StreamSource = stream;
                    bit.EndInit();
                    bit.Freeze();
                }
            }
            else
            {
                return null;
            }
            return bit;
        }

        private void ImageGrid_KeyDown(object sender, KeyEventArgs e)
        {
            savedLbl.Visibility = Visibility.Hidden;
            if (ImageGrid.Visibility != Visibility.Visible)
                return;
            switch (e.Key)
            {
                case Key.Down:
                    //playerIndex++;
                    //urlIndex = 0;
                    //imageUrls = GetImageUrlsFromInternet(sels[playerIndex].Name);
                    //SetImageInWindow();
                    playerIndex++;
                    SetupImagesForBias(meds[playerIndex]);
                    break;

                case Key.Up:
                    //if (playerIndex <= 0)
                    //    break;
                    //playerIndex--;
                    //urlIndex = 0;
                    //imageUrls = GetImageUrlsFromInternet(sels[playerIndex].Name);
                    //SetImageInWindow();
                    playerIndex--;
                    SetupImagesForBias(meds[playerIndex], true);
                    break;

                case Key.Left:
                    //if (urlIndex > 0)
                    //    urlIndex--;
                    //SetImageInWindow();
                    SetSelectionBias(meds[playerIndex], 1);
                    playerIndex++;
                    SetupImagesForBias(meds[playerIndex]);
                    break;

                case Key.Right:
                    //if(urlIndex < imageUrls.Count - 1)
                    //{
                    //    urlIndex++;
                    //    SetImageInWindow();
                    //}
                    SetSelectionBias(meds[playerIndex], 1);
                    playerIndex++;
                    SetupImagesForBias(meds[playerIndex]);
                    break;

                case Key.W:
                    if (imageUrls.Count > 0)
                    {
                        SaveImageFromInternet(imageUrls[urlIndex]);
                        savedLbl.Visibility = Visibility.Visible;
                    }
                    playerIndex++;
                    urlIndex = 0;
                    imageUrls = GetImageUrlsFromInternet(sels[playerIndex].Name);
                    myGuiProperties.Image1 = SetImageInWindow(imageUrls[urlIndex]);
                    break;

                case Key.F:
                    SetFemale(sels[playerIndex]);
                    playerIndex++;
                    urlIndex = 0;
                    imageUrls = GetImageUrlsFromInternet(sels[playerIndex].Name);
                    myGuiProperties.Image1 = SetImageInWindow(imageUrls[urlIndex]);
                    break;

                case Key.M:
                    SetMale(sels[playerIndex]);
                    playerIndex++;
                    urlIndex = 0;
                    imageUrls = GetImageUrlsFromInternet(sels[playerIndex].Name);
                    myGuiProperties.Image1 = SetImageInWindow(imageUrls[urlIndex]);
                    break;

                case Key.S:
                    var d = GetPlayerMetrics(GetHtmlCode(selectedName));
                    WritePlayerMetrics(d, selectedName);
                    break;

                case Key.A:
                    SetSelectionBias(meds[playerIndex], 0);
                    playerIndex++;
                    SetupImagesForBias(meds[playerIndex]);
                    break;

                case Key.D:
                    SetSelectionBias(meds[playerIndex], 1);
                    playerIndex++;
                    SetupImagesForBias(meds[playerIndex]);
                    break;

                case Key.X:
                    SetSelectionBiasFalse(meds[playerIndex]);
                    playerIndex++;
                    SetupImagesForBias(meds[playerIndex]);
                    break;

                default:
                    break;
            }

            playerGrid.Focus();
        }

        private void SetSelectionBias(IGrouping<string, OddsInfoMedian> grouping, int v)
        {
            if (grouping == null)
                return;
            using (var db = new SportsDatabaseModel())
            {
                var g = grouping.Skip(v).First().SelectionName;
                var e = grouping.First();
                var choice = db.oddsInfoMed.First(x => x.EventName == e.EventName && x.EventDate == e.EventDate && x.SelectionName == g);
                choice.SelectionBias = true;
                choice = db.oddsInfoMed.First(x => x.EventName == e.EventName && x.EventDate == e.EventDate && x.SelectionName != g);
                choice.SelectionBias = false;
                db.SaveChanges();
            }
        }

        private void SetSelectionBiasFalse(IGrouping<string, OddsInfoMedian> grouping)
        {
            using (var db = new SportsDatabaseModel())
            {
                var g = grouping.First().SelectionName;
                var e = grouping.First();
                var choices = db.oddsInfoMed.Where(x => x.EventName == e.EventName && x.EventDate == e.EventDate).ToList();
                for (int i = 0; i < choices.Count(); i++)
                {
                    choices[i].SelectionBias = false;
                }
                db.SaveChanges();
            }
        }

        private void WritePlayerMetrics(IDictionary<string, string> d, string selectedName)
        {
            using (var db = new SportsDatabaseModel())
            {
                var plyr = db.playerInfo.FirstOrDefault(x => x.Name == selectedName);
                if (plyr == null)
                {
                    return;
                }
                plyr.GrapplingAccuracy = float.Parse(d["GA"] == "" ? "0" : d["GA"]);
                plyr.Height = float.Parse(d["Height"] == "" ? "0" : d["Height"]);
                plyr.LegReach = float.Parse(d["LegReach"] == "" ? "0" : d["LegReach"]);
                plyr.Reach = float.Parse(d["Reach"] == "" ? "0" : d["Reach"]);
                plyr.SigStrikeDef = float.Parse(d["SSD"] == "" ? "0" : d["SSD"]);
                plyr.SigStrikesAbs = float.Parse(d["StrAb"] == "" ? "0" : d["StrAb"]);
                plyr.SigStrikesLand = float.Parse(d["StrL"] == "" ? "0" : d["StrL"]);
                plyr.StrikingAccuracy = float.Parse(d["SA"] == "" ? "0" : d["SA"]);
                plyr.SubmissionAvg = float.Parse(d["SubAv"] == "" ? "0" : d["SubAv"]);
                plyr.TakedownAvg = float.Parse(d["TAv"] == "" ? "0" : d["TAv"]);
                plyr.Weight = float.Parse(d["Weight"] == "" ? "0" : d["Weight"]);

                db.SaveChanges();
            }
        }

        private void SetFemale(PlayerInfo playerInfo)
        {
            using (var db = new SportsDatabaseModel())
            {
                db.playerInfo.First(x => x.Name == selectedName).Gender = 1;

                db.SaveChanges();
            }
        }

        private void SetMale(PlayerInfo playerInfo)
        {
            using (var db = new SportsDatabaseModel())
            {
                db.playerInfo.First(x => x.Name == selectedName).Gender = 0;

                db.SaveChanges();
            }
        }
    }
}
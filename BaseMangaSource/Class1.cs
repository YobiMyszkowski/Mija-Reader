using System.Threading.Tasks;
using System.ComponentModel; //INotifyPropertyChanged
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Media;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions; //Regex
using System;
using System.Text;

namespace BaseMangaSource
{
    public enum MangaType
    {
        Manga_RightToLeft,
        Manhwa_LeftToRight
    }
    public enum MangaStatus
    {
        Ongoing,
        Completed
    }
    public class MangaSearchData : INotifyPropertyChanged
    {
        private string _Name;
        public string Name { get { return _Name; } set { if (_Name != value) { _Name = value; RaisePropertyChanged("Name"); } } } // Chapter name
        private string _Website;
        public string Website { get { return _Website; } set { if (_Website != value) { _Website = value; RaisePropertyChanged("Website"); } } } // Url to main website (place where we can get more data)
        private string _Image;
        public string Image { get { return _Image; } set { if (_Image != value) { _Image = value; RaisePropertyChanged("Image"); } } } // Url to image
        private int _NextPage;
        public int NextPage { get { return _NextPage; } set { if (_NextPage != value) { _NextPage = value; RaisePropertyChanged("NextPage"); } } } // Value for next page
        private int _PrevPage;
        public int PrevPage { get { return _PrevPage; } set { if (_PrevPage != value) { _PrevPage = value; RaisePropertyChanged("PrevPage"); } } } // Value for previous page
        private int _FirstPage;
        public int FirstPage { get { return _FirstPage; } set { if (_FirstPage != value) { _FirstPage = value; RaisePropertyChanged("FirstPage"); } } } // Value for first page
        private string _ErrorMessage;
        public string ErrorMessage { get { return _ErrorMessage; } set { if (_ErrorMessage != value) { _ErrorMessage = value; RaisePropertyChanged("ErrorMessage"); } } } // Error Message

        #region Methods
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
    public class MangaPageData : INotifyPropertyChanged
    {
        private string _Image;
        public string Image { get { return _Image; } set { if (_Image != value) { _Image = value; RaisePropertyChanged("Image"); } } } // Url to image
        private string _Name;
        public string Name { get { return _Name; } set { if (_Name != value) { _Name = value; RaisePropertyChanged("Name"); } } } // manga name
        private string _AlternateName;
        public string AlternateName { get { return _AlternateName; } set { if (_AlternateName != value) { _AlternateName = value; RaisePropertyChanged("AlternateName"); } } } // manga other names
        private string _YearOfRelease;
        public string YearOfRelease { get { return _YearOfRelease; } set { if (_YearOfRelease != value) { _YearOfRelease = value; RaisePropertyChanged("YearOfRelease"); } } } // manga year of release
        private MangaStatus _Status;
        public MangaStatus Status { get { return _Status; } set { if (_Status != value) { _Status = value; RaisePropertyChanged("Status"); } } } // manga status
        private string _Author;
        public string Author { get { return _Author; } set { if (_Author != value) { _Author = value; RaisePropertyChanged("Author"); } } } // manga author
        private string _Artist;
        public string Artist { get { return _Artist; } set { if (_Artist != value) { _Artist = value; RaisePropertyChanged("Artist"); } } } // manga artist
        private MangaType _Type;
        public MangaType Type { get { return _Type; } set { if (_Type != value) { _Type = value; RaisePropertyChanged("Type"); } } } // manga type
        private string _Description;
        public string Description { get { return _Description; } set { if (_Description != value) { _Description = value; RaisePropertyChanged("Description"); } } } // manga description
        private string _Genre;
        public string Genre { get { return _Genre; } set { if (_Genre != value) { _Genre = value; RaisePropertyChanged("Genre"); } } } // manga genre
        private string _Website;
        public string Website { get { return _Website; } set { if (_Website != value) { _Website = value; RaisePropertyChanged("Website"); } } } //Manga plugin (what plugin we used to find it and add)
        private string _UrlToMainpage;
        public string UrlToMainpage { get { return _UrlToMainpage; } set { if (_UrlToMainpage != value) { _UrlToMainpage = value; RaisePropertyChanged("UrlToMainpage"); } } } //Manga website 
        private int _UnreadChapters;
        public int UnreadChapters { get { return _UnreadChapters; } set { if (_UnreadChapters != value) { _UnreadChapters = value; RaisePropertyChanged("UnreadChapters"); } } } //amount of unread chapters


        private string _ErrorMessage;
        public string ErrorMessage { get { return _ErrorMessage; } set { if (_ErrorMessage != value) { _ErrorMessage = value; RaisePropertyChanged("ErrorMessage"); } } } // Error Message

        public static bool operator ==(MangaPageData x, MangaPageData y)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(x, y))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }
            if (x.Image == y.Image
                && x.Name == y.Name
                && x.AlternateName == y.AlternateName
                && x.YearOfRelease == y.YearOfRelease
                && x.Status == y.Status
                && x.Type == y.Type
                && x.Author == y.Author
                && x.Artist == y.Artist
                && x.Description == y.Description
                && x.Genre == y.Genre
                && x.Website == y.Website
                && x.UrlToMainpage == y.UrlToMainpage
                && x.UnreadChapters == y.UnreadChapters)
                return true;
            else
                return false;
        }
        public static bool operator !=(MangaPageData x, MangaPageData y)
        {
            return !(x == y);
        }


        #region Methods
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    };
    public class MangaPageChapters : INotifyPropertyChanged
    {
        private string _Name;
        public string Name { get { return _Name; } set { if (_Name != value) { _Name = value; RaisePropertyChanged("Name"); } } } // Chapter name
        private string _RealName;
        public string RealName { get { return _RealName; } set { if (_RealName != value) { _RealName = value; RaisePropertyChanged("RealName"); } } } // Chapter real name
        private string _UrlToPage;
        public string UrlToPage { get { return _UrlToPage; } set { if (_UrlToPage != value) { _UrlToPage = value; RaisePropertyChanged("UrlToPage"); } } } // Chapter website
        private SolidColorBrush _Foreground;
        public SolidColorBrush Foreground { get { return _Foreground; } set { if (_Foreground != value) { _Foreground = value; RaisePropertyChanged("Foreground"); } } } // Chapter website

        #region Methods
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    };
    public class MangaImagesData : INotifyPropertyChanged
    {
        private uint _PageNumber;
        public uint PageNumber { get { return _PageNumber; } set { if (_PageNumber != value) { _PageNumber = value; RaisePropertyChanged("PageNumber"); } } } // current page number
        private string _ImageLink;
        public string ImageLink { get { return _ImageLink; } set { if (_ImageLink != value) { _ImageLink = value; RaisePropertyChanged("ImageLink"); } } } // image link
        private string _PageLink;
        public string PageLink { get { return _PageLink; } set { if (_PageLink != value) { _PageLink = value; RaisePropertyChanged("PageLink"); } } } // current page link
        private string _NextLink;
        public string NextLink { get { return _NextLink; } set { if (_NextLink != value) { _NextLink = value; RaisePropertyChanged("NextLink"); } } } // next page link
        private string _PrewLink;
        public string PrewLink { get { return _PrewLink; } set { if (_PrewLink != value) { _PrewLink = value; RaisePropertyChanged("PrewLink"); } } } // previous page link
        private uint _MaxPages;
        public uint MaxPages { get { return _MaxPages; } set { if (_MaxPages != value) { _MaxPages = value; RaisePropertyChanged("MaxPages"); } } } // max pages number
        private List<string> _Covers = new List<string>();
        public List<string> Covers { get { return _Covers; } set { if (_Covers != value) { _Covers = value; RaisePropertyChanged("Covers"); } } } // list off all images/work only LoadImagesFromOnePage=true
        private string _ErrorMessage;
        public string ErrorMessage { get { return _ErrorMessage; } set { if (_ErrorMessage != value) { _ErrorMessage = value; RaisePropertyChanged("ErrorMessage"); } } } // previous page link

        #region Methods
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    public interface IPlugin
    {
        string Lang { get; }
        string Author { get; }

        string Name { get; }
        string Website { get; set; }
        string Icon { get; }
        int Page { get; set; }
        int FirstPage { get; }
        string SearchQuote { get; set; }
        string ErrorMessage { get; }
        bool LoadImagesFromOnePage { get; }

        Task<bool> ParseSearchAsync(string SearchQuote, bool IgnorePages, int PageNumber, ObservableCollection<MangaSearchData> SearchResults);
        Task<bool> ParseSelectedPageAsync(string URL, bool ParseJustChapters, ObservableCollection<MangaPageData> DetailedInfo, ObservableCollection<MangaPageChapters> ChaptersInfo);
        Task<bool> ParseImagesAsync(string URL, ObservableCollection<MangaImagesData> ReaderInfo);
    }

    public static class config
    {
        public class HttpDownloader
        {
            private readonly string _referer;
            private readonly string _userAgent;

            public Encoding Encoding { get; set; }
            public WebHeaderCollection Headers { get; set; }
            public Uri Url { get; set; }

            public HttpDownloader(string url, string referer, string userAgent)
            {
                Encoding = Encoding.GetEncoding("ISO-8859-1");
                Url = new Uri(url); // verify the uri
                _userAgent = userAgent;
                _referer = referer;
            }

            public string GetPage()
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                if (!string.IsNullOrEmpty(_referer))
                    request.Referer = _referer;
                if (!string.IsNullOrEmpty(_userAgent))
                    request.UserAgent = _userAgent;

                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Headers = response.Headers;
                    Url = response.ResponseUri;
                    return ProcessContent(response);
                }
            }
            public async Task<string> GetPageAsync()
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                if (!string.IsNullOrEmpty(_referer))
                    request.Referer = _referer;
                if (!string.IsNullOrEmpty(_userAgent))
                    request.UserAgent = _userAgent;

                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

                using (WebResponse response = await request.GetResponseAsync())
                {
                    Headers = response.Headers;
                    Url = response.ResponseUri;
                    return ProcessContent((HttpWebResponse)response);
                }
            }
            private string ProcessContent(HttpWebResponse response)
            {
                SetEncodingFromHeader(response);

                Stream s = response.GetResponseStream();
                if (response.ContentEncoding.ToLower().Contains("gzip"))
                    s = new GZipStream(s, CompressionMode.Decompress);
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    s = new DeflateStream(s, CompressionMode.Decompress);

                MemoryStream memStream = new MemoryStream();
                int bytesRead;
                byte[] buffer = new byte[0x1000];
                for (bytesRead = s.Read(buffer, 0, buffer.Length); bytesRead > 0; bytesRead = s.Read(buffer, 0, buffer.Length))
                {
                    memStream.Write(buffer, 0, bytesRead);
                }
                s.Close();
                string html;
                memStream.Position = 0;
                using (StreamReader r = new StreamReader(memStream, Encoding))
                {
                    html = r.ReadToEnd().Trim();
                    html = CheckMetaCharSetAndReEncode(memStream, html);
                }

                return html;
            }
            private void SetEncodingFromHeader(HttpWebResponse response)
            {
                string charset = null;
                if (string.IsNullOrEmpty(response.CharacterSet))
                {
                    Match m = Regex.Match(response.ContentType, @";\s*charset\s*=\s*(?<charset>.*)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        charset = m.Groups["charset"].Value.Trim(new[] { '\'', '"' });
                    }
                }
                else
                {
                    charset = response.CharacterSet;
                }
                if (!string.IsNullOrEmpty(charset))
                {
                    try
                    {
                        Encoding = Encoding.GetEncoding(charset);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            private string CheckMetaCharSetAndReEncode(Stream memStream, string html)
            {
                Match m = new Regex(@"<meta\s+.*?charset\s*=\s*(?<charset>[A-Za-z0-9_-]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Match(html);
                if (m.Success)
                {
                    string charset = m.Groups["charset"].Value.ToLower() ?? "iso-8859-1";
                    if ((charset == "unicode") || (charset == "utf-16"))
                    {
                        charset = "utf-8";
                    }

                    try
                    {
                        Encoding metaEncoding = Encoding.GetEncoding(charset);
                        if (Encoding != metaEncoding)
                        {
                            memStream.Position = 0L;
                            StreamReader recodeReader = new StreamReader(memStream, metaEncoding);
                            html = recodeReader.ReadToEnd().Trim();
                            recodeReader.Close();
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                return html;
            }
        }
    }
}

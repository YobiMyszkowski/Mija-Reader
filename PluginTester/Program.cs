using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions; //Regex
using System.Windows.Media;

using System.Collections.ObjectModel;

using System.IO;
using System.IO.Compression;

using BaseMangaSource;


public class Parser : IPlugin
{
    public string Name
    {
        get
        {
            return "My Manga Online";
        }
    }
    public string Icon
    {
        get
        {
            return "http://www.mymangaonline.com/images/icon/favicon.ico";
        }
    }
    private string _Website = "http://mymangaonline.net";
    public string Website
    {
        get
        {
            return _Website;
        }
        set
        {
            _Website = value;
        }
    }
    public string Author
    {
        get
        {
            return "Michał Jachura";
        }
    }
    public string Lang
    {
        get
        {
            return "English";
        }
    }
    private string _ErrorMessage = "";
    public string ErrorMessage
    {
        get
        {
            return _ErrorMessage;
        }
    }
    public int FirstPage
    {
        get
        {
            return 0;
        }
    }
    private int _Page = 0;
    public int Page
    {
        get
        {
            return _Page;
        }

        set
        {
            _Page = value;
        }
    }
    private string _SearchQuote = "";
    public string SearchQuote
    {
        get
        {
            return _SearchQuote;
        }

        set
        {
            _SearchQuote = value;
        }
    }
    public string SearchLink
    {
        get
        {
            return string.Format("{0}/search?keyword={1}&page={2}",
                Website, SearchQuote, Page);
        }
    }
    private bool _LoadImagesFromOnePage = true;
    public bool LoadImagesFromOnePage
    {
        get
        {
            return _LoadImagesFromOnePage;
        }

        set
        {
            _LoadImagesFromOnePage = value;
        }
    }

    public async Task<bool> ParseSearchAsync(string SearchQuote, bool IgnorePages, int PageNumber, ObservableCollection<MangaSearchData> SearchResults)
    {
        _ErrorMessage = "";
        _Page = PageNumber;
        _SearchQuote = SearchQuote;

        string result = "";
        try
        {
            config.HttpDownloader request = new config.HttpDownloader(SearchLink, "", "");
            result = await request.GetPageAsync();
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                throw new Exception("Status Code : " + ((HttpWebResponse)ex.Response).StatusCode + " \nStatus Description : " + ((HttpWebResponse)ex.Response).StatusDescription);
            }
        }
        catch (Exception ex)
        {
            string error = String.Format("{0} - Error - Website: {1} - Reason: {2}", DateTime.Now.ToLongTimeString(), SearchLink, ex.Message);
            throw new Exception(error, ex);
        }

        #region parse website
        if (result.Length > 0 && result != null)
        {
            int NextPage = 0;
            int PrevPage = 0;
            if (IgnorePages != true)
            {
                Match match = Regex.Match(result.ToString(), "<ul class=\"pagination-list\" id=\"yw0\">((.|\n)*?)</ul>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string mangaPageString = match.Groups["1"].Value;
                    match = Regex.Match(mangaPageString, "<li class=\"previous\"><a href=\"(.*?)\">&lt; Previous</a></li>");
                    while (match.Success)
                    {
                        bool convertResult = Int32.TryParse(match.Groups[1].Value.Split('=').Last(), out PrevPage);

                        if (!convertResult)
                            PrevPage = 0;

                        match = match.NextMatch();
                        if (match.Index == 0)
                            break;
                    }
                    match = Regex.Match(mangaPageString, "<li class=\"next\"><a href=\"(.*?)\">Next &gt;</a></li>");
                    while (match.Success)
                    {
                        bool convertResult = Int32.TryParse(match.Groups[1].Value.Split('=').Last(), out NextPage);
                        if (!convertResult)
                            NextPage = 0;

                        match = match.NextMatch();
                        if (match.Index == 0)
                            break;
                    }
                }
            }

            Regex reg = new Regex("<div class=\"box-item(?<Useless>[^<]+)\">(?<Useless>[^<]+)<a href=\"(.*?)\" class=\"image\">(?<Useless>[^<]+)<img class=\"loadanime\" width=\"150\" heigh=\"148\" alt=\"(.*?)\" src=\"(.*?)\" />", RegexOptions.IgnoreCase & RegexOptions.Multiline);
            MatchCollection matches = reg.Matches(result.ToString());

            string MangaCover = "";
            string MangaName = "";
            string MangaWebsite = "";

            foreach (Match match in matches)
            {
                MangaCover = match.Groups[3].Value;
                MangaName = WebUtility.HtmlDecode(match.Groups[2].Value);
                MangaWebsite = Website + match.Groups[1].Value;
                SearchResults.Add(new MangaSearchData() { Name = MangaName, Image = MangaCover, Website = MangaWebsite, FirstPage = Page, NextPage = NextPage, PrevPage = PrevPage });
            }
            return true;
        }
        else
        {
            _ErrorMessage = "Can't load website content.";
            return false;
        }
        #endregion
    }
    public async Task<bool> ParseSelectedPageAsync(string URL, bool ParseJustChapters, ObservableCollection<MangaPageData> DetailedInfo, ObservableCollection<MangaPageChapters> ChaptersInfo)
    {
        string UrlToPage = "";
        string ChapterName = "";
        string RealChapterName = "";

        string result = "";
        try
        {
            config.HttpDownloader request = new config.HttpDownloader(URL, "", "");
            result = await request.GetPageAsync();
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                throw new Exception("Status Code : " + ((HttpWebResponse)ex.Response).StatusCode + " \nStatus Description : " + ((HttpWebResponse)ex.Response).StatusDescription);
            }
        }
        catch (Exception ex)
        {
            string error = String.Format("{0} - Error - Website: {1} - Reason: {2}", DateTime.Now.ToLongTimeString(), SearchLink, ex.Message);
            throw new Exception(error, ex);
        }

        if (result.Length > 0 && result != null)
        {
            if (ParseJustChapters != true)
            {
                MangaPageData data = new MangaPageData();
                data.Image = "";
                data.Name = "";
                data.AlternateName = "";
                data.YearOfRelease = "";
                data.Status = MangaStatus.Completed;
                data.Author = "";
                data.Artist = "";
                data.Type = MangaType.Manga_RightToLeft;
                data.Description = "";
                data.Website = URL;
                data.UrlToMainpage = Website;
                data.UnreadChapters = 0;

                Match match = Regex.Match(result.ToString(), "<div class=\"info\">((.|\n)*?)<div class=\"clearfix\">", RegexOptions.IgnoreCase);
                while (match.Success)
                {
                    string mangaPageString = match.Groups[1].Value;
                    match = Regex.Match(mangaPageString, "<h1>(.*?)</h1>");
                    while (match.Success)
                    {
                        data.Name = WebUtility.HtmlDecode(match.Groups[1].Value);
                        break;
                    }
                    match = Regex.Match(mangaPageString, "div class=\"other_name fields\"><span>Other Name : </span>(.*?)</div>");
                    while (match.Success)
                    {
                        data.AlternateName = WebUtility.HtmlDecode(match.Groups[1].Value);
                        break;
                    }
                    match = Regex.Match(mangaPageString, "<img src=\"(.*?)\" />");
                    while (match.Success)
                    {
                        data.Image = match.Groups[1].Value;
                        break;
                    }
                    match = Regex.Match(mangaPageString, "<span>Year of release : </span>(.*?)</div>");
                    while (match.Success)
                    {
                        data.YearOfRelease = match.Groups[1].Value;
                        break;
                    }
                    match = Regex.Match(mangaPageString, "<span>Status : </span><a href=\"(.*?)\" >");
                    while (match.Success)
                    {
                        if (match.Groups[1].Value.Split('/').Last().ToString() == "Completed")
                            data.Status = MangaStatus.Completed;
                        else
                            data.Status = MangaStatus.Ongoing;
                        break;
                    }
                    match = Regex.Match(mangaPageString, "<span>Author : </span>(.*?)</div>");
                    while (match.Success)
                    {
                        data.Author = WebUtility.HtmlDecode(match.Groups[1].Value);
                        data.Artist = WebUtility.HtmlDecode(match.Groups[1].Value);
                        break;
                    }
                    data.Type = MangaType.Manga_RightToLeft;

                    match = Regex.Match(result.ToString(), "<meta name=\"description\" content=\"((.|\n)*?)\" />");
                    while (match.Success)
                    {
                        data.Description = WebUtility.HtmlDecode(match.Groups[1].Value);
                        break;
                    }
                    Regex reg = new Regex("<a href=\'(.*?)\' title=\'(.*?)\'>(.*?)</a>",
                            RegexOptions.IgnoreCase);
                    MatchCollection matches = reg.Matches(mangaPageString);
                    StringBuilder sb = new StringBuilder();
                    foreach (Match matchGenre in matches)
                    {
                        sb.Append(WebUtility.HtmlDecode(matchGenre.Groups[2].Value));
                        sb.Append(", ");
                    }
                    data.Genre = sb.ToString();

                    DetailedInfo.Add(data);

                    return true;
                }

            }
            else
            {
                Regex reg = new Regex("<div class=\"item-chapter\">(?<Useless>[^<]+)<a href=\"(.*?)\"(?<Useless>[^<]+)title=\"(.*?)\">((.|\n)*?)</a>",
                    RegexOptions.IgnoreCase);
                MatchCollection matches = reg.Matches(result.ToString());

                foreach (Match match in matches)
                {
                    UrlToPage = Website + match.Groups[1].Value;
                    ChapterName = WebUtility.HtmlDecode(match.Groups[2].Value);
                    RealChapterName = WebUtility.HtmlDecode(match.Groups[3].Value.Trim());

                    ChaptersInfo.Add(new MangaPageChapters()
                    {
                        Name = ChapterName,
                        RealName = RealChapterName,
                        UrlToPage = UrlToPage,
                        Foreground = Brushes.Black
                    });
                }

                if (matches.Count == 0)
                {
                    // data.ErrorMessage = "There isn't any chapters in this manga.";
                    return false;
                }
                return true;
            }
        }

        return true;
    }

    public async Task<bool> ParseImagesAsync(string URL, ObservableCollection<MangaImagesData> ReaderInfo)
    {
        MangaImagesData data = new MangaImagesData();
        data.PageNumber = 0;
        data.ImageLink = "";
        data.PageLink = "";
        data.NextLink = "";
        data.PrewLink = "";
        data.MaxPages = 0;

        string result = "";
        try
        {
            config.HttpDownloader request = new config.HttpDownloader(URL, "", "");
            result = await request.GetPageAsync();
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                throw new Exception("Status Code : " + ((HttpWebResponse)ex.Response).StatusCode + " \nStatus Description : " + ((HttpWebResponse)ex.Response).StatusDescription);
            }
        }
        catch (Exception ex)
        {
            string error = String.Format("{0} - Error - Website: {1} - Reason: {2}", DateTime.Now.ToLongTimeString(), SearchLink, ex.Message);
            throw new Exception(error, ex);
        }

        if (result.Length > 0 && result != null)
        {
            Regex reg = new Regex("<p><img src=\"(.*?)\"></p>", RegexOptions.IgnoreCase);
            MatchCollection matches = reg.Matches(result.ToString());
            
            if (matches.Count > 0 && Page < matches.Count)
            {
                data.MaxPages = (uint)matches.Count;
                data.ImageLink = matches[Page].Groups[1].Value;
                data.PageLink = URL;
                data.PageNumber = (uint)Page+1;
                if (Page <= matches.Count)
                {
                    data.NextLink = URL;
                }
                else
                {
                    data.NextLink = "";
                    Page = 0;
                }
                if (Page == 1)
                {
                    data.PrewLink = "";
                }
                else
                {
                    data.PrewLink = URL;
                }
            }

            ReaderInfo.Add(data);
            return true;
        }
        return false;
    }
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
namespace PluginTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser();
            //search test
           /* ObservableCollection<MangaSearchData> result = new ObservableCollection<MangaSearchData>();
                if(parser.ParseSearchAsync("nar", false, 0, result).Result == true)
                {
                    foreach(MangaSearchData item in result)
                    {
                        Console.WriteLine("_____________________________");
                        Console.WriteLine("Manga Name: " + item.Name);
                        Console.WriteLine("Manga Image Url: " + item.Image);
                        Console.WriteLine("Manga Main Website: " + item.Website);
                        Console.WriteLine("Next Page With Search Results: " + item.NextPage);
                        Console.WriteLine("Previous Page With Search Results: " + item.PrevPage);
                    }
                }*/

                //ObservableCollection<MangaPageChapters> resultCh = new ObservableCollection<MangaPageChapters>();
                //ObservableCollection<MangaPageData> resultData = new ObservableCollection<MangaPageData>();
                /*if (parser.ParseSelectedPageAsync("http://mymangaonline.net/manga-info/fairy-tail.html", false, resultData, resultCh).Result == true)
                {
                    Console.WriteLine("Manga Name: " + resultData.FirstOrDefault().Name);
                    Console.WriteLine("Manga Alternate Name: " + resultData.FirstOrDefault().AlternateName);
                    Console.WriteLine("Manga Author: " + resultData.FirstOrDefault().Author);
                    Console.WriteLine("Manga Artist: " + resultData.FirstOrDefault().Artist);
                    Console.WriteLine("Manga Genre: " + resultData.FirstOrDefault().Genre);
                    Console.WriteLine("Manga Type: " + resultData.FirstOrDefault().Type);
                    Console.WriteLine("Manga Status: " + resultData.FirstOrDefault().Status);
                    Console.WriteLine("Manga Year of Release: " + resultData.FirstOrDefault().YearOfRelease);
                    Console.WriteLine("Manga Website: " + resultData.FirstOrDefault().Website);
                    Console.WriteLine("Manga Image: " + resultData.FirstOrDefault().Image);
                    Console.WriteLine("Manga Description: " + resultData.FirstOrDefault().Description);
                }*/
            /* if (parser.ParseSelectedPageAsync("http://mymangaonline.net/manga-info/fairy-tail.html", true, resultData, resultCh).Result == true)
             {
                 foreach(MangaPageChapters data in resultCh)
                 {
                     Console.WriteLine("Chapter Name: " + data.Name);
                     Console.WriteLine("Chapter RealName: " + data.RealName);
                     Console.WriteLine("Chapter Website: " + data.UrlToPage);
                 }
             }*/
            ObservableCollection<MangaImagesData> result = new ObservableCollection<MangaImagesData>();
            string nextLink = "http://mymangaonline.net/read-online/Mujang-Ch-1.html";
            while (nextLink != "")
            {
                if (parser.ParseImagesAsync(nextLink, result).Result == true)
                {
                    Console.WriteLine("Image: " + result.Last().ImageLink);
                    Console.WriteLine("Page Link: " + result.Last().PageLink);
                    Console.WriteLine("NextPage Link: " + result.Last().NextLink);
                    Console.WriteLine("PrevPage Link: " + result.Last().PrewLink);
                    Console.WriteLine("Page: " + result.Last().PageNumber + "/" + result.Last().MaxPages);
                    if(parser.Page < result.Last().MaxPages)
                        parser.Page += 1;
                    else { break; }

                    nextLink = result.Last().NextLink;
                    
                }
            }

            Console.ReadKey();
        }
    }
}

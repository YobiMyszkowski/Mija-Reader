using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions; //Regex
using System.Threading.Tasks;
using System.Windows.Media;

using System.Collections.ObjectModel;

using BaseMangaSource;

namespace MangaParser
{
    public class Parser : IPlugin
    {
        public string Name
        {
            get
            {
                return "Manga Here";
            }
        }
        public string Icon
        {
            get
            {
                return "http://www.mangahere.co/favicon32.ico";
            }
        }
        private string _Website = "http://www.mangahere.co";
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
                return 1;
            }
        }
        private int _Page = 1;
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
                return string.Format("{0}/search.php?&name={1}&page={2}",
                    Website, SearchQuote, Page);
            }
        }
        private bool _LoadImagesFromOnePage = false;
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

            StringBuilder result = new StringBuilder();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SearchLink);
                request.Credentials = CredentialCache.DefaultCredentials;
                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (System.IO.Stream responseStream = response.GetResponseStream())
                    {
                        byte[] urlContents = new byte[2048];
                        int bytesSize = 0;
                        while ((bytesSize = await responseStream.ReadAsync(urlContents, 0, urlContents.Length)) > 0)
                        {
                            result.Append(Encoding.UTF8.GetString(urlContents, 0, bytesSize));
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string error = String.Format("{0} - Error - Website: {1} - Reason: {2}", DateTime.Now.ToLongTimeString(), SearchLink, ex.Message);
                throw new Exception(error, ex);
            }

            if (result.Length > 0 && result != null)
            {
                Regex reg = new Regex("<a href=\"(?<MangaWebsite>[^\"]+)\" class=\"manga_info name_one\" rel=\"(?<MangaName>[^\"]+)\">(?<Useless>[^<]+)</a>",
                    RegexOptions.IgnoreCase);
                MatchCollection matches = reg.Matches(result.ToString());

                List<string> Covers = new List<string>();
                List<string> Names = new List<string>();
                List<string> Websites = new List<string>();

                foreach (Match match in matches)
                {
                    Websites.Add(match.Groups["MangaWebsite"].Value);//.Substring(Website.Length));
                    Names.Add(match.Groups["MangaName"].Value);

                    HttpWebRequest web = (HttpWebRequest)WebRequest.Create(String.Format("{0}/ajax/series.php", Website));
                    web.Method = "POST";
                    web.ContentType = "application/x-www-form-urlencoded";
                    String PayloadContent = String.Format("name={0}", Uri.EscapeDataString(match.Groups["MangaName"].Value));
                    Byte[] PayloadBuffer = Encoding.UTF8.GetBytes(PayloadContent.ToCharArray());
                    web.ContentLength = PayloadBuffer.Length;
                    //web.GetRequestStream().Write(PayloadBuffer, 0, PayloadBuffer.Length);
                    web.GetRequestStreamAsync().Result.Write(PayloadBuffer, 0, PayloadBuffer.Length);

                    WebResponse response = await web.GetResponseAsync();

                    System.IO.Stream dataStream = response.GetResponseStream();
                    System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);

                    string responseFromServer = reader.ReadToEnd();

                    String[] Details = responseFromServer.Replace("\\/", "/").Split(new String[] { "\",\"" }, StringSplitOptions.None);
                    Covers.Add(Details[1].Substring(0, Details[1].LastIndexOf('?')));

                    response.Close();
                }

                int NextPage = 0;
                if (IgnorePages != true)
                {
                    Match match = Regex.Match(result.ToString(), "<a href=\"(?<MangaWebsite>[^\"]+)\" class=\"next\"><i></i>Next</a>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        NextPage = Convert.ToInt32(match.Groups["MangaWebsite"].Value.Split('=').Last());
                    }
                    else
                    {
                        NextPage = 1;
                        //_ErrorMessage = "No Pages";
                    }
                }

                for (int i = 0; i < Covers.Count; i++)
                {
                    SearchResults.Add(new MangaSearchData() { Name = Names[i].ToString(), Image = Covers[i].ToString(), Website = Websites[i].ToString(), FirstPage = Page, NextPage = NextPage });
                }
                return true;
            }
            else
            {
                _ErrorMessage = "Nothing was found";
                return false;
            }
        }
        public async Task<bool> ParseSelectedPageAsync(string URL, bool ParseJustChapters, ObservableCollection<MangaPageData> DetailedInfo, ObservableCollection<MangaPageChapters> ChaptersInfo)
        {
            string UrlToPage = "";
            string ChapterName = "";
            string RealChapterName = "";

            StringBuilder result = new StringBuilder();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Credentials = CredentialCache.DefaultCredentials;
                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (System.IO.Stream responseStream = response.GetResponseStream())
                    {
                        byte[] urlContents = new byte[2048];
                        int bytesSize = 0;
                        while ((bytesSize = await responseStream.ReadAsync(urlContents, 0, urlContents.Length)) > 0)
                        {
                            result.Append(Encoding.UTF8.GetString(urlContents, 0, bytesSize));
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
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

                    Regex reg = new Regex("<img src=\"(?<MangaImage>[^\"]+)\" onerror=\"this.src='(?<MangaImageIsntExist>[^\"]+)'\" class=\"img\" />",
                    RegexOptions.IgnoreCase);
                    MatchCollection matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Image = match.Groups["MangaImage"].Value;
                        break;
                    }
                    reg = new Regex("<h1 class=\"title\"><span class=\"title_icon\"></span>(?<MangaName>[^\"]+)</h1>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Name = match.Groups["MangaName"].Value;
                        break;
                    }
                    reg = new Regex("<li><label>Alternative Name:</label>(?<MangaName>[^<]+)</li>",
                    RegexOptions.Singleline);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.AlternateName = match.Groups["MangaName"].Value;
                        break;
                    }
                    reg = new Regex("<li><label>Genre(?<Useless>[^<]+):</label>(?<MangaName>[^\"]+)</li>",
                    RegexOptions.Singleline);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Genre = match.Groups["MangaName"].Value;
                        break;
                    }
                    reg = new Regex("<li><label>Author(?<Useless>[^<]+):</label><a class=\"color_0077\" href=\"(?<Useless>[^<]+)\">(?<MangaName>[^\"]+)</a></li>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Author = match.Groups["MangaName"].Value;
                        break;
                    }
                    reg = new Regex("<li><label>Status:</label>(?<MangaName>[^<]+)</li>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        if (match.Groups["MangaName"].Value == "Ongoing")
                            data.Status = MangaStatus.Ongoing;
                        else
                            data.Status = MangaStatus.Completed;

                        break;
                    }
                    reg = new Regex("<li><label>Artist(?<Useless>[^<]+):</label><a class=\"color_0077\" href=\"(?<Useless>[^<]+)\">(?<MangaName>[^\"]+)</a></li>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Artist = match.Groups["MangaName"].Value;
                        break;
                    }
                    reg = new Regex("<p id=\"show\" style=\"display:none;\">(?<MangaName>[^\"]+)<a (?<Useless>[^<]+)>Show less</a></p>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Description = match.Groups["MangaName"].Value;
                        break;
                    }

                    DetailedInfo.Add(data);

                    return true;
                }
                else
                {
                    if (result.ToString().ToLower().Contains("has been licensed, it is not available in MangaHere.".ToLower()))
                    {
                        _ErrorMessage = "This manga has been licensed, it is not available in MangaHere.";
                        return false;
                    }
                    else
                    {
                        Regex reg = new Regex("<a class=\"color_0077\" href=\"(?<MangaChapterLink>[^<]+)\" (?<Useless>[^<]+)?>(?<MangaChapterName>[^<]+)</a>(?<Useless>[^<]+)<span class=\"mr6\">(?<MangaChapterNameFirstPart>[^<]+)?</span>(?<MangaChapterNameSecondPart>[^<]+)?</span>",
                         RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        MatchCollection matches = reg.Matches(result.ToString());
                        foreach (Match match in matches)
                        {
                            ChapterName = match.Groups["MangaChapterName"].Value.Trim();
                            UrlToPage = match.Groups["MangaChapterLink"].Value;//.Substring(website_.Length));
                            //string realname = match.Groups["MangaChapterNameFirstPart"].Value + " " + match.Groups["MangaChapterNameSecondPart"].Value;
                            RealChapterName = match.Groups["MangaChapterNameFirstPart"].Value + " " + match.Groups["MangaChapterNameSecondPart"].Value;
                            ChaptersInfo.Add(new MangaPageChapters()
                            {
                                Name = ChapterName,
                                RealName = RealChapterName,
                                UrlToPage = UrlToPage,
                                Foreground = Brushes.White
                            });
                        }
                        if (matches.Count == 0)
                        {
                            //_ErrorMessage = "There isn't any chapters in this manga.";
                            return false;
                        }

                        for (int i = 0; i < ChaptersInfo.Count; i++)
                        {
                            ChaptersInfo.Move(ChaptersInfo.Count - 1, i); //reverse list
                        }

                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
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

            StringBuilder result = new StringBuilder();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Credentials = CredentialCache.DefaultCredentials;
                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (System.IO.Stream responseStream = response.GetResponseStream())
                    {
                        byte[] downBuffer = new byte[2048];
                        int bytesSize = 0;
                        while ((bytesSize = await responseStream.ReadAsync(downBuffer, 0, downBuffer.Length)) > 0)
                        {
                            result.Append(Encoding.UTF8.GetString(downBuffer, 0, bytesSize));
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string error = String.Format("{0} - Error - Website: {1} - Reason: {2}", DateTime.Now.ToLongTimeString(), SearchLink, ex.Message);
                throw new Exception(error, ex);
            }

            if (result.Length > 0 && result != null)
            {
                Match match = Regex.Match(result.ToString(), "<img src=\"(?<ImageLink>[^\"]+)\" (?<Useless>[^<]+) />");
                if (match.Success)
                {
                    data.ImageLink = match.Groups["ImageLink"].Value;

                }

                match = Regex.Match(result.ToString(), "<select class=\"wid60\" onchange=\"(?<Useless>[^<]+)\">(.*?)</select>", RegexOptions.Multiline | RegexOptions.Singleline);
                if (match.Success)
                {
                    match = Regex.Match(match.Groups[1].Value, @"<option value=""(.*?)""(.*?)>(.*?)</option>", RegexOptions.Singleline);
                    Match nextMatch = null;
                    Match prevMatch = null;
                    while (match.Success)
                    {
                        if (match.Groups[2].Value == @" selected=""selected""" | match.Groups[2].Value == @"selected=""selected""")
                        {
                            data.PageNumber = UInt32.Parse(match.Groups[3].Value);
                            data.PageLink = match.Groups[1].Value;

                            nextMatch = match.NextMatch();
                        }

                        if (nextMatch == null)
                        {
                            prevMatch = match;
                        }

                        data.MaxPages = UInt32.Parse(match.Groups[3].Value);

                        match = match.NextMatch();
                        if (match.Index == 0)
                        {
                            break;
                        }
                    }
                    if (nextMatch.Index != 0)
                    {
                        data.NextLink = nextMatch.Groups[1].Value;
                    }
                    else
                    {
                        data.NextLink = null;
                    }
                    if (prevMatch != null)
                    {
                        if (prevMatch.Index != 0)
                        {
                            data.PrewLink = prevMatch.Groups[1].Value;
                        }
                        else
                        {
                            data.PrewLink = null;
                        }
                    }
                    else
                    {
                        data.PrewLink = null;
                    }

                    ReaderInfo.Add(data);
                    return true;
                }
                else
                {
                    //_ErrorMessage = "Nothing was found"; 
                    return false;
                }
            }
            return false;
        }
    }

}

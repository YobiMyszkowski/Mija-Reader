using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions; //Regex
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

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
                    for (int i = 0; i < ChaptersInfo.Count; i++)
                    {
                        ChaptersInfo.Move(ChaptersInfo.Count - 1, i); //reverse list
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
                    if (data.PageNumber < matches.Count)
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
}

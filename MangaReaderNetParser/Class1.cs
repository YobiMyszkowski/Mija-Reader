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
                return "Manga Reader";
            }
        }
        public string Icon
        {
            get
            {
                return "http://s4.mangareader.net/favicon.ico";
            }
        }
        private string _Website = "http://www.mangareader.net";
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
                return string.Format("{0}/search/?w={1}&p={2}",
                    Website, SearchQuote, Page);
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

            #region parse website
            if (result.Length > 0 && result != null)
            {
                Regex reg = new Regex(@"<div class=""imgsearchresults"" style=""background-image:url(.*?)"">",
                    RegexOptions.IgnoreCase);
                MatchCollection matches = reg.Matches(result.ToString());

                List<string> Covers = new List<string>();
                List<string> Names = new List<string>();
                List<string> Websites = new List<string>();

                foreach (Match match in matches)
                {
                    Covers.Add(match.Groups[1].Value.Replace("('", "").Replace("')", ""));
                }

                reg = new Regex(@"<h3><a href=""(.*?)"">(?<Useless>[^<]+)</a></h3>",
                    RegexOptions.IgnoreCase);
                matches = reg.Matches(result.ToString());
                foreach (Match match in matches)
                {
                    Websites.Add(Website + match.Groups[1].Value);
                    Names.Add(match.Groups[2].Value);
                }

                int NextPage = 0;
                int PrevPage = 0;
                if (IgnorePages != true)
                {
                    Match match = Regex.Match(result.ToString(), @"<div id=""sp"">(.*?)</div>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string mangaPageString = match.Groups[1].Value;
                        match = Regex.Match(mangaPageString, @"<a href=""(.*?)"">(.*?)</a>");
                        while (match.Success)
                        {
                            if (match.Groups[2].Value == ">" || match.Groups[2].Value == "&gt;")
                            {
                                NextPage = Convert.ToInt32(match.Groups[1].Value.Split('=').Last());
                            }
                            else if (match.Groups[2].Value == "<" || match.Groups[2].Value == "&lt;")
                            {
                                PrevPage = Convert.ToInt32(match.Groups[1].Value.Split('=').Last());
                            }

                            match = match.NextMatch();
                            if (match.Index == 0)
                                break;
                        }
                    }
                }

                for (int i = 0; i < Covers.Count; i++)
                {
                    SearchResults.Add(new MangaSearchData() { Name = Names[i].ToString(), Image = Covers[i].ToString(), Website = Websites[i].ToString(), FirstPage = Page, NextPage = NextPage, PrevPage = PrevPage });
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

                    Regex reg = new Regex(@"<h2 class=""aname"">(.*?)</h2>",
                        RegexOptions.IgnoreCase);
                    MatchCollection matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Name = match.Groups[1].Value;
                        break;
                    }

                    reg = new Regex(@"<div id=""mangaimg""><img src=""(.*?)""",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Image = match.Groups[1].Value;
                    }
                    reg = new Regex(@"<td class=""propertytitle"">(.*?)</td>\n<td>(.*?)</td>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        if (match.Groups[1].Value == "Alternate Name:")
                            data.AlternateName = match.Groups[2].Value;
                        else if (match.Groups[1].Value == "Year of Release:")
                            data.YearOfRelease = match.Groups[2].Value;
                        else if (match.Groups[1].Value == "Status:")
                        {
                            if (match.Groups[2].Value == "Ongoing")
                                data.Status = MangaStatus.Ongoing;
                            else
                                data.Status = MangaStatus.Completed;
                        }
                        else if (match.Groups[1].Value == "Author:")
                            data.Author = match.Groups[2].Value;
                        else if (match.Groups[1].Value == "Artist:")
                            data.Artist = match.Groups[2].Value;
                        else if (match.Groups[1].Value == "Reading Direction:")
                        {
                            if (match.Groups[2].Value == "Right to Left")
                                data.Type = MangaType.Manga_RightToLeft;
                            else
                                data.Type = MangaType.Manhwa_LeftToRight;
                        }
                        else if (match.Groups[1].Value == "Genre:")
                        {
                            reg = new Regex(@"<span class=""genretags"">(.*?)</span>",
                                RegexOptions.IgnoreCase);
                            matches = reg.Matches(match.Groups[2].Value);
                            StringBuilder sb = new StringBuilder();
                            foreach (Match matchGenre in matches)
                            {

                                sb.Append(matchGenre.Groups[1].Value);
                                sb.Append(", ");
                            }
                            data.Genre = sb.ToString();
                            break;
                        }


                    }
                    reg = new Regex("<div id=\"readmangasum\">(?<NotImportant>[^\"]+)<p>(?<Value>[^\"]+)</p>",
                        RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Description = match.Groups["Value"].Value;
                        break;
                    }

                    DetailedInfo.Add(data);

                    return true;
                }
                else
                {
                    Regex reg = new Regex("<a href=\"(?<Value>[^\"]+)\">(?<Text>[^<]+)</a>(?<RealName>[^<]+)</td>",
                        RegexOptions.IgnoreCase);
                    MatchCollection matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        UrlToPage = Website + match.Groups["Value"].Value;
                        ChapterName = match.Groups["Text"].Value;
                        RealChapterName = match.Groups["RealName"].Value;

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
                        // data.ErrorMessage = "There isn't any chapters in this manga.";
                        return false;
                    }
                    return true;
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
                Match match = Regex.Match(result.ToString(), @"<div id=""selectpage"">");
                if (match.Success)
                {
                    match = Regex.Match(result.ToString(), @"<img id=""img""(.*?)src=(.*?)alt", RegexOptions.Singleline);
                    if (match.Success)
                    {
                        data.ImageLink = match.Groups[2].Value.Replace(@"""", "");
                    }
                    match = Regex.Match(result.ToString(), @"<div id=""selectpage"">(.*?)</div>", RegexOptions.Singleline);
                    if (match.Success)
                    {
                        string textToParse = match.Groups[1].Value;

                        match = Regex.Match(textToParse, @"<option value=""(.*?)""(.*?)>(.*?)</option>", RegexOptions.Singleline);
                        Match nextMatch = null;
                        Match prevMatch = null;
                        while (match.Success)
                        {
                            if (match.Groups[2].Value == @" selected=""selected""" | match.Groups[2].Value == @"selected=""selected""")
                            {
                                data.PageNumber = UInt32.Parse(match.Groups[3].Value);
                                data.PageLink = Website + match.Groups[1].Value;

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
                            data.NextLink = Website + nextMatch.Groups[1].Value;
                        }
                        else
                        {
                            data.NextLink = null;
                        }
                        if (prevMatch != null)
                        {
                            if (prevMatch.Index != 0)
                            {
                                data.PrewLink = Website + prevMatch.Groups[1].Value;
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
                    }
                    ReaderInfo.Add(data);
                    return true;
                }
                else
                {
                    // data.ErrorMessage = "Nothing was found";
                    return false;
                }
            }
            return false;
        }

    }
}

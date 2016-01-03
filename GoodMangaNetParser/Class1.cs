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
                return "Good Manga";
            }
        }
        public string Icon
        {
            get
            {
                return "http://www.goodmanga.net/favicon.gif";
            }
        }
        private string _Website = "http://www.goodmanga.net";
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
                return string.Format("{0}/advanced-search?key={1}&page={2}",
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

            if (PageNumber == 0)
                _Page = 1;

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
                Regex reg = new Regex(@"<a href=""(.*?)""><img src=""(.*?)"" width=""(.*?)"" height=""(.*?)"" alt=""(.*?)""/></a>",
                           RegexOptions.IgnoreCase);
                MatchCollection matches = reg.Matches(result.ToString());

                List<string> Covers = new List<string>();
                List<string> Names = new List<string>();
                List<string> Websites = new List<string>();

                foreach (Match match in matches)
                {
                    Covers.Add(match.Groups[2].Value);
                    Websites.Add(match.Groups[1].Value);
                    Names.Add(match.Groups[5].Value.Substring("Read ".Length));
                }
                int NextPage = 1;
                int PrevPage = 0;
                if (IgnorePages != true)
                {
                    //<li><a href="http://www.goodmanga.net/advanced-search?key=ki&amp;wg=&amp;wog=&amp;status=&amp;page=2">Next</a></li>
                    Match match = Regex.Match(result.ToString(), "<li><a href=\"(?<MangaWebsite>[^\"]+)\">Next</a></li>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        NextPage = Convert.ToInt32(match.Groups["MangaWebsite"].Value.Split('=').Last());
                    }
                    else
                    {
                        NextPage = 1;
                    }
                    //<li><a href="http://www.goodmanga.net/advanced-search?key=k&amp;wg=&amp;wog=&amp;status=&amp;page=4">Prev</a></li>
                    match = Regex.Match(result.ToString(), "<li><a href=\"(?<MangaWebsite>[^\"]+)\">Prev</a></li>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        PrevPage = Convert.ToInt32(match.Groups["MangaWebsite"].Value.Split('=').Last());
                    }
                    else
                    {
                        PrevPage = 1;
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
        }
        public async Task<bool> ParseSelectedPageAsync(string URL, bool ParseJustChapters, ObservableCollection<MangaPageData> DetailedInfo, ObservableCollection<MangaPageChapters> ChaptersInfo)
        {
        up:
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

                    Regex reg = new Regex("<img src=\"(?<MangaImage>[^\"]+)\" id=\"series_image\" width=\"(?<Useless>[^<]+)\" height=\"(?<Useless>[^<]+)\" alt=\"(?<MangaName>[^\"]+)\"/>", RegexOptions.IgnoreCase);
                    MatchCollection matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Name = match.Groups["MangaName"].Value.Substring("Read ".Length);
                        data.Image = match.Groups["MangaImage"].Value;

                        break;
                    }
                    reg = new Regex("<span>Alternative Titles: </span>(?<MangaAlternativeTitles>[^<]+)</div>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.AlternateName = match.Groups["MangaAlternativeTitles"].Value.Trim();
                        break;
                    }
                    reg = new Regex("<span>Authors:</span>(?<MangaAuthor>[^<]+)</div>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Author = match.Groups["MangaAuthor"].Value.Trim();
                        data.Artist = match.Groups["MangaAuthor"].Value.Trim();
                        break;
                    }
                    reg = new Regex("<span>Synopsis:</span>(?<Useless>[^<]+)<div>(?<MangaDescription>[^<]+)</div>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Description = match.Groups["MangaDescription"].Value.Trim();
                        break;
                    }
                    reg = new Regex("<span>Status:</span>(?<MangaStatus>[^<]+)</div>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        if (match.Groups["MangaStatus"].Value.Trim() == "Ongoing")
                        {
                            data.Status = MangaStatus.Ongoing;
                            break;
                        }
                        else
                        {
                            data.Status = MangaStatus.Completed;
                            break;
                        }
                    }
                    reg = new Regex("<span>Released:</span>(?<MangaReleased>[^<]+)</div>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.YearOfRelease = match.Groups["MangaReleased"].Value.Trim();
                        break;
                    }
                    reg = new Regex("<span class=\"red_box\"><a href=\"(?<Useless>[^<]+)\">(?<MangaGenre>[^<]+)</a></span>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    StringBuilder sb = new StringBuilder();
                    foreach (Match match in matches)
                    {
                        sb.Append(match.Groups["MangaGenre"].Value.Trim());
                        sb.Append(", ");
                    }
                    data.Genre = sb.ToString();

                    DetailedInfo.Add(data);

                    return true;
                }
                else
                {
                    Regex reg = new Regex("<li>(?<Useless>[^<]+)<a href=\"(?<MangaChapterLink>[^<]+)\">(?<MangaChapterName>[^<]+)</a>(?<Useless>[^<]+)<span class=\"right_text\">(?<Useless>[^<]+)</span>(?<Useless>[^<]+)</li>",
                               RegexOptions.IgnoreCase);
                    MatchCollection matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        ChaptersInfo.Add(new MangaPageChapters()
                        {
                            Name = match.Groups["MangaChapterName"].Value.Trim(),
                            RealName = "",
                            UrlToPage = match.Groups["MangaChapterLink"].Value,
                            Foreground = Brushes.White
                        });
                    }
                    if (matches.Count == 0)
                    {
                        //data.ErrorMessage = "There isn't any chapters in this manga.";
                    }
                    //<a href="http://www.goodmanga.net/9/anima?page=2">Next</a>
                    reg = new Regex("<a href=\"(?<MangaChapterNextPage>[^<]+)\">Next</a>",
                        RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        URL = match.Groups["MangaChapterNextPage"].Value;
                        goto up;
                    }

                    for (int i = 0; i < ChaptersInfo.Count; i++)
                    {
                        ChaptersInfo.Move(ChaptersInfo.Count - 1, i); //reverse list
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
                Match match = Regex.Match(result.ToString(), "<a href=\"(?<NextPageLink>[^\"]+)\"><img src=\"(?<ImageLink>[^\"]+)\"");
                if (match.Success)
                {
                    data.ImageLink = match.Groups["ImageLink"].Value;
                }
                else
                {
                    data.ImageLink = "";
                }
                match = Regex.Match(result.ToString(), "<div id=\"asset_2\">(.*?)</div>", RegexOptions.Multiline | RegexOptions.Singleline);
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
            }
            else
            {
                // data.ErrorMessage = "Nothing was found";
                return false;
            }
            return false;
        }
    }
}

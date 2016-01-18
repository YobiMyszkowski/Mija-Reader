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
                return "Sen Manga";
            }
        }
        public string Icon
        {
            get
            {
                return "http://www.senmanga.com/wp-content/themes/Green/images/Favi.png";
            }
        }
        private string _Website = "http://www.senmanga.com";
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
                return string.Format("{0}/directory/search/{1}/name-az/{2}/",
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


            if (result.Length > 0 && result != null)
            {
                Regex reg = new Regex("<a class=\"lst mng_det_pop\" href=\"(.*?)\" title=\"(.*?)\" rel=\"(.*?)\">(?<Useless>[^<]+)<img src=\"(?<Cover>[^<]+)\" alt=\"(.*?)\"/>", RegexOptions.IgnoreCase & RegexOptions.Multiline);
                MatchCollection matches = reg.Matches(result.ToString());

                List<string> Covers = new List<string>();
                List<string> Names = new List<string>();
                List<string> Websites = new List<string>();

                foreach (Match match in matches)
                {
                    Covers.Add(string.Format("http://{0}", match.Groups["Cover"].Value.Substring("//".Length)));
                    Websites.Add(match.Groups[1].Value);
                    Names.Add(WebUtility.HtmlDecode(match.Groups[2].Value));
                }
                int NextPage = 1;
                int PrevPage = 0;
                if (IgnorePages != true)
                {
                    Match match = Regex.Match(result.ToString(), "<li><a href=\"(?<MangaWebsite>[^<]+)\">Next</a></li>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        Int32 value = 0;
                        Int32.TryParse(match.Groups["MangaWebsite"].Value.Remove(match.Groups["MangaWebsite"].Value.Length - 1).Split('/').Last(), out value);
                        NextPage = value;
                    }
                    else
                    {
                        NextPage = 1;
                    }
                    match = Regex.Match(result.ToString(), "<li><a href=\"(?<MangaWebsite>[^<]+)\">Previous</a></li>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        Int32 value = 0;
                        Int32.TryParse(match.Groups["MangaWebsite"].Value.Remove(match.Groups["MangaWebsite"].Value.Length - 1).Split('/').Last(), out value);
                        PrevPage = value;
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

                    Match match = Regex.Match(result.ToString(), "<img class=\"cvr\" src=\"(?<Image>[^<]+)\" alt=\"(?<Name>[^<]+)\"/>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        data.Image = string.Format("http://{0}", match.Groups["Image"].Value.Substring("//".Length));
                        data.Name = WebUtility.HtmlDecode(match.Groups["Name"].Value);
                    }

                    match = Regex.Match(result.ToString(), "<p><b>Altenative name</b>:(?<AlternateName>[^<]+)</p>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        data.AlternateName = WebUtility.HtmlDecode(match.Groups["AlternateName"].Value);
                    }

                    match = Regex.Match(result.ToString(), "<p><b>Status</b>: <a href=\"(?<Page>[^<]+)\">(?<Status>[^<]+)</a></p>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        if (match.Groups["Status"].Value == "Completed")
                        {
                            data.Status = MangaStatus.Completed;
                        }
                        else
                        {
                            data.Status = MangaStatus.Ongoing;
                        }
                    }

                    match = Regex.Match(result.ToString(), "<b>Author</b>:(?<Useless>[^<]+)<a href=\"(?<Page>[^<]+)\">(?<Author>[^<]+)</a> </p>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        data.Author = WebUtility.HtmlDecode(match.Groups["Author"].Value);
                    }

                    match = Regex.Match(result.ToString(), "<b>Artist</b>:(?<Useless>[^<]+)<a href=\"(?<Page>[^<]+)\">(?<Author>[^<]+)</a> </p>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        data.Artist = WebUtility.HtmlDecode(match.Groups["Artist"].Value);
                    }
                    match = Regex.Match(result.ToString(), "<b>Released in</b>: (?<ReleaseDate>[^<]+)</p>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        data.YearOfRelease = WebUtility.HtmlDecode(match.Groups["ReleaseDate"].Value);
                    }
                    match = Regex.Match(result.ToString(), "<p><b>Reading Direction</b>: (?<Type>[^<]+)</p>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        if (match.Groups["Type"].Value == "Right to left")
                        {
                            data.Type = MangaType.Manga_RightToLeft;
                        }
                        else
                        {
                            data.Type = MangaType.Manhwa_LeftToRight;
                        }
                    }

                    match = Regex.Match(result.ToString(), "<div class=\"det\">(?<Useless>[^<]+)<p>(?<Description>[^<]+)</p>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        data.Description = WebUtility.HtmlDecode(match.Groups["Description"].Value);
                    }

                    match = Regex.Match(result.ToString(), "<b>Category</b>:((.|\n)*?)</p>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        MatchCollection matches = Regex.Matches(match.Groups[1].Value, "<a href=\"(.*?)\">(.*?)</a>");
                        foreach (Match genreMatch in matches)
                        {
                            data.Genre += WebUtility.HtmlDecode(genreMatch.Groups[2].Value) + ", ";
                        }
                    }

                    DetailedInfo.Add(data);

                    return true;
                }
                else
                {
                    MatchCollection matches = Regex.Matches(result.ToString(), "<a class=\"lst\" href=\"(?<Website>[^<]+)\" title=\"(?<Name>[^<]+)\">(?<Useless>[^<]+)<b class=\"val\">(?<RealName>[^<]+)</b>", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            ChaptersInfo.Add(new MangaPageChapters()
                            {
                                Name = match.Groups["Name"].Value,
                                RealName = match.Groups["RealName"].Value,
                                UrlToPage = match.Groups["Website"].Value,
                                Foreground = Brushes.White
                            });
                        }
                    }
                    else
                    {
                        matches = Regex.Matches(result.ToString(), "<a class=\"lst\" href=\"(?<Website>[^<]+)\" title=\"(?<Name>[^<]+)\">", RegexOptions.IgnoreCase);
                        foreach (Match match in matches)
                        {
                            ChaptersInfo.Add(new MangaPageChapters()
                            {
                                Name = match.Groups["Name"].Value,
                                RealName = "",
                                UrlToPage = match.Groups["Website"].Value,
                                Foreground = Brushes.White
                            });
                        }
                    }
                    if (matches.Count == 0)
                    {
                        //data.ErrorMessage = "There isn't any chapters in this manga.";
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
                Match match = Regex.Match(result.ToString(), "<img src=\"(?<CurrentImage>[^<]+)\" id=\"picture\" (?<Usseles>[^<]+)/></a>", RegexOptions.Singleline);
                if (match.Success)
                {
                    data.ImageLink = match.Groups["CurrentImage"].Value;
                }

                match = Regex.Match(result.ToString(), "<select name=\"page\" onchange=\"(.*?)\">((.|\n)*?)</select>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string textToParse = match.Groups[2].Value;
                    string chapterName = match.Groups[1].Value.Split('\'').ElementAt(1);
                    string chapterNumber = match.Groups[1].Value.Split('\'').ElementAt(3);

                    MatchCollection matches = Regex.Matches(textToParse, "<option value=\'(.*?)\'(?<Selected>.*?)>(.*?)</option>", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                    {
                        data.MaxPages = (UInt32)matches.Count;
                    }
                    match = Regex.Match(textToParse, "<option value=\'(?<PageNumber>[^\"]+)\' selected=\'selected\'>(.*?)</option>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        Int32 value = 0;
                        Int32.TryParse(match.Groups[1].Value, out value);
                        data.PageNumber = (uint)value;
                        data.PageLink = Website + "/" + chapterName + "/" + chapterNumber + "/" + data.PageNumber;
                    }
                    match = Regex.Match(result, "<a href=\"(?<PrevLink>[^\"]+)\"><span>Previous Page</span>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        data.PrewLink = match.Groups["PrevLink"].Value;
                    }
                    match = Regex.Match(result, "<a href=\"(?<NextLink>[^\"]+)\"><span>Next Page</span>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        data.NextLink = match.Groups["NextLink"].Value;
                    }
                }
                ReaderInfo.Add(data);
                return true;
            }
            else
            {
                data.ErrorMessage = "Nothing was found";
                return false;
            };
        }
    }
}

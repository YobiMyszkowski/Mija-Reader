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
                Regex reg = new Regex("<a href=\"(?<MangaWebsite>[^\"]+)\" class=\"manga_info name_one\" rel=\"(?<MangaName>[^\"]+)\">(?<Useless>[^<]+)</a>",
                    RegexOptions.IgnoreCase);
                MatchCollection matches = reg.Matches(result.ToString());

                string MangaCover = "";
                string MangaName = "";
                string MangaWebsite = "";

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
                    }
                }

                foreach (Match match in matches)
                {
                    MangaWebsite = match.Groups["MangaWebsite"].Value;
                    MangaName = match.Groups["MangaName"].Value;

                    WebRequest request = WebRequest.Create(String.Format("{0}/ajax/series.php", Website));
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Method = "POST";
                    String PayloadContent = String.Format("name={0}", Uri.EscapeDataString(match.Groups["MangaName"].Value));
                    Byte[] PayloadBuffer = Encoding.UTF8.GetBytes(PayloadContent.ToCharArray());
                    request.ContentLength = PayloadBuffer.Length;
                    System.IO.Stream stream = await request.GetRequestStreamAsync();
                    await stream.WriteAsync(PayloadBuffer, 0, PayloadBuffer.Length);
                    using (WebResponse response = await request.GetResponseAsync())
                    {
                        using (System.IO.Stream responseStream = response.GetResponseStream())
                        {
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(responseStream, Encoding.UTF8, true))
                            {
                                string responseFromServer = sr.ReadToEnd();
                                String[] Details = responseFromServer.Replace("\\/", "/").Split(new String[] { "\",\"" }, StringSplitOptions.None);

                                MangaCover = Details[1].Substring(0, Details[1].LastIndexOf('?'));

                                SearchResults.Add(new MangaSearchData() { Name = MangaName, Image = MangaCover, Website = MangaWebsite, FirstPage = Page, NextPage = NextPage });
                            }
                        }
                    }
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
                string error = String.Format("{0} - Error - Website: {1} - Reason: {2}", DateTime.Now.ToLongTimeString(), URL, ex.Message);
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
                        data.Name = WebUtility.HtmlDecode(match.Groups["MangaName"].Value);
                        break;
                    }
                    reg = new Regex("<li><label>Alternative Name:</label>(?<MangaName>[^<]+)</li>",
                    RegexOptions.Singleline);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.AlternateName = WebUtility.HtmlDecode(match.Groups["MangaName"].Value);
                        break;
                    }
                    reg = new Regex("<li><label>Genre(?<Useless>[^<]+):</label>(?<MangaName>[^\"]+)</li>",
                    RegexOptions.Singleline);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Genre = WebUtility.HtmlDecode(match.Groups["MangaName"].Value);
                        break;
                    }
                    reg = new Regex("<li><label>Author(?<Useless>[^<]+):</label><a class=\"color_0077\" href=\"(?<Useless>[^<]+)\">(?<MangaName>[^\"]+)</a></li>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Author = WebUtility.HtmlDecode(match.Groups["MangaName"].Value);
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
                        data.Artist = WebUtility.HtmlDecode(match.Groups["MangaName"].Value);
                        break;
                    }
                    reg = new Regex("<p id=\"show\" style=\"display:none;\">(?<MangaName>[^\"]+)<a (?<Useless>[^<]+)>Show less</a></p>",
                    RegexOptions.IgnoreCase);
                    matches = reg.Matches(result.ToString());
                    foreach (Match match in matches)
                    {
                        data.Description = WebUtility.HtmlDecode(match.Groups["MangaName"].Value);
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
                            ChapterName = WebUtility.HtmlDecode(match.Groups["MangaChapterName"].Value.Trim());
                            UrlToPage = match.Groups["MangaChapterLink"].Value;
                            RealChapterName = WebUtility.HtmlDecode(match.Groups["MangaChapterNameFirstPart"].Value + " " + match.Groups["MangaChapterNameSecondPart"].Value);
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
                            _ErrorMessage = "There isn't any chapters in this manga.";
                            return false;
                        }

                        for (int i = 0; i < ChaptersInfo.Count; i++)
                        {
                            ChaptersInfo.Move(ChaptersInfo.Count - 1, i); //reverse list, since it start from end to beginning
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

            string result = "";
            try
            {
                if (URL != "")
                {
                    config.HttpDownloader request = new config.HttpDownloader(URL, "", "");
                    result = await request.GetPageAsync();
                }
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
                throw new Exception(ex.Message, ex);
            }

            if (result.Length > 0 && result != null)
            {
                Match match = Regex.Match(result, "<img src=\"(?<ImageLink>[^\"]+)\" (?<Useless>[^<]+) />");
                if (match.Success)
                {
                    data.ImageLink = match.Groups["ImageLink"].Value;
                }
                match = Regex.Match(result, @"<select class=""(.*?)"" onchange=""(.*?)"">(?<Content>.*?)</select>", RegexOptions.Singleline);
                if (match.Success)
                {
                    string textToParse = match.Groups["Content"].Value;

                    MatchCollection matches = Regex.Matches(textToParse, @"<option value=""(.*?)""(?<Selected>.*?)>(.*?)</option>", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                    {
                        data.MaxPages = (UInt32)matches.Count;
                    }
                    match = Regex.Match(textToParse, "<option value=\"(?<Url>[^\"]+)\" selected=\"selected\">(.*?)</option>", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        data.PageNumber = UInt32.Parse(match.Groups[1].Value);
                        data.PageLink = match.Groups["Url"].Value;
                    }

                    match = Regex.Match(result, "<a href=\"(?<PrevLink>[^\"]+)\" class=\"prew_page\">", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        if (!match.Groups["PrevLink"].Value.Contains("javascript:void(0)"))
                        {
                            data.PrewLink = match.Groups["PrevLink"].Value;
                        }
                        else
                        {
                            data.PrewLink = null;
                        }
                    }

                    match = Regex.Match(result, "<a href=\"(?<NextLink>[^\"]+)\" class=\"next_page\">", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        if (!match.Groups["NextLink"].Value.Contains("javascript:void(0)"))
                        {
                            data.NextLink = match.Groups["NextLink"].Value;
                        }
                        else
                        {
                            data.NextLink = null;
                        }
                    }

                    ReaderInfo.Add(data);
                    return true;
                }
                else
                {
                    _ErrorMessage = "Nothing was found";
                    return false;
                }

            }
            return false;
        }
    }
}

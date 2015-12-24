using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reflection; //Assembly
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using DropNet;

using Mija_Reader.AdditionalControls;

namespace System.Windows.Controls
{
    public static class MyExt2
    {
        public static void AppendText(this RichTextBox box, string text, string color)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString(color));
            }
            catch (FormatException) { }
        }
        public static void PerformClick(this Button btn)
        {
            btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
    }
}
namespace Mija_Reader
{
    public partial class MainWindow : Window
    {
        private const string _apiKey = "1d17vae78l691bl";
        private const string _appsecret = "8gn5q15w1fy3gpm";
        DropNetClient _client = null;
        Core.Ini MyIni = null;
        Core.XMLLibrary library = new Core.XMLLibrary(@"Data\MangaLibrary.xml");
        BaseMangaSource.IPlugin parser;
        MetroMessageBox mbox = new MetroMessageBox();
        private ObservableCollection<BaseMangaSource.MangaPageData> _ReadingData = new ObservableCollection<BaseMangaSource.MangaPageData>();
        public ObservableCollection<BaseMangaSource.MangaPageData> ReadingData
        {
            get { return _ReadingData; }
            set { _ReadingData = value; }
        }
        private ObservableCollection<BaseMangaSource.MangaPageData> _FinishedData = new ObservableCollection<BaseMangaSource.MangaPageData>();
        public ObservableCollection<BaseMangaSource.MangaPageData> FinishedData
        {
            get { return _FinishedData; }
            set { _FinishedData = value; }
        }
        private ObservableCollection<BaseMangaSource.MangaPageData> _AbandonedData = new ObservableCollection<BaseMangaSource.MangaPageData>();
        public ObservableCollection<BaseMangaSource.MangaPageData> AbandonedData
        {
            get { return _AbandonedData; }
            set { _AbandonedData = value; }
        }
        private ObservableCollection<dynamic> _Languages = new ObservableCollection<dynamic>();
        public ObservableCollection<dynamic> Languages
        {
            get { return _Languages; }
            set { _Languages = value; }
        }
        ObservableCollection<BaseMangaSource.MangaPageData> _DetailedInfo = new ObservableCollection<BaseMangaSource.MangaPageData>();
        public ObservableCollection<BaseMangaSource.MangaPageData> DetailedInfo
        {
            get { return _DetailedInfo; }
            set { _DetailedInfo = value; }
        }
        ObservableCollection<BaseMangaSource.MangaPageChapters> _ChaptersInfo = new ObservableCollection<BaseMangaSource.MangaPageChapters>();
        public ObservableCollection<BaseMangaSource.MangaPageChapters> ChaptersInfo
        {
            get { return _ChaptersInfo; }
            set { _ChaptersInfo = value; }
        }
        private ObservableCollection<BaseMangaSource.IPlugin> _SearchPluginData = new ObservableCollection<BaseMangaSource.IPlugin>();
        public ObservableCollection<BaseMangaSource.IPlugin> SearchPluginData
        {
            get { return _SearchPluginData; }
            set { _SearchPluginData = value; }
        }
        private ObservableCollection<BaseMangaSource.MangaSearchData> _SearchResultsData = new ObservableCollection<BaseMangaSource.MangaSearchData>();
        public ObservableCollection<BaseMangaSource.MangaSearchData> SearchResultsData
        {
            get { return _SearchResultsData; }
            set { _SearchResultsData = value; }
        }
        private Core.BaseLanguage _SelectedLanguage = new Core.BaseLanguage();
        public Core.BaseLanguage SelectedLanguage
        {
            get { return _SelectedLanguage; }
            set { _SelectedLanguage = value; }
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SourceInitialized += MainWindow_SourceInitialized;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            #region ini settings
            MyIni = new Core.Ini("Settings.ini");
            #endregion
            #region load_Languages
            if (System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory().ToString() + @"\Data\Languages\") == true)
            {
                string[] filePaths = System.IO.Directory.GetFiles(Environment.CurrentDirectory.ToString() + @"\Data\Languages\", "*Language.dll");

                if (filePaths.Count() == 0)
                {
                    c_LanguageCB.IsEnabled = false;
                }
                else
                {
                    for (int i = 0; i < filePaths.Count(); i++)
                    {
                        try
                        {
                            var DLL = Assembly.LoadFile(filePaths[i]);

                            foreach (Type type in DLL.GetExportedTypes())
                            {
                                dynamic c = Activator.CreateInstance(type);
                                Languages.Add(c);

                                if (MyIni.KeyExists("Language", "WindowData"))
                                {
                                    if (MyIni.Read("Language", "WindowData") == c.LanguageName)
                                    {
                                        c_LanguageCB.SelectedIndex = i;
                                    }
                                }
                                else
                                {
                                    c_LanguageCB.SelectedIndex = 0; // select english language after first run
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                        c_LanguageCB.IsEnabled = true;
                    }
                }
            }
            else
            {
                c_LanguageCB.IsEnabled = false;
            }
            #endregion

            #region load_Manga Sources
            if (System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory().ToString() + @"\Data\MangaSources\") == true)
            {
                string[] filePaths = System.IO.Directory.GetFiles(Environment.CurrentDirectory.ToString() + @"\Data\MangaSources\", "*Parser.dll");

                if (filePaths.Count() == 0)
                {
                    c_SearchCB.IsEnabled = false;
                }
                else
                {
                    for (int i = 0; i < filePaths.Count(); i++)
                    {
                        try
                        {
                            int pos = filePaths.ToList<string>()[i].LastIndexOf(@"\") + 1;                
                            SearchPluginData.Add(LoadPluginByWebsite(filePaths.ToList<string>()[i].Substring(pos)));

                            if (MyIni.KeyExists("MangaSource", "WindowData"))
                            {
                                if (MyIni.Read("MangaSource", "WindowData") == SearchPluginData.ElementAt(i).Website)
                                {
                                    parser = SearchPluginData.ElementAt(i);
                                    c_SearchCB.SelectedIndex = i;
                                    c_SearchCB.ToolTip = parser.Website + ", " + parser.Lang + ", " + parser.Author;
                                }
                            }
                            else
                            {
                                c_SearchCB.SelectedIndex = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                        c_SearchCB.IsEnabled = true;
                    }
                }
            }
            else
            {
                c_SearchCB.IsEnabled = false;
            }
            #endregion

            #region load_Library
            if (!library.IsFileExist())
            {
                if (!library.IsFolderExist())
                {
                    library.CreateFolder();
                }
                library.Create();
                library.Load();
                library.LoadLibrary(ReadingData, FinishedData, AbandonedData);
            }
            else
            {
                library.Load();
                library.LoadLibrary(ReadingData, FinishedData, AbandonedData);
            }
            #endregion

            #region InitializeDropBox
            if (MyIni.KeyExists("accessToken", "DropBox") && MyIni.KeyExists("accessSecret", "DropBox"))
            {
                (c_MainTabTC.Items[0] as TabItem).IsEnabled = false;
                for (int i = 1; i < c_MainTabTC.Items.Count; i++)
                {
                    (c_MainTabTC.Items[i] as TabItem).IsEnabled = true;
                }
                c_MainTabTC.SelectedIndex = 1;

                try
                {
                    _client = new DropNetClient(_apiKey, _appsecret);

                    _client.UserLogin = new DropNet.Models.UserLogin { Token = MyIni.Read("accessToken", "DropBox"), Secret = MyIni.Read("accessSecret", "DropBox") };
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                (c_MainTabTC.Items[0] as TabItem).IsEnabled = true;
                for (int i = 1; i < c_MainTabTC.Items.Count; i++)
                {
                    (c_MainTabTC.Items[i] as TabItem).IsEnabled = false;
                }
                c_MainTabTC.SelectedIndex = 0;

                try
                {
                    _client = new DropNetClient(_apiKey, _appsecret);
                    // Sync
                    _client.GetToken();

                    var url = _client.BuildAuthorizeUrl();

                    loginBrowser.Navigate(url);
                    loginBrowser.LoadCompleted += Browser_LoadCompleted;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            #endregion
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
        private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            dynamic doc = (sender as WebBrowser).Document;
            var htmlText = doc.documentElement.OuterText;

            string browserContents = doc.Body.InnerText;

            if (browserContents.Contains(SelectedLanguage.WebBrowserSucces))
            {
                _client.GetAccessTokenAsync((accessToken) =>
                {
                    if (accessToken != null)
                    {
                        //Store this token for "remember me" function
                        MyIni.Write("accessToken", accessToken.Token, "DropBox");
                        MyIni.Write("accessSecret", accessToken.Secret, "DropBox");

                        (c_MainTabTC.Items[0] as TabItem).IsEnabled = false;
                        for (int i = 1; i < c_MainTabTC.Items.Count; i++)
                        {
                            (c_MainTabTC.Items[i] as TabItem).IsEnabled = true;
                        }
                        c_MainTabTC.SelectedIndex = 1;
                    }
                },
                (error) =>
                {
                    throw new Exception(error.Message);
                });
            }
        }

        private BaseMangaSource.IPlugin LoadPluginByWebsite(string website)
        {
            Type ObjType = null;
            try
            {
                Assembly ass = null;
                ass = Assembly.LoadFrom(Environment.CurrentDirectory.ToString() + @"\Data\MangaSources\" + website);
                if (ass != null)
                {
                    ObjType = ass.GetType(@"MangaParser" + ".Parser");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            try
            {
                if (ObjType != null)
                {
                    parser = (BaseMangaSource.IPlugin)Activator.CreateInstance(ObjType);
                    c_SearchCB.ToolTip = parser.Website + ", " + parser.Lang + ", " + parser.Author;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return parser;
        }
        private void c_LanguageCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic c = Languages.ElementAt((sender as ComboBox).SelectedIndex);
            SelectedLanguage.LanguageName = c.LanguageName;
            SelectedLanguage.AuthorName = c.AuthorName;
            SelectedLanguage.LanguageString = c.LanguageString;
            SelectedLanguage.Reading = c.Reading;
            SelectedLanguage.Finished = c.Finished;
            SelectedLanguage.Abandoned = c.Abandoned;
            SelectedLanguage.WebBrowserSucces = c.WebBrowserSucces;
            SelectedLanguage.Login = c.Login;
            SelectedLanguage.Home = c.Home;
            SelectedLanguage.Library = c.Library;
            SelectedLanguage.Chapters = c.Chapters;
            SelectedLanguage.Reader = c.Reader;
            SelectedLanguage.Settings = c.Settings;
            SelectedLanguage.LastRead = c.LastRead;
            SelectedLanguage.NewChapters = c.NewChapters;
            SelectedLanguage.Search = c.Search;

            SelectedLanguage.Information = c.Information;
            SelectedLanguage.Warning = c.Warning;
            SelectedLanguage.Error = c.Error;
            SelectedLanguage.Yes = c.Yes;
            SelectedLanguage.No = c.No;
            SelectedLanguage.Cancel = c.Cancel;
            SelectedLanguage.Ok = c.Ok;
            SelectedLanguage.MangaAlreadyExist = c.MangaAlreadyExist;
            SelectedLanguage.SomethingWentWrong = c.SomethingWentWrong;
            SelectedLanguage.NoSelectedSource = c.NoSelectedSource;


            MyIni.Write("Language", SelectedLanguage.LanguageName, "WindowData");
        }

        private void c_SearchCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            parser = SearchPluginData.ElementAt((sender as ComboBox).SelectedIndex);
            c_SearchCB.ToolTip = parser.Website + ", " + parser.Lang + ", " + parser.Author;

            MyIni.Write("MangaSource", parser.Website, "WindowData");
        }

        private async void SearchTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) // if 'Enter' is pressed
            {
                SearchResultsData.Clear();

                if (c_SearchCB.SelectedItem != null)
                {
                    try
                    {
                        if (parser != null)
                        {
                            var result = await parser.ParseSearchAsync(tvSearchText.Text.ToString(), false, 0, SearchResultsData);
                            if (result == true)
                            {
                                if (SearchResultsData.Count > 0)
                                {
                                    if (SearchResultsData.FirstOrDefault().NextPage > SearchResultsData.FirstOrDefault().FirstPage)
                                    {
                                        tvSearchNext.ToolTip = SearchResultsData.FirstOrDefault().NextPage;
                                        tvSearchNext.IsEnabled = true;
                                    }
                                    else
                                    {
                                        tvSearchNext.ToolTip = "";
                                        tvSearchNext.IsEnabled = false;
                                    }
                                    if (SearchResultsData.FirstOrDefault().PrevPage < SearchResultsData.FirstOrDefault().NextPage)
                                    {
                                        tvSearchPrev.ToolTip = SearchResultsData.FirstOrDefault().PrevPage;
                                        tvSearchPrev.IsEnabled = true;
                                    }
                                    else
                                    {
                                        tvSearchPrev.ToolTip = "";
                                        tvSearchPrev.IsEnabled = false;
                                    }
                                }
                            }
                            else
                            {
                                MetroMessageBox mbox = new MetroMessageBox();
                                mbox.MessageBoxBtnYes.Click += (s, en) => { mbox.Close(); };
                                mbox.ShowMessage(this, SelectedLanguage.SomethingWentWrong, SelectedLanguage.Error, MessageBoxMessage.information, MessageBoxButton.OK);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    MetroMessageBox mbox = new MetroMessageBox();
                    mbox.MessageBoxBtnYes.Click += (s, en) => { mbox.Close(); };
                    mbox.ShowMessage(this, SelectedLanguage.NoSelectedSource, SelectedLanguage.Error, MessageBoxMessage.information, MessageBoxButton.OK);
                }
            }
        }

        private async void tvSearchPrev_Click(object sender, RoutedEventArgs e)
        {
            SearchResultsData.Clear();

            if (c_SearchCB.SelectedItem != null)
            {
                try
                {
                    if (parser != null)
                    {

                        var result = await parser.ParseSearchAsync(tvSearchText.Text.ToString(), false, Int32.Parse(tvSearchPrev.ToolTip.ToString()), SearchResultsData);

                        if (result == true)
                        {
                            if (SearchResultsData.Count > 0)
                            {
                                if (SearchResultsData.FirstOrDefault().NextPage > SearchResultsData.FirstOrDefault().FirstPage)
                                {
                                    tvSearchNext.ToolTip = SearchResultsData.FirstOrDefault().NextPage;

                                    tvSearchNext.IsEnabled = true;
                                }
                                else
                                {
                                    tvSearchNext.ToolTip = "";

                                    tvSearchNext.IsEnabled = false;
                                }
                                if (SearchResultsData.FirstOrDefault().PrevPage < SearchResultsData.FirstOrDefault().NextPage)
                                {
                                    tvSearchPrev.ToolTip = SearchResultsData.FirstOrDefault().PrevPage;
                                    tvSearchPrev.IsEnabled = true;
                                }
                                else
                                {
                                    tvSearchPrev.ToolTip = "";
                                    tvSearchPrev.IsEnabled = false;
                                }
                            }
                            else
                            {
                                tvSearchNext.ToolTip = parser.FirstPage;

                                result = await parser.ParseSearchAsync(tvSearchText.Text.ToString(), false, Int32.Parse(tvSearchPrev.ToolTip.ToString()), SearchResultsData);
                                if (result == true)
                                {
                                    if (SearchResultsData.Count > 0)
                                    {
                                        if (SearchResultsData.FirstOrDefault().NextPage > SearchResultsData.FirstOrDefault().FirstPage)
                                        {
                                            tvSearchNext.ToolTip = SearchResultsData.FirstOrDefault().NextPage;

                                            tvSearchNext.IsEnabled = true;
                                        }
                                        else
                                        {
                                            tvSearchNext.ToolTip = "";

                                            tvSearchNext.IsEnabled = false;
                                        }
                                        if (SearchResultsData.FirstOrDefault().PrevPage < SearchResultsData.FirstOrDefault().NextPage)
                                        {
                                            tvSearchPrev.ToolTip = SearchResultsData.FirstOrDefault().PrevPage;
                                            tvSearchPrev.IsEnabled = true;
                                        }
                                        else
                                        {
                                            tvSearchPrev.ToolTip = "";
                                            tvSearchPrev.IsEnabled = false;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            MetroMessageBox mbox = new MetroMessageBox();
                            mbox.MessageBoxBtnYes.Click += (s, en) => { mbox.Close(); };
                            mbox.ShowMessage(this, SelectedLanguage.SomethingWentWrong, SelectedLanguage.Error, MessageBoxMessage.information, MessageBoxButton.OK);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

        }

        private async void tvSearchNext_Click(object sender, RoutedEventArgs e)
        {
            SearchResultsData.Clear();

            if (c_SearchCB.SelectedItem != null)
            {
                try
                {
                    if (parser != null)
                    {

                        var result = await parser.ParseSearchAsync(tvSearchText.Text.ToString(), false, Int32.Parse(tvSearchNext.ToolTip.ToString()), SearchResultsData);

                        if (result == true)
                        {
                            if (SearchResultsData.Count > 0)
                            {
                                if (SearchResultsData.FirstOrDefault().NextPage > SearchResultsData.FirstOrDefault().FirstPage)
                                {
                                    tvSearchNext.ToolTip = SearchResultsData.FirstOrDefault().NextPage;

                                    tvSearchNext.IsEnabled = true;
                                }
                                else
                                {
                                    tvSearchNext.ToolTip = "";

                                    tvSearchNext.IsEnabled = false;
                                }
                                if (SearchResultsData.FirstOrDefault().PrevPage < SearchResultsData.FirstOrDefault().NextPage)
                                {
                                    tvSearchPrev.ToolTip = SearchResultsData.FirstOrDefault().PrevPage;
                                    tvSearchPrev.IsEnabled = true;
                                }
                                else
                                {
                                    tvSearchPrev.ToolTip = "";
                                    tvSearchPrev.IsEnabled = false;
                                }
                            }
                            else
                            {
                                tvSearchNext.ToolTip = parser.FirstPage;

                                result = await parser.ParseSearchAsync(tvSearchText.Text.ToString(), false, Int32.Parse(tvSearchNext.ToolTip.ToString()), SearchResultsData);
                                if (result == true)
                                {
                                    if (SearchResultsData.Count > 0)
                                    {
                                        if (SearchResultsData.FirstOrDefault().NextPage > SearchResultsData.FirstOrDefault().FirstPage)
                                        {
                                            tvSearchNext.ToolTip = SearchResultsData.FirstOrDefault().NextPage;

                                            tvSearchNext.IsEnabled = true;
                                        }
                                        else
                                        {
                                            tvSearchNext.ToolTip = "";

                                            tvSearchNext.IsEnabled = false;
                                        }
                                        if (SearchResultsData.FirstOrDefault().PrevPage < SearchResultsData.FirstOrDefault().NextPage)
                                        {
                                            tvSearchPrev.ToolTip = SearchResultsData.FirstOrDefault().PrevPage;
                                            tvSearchPrev.IsEnabled = true;
                                        }
                                        else
                                        {
                                            tvSearchPrev.ToolTip = "";
                                            tvSearchPrev.IsEnabled = false;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            MetroMessageBox mbox = new MetroMessageBox();
                            mbox.MessageBoxBtnYes.Click += (s, en) => { mbox.Close(); };
                            mbox.ShowMessage(this, SelectedLanguage.SomethingWentWrong, SelectedLanguage.Error, MessageBoxMessage.information, MessageBoxButton.OK);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        MangaDetailsViewer details;
        private async void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if((sender as ListView).Items.Count > 0)
            {
                if ((sender as ListView).SelectedItem != null)
                {
                    details = new MangaDetailsViewer();
                    details.Owner = this;
                    details.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    BaseMangaSource.MangaSearchData selectedManga = SearchResultsData.ElementAt((sender as ListView).SelectedIndex);
                    if (selectedManga != null)
                    {
                        DetailedInfo.Clear();

                        var result = await parser.ParseSelectedPageAsync(selectedManga.Website, false, DetailedInfo, ChaptersInfo);
                        if (result == true)
                        {
                            details.tvDetails.Document.Blocks.Clear();
                            details.tvDescription.Document.Blocks.Clear();

                            details.tvImage.Source = new BitmapImage(new Uri(DetailedInfo.FirstOrDefault().Image));

                            details.tvDescription.AppendText(DetailedInfo.FirstOrDefault().Description, "Gray");

                            details.tvDetails.AppendText("Name" + ": ", "LightBlue");
                            details.tvDetails.AppendText(DetailedInfo.FirstOrDefault().Name, "Gray");
                            details.tvDetails.AppendText("\r");
                            details.tvDetails.AppendText("AlternateName" + ": ", "LightBlue");
                            details.tvDetails.AppendText(DetailedInfo.FirstOrDefault().AlternateName, "Gray");
                            details.tvDetails.AppendText("\r");
                            details.tvDetails.AppendText("YearOfRelease" + ": ", "LightBlue");
                            details.tvDetails.AppendText(DetailedInfo.FirstOrDefault().YearOfRelease, "Gray");
                            details.tvDetails.AppendText("\r");
                            details.tvDetails.AppendText("Status" + ": ", "LightBlue");
                            if (DetailedInfo.FirstOrDefault().Status == BaseMangaSource.MangaStatus.Completed)
                                details.tvDetails.AppendText("Completed", "Gray");
                            else
                                details.tvDetails.AppendText("Ongoing", "Gray");
                            details.tvDetails.AppendText("\r");
                            details.tvDetails.AppendText("Author" + ": ", "LightBlue");
                            details.tvDetails.AppendText(DetailedInfo.FirstOrDefault().Author, "Gray");
                            details.tvDetails.AppendText("\r");
                            details.tvDetails.AppendText("Artist" + ": ", "LightBlue");
                            details.tvDetails.AppendText(DetailedInfo.FirstOrDefault().Artist, "Gray");
                            details.tvDetails.AppendText("\r");
                            details.tvDetails.AppendText("Directin" + ": ", "LightBlue");
                            if (DetailedInfo.FirstOrDefault().Type == BaseMangaSource.MangaType.Manga_RightToLeft)
                                details.tvDetails.AppendText("manga", "Gray");
                            else
                                details.tvDetails.AppendText("manhwa", "Gray");
                            details.tvDetails.AppendText("\r");
                            details.tvDetails.AppendText("Genre" + ": ", "LightBlue");
                            details.tvDetails.AppendText(DetailedInfo.FirstOrDefault().Genre, "Gray");
                            details.tvDetails.AppendText("\r");
                        }
                        else
                        {
                            return;
                        }
                    }
                    details.tvAddToLibrary.Click += TvAddToLibrary_Click;
                    details.ShowDialog();
                }
            }
        }
        private async void TvAddToLibrary_Click(object sender, RoutedEventArgs e)
        {
            bool foundMatch = false;
            string foundIn = "";
            foreach (TabItem libraryChild in c_LibraryTabTC.Items)
            {
                ObservableCollection<BaseMangaSource.MangaPageData> data = ((libraryChild.Content as ListView).ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
                for(int i = 0; i < data.Count; i++)
                {
                    if(data.ElementAt(i).Name == SearchResultsData.ElementAt(tvSearchResults.SelectedIndex).Name && data.ElementAt(i).Website == SearchResultsData.ElementAt(tvSearchResults.SelectedIndex).Website)
                    {
                        foundMatch = true;
                        foundIn = libraryChild.Header.ToString();
                        break;
                    }
                }
            }
            if (foundMatch == true)
            {
                MetroMessageBox mbox = new MetroMessageBox();
                mbox.MessageBoxBtnYes.Click += (s, en) => { mbox.Close(); };
                mbox.ShowMessage(this, string.Format(SelectedLanguage.MangaAlreadyExist, SearchResultsData.ElementAt(tvSearchResults.SelectedIndex).Name, foundIn ), SelectedLanguage.Information, MessageBoxMessage.information, MessageBoxButton.OK);
            }
            else
            {
                DetailedInfo.Clear();
                bool x = await parser.ParseSelectedPageAsync(SearchResultsData.ElementAt(tvSearchResults.SelectedIndex).Website, false, DetailedInfo, null);

                if (ChaptersInfo != null)
                {
                    ReadingData.Add(DetailedInfo.FirstOrDefault());
                    details.Close();
                    c_MainTabTC.SelectedIndex = 2;
                    c_LibraryTabTC.SelectedIndex = 0;

                    library.AddManga(DetailedInfo, Core.PlaceInLibrary.Reading);
                }
            }
        }
    }
}
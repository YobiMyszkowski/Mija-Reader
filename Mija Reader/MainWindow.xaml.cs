using System;
using System.Linq;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System.Reflection; //Assembly
using DropNet;

using Mija_Reader.AdditionalControls;

namespace Mija_Reader
{
    public partial class MainWindow : Window
    {
        private const string _apiKey = "1d17vae78l691bl";
        private const string _appsecret = "8gn5q15w1fy3gpm";
        DropNetClient _client = null;
        Core.Ini MyIni = null;
        BaseMangaSource.IPlugin parser;
        MetroMessageBox mbox = new MetroMessageBox();
        private ObservableCollection<dynamic> _Languages = new ObservableCollection<dynamic>();
        public ObservableCollection<dynamic> Languages
        {
            get { return _Languages; }
            set { _Languages = value; }
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
                    //Handle error
                    MessageBox.Show(error.Message);
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
                                mbox.ShowMessage(this, "Something went wrong while searching", "Error", MessageBoxMessage.information, MessageBoxButton.OK);
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
                    mbox.ShowMessage(this, "There isn't any manga source selected", "Error", MessageBoxMessage.information, MessageBoxButton.OK);
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
                            mbox.ShowMessage(this, "Something went wrong while searching", "Error", MessageBoxMessage.information, MessageBoxButton.OK);
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
                            mbox.ShowMessage(this, "Something went wrong while searching", "Error", MessageBoxMessage.information, MessageBoxButton.OK);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
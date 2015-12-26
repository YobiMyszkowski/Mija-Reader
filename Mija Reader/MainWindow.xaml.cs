﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reflection; //Assembly
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows.Input;
using DropNet;
using System.Collections.Generic;
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
        public static IEnumerable<DependencyObject> GetChildObjects(
                                   this DependencyObject parent)
        {
            if (parent == null) yield break;


            if (parent is ContentElement || parent is FrameworkElement)
            {
                //use the logical tree for content / framework elements
                foreach (object obj in LogicalTreeHelper.GetChildren(parent))
                {
                    var depObj = obj as DependencyObject;
                    if (depObj != null) yield return (DependencyObject)obj;
                }
            }
            else
            {
                //use the visual tree per default
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    yield return VisualTreeHelper.GetChild(parent, i);
                }
            }
        }
        public static IEnumerable<T> FindChildren<T>(this DependencyObject source)
                                             where T : DependencyObject
        {
            if (source != null)
            {
                var childs = GetChildObjects(source);
                foreach (DependencyObject child in childs)
                {
                    //analyze if children match the requested type
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    //recurse tree
                    foreach (T descendant in FindChildren<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }
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
        private ObservableCollection<BaseMangaSource.MangaImagesData> _ReaderInfo = new ObservableCollection<BaseMangaSource.MangaImagesData>();
        public ObservableCollection<BaseMangaSource.MangaImagesData> ReaderInfo
        {
            get { return _ReaderInfo; }
            set { _ReaderInfo = value; }
        }
        private ObservableCollection<BaseMangaSource.MangaImagesData> _NextReaderInfo = new ObservableCollection<BaseMangaSource.MangaImagesData>();
        public ObservableCollection<BaseMangaSource.MangaImagesData> NextReaderInfo
        {
            get { return _NextReaderInfo; }
            set { _NextReaderInfo = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SourceInitialized += MainWindow_SourceInitialized;

            PreviewKeyDown += (s, e) =>
            {
                Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
                if (key.Equals(Key.F11) || (e.KeyboardDevice.Modifiers.Equals(ModifierKeys.Alt) && key.Equals(Key.Enter)))
                {
                    if (Topmost == false)
                    {
                        Topmost = true;
                        WindowState = WindowState.Maximized;
                    }
                    else
                    {
                        Topmost = false;
                        WindowState = WindowState.Normal;
                    }
                }
                if (key.Equals(Key.Right))
                {
                    if ((c_MainTabTC.SelectedItem as TabItem).Header.ToString() == SelectedLanguage.Reader)
                    {
                        ContextMenu menu = tvImageView.Resources["MainCM"] as System.Windows.Controls.ContextMenu;
                        MenuItem nextmenu = menu.Items.GetItemAt(2) as MenuItem;
                        if(nextmenu.IsEnabled)
                            NextImage();
                    }
                }
                if (key.Equals(Key.Left))
                {
                    if ((c_MainTabTC.SelectedItem as TabItem).Header.ToString() == SelectedLanguage.Reader)
                    {
                        ContextMenu menu = tvImageView.Resources["MainCM"] as System.Windows.Controls.ContextMenu;
                        MenuItem prevmenu = menu.Items.GetItemAt(1) as MenuItem;
                        if (prevmenu.IsEnabled)
                            PrevImage();
                    }
                }
                if (key.Equals(Key.Down))
                {
                    if ((c_MainTabTC.SelectedItem as TabItem).Header.ToString() == SelectedLanguage.Reader)
                    {
                        if (!tvImageScrool.IsFocused)
                            tvImageScrool.ScrollToVerticalOffset(tvImageScrool.VerticalOffset + 10);
                    }
                }
                if (key.Equals(Key.Up))
                {
                    if ((c_MainTabTC.SelectedItem as TabItem).Header.ToString() == SelectedLanguage.Reader)
                    {
                        if (!tvImageScrool.IsFocused)
                            tvImageScrool.ScrollToVerticalOffset(tvImageScrool.VerticalOffset - 10);
                    }
                }
                if (e.KeyboardDevice.Modifiers.Equals(ModifierKeys.Control) && key.Equals(Key.Space))
                {
                    tvImageViewZoomBorder.Reset();
                }
                if (e.KeyboardDevice.Modifiers.Equals(ModifierKeys.Control) && key.Equals(Key.Add))
                {
                    //scale up just 5x
                    tvImageViewZoomBorder.ScaleUp(.5);
                }
                if (e.KeyboardDevice.Modifiers.Equals(ModifierKeys.Control) && key.Equals(Key.Subtract))
                {
                    //scale down just 5x
                    tvImageViewZoomBorder.ScaleDown(.5);
                }
                if (key.Equals(Key.F5))
                {
                    if ((c_MainTabTC.SelectedItem as TabItem).Header.ToString() == SelectedLanguage.Library && (c_LibraryTabTC.SelectedItem as TabItem).Header.ToString() == SelectedLanguage.Reading)
                    {
                        CheckForNewChapters();
                    };
                }
            };
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

            #region load_Chapters display direction
            if(MyIni.KeyExists("ChapterDisplayDirection", "WindowData") == true)
            {
                if (MyIni.Read("ChapterDisplayDirection", "WindowData") == "Start")
                {
                    c_ChapterDirectionCB.SelectedIndex = 0;
                }
                else
                {
                    c_ChapterDirectionCB.SelectedIndex = 1;
                }
            }
            else
            {
                MyIni.Write("ChapterDisplayDirection", "Start", "WindowData");
                c_ChapterDirectionCB.SelectedIndex = 0;
            }
            #endregion

            #region Initialize Syncing library
            loginAfterFirstRun:
            if (MyIni.KeyExists("SyncSerwer", "WindowData"))
            {
                // dropbox.com
                #region dropbox.com
                if (MyIni.Read("SyncSerwer", "WindowData") == "www.dropbox.com")
                {
                    loginServer.SelectedIndex = 0;
                    if (MyIni.KeyExists("accessToken", "DropBox") && MyIni.KeyExists("accessSecret", "DropBox"))
                    {
                        for (int i = 1; i < c_MainTabTC.Items.Count; i++)
                        {
                            (c_MainTabTC.Items[i] as TabItem).IsEnabled = true;
                        }
                        try
                        {
                            _client = new DropNetClient(_apiKey, _appsecret);

                            _client.UserLogin = new DropNet.Models.UserLogin { Token = MyIni.Read("accessToken", "DropBox"), Secret = MyIni.Read("accessSecret", "DropBox") };

                            loginSyncLibrary.IsEnabled = true;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else
                    {
                        for (int i = 1; i < c_MainTabTC.Items.Count; i++)
                        {
                            (c_MainTabTC.Items[i] as TabItem).IsEnabled = false;
                        }
                        try
                        {
                            _client = new DropNetClient(_apiKey, _appsecret);
                            _client.GetToken();

                            var url = _client.BuildAuthorizeUrl();

                            loginBrowser.Navigate(url);
                            loginBrowser.LoadCompleted += Browser_DropBoxLoadCompleted;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
                #endregion
                else if(MyIni.Read("SyncSerwer", "WindowData") == "www.googledrive.com")
                {
                }
                else
                {
                }
            }
            else
            {
                // pick up default server to sync library 
                loginServer.SelectedIndex = 0;
                MyIni.Write("SyncSerwer", (loginServer.SelectedItem as ComboBoxItem).Content.ToString(), "WindowData");
                goto loginAfterFirstRun;
            }
            #endregion
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
        private void Browser_DropBoxLoadCompleted(object sender, NavigationEventArgs e)
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
                        MyIni.Write("accessToken", accessToken.Token, "DropBox");
                        MyIni.Write("accessSecret", accessToken.Secret, "DropBox");

                        for (int i = 1; i < c_MainTabTC.Items.Count; i++)
                        {
                            (c_MainTabTC.Items[i] as TabItem).IsEnabled = true;
                        }
                        loginSyncLibrary.IsEnabled = true;
                    }
                },
                (error) =>
                {
                    throw new Exception(error.Message);
                });
            }
        }
        private void loginServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyIni.Write("SyncSerwer", (loginServer.SelectedItem as ComboBoxItem).Content.ToString(), "WindowData");
        }
        private void loginSyncLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (MyIni.KeyExists("SyncSerwer", "WindowData"))
            {
                // dropbox.com
                #region dropbox.com
                if (MyIni.Read("SyncSerwer", "WindowData") == "www.dropbox.com")
                {
                    _client.UseSandbox = true;
                    _client.GetMetaDataAsync(library.Path.Substring(library.Path.IndexOf('\\') + 1),
                    (metaData) =>
                    {
                        System.IO.FileStream info = System.IO.File.OpenRead(library.Path);
                        if (metaData.Bytes == info.Length)
                        {
                               _client.GetFileAsync(library.Path.Substring(library.Path.IndexOf('\\') + 1),
                               (response) =>
                               {
                                   if (response.RawBytes.SequenceEqual(System.IO.File.ReadAllBytes(library.Path)))
                                   {
                                       // file is same as 1 on disk
                                       this.Dispatcher.BeginInvoke((Action)(() =>
                                       {
                                           MetroMessageBox mbox = new MetroMessageBox();
                                           mbox.MessageBoxBtnYes.Click += (s, en) => { mbox.Close(); };
                                           mbox.MessageBoxBtnYes.Content = SelectedLanguage.Cancel;
                                           mbox.ShowMessage(this, SelectedLanguage.UploadedFileIsSameAsOurs, SelectedLanguage.Information, MessageBoxMessage.information, MessageBoxButton.OK);
                                       }));
                                   }
                                   else
                                   {
                                       // file on serwer and disk is different in content but same size, upload ours
                                       _client.UploadFileAsync("/", library.Path.Substring(library.Path.IndexOf('\\') + 1), System.IO.File.ReadAllBytes(library.Path),
                                        (upresponse) =>
                                        {
                                            this.Dispatcher.BeginInvoke((Action)(() =>
                                            {
                                                MetroMessageBox mbox = new MetroMessageBox();
                                                mbox.MessageBoxBtnYes.Click += (s, en) => { mbox.Close(); };
                                                mbox.MessageBoxBtnYes.Content = SelectedLanguage.Cancel;
                                                mbox.ShowMessage(this, SelectedLanguage.UploadSucces, SelectedLanguage.Information, MessageBoxMessage.information, MessageBoxButton.OK);
                                            }));
                                        },
                                        (error) =>
                                        {
                                            throw new Exception(error.Message);
                                        });
                                   }
                               },
                            (error) =>
                            {
                                throw new Exception(error.Message);
                            });                     
                        }
                        else if (metaData.Bytes > info.Length)
                        {
                            // server file is larger download it and replace ours 1 from disk
                            _client.GetFileAsync(library.Path.Substring(library.Path.IndexOf('\\') + 1),
                            (response) =>
                            {
                                this.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    System.IO.File.WriteAllBytes(library.Path, response.RawBytes);
                                    #region load_Library
                                    ReadingData.Clear();
                                    FinishedData.Clear();
                                    AbandonedData.Clear();

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
                                    MetroMessageBox mbox = new MetroMessageBox();
                                    mbox.MessageBoxBtnYes.Click += (s, en) => { mbox.Close(); };
                                    mbox.MessageBoxBtnYes.Content = SelectedLanguage.Cancel;
                                    mbox.ShowMessage(this, SelectedLanguage.DownloadSuccesAndReplaced, SelectedLanguage.Information, MessageBoxMessage.information, MessageBoxButton.OK);

                                }));
                            },
                            (error) =>
                            {
                                throw new Exception(error.Message);
                            });
                        }
                        else if (metaData.Bytes < info.Length)
                        {
                            // our file is larger upload it to server
                            _client.UploadFileAsync("/", library.Path.Substring(library.Path.IndexOf('\\') + 1), System.IO.File.ReadAllBytes(library.Path),
                            (response) =>
                            {
                                this.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    MetroMessageBox mbox = new MetroMessageBox();
                                    mbox.MessageBoxBtnYes.Click += (s, en) => { mbox.Close(); };
                                    mbox.MessageBoxBtnYes.Content = SelectedLanguage.Cancel;
                                    mbox.ShowMessage(this, SelectedLanguage.UploadSucces, SelectedLanguage.Information, MessageBoxMessage.information, MessageBoxButton.OK);
                                }));
                            },
                            (error) =>
                            {
                                throw new Exception(error.Message);
                            });
                        }
                        else
                        {
                            // 
                        }
                        info.Close();
                    },
                    (error) =>
                    {
                        throw new Exception(error.Message);
                    });
                }
                #endregion
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
            SelectedLanguage.ShowChapters = c.ShowChapters;
            SelectedLanguage.Move = c.Move;
            SelectedLanguage.MoveTo = c.MoveTo;
            SelectedLanguage.Up = c.Up;
            SelectedLanguage.Down = c.Down;
            SelectedLanguage.Top = c.Top;
            SelectedLanguage.End = c.End;
            SelectedLanguage.RemoveFromLibrary = c.RemoveFromLibrary;
            SelectedLanguage.ViewOnline = c.ViewOnline;
            SelectedLanguage.MarkAsRead = c.MarkAsRead;
            SelectedLanguage.MarkAllPreviousAsRead = c.MarkAllPreviousAsRead;
            SelectedLanguage.ChaptersDirection = c.ChaptersDirection;
            SelectedLanguage.FromStartToEnd = c.FromStartToEnd;
            SelectedLanguage.FromEndToStart = c.FromEndToStart;
            SelectedLanguage.UploadSucces = c.UploadSucces;
            SelectedLanguage.DownloadSuccesAndReplaced = c.DownloadSuccesAndReplaced;
            SelectedLanguage.UploadedFileIsSameAsOurs = c.UploadedFileIsSameAsOurs;
            SelectedLanguage.LoadingImagesMessage = c.LoadingImagesMessage;
            SelectedLanguage.ClosePage = c.ClosePage;
            SelectedLanguage.PrevImage = c.PrevImage;
            SelectedLanguage.NextImage = c.NextImage;
            SelectedLanguage.Page = c.Page;
            SelectedLanguage.CheckForNewChaptersMessage = c.CheckForNewChaptersMessage;

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
                                mbox.MessageBoxBtnYes.Content = SelectedLanguage.Cancel;
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
                    mbox.MessageBoxBtnYes.Content = SelectedLanguage.Cancel;
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
                            mbox.MessageBoxBtnYes.Content = SelectedLanguage.Cancel;
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
                            mbox.MessageBoxBtnYes.Content = SelectedLanguage.Cancel;
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
                mbox.MessageBoxBtnYes.Content = SelectedLanguage.Ok;
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
        private void MoveItem(int direction)
        {
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
            ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
            if (selectedListView.FirstOrDefault().SelectedIndex != -1)
            {
                BaseMangaSource.MangaPageData panel = data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex);
                int oldIndex = selectedListView.FirstOrDefault().SelectedIndex;
                int newIndex = selectedListView.FirstOrDefault().SelectedIndex + direction;
                int itemsCount = data.Count;
                if (panel != null)
                {
                    if (newIndex < 0 || newIndex >= itemsCount)
                        return; // Index out of range - close

                    data.Move(oldIndex, newIndex);
                    if (direction == 1) //down
                    {
                        library.MoveMangaUpDown(panel.Name, panel.Website, false);
                    }
                    else //up
                    {
                        library.MoveMangaUpDown(panel.Name, panel.Website, true);
                    }
                }
            }
        }
        private void MoveItemTopEnd(bool direction)
        {
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
            ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
            if (selectedListView.FirstOrDefault().SelectedIndex != -1)
            {
                BaseMangaSource.MangaPageData panel = data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex);
                int oldIndex = selectedListView.FirstOrDefault().SelectedIndex;
                int newIndex = 0;
                if (direction == true)
                    newIndex = 0;
                else
                    newIndex = data.Count - 1;

                int itemsCount = data.Count;
                if (panel != null)
                {
                    if (newIndex < 0 || newIndex >= itemsCount)
                        return; // Index out of range - nothing to do
                    data.Move(oldIndex, newIndex);

                    if (direction == true) //top
                    {
                        library.MoveMangaTopEnd(panel.Name, panel.Website, false);
                    }
                    else //end
                    {
                        library.MoveMangaTopEnd(panel.Name, panel.Website, true);
                    }
                }
            }
        }
        private void MoveUp(object sender, RoutedEventArgs e)
        {
            MoveItem(-1);
        }
        private void MoveDown(object sender, RoutedEventArgs e)
        {
            MoveItem(1);
        }
        private void MoveTop(object sender, RoutedEventArgs e)
        {
            MoveItemTopEnd(true);
        }
        private void MoveEnd(object sender, RoutedEventArgs e)
        {
            MoveItemTopEnd(false);
        }
        private void MenuItem_Click_MoveToReading(object sender, RoutedEventArgs e)
        {
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
            ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
            if (selectedListView.FirstOrDefault().SelectedIndex != -1)
            {
                library.ChangePlace(data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex).Name, data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex).Website, Core.PlaceInLibrary.Reading);
                ReadingData.Add(data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex));
                data.RemoveAt(selectedListView.FirstOrDefault().SelectedIndex);  
            }
        }
        private void MenuItem_Click_MoveToFinished(object sender, RoutedEventArgs e)
        {
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
            ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
            if (selectedListView.FirstOrDefault().SelectedIndex != -1)
            {
                library.ChangePlace(data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex).Name, data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex).Website, Core.PlaceInLibrary.Finished);
                FinishedData.Add(data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex));
                data.RemoveAt(selectedListView.FirstOrDefault().SelectedIndex);
            }
        }
        private void MenuItem_Click_MoveToAbandoned(object sender, RoutedEventArgs e)
        {
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
            ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
            if (selectedListView.FirstOrDefault().SelectedIndex != -1)
            {
                library.ChangePlace(data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex).Name, data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex).Website, Core.PlaceInLibrary.Abandoned);
                AbandonedData.Add(data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex));
                data.RemoveAt(selectedListView.FirstOrDefault().SelectedIndex);         
            }
        }
        private void MenuItem_Click_RemoveFromLibrary(object sender, RoutedEventArgs e)
        {
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren< ListView>();
            ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
            if (selectedListView.FirstOrDefault().SelectedIndex != -1)
            {
                library.RemoveMangaAndAllData(data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex).Name, data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex).Website);
                data.RemoveAt(selectedListView.FirstOrDefault().SelectedIndex);              
            }
        }
        private void tvLibrary_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
            if (selectedListView.FirstOrDefault().SelectedItem != null)
            {
                // Enable all contextmenu items
                ContextMenu cMenu = (selectedListView.FirstOrDefault().Resources["MainCM"] as System.Windows.Controls.ContextMenu);
                foreach (MenuItem childMenu in cMenu.Items)
                {
                    childMenu.IsEnabled = true;
                    if (childMenu.HasItems)
                    {
                        foreach (MenuItem childsOfChildMenu in childMenu.Items)
                        {
                            childsOfChildMenu.IsEnabled = true;
                        }
                    }
                }
                if (selectedListView.FirstOrDefault().SelectedIndex == 0)
                {
                    ((cMenu.Items[1] as MenuItem).Items[0] as MenuItem).IsEnabled = false; // disable move up
                    ((cMenu.Items[1] as MenuItem).Items[2] as MenuItem).IsEnabled = false; // disable move to beginning
                }
                ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
                if (selectedListView.FirstOrDefault().SelectedIndex == data.Count - 1)
                {
                    ((cMenu.Items[1] as MenuItem).Items[1] as MenuItem).IsEnabled = false; // disable move down
                    ((cMenu.Items[1] as MenuItem).Items[3] as MenuItem).IsEnabled = false; // disable move to end
                }
            }
            else
            {
                // disable all contextmenu items
                if (selectedListView.FirstOrDefault().SelectedIndex == -1)
                {
                    ContextMenu cMenu = ((sender as ListView).Resources["MainCM"] as System.Windows.Controls.ContextMenu);
                    foreach (MenuItem childMenu in cMenu.Items)
                    {
                        childMenu.IsEnabled = false;
                        if (childMenu.HasItems)
                        {
                            foreach (MenuItem childsOfChildMenu in childMenu.Items)
                            {
                                childsOfChildMenu.IsEnabled = false;
                            }
                        }
                    }
                }
            }
        }
        private async void MenuItem_Click_ShowChaptersLibrary(object sender, RoutedEventArgs e)
        {
            ChaptersInfo.Clear();
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
            ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
            if (selectedListView.FirstOrDefault().SelectedIndex != -1)
            {
                BaseMangaSource.MangaPageData panel = data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex);
                if (panel != null)
                {
                    if (SearchPluginData.ElementAt(c_SearchCB.SelectedIndex).Website == panel.UrlToMainpage)
                    {
                        // dont do anything we got good parser
                    }
                    else
                    {
                        // pick up good parser
                        for (int i = 0; i < SearchPluginData.Count; i++)
                        {
                            c_SearchCB.SelectedIndex = i;
                            if (SearchPluginData.ElementAt(c_SearchCB.SelectedIndex).Website == panel.UrlToMainpage)
                            {
                                break;
                            }
                        }
                    }
                    bool reult = await parser.ParseSelectedPageAsync(panel.Website, true, null, ChaptersInfo);
                    if (reult == true)
                    {
                        if (MyIni.KeyExists("ChapterDisplayDirection", "WindowData") == true)
                        {
                            if (MyIni.Read("ChapterDisplayDirection", "WindowData") == "Start")
                            {
                                for (int i = 0; i < ChaptersInfo.Count; i++)
                                {
                                    if (library.IsChapterMarkedAsReadInManga(panel.Name, panel.Website, ChaptersInfo.ElementAt(i).Name, ChaptersInfo.ElementAt(i).UrlToPage))
                                    {
                                        ChaptersInfo.ElementAt(i).Foreground = Brushes.Gray;
                                    }
                                    else
                                    {
                                        ChaptersInfo.ElementAt(i).Foreground = Brushes.Black;
                                    }
                                }
                                c_MainTabTC.SelectedIndex = 3;
                            }
                            else //end
                            {
                                for (int i = 0; i < ChaptersInfo.Count; i++)
                                {
                                    ChaptersInfo.Move(ChaptersInfo.Count - 1, i); //reverse list

                                    if (library.IsChapterMarkedAsReadInManga(panel.Name, panel.Website, ChaptersInfo.ElementAt(i).Name, ChaptersInfo.ElementAt(i).UrlToPage))
                                    {
                                        ChaptersInfo.ElementAt(i).Foreground = Brushes.Gray;
                                    }
                                    else
                                    {
                                        ChaptersInfo.ElementAt(i).Foreground = Brushes.Black;
                                    }
                                }
                                c_MainTabTC.SelectedIndex = 3;
                            }
                            // check for new chapters
                            int getSavedChapters = library.GetChaptersAmount(panel.Name, panel.Website);
                            int getUnreadChapters = panel.UnreadChapters;
                            if ((getSavedChapters + getUnreadChapters) < ChaptersInfo.Count)
                            {
                                // we got new chapters
                                int newChapters = ChaptersInfo.Count - (getSavedChapters + getUnreadChapters);
                                library.ChangeUnreadChapters(panel.Name, panel.Website, string.Format("{0}", newChapters + getUnreadChapters));
                                library.Save();
                                panel.UnreadChapters = newChapters + getUnreadChapters;
                            }
                        }

                    }
                }
            }
        }
        private void MenuItem_Click_MarkAsRead(object sender, RoutedEventArgs e)
        {
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
            ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
            if (selectedListView.FirstOrDefault().SelectedIndex != -1)
            {
                BaseMangaSource.MangaPageData panel = data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex);
                if (panel != null)
                {
                    BaseMangaSource.MangaPageChapters selectedChapter = null;
                    if (tvChaptersList.SelectedItem != null)
                        selectedChapter = ChaptersInfo.ElementAt(tvChaptersList.SelectedIndex);

                    if (selectedChapter != null)
                    {
                        if (IsEqual(selectedChapter.Foreground, Brushes.Gray))
                        {
                            // it's already marked as read, no need to do anything
                        }
                        else
                        {
                            selectedChapter.Foreground = Brushes.Gray;
                            // mark chapter and decrease unread ammount
                            if (library.IsChapterExistInManga(panel.Name, panel.Website, selectedChapter.Name, selectedChapter.UrlToPage))
                            {
                                library.MarkChapter(panel.Name, panel.Website, selectedChapter.Name, selectedChapter.UrlToPage, true);

                                int getUnreadChapters = panel.UnreadChapters;
                                if (getUnreadChapters > 0)
                                {
                                    library.ChangeUnreadChapters(panel.Name, panel.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                    panel.UnreadChapters = getUnreadChapters - 1;
                                }
                            }
                            else
                            {
                                library.AddChapterToManga(panel.Name, panel.Website, selectedChapter.Name, selectedChapter.UrlToPage, true);

                                int getUnreadChapters = panel.UnreadChapters;
                                if (getUnreadChapters > 0)
                                {
                                    library.ChangeUnreadChapters(panel.Name, panel.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                    panel.UnreadChapters = getUnreadChapters - 1;
                                }
                            }
                            library.Save();
                        }
                    }
                }
            }
        }
        private void MenuItem_Click_MarkAllPreviousAsRead(object sender, RoutedEventArgs e)
        {
            IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
            ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
            if (selectedListView.FirstOrDefault().SelectedIndex != -1)
            {
                BaseMangaSource.MangaPageData panel = data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex);
                if (panel != null)
                {
                    BaseMangaSource.MangaPageChapters selectedChapter = null;
                    if (tvChaptersList.SelectedItem != null)
                        selectedChapter = ChaptersInfo.ElementAt(tvChaptersList.SelectedIndex);

                    if (selectedChapter != null)
                    {
                        if (MyIni.KeyExists("ChapterDisplayDirection", "WindowData") == true)
                        {
                            new Task(() =>
                            {
                                if (MyIni.Read("ChapterDisplayDirection", "WindowData") == "Start")
                                {
                                    for (int i = 0; i < tvChaptersList.Items.Count; i++)
                                    {
                                        BaseMangaSource.MangaPageChapters itemToCompare = ChaptersInfo.ElementAt(i);
                                        if (selectedChapter.Name == itemToCompare.Name && selectedChapter.UrlToPage == itemToCompare.UrlToPage)
                                        {
                                            library.Save();
                                            return;
                                        }
                                        else
                                        {
                                            if (IsEqual(itemToCompare.Foreground, Brushes.Gray))
                                            {
                                                //dont do anything, since its already marked :p
                                            }
                                            else
                                            {
                                                itemToCompare.Foreground = Brushes.Gray;

                                                if (library.IsChapterExistInManga(panel.Name, panel.Website, itemToCompare.Name, itemToCompare.UrlToPage))
                                                {
                                                    library.MarkChapter(panel.Name, panel.Website, itemToCompare.Name, itemToCompare.UrlToPage, true);
                                                    int getUnreadChapters = panel.UnreadChapters;
                                                    if (getUnreadChapters > 0)
                                                    {
                                                        library.ChangeUnreadChapters(panel.Name, panel.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                                        panel.UnreadChapters = getUnreadChapters - 1;
                                                    }
                                                }
                                                else
                                                {
                                                    library.AddChapterToManga(panel.Name, panel.Website, itemToCompare.Name, itemToCompare.UrlToPage, true);
                                                    int getUnreadChapters = panel.UnreadChapters;
                                                    if (getUnreadChapters > 0)
                                                    {
                                                        library.ChangeUnreadChapters(panel.Name, panel.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                                        panel.UnreadChapters = getUnreadChapters - 1;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    library.Save();
                                }
                                else //"End"
                                {
                                    for (int i = tvChaptersList.Items.Count - 1; i > 0; i--)
                                    {
                                        BaseMangaSource.MangaPageChapters itemToCompare = ChaptersInfo.ElementAt(i);
                                        if (selectedChapter.Name == itemToCompare.Name && selectedChapter.UrlToPage == itemToCompare.UrlToPage)
                                        {
                                            library.Save();
                                            return;
                                        }
                                        else
                                        {
                                            if (IsEqual(itemToCompare.Foreground, Brushes.Gray))
                                            {
                                                //dont do anything, since its already marked :p
                                            }
                                            else
                                            {
                                                itemToCompare.Foreground = Brushes.Gray;

                                                if (library.IsChapterExistInManga(panel.Name, panel.Website, itemToCompare.Name, itemToCompare.UrlToPage))
                                                {
                                                    library.MarkChapter(panel.Name, panel.Website, itemToCompare.Name, itemToCompare.UrlToPage, true);
                                                    int getUnreadChapters = panel.UnreadChapters;
                                                    if (getUnreadChapters > 0)
                                                    {
                                                        library.ChangeUnreadChapters(panel.Name, panel.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                                        panel.UnreadChapters = getUnreadChapters - 1;
                                                    }
                                                }
                                                else
                                                {
                                                    library.AddChapterToManga(panel.Name, panel.Website, itemToCompare.Name, itemToCompare.UrlToPage, true);
                                                    int getUnreadChapters = panel.UnreadChapters;
                                                    if (getUnreadChapters > 0)
                                                    {
                                                        library.ChangeUnreadChapters(panel.Name, panel.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                                        panel.UnreadChapters = getUnreadChapters - 1;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    library.Save();

                                }
                            }).Start();
                        }
                    }
                }
            }
        }
        private void c_ChapterDirectionCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((c_ChapterDirectionCB.SelectedItem as ComboBoxItem).Content as string == SelectedLanguage.FromStartToEnd)
            {
                if (ChaptersInfo.Count > 0)
                {
                    for (int i = 0; i < ChaptersInfo.Count; i++)
                    {
                        ChaptersInfo.Move(ChaptersInfo.Count - 1, i); //reverse list
                    }
                }
                MyIni.Write("ChapterDisplayDirection", "Start", "WindowData");
            }
            else // End to Start
            {
                if (ChaptersInfo.Count > 0)
                {
                    for (int i = 0; i < ChaptersInfo.Count; i++)
                    {
                        ChaptersInfo.Move(ChaptersInfo.Count - 1, i); //reverse list
                    }
                }
                MyIni.Write("ChapterDisplayDirection", "End", "WindowData");
            }
        }
        public static bool IsEqual(Brush aBrush1, Brush aBrush2)
        {
            if (aBrush1.GetType() != aBrush2.GetType())
                return false;
            else
            {
                if (aBrush1 is SolidColorBrush)
                {
                    return (aBrush1 as SolidColorBrush).Color ==
                      (aBrush2 as SolidColorBrush).Color &&
                      (aBrush1 as SolidColorBrush).Opacity == (aBrush2 as SolidColorBrush).Opacity;
                }
                else if (aBrush1 is LinearGradientBrush)
                {
                    bool result = true;
                    result = (aBrush1 as LinearGradientBrush).ColorInterpolationMode ==
                      (aBrush2 as LinearGradientBrush).ColorInterpolationMode && result;
                    result = (aBrush1 as LinearGradientBrush).EndPoint ==
                      (aBrush2 as LinearGradientBrush).EndPoint && result;
                    result = (aBrush1 as LinearGradientBrush).MappingMode ==
                      (aBrush2 as LinearGradientBrush).MappingMode && result;
                    result = (aBrush1 as LinearGradientBrush).Opacity ==
                      (aBrush2 as LinearGradientBrush).Opacity && result;
                    result = (aBrush1 as LinearGradientBrush).StartPoint ==
                      (aBrush2 as LinearGradientBrush).StartPoint && result;
                    result = (aBrush1 as LinearGradientBrush).SpreadMethod ==
                      (aBrush2 as LinearGradientBrush).SpreadMethod && result;
                    result = (aBrush1 as LinearGradientBrush).GradientStops.Count ==
                      (aBrush2 as LinearGradientBrush).GradientStops.Count && result;
                    if (result && (aBrush1 as LinearGradientBrush).GradientStops.Count ==
                              (aBrush2 as LinearGradientBrush).GradientStops.Count)
                    {
                        for (int i = 0; i < (aBrush1 as LinearGradientBrush).GradientStops.Count; i++)
                        {
                            result = (aBrush1 as LinearGradientBrush).GradientStops[i].Color ==
                              (aBrush2 as LinearGradientBrush).GradientStops[i].Color && result;
                            result = (aBrush1 as LinearGradientBrush).GradientStops[i].Offset ==
                              (aBrush2 as LinearGradientBrush).GradientStops[i].Offset && result;
                            if (!result)
                                return result;
                        }
                    }
                    return result;
                }
                else if (aBrush1 is RadialGradientBrush)
                {
                    bool result = true;
                    result = (aBrush1 as RadialGradientBrush).ColorInterpolationMode ==
                                 (aBrush2 as RadialGradientBrush).ColorInterpolationMode && result;
                    result = (aBrush1 as RadialGradientBrush).GradientOrigin ==
                                (aBrush2 as RadialGradientBrush).GradientOrigin && result;
                    result = (aBrush1 as RadialGradientBrush).MappingMode == (aBrush2 as RadialGradientBrush).MappingMode && result;
                    result = (aBrush1 as RadialGradientBrush).Opacity == (aBrush2 as RadialGradientBrush).Opacity && result;
                    result = (aBrush1 as RadialGradientBrush).RadiusX == (aBrush2 as RadialGradientBrush).RadiusX && result;
                    result = (aBrush1 as RadialGradientBrush).RadiusY == (aBrush2 as RadialGradientBrush).RadiusY && result;
                    result = (aBrush1 as RadialGradientBrush).SpreadMethod == (aBrush2 as RadialGradientBrush).SpreadMethod && result;
                    result = (aBrush1 as RadialGradientBrush).GradientStops.Count == (aBrush2 as RadialGradientBrush).GradientStops.Count && result;
                    if (result && (aBrush1 as RadialGradientBrush).GradientStops.Count == (aBrush2 as RadialGradientBrush).GradientStops.Count)
                    {
                        for (int i = 0; i < (aBrush1 as RadialGradientBrush).GradientStops.Count; i++)
                        {
                            result = (aBrush1 as RadialGradientBrush).GradientStops[i].Color ==
                                          (aBrush2 as RadialGradientBrush).GradientStops[i].Color && result;
                            result = (aBrush1 as RadialGradientBrush).GradientStops[i].Offset ==
                                              (aBrush2 as RadialGradientBrush).GradientStops[i].Offset && result;
                            if (!result)
                                return result;
                        }
                    }
                    return result;
                }
                else if (aBrush1 is ImageBrush)
                {
                    bool result = true;
                    result = (aBrush1 as ImageBrush).AlignmentX == (aBrush2 as ImageBrush).AlignmentX && result;
                    result = (aBrush1 as ImageBrush).AlignmentY == (aBrush2 as ImageBrush).AlignmentY && result;
                    result = (aBrush1 as ImageBrush).Opacity == (aBrush2 as ImageBrush).Opacity && result;
                    result = (aBrush1 as ImageBrush).Stretch == (aBrush2 as ImageBrush).Stretch && result;
                    result = (aBrush1 as ImageBrush).TileMode == (aBrush2 as ImageBrush).TileMode && result;
                    result = (aBrush1 as ImageBrush).Viewbox == (aBrush2 as ImageBrush).Viewbox && result;
                    result = (aBrush1 as ImageBrush).ViewboxUnits == (aBrush2 as ImageBrush).ViewboxUnits && result;
                    result = (aBrush1 as ImageBrush).Viewport == (aBrush2 as ImageBrush).Viewport && result;
                    result = (aBrush1 as ImageBrush).ViewportUnits == (aBrush2 as ImageBrush).ViewportUnits && result;

                    result = (aBrush1 as ImageBrush).ImageSource == (aBrush2 as ImageBrush).ImageSource && result;
                    return result;
                }
            }
            return false;
        }
        private List<BitmapImage> ImageList = new List<BitmapImage>();
        private List<BitmapImage> NextChapterImageList = new List<BitmapImage>();
        private async void LoadNextChapterIfExist()
        {
            BaseMangaSource.MangaPageChapters item = null;
            string ChapterDisplayDirection = null;
            if (MyIni.KeyExists("ChapterDisplayDirection", "WindowData") == true)
            {
                ChapterDisplayDirection = MyIni.Read("ChapterDisplayDirection", "WindowData");
            }
            else
            {
                ChapterDisplayDirection = "Start";
            }

            if (ChapterDisplayDirection == "Start")
            {
                if (ChaptersInfo.IndexOf(tvChaptersList.SelectedItem as BaseMangaSource.MangaPageChapters) < ChaptersInfo.Count - 1)
                {
                    item = ChaptersInfo.ElementAt(tvChaptersList.SelectedIndex + 1);
                }
                else
                {
                    NextChapterImageList.Clear();
                    return;
                }
            }
            else
            {
                if (((ChaptersInfo.Count - 1) - tvChaptersList.SelectedIndex) < ChaptersInfo.Count - 1)
                {
                    item = ChaptersInfo.ElementAt(tvChaptersList.SelectedIndex - 1);
                }
                else 
                {
                    NextChapterImageList.Clear();
                    return;
                }
            }
            if (item != null)
            {
                NextChapterImageList.Clear();
                NextReaderInfo.Clear();
                bool result = await parser.ParseImagesAsync(item.UrlToPage, NextReaderInfo);

                if (result == true)
                {
                    string nextLink = null;

                    nextLink = null;

                    NextChapterImageList.Add(new BitmapImage(new Uri(NextReaderInfo.Last().ImageLink)));

                    for (int i = 0; i < NextReaderInfo.Last().MaxPages; i++)
                    {
                        nextLink = NextReaderInfo.Last().NextLink;

                        if (nextLink != null)
                        {
                            result = await parser.ParseImagesAsync(nextLink, NextReaderInfo);
                            if (NextReaderInfo.Last().ImageLink != "")
                                NextChapterImageList.Add(new BitmapImage(new Uri(NextReaderInfo.Last().ImageLink)));
                            else
                                NextChapterImageList.Add(new BitmapImage());
                        }
                        else
                        {
                        }
                    }
                }
            }
        }
        private async void MenuItem_Click_ViewOnline(object sender, RoutedEventArgs e)
        {
            BaseMangaSource.MangaPageChapters item = null;
            if (tvChaptersList.SelectedItem != null)
                item = ChaptersInfo.ElementAt(tvChaptersList.SelectedIndex);

            if (item != null)
            {
                tvImageView.Source = null;
                ImageList.Clear();
                NextChapterImageList.Clear();
                ReaderInfo.Clear();

                bool result = await parser.ParseImagesAsync(item.UrlToPage, ReaderInfo);

                if (result == true)
                {
                    SelectedLanguage.WindowTitle = string.Format("'{0}'-Page: {1}/{2}", item.Name, ReaderInfo.Last().PageNumber, ReaderInfo.Last().MaxPages);

                    tvImageScrool.ScrollToTop();

                    tvLoadingProgress.Visibility = Visibility.Visible;
                    tvLoadingText.Visibility = Visibility.Visible;
                    tvLoadingProgress.Minimum = 1;
                    tvLoadingProgress.Maximum = ReaderInfo.Last().MaxPages;

                    for (int i = 0; i < c_MainTabTC.Items.Count; i++)
                    {
                        (c_MainTabTC.Items[i] as TabItem).IsEnabled = false;
                    }
                    (c_MainTabTC.Items[4] as TabItem).IsEnabled = true;
                    c_MainTabTC.SelectedIndex = 4;

                    string nextLink = null;

                    ImageList.Add(new BitmapImage(new Uri(ReaderInfo.Last().ImageLink)));

                    for (int i = 0; i < ReaderInfo.Last().MaxPages; i++)
                    {
                        tvLoadingProgress.Value = i;
                        nextLink = ReaderInfo.Last().NextLink;
                        if (nextLink != null)
                        {
                            result = await parser.ParseImagesAsync(nextLink, ReaderInfo);
                            if (ReaderInfo.Last().ImageLink != "")
                                ImageList.Add(new BitmapImage(new Uri(ReaderInfo.Last().ImageLink)));
                            else
                                ImageList.Add(new BitmapImage());
                        }
                        else
                        {
                            tvLoadingProgress.Visibility = Visibility.Collapsed;
                            tvLoadingText.Visibility = Visibility.Collapsed;
                            tvLoadingProgress.Value = 1;
                            tvImageView.Source = ImageList[0];
                            tvImageScrool.ScrollToTop();
                            tvImageList.SelectedIndex = 0;              
                            tvLoadingProgress.Visibility = Visibility.Collapsed;

                            ContextMenu menu = tvImageView.Resources["MainCM"] as System.Windows.Controls.ContextMenu;
                            MenuItem nextmenu = menu.Items.GetItemAt(2) as MenuItem; // next
                            nextmenu.IsEnabled = true;

                            LoadNextChapterIfExist();

                            return;
                        }
                    }
                }
                else
                {
                }
            }
        }
        private void tvImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tvImageScrool.ScrollToTop();
            if ((sender as ListView).SelectedItem != null)
            {
                tvImageView.Source = ImageList[(sender as ListView).SelectedIndex];

                SelectedLanguage.WindowTitle = string.Format("'{0}'-{1}: {2}/{3}", ChaptersInfo.ElementAt(tvChaptersList.SelectedIndex).Name, SelectedLanguage.Page, (sender as ListView).SelectedIndex + 1, ReaderInfo.Last().MaxPages);

                if ((sender as ListView).SelectedIndex == 0)
                {
                    ContextMenu menu = tvImageView.Resources["MainCM"] as System.Windows.Controls.ContextMenu;
                    MenuItem prevmenu = menu.Items.GetItemAt(1) as MenuItem;
                    prevmenu.IsEnabled = false;
                }
                else
                {
                    ContextMenu menu = tvImageView.Resources["MainCM"] as System.Windows.Controls.ContextMenu;
                    MenuItem prevmenu = menu.Items.GetItemAt(1) as MenuItem;
                    prevmenu.IsEnabled = true;
                }
            }
        }
        private void NextImage()
        {
            ContextMenu menu = tvImageView.Resources["MainCM"] as System.Windows.Controls.ContextMenu;
            int maxPages = tvImageList.Items.Count - 1;
            int currentPage = tvImageList.SelectedIndex;

            if (currentPage == maxPages)
            {
                if (NextChapterImageList.Count > 0 && NextChapterImageList != null)
                {
                    ReaderInfo.Clear();
                    ImageList.Clear();
                    ImageList = NextChapterImageList.ToList();
                    try
                    {
                        foreach (BaseMangaSource.MangaImagesData imageData in NextReaderInfo)
                        {
                            ReaderInfo.Add(imageData);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        tvImageView.Source = ImageList[0];
                        tvImageScrool.ScrollToTop();

                        NextReaderInfo.Clear();
                        NextChapterImageList.Clear();
                        
                        maxPages = tvImageList.Items.Count;

                        MenuItem prevmenu = menu.Items.GetItemAt(1) as MenuItem; // prev
                        prevmenu.IsEnabled = false;
                        MenuItem nextmenu = menu.Items.GetItemAt(2) as MenuItem; // next
                        nextmenu.IsEnabled = true;

                        tvImageList.SelectedIndex = 0;

                        BaseMangaSource.MangaPageData item = null;
                        IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
                        ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
                        if (selectedListView.FirstOrDefault().SelectedIndex != -1)
                        {
                            item = data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex);
                        }

                        if (item != null)
                        {
                            BaseMangaSource.MangaPageChapters itm = null;

                            if (tvChaptersList.SelectedItem != null)
                                itm = ChaptersInfo.ElementAt(tvChaptersList.SelectedIndex);
                            SelectedLanguage.WindowTitle = string.Format("'{0}'-{1}: {2}/{3}", itm.Name, SelectedLanguage.Page, 1, ReaderInfo.Last().MaxPages);

                            if (itm != null)
                            {
                                if (IsEqual(itm.Foreground, Brushes.Gray))
                                {
                                }
                                else
                                {
                                    itm.Foreground = Brushes.Gray;
                                    if (library.IsChapterExistInManga(item.Name, item.Website, itm.Name, itm.UrlToPage))
                                    {
                                        library.MarkChapter(item.Name, item.Website, itm.Name, itm.UrlToPage, true);
                                        int getUnreadChapters = item.UnreadChapters;
                                        if (getUnreadChapters > 0)
                                        {
                                            library.ChangeUnreadChapters(item.Name, item.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                            item.UnreadChapters = getUnreadChapters - 1;
                                        }
                                    }
                                    else
                                    {
                                        library.AddChapterToManga(item.Name, item.Website, itm.Name, itm.UrlToPage, true);
                                        int getUnreadChapters = item.UnreadChapters;
                                        if (getUnreadChapters > 0)
                                        {
                                            library.ChangeUnreadChapters(item.Name, item.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                            item.UnreadChapters = getUnreadChapters - 1;
                                        }
                                    }
                                    library.Save();
                                }
                            }
                        }

                        string ChapterDisplayDirection = null;
                        if (MyIni.KeyExists("ChapterDisplayDirection", "WindowData") == true)
                        {
                            ChapterDisplayDirection = MyIni.Read("ChapterDisplayDirection", "WindowData");
                        }
                        else
                        {
                            ChapterDisplayDirection = "Start";
                        }

                        if (ChapterDisplayDirection == "Start")
                        {
                            tvChaptersList.SelectedIndex += 1;
                        }
                        else
                        {
                            if (((ChaptersInfo.Count - 1) - tvChaptersList.SelectedIndex) < ChaptersInfo.Count - 1)
                            {
                                tvChaptersList.SelectedIndex -= 1;
                            }
                            else 
                            {
                            }
                        }
                        LoadNextChapterIfExist();
                    }
                }
                else
                {
                    BaseMangaSource.MangaPageData item = null;
                    IEnumerable<ListView> selectedListView = (c_LibraryTabTC.SelectedItem as TabItem).FindChildren<ListView>();
                    ObservableCollection<BaseMangaSource.MangaPageData> data = (selectedListView.FirstOrDefault().ItemsSource as ObservableCollection<BaseMangaSource.MangaPageData>);
                    if (selectedListView.FirstOrDefault().SelectedIndex != -1)
                    {
                        item = data.ElementAt(selectedListView.FirstOrDefault().SelectedIndex);
                        SelectedLanguage.WindowTitle = string.Format("'{0}'-{1}: {2}/{3}", item.Name, SelectedLanguage.Page, 1, ReaderInfo.Last().MaxPages);
                    }

                    if (item != null)
                    {
                        BaseMangaSource.MangaPageChapters itm = null;

                        if (tvChaptersList.SelectedItem != null)
                            itm = ChaptersInfo.ElementAt(tvChaptersList.SelectedIndex);

                        if (itm != null)
                        {
                            if (IsEqual(itm.Foreground, Brushes.Gray))
                            {
                            }
                            else
                            {
                                itm.Foreground = Brushes.Gray;
                                if (library.IsChapterExistInManga(item.Name, item.Website, itm.Name, itm.UrlToPage))
                                {
                                    library.MarkChapter(item.Name, item.Website, itm.Name, itm.UrlToPage, true);
                                    int getUnreadChapters = item.UnreadChapters;
                                    if (getUnreadChapters > 0)
                                    {
                                        library.ChangeUnreadChapters(item.Name, item.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                        item.UnreadChapters = getUnreadChapters - 1;
                                    }
                                }
                                else
                                {
                                    library.AddChapterToManga(item.Name, item.Website, itm.Name, itm.UrlToPage, true);
                                    int getUnreadChapters = item.UnreadChapters;
                                    if (getUnreadChapters > 0)
                                    {
                                        library.ChangeUnreadChapters(item.Name, item.Website, string.Format("{0}", (getUnreadChapters - 1)));
                                        item.UnreadChapters = getUnreadChapters - 1;
                                    }
                                }
                                library.Save();
                            }
                        }
                    }

                    MenuItem prevmenu = menu.Items.GetItemAt(1) as MenuItem; // prev
                    prevmenu.IsEnabled = false;
                    MenuItem nextmenu = menu.Items.GetItemAt(2) as MenuItem; // next
                    nextmenu.IsEnabled = false;

                    tvImageView.Source = null;                   
                    SelectedLanguage.WindowTitle = "";

                    ReaderInfo.Clear();
                    ImageList.Clear();

                    for (int i = 0; i < c_MainTabTC.Items.Count; i++)
                    {
                        (c_MainTabTC.Items[i] as TabItem).IsEnabled = true;
                    }
                    c_MainTabTC.SelectedIndex = 3;
                }
            }
            else
            {
                MenuItem prevmenu = menu.Items.GetItemAt(1) as MenuItem; // prev
                prevmenu.IsEnabled = true;
                MenuItem nextmenu = menu.Items.GetItemAt(2) as MenuItem; // next
                nextmenu.IsEnabled = true;
                currentPage += 1;
                tvImageList.SelectedIndex = currentPage;
            }
        }
        private void PrevImage()
        {
            ContextMenu menu = tvImageView.Resources["MainCM"] as System.Windows.Controls.ContextMenu;
            int currentPage = tvImageList.SelectedIndex;
            if (currentPage == 0)
            {
                MenuItem prevmenu = menu.Items.GetItemAt(1) as MenuItem;
                prevmenu.IsEnabled = false;
            }
            else
            {
                MenuItem prevmenu = menu.Items.GetItemAt(1) as MenuItem;
                prevmenu.IsEnabled = true;
                currentPage -= 1;
                tvImageList.SelectedIndex = currentPage;
                if (currentPage == 0)
                {
                    prevmenu = menu.Items.GetItemAt(1) as MenuItem;
                    prevmenu.IsEnabled = false;
                }
            }
        }
        private void MenuItem_Click_ClosePage(object sender, RoutedEventArgs e)
        {
            tvImageView.Source = null;

            ReaderInfo.Clear();
            NextReaderInfo.Clear();
            tvImageList.Visibility = Visibility.Collapsed;

            ContextMenu menu = tvImageView.Resources["MainCM"] as System.Windows.Controls.ContextMenu;
            MenuItem prevmenu = menu.Items.GetItemAt(1) as MenuItem; // prev
            prevmenu.IsEnabled = false;
            MenuItem nextmenu = menu.Items.GetItemAt(2) as MenuItem; // next
            nextmenu.IsEnabled = false;

            for (int i = 0; i < c_MainTabTC.Items.Count; i++)
            {
                (c_MainTabTC.Items[i] as TabItem).IsEnabled = true;
            }

            c_MainTabTC.SelectedIndex = 3;

            SelectedLanguage.WindowTitle = "";

            ImageList.Clear();
            NextChapterImageList.Clear();
        }
        private void MenuItem_Click_PrevPage(object sender, RoutedEventArgs e)
        {
            PrevImage();
        }
        private void MenuItem_Click_NextPage(object sender, RoutedEventArgs e)
        {
            NextImage();
        }
        private async void CheckForNewChapters()
        {
            if (ReadingData.Count > 0)
            {
                tvLibraryUpdateText.Visibility = Visibility.Visible;
                tvLibraryUpdateProgress.Visibility = Visibility.Visible;
                tvLibraryUpdateProgress.Maximum = ReadingData.Count;

                for (int i = 0; i < ReadingData.Count; i++)
                {
                    tvLibraryUpdateProgress.Value = i;

                    BaseMangaSource.MangaPageData item = ReadingData.ElementAt(i);
                    // check if good parser is choosed
                    if (SearchPluginData.ElementAt(c_SearchCB.SelectedIndex).Website == item.UrlToMainpage)
                    {
                        // dont do anything we got good parser
                    }
                    else
                    {
                        // pick up good parser
                        for (int j = 0; j < SearchPluginData.Count; j++)
                        {
                            c_SearchCB.SelectedIndex = j;
                            if (SearchPluginData.ElementAt(c_SearchCB.SelectedIndex).Website == item.UrlToMainpage)
                            {
                                break;
                            }
                        }
                    }

                    ChaptersInfo.Clear();
                    bool result = await parser.ParseSelectedPageAsync(item.Website, true, null, ChaptersInfo);

                    if (result == true)
                    {
                        int getSavedChapters = library.GetChaptersAmount(item.Name, item.Website);
                        int getUnreadChapters = item.UnreadChapters;
                        if ((getSavedChapters + getUnreadChapters) < ChaptersInfo.Count)
                        {
                            int newChapters = ChaptersInfo.Count - (getSavedChapters + getUnreadChapters);
                            library.ChangeUnreadChapters(item.Name, item.Website, string.Format("{0}", newChapters + getUnreadChapters));
                            item.UnreadChapters = newChapters + getUnreadChapters;

                            MoveItemTopEnd(true);
                        }
                    }
                }
                library.Save();

                tvLibraryUpdateText.Visibility = Visibility.Collapsed;
                tvLibraryUpdateProgress.Visibility = Visibility.Collapsed;
                tvLibraryUpdateProgress.Value = 0;
            }
            else
            {
            }
        }
    }
}
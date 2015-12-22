using System;
using System.Linq;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System.Reflection; //Assembly
using DropNet;


namespace Mija_Reader
{
    public partial class MainWindow : Window
    {
        private const string _apiKey = "1d17vae78l691bl";
        private const string _appsecret = "8gn5q15w1fy3gpm";
        DropNetClient _client = null;
        Core.Ini MyIni = null;

        private ObservableCollection<dynamic> _Languages = new ObservableCollection<dynamic>();
        public ObservableCollection<dynamic> Languages
        {
            get { return _Languages; }
            set { _Languages = value; }
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

            #region InitializeDropBox
            _client = new DropNetClient(_apiKey, _appsecret);
            // Sync
            _client.GetToken();
            // Async
            _client.GetTokenAsync((userLogin) =>
            {
                //Dont really need to do anything with userLogin, DropNet takes care of it for now
            },
            (error) =>
            {
                //Handle error
            });
            var url = _client.BuildAuthorizeUrl();

            loginBrowser.Navigate(url);
            loginBrowser.LoadCompleted += Browser_LoadCompleted;
            #endregion
        }

        private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            dynamic doc = (sender as WebBrowser).Document;
            var htmlText = doc.documentElement.OuterText;

            string browserContents = doc.Body.InnerText;

            if(browserContents.Contains(SelectedLanguage.WebBrowserSucces))
            {
                _client.GetAccessTokenAsync((accessToken) =>
                {
                    //Store this token for "remember me" function
                    if (accessToken != null)
                    {
                        //Store this token for "remember me" function
                    }
                },
                (error) =>
                {
                    //Handle error
                    MessageBox.Show(error.Message);
                });
            }
        }

        private void c_LanguageCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic c = Languages.ElementAt((sender as ComboBox).SelectedIndex);
            SelectedLanguage.LanguageName = c.LanguageName;
            SelectedLanguage.AuthorName = c.AuthorName;
            SelectedLanguage.Reading = c.Reading;
            SelectedLanguage.Finished = c.Finished;
            SelectedLanguage.Abandoned = c.Abandoned;
            SelectedLanguage.WebBrowserSucces = c.WebBrowserSucces;

            MyIni.Write("Language", SelectedLanguage.LanguageName, "WindowData");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel; //INotifyPropertyChanged

namespace Mija_Reader.Core
{
    public class BaseLanguage : INotifyPropertyChanged
    {
        private string _LanguageName = "English";
        public string LanguageName
        {
            get { return _LanguageName; }
            set { _LanguageName = value; RaisePropertyChanged("LanguageName"); }
        }
        private string _LanguageString = "Language";
        public string LanguageString
        {
            get { return _LanguageString; }
            set { _LanguageString = value; RaisePropertyChanged("LanguageString"); }
        }
        private string _AuthorName = "Michał Jachura";
        public string AuthorName
        {
            get { return _AuthorName; }
            set { _AuthorName = value; RaisePropertyChanged("AuthorName"); }
        }
        private string _Reading = "Reading";
        public string Reading
        {
            get { return _Reading; }
            set { _Reading = value; RaisePropertyChanged("Reading"); }
        }
        private string _Finished = "Finished";
        public string Finished
        {
            get { return _Finished; }
            set { _Finished = value; RaisePropertyChanged("Finished"); }
        }
        private string _Abandoned = "Abandoned";
        public string Abandoned
        {
            get { return _Abandoned; }
            set { _Abandoned = value; RaisePropertyChanged("Abandoned"); }
        }
        private string _WebBrowserSucces = "Udało się! Mija Reader jest teraz połączona z Twoim kontem Dropbox.";
        public string WebBrowserSucces
        {
            get { return _WebBrowserSucces; }
            set { _WebBrowserSucces = value; RaisePropertyChanged("WebBrowserSucces"); }
        }
        private string _Login = "Login";
        public string Login
        {
            get { return _Login; }
            set { _Login = value; RaisePropertyChanged("Login"); }
        }
        private string _Home = "Home";
        public string Home
        {
            get { return _Home; }
            set { _Home = value; RaisePropertyChanged("Home"); }
        }
        private string _Library = "Library";
        public string Library
        {
            get { return _Library; }
            set { _Library = value; RaisePropertyChanged("Library"); }
        }
        private string _Chapters = "Chapters";
        public string Chapters
        {
            get { return _Chapters; }
            set { _Chapters = value; RaisePropertyChanged("Chapters"); }
        }
        private string _Reader = "Reader";
        public string Reader
        {
            get { return _Reader; }
            set { _Reader = value; RaisePropertyChanged("Reader"); }
        }
        private string _Settings = "Settings";
        public string Settings
        {
            get { return _Settings; }
            set { _Settings = value; RaisePropertyChanged("Settings"); }
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region Methods
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
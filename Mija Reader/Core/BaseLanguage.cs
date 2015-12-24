﻿using System;
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
        private string _LastRead = "Last read:";
        public string LastRead
        {
            get { return _LastRead; }
            set { _LastRead = value; RaisePropertyChanged("LastRead"); }
        }
        private string _NewChapters = "New chapters:";
        public string NewChapters
        {
            get { return _NewChapters; }
            set { _NewChapters = value; RaisePropertyChanged("NewChapters"); }
        }
        private string _Search = "Search:";
        public string Search
        {
            get { return _Search; }
            set { _Search = value; RaisePropertyChanged("Search"); }
        }
        private string _Information = "Information";
        public string Information
        {
            get { return _Information; }
            set { _Information = value; RaisePropertyChanged("Information"); }
        }
        private string _Warning = "Warning";
        public string Warning
        {
            get { return _Warning; }
            set { _Warning = value; RaisePropertyChanged("Warning"); }
        }
        private string _Error = "Error";
        public string Error
        {
            get { return _Error; }
            set { _Error = value; RaisePropertyChanged("Error"); }
        }
        private string _Yes = "Yes";
        public string Yes
        {
            get { return _Yes; }
            set { _Yes = value; RaisePropertyChanged("Yes"); }
        }
        private string _No = "No";
        public string No
        {
            get { return _No; }
            set { _No = value; RaisePropertyChanged("No"); }
        }
        private string _Cancel = "Cancel";
        public string Cancel
        {
            get { return _Cancel; }
            set { _Cancel = value; RaisePropertyChanged("Cancel"); }
        }
        private string _Ok = "Ok";
        public string Ok
        {
            get { return _Ok; }
            set { _Ok = value; RaisePropertyChanged("Ok"); }
        }
        private string _MangaAlreadyExist = "Manga {0} is already exist in your library. You can Find it in {1}";
        public string MangaAlreadyExist
        {
            get { return _MangaAlreadyExist; }
            set { _MangaAlreadyExist = value; RaisePropertyChanged("MangaAlreadyExist"); }
        }
        private string _SomethingWentWrong = "Something went wrong while searching.";
        public string SomethingWentWrong
        {
            get { return _SomethingWentWrong; }
            set { _SomethingWentWrong = value; RaisePropertyChanged("SomethingWentWrong"); }
        }
        private string _NoSelectedSource = "There isn't any manga source selected. If you can't select any, that mean you don't have any plugins instaled. Please reinstall program or add sources manually into proper folder.";
        public string NoSelectedSource
        {
            get { return _NoSelectedSource; }
            set { _NoSelectedSource = value; RaisePropertyChanged("NoSelectedSource"); }
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
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
        private string _ShowChapters = "Show chapters";
        public string ShowChapters
        {
            get { return _ShowChapters; }
            set { _ShowChapters = value; RaisePropertyChanged("ShowChapters"); }
        }
        private string _Move = "Move:";
        public string Move
        {
            get { return _Move; }
            set { _Move = value; RaisePropertyChanged("Move"); }
        }
        private string _MoveTo = "Move to:";
        public string MoveTo
        {
            get { return _MoveTo; }
            set { _MoveTo = value; RaisePropertyChanged("MoveTo"); }
        }
        private string _Up = "Up";
        public string Up
        {
            get { return _Up; }
            set { _Up = value; RaisePropertyChanged("Up"); }
        }
        private string _Down = "Down";
        public string Down
        {
            get { return _Down; }
            set { _Down = value; RaisePropertyChanged("Down"); }
        }
        private string _Top = "beginning";
        public string Top
        {
            get { return _Top; }
            set { _Top = value; RaisePropertyChanged("Top"); }
        }
        private string _End = "end";
        public string End
        {
            get { return _End; }
            set { _End = value; RaisePropertyChanged("End"); }
        }
        private string _RemoveFromLibrary = "Remove from library";
        public string RemoveFromLibrary
        {
            get { return _RemoveFromLibrary; }
            set { _RemoveFromLibrary = value; RaisePropertyChanged("RemoveFromLibrary"); }
        }
        private string _ViewOnline = "View online";
        public string ViewOnline
        {
            get { return _ViewOnline; }
            set { _ViewOnline = value; RaisePropertyChanged("ViewOnline"); }
        }
        private string _MarkAsRead = "Mark as read";
        public string MarkAsRead
        {
            get { return _MarkAsRead; }
            set { _MarkAsRead = value; RaisePropertyChanged("MarkAsRead"); }
        }
        private string _MarkAllPreviousAsRead = "Mark all previous as read";
        public string MarkAllPreviousAsRead
        {
            get { return _MarkAllPreviousAsRead; }
            set { _MarkAllPreviousAsRead = value; RaisePropertyChanged("MarkAllPreviousAsRead"); }
        }
        private string _ChaptersDirection = "Displaying direction";
        public string ChaptersDirection
        {
            get { return _ChaptersDirection; }
            set { _ChaptersDirection = value; RaisePropertyChanged("ChaptersDirection"); }
        }
        private string _FromStartToEnd = "From beginning to end";
        public string FromStartToEnd
        {
            get { return _FromStartToEnd; }
            set { _FromStartToEnd = value; RaisePropertyChanged("FromStartToEnd"); }
        }
        private string _FromEndToStart = "From end to beginning";
        public string FromEndToStart
        {
            get { return _FromEndToStart; }
            set { _FromEndToStart = value; RaisePropertyChanged("FromEndToStart"); }
        }
        private string _UploadSucces = "Library was uploaded succesfully";
        public string UploadSucces
        {
            get { return _UploadSucces; }
            set { _UploadSucces = value; RaisePropertyChanged("UploadSucces"); }
        }
        private string _DownloadSuccesAndReplaced = "Library was downloaded succesfully and replaced old one";
        public string DownloadSuccesAndReplaced
        {
            get { return _DownloadSuccesAndReplaced; }
            set { _DownloadSuccesAndReplaced = value; RaisePropertyChanged("DownloadSuccesAndReplaced"); }
        }
        private string _UploadedFileIsSameAsOurs = "Uploaded library is same as one on disk";
        public string UploadedFileIsSameAsOurs
        {
            get { return _UploadedFileIsSameAsOurs; }
            set { _UploadedFileIsSameAsOurs = value; RaisePropertyChanged("UploadedFileIsSameAsOurs"); }
        }
        private string _LoadingImagesMessage = "Please wait till all images finish loading";
        public string LoadingImagesMessage
        {
            get { return _LoadingImagesMessage; }
            set { _LoadingImagesMessage = value; RaisePropertyChanged("LoadingImagesMessage"); }
        }
        private string _WindowTitle = "";
        public string WindowTitle
        {
            get { return _WindowTitle; }
            set { _WindowTitle = value; RaisePropertyChanged("WindowTitle"); }
        }
        private string _ClosePage = "Close";
        public string ClosePage
        {
            get { return _ClosePage; }
            set { _ClosePage = value; RaisePropertyChanged("ClosePage"); }
        }
        private string _NextImage = "Next image";
        public string NextImage
        {
            get { return _NextImage; }
            set { _NextImage = value; RaisePropertyChanged("NextImage"); }
        }
        private string _PrevImage = "Prev image";
        public string PrevImage
        {
            get { return _PrevImage; }
            set { _PrevImage = value; RaisePropertyChanged("PrevImage"); }
        }
        private string _Page = "Page";
        public string Page
        {
            get { return _Page; }
            set { _Page = value; RaisePropertyChanged("Page"); }
        }
        private string _CheckForNewChaptersMessage = "Please wait till program finish checking for new chapters";
        public string CheckForNewChaptersMessage
        {
            get { return _CheckForNewChaptersMessage; }
            set { _CheckForNewChaptersMessage = value; RaisePropertyChanged("CheckForNewChaptersMessage"); }
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
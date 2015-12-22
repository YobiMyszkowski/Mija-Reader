using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel; //INotifyPropertyChanged

namespace Mija_Reader.Core
{
    public class BaseLanguage: INotifyPropertyChanged
    {
        private string _LanguageName = "";
        public string LanguageName
        {
            get { return _LanguageName; }
            set { _LanguageName = value; RaisePropertyChanged("LanguageName"); }
        }
        private string _AuthorName = "";
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
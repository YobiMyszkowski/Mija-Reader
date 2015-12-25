namespace Language
{
    public class Lang
    {
        public string LanguageName { get { return "English"; } }
        public string AuthorName { get { return "Michał Jachura"; } }

        public string LanguageString { get { return "Language"; } }

        public string Reading { get { return "Reading"; } }
        public string Finished { get { return "Completed"; } }
        public string Abandoned { get { return "Abandoned"; } }
        public string WebBrowserSucces { get { return "Udało się! Mija Reader jest teraz połączona z Twoim kontem Dropbox."; } }

        public string Login { get { return "Login"; } }
        public string Home { get { return "Home"; } }
        public string Library { get { return "Library"; } }
        public string Chapters { get { return "Chapters"; } }
        public string Reader { get { return "Reader"; } }
        public string Settings { get { return "Settings"; } }

        public string LastRead { get { return "Last read:"; } }
        public string NewChapters { get { return "Last updated:"; } }
        public string Search { get { return "Search:"; } }

        public string Information { get { return "Information"; } }
        public string Warning { get { return "Warning"; } }
        public string Error { get { return "Error"; } }
        public string Yes { get { return "Yes"; } }
        public string No { get { return "No"; } }
        public string Cancel { get { return "Cancel"; } }
        public string Ok { get { return "Ok"; } }
        public string MangaAlreadyExist { get { return "Manga '{0}' is already exist in your library. You can Find it in '{1}'."; } }
        public string SomethingWentWrong { get { return "Something went wrong while searching."; } }
        public string NoSelectedSource { get { return "There isn't any manga source selected. If you can't select any, that mean plugins wan't installed succesfully. Please reinstall program or add sources manually into proper folder."; } }

        public string ShowChapters { get { return "Show chapters"; } }
        public string Move { get { return "Move:"; } }
        public string MoveTo { get { return "Move to:"; } }
        public string Up { get { return "up"; } }
        public string Down { get { return "down"; } }
        public string Top { get { return "beginning"; } }
        public string End { get { return "end"; } }
        public string RemoveFromLibrary { get { return "Remove from library"; } }
        public string ViewOnline { get { return "View online"; } }
        public string MarkAsRead { get { return "Mark as read"; } }
        public string MarkAllPreviousAsRead { get { return "Mark all previous as read"; } }
        public string ChaptersDirection { get { return "Chapters direction"; } }
        public string FromStartToEnd { get { return "From beginning to end"; } }
        public string FromEndToStart { get { return "From end to beginning"; } }
        public string UploadSucces { get { return "Library was uploaded succesfully"; } }
        public string DownloadSuccesAndReplaced { get { return "Library was downloaded succesfully and replaced ours"; } }
        public string UploadedFileIsSameAsOurs { get { return "Uploaded library is same as one saved on drive"; } }
    }
}

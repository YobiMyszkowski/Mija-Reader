﻿using System;
using System.Linq;
using System.Xml;
using System.Collections.ObjectModel;
using BaseMangaSource;

namespace System.Xml
{
    public static class MyExt2
    {
        public static void InsertAt(this XmlNode node, XmlNode insertingNode, int index = 0)
        {
            if (insertingNode == null)
                return;
            if (index < 0)
                index = 0;

            var childNodes = node.ChildNodes;
            var childrenCount = childNodes.Count;

            if (index >= childrenCount)
            {
                node.AppendChild(insertingNode);
                return;
            }

            var followingNode = childNodes[index];

            node.InsertBefore(insertingNode, followingNode);
        }
    }
}

namespace Mija_Reader.Core
{
    public enum PlaceInLibrary
    {
        Reading,
        Finished,
        Abandoned
    };
    public partial class XMLLibrary
    {
        private string name_;
        private XmlDocument doc_;
        public XMLLibrary(string Name)
        {
            name_ = Name;
            doc_ = new XmlDocument();
        }
        public void Create()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter writer = XmlWriter.Create(name_, settings);
            writer.WriteStartDocument();

            writer.WriteComment("This file is generated by the program.");

            writer.WriteStartElement("Mangas");

            writer.WriteStartElement("Reading");
            writer.WriteEndElement();
            writer.WriteStartElement("Finished");
            writer.WriteEndElement();
            writer.WriteStartElement("Abandoned");
            writer.WriteEndElement();

            writer.WriteEndElement(); // </Mangas>

            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }
        public void Load()
        {
            XmlDocument xmldoc = new XmlDocument();
            System.IO.FileStream fs = new System.IO.FileStream(name_, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            xmldoc.Load(fs);
            fs.Close();
            doc_ = xmldoc;
        }
        public void Save()
        {
            doc_.Save(name_);
        }
        public XmlNode GetMangasNode()
        {
            return doc_.SelectSingleNode("Mangas");
        }
        public bool MarkChapter(string Name, string Website, string ChapterName, string ChapterWebsite, bool MarkOrUnmark)
        {
            if (!IsChapterExistInManga(Name, Website, ChapterName, ChapterWebsite))
            {
                return false;
            }
            else
            {
                foreach (XmlNode node in GetManga(Name, Website))
                {
                    if (node.Name == "Chapters" && node.ChildNodes.Count > 0)
                    {
                        foreach (XmlNode node2 in node)
                        {
                            if (node2.Name == "Chapter" && node2.Attributes["Name"].Value == ChapterName && node2.Attributes["Website"].Value == ChapterWebsite)
                            {
                                node2.Attributes["MarkedAsRead"].Value = string.Format("{0}", MarkOrUnmark);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
        public bool IsChapterMarkedAsReadInManga(string Name, string Website, string ChapterName, string ChapterWebsite)
        {
            if (!IsChapterExistInManga(Name, Website, ChapterName, ChapterWebsite))
            {
                return false;
            }
            else
            {
                foreach (XmlNode node in GetManga(Name, Website))
                {
                    if (node.Name == "Chapters" && node.ChildNodes.Count > 0)
                    {
                        foreach (XmlNode node2 in node)
                        {
                            if (node2.Name == "Chapter" && node2.Attributes["Name"].Value == ChapterName && node2.Attributes["Website"].Value == ChapterWebsite)
                            {
                                if (node2.Attributes["MarkedAsRead"].Value == "True")
                                {
                                    return true;
                                }
                                return false;
                            }
                        }
                    }
                }
                return false;
            }
        }
        public bool IsChapterExistInManga(string Name, string Website, string ChapterName, string ChapterWebsite)
        {
            if (!IsMangaExist(Name, Website))
            {
                return false;
            }
            else
            {
                foreach (XmlNode node in GetManga(Name, Website))
                {
                    if (node.Name == "Chapters" && node.ChildNodes.Count > 0)
                    {
                        foreach (XmlNode node2 in node)
                        {
                            if (node2.Name == "Chapter" && node2.Attributes["Name"].Value == ChapterName && node2.Attributes["Website"].Value == ChapterWebsite)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
        public bool AddChapterToManga(string Name, string Website, string ChapterName, string ChapterWebsite, bool MarkedAsRead)
        {
            if (!IsChapterExistInManga(Name, Website, ChapterName, ChapterWebsite))
            {
                foreach (XmlNode node in GetManga(Name, Website))
                {
                    if (node.Name == "Chapters")
                    {
                        XmlNode manga = doc_.CreateNode(XmlNodeType.Element, "Chapter", null);
                        XmlAttribute nameAttr = doc_.CreateAttribute("Name");
                        nameAttr.Value = ChapterName;
                        XmlAttribute websiteAttr = doc_.CreateAttribute("Website");
                        websiteAttr.Value = ChapterWebsite;
                        XmlAttribute markedAttr = doc_.CreateAttribute("MarkedAsRead");
                        markedAttr.Value = string.Format("{0}", MarkedAsRead);

                        manga.Attributes.Append(nameAttr);
                        manga.Attributes.Append(websiteAttr);
                        manga.Attributes.Append(markedAttr);

                        node.AppendChild(manga);

                        return true;
                    }
                }
            }
            return false;
        }
        public bool MangaContainsChapters(string Name, string Website)
        {
            if (IsMangaExist(Name, Website))
            {
                return false;
            }
            else
            {
                foreach (XmlNode node in GetManga(Name, Website))
                {
                    if (node.Name == "Chapters" && node.ChildNodes.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool AddManga(ObservableCollection<MangaPageData> DetailedInfo, PlaceInLibrary place)
        {
            if (IsMangaExist(DetailedInfo.FirstOrDefault().Name, DetailedInfo.FirstOrDefault().Website))
            {
                return false;
            }
            else
            {
                XmlNode manga = doc_.CreateNode(XmlNodeType.Element, "Manga", null);
                XmlAttribute nameAttr = doc_.CreateAttribute("Name");
                nameAttr.Value = DetailedInfo.FirstOrDefault().Name;
                XmlAttribute websiteAttr = doc_.CreateAttribute("Website");
                websiteAttr.Value = DetailedInfo.FirstOrDefault().Website;

                manga.Attributes.Append(nameAttr);
                manga.Attributes.Append(websiteAttr);

                if (place == PlaceInLibrary.Reading)
                {
                    doc_.SelectSingleNode("Mangas").ChildNodes[0].AppendChild(manga);
                }
                else if (place == PlaceInLibrary.Finished)
                {
                    doc_.SelectSingleNode("Mangas").ChildNodes[1].AppendChild(manga);
                }
                else if (place == PlaceInLibrary.Abandoned)
                {
                    doc_.SelectSingleNode("Mangas").ChildNodes[2].AppendChild(manga);
                }
                else
                {
                    return false;
                }

                XmlNode mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Image", null);
                mangaChilds.InnerText = DetailedInfo.FirstOrDefault().Image;
                manga.AppendChild(mangaChilds);

                mangaChilds = doc_.CreateNode(XmlNodeType.Element, "YearOfRelease", null);
                mangaChilds.InnerText = DetailedInfo.FirstOrDefault().YearOfRelease;
                manga.AppendChild(mangaChilds);

                mangaChilds = doc_.CreateNode(XmlNodeType.Element, "AlternateName", null);
                mangaChilds.InnerText = DetailedInfo.FirstOrDefault().AlternateName;
                manga.AppendChild(mangaChilds);

                mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Author", null);
                mangaChilds.InnerText = DetailedInfo.FirstOrDefault().Author;
                manga.AppendChild(mangaChilds);

                mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Artist", null);
                mangaChilds.InnerText = DetailedInfo.FirstOrDefault().Artist;
                manga.AppendChild(mangaChilds);

                mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Genre", null);
                mangaChilds.InnerText = DetailedInfo.FirstOrDefault().Genre;
                manga.AppendChild(mangaChilds);

                if (DetailedInfo.FirstOrDefault().Type == MangaType.Manga_RightToLeft)
                {
                    mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Type", null);
                    mangaChilds.InnerText = "Manga_RightToLeft";
                    manga.AppendChild(mangaChilds);
                }
                else
                {
                    mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Type", null);
                    mangaChilds.InnerText = "Manhwa_LeftToRight";
                    manga.AppendChild(mangaChilds);
                }


                if (DetailedInfo.FirstOrDefault().Status == MangaStatus.Completed)
                {
                    mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Status", null);
                    mangaChilds.InnerText = "Completed";
                    manga.AppendChild(mangaChilds);
                }
                else
                {
                    mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Status", null);
                    mangaChilds.InnerText = "Ongoing";
                    manga.AppendChild(mangaChilds);
                }

                mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Description", null);
                mangaChilds.InnerText = DetailedInfo.FirstOrDefault().Description;
                manga.AppendChild(mangaChilds);

                mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Plugin", null);
                mangaChilds.InnerText = DetailedInfo.FirstOrDefault().UrlToMainpage;
                manga.AppendChild(mangaChilds);

                mangaChilds = doc_.CreateNode(XmlNodeType.Element, "UnreadChapters", null);
                mangaChilds.InnerText = string.Format("{0}", DetailedInfo.FirstOrDefault().UnreadChapters);
                manga.AppendChild(mangaChilds);

                mangaChilds = doc_.CreateNode(XmlNodeType.Element, "Chapters", null);
                manga.AppendChild(mangaChilds);

                doc_.Save(name_);

                return true;
            }
        }
        public bool IsMangaExist(string Name, string Website)
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (XmlNode node in doc_.SelectSingleNode("Mangas").ChildNodes[i])
                {
                    if (node.Name == "Manga")
                    {
                        if (node.Attributes != null)
                        {
                            var nameAttribute = node.Attributes["Name"];
                            var websiteAttribute = node.Attributes["Website"];
                            if (nameAttribute.Value == Name && websiteAttribute.Value == Website)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool MoveMangaUpDown(string Name, string Website, bool up)
        {
            if (!IsMangaExist(Name, Website))
            {
                return false;
            }
            else
            {
                if (up == true)
                {
                    XmlNode copied = GetManga(Name, Website);
                    copied.ParentNode.InsertBefore(copied, copied.PreviousSibling);
                    doc_.Save(name_);

                    return true;
                }
                else
                {
                    XmlNode copied = GetManga(Name, Website);
                    copied.ParentNode.InsertAfter(copied, copied.NextSibling);
                    doc_.Save(name_);

                    return true;
                }
            }
        }
        public bool MoveMangaTopEnd(string Name, string Website, bool top)
        {
            if (!IsMangaExist(Name, Website))
            {
                return false;
            }
            else
            {
                if (top == true)
                {
                    XmlNode copied = GetManga(Name, Website);
                    copied.ParentNode.InsertAfter(copied, copied.ParentNode.LastChild);
                    doc_.Save(name_);

                    return true;
                }
                else
                {
                    XmlNode copied = GetManga(Name, Website);
                    copied.ParentNode.InsertBefore(copied, copied.ParentNode.FirstChild);
                    doc_.Save(name_);

                    return true;
                }
            }
        }
        public bool MoveMangaToIndex(string Name, string Website, int CurrentIndex, int DestinationIndex)
        {
            if (!IsMangaExist(Name, Website))
            {
                return false;
            }
            else
            {
                XmlNode copied = GetManga(Name, Website);

                if (CurrentIndex < DestinationIndex)
                {
                    //after
                    var childNodes = copied.ParentNode.ChildNodes;
                    copied.ParentNode.InsertAfter(copied, childNodes[DestinationIndex]);
                }
                else
                {
                    //before
                    var childNodes = copied.ParentNode.ChildNodes;
                    copied.ParentNode.InsertBefore(copied, childNodes[DestinationIndex]);
                }

                doc_.Save(name_);

                return true;
            }
        }
        public XmlNode GetManga(string Name, string Website)
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (XmlNode node in doc_.SelectSingleNode("Mangas").ChildNodes[i])
                {
                    if (node.Name == "Manga")
                    {
                        if (node.Attributes != null)
                        {
                            var nameAttribute = node.Attributes["Name"];
                            var websiteAttribute = node.Attributes["Website"];
                            if (nameAttribute.Value == Name && websiteAttribute.Value == Website)
                            {
                                return node;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public void CreateFolder()
        {
            System.IO.Directory.CreateDirectory(@"Data");
        }
        public bool IsFolderExist()
        {
            if (System.IO.Directory.Exists(@"Data"))
            {
                return true;
            }
            return false;
        }
        public bool IsFileExist()
        {
            if (System.IO.File.Exists(name_))
            {
                return true;
            }
            return false;
        }
        public bool RemoveMangaAndAllData(string Name, string Website)
        {
            if (!IsMangaExist(Name, Website))
            {
                return false;
            }
            else
            {
                XmlNode eleToRemove = GetManga(Name, Website);
                if (eleToRemove != null)
                {
                    eleToRemove.ParentNode.RemoveChild(eleToRemove);
                    doc_.Save(name_);
                    return true;
                }
                return false;
            }
        }
        public bool LoadLibrary(ObservableCollection<MangaPageData> ReadingData, ObservableCollection<MangaPageData> FinishedData, ObservableCollection<MangaPageData> AbandonedData)
        {
            XmlNode topNode = doc_.SelectSingleNode("Mangas");
            if (topNode.HasChildNodes)
            {
                foreach (XmlNode mangaNode in topNode.ChildNodes[0].ChildNodes)
                {
                    if (mangaNode.Name == "Manga")
                    {
                        if (mangaNode.Attributes != null)
                        {
                            var nameAttribute = mangaNode.Attributes["Name"];
                            var websiteAttribute = mangaNode.Attributes["Website"];
                            string Image = "";
                            string YearOfRelease = "";
                            string AlternateName = "";
                            string Author = "";
                            string Artist = "";
                            string Genre = "";
                            string Website = "";
                            string Description = "";
                            MangaStatus status = MangaStatus.Completed;
                            MangaType type = MangaType.Manga_RightToLeft;
                            int UnreadChapters = 0;
                            foreach (XmlNode mangaNodeData in mangaNode.ChildNodes)
                            {
                                if (mangaNodeData.Name == "Image")
                                {
                                    Image = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "YearOfRelease")
                                {
                                    YearOfRelease = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "AlternateName")
                                {
                                    AlternateName = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Author")
                                {
                                    Author = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Artist")
                                {
                                    Artist = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Status")
                                {
                                    if (mangaNodeData.InnerText == "Completed")
                                        status = MangaStatus.Completed;
                                    else
                                        status = MangaStatus.Ongoing;
                                }
                                if (mangaNodeData.Name == "Genre")
                                {
                                    Genre = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Type")
                                {
                                    if (mangaNodeData.InnerText == "Manga_RightToLeft")
                                        type = MangaType.Manga_RightToLeft;
                                    else
                                        type = MangaType.Manhwa_LeftToRight;
                                }
                                if (mangaNodeData.Name == "Plugin")
                                {
                                    Website = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Desription")
                                {
                                    Description = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "UnreadChapters")
                                {
                                    UnreadChapters = Int32.Parse(mangaNodeData.InnerText);
                                }
                            }
                            ReadingData.Add(new MangaPageData
                            {
                                Name = nameAttribute.Value,
                                UrlToMainpage = Website,
                                Image = Image,
                                YearOfRelease = YearOfRelease,
                                AlternateName = AlternateName,
                                Artist = Artist,
                                Author = Author,
                                Status = status,
                                Genre = Genre,
                                Type = type,
                                Website = websiteAttribute.Value,
                                Description = Description,
                                UnreadChapters = UnreadChapters
                            });
                        }
                    }
                }
                foreach (XmlNode mangaNode in topNode.ChildNodes[1].ChildNodes)
                {
                    if (mangaNode.Name == "Manga")
                    {
                        if (mangaNode.Attributes != null)
                        {
                            var nameAttribute = mangaNode.Attributes["Name"];
                            var websiteAttribute = mangaNode.Attributes["Website"];
                            string Image = "";
                            string YearOfRelease = "";
                            string AlternateName = "";
                            string Author = "";
                            string Artist = "";
                            string Genre = "";
                            string Website = "";
                            string Description = "";
                            MangaStatus status = MangaStatus.Completed;
                            MangaType type = MangaType.Manga_RightToLeft;
                            int UnreadChapters = 0;
                            foreach (XmlNode mangaNodeData in mangaNode.ChildNodes)
                            {
                                if (mangaNodeData.Name == "Image")
                                {
                                    Image = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "YearOfRelease")
                                {
                                    YearOfRelease = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "AlternateName")
                                {
                                    AlternateName = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Author")
                                {
                                    Author = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Artist")
                                {
                                    Artist = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Status")
                                {
                                    if (mangaNodeData.InnerText == "Completed")
                                        status = MangaStatus.Completed;
                                    else
                                        status = MangaStatus.Ongoing;
                                }
                                if (mangaNodeData.Name == "Genre")
                                {
                                    Genre = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Type")
                                {
                                    if (mangaNodeData.InnerText == "Manga_RightToLeft")
                                        type = MangaType.Manga_RightToLeft;
                                    else
                                        type = MangaType.Manhwa_LeftToRight;
                                }
                                if (mangaNodeData.Name == "Website")
                                {
                                    Website = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Desription")
                                {
                                    Description = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "UnreadChapters")
                                {
                                    UnreadChapters = Int32.Parse(mangaNodeData.InnerText);
                                }
                            }
                            FinishedData.Add(new MangaPageData
                            {
                                Name = nameAttribute.Value,
                                UrlToMainpage = Website,
                                Image = Image,
                                YearOfRelease = YearOfRelease,
                                AlternateName = AlternateName,
                                Artist = Artist,
                                Author = Author,
                                Status = status,
                                Genre = Genre,
                                Type = type,
                                Website = websiteAttribute.Value,
                                Description = Description,
                                UnreadChapters = UnreadChapters
                            });
                        }
                    }
                }
                foreach (XmlNode mangaNode in topNode.ChildNodes[2].ChildNodes)
                {
                    if (mangaNode.Name == "Manga")
                    {
                        if (mangaNode.Attributes != null)
                        {
                            var nameAttribute = mangaNode.Attributes["Name"];
                            var websiteAttribute = mangaNode.Attributes["Website"];
                            string Image = "";
                            string YearOfRelease = "";
                            string AlternateName = "";
                            string Author = "";
                            string Artist = "";
                            string Genre = "";
                            string Website = "";
                            string Description = "";
                            MangaStatus status = MangaStatus.Completed;
                            MangaType type = MangaType.Manga_RightToLeft;
                            int UnreadChapters = 0;
                            foreach (XmlNode mangaNodeData in mangaNode.ChildNodes)
                            {
                                if (mangaNodeData.Name == "Image")
                                {
                                    Image = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "YearOfRelease")
                                {
                                    YearOfRelease = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "AlternateName")
                                {
                                    AlternateName = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Author")
                                {
                                    Author = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Artist")
                                {
                                    Artist = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Status")
                                {
                                    if (mangaNodeData.InnerText == "Completed")
                                        status = MangaStatus.Completed;
                                    else
                                        status = MangaStatus.Ongoing;
                                }
                                if (mangaNodeData.Name == "Genre")
                                {
                                    Genre = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Type")
                                {
                                    if (mangaNodeData.InnerText == "Manga_RightToLeft")
                                        type = MangaType.Manga_RightToLeft;
                                    else
                                        type = MangaType.Manhwa_LeftToRight;
                                }
                                if (mangaNodeData.Name == "Website")
                                {
                                    Website = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "Desription")
                                {
                                    Description = mangaNodeData.InnerText;
                                }
                                if (mangaNodeData.Name == "UnreadChapters")
                                {
                                    UnreadChapters = Int32.Parse(mangaNodeData.InnerText);
                                }
                            }
                            AbandonedData.Add(new MangaPageData
                            {
                                Name = nameAttribute.Value,
                                UrlToMainpage = Website,
                                Image = Image,
                                YearOfRelease = YearOfRelease,
                                AlternateName = AlternateName,
                                Artist = Artist,
                                Author = Author,
                                Status = status,
                                Genre = Genre,
                                Type = type,
                                Website = websiteAttribute.Value,
                                Description = Description,
                                UnreadChapters = UnreadChapters
                            });
                        }
                    }
                }
                return true;
            }
            return false;
        }
        public bool ChangePlace(string Name, string Website, PlaceInLibrary place)
        {
            if (!IsMangaExist(Name, Website))
            {
                return false;
            }
            else
            {
                if (place == PlaceInLibrary.Reading)
                {
                    XmlNode copied = GetManga(Name, Website);
                    doc_.SelectSingleNode("Mangas").ChildNodes[0].InsertAfter(copied, doc_.SelectSingleNode("Mangas").ChildNodes[0].LastChild);
                    doc_.Save(name_);

                    return true;
                }
                else if (place == PlaceInLibrary.Finished)
                {
                    XmlNode copied = GetManga(Name, Website);
                    doc_.SelectSingleNode("Mangas").ChildNodes[1].InsertAfter(copied, doc_.SelectSingleNode("Mangas").ChildNodes[1].LastChild);
                    doc_.Save(name_);

                    return true;
                }
                else if (place == PlaceInLibrary.Abandoned)
                {
                    XmlNode copied = GetManga(Name, Website);
                    doc_.SelectSingleNode("Mangas").ChildNodes[2].InsertAfter(copied, doc_.SelectSingleNode("Mangas").ChildNodes[2].LastChild);
                    doc_.Save(name_);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool ChangeUnreadChapters(string Name, string Website, string UnreadChaptersChapter)
        {
            if (!IsMangaExist(Name, Website))
            {
                return false;
            }
            else
            {
                foreach (XmlNode node in GetManga(Name, Website))
                {
                    if (node.Name == "UnreadChapters")
                    {
                        node.InnerText = UnreadChaptersChapter;
                        return true;
                    }
                }
                return false;
            }
        }
        public int GetChaptersAmount(string Name, string Website)
        {
            if (!IsMangaExist(Name, Website))
            {
                return 0;
            }
            else
            {
                foreach (XmlNode node in GetManga(Name, Website))
                {
                    if (node.Name == "Chapters")
                    {
                        if (node.HasChildNodes)
                        {
                            return node.ChildNodes.Count;
                        }
                        return 0;
                    }
                }
                return 0;
            }
        }
        public string GetUnreadChapters(string Name, string Website)
        {
            if (!IsMangaExist(Name, Website))
            {
                return "0";
            }
            else
            {
                foreach (XmlNode node in GetManga(Name, Website))
                {
                    if (node.Name == "UnreadChapters")
                    {
                        return node.InnerText;
                    }
                }
                return "0";
            }
        }
        public string GetWebsite(string Name, string Website)
        {
            if (!IsMangaExist(Name, Website))
            {
                return "";
            }
            else
            {
                foreach (XmlNode node in GetManga(Name, Website))
                {
                    if (node.Name == "Website")
                    {
                        return node.InnerText;
                    }
                }
                return "";
            }
        }
        public string Path
        {
            get
            {
                return name_;
            }
        }
    }
}


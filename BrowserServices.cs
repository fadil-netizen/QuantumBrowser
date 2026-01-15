using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace QuantumBrowser
{
    // --- Data Models ---
    public class HistoryItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
    }

    public class BookmarkItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
    }

    public class DownloadItem
    {
        public string FileName { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public long BytesReceived { get; set; }
        public long TotalBytes { get; set; }
        public string State { get; set; } // "InProgress", "Completed", "Interrupted"
        public DateTime Date { get; set; }
    }

    public class SitePermissions
    {
        public bool Camera { get; set; }
        public bool Microphone { get; set; }
        public bool Location { get; set; }
        public bool Notifications { get; set; }
        public bool Sound { get; set; }

        public SitePermissions()
        {
            Camera = true;
            Microphone = true;
            Location = true;
            Notifications = true;
            Sound = true;
        }
    }

    public class SessionData
    {
        public List<string> OpenUrls { get; set; }
    }

    public class SavedPassword
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        
        [XmlIgnore]
        public string Password 
        { 
            get 
            {
                if (string.IsNullOrEmpty(EncryptedPassword)) return string.Empty;
                try
                {
                    byte[] protectedBytes = Convert.FromBase64String(EncryptedPassword);
                    byte[] bytes = System.Security.Cryptography.ProtectedData.Unprotect(protectedBytes, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    return System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch { return string.Empty; }
            }
            set 
            {
                if (string.IsNullOrEmpty(value)) 
                {
                    EncryptedPassword = null;
                    return;
                }
                try
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
                    byte[] protectedBytes = System.Security.Cryptography.ProtectedData.Protect(bytes, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    EncryptedPassword = Convert.ToBase64String(protectedBytes);
                }
                catch { }
            }
        }
    }

    // --- Services ---
    public static class BrowserServices
    {
        private static string DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuantumBrowser");

        public static List<HistoryItem> History { get; private set; }
        public static List<BookmarkItem> Bookmarks { get; private set; }
        public static List<DownloadItem> Downloads { get; private set; }
        public static List<SavedPassword> Passwords { get; private set; }
        public static List<ShortcutItem> Shortcuts { get; private set; }
        public static SessionData LastSession { get; private set; }
        public static Dictionary<string, SitePermissions> SitePermissionMap { get; private set; }

        static BrowserServices()
        {
            History = new List<HistoryItem>();
            Bookmarks = new List<BookmarkItem>();
            Downloads = new List<DownloadItem>();
            Passwords = new List<SavedPassword>();
            Shortcuts = new List<ShortcutItem>();
            LastSession = new SessionData();
            SitePermissionMap = new Dictionary<string, SitePermissions>();
            
            if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);
            LoadData();
        }

        public static void LoadData()
        {
            History = Load<List<HistoryItem>>("history.xml") ?? new List<HistoryItem>();
            Bookmarks = Load<List<BookmarkItem>>("bookmarks.xml") ?? new List<BookmarkItem>();
            Downloads = Load<List<DownloadItem>>("downloads.xml") ?? new List<DownloadItem>();
            Passwords = Load<List<SavedPassword>>("passwords.xml") ?? new List<SavedPassword>();
            Shortcuts = Load<List<ShortcutItem>>("shortcuts.xml") ?? new List<ShortcutItem>();
            if (Shortcuts.Count == 0)
            {
                Shortcuts.Add(new ShortcutItem { Title = "YouTube", Url = "https://www.youtube.com" });
                Shortcuts.Add(new ShortcutItem { Title = "Google", Url = "https://www.google.com" });
                Shortcuts.Add(new ShortcutItem { Title = "Gemini", Url = "https://gemini.google.com" });
                SaveShortcuts();
            }
            LastSession = Load<SessionData>("session.xml") ?? new SessionData();
        }

        public static void SaveHistory()
        {
            Save("history.xml", History);
        }

        public static void SaveBookmarks()
        {
            Save("bookmarks.xml", Bookmarks);
        }

        public static void SaveDownloads()
        {
            Save("downloads.xml", Downloads);
        }

        public static void SavePasswords()
        {
            Save("passwords.xml", Passwords);
        }

        public static void SaveLastSession()
        {
            Save("session.xml", LastSession);
        }

        private static T Load<T>(string filename)
        {
            try
            {
                string path = Path.Combine(DataDir, filename);
                if (File.Exists(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        return (T)serializer.Deserialize(fs);
                    }
                }
            }
            catch { }
            return default(T);
        }

        private static void Save<T>(string filename, T data)
        {
            try
            {
                string path = Path.Combine(DataDir, filename);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    serializer.Serialize(fs, data);
                }
            }
            catch { }
        }

        // --- Helpers ---
        public static void AddHistory(string title, string url)
        {
            if (string.IsNullOrEmpty(url) || url.StartsWith("quantum://") || url == "about:blank") return;
            
            // Remove dupe if recent
            for (int i = History.Count - 1; i >= 0; i--)
            {
                if (History[i].Url == url && (DateTime.Now - History[i].Date).TotalMinutes < 1)
                {
                    History.RemoveAt(i);
                }
            }
            
            History.Insert(0, new HistoryItem { Title = title, Url = url, Date = DateTime.Now });
            if (History.Count > 1000) History.RemoveRange(1000, History.Count - 1000); // Limit
            SaveHistory();
        }

        public static void AddDownload(DownloadItem item)
        {
            Downloads.Insert(0, item);
            SaveDownloads();
        }

        // --- Shortcuts ---

        public static void SaveShortcuts()
        {
            Save("shortcuts.xml", Shortcuts);
        }

        public static void AddShortcut(string title, string url)
        {
            Shortcuts.Add(new ShortcutItem { Title = title, Url = url });
            SaveShortcuts();
        }

        public static void RemoveShortcut(string url)
        {
            Shortcuts.RemoveAll(x => x.Url == url);
            SaveShortcuts();
        }
    }

    public class ShortcutItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }
}

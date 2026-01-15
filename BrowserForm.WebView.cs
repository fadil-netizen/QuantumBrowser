using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Net;

namespace QuantumBrowser
{
    // Fix CS1540: Helper class to access DoubleBuffered
    public class TabPanel : Panel
    {
        public TabPanel() 
        { 
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }
    }

    public class BrowserTab
    {
        public TabPanel TabPanel { get; set; }
        public Label TitleLabel { get; set; }
        public PictureBox FaviconBox { get; set; }
        public Button CloseButton { get; set; }
        public WebView2 WebView { get; set; }
        public string Title { get; set; }
        public string CurrentUrl { get; set; }
    }

    public partial class BrowserForm
    {
        // Tab Management
        private List<BrowserTab> tabs = new List<BrowserTab>();
        private BrowserTab activeTab = null;
        private List<NavLog> navLogs = new List<NavLog>();

        // Fullscreen State
        private bool _isFullScreen = false;
        private FormWindowState _prevWindowState = FormWindowState.Normal;
        private FormBorderStyle _prevBorderStyle = FormBorderStyle.Sizable;

        private class NavLog
        {
            public DateTime Time { get; set; }
            public string Type { get; set; } // Start, Redirect, Complete
            public string Url { get; set; }
            public string Status { get; set; }
        }
        
        // Helper to get current WebView
        private WebView2 CurrentWebView 
        { 
            get { return activeTab != null ? activeTab.WebView : null; } 
        }

        // Use this accessor to satisfy legacy code referencing 'webView'
        private WebView2 webView 
        { 
            get { return CurrentWebView; } 
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Save Session
                SessionData session = new SessionData();
                session.OpenUrls = new List<string>();
                foreach(var tab in tabs)
                {
                    if(!string.IsNullOrEmpty(tab.CurrentUrl) && tab.CurrentUrl != "about:blank")
                    {
                        session.OpenUrls.Add(tab.CurrentUrl);
                    }
                }
                BrowserServices.LastSession.OpenUrls = session.OpenUrls;
                BrowserServices.SaveLastSession();
            }
            catch {}
            
            base.OnFormClosing(e);
        }

        private void InitializeWebView()
        {
            // Initialize Core Logic (once)
            InitializeSettings();
            
            // Startup Logic
            string behavior = appConfig.ContainsKey("StartupBehavior") ? appConfig["StartupBehavior"] : "NewTab";
            
            if (behavior == "SpecificPage" && appConfig.ContainsKey("StartupPages") && !string.IsNullOrEmpty(appConfig["StartupPages"]))
            {
                 string[] pages = appConfig["StartupPages"].Split(',');
                 foreach(var p in pages) {
                     if(!string.IsNullOrWhiteSpace(p)) AddNewTab(p.Trim().StartsWith("http") ? p.Trim() : "http://" + p.Trim());
                 }
            }
            else if (behavior == "Continue")
            {
                if (BrowserServices.LastSession != null && BrowserServices.LastSession.OpenUrls != null && BrowserServices.LastSession.OpenUrls.Count > 0)
                {
                    foreach(var url in BrowserServices.LastSession.OpenUrls)
                    {
                        AddNewTab(url);
                    }
                }
                else
                {
                    // NewTab or default
                    AddNewTab(IsIncognito ? "quantum://home" : "home"); 
                }
            }
            else
            {
                // NewTab or default
                AddNewTab(IsIncognito ? "quantum://home" : "home");
            }
        }

        private void AddNewTab(string startUrl = "home")
        {
            // Create WebView Handle
            WebView2 newWebView = new WebView2();
            newWebView.Dock = DockStyle.Fill;
            newWebView.AllowExternalDrop = true;
            newWebView.Visible = false; // Default hidden
            // Ensure container exists (might be null during designer init, but we are runtime)
            if (webViewContainer != null) webViewContainer.Controls.Add(newWebView);

            BrowserTab tab = new BrowserTab
            {
                WebView = newWebView,
                Title = "New Tab",
                CurrentUrl = "about:blank"
            };

            // --- Custom Tab UI (Modern Chrome-like) ---
            TabPanel pnlTab = new TabPanel();
            pnlTab.Size = new Size(200, 34); 
            pnlTab.BackColor = Color.Transparent; 
            
            // 1. Favicon
            PictureBox pbFav = new PictureBox();
            pbFav.Size = new Size(16, 16);
            pbFav.Location = new Point(12, 9);
            pbFav.BackColor = Color.Transparent;
            pbFav.SizeMode = PictureBoxSizeMode.StretchImage;
            pnlTab.Controls.Add(pbFav);

            // 2. Title
            Label lblTitle = new Label();
            lblTitle.Text = "New Tab";
            lblTitle.Location = new Point(34, 0); 
            lblTitle.Size = new Size(130, 34);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.ForeColor = colorText;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Segoe UI", 9f);
            lblTitle.AutoEllipsis = true; 
            pnlTab.Controls.Add(lblTitle);

            // 3. Close Button
            Button btnClose = new Button();
            btnClose.Text = "✕";
            btnClose.Size = new Size(24, 24);
            btnClose.Location = new Point(170, 5);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = Color.Transparent;
            btnClose.ForeColor = colorIcon;
            btnClose.Font = new Font("Segoe UI", 8f);
            btnClose.Cursor = Cursors.Hand;
            
            // Close Hover
            btnClose.MouseEnter += (s, e) => { 
                btnClose.BackColor = Color.FromArgb(232, 17, 35); 
                btnClose.ForeColor = Color.White; 
            };
            btnClose.MouseLeave += (s, e) => { 
                btnClose.BackColor = Color.Transparent; 
                btnClose.ForeColor = colorIcon; 
            };
            btnClose.Click += (s, e) => CloseTab(tab);
            pnlTab.Controls.Add(btnClose);

            // Tab Click Events (Switch)
            EventHandler switchHandler = (s, e) => SwitchToTab(tab);
            pnlTab.Click += switchHandler;
            lblTitle.Click += switchHandler;
            pbFav.Click += switchHandler;

            // Custom Painting for Rounded Tab Shape
            pnlTab.Paint += (s, e) => {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Determine Color
                Color bgColor = (activeTab == tab) ? colorActiveTab : colorTabInactive;
                
                // Draw Shape (Rounded Top corners)
                int cornerRadius = 12;
                GraphicsPath path = new GraphicsPath();
                path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90); 
                path.AddLine(cornerRadius, 0, pnlTab.Width - cornerRadius, 0); 
                path.AddArc(pnlTab.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90); 
                path.AddLine(pnlTab.Width, cornerRadius, pnlTab.Width, pnlTab.Height); 
                path.AddLine(pnlTab.Width, pnlTab.Height, 0, pnlTab.Height); 
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    g.FillPath(brush, path);
                }

                // Active Tab Indicator (Underline)
                if (activeTab == tab)
                {
                    using (Pen pen = new Pen(Color.FromArgb(138, 180, 248), 3)) // Blue Accent
                    {
                        g.DrawLine(pen, 10, pnlTab.Height - 1, pnlTab.Width - 10, pnlTab.Height - 1);
                    }
                }
            };
            
            // Hover Tab Effect
            pnlTab.MouseEnter += (s, e) => { if (activeTab != tab) { pnlTab.BackColor = colorTabHover; pnlTab.Invalidate(); } };
            pnlTab.MouseLeave += (s, e) => { if (activeTab != tab) { pnlTab.BackColor = Color.Transparent; pnlTab.Invalidate(); } };

            // --- Drag & Drop (Detach Logic) ---
            bool isDragging = false;
            Point dragStart = Point.Empty;
            MouseEventHandler dragDown = (s, e) => { 
                if(e.Button == MouseButtons.Left){ isDragging = true; dragStart = e.Location; } 
                SwitchToTab(tab); 
            };
            MouseEventHandler dragUp = (s, e) => { isDragging = false; };
            MouseEventHandler dragMove = (s, e) => {
                if (isDragging && e.Button == MouseButtons.Left && (Math.Abs(e.X - dragStart.X) > 5 || Math.Abs(e.Y - dragStart.Y) > 5)) {
                    isDragging = false;
                    DataObject data = new DataObject();
                    data.SetData("QuantumTabUrl", tab.CurrentUrl);
                    data.SetData("SourceForm", this);
                    data.SetData("TabInstance", tab);

                    // Track cancellation
                    bool cancelled = false;
                    QueryContinueDragEventHandler qc = (qs, qe) => { 
                        if (qe.Action == DragAction.Cancel || qe.EscapePressed) cancelled = true; 
                    };
                    pnlTab.QueryContinueDrag += qc;
                    
                    // Start Drag
                    DragDropEffects eff = pnlTab.DoDragDrop(data, DragDropEffects.Move);
                    
                    pnlTab.QueryContinueDrag -= qc;
                    
                    // Detach if not cancelled and no effect (dropped on "nothing" or invalid target)
                    if (eff == DragDropEffects.None && !cancelled) {
                        BrowserForm f = new BrowserForm();
                        f.StartPosition = FormStartPosition.Manual;
                        f.Location = new Point(Cursor.Position.X - 50, Cursor.Position.Y - 20);
                        f.Show();
                        
                        f.AddNewTab(tab.CurrentUrl); 

                        // Remove initial tabs (e.g. Home) created by default constructor
                        // We keep the LAST tab (which is the one we just added)
                        while (f.tabs.Count > 1)
                        {
                            f.CloseTab(f.tabs[0]); // Private access allowed within same class
                        }
                        
                        CloseTab(tab);
                    }
                }
            };
            
            pnlTab.MouseDown += dragDown; 
            pnlTab.MouseUp += dragUp; 
            pnlTab.MouseMove += dragMove;
            
            // Hook children so they don't block drag
            lblTitle.MouseDown += dragDown; 
            lblTitle.MouseUp += dragUp; 
            lblTitle.MouseMove += dragMove;
            pbFav.MouseDown += dragDown; 
            pbFav.MouseUp += dragUp; 
            pbFav.MouseMove += dragMove;

            tab.TabPanel = pnlTab;
            tab.TitleLabel = lblTitle;
            tab.FaviconBox = pbFav;
            tab.CloseButton = btnClose;

            tabs.Add(tab);
            tabBar.Controls.Add(pnlTab);

            UpdateTabPositions();
            InitializeWebViewForTab(tab, startUrl);
        }

        private void UpdateTabPositions()
        {
            if (tabBar == null) return;
            
            // Safety check for scaleFactor if not initialized yet
            float sf = scaleFactor > 0 ? scaleFactor : 1.0f;

            int startX = (int)(5 * sf);
            // Window controls area (Minimize, Maximize, Close) + Gap
            int controlsWidth = (int)(150 * sf); 
            int newTabBtnWidth = (btnNewTab != null) ? btnNewTab.Width + 10 : 40;
            
            // Available width for tabs
            int availableWidth = tabBar.Width - startX - controlsWidth - newTabBtnWidth;
            if (availableWidth < 100) availableWidth = 100; // Protection

            int maxTabWidth = (int)(205 * sf);
            int minTabWidth = (int)(36 * sf); // Icon width basically

            int tabWidth = maxTabWidth;

            if (tabs.Count > 0)
            {
                 int requiredWidth = tabs.Count * maxTabWidth;
                 if (requiredWidth > availableWidth)
                 {
                     tabWidth = availableWidth / tabs.Count;
                     if (tabWidth < minTabWidth) tabWidth = minTabWidth;
                 }
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                int x = startX + (i * tabWidth);
                
                // Height based on TabBar or fixed
                int tabHeight = (int)(34 * sf); 
                
                tab.TabPanel.Size = new Size(tabWidth, tabHeight);
                tab.TabPanel.Location = new Point(x, tabBar.Height - tabHeight);
                tab.TabPanel.BringToFront(); 
                
                // Adjust Internal Controls
                if (tab.CloseButton != null && tab.TitleLabel != null && tab.FaviconBox != null)
                {
                    int pad = (int)(5 * sf);
                    
                    if (tabWidth < (int)(60 * sf))
                    {
                        // Compact Mode
                        tab.CloseButton.Visible = false;
                        tab.TitleLabel.Visible = false;
                        
                        // Center Icon
                        if (tabWidth < (int)(40 * sf))
                             tab.FaviconBox.Left = (tabWidth - tab.FaviconBox.Width) / 2;
                        else
                             tab.FaviconBox.Left = (int)(12 * sf);
                    }
                    else
                    {
                        // Normal Mode
                        tab.CloseButton.Visible = true;
                        tab.TitleLabel.Visible = true;
                        
                        tab.FaviconBox.Left = (int)(12 * sf);
                        
                        // Close Button Pos
                        tab.CloseButton.Left = tabWidth - tab.CloseButton.Width - pad;
                        
                        // Title Layout
                        int titleLeft = tab.FaviconBox.Right + pad;
                        int titleRight = tab.CloseButton.Left - pad;
                        int titleW = titleRight - titleLeft;
                        
                        if (titleW < 0) titleW = 0;
                        
                        tab.TitleLabel.Left = titleLeft;
                        tab.TitleLabel.Width = titleW;
                    }
                }
                
                tab.TabPanel.Invalidate();
            }
            
            if (btnNewTab != null)
            {
                btnNewTab.Location = new Point(startX + (tabs.Count * tabWidth) + 5, (tabBar.Height - btnNewTab.Height) / 2);
            }
        }

        private async void InitializeWebViewForTab(BrowserTab tab, string startUrl)
        {
            try
            {
                // Prepare Environment Options (DNS, Privacy)
                var options = new CoreWebView2EnvironmentOptions();
                string args = " --disable-logging";

                // Private DNS (DoH) settings
                if (appConfig.ContainsKey("DnsProvider") && appConfig["DnsProvider"] != "Off")
                {
                    string provider = appConfig["DnsProvider"];
                    string dohUrl = "";
                    
                    if (provider == "Google DNS") dohUrl = "https://dns.google/dns-query";
                    else if (provider == "Cloudflare") dohUrl = "https://cloudflare-dns.com/dns-query";
                    else if (provider == "NextDNS") dohUrl = "https://dns.nextdns.io";
                    else if (provider == "Custom" && appConfig.ContainsKey("DnsCustomUrl")) dohUrl = appConfig["DnsCustomUrl"];

                    if (provider == "Automatic")
                    {
                        // Automatic: Use system DNS but upgrade if possible
                        args += " --built-in-dns-client-enabled --enable-features=DnsOverHttpsUpgrade";
                    }
                    else if (!string.IsNullOrEmpty(dohUrl))
                    {
                        // Explicit Provider
                        args += string.Format(" --built-in-dns-client-enabled --doh-template=\"{0}\"", dohUrl);
                    }
                }

                // Apply Language Setting
                if (appConfig.ContainsKey("Language"))
                {
                    options.Language = appConfig["Language"];
                }
                else
                {
                    options.Language = "en-US"; // Default
                }

                // Tracking Prevention (Do Not Track header)
                // Note: Actual Tracking Prevention Level is set via Profile AFTER initialization, 
                // but we can set some flags here if needed.
                // We'll set the args we constructed.
                if (!string.IsNullOrEmpty(args)) options.AdditionalBrowserArguments = args;

                // Create Environment with Writable User Data Folder
                string userDataFolder;
                if (IsIncognito)
                {
                    // Isolated Incognito Session: Use a unique temporary folder
                    userDataFolder = Path.Combine(Path.GetTempPath(), "QuantumIncognito_" + Guid.NewGuid().ToString());
                }
                else
                {
                    // Standard Persistent Session
                    userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuantumBrowser");
                }

                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
                await tab.WebView.EnsureCoreWebView2Async(env);

                // --- Apply Post-Init Settings ---
                // Tracking Prevention Level
                if (appConfig.ContainsKey("TrackingPrevention"))
                {
                   string level = appConfig["TrackingPrevention"];
                   // Valid values: None, Basic, Balanced, Strict
                   // WebView2 uses: CoreWebView2TrackingPreventionLevel.Balanced (Default)
                   try {
                       if (level == "Strict") tab.WebView.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Strict;
                       else if (level == "Balanced" || level == "Standard") tab.WebView.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Balanced;
                       else if (level == "Custom") tab.WebView.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Balanced; // Fallback
                       else tab.WebView.CoreWebView2.Profile.PreferredTrackingPreventionLevel = CoreWebView2TrackingPreventionLevel.Basic;
                   } catch {}
                }

                // Spoof Chrome User Agent for Web Store Compatibility
                try {
                    // Start with a clean Chrome UA as requested
                    string chromeUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
                    tab.WebView.CoreWebView2.Settings.UserAgent = chromeUA;
                } catch {}
                
                // User Agent / Do Not Track
                // CoreWebView2 currently doesn't have a direct boolean property for 'Do Not Track' header in all versions,
                // but typically it's handled by tracking prevention or can be injected.
                
                // HTTPS-Only (Mocking logic or simply relying on SmartScreen)
                
                tab.WebView.CoreWebView2.Settings.IsScriptEnabled = true;
                tab.WebView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;

                string assetsPath = Path.Combine(Application.StartupPath, "assets");
                tab.WebView.CoreWebView2.SetVirtualHostNameToFolderMapping("app.assets", assetsPath, CoreWebView2HostResourceAccessKind.Allow);

                tab.WebView.CoreWebView2.WebMessageReceived += HandleWebMessage;

                tab.WebView.CoreWebView2.PermissionRequested += (permSender, permArgs) => {
                     try {
                         string uri = permArgs.Uri;
                         Uri u = new Uri(uri);
                         string host = u.Host;
                         if (BrowserServices.SitePermissionMap.ContainsKey(host))
                         {
                             var perms = BrowserServices.SitePermissionMap[host];
                             bool allow = true;
                             switch(permArgs.PermissionKind)
                             {
                                 case CoreWebView2PermissionKind.Camera: allow = perms.Camera; break;
                                 case CoreWebView2PermissionKind.Microphone: allow = perms.Microphone; break;
                                 case CoreWebView2PermissionKind.Geolocation: allow = perms.Location; break;
                                 case CoreWebView2PermissionKind.Notifications: allow = perms.Notifications; break;
                             }
                             
                             if (!allow) permArgs.State = CoreWebView2PermissionState.Deny;
                             else permArgs.State = CoreWebView2PermissionState.Allow;
                         }
                     } catch {}
                };

                // --- Events ---
                tab.WebView.CoreWebView2.NavigationStarting += (s, e) => {
                    if (tab.WebView.IsDisposed) return;
                    try {
                        string uri = e.Uri;
                    
                    if (uri.StartsWith("quantum://"))
                    {
                         HandleInternalProtocol(e, tab);
                         return;
                    }

                    // Log Start
                    navLogs.Insert(0, new NavLog { Time = DateTime.Now, Type = "Starting", Url = uri, Status = "..." });
                    if (navLogs.Count > 200) navLogs.RemoveAt(navLogs.Count - 1);

                    if (activeTab == tab)
                    {
                        loadingBar.Visible = true;
                        loadingBar.Value = 0;
                        UpdateNavButtons();
                    }
                    } catch {}
                };

                tab.WebView.CoreWebView2.NavigationCompleted += (s, e) => {
                    if (tab.WebView.IsDisposed) return;
                    try {
                        tab.Title = tab.WebView.CoreWebView2.DocumentTitle;
                    if (string.IsNullOrEmpty(tab.Title)) tab.Title = "New Tab";
                    
                    if (e.IsSuccess && !tab.CurrentUrl.StartsWith("quantum://")) 
                        BrowserServices.AddHistory(tab.Title, tab.CurrentUrl);

                    // Log Completion
                    navLogs.Insert(0, new NavLog { Time = DateTime.Now, Type = "Completed", Url = tab.CurrentUrl, Status = e.IsSuccess ? "Success" : "Failed" });
                    if (navLogs.Count > 200) navLogs.RemoveAt(navLogs.Count - 1);

                    // Update UI
                    tab.TitleLabel.Text = tab.Title; 
                    tab.TabPanel.Invalidate(); 

                    if (activeTab == tab)
                    {
                        loadingBar.Visible = false;
                        UpdateNavButtons();
                        this.Text = tab.Title + " - Quantum Browser";
                        UpdateSecurityIcon(tab.CurrentUrl);
                    }
                    
                    // Fetch Favicon via Google Service (Compatible Way)
                    try 
                    {
                       Uri currentUri = new Uri(tab.CurrentUrl);
                       if (currentUri.Scheme.StartsWith("http")) 
                       {
                           string favUrl = "https://www.google.com/s2/favicons?domain=" + currentUri.Host + "&sz=32";
                           LoadFavicon(tab, favUrl);
                       }
                    } catch {}
                    } catch {}
                };

                tab.WebView.CoreWebView2.DownloadStarting += (s, e) => {
                    var item = HandleDownload(e);
                    BrowserServices.AddDownload(item);
                };

                tab.WebView.CoreWebView2.SourceChanged += (s, e) => {
                     string url = tab.WebView.Source.ToString();
                     if (url.StartsWith("http://app.assets")) url = "quantum://home";
                     tab.CurrentUrl = url;
                     if (activeTab == tab) 
                     {
                         addressBar.Text = url;
                         UpdateSecurityIcon(url);
                     }
                };

                tab.WebView.CoreWebView2.ContainsFullScreenElementChanged += (s, e) => {
                    this.BeginInvoke(new MethodInvoker(() => {
                        if (activeTab == tab)
                        {
                            if (tab.WebView.CoreWebView2.ContainsFullScreenElement) EnterFullScreenMode();
                            else ExitFullScreenMode();
                        }
                    }));
                };

                tab.WebView.CoreWebView2.WebMessageReceived += (s, ev) => {
                    try {
                        string msg = ev.TryGetWebMessageAsString();
                        if (msg == "esc_pressed" && tab.WebView.CoreWebView2.ContainsFullScreenElement)
                        {
                            tab.WebView.CoreWebView2.ExecuteScriptAsync("document.exitFullscreen();");
                        }
                    } catch {}
                };

                // Inject Esc listener
                tab.WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                    window.addEventListener('keydown', (e) => {
                        if (e.key === 'Escape') {
                            window.chrome.webview.postMessage('esc_pressed');
                        }
                    });
                ");

                tab.WebView.CoreWebView2.NewWindowRequested += (s, e) => {
                    e.Handled = true;
                    string targetUrl = e.Uri;
                    this.BeginInvoke(new MethodInvoker(() => AddNewTab(targetUrl)));
                };

                SwitchToTab(tab);

                if (startUrl == "home")
                {
                    string target = !string.IsNullOrEmpty(homePageUrl) ? homePageUrl : "http://app.assets/index.html";
                    tab.WebView.CoreWebView2.Navigate(target);
                }
                else
                {
                    Navigate(startUrl);
                }
            }
            catch (Exception ex) 
            {
               // Silent fail to avoid interrupting user flow (e.g. drag drop)
               System.Diagnostics.Debug.WriteLine("Tab Init Error: " + ex.Message); 
            }
        }
        
        private void LoadFavicon(BrowserTab tab, string url)
        {
            try
            {
                WebClient client = new WebClient();
                client.DownloadDataCompleted += (s, e) => {
                    if (e.Error == null)
                    {
                        try 
                        {
                            using (var ms = new MemoryStream(e.Result))
                            {
                                tab.FaviconBox.Image = Image.FromStream(ms);
                            }
                        } catch {}
                    }
                };
                client.DownloadDataAsync(new Uri(url));
            }
            catch {}
        }

        private void SwitchToTab(BrowserTab tab)
        {
            if (activeTab != null && activeTab != tab)
            {
                activeTab.WebView.Visible = false;
                activeTab.TabPanel.Invalidate(); // Repaint as inactive
            }

            activeTab = tab;
            activeTab.WebView.Visible = true;
            activeTab.WebView.BringToFront();
            
            // Visual Update
            tab.TabPanel.Invalidate(); 
            tab.TabPanel.BringToFront(); // Active on top
            
            addressBar.Text = tab.CurrentUrl;
            UpdateSecurityIcon(tab.CurrentUrl);
            this.Text = tab.Title + " - Quantum Browser";
            UpdateNavButtons();

            // Check Fullscreen State
            if (activeTab != null && activeTab.WebView != null && activeTab.WebView.CoreWebView2 != null)
            {
                if (activeTab.WebView.CoreWebView2.ContainsFullScreenElement) EnterFullScreenMode();
                else ExitFullScreenMode();
            }
        }

        private void CloseTab(BrowserTab tab)
        {
            if (tabs.Count <= 1)
            {
                this.Close(); // Close this window only, don't exit entire app (unless it's the last one)
                return;
            }

            int index = tabs.IndexOf(tab);
            tabs.Remove(tab);
            tabBar.Controls.Remove(tab.TabPanel); // Remove Panel

            if (activeTab == tab)
            {
                int nextIndex = Math.Min(index, tabs.Count - 1);
                SwitchToTab(tabs[nextIndex]);
            }

            UpdateTabPositions();
            
            // Dispose resources to free memory
            try {
                tab.WebView.Dispose();
                tab.TabPanel.Dispose();
                // Force Garbage Collection to reclaim memory immediately (optional but helps "lite" feel)
                GC.Collect();
                GC.WaitForPendingFinalizers();
            } catch {}
        }

        private void EnterFullScreenMode()
        {
            if (_isFullScreen) return;
            _isFullScreen = true;
            
            _prevWindowState = this.WindowState;
            _prevBorderStyle = this.FormBorderStyle;
            
            this.Padding = new Padding(0);

            if (topBar != null) topBar.Visible = false;
            if (tabBar != null) tabBar.Visible = false;
            
            if (webViewContainer != null) 
            {
                this.Controls.Add(webViewContainer);
                webViewContainer.Dock = DockStyle.Fill;
                webViewContainer.BringToFront(); 
            }
            
            this.MaximumSize = new Size(0, 0); 
            this.FormBorderStyle = FormBorderStyle.None;
            
            Screen screen = Screen.FromHandle(this.Handle);
            this.WindowState = FormWindowState.Normal; 
            this.Bounds = screen.Bounds;
            
            this.TopMost = true;
        }

        private void ExitFullScreenMode()
        {
            if (!_isFullScreen) return;
            _isFullScreen = false;
            
            this.TopMost = false;
            this.SuspendLayout();

            // 1. Force a clean transition: Restore Style and State variables
            this.FormBorderStyle = _prevBorderStyle;
            
            // 2. CRITICAL: Clear and Re-add in exact docking order
            // WinForms docking priority is based on addition order (Index 0 is inner, Index 2 is outer)
            this.Controls.Remove(webViewContainer);
            this.Controls.Remove(topBar);
            this.Controls.Remove(tabBar);

            // Addition Order: Bottom/Fill -> Middle -> Top
            if (webViewContainer != null) 
            {
                this.Controls.Add(webViewContainer); // Index 0
                webViewContainer.Dock = DockStyle.Fill;
            }
            if (topBar != null)
            {
                this.Controls.Add(topBar);           // Index 1 (Docks above WebView)
                topBar.Visible = true;
            }
            if (tabBar != null)
            {
                this.Controls.Add(tabBar);           // Index 2 (Docks at very top)
                tabBar.Visible = true;
            }

            // 3. Force Frame Refresh (Universal Fix for Title Bar / Black Bar Ghosting)
            SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

            // 4. Restore Final Window State with Layout Trigger
            if (_prevWindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.WindowState = _prevWindowState;
            }

            this.ResumeLayout(true);
            UpdateTabPositions();
            this.PerformLayout();
            this.Refresh();
        }

        // --- Extracted Helpers ---
        
        private void HandleInternalProtocol(CoreWebView2NavigationStartingEventArgs e, BrowserTab tab)
        {
             string uri = e.Uri;
             if (uri.StartsWith("quantum://navigate")) {
                 e.Cancel = true;
                 try {
                    string q = uri.Substring(uri.IndexOf("?q=") + 3);
                    if(q.Contains("&")) q = q.Substring(0, q.IndexOf("&"));
                    string query = Uri.UnescapeDataString(q);
                    Navigate(query);
                 } catch {}
             }
             else if (uri == "quantum://history") { e.Cancel = true; tab.WebView.NavigateToString(GenerateHistoryHtml()); }
             else if (uri == "quantum://downloads") { e.Cancel = true; tab.WebView.NavigateToString(GenerateDownloadsHtml()); }
             else if (uri == "quantum://bookmarks") { e.Cancel = true; tab.WebView.NavigateToString(GenerateBookmarksHtml()); }
             else if (uri == "quantum://extensions") { e.Cancel = true; tab.WebView.NavigateToString(GenerateExtensionsHtml()); }
             else if (uri == "quantum://settings/passwords") { e.Cancel = true; tab.WebView.NavigateToString(GeneratePasswordsHtml()); }
             else if (uri == "quantum://network-log") { e.Cancel = true; tab.WebView.NavigateToString(GenerateNetworkLogHtml()); }
             else if (uri == "quantum://home") { e.Cancel = true; tab.WebView.NavigateToString(GenerateHomeHtml()); }
             else if (uri.StartsWith("quantum://settings")) { e.Cancel = true; tab.WebView.NavigateToString(GenerateSettingsHtml(uri)); }
        }

        private DownloadItem HandleDownload(CoreWebView2DownloadStartingEventArgs e)
        {
            string fileName = Path.GetFileName(e.ResultFilePath);
            bool isCrxFile = fileName.EndsWith(".crx", StringComparison.OrdinalIgnoreCase);
            
            // Save .crx files to extensions folder
            string downloadPath;
            if (isCrxFile)
            {
                string extensionsDir = Path.Combine(Application.StartupPath, "extensions");
                if (!Directory.Exists(extensionsDir)) Directory.CreateDirectory(extensionsDir);
                downloadPath = Path.Combine(extensionsDir, fileName);
            }
            else
            {
                downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);
            }
            
            e.ResultFilePath = downloadPath;
            e.Handled = true;
            
            var item = new DownloadItem { 
                FileName = fileName,
                Url = e.DownloadOperation.Uri,
                Path = downloadPath,
                State = "InProgress",
                Date = DateTime.Now
            };

            e.DownloadOperation.StateChanged += (sender, args) => {
                item.State = e.DownloadOperation.State.ToString();
                if (e.DownloadOperation.State == CoreWebView2DownloadState.Completed)
                {
                    item.BytesReceived = item.TotalBytes;
                    
                    // Prompt to install .crx extension
                    if (isCrxFile)
                    {
                        this.Invoke(new Action(() => {
                            var result = MessageBox.Show(
                                "Extension '" + fileName + "' has been downloaded.\n\nWould you like to extract and install it now?",
                                "Extension Downloaded",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                            
                            if (result == DialogResult.Yes)
                            {
                                try
                                {
                                    // Extract CRX to folder
                                    string extractPath = Path.Combine(Path.GetDirectoryName(downloadPath), Path.GetFileNameWithoutExtension(fileName));
                                    if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
                                    
                                    // CRX is essentially a ZIP file with a header
                                    System.IO.Compression.ZipFile.ExtractToDirectory(downloadPath, extractPath);
                                    
                                    // Load the extension
                                    activeTab.WebView.CoreWebView2.Profile.AddBrowserExtensionAsync(extractPath).ContinueWith(task => {
                                        this.Invoke(new Action(() => {
                                            if (task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion && task.Exception == null)
                                                MessageBox.Show("Extension installed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            else
                                                MessageBox.Show("Failed to install extension. Try using 'Load Unpacked Extension' from quantum://extensions", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        }));
                                    });
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Failed to extract extension: " + ex.Message + "\n\nPlease extract manually and use 'Load Unpacked Extension'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }));
                    }
                }
                BrowserServices.SaveDownloads();
            };
            return item;
        }
        
        private void Navigate(string address) 
        {
            if (CurrentWebView == null) return;
            string url = address;
            if (url == "home" || url == "quantum://home") url = !string.IsNullOrEmpty(homePageUrl) ? homePageUrl : "http://app.assets/index.html";
            else if (!url.StartsWith("http") && !url.StartsWith("file") && !url.StartsWith("quantum"))
            {
                if (url.Contains(".") && !url.Contains(" ")) url = "https://" + url;
                else {
                    string searchUrl = "https://www.google.com/search?q=%s";
                    if (searchEngines.ContainsKey(currentEngineName)) searchUrl = searchEngines[currentEngineName];
                    url = searchUrl.Replace("%s", Uri.EscapeDataString(url));
                }
            }
            try { CurrentWebView.CoreWebView2.Navigate(url); } catch {}
        }

        private void UpdateNavButtons()
        {
            if (CurrentWebView == null) return;
            btnBack.Enabled = CurrentWebView.CanGoBack;
            btnForward.Enabled = CurrentWebView.CanGoForward;
            btnBack.ForeColor = btnBack.Enabled ? colorIcon : Color.DimGray;
            btnForward.ForeColor = btnForward.Enabled ? colorIcon : Color.DimGray;
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Navigate(addressBar.Text);
            }
        }
       
       protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.T)) { AddNewTab(); return true; }
            if (keyData == (Keys.Control | Keys.W)) { if (activeTab != null) CloseTab(activeTab); return true; }
            if (keyData == (Keys.Control | Keys.H)) { AddNewTab("quantum://history"); return true; }
            if (keyData == (Keys.Control | Keys.J)) { AddNewTab("quantum://downloads"); return true; }
            if (keyData == (Keys.Control | Keys.Shift | Keys.O)) { AddNewTab("quantum://bookmarks"); return true; }
            if (keyData == (Keys.Control | Keys.Shift | Keys.Delete)) { 
                if (activeTab != null && activeTab.WebView != null && activeTab.WebView.CoreWebView2 != null) {
                    activeTab.WebView.CoreWebView2.Profile.ClearBrowsingDataAsync();
                    MessageBox.Show("Browsing data cleared.");
                }
                return true; 
            }
            if (keyData == (Keys.Control | Keys.N)) { System.Diagnostics.Process.Start(Application.ExecutablePath); return true; }
            if (keyData == (Keys.Control | Keys.Shift | Keys.N)) { new BrowserForm(true).Show(); return true; }
            if (keyData == (Keys.Control | Keys.R) || keyData == Keys.F5) { if (activeTab != null && activeTab.WebView.CoreWebView2 != null) activeTab.WebView.CoreWebView2.Reload(); return true; }
            if (keyData == (Keys.Control | Keys.L) || keyData == Keys.F6) { addressBar.Focus(); addressBar.SelectAll(); return true; }
            if (keyData == (Keys.Control | Keys.Tab)) { 
                int idx = tabs.IndexOf(activeTab) + 1;
                if (idx >= tabs.Count) idx = 0;
                SwitchToTab(tabs[idx]);
                return true;
            }
            if (keyData == (Keys.Control | Keys.Shift | Keys.Tab)) { 
                int idx = tabs.IndexOf(activeTab) - 1;
                if (idx < 0) idx = tabs.Count - 1;
                SwitchToTab(tabs[idx]);
                return true;
            }
            if (keyData == (Keys.Alt | Keys.Left)) { if (activeTab != null && activeTab.WebView != null && activeTab.WebView.CoreWebView2 != null && activeTab.WebView.CoreWebView2.CanGoBack) activeTab.WebView.CoreWebView2.GoBack(); return true; }
            if (keyData == (Keys.Alt | Keys.Right)) { if (activeTab != null && activeTab.WebView != null && activeTab.WebView.CoreWebView2 != null && activeTab.WebView.CoreWebView2.CanGoForward) activeTab.WebView.CoreWebView2.GoForward(); return true; }
            if (keyData == (Keys.Control | Keys.P)) { if (activeTab != null && activeTab.WebView != null && activeTab.WebView.CoreWebView2 != null) activeTab.WebView.CoreWebView2.ShowPrintUI(); return true; }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }
        
        private void HandleWebMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(message)) return;

                if (message.StartsWith("delete-history:")) {
                    string url = message.Substring("delete-history:".Length);
                    BrowserServices.History.RemoveAll(h => h.Url == url);
                    BrowserServices.SaveHistory();
                    if (activeTab != null && activeTab.CurrentUrl == "quantum://history") activeTab.WebView.NavigateToString(GenerateHistoryHtml());
                }
                else if (message == "clear-history") {
                    BrowserServices.History.Clear();
                    BrowserServices.SaveHistory();
                    if (activeTab != null && activeTab.CurrentUrl == "quantum://history") activeTab.WebView.NavigateToString(GenerateHistoryHtml());
                }
                else if (message.StartsWith("open-file:")) {
                    string path = message.Substring("open-file:".Length);
                    if (File.Exists(path)) System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + path + "\"");
                }
                else if (message == "clear-downloads") {
                    BrowserServices.Downloads.Clear();
                    BrowserServices.SaveDownloads();
                    if (activeTab != null && activeTab.CurrentUrl == "quantum://downloads") activeTab.WebView.NavigateToString(GenerateDownloadsHtml());
                }
                else if (message == "get-shortcuts")
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append("[");
                    for(int i=0; i<BrowserServices.Shortcuts.Count; i++)
                    {
                        var s = BrowserServices.Shortcuts[i];
                        sb.Append("{");
                        sb.Append(string.Format("\"title\": \"{0}\", \"url\": \"{1}\"", 
                            s.Title.Replace("\\", "\\\\").Replace("\"", "\\\""), 
                            s.Url.Replace("\\", "\\\\").Replace("\"", "\\\"")));
                        sb.Append("}");
                        if (i < BrowserServices.Shortcuts.Count - 1) sb.Append(",");
                    }
                    sb.Append("]");
                    if (activeTab != null && activeTab.WebView != null && activeTab.WebView.CoreWebView2 != null) 
                        activeTab.WebView.CoreWebView2.PostWebMessageAsString("shortcuts-data:" + sb.ToString());
                }
                else if (message.StartsWith("add-shortcut:"))
                {
                    string content = message.Substring("add-shortcut:".Length);
                    string[] parts = content.Split(new char[]{'|'}, 2);
                    if (parts.Length == 2)
                    {
                        BrowserServices.AddShortcut(parts[0], parts[1]);
                        if (activeTab != null && activeTab.WebView != null && activeTab.WebView.CoreWebView2 != null) 
                            ReloadAllInternalPages();
                    }
                }
                else if (message == "add-shortcut-prompt")
                {
                    this.Invoke(new Action(ShowAddShortcutDialog));
                }
                else if (message.StartsWith("edit-shortcut-prompt:"))
                {
                     string content = message.Substring("edit-shortcut-prompt:".Length);
                     string[] parts = content.Split(new char[]{'|'}, 2);
                     if (parts.Length == 2)
                     {
                         this.Invoke(new Action(() => ShowEditShortcutDialog(parts[0], parts[1])));
                     }
                }
                else if (message.StartsWith("delete-shortcut:"))
                {
                    string url = message.Substring("delete-shortcut:".Length);
                    BrowserServices.RemoveShortcut(url);
                    ReloadAllInternalPages();
                }
                // Settings & Appearance
                else if (message.StartsWith("set-config:"))
                {
                    string[] parts = message.Substring(11).Split(new char[]{'|'}, 2);
                    if (parts.Length == 2) { 
                        appConfig[parts[0]] = parts[1]; 
                        SaveConfiguration(); 
                        ApplyTheme(); 
                        SaveConfiguration(); 
                        ApplyTheme(); 
                        ReloadAllInternalPages();
                    }
                }
                else if (message == "choose-wallpaper")
                {
                    this.Invoke(new Action(() => {
                        OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.png;*.jpeg;*.bmp" };
                        if (ofd.ShowDialog() == DialogResult.OK) {
                           appConfig["WallpaperPath"] = ofd.FileName;
                           SaveConfiguration(); 
                           ApplyTheme();
                           ReloadAllInternalPages();
                        }
                    }));
                }
                else if (message == "clear-wallpaper")
                {
                    if (appConfig.ContainsKey("WallpaperPath")) appConfig.Remove("WallpaperPath");
                    SaveConfiguration();
                    ApplyTheme();
                    ReloadAllInternalPages();
                }
                // Extensions
                else if (message == "load-extension")
                {
                    this.Invoke(new Action(async () => {
                        FolderBrowserDialog fbd = new FolderBrowserDialog { Description = "Select Unpacked Extension Folder" };
                        if (fbd.ShowDialog() == DialogResult.OK) {
                            try {
                                await activeTab.WebView.CoreWebView2.Profile.AddBrowserExtensionAsync(fbd.SelectedPath);
                                MessageBox.Show("Extension loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            } catch (Exception ex) {
                                MessageBox.Show("Failed to load extension: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }));
                }
                else if (message == "install-crx")
                {
                   MessageBox.Show("To install a CRX file:\n1. Rename .crx to .zip\n2. Extract it to a folder\n3. Use 'Load Unpacked Extension' to load that folder.\n\nDirect CRX installation is essentially this process.", "CRX Installation Guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch { }
        }

        private string GenerateNetworkLogHtml()
        {
             var sb = new System.Text.StringBuilder();
             sb.Append("<div style='padding:10px;'><h2 style='border-bottom:1px solid #555; padding-bottom:10px;'>Network Navigation Log</h2>");
             sb.Append("<button class='action-btn' onclick=\"window.location.reload()\">Refresh Log</button><br/><br/>");
             sb.Append("<table style='width:100%; border-collapse:collapse; color:#ddd;'>");
             sb.Append("<tr style='background:#333; text-align:left;'><th>Time</th><th>Type</th><th>URL</th><th>Status</th></tr>");
             
             foreach(var log in navLogs)
             {
                 string color = log.Type == "Starting" ? "#8ab4f8" : (log.Status == "Success" ? "#81c995" : "#f28b82");
                 sb.Append(string.Format("<tr style='border-bottom:1px solid #444;'><td>{0}</td><td style='color:{1}'>{2}</td><td style='word-break:break-all;'>{3}</td><td>{4}</td></tr>", 
                     log.Time.ToString("HH:mm:ss.fff"), color, log.Type, System.Net.WebUtility.HtmlEncode(log.Url), log.Status));
             }
             sb.Append("</table></div>");
             return GenerateBaseHtml("Network Log", sb.ToString());
        }

        private string GetSharedCss()
        {
            try { return File.ReadAllText(Path.Combine(Application.StartupPath, "assets", "internal.css")); }
            catch { return ""; }
        }

        private string GenerateBaseHtml(string title, string content, string actions = "")
        {
            string css = GetSharedCss();
            
            // Dynamic Theme Injection
            string themeCss = "";
            string bg = "#202124";
            string card = "#292a2d";
            string text = "#e8eaed";
            string textSec = "#9aa0a6";

            if (appConfig.ContainsKey("Theme"))
            {
                if (appConfig["Theme"] == "Light")
                {
                    bg = "#ffffff"; card = "#f1f3f4"; text = "#202124"; textSec = "#5f6368";
                }
                else if (appConfig["Theme"] == "Custom" && appConfig.ContainsKey("ThemeColor"))
                {
                    try {
                        Color c = ColorTranslator.FromHtml(appConfig["ThemeColor"]);
                        bg = ColorTranslator.ToHtml(c);
                        card = ColorTranslator.ToHtml(ControlPaint.Light(c, 0.1f));
                        // Contrast
                        double lum = (0.299*c.R + 0.587*c.G + 0.114*c.B)/255;
                        text = lum > 0.5 ? "#000000" : "#ffffff";
                        textSec = lum > 0.5 ? "#444444" : "#cccccc";
                    } catch {}
                }
            }

            themeCss = string.Format(@"
                body {{ background-color: {0}; color: {1}; }}
                .item, .top-bar {{ background-color: {2}; color: {1}; }}
                .url, .timestamp {{ color: {3}; }}
                a {{ color: #8ab4f8; }}
            ", bg, text, card, textSec);

            return string.Format(@"
            <html>
            <head>
                <title>{0}</title>
                <style>{1} {4}</style>
                <script>
                    function post(msg) {{ window.chrome.webview.postMessage(msg); }}
                </script>
            </head>
            <body>
                <div class='top-bar'>
                    <div class='top-bar-title'>{0}</div>
                    <div class='top-bar-actions'>{2}</div>
                </div>
                <div class='container'>
                    {3}
                </div>
            </body>
            </html>", title, css, actions, content, themeCss);
        }

        private string GenerateHomeHtml()
        {
            // 1. Determine Theme Colors & Mode
            bool isDark = true;
            string themeColor = "#202124"; // Default Dark
            if (appConfig.ContainsKey("Theme"))
            {
                if (appConfig["Theme"] == "Light") { isDark = false; themeColor = "#ffffff"; }
                else if (appConfig["Theme"] == "Custom" && appConfig.ContainsKey("ThemeColor")) try { themeColor = appConfig["ThemeColor"]; isDark = (ColorTranslator.FromHtml(themeColor).GetBrightness() < 0.5); } catch {}
            }

            // 2. Wallpaper & Glass Effect
            string wallpaperPath = appConfig.ContainsKey("WallpaperPath") ? appConfig["WallpaperPath"] : "";
            string bgImageStyle = "";
            bool hasWallpaper = !string.IsNullOrEmpty(wallpaperPath) && File.Exists(wallpaperPath);

            if (hasWallpaper)
            {
                try {
                    string base64 = Convert.ToBase64String(File.ReadAllBytes(wallpaperPath));
                    bgImageStyle = string.Format("background-image: url('data:image/png;base64,{0}'); background-size: cover; background-position: center; background-attachment: fixed;", base64);
                } catch {}
            }

            // Colors
            string bgColor = hasWallpaper ? "transparent" : themeColor; // If wallpaper, body is transparent (image covers it)
            string textColor = isDark ? "#ffffff" : "#202124";
            string glassColor = isDark ? "rgba(32, 33, 36, 0.75)" : "rgba(255, 255, 255, 0.75)";
            
            // Specific colors to match screenshots
            string searchBg = isDark ? "rgba(255, 255, 255, 0.1)" : "#f1f3f4"; 
            string searchShadow = isDark ? "0 4px 12px rgba(0,0,0,0.3)" : "none";
            string shortcutHover = isDark ? "rgba(255, 255, 255, 0.1)" : "rgba(32, 33, 36, 0.06)";
            string accentColor = isDark ? "#8ab4f8" : "#4285f4";

            // 3. Logo Loading
            string logoHtml = "<h1>Quantum Browser</h1>";
            try {
                string logoPath = Path.Combine(Application.StartupPath, "logo.png");
                if (!File.Exists(logoPath)) logoPath = @"c:\Users\fsl\Downloads\experimen\MyApplication\logo.png";

                if (File.Exists(logoPath)) {
                    string logoB64 = Convert.ToBase64String(File.ReadAllBytes(logoPath));
                    logoHtml = string.Format(
                        "<img src='data:image/png;base64,{0}' style='width:120px; height:120px; margin-bottom:15px; border-radius:50%; box-shadow: 0 4px 15px rgba(0,0,0,0.2); object-fit:cover;'><br><h1 style='margin-top:0; font-size:2.5rem; display:inline-block;'>Quantum Browser</h1>", 
                        logoB64);
                }
            } catch {}

            // 4. Search Engine
            string searchUrl = searchEngines.ContainsKey(currentEngineName) ? searchEngines[currentEngineName] : "https://www.google.com/search?q=%s";

            var sb = new System.Text.StringBuilder();
            sb.Append(string.Format(@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>New Tab</title>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    * {{ box-sizing: border-box; transition: all 0.2s ease; }}
                    body {{ 
                        margin: 0; padding: 0; 
                        display: flex; justify-content: center; align-items: center; 
                        min-height: 100vh; 
                        background-color: {0};
                        {1}
                        font-family: 'Segoe UI', system-ui, sans-serif;
                        color: {2}; 
                        overflow-y: auto;
                    }}
                    .glass-container {{
                        text-align: center; 
                        width: 90%; max-width: 900px;
                        padding: 60px 40px;
                        border-radius: 24px;
                        {3}
                    }}
                    h1 {{ 
                        font-size: 64px; margin-bottom: 40px; font-weight: 600; letter-spacing: -1.5px;
                        color: {2};
                        text-shadow: 0 4px 12px rgba(0,0,0,0.1);
                    }}
                    .title-accent {{ color: {8}; }}
                    
                    /* Search Box - Larger & Responsive */
                    .search-wrapper {{ position: relative; width: 100%; max-width: 720px; margin: 0 auto; }}
                    .search-box {{ 
                        width: 100%; 
                        padding: 20px 30px; padding-left: 60px;
                        border-radius: 40px; 
                        border: 1px solid rgba(128,128,128,0.2);
                        font-size: 20px; 
                        background-color: {4}; 
                        color: {2};
                        outline: none; 
                        box-shadow: {5};
                    }}
                    .search-box:focus {{ 
                        box-shadow: 0 6px 20px rgba(0,0,0,0.25); 
                        background-color: {4};
                        border-color: #8ab4f8;
                    }}
                    .search-icon {{
                        position: absolute; left: 24px; top: 50%; transform: translateY(-50%);
                        width: 24px; height: 24px; opacity: 0.6;
                        fill: {2};
                    }}

                    /* Shortcuts - Larger Grid */
                    .shortcuts-grid {{ 
                        display: flex; flex-wrap: wrap; justify-content: center; gap: 20px; 
                        margin-top: 60px; 
                    }}
                    .shortcut {{ 
                        display: flex; flex-direction: column; align-items: center; justify-content: center;
                        width: 112px; padding: 16px 10px;
                        border-radius: 12px;
                        text-decoration: none; color: inherit;
                        cursor: pointer;
                    }}
                    .shortcut:hover {{ background-color: {6}; transform: translateY(-4px); }}
                    .icon-circle {{ 
                        width: 64px; height: 64px; 
                        background: #fff; 
                        border-radius: 50%; 
                        display: flex; align-items: center; justify-content: center;
                        margin-bottom: 14px;
                        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                        overflow: hidden;
                    }}
                    .icon-circle img {{ width: 36px; height: 36px; object-fit: contain; }}
                    .shortcut-label {{ 
                        font-size: 14px; font-weight: 500; 
                        white-space: nowrap; overflow: hidden; text-overflow: ellipsis; 
                        max-width: 100px;
                        opacity: 0.95;
                    }}

                    /* Responsive Media Queries */
                    @media (min-width: 1920px) {{
                         h1 {{ font-size: 80px; margin-bottom: 50px; }}
                         .glass-container {{ max-width: 1200px; }}
                         .search-wrapper {{ max-width: 900px; }}
                         .search-box {{ padding: 24px 40px; padding-left: 70px; font-size: 24px; border-radius: 50px; }}
                         .search-icon {{ width: 28px; height: 28px; left: 30px; }}
                         .shortcuts-grid {{ gap: 30px; margin-top: 80px; }}
                         .shortcut {{ width: 140px; padding: 20px 10px; }}
                         .icon-circle {{ width: 80px; height: 80px; }}
                         .icon-circle img {{ width: 48px; height: 48px; }}
                         .shortcut-label {{ font-size: 16px; max-width: 130px; }}
                    }}

                    @media (max-width: 768px) {{
                         h1 {{ font-size: 40px; margin-bottom: 24px; }}
                         .glass-container {{ padding: 30px 15px; width: 96%; }}
                         .search-box {{ padding: 14px 20px; padding-left: 45px; font-size: 16px; }}
                         .search-icon {{ width: 18px; height: 18px; left: 16px; }}
                         .shortcuts-grid {{ gap: 10px; margin-top: 30px; }}
                         .shortcut {{ width: 80px; padding: 10px 5px; }}
                         .icon-circle {{ width: 48px; height: 48px; margin-bottom: 8px; }}
                         .icon-circle img {{ width: 24px; height: 24px; }}
                         .shortcut-label {{ font-size: 12px; max-width: 75px; }}
                    }}
                </style>
                <script>
                    function doSearch(e) {{
                        if (e.key === 'Enter') {{
                            var q = document.getElementById('q').value;
                            if(q) {{
                                var url = '{7}'.replace('%s', encodeURIComponent(q));
                                window.location.href = url;
                            }}
                        }}
                    }}

                    // Context Menu Logic
                    document.addEventListener('contextmenu', function(e) {{
                        var target = e.target.closest('.shortcut');
                        if (target) {{
                            e.preventDefault();
                            var menu = document.getElementById('ctx-menu');
                            menu.style.display = 'block';
                            menu.style.left = e.pageX + 'px';
                            menu.style.top = e.pageY + 'px';
                            
                            var url = target.getAttribute('href');
                            var title = target.querySelector('.shortcut-label').innerText;
                            
                            menu.dataset.url = url;
                            menu.dataset.title = title;
                        }} else {{
                            document.getElementById('ctx-menu').style.display = 'none';
                        }}
                    }});

                    document.addEventListener('click', function(e) {{
                        document.getElementById('ctx-menu').style.display = 'none';
                    }});

                    function ctxAction(action) {{
                        var menu = document.getElementById('ctx-menu');
                        var url = menu.dataset.url;
                        var title = menu.dataset.title;
                        
                        if (action === 'edit') {{
                            window.chrome.webview.postMessage('edit-shortcut-prompt:' + title + '|' + url);
                        }} else if (action === 'delete') {{
                            if(confirm('Delete shortcut for ' + title + '?')) {{
                                window.chrome.webview.postMessage('delete-shortcut:' + url);
                            }}
                        }} else if (action === 'newtab') {{
                             window.open(url, '_blank');
                        }} else if (action === 'newwin') {{
                             window.open(url, '_blank', 'popup=yes'); 
                        }}
                    }}
                </script>
            </head>
            <body>
                <div id='ctx-menu' style='display:none; position:absolute; background:{4}; border:1px solid #555; border-radius:8px; box-shadow:0 4px 12px rgba(0,0,0,0.5); z-index:1000; min-width:150px; overflow:hidden;'>
                    <div class='ctx-item' onclick='ctxAction(""newtab"")'>Open in new tab</div>
                    <div class='ctx-item' onclick='ctxAction(""newwin"")'>Open in new window</div>
                    <div style='height:1px; background:rgba(128,128,128,0.2); margin:4px 0;'></div>
                    <div class='ctx-item' onclick='ctxAction(""edit"")'>Edit shortcut</div>
                    <div class='ctx-item' onclick='ctxAction(""delete"")' style='color:#f28b82;'>Remove</div>
                </div>
                <style>
                    .ctx-item {{ padding: 10px 16px; cursor: pointer; font-size:13px; color:{2}; }}
                    .ctx-item:hover {{ background-color: {6}; }}
                </style>    
                <div class='glass-container'>
                    <h1>Quantum<span class='title-accent'>Browser</span></h1>
                    
                    <div class='search-wrapper'>
                        <svg class='search-icon' viewBox='0 0 24 24'><path d='M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z'/></svg>
                        <input type='text' id='q' class='search-box' placeholder='Search the web or type a URL' onkeydown='doSearch(event)' autofocus />
                    </div>

                    <div class='shortcuts-grid'>", 
            themeColor, bgImageStyle, textColor, 
            hasWallpaper ? "background: " + glassColor + "; backdrop-filter: blur(20px);" : "",
            searchBg, searchShadow, shortcutHover, searchUrl, accentColor));

            // Shortcuts
            foreach (var s in BrowserServices.Shortcuts)
            {
                string favUrl = "https://www.google.com/s2/favicons?domain=" + new Uri(s.Url).Host + "&sz=128";
                sb.Append(string.Format(@"
                <a href='{0}' class='shortcut'>
                    <div class='icon-circle'>
                        <img src='{1}' onerror=""this.src='data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0iIzU1NSI+PHBhdGggZD0iTTEyIDJDMiAybTIgYzIgMm0yIDJNMTAgNHYydjJINHYySDJ2Mmg0djJIMnYyaDR2Mmg2djJoMnYtMmgydi0yaDJ2LTJoMnYtMmgtMnYtMmgtMnYtMmgtMnYtMmgtMnoiLz48L3N2Zz4='"" />
                    </div>
                    <span class='shortcut-label'>{2}</span>
                </a>", s.Url, favUrl, System.Net.WebUtility.HtmlEncode(s.Title)));
            }

            // 'Add Shortcut' Button
            sb.Append(string.Format(@"
                <div class='shortcut' onclick=""window.chrome.webview.postMessage('add-shortcut-prompt')"" style='opacity: 0.7;'>
                    <div class='icon-circle' style='background: rgba(128,128,128,0.2); color: {0}; font-size: 24px;'>+</div>
                    <span class='shortcut-label'>Add</span>
                </div>", textColor));

            sb.Append(@"
                    </div>
                </div>
            </body>
            </html>");
            
            return sb.ToString(); 
        }

        private string GenerateHistoryHtml()
        {
            var sb = new System.Text.StringBuilder();
            string actions = "<button class='action-btn danger' onclick=\"post('clear-history')\">Clear browsing data</button>";
            
            if (BrowserServices.History.Count == 0) sb.Append("<div class='empty-state'>No history found</div>");
            else {
                foreach (var item in BrowserServices.History) {
                    sb.Append(string.Format(@"
                    <div class='item'>
                        <div class='item-info' onclick=""window.location='{0}'"" style='cursor:pointer;'>
                            <div class='title'>{1}</div>
                            <div class='url'>{2}</div>
                        </div>
                        <div class='timestamp'>{3}</div>
                        <div class='item-actions'>
                            <button class='mini-btn' title='Remove' onclick=""post('delete-history:{0}')"">✕</button>
                        </div>
                    </div>", item.Url, System.Net.WebUtility.HtmlEncode(item.Title), System.Net.WebUtility.HtmlEncode(item.Url), item.Date.ToString("g")));
                }
            }
            return GenerateBaseHtml("History", sb.ToString(), actions);
        }

        private string GenerateDownloadsHtml()
        {
            var sb = new System.Text.StringBuilder();
            string actions = "<button class='action-btn' onclick=\"post('clear-downloads')\">Clear all</button>";

            if (BrowserServices.Downloads.Count == 0) sb.Append("<div class='empty-state'>Downloads will appear here</div>");
            else {
                foreach (var item in BrowserServices.Downloads) {
                    bool isDone = item.State == "Completed";
                    sb.Append(string.Format(@"
                    <div class='item'>
                        <div class='item-info'>
                            <div class='title'>{0}</div>
                            <div class='url'>{1}</div>
                            {2}
                        </div>
                        <div class='timestamp'>{3}</div>
                        <div class='item-actions'>
                            <button class='action-btn' onclick=""post('open-file:{4}')"">Show in folder</button>
                        </div>
                    </div>", 
                    System.Net.WebUtility.HtmlEncode(item.FileName), 
                    item.State, 
                    isDone ? "" : "<div class='download-progress-bg'><div class='download-progress-fg' style='width:50%'></div></div>",
                    item.Date.ToString("g"),
                    item.Path.Replace("\\", "\\\\")));
                }
            }
            return GenerateBaseHtml("Downloads", sb.ToString(), actions);
        }

        private string GenerateBookmarksHtml()
        {
            var sb = new System.Text.StringBuilder();
            if (BrowserServices.Bookmarks.Count == 0) sb.Append("<div class='empty-state'>No bookmarks yet</div>");
            else {
                foreach (var item in BrowserServices.Bookmarks) {
                     sb.Append(string.Format(@"
                    <a href='{0}'>
                        <div class='item'>
                            <div class='item-info'>
                                <div class='title'>{1}</div>
                                <div class='url'>{2}</div>
                            </div>
                        </div>
                    </a>", item.Url, System.Net.WebUtility.HtmlEncode(item.Title), System.Net.WebUtility.HtmlEncode(item.Url)));
                }
            }
            return GenerateBaseHtml("Bookmarks", sb.ToString());
        }

        private string GeneratePasswordsHtml()
        {
            var sb = new System.Text.StringBuilder();
            if (BrowserServices.Passwords.Count == 0) sb.Append("<div class='empty-state'>No saved passwords</div>");
            else {
                foreach (var item in BrowserServices.Passwords) {
                     sb.Append(string.Format(@"
                    <div class='item'>
                        <div class='item-info'>
                            <div class='title'>{0}</div>
                            <div class='url'>{1}</div>
                        </div>
                        <div class='timestamp' onclick=""alert('Password: {2}')"" style='cursor:pointer;'>Show Password</div>
                    </div>", 
                    System.Net.WebUtility.HtmlEncode(item.Domain), 
                    System.Net.WebUtility.HtmlEncode(item.Username),
                    System.Net.WebUtility.HtmlEncode(item.Password)));
                }
            }
            return GenerateBaseHtml("Passwords", sb.ToString());
        }

        private string GenerateSettingsHtml(string uri)
        {
            // Determine active section from URI or default
            string activeSection = "appearance"; // default to appearance as per user focus
            if (uri.Contains("passwords")) activeSection = "passwords"; // Legacy support

            // Data
            string theme = appConfig.ContainsKey("Theme") ? appConfig["Theme"] : "Dark";
            string color = appConfig.ContainsKey("ThemeColor") ? appConfig["ThemeColor"] : "#4285f4";
            string wallpaper = appConfig.ContainsKey("WallpaperPath") ? appConfig["WallpaperPath"] : "";
            string homepage = appConfig.ContainsKey("HomePage") ? appConfig["HomePage"] : "quantum://home";
            string startup = appConfig.ContainsKey("StartupBehavior") ? appConfig["StartupBehavior"] : "NewTab";
            string startupPages = appConfig.ContainsKey("StartupPages") ? appConfig["StartupPages"] : "";
            string engine = currentEngineName;

            var sb = new System.Text.StringBuilder();
            sb.Append(@"
            <div class='settings-layout' style='display:flex; height:100vh;'>
                <div class='settings-sidebar' style='width:250px; padding:20px; border-right:1px solid rgba(128,128,128,0.2);'>
                    <h2 style='margin-top:0;'>Settings</h2>
                    <div class='nav-item' onclick=""show('general')"" id='nav-general'>General</div>
                    <div class='nav-item' onclick=""show('startup')"" id='nav-startup'>On startup</div>
                    <div class='nav-item' onclick=""show('appearance')"" id='nav-appearance'>Appearance</div>
                    <div class='nav-item' onclick=""show('search')"" id='nav-search'>Search Engine</div>
                    <div class='nav-item' onclick=""show('privacy')"" id='nav-privacy'>Privacy</div>
                    <div class='nav-item' onclick=""show('about')"" id='nav-about'>About</div>
                </div>
                <div class='settings-content' style='flex:1; padding:40px; overflow-y:auto;'>
                    
                    <!-- General -->
                    <div id='sec-general' class='section' style='display:none;'>
                        <h1>General</h1>
                        <div class='control-group'>
                            <label>Homepage URL</label>
                            <input type='text' value='" + homepage + @"' onchange=""post('set-config:HomePage|'+this.value)"" />
                        </div>
                    </div>

                    <!-- On startup -->
                    <div id='sec-startup' class='section' style='display:none;'>
                        <h1>On startup</h1>
                        <div class='control-group'>
                            <label style='font-weight:normal; margin-bottom:10px;'><input type='radio' name='startup' value='NewTab' onchange=""post('set-config:StartupBehavior|NewTab'); document.getElementById('startup-specific').style.display='none';"" " + (startup=="NewTab"?"checked":"") + @"> Open the New Tab page</label>
                            <label style='font-weight:normal; margin-bottom:10px;'><input type='radio' name='startup' value='Continue' onchange=""post('set-config:StartupBehavior|Continue'); document.getElementById('startup-specific').style.display='none';"" " + (startup=="Continue"?"checked":"") + @"> Continue where you left off</label>
                            <label style='font-weight:normal; margin-bottom:10px;'><input type='radio' name='startup' value='SpecificPage' onchange=""post('set-config:StartupBehavior|SpecificPage'); document.getElementById('startup-specific').style.display='block';"" " + (startup=="SpecificPage"?"checked":"") + @"> Open a specific page or set of pages</label>
                            
                            <div id='startup-specific' style='margin-left:25px; margin-top:5px; display:" + (startup=="SpecificPage"?"block":"none") + @";'>
                                <div style='margin-bottom:5px; font-size:12px; opacity:0.7;'>Enter URLs (comma separated)</div>
                                <input type='text' value='" + startupPages + @"' onchange=""post('set-config:StartupPages|'+this.value)"" placeholder='example.com, google.com' />
                            </div>
                        </div>
                    </div>

                    <!-- Appearance -->
                    <div id='sec-appearance' class='section' style='display:none;'>
                        <h1>Appearance</h1>
                        <div class='control-group'>
                            <label>Theme Mode</label>
                            <div>
                                <label><input type='radio' name='theme' value='Dark' onchange=""post('set-config:Theme|Dark')"" " + (theme=="Dark"?"checked":"") + @"> Dark</label>
                                <label><input type='radio' name='theme' value='Light' onchange=""post('set-config:Theme|Light')"" " + (theme=="Light"?"checked":"") + @"> Light</label>
                                <label><input type='radio' name='theme' value='Custom' onchange=""post('set-config:Theme|Custom')"" " + (theme=="Custom"?"checked":"") + @"> Custom (Material You)</label>
                            </div>
                        </div>
                        <div class='control-group'>
                            <label>Accent Color (for Custom Theme)</label>
                            <input type='color' value='" + color + @"' onchange=""post('set-config:ThemeColor|'+this.value)"" />
                        </div>
                        <div class='control-group'>
                            <label>Homepage Wallpaper</label>
                            <div style='margin-bottom:10px; font-size:12px; opacity:0.7;'>" + (string.IsNullOrEmpty(wallpaper) ? "No wallpaper selected" : System.IO.Path.GetFileName(wallpaper)) + @"</div>
                            <button class='action-btn' onclick=""post('choose-wallpaper')"">Browse Image...</button>
                            <button class='action-btn danger' onclick=""post('clear-wallpaper')"">Remove</button>
                        </div>
                    </div>

                    <!-- Search -->
                    <div id='sec-search' class='section' style='display:none;'>
                        <h1>Search Engine</h1>
                        <div class='control-group'>
                            <label>Address bar search engine</label>
                            <select onchange=""post('set-config:CurrentEngine|'+this.value)"">");
            
            foreach(var k in searchEngines.Keys) {
                sb.Append("<option value='" + k + "' " + (k == engine ? "selected" : "") + ">" + k + "</option>");
            }

            sb.Append(@"    </select>
                        </div>
                    </div>

                    <!-- Privacy -->
                    <div id='sec-privacy' class='section' style='display:none;'>
                        <h1>Privacy & Security</h1>
                        
                        <!-- Private DNS -->
                        <div class='control-group' style='background:rgba(255,255,255,0.03); padding:15px; border-radius:8px; border:1px solid rgba(128,128,128,0.1); margin-bottom:20px;'>
                            <label>Private DNS</label>
                            <p style='margin-bottom:10px; opacity:0.7; font-size:13px;'>Secure your connection and bypass filters.</p>
                            <select onchange=""post('set-config:DnsProvider|'+this.value); if(this.value=='Custom') document.getElementById('dns-custom').style.display='block'; else document.getElementById('dns-custom').style.display='none';"">
                                <option value='Automatic' " + (!appConfig.ContainsKey("DnsProvider") || appConfig["DnsProvider"]=="Automatic"?"selected":"") + @">Automatic (Recommended)</option>
                                <option value='Off' " + (appConfig.ContainsKey("DnsProvider") && appConfig["DnsProvider"]=="Off"?"selected":"") + @">Off</option>
                                <option value='Google DNS' " + (appConfig.ContainsKey("DnsProvider") && appConfig["DnsProvider"]=="Google DNS"?"selected":"") + @">Google DNS (Public)</option>
                                <option value='Cloudflare' " + (appConfig.ContainsKey("DnsProvider") && appConfig["DnsProvider"]=="Cloudflare"?"selected":"") + @">Cloudflare (1.1.1.1)</option>
                                <option value='NextDNS' " + (appConfig.ContainsKey("DnsProvider") && appConfig["DnsProvider"]=="NextDNS"?"selected":"") + @">NextDNS (Bypass Blur)</option>
                                <option value='Custom' " + (appConfig.ContainsKey("DnsProvider") && appConfig["DnsProvider"]=="Custom"?"selected":"") + @">Custom Provider...</option>
                            </select>
                            
                            <div id='dns-custom' style='margin-top:10px; display:" + (appConfig.ContainsKey("DnsProvider") && appConfig["DnsProvider"]=="Custom"?"block":"none") + @";'>
                                <input type='text' placeholder='https://dns.example/query' value='" + (appConfig.ContainsKey("DnsCustomUrl") ? appConfig["DnsCustomUrl"] : "") + @"' onchange=""post('set-config:DnsCustomUrl|'+this.value)"" />
                            </div>
                            
                            <div style='margin-top:10px; font-size:12px; color:#8ab4f8; opacity:0.9;'>Note: A browser restart is required to apply DNS changes.</div>
                        </div>

                        <div class='control-group'>
                            <label><input type='checkbox' " + (appConfig.ContainsKey("HttpsOnly") && appConfig["HttpsOnly"]=="true"?"checked":"") + @" onchange=""post('set-config:HttpsOnly|'+(this.checked?'true':'false'))""> HTTPS-Only Mode</label>
                        </div>
                        <div class='control-group'>
                            <button class='action-btn danger' onclick=""post('clear-history')"">Clear Browsing Data</button>
                        </div>
                    </div>

                    <!-- About -->
                    <div id='sec-about' class='section' style='display:none;'>
                        <h1>About Quantum</h1>
                        <p>Version 1.0.0 (Alpha)</p>
                        <p>Powered by WebView2 & Antigravity AI</p>
                    </div>

                </div>
            </div>

            <style>
                .nav-item { padding: 10px 15px; cursor: pointer; border-radius: 8px; margin-bottom: 5px; }
                .nav-item:hover { background-color: rgba(128,128,128,0.1); }
                .nav-item.active { background-color: rgba(138, 180, 248, 0.2); color: #8ab4f8; font-weight: 500; }
                .control-group { margin-bottom: 25px; }
                label { display: block; margin-bottom: 8px; font-weight: 500; }
                input[type='text'], select { padding: 8px 12px; border-radius: 6px; border: 1px solid rgba(128,128,128,0.3); background: rgba(255,255,255,0.05); color: inherit; width: 100%; max-width: 400px; }
            </style>
            <script>
                function show(id) {
                    document.querySelectorAll('.section').forEach(e => e.style.display = 'none');
                    document.getElementById('sec-'+id).style.display = 'block';
                    document.querySelectorAll('.nav-item').forEach(e => e.classList.remove('active'));
                    document.getElementById('nav-'+id).classList.add('active');
                }
                // Show default
                show('appearance');
            </script>");

            return GenerateBaseHtml("Settings", sb.ToString());
        }

        private string GenerateExtensionsHtml()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(@"
            <div style='padding:40px; max-width:800px; margin:0 auto;'>
                <h1>Extensions</h1>
                <p style='margin-bottom:30px; opacity:0.8;'>Manage your browser extensions. Quantum Browser supports Chromium-based extensions.</p>

                <div class='card' style='background:rgba(255,255,255,0.05); border:1px solid rgba(128,128,128,0.2); border-radius:12px; padding:20px; margin-bottom:20px;'>
                    <h2>Install from Edge Add-ons Store</h2>
                    <p>You can browse the <a href='https://microsoftedge.microsoft.com/addons' target='_blank' style='color:#8ab4f8;'>Microsoft Edge Add-ons Store</a> to find extensions.</p>
                    <p>To install an extension:</p>
                    <ol style='line-height:1.6; opacity:0.9;'>
                        <li>Download the extension as a <b>.crx</b> file (using a CRX Downloader extension or service).</li>
                        <li>Rename the <b>.crx</b> file to <b>.zip</b>.</li>
                        <li>Extract the zip file to a folder.</li>
                        <li>Click <b>'Load Unpacked Extension'</b> below and select that folder.</li>
                    </ol>
                    <div style='margin-top:20px;'>
                        <button class='action-btn' onclick=""post('load-extension')"" style='padding:12px 24px; font-size:16px;'>Load Unpacked Extension</button>
                        <button class='action-btn' onclick=""post('install-crx')"" style='background:transparent; border:1px solid #555; margin-left:10px;'>Install CRX Help</button>
                    </div>
                </div>

                <h3>Installed Extensions</h3>
                <div style='margin-top:20px; opacity:0.6; font-style:italic;'>
                    Extensions loaded via 'Load Unpacked' are active for this session. 
                    <br>Persistent extension management is coming soon.
                </div>
            </div>");
            
            return GenerateBaseHtml("Extensions", sb.ToString());
        }
        private void ShowAddShortcutDialog()
        {
            // Capture reference to ensure we are operating on the correct tab context when the dialog closes
            var targetTab = this.activeTab;

            using (Form prompt = new Form())
            {
                prompt.Width = 400;
                prompt.Height = 200;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Add Shortcut";
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;

                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Name:" };
                TextBox nameBox = new TextBox() { Left = 20, Top = 45, Width = 340 };
                Label urlLabel = new Label() { Left = 20, Top = 75, Text = "URL:" };
                TextBox urlBox = new TextBox() { Left = 20, Top = 100, Width = 340 };
                Button confirm = new Button() { Text = "Add", Left = 260, Width = 100, Top = 130, DialogResult = DialogResult.OK };

                confirm.Click += (s, e) => prompt.Close();

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(nameBox);
                prompt.Controls.Add(urlLabel);
                prompt.Controls.Add(urlBox);
                prompt.Controls.Add(confirm);
                prompt.AcceptButton = confirm;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    string name = nameBox.Text;
                    string urlText = urlBox.Text;
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(urlText))
                    {
                        if (!urlText.StartsWith("http")) urlText = "https://" + urlText;
                        BrowserServices.AddShortcut(name, urlText);

                        // Refresh logic using captured variable
                        ReloadAllInternalPages();
                    }
                }
            }
        }
        private void ShowEditShortcutDialog(string oldTitle, string oldUrl)
        {
            var targetTab = this.activeTab;

            using (Form prompt = new Form())
            {
                prompt.Width = 400; prompt.Height = 200;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Edit Shortcut";
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.MinimizeBox = false; prompt.MaximizeBox = false;

                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Name:" };
                TextBox nameBox = new TextBox() { Left = 20, Top = 45, Width = 340, Text = oldTitle };
                Label urlLabel = new Label() { Left = 20, Top = 75, Text = "URL:" };
                TextBox urlBox = new TextBox() { Left = 20, Top = 100, Width = 340, Text = oldUrl };
                Button confirm = new Button() { Text = "Save", Left = 260, Width = 100, Top = 130, DialogResult = DialogResult.OK };

                confirm.Click += (s, e) => prompt.Close();

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(nameBox);
                prompt.Controls.Add(urlLabel);
                prompt.Controls.Add(urlBox);
                prompt.Controls.Add(confirm);
                prompt.AcceptButton = confirm;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    string name = nameBox.Text;
                    string urlText = urlBox.Text;
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(urlText))
                    {
                        if (!urlText.StartsWith("http")) urlText = "https://" + urlText;
                        
                        // Edit logic: Remove old, Add new (simple replacement)
                        BrowserServices.RemoveShortcut(oldUrl); // Relies on URL as key mostly
                        BrowserServices.AddShortcut(name, urlText);

                        if (targetTab != null && targetTab.WebView != null)
                            ReloadAllInternalPages();
                    }
                }
            }
        }
        private void ReloadAllInternalPages()
        {
            foreach (var tab in tabs)
            {
                try
                {
                    if (tab != null && tab.WebView != null && tab.WebView.CoreWebView2 != null)
                    {
                        if (!string.IsNullOrEmpty(tab.CurrentUrl) && tab.CurrentUrl.StartsWith("quantum://"))
                        {
                            tab.WebView.Reload();
                        }
                    }
                }
                catch { } // handle potential disposed object access
            }
        }
    }
}

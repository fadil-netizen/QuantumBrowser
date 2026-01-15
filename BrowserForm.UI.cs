using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using System.Runtime.InteropServices;

namespace QuantumBrowser
{
    public partial class BrowserForm : Form
    {
        private ProgressBar loadingBar;
        private float scaleFactor = 1.0f;

        // Native Windows Support
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_FRAMECHANGED = 0x0020;

        public struct MARGINS { public int leftWidth; public int rightWidth; public int topHeight; public int bottomHeight; }

        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WS_MINIMIZEBOX = 0x20000;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_SYSMENU = 0x80000;
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_CAPTION = 0x00C00000;

        // Hit Test Values
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTMINBUTTON = 8;
        private const int HTMAXBUTTON = 9;
        private const int HTCLOSE = 20;
        private const int HTTOP = 12;
        private const int HTBOTTOM = 15;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        // Panel Borders
        private int resizeBorder = 8; 
        
        // Panels
        private Panel tabBar;
        private Panel topBar;
        private Panel webViewContainer;
        
        // Window Controls
        private Button btnCloseWindow;
        private Button btnMaximizeWindow;
        private Button btnMinimizeWindow;
        
        // Controls
        private Button btnBack;
        private Button btnForward;
        private Button btnReload;
        private Button btnHome;
        private TextBox addressBar;
        private Button btnSiteInfo;
        private Button btnMenu;
        private Button btnNewTab;
        
        // Colors
        private Color colorBgDark = Color.FromArgb(32, 33, 36);  // Chrome Tab Bar
        private Color colorBgLight = Color.FromArgb(53, 54, 58); // Chrome Toolbar
        private Color colorActiveTab = Color.FromArgb(53, 54, 58);
        private Color colorTabInactive = Color.FromArgb(32, 33, 36);
        private Color colorTabHover = Color.FromArgb(65, 65, 65);
        private Color colorTabActive = Color.FromArgb(53, 54, 58);
        private Color colorText = Color.FromArgb(232, 234, 237);
        private Color colorIcon = Color.FromArgb(189, 193, 198);

        private void InitializeUI()
        {
            // 1. Detect DPI Scale
            using (Graphics g = this.CreateGraphics())
            {
                scaleFactor = g.DpiX / 96.0f;
            }
            
            // Adjust resize sensitivity based on DPI
            resizeBorder = (int)(10 * scaleFactor);

            this.Icon = SystemIcons.Application; // Default Icon
            this.Text = "Quantum Browser";
            this.BackColor = Color.FromArgb(20, 20, 20); 
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable; 
            this.ResizeRedraw = true;
            this.Size = new Size((int)(1200 * scaleFactor), (int)(800 * scaleFactor));
            
            // CRITICAL: Add Padding to expose Form edges for resizing commands (WM_NCHITTEST)
            // This prevents WebView2 from consuming mouse events at the absolute edge.
            int edgeGap = (int)(2 * scaleFactor);
            if (edgeGap < 2) edgeGap = 2;
            this.Padding = new Padding(edgeGap); // Apply padding to all sides (Top, Bottom, Left, Right)

            // 1. CONTAINER FOR WEBVIEWS
            webViewContainer = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = colorBgDark // Match theme
            };
            this.Controls.Add(webViewContainer);
            
            // Native DWM Shadow & Frame Extension (Critical for Snap Assist & Invisible Resize Borders)
            MARGINS margins = new MARGINS { leftWidth = 1, rightWidth = 1, topHeight = 1, bottomHeight = 1 };
            try { DwmExtendFrameIntoClientArea(this.Handle, ref margins); } catch {}


            // 2. TOP TOOLBAR (Address Bar)
            int topBarHeight = (int)(40 * scaleFactor);
            topBar = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = topBarHeight, 
                BackColor = colorBgLight,
                Padding = new Padding((int)(5 * scaleFactor))
            };
            this.Controls.Add(topBar);

            // 3. TAB BAR (Window/Tabs)
            int tabBarHeight = (int)(40 * scaleFactor);
            tabBar = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = tabBarHeight, 
                BackColor = colorBgDark
            };
            tabBar.AllowDrop = true;
            tabBar.DragEnter += (s, e) => {
                 if (e.Data.GetDataPresent("QuantumTabUrl")) e.Effect = DragDropEffects.Move;
                 else e.Effect = DragDropEffects.None;
            };
            tabBar.DragDrop += (s, e) => {
                 if (e.Data.GetDataPresent("QuantumTabUrl"))
                 {
                     string url = (string)e.Data.GetData("QuantumTabUrl");
                     BrowserForm source = e.Data.GetData("SourceForm") as BrowserForm;
                     BrowserTab sourceTab = e.Data.GetData("TabInstance") as BrowserTab;
                     
                     if (source != this)
                     {
                         // Attach
                         this.AddNewTab(url);
                         
                         // Signal source to close
                         if (source != null && sourceTab != null)
                         {
                             source.CloseTab(sourceTab);
                         }
                         
                         this.Activate(); 
                     }
                 }
            };
            tabBar.MouseDown += (s, e) => { 
                if (e.Button == MouseButtons.Left && e.Clicks == 1) 
                { 
                     ReleaseCapture(); 
                     SendMessage(Handle, 0xA1, 0x2, 0); 
                }
                // Double click to maximize/restore
                if (e.Button == MouseButtons.Left && e.Clicks == 2)
                {
                    if (this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized;
                    else this.WindowState = FormWindowState.Normal;
                }
            };
            tabBar.Resize += (s, e) => UpdateTabPositions();
            this.Controls.Add(tabBar);


            // --- Initialize Window Controls (Right side of TabBar) ---
            int winBtnWidth = (int)(45 * scaleFactor);
            int winBtnHeight = tabBarHeight;
            
            btnCloseWindow = CreateWindowButton("✕", Color.Transparent, Color.Red);
            btnCloseWindow.Location = new Point(tabBar.Width - winBtnWidth, 0);
            btnCloseWindow.Click += (s, e) => Application.Exit();
            
            btnMaximizeWindow = CreateWindowButton("☐", Color.Transparent, Color.FromArgb(60, 60, 60));
            btnMaximizeWindow.Location = new Point(tabBar.Width - (winBtnWidth * 2), 0);
            btnMaximizeWindow.Click += (s, e) => { 
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
                    this.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                }
            };
            
            btnMinimizeWindow = CreateWindowButton("―", Color.Transparent, Color.FromArgb(60, 60, 60));
            btnMinimizeWindow.Location = new Point(tabBar.Width - (winBtnWidth * 3), 0);
            btnMinimizeWindow.Click += (s, e) => { this.WindowState = FormWindowState.Minimized; };

            tabBar.Controls.Add(btnCloseWindow);
            tabBar.Controls.Add(btnMaximizeWindow);
            tabBar.Controls.Add(btnMinimizeWindow);
            
            // Anchor them to right
            btnCloseWindow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximizeWindow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimizeWindow.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // --- Initialize Toolbar Contents ---
            int spacing = (int)(5 * scaleFactor);
            int btnSize = (int)(28 * scaleFactor);
            int fontSize = (int)(11 * scaleFactor);
            int x = spacing;

            // Nav Buttons
            btnBack = CreateNavButton("←", x, btnSize, fontSize);
            btnBack.Click += (s, e) => { if (CurrentWebView != null && CurrentWebView.CanGoBack) CurrentWebView.GoBack(); };
            x += btnSize + spacing;

            btnForward = CreateNavButton("→", x, btnSize, fontSize);
            btnForward.Click += (s, e) => { if (CurrentWebView != null && CurrentWebView.CanGoForward) CurrentWebView.GoForward(); };
            x += btnSize + spacing;

            btnReload = CreateNavButton("↻", x, btnSize, fontSize);
            btnReload.Click += (s, e) => { if (CurrentWebView != null) CurrentWebView.Reload(); };
            x += btnSize + spacing;

            // Address Bar
            btnMenu = CreateNavButton("⋮", topBar.Width - btnSize - spacing, btnSize, fontSize);
            btnMenu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMenu.Click += ShowMenu; // Now shows Context Menu with Incognito option

            // Rounded Address Bar Container
            Panel addressBarBg = new Panel
            {
                Location = new Point(x, (int)(4 * scaleFactor)),
                Height = topBarHeight - (int)(10 * scaleFactor),
                Width = (btnMenu.Left - spacing) - x,
                BackColor = Color.FromArgb(32, 33, 36), // Darker than toolbar
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            // Round circles for address bar manually painted in paint event if needed, but Panel simple is okay.
            // Let's just use TextBox with no border inside the panel
            
            // Site Info Button
            btnSiteInfo = new Button
            {
                Text = "🔒",
                Dock = DockStyle.Left,
                Width = (int)(30 * scaleFactor),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(60, 60, 60) },
                ForeColor = Color.Green,
                Font = new Font("Segoe UI", 10 * scaleFactor),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnSiteInfo.Click += ShowSiteInfo;
            addressBarBg.Controls.Add(btnSiteInfo);

            addressBar = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(32, 33, 36),
                ForeColor = colorText,
                Font = new Font("Segoe UI", 10.5f * scaleFactor),
                Location = new Point(btnSiteInfo.Width + (int)(5 * scaleFactor), (int)(4 * scaleFactor)),
                Width = addressBarBg.Width - btnSiteInfo.Width - (int)(15 * scaleFactor),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            addressBar.KeyDown += AddressBar_KeyDown;
            
            addressBarBg.Controls.Add(addressBar);
            topBar.Controls.Add(addressBarBg);
            
            // Loading Bar
            loadingBar = new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                Height = (int)(2 * scaleFactor),
                Dock = DockStyle.Bottom,
                Visible = false
            };
            topBar.Controls.Add(loadingBar);
            
            // Add buttons
            topBar.Controls.Add(btnBack);
            topBar.Controls.Add(btnForward);
            topBar.Controls.Add(btnReload);
            topBar.Controls.Add(btnMenu);
            
            // --- New Tab Button ---
            btnNewTab = new Button
            {
                Text = "+",
                Size = new Size((int)(28 * scaleFactor), (int)(28 * scaleFactor)),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(80, 80, 80) },
                ForeColor = colorText,
                Font = new Font("Segoe UI", 12 * scaleFactor),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnNewTab.Click += (s, e) => AddNewTab();
            tabBar.Controls.Add(btnNewTab);

            // Apply Initial Theme
            ApplyTheme();

            // Global Key Handler
            this.KeyDown += delegate(object sender, KeyEventArgs e) {
                if (e.KeyCode == Keys.Escape && _isFullScreen)
                {
                    ExitFullScreenMode();
                }
            };
        }

        public void ApplyTheme()
        {
            // Defaults (Dark)
            colorBgDark = Color.FromArgb(32, 33, 36);
            colorBgLight = Color.FromArgb(53, 54, 58);
            colorActiveTab = Color.FromArgb(53, 54, 58);
            colorTabInactive = Color.FromArgb(32, 33, 36);
            colorTabHover = Color.FromArgb(65, 65, 65);
            colorTabActive = Color.FromArgb(53, 54, 58);
            colorText = Color.FromArgb(232, 234, 237);
            colorIcon = Color.FromArgb(189, 193, 198);

            // Load from Config if available
            if (!IsIncognito && appConfig != null && appConfig.ContainsKey("Theme"))
            {
               string theme = appConfig["Theme"];
               if (theme == "Light")
               {
                   colorBgDark = Color.FromArgb(222, 225, 230);
                   colorBgLight = Color.White;
                   colorActiveTab = Color.White;
                   colorTabInactive = Color.FromArgb(222, 225, 230);
                   colorTabHover = Color.FromArgb(235, 237, 240);
                   colorTabActive = Color.White;
                   colorText = Color.Black;
                   colorIcon = Color.FromArgb(95, 99, 104);
               }
               else if (theme == "Custom" && appConfig.ContainsKey("ThemeColor"))
               {
                   try {
                       Color baseColor = ColorTranslator.FromHtml(appConfig["ThemeColor"]);
                       colorBgDark = baseColor;
                       colorBgLight = ControlPaint.Light(baseColor, 0.1f);
                       colorActiveTab = colorBgLight;
                       colorTabInactive = baseColor;
                       colorTabHover = ControlPaint.Light(baseColor, 0.05f);
                       
                       // Contrast Check
                       double luminance = (0.299 * baseColor.R + 0.587 * baseColor.G + 0.114 * baseColor.B) / 255;
                       if (luminance > 0.5) 
                       {
                           colorText = Color.Black;
                           colorIcon = Color.FromArgb(60, 60, 60);
                       }
                       else 
                       {
                           colorText = Color.White;
                           colorIcon = Color.FromArgb(200, 200, 200);
                       }
                   } catch {}
               }
            }
            else if (IsIncognito)
            {
                 // Enforce Incognito Dark Mode
                 colorBgDark = Color.FromArgb(32, 33, 36); // Chrome Dark
                 colorBgLight = Color.FromArgb(50, 50, 50); // Slightly Lighter
                 colorActiveTab = Color.FromArgb(50, 50, 50);
                 colorTabInactive = Color.FromArgb(32, 33, 36);
                 colorTabHover = Color.FromArgb(60, 60, 60);
                 colorTabActive = Color.FromArgb(50, 50, 50);
                 colorText = Color.White;
                 colorIcon = Color.White;
            }

            // Apply to Controls
            this.BackColor = colorBgDark;
            if(topBar != null) topBar.BackColor = colorBgLight;
            if(tabBar != null) tabBar.BackColor = colorBgDark;
            
            if (btnBack != null) btnBack.ForeColor = colorIcon;
            if (btnForward != null) btnForward.ForeColor = colorIcon;
            if (btnReload != null) btnReload.ForeColor = colorIcon;
            if (btnMenu != null) btnMenu.ForeColor = colorIcon;
            if (btnNewTab != null) btnNewTab.ForeColor = colorText;
            
            // Address Bar Update
            if (addressBar != null && addressBar.Parent != null) { 
                addressBar.ForeColor = colorText; 
                addressBar.BackColor = (colorBgLight.R + colorBgLight.G + colorBgLight.B) > 382 ? ControlPaint.Dark(colorBgLight, 0.05f) : ControlPaint.Light(colorBgLight, 0.1f);
                addressBar.Parent.BackColor = addressBar.BackColor;
            }
            if(btnSiteInfo != null && !btnSiteInfo.IsDisposed)
            {
                btnSiteInfo.BackColor = Color.Transparent; // Or match addressBarBg
            }
            
            // Window Controls
            if(btnCloseWindow != null) btnCloseWindow.ForeColor = colorText;
            if(btnMaximizeWindow != null) btnMaximizeWindow.ForeColor = colorText;
            if(btnMinimizeWindow != null) btnMinimizeWindow.ForeColor = colorText;

            // Invalidate to redraw tabs
            if (tabBar != null) tabBar.Invalidate(true);
            this.Invalidate(true);
        }

        private Button CreateNavButton(string text, int x, int size, int fontSize)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, (topBar.Height - size) / 2),
                Size = new Size(size, size),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", fontSize),
                BackColor = Color.Transparent,
                ForeColor = colorIcon,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
            return btn;
        }
        
        private Button CreateWindowButton(string text, Color baseColor, Color hoverColor)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size((int)(45 * scaleFactor), tabBar.Height),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9 * scaleFactor),
                BackColor = baseColor,
                ForeColor = colorText,
                Cursor = Cursors.Default
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = hoverColor;
            return btn;
        }

        // Win32 API for Dragging - Deprecated in favor of native HTCAPTION
        // [System.Runtime.InteropServices.DllImport("user32.dll")]
        // public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        // [System.Runtime.InteropServices.DllImport("user32.dll")]
        // public static extern bool ReleaseCapture();

        // Win32 API for Dragging - Deprecated in favor of native HTCAPTION
        // ... (API definitions kept if needed for reference) ...

                                        private void ShowMenu(object sender, EventArgs e)
        {
            // Exact Chrome Dark Mode Palette from Screenshot
            Color chromeBg = Color.FromArgb(41, 42, 45);       // #292A2D
            Color chromeText = Color.FromArgb(232, 234, 237);  // #E8EAED
            Color chromeHover = Color.FromArgb(60, 64, 67);    // #3C4043
            Color chromeSep = Color.FromArgb(74, 76, 80);      // #4A4C50

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Renderer = new ToolStripProfessionalRenderer(new CustomColorTable(IsIncognito));
            
            bool isDark = IsIncognito || (colorBgDark.R < 100); 
            if (isDark)
            {
                menu.BackColor = chromeBg;
                menu.ForeColor = chromeText;
            }
            else
            {
                menu.BackColor = Color.White;
                menu.ForeColor = Color.Black;
            }

            menu.ShowImageMargin = false; 
            menu.Font = new Font("Segoe UI", 9.5f * scaleFactor);
            menu.Padding = new Padding(0, 8, 0, 8); 
            menu.MinimumSize = new Size((int)(312 * scaleFactor), 0);
            menu.AutoSize = true;

            // Helper for Menu Items (C# 5 Delegate)
            Func<string, EventHandler, string, ToolStripMenuItem> AddItem = delegate(string text, EventHandler onClick, string shortcut)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(text);
                if (onClick != null) item.Click += onClick;
                if (!string.IsNullOrEmpty(shortcut)) item.ShortcutKeyDisplayString = shortcut;
                item.ForeColor = menu.ForeColor;
                item.Padding = new Padding(0, 4, 0, 4);
                item.TextAlign = ContentAlignment.MiddleLeft;
                menu.Items.Add(item);
                return item;
            };

            // 1. Session Control
            AddItem(T("New tab"), delegate { AddNewTab(); }, "Ctrl+T");
            AddItem(T("New window"), delegate { OpenNewWindow(false); }, "Ctrl+N");
            AddItem(T("New Incognito window"), delegate { OpenNewWindow(true); }, "Ctrl+Shift+N");
            
            menu.Items.Add(new ToolStripSeparator { BackColor = menu.BackColor, ForeColor = chromeSep });

            // 2. Main Features
            AddItem(T("Passwords and autofill"), delegate { AddNewTab("quantum://settings/passwords"); }, ""); 
            AddItem(T("History"), delegate { AddNewTab("quantum://history"); }, "Ctrl+H");
            AddItem(T("Downloads"), delegate { AddNewTab("quantum://downloads"); }, "Ctrl+J");
            AddItem(T("Bookmarks"), delegate { AddNewTab("quantum://bookmarks"); }, "Ctrl+Shift+O");
            
            var itemTabGroups = AddItem(T("Tab groups"), null, "");
            itemTabGroups.DropDownItems.Add("Empty"); 

            var itemExt = AddItem(T("Extensions"), null, "");
            itemExt.DropDownItems.Add("Manage extensions", null, delegate { AddNewTab("quantum://extensions"); });
            itemExt.DropDownItems.Add("Visit Chrome Web Store", null, delegate { AddNewTab("https://chrome.google.com/webstore"); });

            AddItem(T("Delete browsing data..."), delegate {
                 if(webView != null && webView.CoreWebView2 != null) webView.CoreWebView2.Profile.ClearBrowsingDataAsync();
                 MessageBox.Show(T("Browsing data cleared."));
            }, "Ctrl+Shift+Del");

            menu.Items.Add(new ToolStripSeparator { ForeColor = chromeSep });

            // 3. Zoom Control (Panel with matching style)
            Panel zoomPanel = new Panel { BackColor = Color.Transparent, Size = new Size((int)(310 * scaleFactor), 36) };
            Label lblZoom = new Label { Text = T("Zoom"), ForeColor = menu.ForeColor, Location = new Point(16, 7), AutoSize = true, Font = menu.Font };
            
            int btnSize = 34; 
            int rightStart = zoomPanel.Width - 10;

            Button btnFull = new Button { Text = "⛶", Size = new Size(btnSize, btnSize), Location = new Point(rightStart - btnSize, 1), FlatStyle = FlatStyle.Flat, ForeColor = menu.ForeColor, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter };
            btnFull.FlatAppearance.BorderSize = 0; btnFull.FlatAppearance.MouseOverBackColor = chromeHover;
            btnFull.Click += delegate { 
                if(this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized; 
                else this.WindowState = FormWindowState.Normal; 
            };

            Button btnPlus = new Button { Text = "+", Size = new Size(btnSize, btnSize), Location = new Point(btnFull.Left - btnSize - 6, 1), FlatStyle = FlatStyle.Flat, ForeColor = menu.ForeColor, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter };
            btnPlus.FlatAppearance.BorderSize = 0; btnPlus.FlatAppearance.MouseOverBackColor = chromeHover;
            btnPlus.Click += delegate { if(CurrentWebView != null) CurrentWebView.ZoomFactor += 0.1; };

            Label lblPct = new Label { Text = "100%", ForeColor = menu.ForeColor, AutoSize = false, Size = new Size(50, btnSize), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(btnPlus.Left - 50, 1), Font = menu.Font };

            Button btnMinus = new Button { Text = "−", Size = new Size(btnSize, btnSize), Location = new Point(lblPct.Left - btnSize, 1), FlatStyle = FlatStyle.Flat, ForeColor = menu.ForeColor, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter };
            btnMinus.FlatAppearance.BorderSize = 0; btnMinus.FlatAppearance.MouseOverBackColor = chromeHover;
            btnMinus.Click += delegate { if(CurrentWebView != null) CurrentWebView.ZoomFactor -= 0.1; };

            zoomPanel.Controls.AddRange(new Control[] { lblZoom, btnFull, btnPlus, lblPct, btnMinus });
            menu.Items.Add(new ToolStripControlHost(zoomPanel) { Padding = Padding.Empty, Margin = Padding.Empty });

            menu.Items.Add(new ToolStripSeparator { ForeColor = chromeSep });

            // 4. Tools
            AddItem(T("Print..."), delegate { if (CurrentWebView != null) CurrentWebView.ExecuteScriptAsync("window.print();"); }, "Ctrl+P");
            AddItem(T("Translate page..."), delegate { TranslateCurrentPage(); }, "");
            AddItem(T("Search with Google Lens"), null, "");
            
            var itemFind = AddItem(T("Find and edit"), null, "");
            itemFind.DropDownItems.Add("Find...", null, delegate { /* Find logic */ }); 
            
            var itemCast = AddItem(T("Cast, save, and share"), null, "");
            itemCast.DropDownItems.Add("Cast...", null, delegate { /* Cast logic */ });

            var itemMore = AddItem(T("More tools"), null, "");
            itemMore.DropDownItems.Add(T("Developer tools"), null, delegate { if(CurrentWebView != null) CurrentWebView.CoreWebView2.OpenDevToolsWindow(); });
            itemMore.DropDownItems.Add(T("Network Log"), null, delegate { AddNewTab("quantum://network-log"); });

            menu.Items.Add(new ToolStripSeparator { ForeColor = chromeSep });

            // 5. System
            var itemHelp = AddItem(T("Help"), null, "");
            itemHelp.DropDownItems.Add("About Quantum Browser", null, delegate { AddNewTab("quantum://settings"); });

            AddItem(T("Settings"), delegate { AddNewTab("quantum://settings"); }, "");
            AddItem(T("Exit"), delegate { Application.Exit(); }, "");

            menu.Show(btnMenu, new Point(btnMenu.Width, btnMenu.Height), ToolStripDropDownDirection.BelowLeft);
        }

        private void OpenNewWindow(bool incognito)
        {
            var newForm = new BrowserForm(incognito);
            newForm.Show();
        }

        public class CustomColorTable : ProfessionalColorTable
        {
            bool isDark;
            Color chromeBg = Color.FromArgb(41, 42, 45);       
            Color chromeHover = Color.FromArgb(60, 64, 67);    
            Color chromeSep = Color.FromArgb(74, 76, 80);

            public CustomColorTable(bool dark) { isDark = dark; }
            
            public override Color MenuItemSelected { get { return isDark ? chromeHover : base.MenuItemSelected; } }
            public override Color MenuItemBorder { get { return Color.Transparent; } }
            public override Color ToolStripDropDownBackground { get { return isDark ? chromeBg : Color.White; } }
            public override Color ImageMarginGradientBegin { get { return isDark ? chromeBg : base.ImageMarginGradientBegin; } }
            public override Color ImageMarginGradientMiddle { get { return isDark ? chromeBg : base.ImageMarginGradientMiddle; } }
            public override Color ImageMarginGradientEnd { get { return isDark ? chromeBg : base.ImageMarginGradientEnd; } }
            public override Color SeparatorDark { get { return isDark ? chromeSep : base.SeparatorDark; } }
            public override Color MenuBorder { get { return isDark ? Color.FromArgb(60, 60, 60) : base.MenuBorder; } }
        }

private void ShowSiteInfo(object sender, EventArgs e)
        {
            if (activeTab == null) return;
            
            Form popup = new Form();
            popup.Size = new Size(382, 450); // Match React component width roughly
            popup.StartPosition = FormStartPosition.Manual;
            Point pt = btnSiteInfo.PointToScreen(new Point(0, btnSiteInfo.Height + 5));
            popup.Location = pt;
            popup.FormBorderStyle = FormBorderStyle.None;
            popup.ShowInTaskbar = false;
            // Close on blur
            popup.Deactivate += (s, ev) => popup.Close();
            
            WebView2 wv = new WebView2();
            wv.Dock = DockStyle.Fill;
            popup.Controls.Add(wv);
            
            popup.Load += async (s, ev) => {
                if (popup.IsDisposed) return;
                try {
                    await wv.EnsureCoreWebView2Async();
                    
                    // Handle Messages from React UI
                    wv.CoreWebView2.WebMessageReceived += (msgSender, msgArgs) => {
                         try {
                             string msg = msgArgs.TryGetWebMessageAsString();
                             if (!string.IsNullOrEmpty(msg) && msg.StartsWith("toggle:")) {
                                 string[] parts = msg.Split(':');
                                 if (parts.Length > 1) {
                                     string[] val = parts[1].Split('|'); // key|true
                                     if (activeTab != null && !string.IsNullOrEmpty(activeTab.CurrentUrl)) {
                                         string host = new Uri(activeTab.CurrentUrl).Host;
                                         if (!BrowserServices.SitePermissionMap.ContainsKey(host)) 
                                             BrowserServices.SitePermissionMap[host] = new SitePermissions();
                                         
                                         var p = BrowserServices.SitePermissionMap[host];
                                         bool state = val[1] == "true";
                                         
                                         if (val[0] == "camera") p.Camera = state;
                                         else if (val[0] == "microphone") p.Microphone = state;
                                         else if (val[0] == "location") p.Location = state;
                                         else if (val[0] == "notifications") p.Notifications = state;
                                         else if (val[0] == "speaker") p.Sound = state;
                                     }
                                 }
                             }
                         } catch {}
                    };

                    // 1. Force Dark Background
                    wv.DefaultBackgroundColor = System.Drawing.Color.FromArgb(255, 32, 33, 36);

                    // 2. Prepare Data
                    string currentUrl = activeTab.CurrentUrl ?? "";
                    bool isSecure = currentUrl.StartsWith("https");
                    string siteHost = "";
                    try { siteHost = new Uri(currentUrl).Host; } catch { siteHost = currentUrl; }
                    
                    // Permissions
                    bool pCam=true, pMic=true, pLoc=true;
                    if (BrowserServices.SitePermissionMap.ContainsKey(siteHost)) {
                        var p = BrowserServices.SitePermissionMap[siteHost];
                        pCam = p.Camera; pMic = p.Microphone; pLoc = p.Location;
                    }

                    // 3. Construct HTML directly in C# (Fail-safe)
                    string html = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <style>
        body { margin: 0; padding: 0; background-color: #202124; color: #e8eaed; font-family: 'Segoe UI', Tahoma, sans-serif; font-size: 13px; user-select: none; overflow: hidden; cursor: default; }
        ::-webkit-scrollbar { width: 8px; } ::-webkit-scrollbar-track { background: transparent; } ::-webkit-scrollbar-thumb { background: #5f6368; border-radius: 4px; }
        
        .container { display: flex; flex-direction: column; height: 100vh; }
        .header { padding: 16px 16px 8px 16px; }
        .title-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
        .site-ident { font-size: 15px; font-weight: 600; color: #e8eaed; }
        .close-btn { width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; cursor: pointer; }
        .close-btn:hover { background-color: #3c4043; }
        
        .row-btn { display: flex; align-items: center; padding: 10px 16px; cursor: pointer; transition: background 0.1s; }
        .row-btn:hover { background-color: #35363a; }
        .row-icon { width: 20px; height: 20px; margin-right: 12px; fill: #9aa0a6; display: flex; align-items: center; justify-content: center; }
        .row-text { flex: 1; font-size: 13px; }
        .row-end { fill: #9aa0a6; width: 16px; height: 16px; }
        
        .secure-text { color: " + (isSecure ? "#81c995" : "#f28b82") + @"; font-weight: 500; }
        
        .separator { height: 1px; background-color: #5f6368; opacity: 0.3; margin: 4px 0; }
        
        .perm-section { padding: 4px 0; }
        .perm-row { display: flex; align-items: center; justify-content: space-between; padding: 8px 16px; }
        .perm-row:hover { background-color: #35363a; }
        .perm-left { display: flex; align-items: center; gap: 12px; }
        
        /* Toggle */
        .toggle { position: relative; width: 30px; height: 16px; display: inline-block; cursor: pointer;}
        .toggle input { opacity: 0; width: 0; height: 0; }
        .slider { position: absolute; top: 0; left: 0; right: 0; bottom: 0; background-color: #5f6368; transition: .2s; border-radius: 16px; }
        .slider:before { position: absolute; content: ''; height: 12px; width: 12px; left: 2px; bottom: 2px; background-color: #202124; transition: .2s; border-radius: 50%; }
        input:checked + .slider { background-color: #8ab4f8; }
        input:checked + .slider:before { transform: translateX(14px); }
        
        .reset-btn { margin: 8px 16px 12px 16px; padding: 6px 16px; border: 1px solid #5f6368; border-radius: 16px; background: transparent; color: #8ab4f8; font-family: inherit; font-size: 12px; cursor: pointer; width: fit-content; }
        .reset-btn:hover { background-color: #28292c; }
    </style>
</head>
<body>
    <div class='container'>
        <!-- Header -->
        <div class='header'>
            <div class='title-row'>
                <div class='site-ident'>" + siteHost + @"</div>
                <div class='close-btn' onclick='window.chrome.webview.postMessage(""close"")'>
                    <svg width='16' height='16' viewBox='0 0 24 24' fill='#e8eaed'><path d='M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z'/></svg>
                </div>
            </div>
            <div class='secure-text'>" + (isSecure ? "Connection is secure" : "Connection is Not Secure") + @"</div>
            <div style='color:#9aa0a6; font-size:12px; margin-top:2px;'>Certificate is valid</div>
        </div>

        <div class='separator'></div>

        <!-- Permissions -->
        <div class='perm-section'>" + 
        (pCam ? @"
            <div class='perm-row'>
                <div class='perm-left'>
                    <svg class='row-icon' viewBox='0 0 24 24'><path d='M9.4 6.6L9 8H6a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V10a2 2 0 0 0-2-2h-3l-2-.6-1-1.4h-5l-.6 1.6z M12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5z'/></svg>
                    <span>Camera</span>
                </div>
                <label class='toggle'><input type='checkbox' checked onchange='toggle(""camera"", this.checked)'><span class='slider'></span></label>
            </div>" : "") + 
        (pMic ? @"
            <div class='perm-row'>
                <div class='perm-left'>
                    <svg class='row-icon' viewBox='0 0 24 24'><path d='M12 14c1.66 0 3-1.34 3-3V5c0-1.66-1.34-3-3-3S9 3.66 9 5v6c0 1.66 1.34 3 3 3z M17 11c0 2.76-2.24 5-5 5s-5-2.24-5-5H5c0 3.53 2.61 6.43 6 6.92V21h2v-3.08c3.39-.49 6-3.39 6-6.92h-2z'/></svg>
                    <span>Microphone</span>
                </div>
                <label class='toggle'><input type='checkbox' checked onchange='toggle(""microphone"", this.checked)'><span class='slider'></span></label>
            </div>" : "") +
        (pLoc ? @"
            <div class='perm-row'>
                <div class='perm-left'>
                    <svg class='row-icon' viewBox='0 0 24 24'><path d='M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z'/></svg>
                    <span>Location</span>
                </div>
                <label class='toggle'><input type='checkbox' checked onchange='toggle(""location"", this.checked)'><span class='slider'></span></label>
            </div>" : "") + 
        @"
            <div class='perm-row'>
                <div class='perm-left'>
                    <svg class='row-icon' viewBox='0 0 24 24'><path d='M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z'/></svg>
                    <span>Notifications</span>
                </div>
                <label class='toggle'><input type='checkbox' checked onchange='toggle(""notifications"", this.checked)'><span class='slider'></span></label>
            </div>" +
        @"
            <div class='perm-row'>
                <div class='perm-left'>
                    <svg class='row-icon' viewBox='0 0 24 24'><path d='M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z'/></svg>
                    <span>Sound</span>
                </div>
                <label class='toggle'><input type='checkbox' checked onchange='toggle(""speaker"", this.checked)'><span class='slider'></span></label>
            </div>" + @"
            
            <button class='reset-btn'>Reset permission</button>
        </div>

        <div class='separator'></div>

        <!-- Links -->
        <div class='row-btn'>
            <svg class='row-icon' viewBox='0 0 24 24'><path d='M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8z'/><path d='M11 7h2v6h-2zm0 8h2v2h-2z'/></svg>
            <div class='row-text'>Cookies and site data</div>
            <svg class='row-end' viewBox='0 0 24 24'><path d='M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z'/></svg>
        </div>
        <div class='row-btn'>
            <svg class='row-icon' viewBox='0 0 24 24'><path d='M19.43 12.98c.04-.32.07-.64.07-.98s-.03-.66-.07-.98l2.11-1.65c.19-.15.24-.42.12-.64l-2-3.46c-.12-.22-.39-.3-.61-.22l-2.49 1c-.52-.4-1.08-.73-1.69-.98l-.38-2.65C14.46 2.18 14.25 2 14 2h-4c-.25 0-.46.18-.49.42l-.38 2.65c-.61.25-1.17.59-1.69.98l-2.49-1c-.23-.09-.49 0-.61.22l-2 3.46c-.13.22-.07.49.12.64l2.11 1.65c-.04.32-.07.65-.07.98s.03.66.07.98l-2.11 1.65c-.19.15-.24.42-.12.64l2 3.46c.12.22.39.3.61.22l2.49-1c.52.4 1.08.73 1.69.98l.38 2.65c.03.24.24.42.49.42h4c.25 0 .46-.18.49-.42l.38-2.65c.61-.25 1.17-.59 1.69-.98l2.49 1c.23.09.49 0 .61-.22l2-3.46c.12-.22.07-.49-.12-.64l-2.11-1.65zM12 15.5c-1.93 0-3.5-1.57-3.5-3.5s1.57-3.5 3.5-3.5 3.5 1.57 3.5 3.5-1.57 3.5-3.5 3.5z'/></svg>
            <div class='row-text'>Site settings</div>
            <svg class='row-end' viewBox='0 0 24 24'><path d='M19 19H5V5h7V3H5c-1.11 0-2 .9-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2v-7h-2v7zM14 3v2h3.59l-9.83 9.83 1.41 1.41L19 6.41V10h2V3h-7z'/></svg>
        </div>
        <div class='row-btn'>
            <svg class='row-icon' viewBox='0 0 24 24'><path d='M11 18h2v-2h-2v2zm1-16C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm0-14c-2.21 0-4 1.79-4 4h2c0-1.1.9-2 2-2s2 .9 2 2c0 2-3 1.75-3 5h2c0-2.25 3-2.5 3-5 0-2.21-1.79-4-4-4z'/></svg>
            <div class='row-text'>About this page</div>
             <svg class='row-end' viewBox='0 0 24 24'><path d='M19 19H5V5h7V3H5c-1.11 0-2 .9-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2v-7h-2v7zM14 3v2h3.59l-9.83 9.83 1.41 1.41L19 6.41V10h2V3h-7z'/></svg>
        </div>
    </div>
    <script>
        function toggle(id, val) {
             if(window.chrome && window.chrome.webview) window.chrome.webview.postMessage('toggle:'+id+'|'+val);
        }
    </script>
</body>
</html>";
                    wv.NavigateToString(html);
                } catch {}
            };
            
            popup.Show(this);
        }

        private void UpdateSecurityIcon(string url)
        {
            if (btnSiteInfo == null) return;
            if (string.IsNullOrEmpty(url)) return;

            if (url.StartsWith("https://"))
            {
                btnSiteInfo.Text = "🔒";
                btnSiteInfo.ForeColor = Color.Green;
            }
            else if (url.StartsWith("quantum://") || url.StartsWith("about:") || url.StartsWith("chrome://"))
            {
                btnSiteInfo.Text = "🛡";
                btnSiteInfo.ForeColor = Color.Blue;
            }
            else
            {
                btnSiteInfo.Text = "🔓";
                btnSiteInfo.ForeColor = Color.Red;
            }
        }

        // --- Native Window Logic for Snap Layouts & Resizing ---
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;
                cp.Style |= WS_MAXIMIZEBOX;
                cp.Style |= WS_THICKFRAME;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case WM_NCCALCSIZE:
                    if (m.WParam != IntPtr.Zero)
                    {
                        // Setting m.Result to 0 tells Windows the client area is the entire window.
                        // This effectively hides the native title bar and borders.
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM_NCHITTEST:
                    base.WndProc(ref m);
                    if (m.Result.ToInt32() == HTCLIENT)
                    {
                        Point screenPoint = new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16);
                        Point clientPoint = this.PointToClient(screenPoint);

                        // Window Controls Hit Testing (Crucial for Snap Layouts)
                        if (btnMaximizeWindow != null && btnMaximizeWindow.Bounds.Contains(clientPoint))
                        {
                            m.Result = (IntPtr)HTMAXBUTTON;
                            return;
                        }
                        if (btnMinimizeWindow != null && btnMinimizeWindow.Bounds.Contains(clientPoint))
                        {
                            m.Result = (IntPtr)HTMINBUTTON;
                            return;
                        }
                        if (btnCloseWindow != null && btnCloseWindow.Bounds.Contains(clientPoint))
                        {
                            m.Result = (IntPtr)HTCLOSE;
                            return;
                        }

                        // Resize Borders
                        if (WindowState == FormWindowState.Normal)
                        {
                            if (clientPoint.X <= resizeBorder && clientPoint.Y <= resizeBorder) m.Result = (IntPtr)HTTOPLEFT;
                            else if (clientPoint.X >= ClientSize.Width - resizeBorder && clientPoint.Y <= resizeBorder) m.Result = (IntPtr)HTTOPRIGHT;
                            else if (clientPoint.X <= resizeBorder && clientPoint.Y >= ClientSize.Height - resizeBorder) m.Result = (IntPtr)HTBOTTOMLEFT;
                            else if (clientPoint.X >= ClientSize.Width - resizeBorder && clientPoint.Y >= ClientSize.Height - resizeBorder) m.Result = (IntPtr)HTBOTTOMRIGHT;
                            else if (clientPoint.X <= resizeBorder) m.Result = (IntPtr)HTLEFT;
                            else if (clientPoint.X >= ClientSize.Width - resizeBorder) m.Result = (IntPtr)HTRIGHT;
                            else if (clientPoint.Y <= resizeBorder) m.Result = (IntPtr)HTTOP;
                            else if (clientPoint.Y >= ClientSize.Height - resizeBorder) m.Result = (IntPtr)HTBOTTOM;
                        }

                        // Caption / Drag Area (Top Bar and Tab Bar empty space)
                        if (m.Result.ToInt32() == HTCLIENT) // If still client, check if proper drag area
                        {
                            bool isOverContent = false;
                            
                            // Check if over a tab? We let tabs handle their own clicks (HTCLIENT)
                            // But empty space in tabBar should be draggable
                            // Since standard hit test passes HTCLIENT for child controls unless we intersect them
                            // Simple logic: If Y is within Top Area and NOT over a known interactive control, return HTCAPTION
                            
                            if (clientPoint.Y <= (topBar.Height + tabBar.Height))
                            {
                                // We are in the top area. 
                                // WinForms usually handles child controls (buttons) before this if we return HTCLIENT.
                                // However, we want to allow dragging on the empty parts of the panel.
                                // Since we already checked Min/Max/Close, we are safe there.
                                
                                // If it's effectively "background" of topBar or tabBar => HTCAPTION
                                m.Result = (IntPtr)HTCAPTION;
                            }
                        }
                    }
                    return;
            }
            base.WndProc(ref m);
        }
    }
}

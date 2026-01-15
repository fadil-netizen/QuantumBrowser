
$file = "c:\Users\fsl\Downloads\experimen\MyApplication\BrowserForm.UI.cs"
$content = Get-Content -Path $file -Raw

$startMarker = "private void ShowMenu(object sender, EventArgs e)"
$endMarker = "private void ShowSiteInfo(object sender, EventArgs e)"

$startIndex = $content.IndexOf($startMarker)
$endIndex = $content.IndexOf($endMarker)

if ($startIndex -ge 0 -and $endIndex -gt $startIndex) {
    $before = $content.Substring(0, $startIndex)
    $after = $content.Substring($endIndex)
    
    $newCode = @"
        private void ShowMenu(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Renderer = new ToolStripProfessionalRenderer(new CustomColorTable(IsIncognito));
            
            // Standard Dark Theme
            bool isDark = IsIncognito || (colorBgDark.R < 100); 
            
            if (isDark)
            {
                menu.BackColor = Color.FromArgb(41, 42, 45); // Standard Dark
                menu.ForeColor = Color.FromArgb(232, 234, 237);
            }
            else
            {
                menu.BackColor = Color.White;
                menu.ForeColor = Color.Black;
            }

            menu.ShowImageMargin = false; 
            menu.Font = new Font("Segoe UI", 9.5f * scaleFactor);
            menu.Padding = new Padding(0, 8, 0, 8);
            menu.MinimumSize = new Size((int)(300 * scaleFactor), 0);
            menu.AutoSize = true;

            // Helper (Local Function for cleanliness)
            ToolStripMenuItem AddItem(string text, EventHandler onClick, string shortcut = null)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(text);
                if (onClick != null) item.Click += onClick;
                if (!string.IsNullOrEmpty(shortcut)) item.ShortcutKeyDisplayString = shortcut;
                item.ForeColor = menu.ForeColor;
                item.Padding = new Padding(0, 4, 0, 4);
                item.TextAlign = ContentAlignment.MiddleLeft;
                menu.Items.Add(item);
                return item;
            }

            // 1. Session
            AddItem(T("New tab"), (s, ev) => AddNewTab(), "Ctrl+T");
            AddItem(T("New window"), (s, ev) => OpenNewWindow(false), "Ctrl+N");
            AddItem(T("New Incognito window"), (s, ev) => OpenNewWindow(true), "Ctrl+Shift+N");
            
            menu.Items.Add(new ToolStripSeparator { BackColor = menu.BackColor, ForeColor = Color.FromArgb(80, 80, 80) });

            // 2. Features
            AddItem(T("Passwords and autofill"), (s, ev) => AddNewTab("quantum://settings/passwords"));
            AddItem(T("History"), (s, ev) => AddNewTab("quantum://history"), "Ctrl+H");
            AddItem(T("Downloads"), (s, ev) => AddNewTab("quantum://downloads"), "Ctrl+J");
            AddItem(T("Bookmarks"), (s, ev) => AddNewTab("quantum://bookmarks"), "Ctrl+Shift+O");
            
            var itemTabGroups = AddItem(T("Tab groups"), null);
            itemTabGroups.DropDownItems.Add("Empty");

            var itemExt = AddItem(T("Extensions"), null);
            itemExt.DropDownItems.Add("Manage extensions", null, (s, ev) => AddNewTab("quantum://extensions"));
            itemExt.DropDownItems.Add("Visit Chrome Web Store", null, (s, ev) => AddNewTab("https://chrome.google.com/webstore"));

            AddItem(T("Delete browsing data..."), (s, ev) => {
                 if(webView != null && webView.CoreWebView2 != null) webView.CoreWebView2.Profile.ClearBrowsingDataAsync();
                 MessageBox.Show(T("Browsing data cleared."));
            }, "Ctrl+Shift+Del");

            menu.Items.Add(new ToolStripSeparator { ForeColor = Color.FromArgb(80, 80, 80) });

            // 3. Zoom
            Panel zoomPanel = new Panel { BackColor = Color.Transparent, Size = new Size((int)(290 * scaleFactor), 36) };
            Label lblZoom = new Label { Text = T("Zoom"), ForeColor = menu.ForeColor, Location = new Point(16, 7), AutoSize = true, Font = menu.Font };
            
            // Right-aligned controls
            int btnSize = 34; 
            int rightStart = zoomPanel.Width - 10;
            Color hoverColor = Color.FromArgb(60, 64, 67);

            // Fullscreen
            Button btnFull = new Button { Text = "⛶", Size = new Size(btnSize, btnSize), Location = new Point(rightStart - btnSize, 1), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter, ForeColor = menu.ForeColor };
            btnFull.FlatAppearance.BorderSize = 0; btnFull.FlatAppearance.MouseOverBackColor = hoverColor;
            btnFull.Click += (s, ev) => { if(this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized; else this.WindowState = FormWindowState.Normal; };

            // Plus
            Button btnPlus = new Button { Text = "+", Size = new Size(btnSize, btnSize), Location = new Point(btnFull.Left - btnSize - 6, 1), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter, ForeColor = menu.ForeColor };
            btnPlus.FlatAppearance.BorderSize = 0; btnPlus.FlatAppearance.MouseOverBackColor = hoverColor;
            btnPlus.Click += (s, ev) => { if(CurrentWebView != null) CurrentWebView.ZoomFactor += 0.1; };

            // Percent
            Label lblPct = new Label { Text = "100%", ForeColor = menu.ForeColor, AutoSize = false, Size = new Size(50, btnSize), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(btnPlus.Left - 50, 1), Font = new Font("Segoe UI", 9.5f * scaleFactor) };

            // Minus
            Button btnMinus = new Button { Text = "−", Size = new Size(btnSize, btnSize), Location = new Point(lblPct.Left - btnSize, 1), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter, ForeColor = menu.ForeColor };
            btnMinus.FlatAppearance.BorderSize = 0; btnMinus.FlatAppearance.MouseOverBackColor = hoverColor;
            btnMinus.Click += (s, ev) => { if(CurrentWebView != null) CurrentWebView.ZoomFactor -= 0.1; };

            zoomPanel.Controls.AddRange(new Control[] { lblZoom, btnFull, btnPlus, lblPct, btnMinus });
            menu.Items.Add(new ToolStripControlHost(zoomPanel) { Padding = Padding.Empty, Margin = Padding.Empty });

            menu.Items.Add(new ToolStripSeparator { ForeColor = Color.FromArgb(80, 80, 80) });

            // 4. Tools
            AddItem(T("Print..."), (s, ev) => { if (CurrentWebView != null) CurrentWebView.ExecuteScriptAsync("window.print();"); }, "Ctrl+P");
            AddItem(T("Translate page..."), (s, ev) => TranslateCurrentPage());
            AddItem(T("Search with Google Lens"), null);
            
            var itemFind = AddItem(T("Find and edit"), null);
            itemFind.DropDownItems.Add("Find...", null);
            
            var itemCast = AddItem(T("Cast, save, and share"), null);
            itemCast.DropDownItems.Add("Cast...", null);

            var itemMore = AddItem(T("More tools"), null);
            itemMore.DropDownItems.Add(T("Developer tools"), null, (s, ev) => { if(CurrentWebView != null) CurrentWebView.CoreWebView2.OpenDevToolsWindow(); });
            itemMore.DropDownItems.Add(T("Network Log"), null, (s, ev) => AddNewTab("quantum://network-log"));

            menu.Items.Add(new ToolStripSeparator { ForeColor = Color.FromArgb(80, 80, 80) });

            // 5. System
            var itemHelp = AddItem(T("Help"), null);
            itemHelp.DropDownItems.Add("About Quantum Browser", null, (s, ev) => AddNewTab("quantum://settings"));

            AddItem(T("Settings"), (s, ev) => AddNewTab("quantum://settings"));
            AddItem(T("Exit"), (s, ev) => Application.Exit());

            menu.Show(btnMenu, new Point(0, btnMenu.Height));
        }

        private void OpenNewWindow(bool incognito)
        {
            var newForm = new BrowserForm(incognito);
            newForm.Show();
        }

        public class CustomColorTable : ProfessionalColorTable
        {
            bool isDark;
            public CustomColorTable(bool dark) { isDark = dark; }
            public override Color MenuItemSelected { get { return isDark ? Color.FromArgb(60, 64, 67) : base.MenuItemSelected; } }
            public override Color MenuItemBorder { get { return Color.Transparent; } }
            public override Color ToolStripDropDownBackground { get { return isDark ? Color.FromArgb(41, 42, 45) : Color.White; } }
            public override Color ImageMarginGradientBegin { get { return isDark ? Color.FromArgb(41, 42, 45) : base.ImageMarginGradientBegin; } }
            public override Color ImageMarginGradientMiddle { get { return isDark ? Color.FromArgb(41, 42, 45) : base.ImageMarginGradientMiddle; } }
            public override Color ImageMarginGradientEnd { get { return isDark ? Color.FromArgb(41, 42, 45) : base.ImageMarginGradientEnd; } }
            public override Color SeparatorDark { get { return isDark ? Color.FromArgb(80, 80, 80) : base.SeparatorDark; } }
            public override Color MenuBorder { get { return isDark ? Color.FromArgb(60, 60, 60) : base.MenuBorder; } }
        }

"@
    
    $final = $before + $newCode + "`r`n" + $after
    $final | Set-Content -Path $file -NoNewline
    Write-Host "Replaced successfully."
} else {
    Write-Host "Could not find start/end markers."
}

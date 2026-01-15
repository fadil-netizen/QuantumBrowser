
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

"@
    
    $final = $before + $newCode + "`r`n" + $after
    $final | Set-Content -Path $file -NoNewline
    Write-Host "Re-applied Pixel Perfect UI successfully."
} else {
    Write-Host "Could not find start/end markers."
}

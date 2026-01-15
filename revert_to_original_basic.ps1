
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
            
            // Basic Styling (Original Dark Theme)
            bool isDark = IsIncognito || (colorBgDark.R < 100); 
            if (isDark)
            {
                menu.BackColor = Color.FromArgb(40, 41, 44); 
                menu.ForeColor = Color.White;
            }
            else
            {
                menu.BackColor = Color.White;
                menu.ForeColor = Color.Black;
            }

            menu.ShowImageMargin = false; 
            menu.Font = new Font("Segoe UI", 9.5f * scaleFactor);
            
            // Use translation helper (T) for all items
            // Delegate for C# 5 compatibility
            Action<string, EventHandler, string> AddBasicItem = delegate(string text, EventHandler onClick, string shortcut)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(text);
                if (onClick != null) item.Click += onClick;
                if (!string.IsNullOrEmpty(shortcut)) item.ShortcutKeyDisplayString = shortcut;
                item.ForeColor = menu.ForeColor;
                menu.Items.Add(item);
            };

            // 1. Session Group
            AddBasicItem(T("New tab"), delegate { AddNewTab(); }, "Ctrl+T");
            AddBasicItem(T("New window"), delegate { OpenNewWindow(false); }, "Ctrl+N");
            AddBasicItem(T("New Incognito window"), delegate { OpenNewWindow(true); }, "Ctrl+Shift+N");
            
            menu.Items.Add(new ToolStripSeparator());

            // 2. Main Features
            AddBasicItem(T("Passwords and autofill"), delegate { AddNewTab("quantum://settings/passwords"); }, "");
            AddBasicItem(T("History"), delegate { AddNewTab("quantum://history"); }, "Ctrl+H");
            AddBasicItem(T("Downloads"), delegate { AddNewTab("quantum://downloads"); }, "Ctrl+J");
            AddBasicItem(T("Bookmarks"), delegate { AddNewTab("quantum://bookmarks"); }, "Ctrl+Shift+O");
            AddBasicItem(T("Delete browsing data..."), delegate {
                 if(webView != null && webView.CoreWebView2 != null) webView.CoreWebView2.Profile.ClearBrowsingDataAsync();
                 MessageBox.Show(T("Browsing data cleared."));
            }, "Ctrl+Shift+Del");

            menu.Items.Add(new ToolStripSeparator());

            // 3. System Group
            AddBasicItem(T("Help"), delegate { AddNewTab("quantum://settings"); }, "");
            AddBasicItem(T("Settings"), delegate { AddNewTab("quantum://settings"); }, "");
            AddBasicItem(T("Exit"), delegate { Application.Exit(); }, "");

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
    Write-Host "Reversed to Original Basic state successfully."
} else {
    Write-Host "Could not find start/end markers."
}

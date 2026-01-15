using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;

namespace QuantumInstaller
{
    public class InstallerForm : Form
    {
        private PictureBox logoBox;
        private Button btnInstall;
        private Label lblStatus;
        private ProgressBar progressBar;
        private System.Windows.Forms.Timer animTimer;
        
        // Configuration
        private string appName = "Quantum Browser";
        private string exeName = "QuantumBrowser.exe";

        public InstallerForm()
        {
            this.Text = "Quantum Browser Setup";
            this.Size = new Size(500, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 20, 20); // Dark theme
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Logo
            logoBox = new PictureBox();
            try {
                // Load logo from embedded resource "logo.png"
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("logo.png");
                if(s != null) logoBox.Image = Image.FromStream(s);
            } catch {}
            
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoBox.Size = new Size(150, 150);
            logoBox.Location = new Point((this.ClientSize.Width - logoBox.Width) / 2, 30);
            this.Controls.Add(logoBox);

            // Title
            Label title = new Label();
            title.Text = "Install Quantum Browser";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.AutoSize = true;
            title.Location = new Point((this.ClientSize.Width - title.Width) / 2, 200);
            // Re-center after auto-size
            title.SizeChanged += (s, e) => { title.Left = (this.ClientSize.Width - title.Width) / 2; };
            this.Controls.Add(title);

            // Status
            lblStatus = new Label();
            lblStatus.Text = "Ready to install.";
            lblStatus.Font = new Font("Segoe UI", 10);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.AutoSize = false;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.Size = new Size(400, 30);
            lblStatus.Location = new Point((this.ClientSize.Width - lblStatus.Width) / 2, 240);
            this.Controls.Add(lblStatus);

            // Progress
            progressBar = new ProgressBar();
            progressBar.Size = new Size(300, 10);
            progressBar.Location = new Point((this.ClientSize.Width - progressBar.Width) / 2, 270);
            progressBar.Visible = false;
            this.Controls.Add(progressBar);

            // Button
            btnInstall = new Button();
            btnInstall.Text = "Install";
            btnInstall.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnInstall.Size = new Size(150, 40);
            btnInstall.Location = new Point((this.ClientSize.Width - btnInstall.Width) / 2, 300);
            btnInstall.BackColor = Color.FromArgb(0, 120, 215);
            btnInstall.ForeColor = Color.White;
            btnInstall.FlatStyle = FlatStyle.Flat;
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;
            this.Controls.Add(btnInstall);

            // Animation Timer
            animTimer = new System.Windows.Forms.Timer();
            animTimer.Interval = 50;
            animTimer.Tick += (s, e) => {
                // Simple pulse animation for logo
                if(progressBar.Visible) {
                   // logic to animate logo can go here
                }
            };
            animTimer.Start();
        }

        private async void BtnInstall_Click(object sender, EventArgs e)
        {
            btnInstall.Enabled = false;
            btnInstall.Text = "Installing...";
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            
            lblStatus.Text = "Preparing...";
            
            await System.Threading.Tasks.Task.Run(() => {
                try {
                    InstallProcess();
                    this.Invoke((MethodInvoker)delegate {
                        lblStatus.Text = "Installation Complete!";
                        progressBar.Style = ProgressBarStyle.Blocks;
                        progressBar.Value = 100;
                        btnInstall.Text = "Launch";
                        btnInstall.Click -= BtnInstall_Click;
                        btnInstall.Click += (s2, e2) => {
                            try {
                                string targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), appName);
                                // Try to launch non-elevated (Explorer) if possible, but Process.Start usually inherits.
                                // For now, simple launch.
                                System.Diagnostics.Process.Start(Path.Combine(targetDir, exeName));
                                Application.Exit();
                            } catch (Exception exLaunch) {
                                MessageBox.Show("Failed to launch: " + exLaunch.Message);
                            }
                        };
                        btnInstall.Enabled = true;
                        MessageBox.Show("Quantum Browser installed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                } catch (Exception ex) {
                    this.Invoke((MethodInvoker)delegate {
                        lblStatus.Text = "Error: " + ex.Message;
                        MessageBox.Show(ex.ToString(), "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnInstall.Enabled = true;
                        btnInstall.Text = "Retry";
                    });
                }
            });
        }

        private void InstallProcess()
        {
            // PROGAM FILES
            string targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), appName);
            
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Extract Zip
            this.Invoke((MethodInvoker)delegate { lblStatus.Text = "Extracting files to Program Files..."; });
            
            Stream zipStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip");
            if(zipStream == null) throw new Exception("Payload not found! Check resource embedding.");

            using (ZipArchive archive = new ZipArchive(zipStream))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string completeFileName = Path.Combine(targetDir, entry.FullName);
                    string directoryPath = Path.GetDirectoryName(completeFileName);
                    
                    if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        try {
                            if(File.Exists(completeFileName)) File.Delete(completeFileName);
                            entry.ExtractToFile(completeFileName, true);
                        } catch (Exception ex) {
                            Console.WriteLine("Error extracting " + entry.Name + ": " + ex.Message);
                        }
                    }
                }
            }
            
            // --- Install WebView2 Runtime if needed ---
            string setupPath = Path.Combine(targetDir, "MicrosoftEdgeWebview2Setup.exe");
            if (File.Exists(setupPath))
            {
                this.Invoke((MethodInvoker)delegate { lblStatus.Text = "Checking/Installing WebView2 Runtime..."; });
                try 
                {
                   System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo();
                   si.FileName = setupPath;
                   si.Arguments = "/silent /install";
                   si.UseShellExecute = true;
                   si.Verb = "runas"; // Ensure admin
                   System.Diagnostics.Process.Start(si).WaitForExit();
                } 
                catch (Exception ex) 
                {
                   Console.WriteLine("WebView2 Setup Error: " + ex.Message);
                }
            }

            string exePath = Path.Combine(targetDir, exeName);

            // Shortcuts
            this.Invoke((MethodInvoker)delegate { lblStatus.Text = "Creating shortcuts..."; });
            
            // Desktop (Public for all users if admin, or User Desktop)
            string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
            CreateShortcut(exePath, Path.Combine(commonDesktop, appName + ".lnk"));

            // Start Menu (Common Start Menu)
            string commonStartMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
            CreateShortcut(exePath, Path.Combine(commonStartMenu, appName + ".lnk"));

            // Registry Registration (Add/Remove Programs)
            this.Invoke((MethodInvoker)delegate { lblStatus.Text = "Registering application..."; });
            RegisterApplication(targetDir, exePath);
            CreateUninstaller(targetDir);

            Thread.Sleep(500); 
        }

        private void CreateShortcut(string targetExe, string shortcutPath)
        {
            try {
                string psScript = "$s=(New-Object -COM WScript.Shell).CreateShortcut('" + shortcutPath + "');$s.TargetPath='" + targetExe + "';$s.Save()";
                
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "powershell";
                psi.Arguments = "-Command \"" + psScript + "\"";
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                System.Diagnostics.Process.Start(psi).WaitForExit();
            } catch {}
        }

        private void RegisterApplication(string installDir, string exePath)
        {
            try {
                using(Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\QuantumBrowser"))
                {
                    if(key != null) {
                        key.SetValue("DisplayName", "Quantum Browser");
                        key.SetValue("DisplayIcon", exePath);
                        key.SetValue("DisplayVersion", "1.0.0");
                        key.SetValue("Publisher", "FSL");
                        key.SetValue("UninstallString", Path.Combine(installDir, "uninstall.bat"));
                        key.SetValue("InstallLocation", installDir);
                    }
                }
            } catch (Exception ex) {
                // Not fatal, just won't show in Add/Remove
                Console.WriteLine("Registry error: " + ex.Message);
            }
        }

        private void CreateUninstaller(string installDir)
        {
            try {
                // Robust Uninstaller: Copies itself to TEMP to avoid "file in use" errors when deleting the install folder
                string batContent = "@echo off\r\n" +
                    "if \"%1\"==\"__DELETING__\" goto :delete\r\n" +
                    "copy \"%~f0\" \"%TEMP%\\qb_uninstall.bat\" >nul\r\n" +
                    "\"%TEMP%\\qb_uninstall.bat\" __DELETING__ \"%~dp0\"\r\n" +
                    "exit /b\r\n" +
                    "\r\n" +
                    ":delete\r\n" +
                    "set \"INSTALLDIR=%~2\"\r\n" +
                    "echo Uninstalling Quantum Browser...\r\n" +
                    "taskkill /f /im QuantumBrowser.exe >nul 2>&1\r\n" +
                    "timeout /t 2 /nobreak >nul\r\n" +
                    "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\QuantumBrowser\" /f\r\n" +
                    "del \"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), appName + ".lnk") + "\"\r\n" +
                    "del \"" + Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"), appName + ".lnk") + "\"\r\n" +
                    "rmdir /s /q \"%INSTALLDIR%\"\r\n" + 
                    "echo Done.\r\n" + 
                    "pause\r\n" +
                    "del \"%~f0\"\r\n";
                
                File.WriteAllText(Path.Combine(installDir, "uninstall.bat"), batContent);
            } catch {}
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerForm());
        }
    }
}

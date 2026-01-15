using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuantumBrowser
{
    public partial class BrowserForm : Form
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try 
            {
                Application.Run(new BrowserForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }

        public bool IsIncognito { get; private set; }

        public BrowserForm(bool isIncognito = false)
        {
            this.IsIncognito = isIncognito;

            // Initialize Components
            this.KeyPreview = true;
            InitializeUI();
            InitializeWebView();

            // Handle Resize to position correctly when maximized (only if NOT in FullScreen Mode)
            this.Resize += (s, e) => {
                if (_isFullScreen) return; // FullScreen handles its own layout
                
                if (this.WindowState == FormWindowState.Maximized)
                {
                    // In maximized mode, Windows extends the frame 8px outside the workspace.
                    int pad = (int)(8 * scaleFactor);
                    this.Padding = new Padding(pad, pad, pad, 0);
                }
                else
                {
                    // In normal mode, use original edge gap for resizing
                    int edgeGap = (int)(2 * scaleFactor);
                    if (edgeGap < 2) edgeGap = 2;
                    this.Padding = new Padding(edgeGap);
                }
            };
        }
    }
}

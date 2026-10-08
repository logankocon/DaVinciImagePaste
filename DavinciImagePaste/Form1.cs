using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace DavinciImagePaste
{
    public partial class Form1 : Form
    {
        //Saved folder file path
        private string? selectedPath = null;

        // true if program is extiing
        private bool exiting = false;


        //Keyboard shortcut values
        private const int PasteHotkeyId = 1;
        private const int WM_HOTKEY = 0x0312;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_NOREPEAT = 0x4000;

        //user32 Windows imports 

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(
            IntPtr hWnd, int id, uint modifiers, uint key);

        [DllImport("user32.dll", SetLastError = true)]

        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(
            IntPtr hWnd, StringBuilder text, int maxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(
            IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);


        public Form1()
        {
            InitializeComponent();

            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            //ClientSize = new Size(320, 180);

            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Text = "ResolvePaste";
            trayIcon.Visible = true;
            trayIcon.MouseClick += TrayIcon_MouseClick;

            // Hide the form after its initial display.
            Shown += (_, _) => Hide();

            ContextMenuStrip trayMenu = new ContextMenuStrip(components);

            trayMenu.Items.Add("Quit", null, (_, _) =>
            {
                exiting = true;
                trayIcon.Visible = false;
                Close();
            });

            trayIcon.ContextMenuStrip = trayMenu;
        }

        //For registering shortcut hot key
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            bool registered = RegisterHotKey(
                Handle,
                PasteHotkeyId,
                MOD_CONTROL | MOD_ALT | MOD_NOREPEAT,
                (uint)Keys.V
            );

            if (!registered)
            {
                MessageBox.Show("Couldn't register Ctrl+Alt+V.");
            }
        }

        //When hotkey is pressed, Windows sends message back to this application. This method recieves that message
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY &&
                m.WParam.ToInt32() == PasteHotkeyId)
            {
                shortcut_Pressed();
                return;
            }

            base.WndProc(ref m);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnregisterHotKey(Handle, PasteHotkeyId);
            base.OnHandleDestroyed(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //Hides program instead of closing it
            if (!exiting && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }

            base.OnFormClosing(e);
        }




        //Handles when shortcut is pressed (CTRL + ALT + V)
        private async void shortcut_Pressed()
        {

            Process[] resolveProcesses = Process.GetProcessesByName("Resolve");

            if (resolveProcesses.Length == 0)
            {
                MessageBox.Show("Open DaVinci Resolve first.");
                return;
            }

            IntPtr resolveWindow = resolveProcesses[0].MainWindowHandle;

            if (resolveWindow == IntPtr.Zero || resolveWindow != GetForegroundWindow())
            {
                MessageBox.Show("Move DaVinci Resolve to foreground");
                return;
            }

            await WaitForShortcutRelease();

            using var image = Clipboard.GetImage();

            if (image is null)
            {
                MessageBox.Show("Couldn't read the image.");
                return;
            }

            if (selectedPath is null)
            {
                MessageBox.Show("No file path selected.");
                return;
            }

            Directory.CreateDirectory(selectedPath);

            string filename = $"image_{Guid.NewGuid():N}.png";
            string filePath = Path.Combine(selectedPath, filename);

            image.Save(filePath, ImageFormat.Png);

            if (filePath is null)
            {
                MessageBox.Show("Image save failed.");
                return;
            }

            //Opens DaVinci's import menu
            SendKeys.SendWait("^i");

            IntPtr importWindow = await WaitForImportDialog(resolveProcesses[0].Id);

            if (importWindow == IntPtr.Zero)
            {
                MessageBox.Show("Failed to import media to DaVinci Resolve");
                return;
            }

            Clipboard.SetText(filePath);
            await Task.Delay(200);

            //Copies image's file path to import window and confirms
            SendKeys.SendWait("^v");
            SendKeys.SendWait("{ENTER}");


            bool importClose = await WaitForDialogToClose(importWindow);
            if (!importClose)
            {
                MessageBox.Show("Failed to import media to DaVinci Resolve");
                return;
            }

            await Task.Delay(200);

            if (GetForegroundWindow() != resolveWindow)
            {
                MessageBox.Show("Keep DaVinci Resolve in the foreground.");
                return;
            }

            //Puts imported image on timeline at playhead
            SendKeys.SendWait("{F12}");

            //Puts copied image back onto clipboard for consistency 
            Clipboard.SetImage(image);
        }


        private async Task WaitForShortcutRelease()
        {
            while (GetAsyncKeyState((int)Keys.ControlKey) < 0 ||
                   GetAsyncKeyState((int)Keys.Menu) < 0 ||
                   GetAsyncKeyState((int)Keys.V) < 0)
            {
                await Task.Delay(20);
            }
        }

        private async Task<IntPtr> WaitForImportDialog(int resolveProcessId)
        {
            Stopwatch timer = Stopwatch.StartNew();

            while (timer.ElapsedMilliseconds < 5000)
            {
                IntPtr window = GetForegroundWindow();

                GetWindowThreadProcessId(window, out uint processId);

                StringBuilder title = new StringBuilder(256);
                GetWindowText(window, title, title.Capacity);

                if (processId == (uint)resolveProcessId &&
                    title.ToString() == "Import Media")
                {
                    return window;
                }

                await Task.Delay(20);
            }

            return IntPtr.Zero;
        }

        private async Task<bool> WaitForDialogToClose(IntPtr importWindow)
        {
            Stopwatch timer = Stopwatch.StartNew();

            while (IsWindow(importWindow) && IsWindowVisible(importWindow))
            {
                if (timer.ElapsedMilliseconds >= 5000)
                {
                    return false;
                }

                await Task.Delay(20);
            }

            return true;
        }

        private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (Visible)
            {
                Hide();
                return;
            }

            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;

            Location = new Point(
                area.Right - Width - 10,
                area.Bottom - Height - 10
            );

            Show();
            Activate();
        }


        private void folderButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                folderPathTextBox.Text = folderBrowserDialog1.SelectedPath;
                selectedPath = folderBrowserDialog1.SelectedPath;
            }
        }



        //Empty methods for now
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void folderPathTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

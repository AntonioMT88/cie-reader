using CieReader.Utils;
using Microsoft.Win32;
using System.Diagnostics;

namespace CieReader.Service
{
    public class TrayAppContext : ApplicationContext, IDisposable
    {
        private ConfigReader configReader;
        private NotifyIcon trayIcon;
        private WebSocketServer webSocketServer;
        private Reader reader;

        public TrayAppContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = new Icon("Resources/icon.ico"),
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true,
                Text = "CIE Reader"
            };

            NotificationBalloon.NotifyIcon = trayIcon;

            // Aggiunge un'opzione "Esci" nel menu                       
            trayIcon.ContextMenuStrip.Items.Add("Esci", null, OnExit);            

            configReader = new ConfigReader();

            SetStartup(configReader.Configuration.AutoStartUp);

            webSocketServer = new WebSocketServer(configReader.Configuration.WebSocketConfig);           

            reader = new Reader();
            reader.OnCardRead += OnCardRead;
        }      

        private void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Dispose(true);
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {                
                webSocketServer?.StopServer().GetAwaiter().GetResult();
                reader?.StopReaderMonitoring();
                trayIcon?.Dispose();
            }
        }

        protected void OnCardRead(object sender, string message)
        {
            Debug.WriteLine($"Carta Letta: {message}");
            webSocketServer.BroadcastMessageAsync(message);
        }
        private void SetStartup(bool enable)
        {
            string appName = "Cie_Reader";
            string exePath = Application.ExecutablePath;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (rk != null)
            {
                if (enable)
                    rk.SetValue(appName, "\"" + exePath + "\"");
                else
                    rk.DeleteValue(appName, false);
            }
        }       
    }
}

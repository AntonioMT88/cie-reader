using CieReader.Utils;
using Microsoft.Win32;
using System.Diagnostics;

namespace CieReader.Service
{
    public class TrayAppContext : ApplicationContext
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
                Text = "NFC Tray App"
            };

            // Aggiunge un'opzione "Esci" nel menu                       
            trayIcon.ContextMenuStrip.Items.Add("Esci", null, OnExit);

            // (Facoltativo) Registrati per l'avvio automatico
            SetStartup(true);

            configReader = new ConfigReader();

            webSocketServer = new WebSocketServer(configReader.Configuration.WebSocketConfig);           

            reader = new Reader();
            reader.OnCardRead += OnCardRead;
        }

        private void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
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

            if (enable)
                rk.SetValue(appName, "\"" + exePath + "\"");
            else
                rk.DeleteValue(appName, false);
        }     
    }
}

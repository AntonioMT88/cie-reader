using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieReader.Utils
{
    public class NotificationBalloon
    {
        public static void ShowBalloon(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            var notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = new Icon("Resources/icon.ico"),
                Text = "CIE Reader",
                BalloonTipIcon = icon,
                BalloonTipTitle = title,
                BalloonTipText = message
            };
            notifyIcon.ShowBalloonTip(2500);
            Task.Delay(2500).ContinueWith(_ => notifyIcon.Dispose());
        }
    }
}

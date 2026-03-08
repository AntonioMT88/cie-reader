namespace CieReader.Utils
{
    public class NotificationBalloon
    {
        public static NotifyIcon NotifyIcon { get; internal set; }
        public static void ShowBalloon(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            if (NotifyIcon == null) return;

            NotifyIcon.BalloonTipIcon = icon;
            NotifyIcon.BalloonTipTitle = title;
            NotifyIcon.BalloonTipText = message;

            NotifyIcon.ShowBalloonTip(2500);            
        }
    }
}

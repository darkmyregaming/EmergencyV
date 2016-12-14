namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;

    // RPH
    using Rage;
    using Graphics = Rage.Graphics;

    internal static class Notification
    {
        static NotificationData currentNotificationData;
        static bool active = false;
        static DateTime showStartTime; // after easing 

        static bool easing = false;
        static bool easingInverse = false;
        static float easingCurrentTime = 0.0f;
        static float x = 0.0f;

        static float notificationWidth = 400;

        static string font = "";
        static float fontSizeTitle = 21.0f;
        static float fontSizeSubtitle = 14.25f;

        const float easeDuration = 2.5f;

        public static void Show(string title, string subtitle, double seconds)
        {
            Hide();

            currentNotificationData = new NotificationData()
            {
                Title = title,
                Subtitle = subtitle,
                DisplaySeconds = seconds,
                TitleWrapped = AdvancedUI.Util.WrapText(title, font, fontSizeTitle, notificationWidth - 10),
                SubtitleWrapped = AdvancedUI.Util.WrapText(subtitle, font, fontSizeSubtitle, notificationWidth - 10),
                TitleColor = Color.FromArgb(200, 0, 0),
                SubtitleColor = Color.FromArgb(230, 230, 230),
            };
            currentNotificationData.SubtitleYCoordinate = Graphics.MeasureText(currentNotificationData.TitleWrapped, font, fontSizeTitle).Height + 20;
            currentNotificationData.NotificationHeight = currentNotificationData.SubtitleYCoordinate + Graphics.MeasureText(currentNotificationData.SubtitleWrapped, font, fontSizeSubtitle).Height + 10;

            easing = true;
            easingInverse = false;
            easingCurrentTime = easeDuration;
            active = true;
        }

        public static void Hide()
        {
            if (easing || !easingInverse)
            {
                easing = false;
                easingInverse = true;
                easingCurrentTime = 0.0f;
            }
        }

        
        public static void OnUpdate()
        {
            if (active)
            {
                if (easing)
                {
                    x = Util.Easing.OutQuart(easingCurrentTime, Game.Resolution.Width - notificationWidth, notificationWidth, easeDuration);
                    easingCurrentTime -= 0.075f * 20f * Game.FrameTime;
                    if (easingCurrentTime < 0.0f)
                    {
                        easing = false;
                        showStartTime = DateTime.UtcNow;
                    }
                }
                else if (easingInverse)
                {
                    x = Util.Easing.OutQuart(easingCurrentTime, Game.Resolution.Width - notificationWidth, notificationWidth, easeDuration);
                    easingCurrentTime += 0.075f * 18f * Game.FrameTime;
                    if (easingCurrentTime > easeDuration)
                    {
                        active = false;
                    }
                }
                else
                {
                    if ((DateTime.UtcNow - showStartTime).TotalSeconds > currentNotificationData.DisplaySeconds)
                    {
                        Hide();
                    }
                }
            }
        }

        public static void OnDraw(Graphics g)
        {
            if (active)
            {
                g.DrawRectangle(new RectangleF(x, 8, notificationWidth, currentNotificationData.NotificationHeight), Color.FromArgb(190, 3, 3, 3));
                g.DrawText(currentNotificationData.TitleWrapped, font, fontSizeTitle, new PointF(x + 5, 10), currentNotificationData.TitleColor);
                g.DrawText(currentNotificationData.SubtitleWrapped, font, fontSizeSubtitle, new PointF(x + 5, currentNotificationData.SubtitleYCoordinate), currentNotificationData.SubtitleColor);
            }
        }

        public static void ShowTest()
        {
            Show("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse et aliquam purus. Cras eleifend, tortor vel sodales rutrum, purus felis commodo elit, vitae eleifend augue eros quis metus.".ToUpper(),
                 "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sit amet leo sapien. Donec commodo malesuada rhoncus. Donec et metus in quam posuere scelerisque in eu libero. Vivamus suscipit nulla odio, at lobortis lectus mollis maximus. Proin aliquet lacus lectus, vitae lobortis orci faucibus vel. Mauris semper tellus metus, eu aliquam arcu rutrum non. Aenean volutpat id neque in faucibus. Phasellus ut convallis nulla.",
                 15.0);
        }


        private struct NotificationData
        {
            public string Title;
            public string Subtitle;
            public double DisplaySeconds;

            public string TitleWrapped;
            public string SubtitleWrapped;

            public Color TitleColor;
            public Color SubtitleColor;

            public float SubtitleYCoordinate;

            public float NotificationHeight;
        }
    }
}

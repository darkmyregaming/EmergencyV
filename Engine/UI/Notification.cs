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
        public static string Font { get; set; } = "";
        public static float TitleFontSize { get; set; } = 21.0f;
        public static float SubtitleFontSize { get; set; } = 14.25f;

        static NotificationData currentNotificationData;
        static bool active = false;
        static DateTime showStartTime; // after easing 

        static bool easing = false;
        static bool easingInverse = false;
        static float easingCurrentTime = 0.0f;
        static float x = 0.0f;

        static float notificationWidth = 400;

        const float easeDuration = 2.5f;

        public static void Show(string title, string subtitle, double seconds)
        {
            Hide();

            currentNotificationData = new NotificationData()
            {
                EasingStartValue = Game.Resolution.Width - notificationWidth,
                EasingChangeInValue = notificationWidth,
                Title = title,
                Subtitle = subtitle,
                DisplaySeconds = seconds,
                TitleWrapped = AdvancedUI.Util.WrapText(title, Font, TitleFontSize, notificationWidth - 10),
                SubtitleWrapped = AdvancedUI.Util.WrapText(subtitle, Font, SubtitleFontSize, notificationWidth - 10),
                TitleColor = Color.FromArgb(200, 0, 0),
                SubtitleColor = Color.FromArgb(230, 230, 230),
            };
            currentNotificationData.SubtitleYCoordinate = Graphics.MeasureText(currentNotificationData.TitleWrapped, Font, TitleFontSize).Height + 20;
            currentNotificationData.NotificationHeight = currentNotificationData.SubtitleYCoordinate + Graphics.MeasureText(currentNotificationData.SubtitleWrapped, Font, SubtitleFontSize).Height + 10;

            easing = true;
            easingInverse = false;
            easingCurrentTime = easeDuration;
            active = true;
        }

        public static void Show(string title, string subtitle, double seconds, Texture image, float imageWidth = 64.0f)
        {
            Show(title, subtitle, seconds);

            if (currentNotificationData.NotificationHeight < imageWidth + 2 * (2 * 2.5f))
                currentNotificationData.NotificationHeight = imageWidth + 2 * (2 * 2.5f);

            currentNotificationData.Image = image;
            currentNotificationData.ImageWidth = imageWidth;
            currentNotificationData.ImageY = 8 + (currentNotificationData.NotificationHeight / 2 - imageWidth / 2);

            currentNotificationData.EasingStartValue = Game.Resolution.Width - notificationWidth - imageWidth;
            currentNotificationData.EasingChangeInValue = notificationWidth + imageWidth;
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
                    x = Util.Easing.OutQuart(easingCurrentTime, currentNotificationData.EasingStartValue, currentNotificationData.EasingChangeInValue, easeDuration);
                    easingCurrentTime -= 0.075f * 20f * Game.FrameTime;
                    if (easingCurrentTime < 0.0f)
                    {
                        easing = false;
                        showStartTime = DateTime.UtcNow;
                    }
                }
                else if (easingInverse)
                {
                    x = Util.Easing.OutQuart(easingCurrentTime, currentNotificationData.EasingStartValue, currentNotificationData.EasingChangeInValue, easeDuration);
                    easingCurrentTime += 0.075f * 20f * Game.FrameTime;
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
                if (currentNotificationData.Image != null)
                {
                    g.DrawRectangle(new RectangleF(x, 8, currentNotificationData.ImageWidth + (2 * 2.5f), currentNotificationData.NotificationHeight), Color.FromArgb(190, 3, 3, 3));
                    g.DrawTexture(currentNotificationData.Image, new RectangleF(x + 2.5f, currentNotificationData.ImageY, currentNotificationData.ImageWidth, currentNotificationData.ImageWidth));
                }

                float x_ = x + (currentNotificationData.Image != null ? currentNotificationData.ImageWidth + (2 * 2.5f) : 0.0f);
                g.DrawRectangle(new RectangleF(x_, 8, notificationWidth, currentNotificationData.NotificationHeight), Color.FromArgb(190, 3, 3, 3));
                g.DrawText(currentNotificationData.TitleWrapped, Font, TitleFontSize, new PointF(x_ + 5, 10), currentNotificationData.TitleColor);
                g.DrawText(currentNotificationData.SubtitleWrapped, Font, SubtitleFontSize, new PointF(x_ + 5, currentNotificationData.SubtitleYCoordinate), currentNotificationData.SubtitleColor);
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
            public float EasingStartValue;
            public float EasingChangeInValue;

            public string Title;
            public string Subtitle;
            public double DisplaySeconds;

            public string TitleWrapped;
            public string SubtitleWrapped;

            public Color TitleColor;
            public Color SubtitleColor;

            public float SubtitleYCoordinate;

            public float NotificationHeight;

            public Texture Image;
            public float ImageWidth;
            public float ImageY;
        }
    }
}

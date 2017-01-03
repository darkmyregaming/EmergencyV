namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;

    // RPH
    using Rage;
    using Graphics = Rage.Graphics;

    internal class Notification
    {
        private const string Font = "";
        private const float TitleFontSize = 21.0f;
        private const float SubtitleFontSize = 14.25f;

        private const float EaseDuration = 2.5f;

        private const float Width = 400.0f;

        public event Action<Notification> Finished;

        private bool isFinished;
        public bool IsFinished
        {
            get { return isFinished; }
            private set
            {
                if (value == isFinished)
                    return;
                isFinished = value;
                if (isFinished)
                    Finished?.Invoke(this);
            }
        }
        private PointF location;
        public PointF Location { get { return location; } set { location = value; } }

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

        DateTime showStartTime; // after easing 

        bool easing = false;
        bool easingInverse = false;
        float easingCurrentTime = 0.0f;

        bool easingVertically = false;
        float easingVerticallyCurrentTime, easingVerticallyStartValue, easingVerticallyChangeInValue;

        private Notification(string title, string subtitle, double seconds)
        {
            EasingStartValue = Game.Resolution.Width - Width;
            EasingChangeInValue = Width;
            Title = title;
            Subtitle = subtitle;
            DisplaySeconds = seconds;
            TitleWrapped = AdvancedUI.Util.WrapText(title, Font, TitleFontSize, Width - 10);
            SubtitleWrapped = AdvancedUI.Util.WrapText(subtitle, Font, SubtitleFontSize, Width - 10);
            TitleColor = Color.FromArgb(200, 0, 0);
            SubtitleColor = Color.FromArgb(230, 230, 230);
            SubtitleYCoordinate = Graphics.MeasureText(TitleWrapped, Font, TitleFontSize).Height;
            NotificationHeight = SubtitleYCoordinate + Graphics.MeasureText(SubtitleWrapped, Font, SubtitleFontSize).Height + 35;

            easing = true;
            easingInverse = false;
            easingCurrentTime = EaseDuration;

            location.X = Game.Resolution.Width;
        }

        private Notification(string title, string subtitle, double seconds, Texture image, float imageWidth = 64.0f) : this(title, subtitle, seconds)
        {
            float marginTotalWidth = (2 * 2.5f);

            if (NotificationHeight < imageWidth + 2 * marginTotalWidth)
                NotificationHeight = imageWidth + 2 * marginTotalWidth;

            Image = image;
            ImageWidth = imageWidth;
            ImageY = (NotificationHeight / 2 - imageWidth / 2);

            EasingStartValue = Game.Resolution.Width - Width - imageWidth;
            EasingChangeInValue = Width + imageWidth;
        }

        public void Hide()
        {
            if (easing || !easingInverse)
            {
                easing = false;
                easingInverse = true;
                easingCurrentTime = 0.0f;
            }
        }

        public void OnUpdate()
        {
            if (easing)
            {
                location.X = Util.Easing.OutQuart(easingCurrentTime, EasingStartValue, EasingChangeInValue, EaseDuration);

                easingCurrentTime -= 0.075f * 20f * Game.FrameTime;
                if (easingCurrentTime < 0.0f)
                {
                    easing = false;
                    location.X = EasingStartValue;
                    showStartTime = DateTime.UtcNow;
                }
            }
            else if (easingInverse)
            {
                location.X = Util.Easing.OutQuart(easingCurrentTime, EasingStartValue, EasingChangeInValue, EaseDuration);
                easingCurrentTime += 0.075f * 20f * Game.FrameTime;
                if (easingCurrentTime > EaseDuration)
                {
                    IsFinished = true;
                }
            }
            else
            {
                if ((DateTime.UtcNow - showStartTime).TotalSeconds > DisplaySeconds)
                {
                    Hide();
                }
            }

            if (easingVertically)
            {
                location.Y = Util.Easing.OutQuart(easingVerticallyCurrentTime, easingVerticallyStartValue, easingVerticallyChangeInValue, 1.0f);
                easingVerticallyCurrentTime += 0.075f * 20f * Game.FrameTime;
                if (easingVerticallyCurrentTime > 1.0f)
                {
                    location.Y = easingVerticallyStartValue + easingVerticallyChangeInValue;
                    easingVertically = false;
                }
            }
        }

        public void OnDraw(Graphics g)
        {
            if (Image != null)
            {
                float marginTotalWidth = (2 * 2.5f);
                g.DrawRectangle(new RectangleF(location.X - marginTotalWidth, location.Y, ImageWidth + marginTotalWidth, NotificationHeight), Color.FromArgb(190, 3, 3, 3));
                g.DrawTexture(Image, new RectangleF(location.X + 2.5f - marginTotalWidth, location.Y + ImageY, ImageWidth, ImageWidth));
            }

            float x_ = location.X + (Image != null ? ImageWidth : 0.0f);
            g.DrawRectangle(new RectangleF(x_, location.Y, Width + 5.0f, NotificationHeight), Color.FromArgb(190, 3, 3, 3));
            g.DrawText(TitleWrapped, Font, TitleFontSize, new PointF(x_ + 5, location.Y + 2.0f), TitleColor);
            g.DrawText(SubtitleWrapped, Font, SubtitleFontSize, new PointF(x_ + 5, location.Y + SubtitleYCoordinate + 20.0f), SubtitleColor);
        }

        public void MoveVertically(float changeInValue)
        {
            easingVerticallyStartValue = location.Y;
            easingVerticallyChangeInValue = changeInValue;
            easingVerticallyCurrentTime = 0.0f;
            easingVertically = true;
        }

        public static void Show(string title, string subtitle, double seconds)
        {
            Notification n = new Notification(title, subtitle, seconds);
            NotificationsManager.Instance.AddNotification(n);
        }

        public static void Show(string title, string subtitle, double seconds, Texture image, float imageWidth = 64.0f)
        {
            Notification n = new Notification(title, subtitle, seconds, image, imageWidth);
            NotificationsManager.Instance.AddNotification(n);
        }

        public static void Show(string title, string subtitle, double seconds, Color titleColor, Color subtitleColor)
        {
            Notification n = new Notification(title, subtitle, seconds);
            n.TitleColor = titleColor;
            n.SubtitleColor = subtitleColor;
            NotificationsManager.Instance.AddNotification(n);
        }

        public static void Show(string title, string subtitle, double seconds, Color titleColor, Color subtitleColor, Texture image, float imageWidth = 64.0f)
        {
            Notification n = new Notification(title, subtitle, seconds, image, imageWidth);
            n.TitleColor = titleColor;
            n.SubtitleColor = subtitleColor;
            NotificationsManager.Instance.AddNotification(n);
        }

#if DEBUG
        public static void ShowTestLong()
        {
            Show("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse et aliquam purus. Cras eleifend, tortor vel sodales rutrum, purus felis commodo elit, vitae eleifend augue eros quis metus.".ToUpper(),
                 "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sit amet leo sapien. Donec commodo malesuada rhoncus. Donec et metus in quam posuere scelerisque in eu libero. Vivamus suscipit nulla odio, at lobortis lectus mollis maximus. Proin aliquet lacus lectus, vitae lobortis orci faucibus vel. Mauris semper tellus metus, eu aliquam arcu rutrum non. Aenean volutpat id neque in faucibus. Phasellus ut convallis nulla.",
                 15.0);
        }

        public static void ShowTestShort()
        {
            Show("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse et aliquam purus.".ToUpper(),
                 "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sit amet leo sapien. Donec commodo malesuada rhoncus.",
                 15.0);
        }

        public static void ShowTestImageSmall()
        {
            Show("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse et aliquam purus.".ToUpper(),
                 "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sit amet leo sapien. Donec commodo malesuada rhoncus.",
                 15.0,
                 Game.CreateTextureFromFile("DefaultSkin.png"),
                 64.0f);
        }

        public static void ShowTestImageBig()
        {
            Show("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse et aliquam purus.".ToUpper(),
                 "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sit amet leo sapien. Donec commodo malesuada rhoncus.",
                 15.0,
                 Game.CreateTextureFromFile("DefaultSkin.png"),
                 128.0f);
        }
#endif
    }
}

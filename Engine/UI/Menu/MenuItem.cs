namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;

    // RPH
    using Rage;
    using Graphics = Rage.Graphics;

    internal class MenuItem
    {
        public const float ItemWidth = 325.0f;
        public const float ItemHeight = 45.0f;
        public const string ItenFontName = "";
        public const float ItemFontSize = 15.0f;

        public string Text { get; set; }
        public Action Callback { get; set; }
        public Control? ShortcutControl { get; set; }
        public Menu BindedSubmenu { get; set; }

        public string DisplayText
        {
            get
            {
                string controlStr = ShortcutControl.HasValue ? ShortcutControl.Value.GetDisplayText() : "";

                return Text + (String.IsNullOrEmpty(controlStr) ? "" : $" ({controlStr})");
            }
        }

        public MenuItem(string text, Action callback, Control? shortcutControl = null)
        {
            Text = text;
            Callback = callback;
            ShortcutControl = shortcutControl;
        }

        public void OnUpdate()
        {
        }

        public void OnDraw(Graphics g, float x, int position) // position == 0 -> middle of screen, selected item  ;;  position < 0 -> above middle  ;;  position > 0 -> below middle
        {
            float middleY = Game.Resolution.Height / 2 - ItemHeight / 2;
            float offsetY = (ItemHeight + 3.0f) * position;

            float y = middleY + offsetY;

            if (y < -ItemHeight || y > Game.Resolution.Height) // if item isn't on screen don't draw it
                return;

            RectangleF rect = new RectangleF(x, y, ItemWidth, ItemHeight);
            g.DrawRectangle(rect, position == 0 ? Color.FromArgb(200, 200, 200) : Color.FromArgb(100, 5, 5, 5));

            string text = DisplayText;

            SizeF textSize = Graphics.MeasureText(text, ItenFontName, ItemFontSize);

            float textX = rect.X + rect.Width * 0.5f - textSize.Width * 0.5f;
            float textY = rect.Y + rect.Height * 0.5f - textSize.Height * 0.8f;

            g.DrawText(text, ItenFontName, ItemFontSize, new PointF(textX, textY), position == 0 ? Color.FromArgb(0, 0, 0) : Color.FromArgb(100, 240, 240, 240), rect);
        }
    }
}

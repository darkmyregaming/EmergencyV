namespace EmergencyV
{
    using System;
    using System.Drawing;

    using Rage;

    internal class PercentageBar : IDeletable
    {
        public const int Width = 360;
        public const int Height = 30;
        
        public float Percentage { get; set; }

        public Color ForegroundColor { get; set; } = Color.DarkGray;
        public Color BackgroundColor { get; set; } = Color.FromArgb(56, 56, 56);

        public string FontName { get; set; } = "Arial";
        public float FontSize { get; set; } = 15f;

        public bool Visible { get; set; } = true;

        private string Title { get; }

        private PointF Location { get; }

        private RectangleF OuterRect { get; }
        private RectangleF BackRect { get; }

        public PercentageBar(string title) : this(title, Game.Resolution.Width / 2 - Width / 2, 50) // centered horizontally; top margin of 50px
        {
        }

        public PercentageBar(string title, int x, int y) : this(title, new PointF(x, y))
        {
        }

        public PercentageBar(string title, PointF location)
        {
            Title = title;
            Location = location;

            SizeF size = new SizeF(Width, Height);

            OuterRect = new RectangleF(Location, size);
            BackRect = new RectangleF(Location.X + 5, Location.Y + 5, size.Width - 10, size.Height - 10);

            Game.RawFrameRender += render;
        }

        private void render(object sender, GraphicsEventArgs e)
        {
            if (!Visible)
                return;

            float width = MathHelper.Clamp(Percentage, 0.0f, 1.0f) * BackRect.Width;
            RectangleF inner = new RectangleF(BackRect.Location, new SizeF(width, BackRect.Height));

            e.Graphics.DrawRectangle(OuterRect, Color.Black);
            e.Graphics.DrawRectangle(BackRect, BackgroundColor);
            e.Graphics.DrawRectangle(inner, ForegroundColor);

            SizeF textSize = Rage.Graphics.MeasureText(Title, FontName, FontSize);
            PointF titleLoc = new PointF(Location.X + (Width / 2 - textSize.Width / 2), Location.Y + 3); // centered horizontally; top margin of 3px

            e.Graphics.DrawText(Title, FontName, FontSize, titleLoc, Color.White, BackRect);
        }

        public void Delete()
        {
            Visible = false;
            Game.RawFrameRender -= render;
        }
    }
}

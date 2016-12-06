namespace EmergencyV
{
    // RPH
    using Rage;

    internal class UIManager
    {
        private static UIManager instance;
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new UIManager();
                return instance;
            }
        }

        GameFiber fiber;

        private UIManager()
        {
        }

        public void Init()
        {
            fiber = GameFiber.StartNew(() =>
            {
                while (true)
                {
                    GameFiber.Yield();

                    Notification.OnUpdate();
                }
            });
            Game.RawFrameRender += OnRawFrameRender;
        }

        private void OnRawFrameRender(object sender, GraphicsEventArgs e)
        {
            Graphics g = e.Graphics;

            Notification.OnDraw(g);
        }


    }
}

namespace EmergencyV
{
    // System
    using System.Drawing;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Graphics = Rage.Graphics;

    internal class NotificationsManager
    {
        private static NotificationsManager instance;
        public static NotificationsManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new NotificationsManager();
                return instance;
            }
        }

        GameFiber fiber;
        List<Notification> notifications;

        private NotificationsManager()
        {
            notifications = new List<Notification>();
        }

        public void StartFiber()
        {
            fiber = GameFiber.StartNew(() =>
            {
                while (true)
                {
                    GameFiber.Yield();

                    Update();
                }
            });
            Game.RawFrameRender += OnRawFrameRender;
        }

        public void AddNotification(Notification n)
        {
            n.Finished += OnNotificationFinished;
            PointF p = n.Location;
            p.Y = GetNextNotificationYCoordinate();
            n.Location = p;
            notifications.Add(n);
        }

        private void Update()
        {
            for (int i = 0; i < notifications.Count; i++)
            {
                notifications[i].OnUpdate();
            }
        }

        private void OnRawFrameRender(object sender, GraphicsEventArgs e)
        {
            Graphics g = e.Graphics;

            for (int i = 0; i < notifications.Count; i++)
            {
                notifications[i].OnDraw(g);
            }
        }

        private void OnNotificationFinished(Notification n)
        {
            float? changeInValue = null; // move up all notifications below the finished one
            for (int i = notifications.IndexOf(n) + 1; i < notifications.Count; i++)
            {
                int previousIndex = i - 1;
                if (previousIndex != -1)
                {
                    if (!changeInValue.HasValue)
                    {
                        changeInValue = -8.0f - notifications[previousIndex].NotificationHeight;
                    }

                    notifications[i].MoveVertically(changeInValue.Value);
                }
            }

            notifications.Remove(n);
            n.Finished -= OnNotificationFinished;
        }

        private float GetNextNotificationYCoordinate()
        {
            float y = 8.0f;

            foreach (Notification n in notifications)
            {
                y += (n.NotificationHeight + 8.0f);
            }

            return y;
        }
    }
}

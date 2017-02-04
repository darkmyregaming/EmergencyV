namespace EmergencyV
{
    // RPH
    using Rage;

    public abstract class Callout
    {
        public delegate void CalloutFinishedEventHandler();

        internal Blip CalloutAreaBlip { get; private set; }

        /// <summary>
        /// Gets or sets the name that will be displayed in the callout notification.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public virtual string DisplayName { get; set; } = "[DISPLAY NAME NOT SET]";
        /// <summary>
        /// Gets or sets the extra information that will be displayed in the callout notification.
        /// </summary>
        /// <value>
        /// The extra information to display.
        /// </value>
        public virtual string DisplayExtraInfo { get; set; } = "";

        private bool hasBeenAccepted;
        public bool HasBeenAccepted
        {
            get
            {
                return hasBeenAccepted;
            }
            internal set
            {
                if (value == hasBeenAccepted)
                    return;
                hasBeenAccepted = value;
                if (hasBeenAccepted && CalloutAreaBlip)
                    CalloutAreaBlip.Delete();
            }
        }

        public event CalloutFinishedEventHandler Finished;
        public bool HasFinished { get; private set; }
        
        /// <summary>
        /// Called before the callout is displayed to the player.
        /// </summary>
        /// <returns><c>true</c> if the callout should be displayed, otherwise, <c>false</c>.</returns>
        public virtual bool OnBeforeCalloutDisplayed()
        {
            return true;
        }

        /// <summary>
        /// Called after the notification is displayed when the player accepts the callout.
        /// </summary>
        /// <returns><c>true</c> if the callout should keep running, otherwise, <c>false</c>, i.e. it should return <c>false</c> if some important entities fail to spawn.</returns>
        public virtual bool OnCalloutAccepted()
        {
            return true;
        }

        /// <summary>
        /// Called after the notification is displayed when the player doesn't accept the callout.
        /// </summary>
        public virtual void OnCalloutNotAccepted()
        {
        }

        /// <summary>
        /// Called in a loop that starts when the player accepts the callout and after <see cref="OnCalloutAccepted"/> executes.
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// Finishes this callout. 
        /// </summary>
        public void Finish()
        {
            if (!HasFinished)
            {
                if (CalloutAreaBlip)
                {
                    CalloutAreaBlip.Delete();
                }
                OnFinished();
                Finished?.Invoke();
                HasFinished = true;
            }
        }

        /// <summary>
        /// Called when this callout is finished, a.k.a. when <see cref="Finish"/> is called for the first time.
        /// Normally, do all clean up here, i.e. dismiss entities, delete blips, delete fires, etc.
        /// </summary>
        protected virtual void OnFinished()
        {
        }

        protected void ShowCalloutAreaBlipBeforeAccepting(Vector3 position, float radius)
        {
            CalloutAreaBlip = new Blip(position, radius);
            CalloutAreaBlip.Color = System.Drawing.Color.FromArgb(160, 200, 20, 20);
            CalloutAreaBlip.Flash(100, 3000);
        }
    }
}

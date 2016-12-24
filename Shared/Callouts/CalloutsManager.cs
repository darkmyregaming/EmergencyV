namespace EmergencyV
{
    // System
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;

    // RPH
    using Rage;

    internal abstract class CalloutsManager<TCalloutData, TCalloutInfoAttribute> where TCalloutData : RegisteredCalloutData
                                                                                 where TCalloutInfoAttribute : CalloutInfoAttribute
    {
        public const string CalloutsFolder = Plugin.ResourcesFolder + @"Callouts\";

        protected List<TCalloutData> RegisteredCalloutsData = new List<TCalloutData>();
        public bool HasLoadedCallouts { get { return RegisteredCalloutsData != null && RegisteredCalloutsData.Count >= 1; } }

        public bool StartNewCalloutsAutomatically { get; set; } = true;

        public virtual bool CanUpdate { get { return HasLoadedCallouts; } }

        private Callout currentCallout;
        protected Callout CurrentCallout { get { return currentCallout; } }

        public bool IsCalloutRunning { get { return currentCallout != null; } }

        private DateTime lastCalloutFinishTime = DateTime.UtcNow;
        private double secondsForNextCallout = 0.0f;

        private bool isDisplayingNewCallout = false;

        private bool runCalloutUpdate;

        protected CalloutsManager()
        {
            if (!Directory.Exists(CalloutsFolder))
                Directory.CreateDirectory(CalloutsFolder);
        }

        public void Update()
        {
            if (CanUpdate)
            {
                if (currentCallout != null && runCalloutUpdate)
                {
                    currentCallout.Update();

                    if (Plugin.Controls.END_CALLOUT.IsJustPressed())
                        FinishCurrentCallout();
                }
                else
                {
                    if (StartNewCalloutsAutomatically && !isDisplayingNewCallout)
                    {
                        if ((DateTime.UtcNow - lastCalloutFinishTime).TotalSeconds > secondsForNextCallout)
                        {
                            StartRandomCallout();
                        }
                    }
                }

                if (Plugin.Controls.FORCE_CALLOUT.IsJustPressed())
                {
                    StartRandomCallout();
                }
            }
        }

        public void StartRandomCallout()
        {
            StartCallout(GetRandomCalloutData());
        }

        public void StartCallout(TCalloutData data)
        {
            FinishCurrentCallout();

            TCalloutData calloutData = data;

            isDisplayingNewCallout = true;
            Game.LogTrivial("Starting callout " + calloutData.InternalName);
            currentCallout = (Callout)Activator.CreateInstance(calloutData.CalloutType);
            currentCallout.Finished += OnCurrentCalloutFinished;
            OnCalloutCreated(currentCallout);
            Game.LogTrivial("Callout - OnBeforeCalloutDisplayed");
            if (currentCallout.OnBeforeCalloutDisplayed())
            {
                const double notificationDisplayTime = 20.0;
                Game.LogTrivial("Callout - Showing notification");
                Notification.Show(currentCallout.DisplayName, currentCallout.DisplayExtraInfo, notificationDisplayTime);
                DateTime startTime = DateTime.UtcNow;

                GameFiber.StartNew(() =>
                {
                    bool accepted = false;
                    Game.LogTrivial("Callout - Start accept key press detect loop");
                    while ((DateTime.UtcNow - startTime).TotalSeconds < notificationDisplayTime + 2.0)
                    {
                        GameFiber.Yield();
                        if (Plugin.Controls.ACCEPT_CALLOUT.IsJustPressed())
                        {
                            Game.LogTrivial("Callout - Pressed accept key, breaking loop");
                            accepted = true;
                            break;
                        }
                    }

                    if (accepted)
                    {
                        Game.LogTrivial("Callout - OnCalloutAccepted");
                        currentCallout.HasBeenAccepted = true;
                        if (currentCallout.OnCalloutAccepted())
                        {
                            Game.LogTrivial("Callout - OnCalloutAccepted:True");
                            runCalloutUpdate = true;
                        }
                        else
                        {
                            Game.LogTrivial("Callout - OnCalloutAccepted:False");
                            FinishCurrentCallout();
                        }
                    }
                    else
                    {
                        Game.LogTrivial("Callout - OnCalloutNotAccepted");
                        Notification.Hide();
                        currentCallout?.OnCalloutNotAccepted();
                        FinishCurrentCallout();
                    }
                });
            }
            else
            {
                FinishCurrentCallout();
            }
        }

        public void FinishCurrentCallout()
        {
            currentCallout?.Finish();
        }

        private void OnCurrentCalloutFinished()
        {
            Game.LogTrivial("Finishing callout");
            runCalloutUpdate = false;
            isDisplayingNewCallout = false;
            lastCalloutFinishTime = DateTime.UtcNow;
            secondsForNextCallout = GetTimeForNextCallout();
            currentCallout = null;
        }

        public void ResetNextCalloutTimer()
        {
            lastCalloutFinishTime = DateTime.UtcNow;
            secondsForNextCallout = GetTimeForNextCallout();
        }

        public void LoadCallouts()
        {
            Game.LogTrivial($"{this.GetType().Name}: Loading callouts...");

            string[] files = Directory.GetFiles(CalloutsFolder, "*.dll", SearchOption.TopDirectoryOnly);
            RegisteredCalloutsData.Clear();

            if (files.Length >= 1)
            {
                foreach (string file in files)
                {
                    Game.LogTrivial($"{this.GetType().Name}: Found file: " + file);

                    // may need to check if it's a valid .NET dll
                    Assembly assembly = Assembly.LoadFrom(file);

                    Type[] calloutTypes = assembly.GetTypes().Where(t => !t.IsAbstract &&
                                                                          t.IsSubclassOf(typeof(Callout)) &&
                                                                          t.CustomAttributes.Any(a => a.AttributeType == typeof(TCalloutInfoAttribute))).ToArray();

                    Game.LogTrivial($"{this.GetType().Name}: Found callout types:");
                    foreach (Type t in calloutTypes)
                    {
                        Game.LogTrivial("       " + t.Name);

                        IEnumerable<TCalloutInfoAttribute> attributes = t.GetCustomAttributes<TCalloutInfoAttribute>();
                        foreach (TCalloutInfoAttribute attribute in attributes)
                        {
                            
                            RegisterCallout((TCalloutData)attribute.GetCalloutData(t));
                        }
                    }
                }

                Game.LogTrivial($"{this.GetType().Name}: Loaded " + RegisteredCalloutsData.Count + " callouts");
            }
            else
            {
                Game.LogTrivial($"{this.GetType().Name}: No files found");
            }
        }

        public TCalloutData GetRandomCalloutData() 
        {
            if (!HasLoadedCallouts)
                throw new InvalidOperationException("No loaded callouts");

            IEnumerable<TCalloutData> possibleCallouts = GetPossibleCallouts();

            int totalProbabilities = possibleCallouts.Sum(d => (int)d.Probability);

            int rndNumber = Plugin.Random.Next(totalProbabilities);

            TCalloutData data = default(TCalloutData);
            foreach (TCalloutData registeredCallout in possibleCallouts)
            {
                if (rndNumber < (int)registeredCallout.Probability)
                {
                    data = registeredCallout;
                    break;
                }

                rndNumber -= (int)registeredCallout.Probability;
            }

            return data;
        }

        public virtual IEnumerable<TCalloutData> GetPossibleCallouts()
        {
            return RegisteredCalloutsData;
        }

        public virtual void RegisterCallout(TCalloutData calloutData)
        {
            if (calloutData.CalloutType.IsAbstract)
                throw new ArgumentException($"The callout type {calloutData.CalloutType.Name} can't be abstract.", nameof(calloutData));
            if (!calloutData.CalloutType.IsSubclassOf(typeof(Callout)))
                throw new ArgumentException($"The callout type {calloutData.CalloutType.Name} must inherit from {nameof(Callout)}", nameof(calloutData));


            Game.LogTrivial($"               { this.GetType().Name}: Registering callout   " + calloutData.CalloutType.Name);
            Game.LogTrivial("                       " + calloutData.InternalName + " - Probability " + calloutData.Probability);
            
            RegisteredCalloutsData.Add(calloutData);
        }

        public virtual void UnregisterCallout(string name)
        {
            IEnumerable<TCalloutData> toRemove = RegisteredCalloutsData.Where(d => d.InternalName == name);

            foreach (TCalloutData d in toRemove)
            {
                Game.LogTrivial($"               { this.GetType().Name}: Unregistering callout   " + d.CalloutType.Name);
                Game.LogTrivial("                       " + d.InternalName + " - Probability " + d.Probability);
                RegisteredCalloutsData.Remove(d);
            }
        }

        public virtual void CleanUp(bool isTerminating)
        {
            if (currentCallout != null)
            {
                currentCallout.Finish();
                currentCallout = null;
            }
        }

        protected virtual void OnCalloutCreated(Callout callout)
        {
        }

        protected static double GetTimeForNextCallout()
        {
            double t = MathHelper.GetRandomDouble(Plugin.UserSettings.CALLOUTS.MIN_SECONDS_BETWEEN_CALLOUTS, Plugin.UserSettings.CALLOUTS.MAX_SECONDS_BETWEEN_CALLOUTS);
            Game.LogTrivial("GetTimeForNextCallout(): " + t);
            return t;
        }
    }
}

namespace EmergencyV
{
    // System
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;

    // RPH
    using Rage;

    internal class FireCalloutsManager
    {
        private const string CalloutsFolder = Plugin.ResourcesFolder + @"Fire Callouts\";

        private static FireCalloutsManager instance;
        public static FireCalloutsManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new FireCalloutsManager();
                return instance;
            }
        }

        private List<FireRegisteredCalloutData> registeredCalloutsData = new List<FireRegisteredCalloutData>();

        public bool HasLoadedCallouts { get { return registeredCalloutsData.Count >= 1; } }

        public bool StartNewCalloutsAutomatically { get; set; } = true;

        private FireCallout currentCallout;

        private DateTime lastCalloutFinishTime = DateTime.UtcNow;
        private double secondsForNextCallout = GetTimeForNextCallout();

        private bool isDisplayingNewCallout = false;

        private bool runCalloutUpdate;

        public FireCalloutsManager()
        {
            if (!Directory.Exists(CalloutsFolder))
                Directory.CreateDirectory(CalloutsFolder);

        }

        public void Update()
        {
            if (PlayerManager.Instance.IsFirefighter && HasLoadedCallouts)
            {
                if (currentCallout != null && runCalloutUpdate)
                {
                    currentCallout.Update();

                    if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
                        FinishCurrentCallout();
                }
                else
                {
                    if (StartNewCalloutsAutomatically && !isDisplayingNewCallout)
                    {
                        if ((DateTime.UtcNow - lastCalloutFinishTime).TotalSeconds > secondsForNextCallout)
                        {
                            StartNewCallout();
                        }
                    }
                }
            }
        }

        public void StartNewCallout()
        {
            FinishCurrentCallout();

            FireRegisteredCalloutData calloutData = GetRandomCalloutData(PlayerManager.Instance.FirefighterRole);

            isDisplayingNewCallout = true;
            Game.LogTrivial("Starting callout " + calloutData.InternalName);
            currentCallout = (FireCallout)Activator.CreateInstance(calloutData.CalloutType);
            currentCallout.Role = PlayerManager.Instance.FirefighterRole;
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
                        if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
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
                        currentCallout.OnCalloutNotAccepted();
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
            if (currentCallout != null)
            {
                Game.LogTrivial("Finishing callout");
                currentCallout.Finish();
                currentCallout = null;
            }
            runCalloutUpdate = false;
            isDisplayingNewCallout = false;
            lastCalloutFinishTime = DateTime.UtcNow;
            secondsForNextCallout = GetTimeForNextCallout();
        }

        public void ResetNextCalloutTimer()
        {
            lastCalloutFinishTime = DateTime.UtcNow;
            secondsForNextCallout = GetTimeForNextCallout();
        }

        public void LoadCallouts()
        {
            Game.LogTrivial("FireCalloutsManager: Loading callouts...");

            string[] files = Directory.GetFiles(CalloutsFolder, "*.dll", SearchOption.TopDirectoryOnly);
            registeredCalloutsData.Clear();

            if (files.Length >= 1)
            {
                foreach (string file in files)
                {
                    Game.LogTrivial("FireCalloutsManager: Found file: " + file);

                    // may need to check if it's a valid .NET dll
                    Assembly assembly = Assembly.LoadFrom(file);

                    Type[] calloutTypes = assembly.GetTypes().Where(t => !t.IsAbstract &&
                                                                          t.IsSubclassOf(typeof(FireCallout)) &&
                                                                          t.CustomAttributes.Any(a => a.AttributeType == typeof(FireCalloutInfoAttribute))).ToArray();

                    Game.LogTrivial("FireCalloutsManager: Found callout types:");
                    foreach (Type t in calloutTypes)
                    {
                        Game.LogTrivial("       " + t.Name);

                        IEnumerable<FireCalloutInfoAttribute> attributes = t.GetCustomAttributes<FireCalloutInfoAttribute>();
                        foreach (FireCalloutInfoAttribute attribute in attributes)
                        {
                            RegisterCallout(t, attribute.CalloutName, attribute.Role, attribute.Probability);
                        }
                    }
                }

                Game.LogTrivial("FireCalloutsManager: Loaded " + registeredCalloutsData.Count + " callouts");
            }
            else
            {
                Game.LogTrivial("FireCalloutsManager: No files found");
            }
        }

        public FireRegisteredCalloutData GetRandomCalloutData(FirefighterRole role = FirefighterRole.None) // if None all callouts can be returned
        {
            if (!HasLoadedCallouts)
                throw new InvalidOperationException("No loaded callouts");

            IEnumerable<FireRegisteredCalloutData> possibleCallouts = role == FirefighterRole.None ? registeredCalloutsData : registeredCalloutsData.Where(d => d.Role == role);

            int totalProbabilities = possibleCallouts.Sum(d => (int)d.Probability);

            int rndNumber = Plugin.Random.Next(totalProbabilities);

            FireRegisteredCalloutData data = new FireRegisteredCalloutData();
            foreach (FireRegisteredCalloutData registeredCallout in possibleCallouts)
            {
                if (rndNumber < (int)registeredCallout.Probability)
                {
                    data = registeredCallout;
                }

                rndNumber -= (int)registeredCallout.Probability;
            }

            return data;
        }

        public void RegisterCallout(Type calloutType, string internalCalloutName, FirefighterRole role, FireCalloutProbability probability)
        {
            if (calloutType.IsAbstract)
                throw new ArgumentException($"The callout type {calloutType.Name} can't be abstract.", nameof(calloutType));
            if (!calloutType.IsSubclassOf(typeof(FireCallout)))
                throw new ArgumentException($"The callout type {calloutType.Name} must inherit from {nameof(FireCallout)}", nameof(calloutType));


            Game.LogTrivial("               FireCalloutsManager: Registering callout   " + calloutType.Name);
            Game.LogTrivial("                       " + internalCalloutName + " for " + role + " and probability " + probability);

            FireRegisteredCalloutData data = new FireRegisteredCalloutData(calloutType, internalCalloutName, role, probability);
            registeredCalloutsData.Add(data);
        }

        public void CleanUp(bool isTerminating)
        {
            if (currentCallout != null)
            {
                currentCallout.Finish();
                currentCallout = null;
            }
        }

        private static double GetTimeForNextCallout()
        {
            double t = MathHelper.GetRandomDouble(Plugin.UserSettings.CALLOUTS.MIN_SECONDS_BETWEEN_CALLOUTS, Plugin.UserSettings.CALLOUTS.MAX_SECONDS_BETWEEN_CALLOUTS);
            Game.LogTrivial("GetTimeForNextCallout(): " + t);
            return t;
        }
    }
}

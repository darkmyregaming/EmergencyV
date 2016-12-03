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

        public FireCalloutsManager()
        {
            if (!Directory.Exists(CalloutsFolder))
                Directory.CreateDirectory(CalloutsFolder);

        }

        public void DoTest()
        {
            if (!HasLoadedCallouts)
                LoadCallouts();

            FireRegisteredCalloutData data = GetRandomCalloutData();

            FireCallout calloutInstance = (FireCallout)Activator.CreateInstance(data.CalloutType);

            Game.LogTrivial("Callout Instance: " + calloutInstance.DisplayName);
            calloutInstance.ExecuteSomething();
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
                            Game.LogTrivial("               " + attribute.CalloutName + " for " + attribute.Role + " and probability " + attribute.Probability);

                            FireRegisteredCalloutData data = new FireRegisteredCalloutData(t, attribute.CalloutName, attribute.Role, attribute.Probability);
                            registeredCalloutsData.Add(data);
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
    }
}

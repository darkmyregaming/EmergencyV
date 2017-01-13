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

    internal class AddonsManager
    {
        private static AddonsManager instance;
        public static AddonsManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new AddonsManager();
                return instance;
            }
        }

        public List<Assembly> LoadedAssemblies = new List<Assembly>();
        public List<API.Addon> CurrentAddons = new List<API.Addon>();

        private AddonsManager()
        {
        }

        public void LoadAddons()
        {
            Game.LogTrivial($"{this.GetType().Name}: Loading addons...");

            string[] files = Directory.GetFiles(Plugin.AddonsFolder, "*.dll", SearchOption.TopDirectoryOnly);
            UnloadAddons();

            if (files.Length >= 1)
            {
                foreach (string file in files)
                {
                    // may need to check if it's a valid .NET dll
                    Assembly assembly = Assembly.LoadFrom(file);
                    if (!LoadedAssemblies.Contains(assembly))
                        LoadedAssemblies.Add(assembly);

                    Type[] addonsTypes = assembly.GetTypes().Where(t => !t.IsAbstract &&
                                                                         t.IsSubclassOf(typeof(API.Addon))).ToArray();
                    
                    foreach (Type t in addonsTypes)
                    {
                        Game.LogTrivial($"{this.GetType().Name}: Creating addon {t.Name} instance...");
                        API.Addon instance = (API.Addon)Activator.CreateInstance(t);
                        CurrentAddons.Add(instance);

                        Game.LogTrivial($"      Addon({instance.Name}) - OnStart()");
                        instance.OnStart();
                    }
                }

                Game.LogTrivial($"{this.GetType().Name}: Loaded " + CurrentAddons.Count + " addons");
            }
        }

        public void UnloadAddons()
        {
            if (CurrentAddons.Count >= 1)
            {
                Game.LogTrivial($"{this.GetType().Name}: Unloading addons...");
                foreach (API.Addon a in CurrentAddons)
                {
                    Game.LogTrivial($"      Addon({a.Name}) - OnCleanUp()");
                    a.OnCleanUp();
                }
                CurrentAddons.Clear();
            }
        }
    }
}

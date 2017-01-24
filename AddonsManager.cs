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
                    try
                    {
                        // may need to check if it's a valid .NET dll
                        //Assembly assembly = Assembly.LoadFrom(file); // LoadFrom locks the files,
                        Assembly assembly = Assembly.Load(File.ReadAllBytes(file));
                        if (!LoadedAssemblies.Contains(assembly))
                            LoadedAssemblies.Add(assembly);

                        Type[] addonsTypes = assembly.GetTypes().Where(t => !t.IsAbstract &&
                                                                             t.IsSubclassOf(typeof(API.Addon))).ToArray();

                        Game.LogTrivial($"{this.GetType().Name}:    - Loading assembly {assembly} from {file}");
                        if (addonsTypes.Length > 0)
                        {
                            foreach (Type t in addonsTypes)
                            {
                                Game.LogTrivial($"{this.GetType().Name}:        - Creating addon instance from {t.FullName}");
                                API.Addon instance = (API.Addon)Activator.CreateInstance(t);
                                CurrentAddons.Add(instance);

                                Game.LogTrivial($"{this.GetType().Name}:            Addon({instance.GetType().FullName}) - OnStart()");
                                instance.OnStart();
                            }
                        }
                        else
                        {
                            Game.LogTrivial($"{this.GetType().Name}:        - The assembly doesn't contain any class that inherits from {nameof(API.Addon)}.");
                        }
                    }
                    catch (BadImageFormatException ex)
                    {
                        Game.LogTrivial($"{this.GetType().Name}: Can't load {file}, it isn't a valid .NET assembly");
                        Game.LogTrivial($"{this.GetType().Name}: Exception: {ex}");
                    }
                    catch (Exception ex)
                    {
                        Game.LogTrivial($"{this.GetType().Name}: Failed to load file as addon: {file}");
                        Game.LogTrivial($"{this.GetType().Name}: Exception: {ex}");
                    }
                    Game.LogTrivial($"{this.GetType().Name}: ");
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
                    Game.LogTrivial($"{this.GetType().Name}:            Addon({a.GetType().FullName}) - OnCleanUp()");
                    a.OnCleanUp();
                }
                CurrentAddons.Clear();
            }
        }
    }
}

namespace EmergencyV
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;

    public abstract class AdvancedPed
    {
        public delegate void AdvancedPedEventHandler(AdvancedPed sender);

        public Ped Ped { get; }
        public bool CanDoUpdates { get; set; } = true;
        public AIController AI { get; }
        public int? PreferedVehicleSeatIndex { get; set; }

        public event AdvancedPedEventHandler Deleted;

        internal AdvancedPed(Ped ped)
        {
            Ped = ped;
            Ped.BlockPermanentEvents = true;
            AI = new AIController(this);

            RegisterAdvancedPed(this);
        }

        internal AdvancedPed(Model model, Vector3 position, float heading) : this(new Ped(model, position, heading))
        {
        }

        internal AdvancedPed(Vector3 position) : this(new Ped(position))
        {
        }

        private void Update()
        {
            AI.Update();

            UpdateInternal();
        }

        protected abstract void UpdateInternal();



        public static T[] GetAllAdvancedPedsOfType<T>() where T : AdvancedPed
        {
            return CurrentAdvancedPedsByType[typeof(T)].Cast<T>().ToArray();
        }

        private static Dictionary<Type, List<AdvancedPed>> CurrentAdvancedPedsByType { get; } = new Dictionary<Type, List<AdvancedPed>>();
        private static Dictionary<Type, GameFiber> UpdateAdvancedPedsFibersByType { get; } = new Dictionary<Type, GameFiber>();

        private static void RegisterAdvancedPed(AdvancedPed a)
        {
            Type t = a.GetType();

            if (CurrentAdvancedPedsByType.ContainsKey(t))
            {
                CurrentAdvancedPedsByType[t].Add(a);
            }
            else
            {
                CurrentAdvancedPedsByType.Add(t, new List<AdvancedPed>() { a });
            }

            if (!UpdateAdvancedPedsFibersByType.ContainsKey(t))
            {
                Game.LogTrivial($"Creating update fiber for AdvancedPed<[{t.Name}]>");
                GameFiber fiber = GameFiber.StartNew(() => { UpdateAdvancedPedsLoop(CurrentAdvancedPedsByType[t]); }, $"AdvancedPed<[{t.Name}]> Update Fiber");
                UpdateAdvancedPedsFibersByType.Add(t, fiber);
            }
        }

        private static void UpdateAdvancedPedsLoop(List<AdvancedPed> pedsList)
        {
            while (true)
            {
                GameFiber.Yield();

                for (int i = pedsList.Count - 1; i >= 0; i--)
                {
                    AdvancedPed a = pedsList[i];
                    if (a != null && a.CanDoUpdates && a.Ped && !a.Ped.IsDead)
                    {
                        a.Update();
                    }
                    else
                    {
                        a.Deleted?.Invoke(a);
                        pedsList.RemoveAt(i);
                    }
                }
            }
        }
    }
}

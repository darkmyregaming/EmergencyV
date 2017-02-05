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
            return UpdateInstancesFibersManager.Instance.GetAllInstancesOfType<T>();
        }
        
        protected static void RegisterAdvancedPed<T>(T a) where T : AdvancedPed
        {
            if (!UpdateInstancesFibersManager.Instance.IsUpdateDataSetForType<T>())
            {
                UpdateInstancesFibersManager.Instance.SetUpdateDataForType<T>(
                    canDoUpdateCallback: (p) => p.Ped && !p.Ped.IsDead,
                    onInstanceUpdateCallback: (p) => { if (p.CanDoUpdates) { p.Update(); } },
                    onInstanceUnregisteredCallback: (p) => p.Deleted?.Invoke(p));
            }

            UpdateInstancesFibersManager.Instance.RegisterInstance(a);
        }
    }
}

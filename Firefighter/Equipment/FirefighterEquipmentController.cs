namespace EmergencyV
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;

    // RPH
    using Rage;
    using Rage.Native;

    internal class FirefighterEquipmentController
    {
        public static readonly Dictionary<Type, IFirefighterEquipment> RegisteredEquipments = new Dictionary<Type, IFirefighterEquipment>();
        private static readonly List<IFirefighterEquipment> RegisteredEquipmentsToUpdate = new List<IFirefighterEquipment>();

        public static void RegisterEquipments()
        {
            RegisteredEquipments.Clear();
            RegisteredEquipmentsToUpdate.Clear();

            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && typeof(IFirefighterEquipment).IsAssignableFrom(t));

            foreach (Type type in types)
            {
                IFirefighterEquipment e = (IFirefighterEquipment)Activator.CreateInstance(type);

                if (e.ShouldUpdateIfEquipped)
                    RegisteredEquipmentsToUpdate.Add(e);

                RegisteredEquipments.Add(type, e);
            }
        }

        public Ped Ped { get; protected set; }
        public virtual bool IsPlayer => false;
        public Dictionary<string, object> Memory { get; } = new Dictionary<string, object>();

        internal FirefighterEquipmentController(Ped ped)
        {
            Ped = ped;
        }

        internal virtual void Update()
        {
            for (int i = 0; i < RegisteredEquipmentsToUpdate.Count; i++)
            {
                if (RegisteredEquipmentsToUpdate[i].IsEquipped(this))
                {
                    RegisteredEquipmentsToUpdate[i].OnEquippedUpdate(this);
                }
            }
        }

        public void SetEquipped<T>(bool equip) where T : IFirefighterEquipment
        {
            if (equip)
                RegisteredEquipments[typeof(T)].OnGetEquipment(this);
            else
                RegisteredEquipments[typeof(T)].OnLeaveEquipment(this);
        }

        public bool IsEquipped<T>() where T : IFirefighterEquipment
        {
            return RegisteredEquipments[typeof(T)].IsEquipped(this);
        }
    }
}

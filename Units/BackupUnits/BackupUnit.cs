namespace EmergencyV
{
    // System
    using System.Drawing;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal abstract class BackupUnit
    {
        protected BlipSprite VehicleBlipSprite { get; }
        protected string VehicleBlipName { get; }
        protected Color VehicleBlipColor;
        
        protected RotatedVector3 SpawnLocation { get; }

        public Blip VehicleBlip { get; private set; }
        public Vehicle Vehicle { get; private set; }
        public AdvancedPed Driver { get; private set; }
        public AdvancedPed[] Peds { get; private set; } // includes driver

        public AIBackupUnitController AI { get; }

        public bool IsCreated { get; private set; }

        public bool IsDismissedOrDeleted { get; private set; }

        private Blip isRespondingVehicleBlip;
        private bool isResponding;
        public bool IsResponding
        {
            get { return isResponding; }
            set
            {
                if (isRespondingVehicleBlip)
                    isRespondingVehicleBlip.Delete();

                if (value)
                {
                    if (Vehicle)
                    {
                        isRespondingVehicleBlip = new Blip(Vehicle);
                        isRespondingVehicleBlip.Sprite = BlipSprite.PoliceChase;
                        isRespondingVehicleBlip.Scale = 0.475f;
                        isRespondingVehicleBlip.Name = VehicleBlipName + " - Responding";
                        isRespondingVehicleBlip.Order = 1;
                    }
                }

                if (VehicleBlip)
                {
                    NativeFunction.Natives.SetBlipAsShortRange(VehicleBlip, !value);
                }

                isResponding = value;
            }
        }

        public bool IsAnyPedInVehicle
        {
            get
            {
                return Peds.Any(a => a.Ped.IsInVehicle(Vehicle, false));
            }
        }

        public bool AreAllPedsInVehicle
        {
            get
            {
                return Peds.All(a => a.Ped.IsInVehicle(Vehicle, false));
            }
        }

        public BackupUnit(Vector3 position, float heading, BlipSprite blipSprite, string blipName, Color blipColor)
        {
            SpawnLocation = new RotatedVector3(position, heading);

            VehicleBlipSprite = blipSprite;
            VehicleBlipColor = blipColor;
            VehicleBlipName = blipName;

            ReCreate();

            AI = new AIBackupUnitController(this);
        }

        public void ReCreate()
        {
            Delete();

            Vehicle veh;
            AdvancedPed[] peds;
            CreateInternal(out veh, out peds);

            Vehicle = veh;
            Peds = peds;
            Driver = Peds.FirstOrDefault();

            VehicleBlip = new Blip(Vehicle);
            VehicleBlip.Sprite = VehicleBlipSprite;
            VehicleBlip.Scale = 0.45f;
            VehicleBlip.Color = VehicleBlipColor;
            VehicleBlip.Name = VehicleBlipName;
            NativeFunction.Natives.SetBlipAsShortRange(VehicleBlip, true);

            IsCreated = true;
            IsDismissedOrDeleted = false;
        }

        public void Dismiss()
        {
            if (isRespondingVehicleBlip)
            {
                isRespondingVehicleBlip.Delete();
                isRespondingVehicleBlip = null;
            }

            if (VehicleBlip)
            {
                VehicleBlip.Delete();
                VehicleBlip = null;
            }

            if (Vehicle)
            {
                Vehicle.Dismiss();
            }

            if (Peds != null)
            {
                foreach (AdvancedPed a in Peds)
                {
                    if (a.Ped)
                    {
                        a.Ped.Dismiss();
                    }
                }
            }

            IsResponding = false;

            DismissInternal();

            IsDismissedOrDeleted = true;
        }

        public void Delete()
        {
            if (AI != null)
            {
                AI.AbortAllTasks();
            }

            if (isRespondingVehicleBlip)
            {
                isRespondingVehicleBlip.Delete();
                isRespondingVehicleBlip = null;
            }

            if (VehicleBlip)
            {
                VehicleBlip.Delete();
                VehicleBlip = null;
            }

            if (Vehicle)
            {
                Vehicle.Delete();
                Vehicle = null;
            }

            if (Peds != null)
            {
                foreach (AdvancedPed a in Peds)
                {
                    if (a.Ped)
                    {
                        a.Ped.Delete();
                    }
                }
                Peds = null;
            }

            IsResponding = false;

            DeleteInternal();

            IsCreated = false;
            IsDismissedOrDeleted = true;
        }

        private void Update()
        {
            if (IsCreated)
            {
                AI.Update();
            }

            UpdateInternal();
        }

        protected abstract void CreateInternal(out Vehicle vehicle, out AdvancedPed[] peds);
        protected abstract void UpdateInternal();
        protected abstract void DeleteInternal();
        protected abstract void DismissInternal();

        public static T[] GetAllBackupUnitsOfType<T>() where T : BackupUnit
        {
            return UpdateInstancesFibersManager.Instance.GetAllInstancesOfType<T>();
        }

        protected static void RegisterBackupUnit<T>(T unit) where T : BackupUnit
        {
            if (!UpdateInstancesFibersManager.Instance.IsUpdateDataSetForType<T>())
            {
                UpdateInstancesFibersManager.Instance.SetUpdateDataForType<T>(
                    canDoUpdateCallback: null,
                    onInstanceUpdateCallback: (u) => u.Update(),
                    onInstanceUnregisteredCallback: (u) => u.Delete());
            }

            UpdateInstancesFibersManager.Instance.RegisterInstance(unit);
        }
    }
}

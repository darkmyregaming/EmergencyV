namespace EmergencyV
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class PlayerFireEquipmentController : FirefighterEquipmentController
    {
        private static PlayerFireEquipmentController instance;
        public static PlayerFireEquipmentController Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayerFireEquipmentController();
                return instance;
            }
        }

        public override bool IsPlayer => true;

        private bool isNearFiretruck = false;
        private DateTime lastFiretrucksCheckTime = DateTime.UtcNow;

        private PlayerFireEquipmentController() : base(Game.LocalPlayer.Character)
        {
        }

        internal override void Update()
        {
            Ped = Game.LocalPlayer.Character;

            base.Update();

            if ((DateTime.UtcNow - lastFiretrucksCheckTime).TotalSeconds > 2.5)
            {
                bool nearFiretruckNow = IsFiretruckNearbyPlayer();

                if (isNearFiretruck != nearFiretruckNow)
                {
                    if (nearFiretruckNow)
                    {
                        CreateVehicleEquipmentMenu();
                    }
                    else
                    {
                        RemoveVehicleEquipmentMenu();
                    }
                }

                isNearFiretruck = nearFiretruckNow;
                lastFiretrucksCheckTime = DateTime.UtcNow;
            }
        }

        internal void CleanUp(bool isTerminating)
        {
        }

        private bool IsFiretruckNearbyPlayer()
        {
            if (!Game.LocalPlayer.Character)
                return false;

            bool isNearAnyFiretruck = false;

            Vehicle[] nearbyFiretrucks = Game.LocalPlayer.Character.GetNearbyVehicles(4).Where(v => v.Model == new Model("firetruk")).ToArray();
            if (nearbyFiretrucks.Length >= 1)
            {
                for (int i = 0; i < nearbyFiretrucks.Length; i++)
                {
                    Vehicle v = nearbyFiretrucks[0];
                    if (v && Vector3.DistanceSquared(v.RearPosition, Game.LocalPlayer.Character.Position) < 2.5f * 2.5f)
                    {
                        isNearAnyFiretruck = true;
                    }
                }
            }

            return isNearAnyFiretruck;
        }

        private void CreateVehicleEquipmentMenu()
        {
            PluginMenu.Instance.AddMenu("VEHICLE_EQUIPMENT_SUBMENU");

            PluginMenu.Instance.AddItem("OPEN_VEHICLE_EQUIPMENT_SUBMENU_ITEM", "MAIN_MENU", "Equipment", null, null, "VEHICLE_EQUIPMENT_SUBMENU");

            foreach (KeyValuePair<Type, IFirefighterEquipment> p in RegisteredEquipments)
            {
                IFirefighterEquipment e = p.Value;
                PluginMenu.Instance.AddItem($"VEHICLE_EQUIPMENT_{e.DisplayName.ToUpper().Replace(" ", "_")}_ITEM", "VEHICLE_EQUIPMENT_SUBMENU", e.IsEquipped(this) ? $"Leave {e.DisplayName}" : $"Get {e.DisplayName}", () =>
                {
                    bool equipped = e.IsEquipped(this);
                    if (equipped)
                    {
                        e.OnLeaveEquipment(this);
                    }
                    else
                    {
                        e.OnGetEquipment(this);
                    }

                    PluginMenu.Instance.UpdateItem($"VEHICLE_EQUIPMENT_{e.DisplayName.ToUpper().Replace(" ", "_")}_ITEM", !equipped ? $"Leave {e.DisplayName}" : $"Get {e.DisplayName}");
                });
            }
        }

        private void RemoveVehicleEquipmentMenu()
        {
            PluginMenu.Instance.RemoveMenu("VEHICLE_EQUIPMENT_SUBMENU");

            PluginMenu.Instance.RemoveItem("OPEN_VEHICLE_EQUIPMENT_SUBMENU_ITEM");
        }

        //private void CreateToggleFlashlightItem()
        //{
        //    PluginMenu.Instance.AddItem("TOGGLE_FLASHLIGHT_ITEM", "ACTIONS_SUBMENU", "Toggle Flashlight", () => { IsFlashlightOn = !IsFlashlightOn; }, Plugin.Controls["TOGGLE_FLASHLIGHT"]);
        //}

        //private void RemoveToggleFlashlightItem()
        //{
        //    PluginMenu.Instance.RemoveItem("TOGGLE_FLASHLIGHT_ITEM");
        //}




    }
}

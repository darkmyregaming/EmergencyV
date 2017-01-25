namespace EmergencyV
{
    // System
    using System;
    using System.Linq;

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

        public override bool HasFireGear
        {
            get
            {
                return base.HasFireGear;
            }

            set
            {
                bool prevValue = base.HasFireGear;
                base.HasFireGear = value;
                if (base.HasFireGear != prevValue && Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
                {
                    if (value)
                    {
                        CreateToggleFlashlightItem();
                    }
                    else
                    {
                        RemoveToggleFlashlightItem();
                    }
                }
            }
        }

        private PlayerFireEquipmentController() : base(Game.LocalPlayer.Character)
        {
        }

        private bool isNearFiretruck = false;
        private DateTime lastFiretrucksCheckTime = DateTime.UtcNow;

        internal override void Update()
        {
            Ped = Game.LocalPlayer.Character;

            if ((DateTime.UtcNow - lastFiretrucksCheckTime).TotalSeconds > 3.25)
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
            }

            if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
            {
                if (IsFlashlightOn)
                {
                    Vector3 flashlightPos = Game.LocalPlayer.Character.GetOffsetPosition(Game.LocalPlayer.Character.GetPositionOffset(Game.LocalPlayer.Character.GetBonePosition(Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE)) + Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET.ToVector3());

                    Util.DrawSpotlightWithShadow(flashlightPos, Game.LocalPlayer.Character.GetBoneRotation(PedBoneId.Spine2).ToVector(), Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_COLOR.ToColor(), 13.25f, 9.25f, 2.0f, 20f, 20.0f);
                }
            }
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

            PluginMenu.Instance.AddItem("VEHICLE_EQUIPMENT_FIRE_GEAR_ITEM", "VEHICLE_EQUIPMENT_SUBMENU", HasFireGear ? "Leave Fire Gear" : "Get Fire Gear", () =>
            {
                HasFireGear = !HasFireGear;
                PluginMenu.Instance.UpdateItem("VEHICLE_EQUIPMENT_FIRE_GEAR_ITEM", HasFireGear ? "Leave Fire Gear" : "Get Fire Gear");
            });
            PluginMenu.Instance.AddItem("VEHICLE_EQUIPMENT_FIRE_EXTINGUISHER_ITEM", "VEHICLE_EQUIPMENT_SUBMENU", HasFireExtinguisher ? "Leave Fire Extinguisher" : "Get Fire Extinguisher", () =>
            {
                HasFireExtinguisher = !HasFireExtinguisher;
                PluginMenu.Instance.UpdateItem("VEHICLE_EQUIPMENT_FIRE_EXTINGUISHER_ITEM", HasFireExtinguisher ? "Leave Fire Extinguisher" : "Get Fire Extinguisher");
            });
        }

        private void RemoveVehicleEquipmentMenu()
        {
            PluginMenu.Instance.RemoveMenu("VEHICLE_EQUIPMENT_SUBMENU");

            PluginMenu.Instance.RemoveItem("OPEN_VEHICLE_EQUIPMENT_SUBMENU_ITEM");
        }

        private void CreateToggleFlashlightItem()
        {
            PluginMenu.Instance.AddItem("TOGGLE_FLASHLIGHT_ITEM", "ACTIONS_SUBMENU", "Toggle Flashlight", () => { IsFlashlightOn = !IsFlashlightOn; }, Plugin.Controls["TOGGLE_FLASHLIGHT"]);
        }

        private void RemoveToggleFlashlightItem()
        {
            PluginMenu.Instance.RemoveItem("TOGGLE_FLASHLIGHT_ITEM");
        }
    }
}

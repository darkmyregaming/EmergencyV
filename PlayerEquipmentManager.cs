namespace EmergencyV
{
    // System
    using System;
    using System.Linq;

    // RPH
    using Rage;
    using Rage.Native;

    internal class PlayerEquipmentManager
    {
        private static PlayerEquipmentManager instance;
        public static PlayerEquipmentManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayerEquipmentManager();
                return instance;
            }
        }

        public bool HasFireExtinguisher
        {
            get
            {
                if (!Plugin.LocalPlayerCharacter)
                    return false;

                return Plugin.LocalPlayerCharacter.Inventory.Weapons.Contains(WeaponHash.FireExtinguisher);
            }
            set
            {
                if (!Plugin.LocalPlayerCharacter)
                    return;

                if (value)
                    Plugin.LocalPlayerCharacter.Inventory.GiveNewWeapon(WeaponHash.FireExtinguisher, 999, true);
                else
                    Plugin.LocalPlayerCharacter.Inventory.Weapons.Remove(WeaponHash.FireExtinguisher);

            }
        }

        public bool HasFireGear
        {
            get
            {
                if (!Plugin.LocalPlayerCharacter)
                    return false;

                int drawableIndex, drawableTextureIndex;
                Plugin.LocalPlayerCharacter.GetVariation(8, out drawableIndex, out drawableTextureIndex);
                int propDrawableIndex = NativeFunction.Natives.GetPedPropIndex<int>(Plugin.LocalPlayerCharacter, 1);
                return drawableIndex == 1 && propDrawableIndex == 0;
            }
            set
            {
                if (!Plugin.LocalPlayerCharacter)
                    return;

                if (value)
                {
                    Plugin.LocalPlayerCharacter.SetVariation(8, 1, 0);
                    NativeFunction.Natives.SetPedPropIndex(Plugin.LocalPlayerCharacter, 1, 0, 0, true);
                }
                else
                {
                    Plugin.LocalPlayerCharacter.SetVariation(8, 0, 0);
                    NativeFunction.Natives.ClearPedProp(Plugin.LocalPlayerCharacter, 1);
                }
            }
        }
        public bool HasAxe { get; }

        private PlayerEquipmentManager()
        {
        }

        public void Update()
        {
            if (PlayerManager.Instance.IsFireFighter)
            {
                FireFighterUpdate();
            }
        }

        private bool isNearFiretruck = false;
        private bool isGettingEquipment = false;
        private DateTime lastFiretrucksCheckTime = DateTime.UtcNow;
        private void FireFighterUpdate()
        {
            if ((DateTime.UtcNow - lastFiretrucksCheckTime).TotalSeconds > 2.0)
            {
                isNearFiretruck = IsFiretruckNearbyPlayer();
            }


            if (isGettingEquipment)
            {
                Game.DisplayHelp("[1] Fire extinguisher~n~[2] Fire gear~n~[3] Axe"); // TODO: some cool GUI for the get equipment menu

                if (Game.IsKeyDown(System.Windows.Forms.Keys.D1))
                {
                    HasFireExtinguisher = !HasFireExtinguisher;

                    isGettingEquipment = false;
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.D2))
                {
                    HasFireGear = !HasFireGear;

                    isGettingEquipment = false;
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.D3))
                {
                    // TODO: use "prop_tool_fireaxe" to give an axe to the player
                    isGettingEquipment = false;
                }
            }
            else if (isNearFiretruck)
            {
                Game.DisplayHelp("Press ~INPUT_CONTEXT~ to get/save equipment", 20);
                if (Game.IsControlJustPressed(0, GameControl.Context))
                {
                    isGettingEquipment = true;
                }
            }
        }


        private bool IsFiretruckNearbyPlayer()
        {
            if (!Plugin.LocalPlayerCharacter)
                return false;

            bool isNearAnyFiretruck = false;

            Vehicle[] nearbyFiretrucks = Plugin.LocalPlayerCharacter.GetNearbyVehicles(4).Where(v => v.Model == new Model("firetruk")).ToArray();
            if (nearbyFiretrucks.Length >= 1)
            {
                for (int i = 0; i < nearbyFiretrucks.Length; i++)
                {
                    Vehicle v = nearbyFiretrucks[0];
                    if (v && Vector3.DistanceSquared(v.RearPosition, Plugin.LocalPlayerCharacter.Position) < 2.5f * 2.5f)
                    {
                        isNearAnyFiretruck = true;
                    }
                }
            }

            return isNearAnyFiretruck;
        }
    }
}

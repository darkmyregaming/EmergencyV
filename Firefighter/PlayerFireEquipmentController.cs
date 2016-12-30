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

        private PlayerFireEquipmentController() : base(Plugin.LocalPlayerCharacter)
        {
        }

        private bool isNearFiretruck = false;
        private bool isGettingEquipment = false;
        private DateTime lastFiretrucksCheckTime = DateTime.UtcNow;

        public override void Update()
        {
            if ((DateTime.UtcNow - lastFiretrucksCheckTime).TotalSeconds > 3.25)
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

            if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
            {
                if (Game.IsKeyDown(System.Windows.Forms.Keys.L))
                {
                    IsFlashlightOn = !IsFlashlightOn;
                }

                if (IsFlashlightOn)
                {
                    Vector3 flashlightPos = Plugin.LocalPlayerCharacter.GetOffsetPosition(Plugin.LocalPlayerCharacter.GetPositionOffset(Plugin.LocalPlayerCharacter.GetBonePosition(Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE)) + Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET.ToVector3());

                    Util.DrawSpotlightWithShadow(flashlightPos, Plugin.LocalPlayerCharacter.GetBoneRotation(PedBoneId.Spine2).ToVector(), Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_COLOR.ToColor(), 13.25f, 9.25f, 2.0f, 20f, 20.0f);
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

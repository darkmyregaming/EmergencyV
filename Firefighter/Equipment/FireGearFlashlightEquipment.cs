//namespace EmergencyV // TODO: reimplement flashlight
//{
//    // RPH
//    using Rage;
//    using Rage.Native;

//    internal class FireGearFlashlightEquipment : IFirefighterEquipment
//    {
//        public const string IsFlashlightOnMemoryKey = "IsFlashlightOn";

//        string IFirefighterEquipment.DisplayName => "Flashlight";
//        bool IFirefighterEquipment.ShouldUpdateIfEquipped => Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED;

//        bool IFirefighterEquipment.IsEquipped(FirefighterEquipmentController controller)
//        {
//            bool isFireGearEquipped = (bool)controller.Memory.GetOrDefault(FireGearEquipment.IsFireGearEquippedMemoryKey, false);

//            if (!isFireGearEquipped)
//                return false;

//            bool isFlashlightOn = (bool)controller.Memory.GetOrDefault(IsFlashlightOnMemoryKey, false);

//            return isFlashlightOn;
//        }

//        void IFirefighterEquipment.OnGetEquipment(FirefighterEquipmentController controller)
//        {
//            bool isFireGearEquipped = (bool)controller.Memory.GetOrDefault(FireGearEquipment.IsFireGearEquippedMemoryKey, false);

//            if (!isFireGearEquipped)
//                return;

//            if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
//            {
//                Ped ped = controller.Ped;
//                foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ON_COMPONENTS)
//                {
//                    ped.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
//                }
//                foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ON_PROPS)
//                {
//                    NativeFunction.Natives.SetPedPropIndex(ped, var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex, true);
//                }
//            }
            
//            controller.Memory.SetOrAdd(IsFlashlightOnMemoryKey, true);
//        }

//        void IFirefighterEquipment.OnLeaveEquipment(FirefighterEquipmentController controller)
//        {
//            bool isFireGearEquipped = (bool)controller.Memory.GetOrDefault(FireGearEquipment.IsFireGearEquippedMemoryKey, false);

//            if (!isFireGearEquipped)
//                return;

//            if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
//            {
//                Ped ped = controller.Ped;
//                foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_OFF_COMPONENTS)
//                {
//                    ped.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
//                }
//                foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ON_PROPS)
//                {
//                    NativeFunction.Natives.ClearPedProp(ped, var.ComponentIndex);
//                }
//            }

//            controller.Memory.SetOrAdd(IsFlashlightOnMemoryKey, false);
//        }

//        void IFirefighterEquipment.OnEquippedUpdate(FirefighterEquipmentController controller)
//        {
//            Ped ped = controller.Ped;
//            Vector3 flashlightPos = ped.GetOffsetPosition(ped.GetPositionOffset(ped.GetBonePosition(Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE)) + Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET.ToVector3());

//            Util.DrawSpotlightWithShadow(flashlightPos, ped.GetBoneRotation(PedBoneId.Spine2).ToVector(), Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_COLOR.ToColor(), 13.25f, 9.25f, 2.0f, 20f, 20.0f);
//        }
//    }
//}

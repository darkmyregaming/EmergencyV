using System.Linq;

namespace EmergencyV
{
    // System
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class ConesEquipment : IFirefighterEquipment
    {
        private const string ConesControllerMemoryKey = "ConesController";

        string IFirefighterEquipment.DisplayName => "Cones";
        bool IFirefighterEquipment.ShouldUpdateIfEquipped => true;

        bool IFirefighterEquipment.IsEquipped(FirefighterEquipmentController controller)
        {
            ConesController c = controller.Memory.GetOrDefault(ConesControllerMemoryKey, null) as ConesController;

            return c != null && c.IsActive;
        }

        void IFirefighterEquipment.OnGetEquipment(FirefighterEquipmentController controller)
        {
            ConesController c = controller.Memory.GetOrDefault(ConesControllerMemoryKey, null) as ConesController;
            if (c == null)
            {
                c = new ConesController(controller);
                controller.Memory.SetOrAdd(ConesControllerMemoryKey, c);
            }

            c.IsActive = true;
        }

        void IFirefighterEquipment.OnLeaveEquipment(FirefighterEquipmentController controller)
        {
            ConesController c = controller.Memory.GetOrDefault(ConesControllerMemoryKey, null) as ConesController;
            if (c == null)
            {
                c = new ConesController(controller);
                controller.Memory.SetOrAdd(ConesControllerMemoryKey, c);
            }

            c.IsActive = false;
        }

        void IFirefighterEquipment.OnEquippedUpdate(FirefighterEquipmentController controller)
        {
            ConesController c = controller.Memory.GetOrDefault(ConesControllerMemoryKey, null) as ConesController;

            c?.OnActiveUpdate();
        }

        public void PlaceCone(FirefighterEquipmentController controller)
        {
            ConesController c = controller.Memory.GetOrDefault(ConesControllerMemoryKey, null) as ConesController;

            if (c != null && c.IsActive)
            {
                c.PlaceCone();
            }
        }

        private class ConesController
        {
            private readonly FirefighterEquipmentController controller;

            private Rage.Object attachedCone;

            private List<Object> spawnedCones;

            private bool isActive = false;

            public bool IsActive
            {
                get { return isActive; }
                set
                {
                    if (value == isActive)
                        return;
                    if (value)
                    {
                        attachedCone = new Rage.Object("prop_mp_cone_02", Vector3.Zero);
                        attachedCone.AttachTo(controller.Ped, controller.Ped.GetBoneIndex(PedBoneId.RightPhHand), new Vector3(-0.05f, 0f, 0f), new Rotator(0f, 0f, 0f));
                        NativeFunction.Natives.SetCurrentPedWeapon(controller.Ped, Game.GetHashKey("WEAPON_UNARMED"), true);
                        controller.Ped.Tasks.PlayAnimation("amb@world_human_aa_coffee@base", "base", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly);

                        if (spawnedCones == null)
                            spawnedCones = new List<Object>();
                    }
                    else
                    {
                        if (attachedCone)
                        {
                            attachedCone.Delete();
                        }

                        controller.Ped.Tasks.ClearSecondary();
                    }

                    isActive = value;
                }
            }

            public ConesController(FirefighterEquipmentController controller)
            {
                this.controller = controller;
            }

            public void OnActiveUpdate()
            {
                if (controller.IsPlayer)
                {
                    Vector3 playerPos = controller.Ped.Position;
                    Object closestCone = spawnedCones.Where(c => Vector3.Distance2D(c.Position, playerPos) < 0.55f).OrderBy(c => Vector3.DistanceSquared(c.Position, playerPos)).FirstOrDefault();

                    if (closestCone)
                    {
                        Game.DisplayHelp("Press ~INPUT_CONTEXT_SECONDARY~ to pickup the cone");

                        if (Game.IsControlJustPressed(0, GameControl.ContextSecondary))
                        {
                            PickupCone(closestCone);
                        }
                    }
                    else
                    {
                        Game.DisplayHelp("Press ~INPUT_CONTEXT~ to place a cone");
                    }

                    if (Game.IsControlJustPressed(0, GameControl.Context))
                    {
                        PlaceCone();
                    }
                }
            }

            public void PlaceCone()
            {
                Vector3 p = controller.Ped.GetOffsetPosition(new Vector3(0f, 0.525f, -0.1225f));
                float? z = World.GetGroundZ(p, true, false);
                if (z.HasValue)
                    p.Z = z.Value;
                
                Object cone = new Object("prop_mp_cone_02", p, controller.Ped.Heading);
                cone.SetPositionWithSnap(p);
                cone.IsPositionFrozen = true;
                cone.CollisionIgnoredEntity = controller.Ped;

                spawnedCones.Add(cone);
            }

            public void PickupCone(Object cone)
            {
                if (cone)
                {
                    if (spawnedCones.Contains(cone))
                    {
                        spawnedCones.Remove(cone);
                        cone.Delete();
                    }
                }
            }
        }
    }
}

namespace EmergencyV.Units.AI.Behaviors.Leafs
{
    // System
    using System.Linq;

    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Leafs;

    internal class IsPlayerExtinguishingFire : Condition
    {
        /// <param name="pedKey">The key where the vehicle to enter is saved in the blackboard's tree memory.</param>
        public IsPlayerExtinguishingFire()
        {
        }

        protected override bool CheckCondition(ref BehaviorTreeContext context)
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Vector3 playerPos = playerPed.Position;
            return (playerPed.Inventory.EquippedWeapon?.Hash == WeaponHash.FireExtinguisher /*|| playerIsUsingHose*/) && World.GetAllFires().Any(f => Vector3.DistanceSquared(f.Position, playerPos) < 20.0f * 20.0f);
        }
    }
}

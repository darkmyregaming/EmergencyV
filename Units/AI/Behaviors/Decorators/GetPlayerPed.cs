namespace EmergencyV.Units.AI.Behaviors.Decorators
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Decorators;

    internal class GetPlayerPed : Service
    {
        /// <param name="key">The key where the player's ped will be saved in the blackboard's tree memory.</param>
        public GetPlayerPed(string key, int interval, BehaviorTask child) : base(interval, (ref BehaviorTreeContext c) => { DoService(key, ref c); } , child)
        {
        }

        /// <param name="key">The key where the player's ped will be saved in the blackboard's tree memory.</param>
        public GetPlayerPed(string key, BehaviorTask child) : base((ref BehaviorTreeContext c) => { DoService(key, ref c); }, child)
        {
        }

        private static void DoService(string key, ref BehaviorTreeContext context)
        {
            Ped p = Game.LocalPlayer.Character;
            context.Agent.Blackboard.Set<Ped>(key, p, context.Tree.Id);
        }
    }
}

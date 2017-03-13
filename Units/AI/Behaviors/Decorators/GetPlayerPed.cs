namespace EmergencyV.Units.AI.Behaviors.Decorators
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Decorators;

    internal class GetPlayerPed : Service
    {
        private readonly BlackboardSetter<Ped> pedSetter;

        /// <param name="key">The key where the player's ped will be saved in the blackboard's tree memory.</param>
        public GetPlayerPed(BlackboardSetter<Ped> pedSetter, int interval, BehaviorTask child) : base(interval, null, child)
        {
            this.pedSetter = pedSetter;

            ServiceMethod = DoService;
        }
        
        /// <param name="pedSetter">Where the player's <see cref="Ped"/> will be saved in the blackboard memory.</param>
        public GetPlayerPed(BlackboardSetter<Ped> pedSetter, BehaviorTask child) : base(null, child)
        {
            this.pedSetter = pedSetter;

            ServiceMethod = DoService;
        }

        private void DoService(ref BehaviorTreeContext context)
        {
            Ped p = Game.LocalPlayer.Character;
            pedSetter.Set(context, this, p);
        }
    }
}

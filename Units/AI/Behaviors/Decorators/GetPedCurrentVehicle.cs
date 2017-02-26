namespace EmergencyV.Units.AI.Behaviors.Decorators
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Decorators;

    internal class GetPedCurrentVehicle : Service
    {
        /// <param name="pedKey">The key where the ped is saved in the blackboard's tree memory.</param>
        /// <param name="key">The key where the ped's current vehicle will be saved in the blackboard's tree memory.</param>
        public GetPedCurrentVehicle(string pedKey, string key, int interval, BehaviorTask child) : base(interval, (ref BehaviorTreeContext c) => { DoService(pedKey, key, ref c); }, child)
        {
        }

        /// <param name="pedKey">The key where the ped is saved in the blackboard's tree memory.</param>
        /// <param name="key">The key where the ped's current vehicle will be saved in the blackboard's tree memory.</param>
        public GetPedCurrentVehicle(string pedKey, string key, BehaviorTask child) : base((ref BehaviorTreeContext c) => { DoService(pedKey, key, ref c); }, child)
        {
        }

        private static void DoService(string pedKey, string key, ref BehaviorTreeContext context)
        {
            Ped ped = context.Agent.Blackboard.Get<Ped>(pedKey, context.Tree.Id);

            if (!ped)
            {
                return;
            }

            Vehicle v = ped.CurrentVehicle;
            context.Agent.Blackboard.Set<Vehicle>(key, v, context.Tree.Id);
        }
    }
}

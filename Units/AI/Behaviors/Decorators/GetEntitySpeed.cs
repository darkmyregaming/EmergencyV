namespace EmergencyV.Units.AI.Behaviors.Decorators
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Decorators;

    internal class GetEntitySpeed : Service
    {
        /// <param name="entityKey">The key where the <see cref="Entity"/> is saved in the blackboard's tree memory.</param>
        /// <param name="key">The key where the speed value will be saved in the blackboard's tree memory.</param>
        public GetEntitySpeed(string entityKey, string key, int interval, BehaviorTask child) : base(interval, (ref BehaviorTreeContext c) => DoService(entityKey, key, ref c), child)
        {
        }

        /// <param name="entityKey">The key where the <see cref="ISpatial"/> is saved in the blackboard's tree memory.</param>
        /// <param name="key">The key where the speed value will be saved in the blackboard's tree memory.</param>
        public GetEntitySpeed(string entityKey, string key, BehaviorTask child) : base((ref BehaviorTreeContext c) => DoService(entityKey, key, ref c), child)
        {
        }

        private static void DoService(string entityKey, string key, ref BehaviorTreeContext context)
        {
            Entity e = context.Agent.Blackboard.Get<Entity>(entityKey, context.Tree.Id);
            context.Agent.Blackboard.Set<float>(key, e.Speed, context.Tree.Id);
        }
    }
}

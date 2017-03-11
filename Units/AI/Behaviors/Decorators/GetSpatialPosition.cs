namespace EmergencyV.Units.AI.Behaviors.Decorators
{
    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Decorators;

    internal class GetSpatialPosition : Service
    {
        /// <param name="spatialKey">The key where the <see cref="ISpatial"/> is saved in the blackboard's tree memory.</param>
        /// <param name="key">The key where the position will be saved in the blackboard's tree memory.</param>
        public GetSpatialPosition(string spatialKey, string key, int interval, BehaviorTask child) : base(interval, (ref BehaviorTreeContext c) => DoService(spatialKey, key, ref c), child)
        {
        }

        /// <param name="spatialKey">The key where the <see cref="ISpatial"/> is saved in the blackboard's tree memory.</param>
        /// <param name="key">The key where the position will be saved in the blackboard's tree memory.</param>
        public GetSpatialPosition(string spatialKey, string key, BehaviorTask child) : base((ref BehaviorTreeContext c) => DoService(spatialKey, key, ref c), child)
        {
        }

        private static void DoService(string spatialKey, string key, ref BehaviorTreeContext context)
        {
            ISpatial p = context.Agent.Blackboard.Get<ISpatial>(spatialKey, context.Tree.Id);
            context.Agent.Blackboard.Set<Vector3>(key, p.Position, context.Tree.Id);
        }
    }
}

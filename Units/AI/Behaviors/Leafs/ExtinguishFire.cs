namespace EmergencyV.Units.AI.Behaviors.Leafs
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;

    // AI
    using RPH.Utilities.AI;
    using RPH.Utilities.AI.Leafs;

    internal class ExtinguishFire : RPH.Utilities.AI.Leafs.Action
    {
        private readonly string positionKey;
        private readonly float range;
        private readonly float rangeSq;

        /// <param name="positionKey">The key where the center position is saved in the blackboard's tree memory.</param>
        public ExtinguishFire(string positionKey, float range)
        {
            this.positionKey = positionKey;
            this.range = range;
            this.rangeSq = range * range;
        }

        protected override void OnOpen(ref BehaviorTreeContext context)
        {
            if (!(context.Agent.Target is Ped))
            {
                throw new InvalidOperationException($"The behavior action {nameof(ExtinguishFire)} can't be used with {context.Agent.Target.GetType().Name}, it can only be used with {nameof(Ped)}s");
            }

            Ped ped = ((Ped)context.Agent.Target);

            if (!ped || ped.IsDead)
            {
                return;
            }

            if (!ped.Inventory.Weapons.Contains(WeaponHash.FireExtinguisher))
                ped.Inventory.GiveNewWeapon(WeaponHash.FireExtinguisher, -1, true);

            GiveTasks(ref context);
        }

        protected override BehaviorStatus OnBehave(ref BehaviorTreeContext context)
        {
            Ped ped = ((Ped)context.Agent.Target);

            if (!ped || ped.IsDead)
            {
                return BehaviorStatus.Failure;
            }

            GiveTasks(ref context);
            
            List<Fire> firesToExtinguish = context.Agent.Blackboard.Get<List<Fire>>("firesToExtinguish", context.Tree.Id, this.Id, null);

            if (firesToExtinguish.Count == 0) // if no fires are found, this task is finished
            {
                return BehaviorStatus.Success;
            }
            else
            {
                return BehaviorStatus.Running;
            }
        }

        protected override void OnClose(ref BehaviorTreeContext context)
        {
            context.Agent.Blackboard.Set<List<Fire>>("firesToExtinguish", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Fire>("closestFire", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Fire>("furthestFire", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Fire>("targetFire", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Task>("fireWeaponAtTargetFireTask", null, context.Tree.Id, this.Id);
            context.Agent.Blackboard.Set<Task>("goToTask", null, context.Tree.Id, this.Id);
        }

        private void GiveTasks(ref BehaviorTreeContext context)
        {
            Ped ped = ((Ped)context.Agent.Target);
            Vector3 position = context.Agent.Blackboard.Get<Vector3>(positionKey, context.Tree.Id);
            List<Fire> firesToExtinguish = context.Agent.Blackboard.Get<List<Fire>>("firesToExtinguish", context.Tree.Id, this.Id, null);
            Fire closestFire = context.Agent.Blackboard.Get<Fire>("closestFire", context.Tree.Id, this.Id, null);
            Fire furthestFire = context.Agent.Blackboard.Get<Fire>("furthestFire", context.Tree.Id, this.Id, null);
            Fire targetFire = context.Agent.Blackboard.Get<Fire>("targetFire", context.Tree.Id, this.Id, null);
            Task fireWeaponAtTargetFireTask = context.Agent.Blackboard.Get<Task>("fireWeaponAtTargetFireTask", context.Tree.Id, this.Id, null);
            Task goToTask = context.Agent.Blackboard.Get<Task>("goToTask", context.Tree.Id, this.Id, null);

            if (firesToExtinguish == null)
            {
                firesToExtinguish = new List<Fire>();
                context.Agent.Blackboard.Set<List<Fire>>("firesToExtinguish", firesToExtinguish, context.Tree.Id, this.Id);
            }

            if (Vector3.DistanceSquared(ped.Position, position) < rangeSq)
            {
                if (firesToExtinguish.Count == 0)
                {
                    foreach (Fire f in World.GetAllFires())
                    {
                        if (Vector3.DistanceSquared(f.Position, position) < rangeSq)
                            firesToExtinguish.Add(f);
                    }

                    if (firesToExtinguish.Count == 0)
                        return;
                }
                else
                {
                    if (!closestFire || !furthestFire)
                    {
                        if (fireWeaponAtTargetFireTask != null && fireWeaponAtTargetFireTask.IsActive)
                        {
                            ped.Tasks.Clear();
                            fireWeaponAtTargetFireTask = null;
                            context.Agent.Blackboard.Set<List<Fire>>("fireWeaponAtTargetFireTask", null, context.Tree.Id, this.Id);
                        }

                        firesToExtinguish.RemoveAll(f => !f.Exists());

                        if (firesToExtinguish.Count >= 1)
                        {
                            IOrderedEnumerable<Fire> orderedFires = firesToExtinguish.OrderBy(f => Vector3.DistanceSquared(f.Position, ped.Position));
                            closestFire = orderedFires.FirstOrDefault();
                            furthestFire = orderedFires.LastOrDefault();

                            context.Agent.Blackboard.Set<Fire>("closestFire", closestFire, context.Tree.Id, this.Id);
                            context.Agent.Blackboard.Set<Fire>("furthestFire", furthestFire, context.Tree.Id, this.Id);

                            targetFire = closestFire;
                            context.Agent.Blackboard.Set<Fire>("targetFire", targetFire, context.Tree.Id, this.Id);
                        }
                    }
                    else
                    {
                        if (Vector3.DistanceSquared(ped.Position, closestFire) > 4.0f * 4.0f)
                        {
                            if (goToTask == null || !goToTask.IsActive)
                            {
                                goToTask = ped.Tasks.FollowNavigationMeshToPosition(targetFire.Position, ped.Position.GetHeadingTowards(targetFire), 2.0f, 2.75f);
                                context.Agent.Blackboard.Set<Task>("goToTask", goToTask, context.Tree.Id, this.Id);
                            }
                        }
                        else
                        {
                            if (fireWeaponAtTargetFireTask == null || !fireWeaponAtTargetFireTask.IsActive)
                            {
                                if ((goToTask != null && goToTask.IsActive))
                                {
                                    ped.Tasks.Clear();
                                    ped.Tasks.ClearSecondary();
                                }

                                fireWeaponAtTargetFireTask = ped.Tasks.FireWeaponAt(targetFire.Position, 7500, FiringPattern.FullAutomatic);
                                context.Agent.Blackboard.Set<Task>("fireWeaponAtTargetFireTask", fireWeaponAtTargetFireTask, context.Tree.Id, this.Id);
                            }
                        }
                    }
                }
            }
            else
            {
                if ((goToTask == null || !goToTask.IsActive))
                {
                    goToTask = ped.Tasks.FollowNavigationMeshToPosition(position.Around2D(range), ped.Position.GetHeadingTowards(position), 2.0f, 5.0f);
                    context.Agent.Blackboard.Set<Task>("goToTask", goToTask, context.Tree.Id, this.Id);
                }
            }
        }
    }
}

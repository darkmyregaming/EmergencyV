namespace EmergencyV
{
    // System
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;

    internal class AIFirefighterTaskExtinguishFireInArea : AIFirefighterTask
    {
        Vector3 position;
        float range;
        float rangeSq;
        List<Fire> firesToExtinguish;
        Fire targetFire;
        Task goToTask;
        Task fireWeaponAtTargetFireTask;

        protected AIFirefighterTaskExtinguishFireInArea(Firefighter firefighter, Vector3 position, float range) : base(firefighter)
        {
            this.position = position;
            this.range = range;
            rangeSq = range * range;
            firesToExtinguish = new List<Fire>();
        }

        public override void Update()
        {
            if (Vector3.DistanceSquared(Ped.Position, position) < rangeSq)
            {
                if (!Firefighter.Equipment.HasFireExtinguisher)
                    Firefighter.Equipment.HasFireExtinguisher = true;

                if (firesToExtinguish.Count == 0)
                {
                    foreach (Fire f in World.GetAllFires())
                    {
                        if (Vector3.DistanceSquared(f.Position, position) < rangeSq)
                            firesToExtinguish.Add(f);
                    }

                    if (firesToExtinguish.Count == 0) // if no fires are found, this task is finished
                    {
                        IsFinished = true;
                        return;
                    }
                }
                else
                {
                    if (!targetFire)
                    {
                        if (fireWeaponAtTargetFireTask != null && fireWeaponAtTargetFireTask.IsActive)
                        {
                            Ped.Tasks.Clear();
                            fireWeaponAtTargetFireTask = null;
                        }

                        firesToExtinguish.RemoveAll(f => !f.Exists());

                        if (firesToExtinguish.Count >= 1)
                        {
                            targetFire = firesToExtinguish.OrderBy(f => Vector3.DistanceSquared(f.Position, Ped.Position)).FirstOrDefault();
                        }
                        else return;
                    }
                    else
                    {
                        if (Vector3.DistanceSquared(Ped.Position, targetFire) > 3.75f * 3.75f)
                        {
                            if (goToTask == null || !goToTask.IsActive)
                            {
                                goToTask = Ped.Tasks.FollowNavigationMeshToPosition(targetFire.Position, Ped.Position.GetHeadingTowards(targetFire), 2.0f, 3.0f);
                            }
                        }
                        else
                        {
                            if (fireWeaponAtTargetFireTask == null || !fireWeaponAtTargetFireTask.IsActive)
                            {
                                fireWeaponAtTargetFireTask = Ped.Tasks.FireWeaponAt(targetFire.Position, 7500, FiringPattern.FullAutomatic);
                            }
                        }
                    }
                }
            }
            else if (goToTask == null || !goToTask.IsActive)
            {
                goToTask = Ped.Tasks.FollowNavigationMeshToPosition(position.Around2D(range), Ped.Position.GetHeadingTowards(position), 2.0f, 5.0f);
            }
        }

        public override void OnFinished()
        {
            Ped.Tasks.Clear();
            firesToExtinguish = null;
            targetFire = null;
            fireWeaponAtTargetFireTask = null;
            goToTask = null;
        }
    }
}

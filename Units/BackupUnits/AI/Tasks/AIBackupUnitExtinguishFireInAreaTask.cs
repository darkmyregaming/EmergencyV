namespace EmergencyV
{
    // System
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class AIBackupUnitExtinguishFireInAreaTask : AIBackupUnitTask
    {
        public override AIBackupUnitTaskPriority Priority { get { return AIBackupUnitTaskPriority.High; } }

        public FirefightersBackupUnit FirefightersUnit { get; }

        Vector3 position;
        float range;
        List<AITask> extinguishFiresTasks;

        protected AIBackupUnitExtinguishFireInAreaTask(BackupUnit unit, Vector3 position, float range) : base(unit)
        {
            FirefightersUnit = unit as FirefightersBackupUnit;
            if (FirefightersUnit == null)
                throw new System.ArgumentException($"The Unit instance isn't a FirefightersBackupUnit instance. The {nameof(AIBackupUnitExtinguishFireInAreaTask)} requires a FirefightersBackupUnit instance.", nameof(unit));

            this.position = position;
            this.range = range;
        }

        protected override void StartInternal()
        {
            if (extinguishFiresTasks == null)
                extinguishFiresTasks = new List<AITask>();

            foreach (Firefighter f in FirefightersUnit.Firefighters)
            {
                f.Ped.Tasks.Clear();
                f.Equipment.SetEquipped<FireExtinguisherEquipment>(true);
                f.Equipment.SetEquipped<FireGearEquipment>(true);
                //f.Equipment.IsFlashlightOn = true;
                extinguishFiresTasks.Add(f.AI.ExtinguishFireInArea(position, range, false));
            }
        }

        protected override void UpdateInternal()
        {
            if (extinguishFiresTasks.All(t => t.IsFinished || !t.Ped || t.Ped.IsDead))
                IsFinished = true;
        }

        protected override void OnFinished(bool aborted)
        {
            foreach (Firefighter f in FirefightersUnit.Firefighters)
            {
                if (f.Ped)
                {
                    f.Ped.Tasks.Clear();

                    f.Equipment.SetEquipped<FireExtinguisherEquipment>(false);
                    f.Equipment.SetEquipped<FireGearEquipment>(false);
                    //f.Equipment.IsFlashlightOn = false;
                }
            }
        }
    }
}

namespace EmergencyV
{
    // System
    using System.Linq;

    // RPH
    using Rage;
    using Rage.Native;

    internal class AIBackupUnitChillAroundTask : AIBackupUnitTask
    {
        public override AIBackupUnitTaskPriority Priority { get { return AIBackupUnitTaskPriority.Low; } }

        uint lastChangeGameTime, nextChangeGameTime;

        protected AIBackupUnitChillAroundTask(BackupUnit unit) : base(unit)
        {
        }

        protected override void StartInternal()
        {
            GiveTasks(true);

            lastChangeGameTime = Game.GameTime;
            nextChangeGameTime = unchecked(lastChangeGameTime + (uint)MathHelper.GetRandomInteger(8500, 30000));
        }

        protected override void UpdateInternal()
        {
            if (Game.GameTime > nextChangeGameTime)
            {
                GiveTasks(false);

                lastChangeGameTime = Game.GameTime;
                nextChangeGameTime = unchecked(lastChangeGameTime + (uint)MathHelper.GetRandomInteger(8500, 30000));
            }
        }

        protected override void OnFinished(bool aborted)
        {
            foreach (AdvancedPed a in Unit.Peds)
            {
                if (a.Ped) a.Ped.Tasks.Clear();
            }
        }

        private void GiveTasks(bool allShouldReceiveTask)
        {
            AdvancedPed chattingPed1 = null, chattingPed2 = null;
            if (Plugin.Random.Next(100) < 70)
            {
                chattingPed1 = MathHelper.Choose(Unit.Peds);
                chattingPed2 = MathHelper.Choose(Unit.Peds.Where(f => f != chattingPed1).ToArray());
                chattingPed1.Ped.Tasks.Clear();
                chattingPed2.Ped.Tasks.Clear();

                Vector3 targetPos = chattingPed1.Ped.GetOffsetPositionFront(1.5f);
                chattingPed2.AI.WalkTo(targetPos, targetPos.GetHeadingTowards(chattingPed1.Ped), 1.0f).Finished += (t, aborted) =>
                {
                    NativeFunction.Natives.TaskChatToPed(chattingPed1.Ped, chattingPed2.Ped, 16, 0f, 0f, 0f, 0f, 0f);
                    NativeFunction.Natives.TaskChatToPed(chattingPed2.Ped, chattingPed1.Ped, 16, 0f, 0f, 0f, 0f, 0f);
                };
            }

            foreach (AdvancedPed a in Unit.Peds)
            {
                if (a != chattingPed1 && a != chattingPed2 && (allShouldReceiveTask || Plugin.Random.Next(101) < Plugin.Random.Next(10, 50)))
                {
                    a.AI.WalkTo((MathHelper.Choose(Unit.Vehicle.FrontPosition, Unit.Vehicle.RearPosition, Unit.Vehicle.RightPosition, Unit.Vehicle.LeftPosition)).Around2D(2.5f, 8f), MathHelper.GetRandomSingle(0f, 360f), 1.0f).Finished += (t, aborted) =>
                    {
                        switch (Plugin.Random.Next(4))
                        {
                            default:
                            case 0: NativeFunction.Natives.TaskUseMobilePhone(t.Ped, 1, MathHelper.Choose(0, 1, 2)); break;
                            case 1: NativeFunction.Natives.TaskStartScenarioInPlace(t.Ped, "WORLD_HUMAN_SMOKING", 0, true); break;
                            case 2: NativeFunction.Natives.TaskStartScenarioInPlace(t.Ped, "WORLD_HUMAN_HANG_OUT_STREET", 0, true); break;
                            case 3: NativeFunction.Natives.TaskStartScenarioInPlace(t.Ped, "WORLD_HUMAN_STAND_IMPATIENT", 0, true); break;
                        }

                    };
                }
            }
        }
    }
}

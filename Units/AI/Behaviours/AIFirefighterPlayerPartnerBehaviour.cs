namespace EmergencyV
{
    // RPH
    using Rage;

    internal class AIFirefighterPlayerPartnerBehaviour : AIBehaviour
    {
        public Firefighter Firefighter { get; }

        uint nextAIUpdateGameTime, lastAIUpdateGameTime;

        AITask followPlayerTask, enterPlayerVehicleTask, leavePlayerVehicleTask;

        protected AIFirefighterPlayerPartnerBehaviour(AIController controller) : base(controller)
        {
            Firefighter = controller.Owner as Firefighter;
            if (Firefighter == null)
                throw new System.ArgumentException("The AIController.Owner instance isn't a Firefighter instance", nameof(controller));
        }
        
        internal override void Update() // TODO: continue working on partners, right now only follows player and enters/leaves its vehicle
        {
            if (Game.GameTime > nextAIUpdateGameTime)
            {
                if (Plugin.LocalPlayer.Character.IsInAnyVehicle(true) )
                {
                    if (!Ped.IsInVehicle(Plugin.LocalPlayer.LastVehicle, true) && (enterPlayerVehicleTask == null || (enterPlayerVehicleTask as AITaskEnterVehicle).Vehicle != Plugin.LocalPlayer.Character.CurrentVehicle))
                    {
                        AborTasks();
                        enterPlayerVehicleTask = Controller.EnterVehicle(Plugin.LocalPlayer.Character.CurrentVehicle, Controller.Owner.PreferedVehicleSeatIndex);
                    }
                }
                else
                {
                    if (leavePlayerVehicleTask == null || leavePlayerVehicleTask.IsFinished)
                    {
                        if (!Plugin.LocalPlayer.Character.IsInAnyVehicle(true) && Ped.IsInAnyVehicle(true))
                        {
                            AborTasks();
                            leavePlayerVehicleTask = Controller.LeaveVehicle(LeaveVehicleFlags.None);
                        }
                        else
                        {
                            if ((followPlayerTask == null || followPlayerTask.IsFinished))
                            {
                                AborTasks();
                                followPlayerTask = Controller.Follow(Plugin.LocalPlayer.Character, new Vector3(0f, -3.25f, 0f), 20f, 4.0f, true);
                            }
                        }
                    }
                }

                lastAIUpdateGameTime = Game.GameTime;
                nextAIUpdateGameTime = lastAIUpdateGameTime + 500;
            }

        }

        private void AborTasks()
        {
            if (Controller.CurrentTask != null && !Controller.CurrentTask.IsFinished)
                Controller.CurrentTask.Abort();
            followPlayerTask = null;
            enterPlayerVehicleTask = null;
            leavePlayerVehicleTask = null;
        }
    }
}

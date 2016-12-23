namespace EmergencyV
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    using Rage;

    internal class MedicActions
    {
        private static MedicActions instance;
        public static MedicActions Instance
        {
            get
            {
                if (instance == null)
                    instance = new MedicActions();
                return instance;
            }
        }

        internal Dictionary<Ped, bool> TreatedPeds = new Dictionary<Ped, bool>();
        
        private Ped Victim { get; set; }

        private bool Reviving { get; set; }
        private bool Abort { get; set; }

        private float Pulse { get; set; }
        private float Rhythm { get; set; } // this should be guarded by a mutex

        public void Update()
        {
            Victim = getClosestDeadPed();

            if (!Victim || Reviving)
                return;
            
            Game.DisplayHelp("Press ~INPUT_CONTEXT~ to revive", 20);

            if (Game.IsControlJustPressed(0, GameControl.Context))
            {
                GameFiber.StartNew(pulseMeterFiber);
                GameFiber.StartNew(rhythmMeterFiber);
                GameFiber.StartNew(cprControlFiber);
                GameFiber.StartNew(cprStatusFiber);
            }
        }

        private void pulseMeterFiber()
        {
            PercentageBar bar = new PercentageBar("PULSE");
            bar.ForegroundColor = Color.Red;
            
            while (!Abort)
            {
                bar.Percentage = Pulse;
                GameFiber.Yield();
            }

            bar.Delete();
        }

        private void rhythmMeterFiber()
        {
            PercentageBar bar = new PercentageBar("RHYTHM", Game.Resolution.Width / 2 - PercentageBar.Width / 2, 50 + PercentageBar.Height + 10);
            bar.ForegroundColor = Color.FromArgb(0x00, 0x6C, 0xFF);
            
            while (!Abort)
            {
                if (Rhythm >= 1.0f)
                    Rhythm = 0f;
                else
                    Rhythm += 0.125f; // 8 increments

                bar.Percentage = Rhythm;

                GameFiber.Sleep(75); // 600ms (100bpm) / 8 increments
            }

            bar.Delete();
        }

        private void cprControlFiber()
        {
            while (!Abort) // TODO: add the ability to abort. maybe require constant key down while performing.
            {
                if (!Game.IsKeyDown(System.Windows.Forms.Keys.F8))
                {
                    GameFiber.Yield();
                    continue;
                }

                Plugin.LocalPlayer.Character.Tasks.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_pumpchest_idle", 1.0f, AnimationFlags.StayInEndFrame);
                Plugin.LocalPlayer.Character.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", 1.0f, AnimationFlags.StayInEndFrame);

                float rhythm = Rhythm;
                if (rhythm >= 0.95f || rhythm <= 0.05f) // if within 10% of the rhythm beat
                    Pulse += 0.1f;
                else
                    Pulse += 0.05f;

                GameFiber.Yield();
            }
        }

        private void cprStatusFiber()
        {
            Plugin.LocalPlayer.Character.Tasks.GoToOffsetFromEntity(Victim, 1.1f, 0.0f, 1.0f).WaitForCompletion(); // NOTE: needs improvement

            Plugin.LocalPlayer.Character.Tasks.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_intro", 1.0f, AnimationFlags.None).WaitForCompletion();
            Plugin.LocalPlayer.Character.Tasks.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_pumpchest_idle", 1.0f, AnimationFlags.StayInEndFrame);

            while (!Abort)
            {
                // TODO: add the possibility they won't make it.
                if (Pulse >= 1.0f)
                {
                    Victim.Resurrect();
                    Victim.BlockPermanentEvents = true;
                    Victim.Tasks.ClearImmediately();
                    Victim.Tasks.StandStill(4000);

                    Plugin.LocalPlayer.Character.Tasks.ClearImmediately();

                    TreatedPeds.Add(Victim, true);

                    Abort = true;
                    Reviving = false;
                    break;
                }

                GameFiber.Yield();
            }
        }

        private Ped getClosestDeadPed()
        {
            Ped victim = null;
            float closestDist = float.MaxValue;
            foreach (Ped p in World.EnumeratePeds()) // find closest dead ped
            {
                if (!p || p.IsPlayer || p.IsAlive || TreatedPeds.ContainsKey(p))
                    continue;
                float dist = Vector3.DistanceSquared(p.Position, Plugin.LocalPlayer.Character.Position);
                if (dist > 4.0f) // 2^2 meters
                    continue;
                if (dist < closestDist)
                {
                    victim = p;
                    closestDist = dist;
                }
            }
            return victim;
        }
    }
}

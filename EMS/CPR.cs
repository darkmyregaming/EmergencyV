namespace EmergencyV
{
    using System;
    using System.Collections.Generic;

    using Rage;
    using Rage.Native;

    // NOTE: credit to alexguirre for the basis of this class.
    internal class CPR
    {
        private enum State
        {
            None,
            Intro,
            Idle,
            Pump,
            Success,
            Failure,
        }
        private State _state;
        private State state
        {
            get { return _state; }
            set
            {
                State old = _state;
                _state = value;
                StateChange.Invoke(this, new StateChangeArgs(old, _state));
            }
        }

        private event EventHandler<StateChangeArgs> StateChange;

        private sealed class StateChangeArgs : EventArgs
        {
            internal State From { get; }
            internal State To { get; }

            public StateChangeArgs(State from, State to)
            {
                From = from;
                To = to;
            }
        }
        
        internal Ped Patient { get; }
        internal Ped Administrant { get; }

        internal bool IsPerforming { get { return state != State.None; } }
        internal bool IsFinished { get { return state == State.Success || state == State.Failure; } }
        internal bool WasSuccessful { get { return state == State.Success; } }

        internal int RequiredPumps { get; set; }
        internal int Pumps { get; private set; }

        private AnimationTask playerTask = null;
        private AnimationTask victimTask = null;

        private bool halting = true;

        public CPR(Ped patient, Ped administrant)
        {
            Patient = patient;
            Administrant = administrant;

            RequiredPumps = getRequiredPumps(Patient);

            StateChange += OnStateChange;
        }

        public void Start()
        {
            halting = false;
        }

        public void Update()
        {
            if (halting)
                return;

            if (IsPerforming)
                goto perform;

            if (!Patient || !Administrant)
                return;
            
            Patient.IsPositionFrozen = true;
            Patient.BlockPermanentEvents = true;
            Patient.CanPlayAmbientAnimations = false;
            Patient.CanPlayGestureAnimations = false;
            Patient.CanPlayVisemeAnimations = false;
            Patient.CollisionIgnoredEntity = Plugin.LocalPlayerCharacter;
            Plugin.LocalPlayerCharacter.CollisionIgnoredEntity = Patient;

            NativeFunction.Natives.SetFacialIdleAnimOverride(Patient, "dead_1", 0); // close the eyes of the victim
            NativeFunction.Natives.StopPedSpeaking(Patient, true);

            Patient.Resurrect();
            Patient.Tasks.ClearImmediately();
            
            Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);

            Pumps = 0;
            state = State.Intro;
perform:
            switch (state)
            {
            case State.Intro:
                if (animationAlmostFinished(playerTask))
                    state = State.Idle;
                break;
            case State.Idle:
                if (Game.IsKeyDown(System.Windows.Forms.Keys.Space))
                    state = State.Pump;
                if (Game.IsKeyDown(System.Windows.Forms.Keys.J))
                    state = State.Failure;
                break;
            case State.Pump:
                if (animationAlmostFinished(playerTask))
                {
                    if (RequiredPumps > 0 && Pumps >= RequiredPumps)
                    {
                        state = State.Success;
                        goto case State.Success;
                    }
                    state = State.Idle;
                }
                break;
            case State.Success:
            case State.Failure:
                if (animationAlmostFinished(playerTask))
                {
                    Administrant.Tasks.Clear();

                    if (Patient)
                    {
                        NativeFunction.Natives.SetEntityNoCollisionEntity(Patient, Plugin.LocalPlayerCharacter, true);
                        NativeFunction.Natives.SetEntityNoCollisionEntity(Plugin.LocalPlayerCharacter, Patient, true);
                        NativeFunction.Natives.StopPedSpeaking(Patient, false);
                        NativeFunction.Natives.ClearFacialIdleAnimOverride(Patient);
                        Patient.IsPositionFrozen = false;
                        Patient.IsCollisionEnabled = true; // request collision, otherwise the ped falls through the ground
                        Patient.NeedsCollision = true;
                        Patient.Tasks.Clear();

                        if (state == State.Failure)
                            Patient.Kill();
                    }

                    Pumps = 0;
                    state = State.None;
                }
                break;
            }
        }

        private bool animationAlmostFinished(AnimationTask t)
        {
            return t != null && t.CurrentTimeRatio > 0.95f;
        }

        private static int getRequiredPumps(Ped p)
        {
            if (!p)
                return 0;

            int deathTime = NativeFunction.CallByHash<int>(0x1E98817B311AE98A, p); // _GET_PED_TIME_OF_DEATH

            if (deathTime > 0)
            {
                long delta = Game.GameTime - deathTime;
                long max = MathHelper.GetRandomInteger(20, 120) * (long) 1e3;

                Game.LogTrivialDebug("[CPR] Time since death: " + delta);
                Game.LogTrivialDebug("[CPR] Max allowable time: " + max);

                if (delta > max) // too late
                    return 0;
            }
            else
            {
                Game.LogTrivialDebug("[CPR] Couldn't get time of death. 75% chance it's too late.");
                if (Plugin.Random.NextDouble() >= 0.25f) // 3:4 chance too late.
                    return 0;
            }

            Game.LogTrivialDebug("[CPR] Not too late. Getting chance of survival...");

            // TODO: replace the random chance with a weighted chance based on injuries.
            double chance = 0f;
            switch (MathHelper.GetRandomInteger(0, 3))
            {
            case 1: chance = 0.50f; break;
            case 2: chance = 0.25f; break;
            case 3: chance = 0.10f; break;
            }

            Game.LogTrivialDebug("[CPR] Chance of survival: " + chance);

            return Plugin.Random.NextDouble() < chance ? MathHelper.GetRandomInteger(5, 15) : 0;
        }

        private void OnStateChange(object sender, StateChangeArgs args)
        {
            switch (args.To)
            {
            case State.Intro:
                Vector3 side = Patient.GetOffsetPosition(new Vector3(-0.99f, -0.01f, 0f));
                float h = Patient.Heading;
                Task t = Administrant.Tasks.GoStraightToPosition(side, 1.0f, side.GetHeadingTowards(Patient), 0.1225f, -1);
                while (t.IsActive) // wait for completion
                {
                    GameFiber.Yield();
                    // sometimes the victim gets up while the player is completing the task, this makes him play the anim again
                    if (!NativeFunction.Natives.IsEntityPlayingAnim<bool>(Patient, "mini@cpr@char_b@cpr_str", "cpr_pumpchest_idle", 3))
                    {
                        Patient.Heading = h; // set the victim's heading to the original heading in case he turned around
                        Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);
                    }
                }

                victimTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_intro", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                playerTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_intro", -1, 4.0f, -8.0f, 0, AnimationFlags.None);

                Game.DisplayHelp("Press " + System.Windows.Forms.Keys.J + " to delare victim dead.", 2000);
                break;
            case State.Idle:
                Game.DisplayHelp("Press " + System.Windows.Forms.Keys.Space + " to pump their chest.", 2000);
                playerTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);
                victimTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);
                break;
            case State.Pump:
                playerTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                victimTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                Pumps++;
                break;
            case State.Success:
                playerTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_success", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                victimTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_success", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                NativeFunction.Natives.ClearFacialIdleAnimOverride(Patient);
                NativeFunction.Natives.SetFacialIdleAnimOverride(Patient, "mood_Happy_1", 0);
                break;
            case State.Failure:
                playerTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_fail", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                victimTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_fail", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                break;
            }
        }

        
    }
}

namespace EmergencyV
{
    using System;

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
        internal bool IsFinished { get; private set; }
        internal bool WasSuccessful { get { return state == State.Success; } }

        internal int RequiredPumps { get; set; }
        internal int Pumps { get; private set; }

        internal int MaxPumps { get; set; } = -1;

        internal bool ShouldShowDeathReportWhenFinished { get; set; } = true;

        private AnimationTask adminTask = null;
        private AnimationTask patientTask = null;

        private bool halting = true;

        private Task adminGoingToPatientTask;
        private Vector3 patientSide;
        private float patientHeading;

        public CPR(Ped patient, Ped administrant)
        {
            Patient = patient;
            Administrant = administrant;

            RequiredPumps = getRequiredPumps(Patient);
            if (!Patient.IsLocalPlayer)
                MaxPumps = getMaxPumps(RequiredPumps);

            StateChange += OnStateChange;
        }

        public void Start()
        {
            halting = false;
        }

        public void Update()
        {
            if (halting || IsFinished)
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
            Patient.CollisionIgnoredEntity = Administrant;
            Administrant.CollisionIgnoredEntity = Patient;

            NativeFunction.Natives.SetFacialIdleAnimOverride(Patient, "dead_1", 0); // close the eyes of the victim
            NativeFunction.Natives.StopPedSpeaking(Patient, true);

            DeathManager.Instance.GetCause(Patient); // get the cause of death before resurrecting the ped, otherwise the death report always shows "unknown"

            Patient.Resurrect();
            Patient.Tasks.ClearImmediately();
            
            Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);

            patientSide = Patient.GetOffsetPosition(new Vector3(-0.99f, -0.01f, 0f));
            patientHeading = Patient.Heading;

            Pumps = 0;
            state = State.Intro;
perform:
            switch (state)
            {
            case State.Intro:
                if (adminGoingToPatientTask == null)
                {
                    adminGoingToPatientTask = Administrant.Tasks.GoStraightToPosition(patientSide, 1.0f, patientSide.GetHeadingTowards(Patient), 0.1225f, -1);
                }
                else if (adminGoingToPatientTask.IsActive)
                {
                    // sometimes the victim gets up while the player is completing the task, this makes him play the anim again
                    if (!NativeFunction.Natives.IsEntityPlayingAnim<bool>(Patient, "mini@cpr@char_b@cpr_str", "cpr_pumpchest_idle", 3))
                    {
                        Patient.Heading = patientHeading; // set the victim's heading to the original heading in case he turned around
                        Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);
                    }
                }
                else if(adminTask == null || patientTask == null)
                {
                    patientTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_intro", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                    adminTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_intro", -1, 4.0f, -8.0f, 0, AnimationFlags.None);

                    if (Administrant.IsLocalPlayer) Game.DisplayHelp("Press " + System.Windows.Forms.Keys.J + " to delare victim dead.", 2000);
                }


                if (animationAlmostFinished(adminTask))
                    state = State.Idle;
                break;
            case State.Idle:
                if (Administrant.IsLocalPlayer)
                {
                    if (Game.IsKeyDown(System.Windows.Forms.Keys.Space))
                        state = State.Pump;
                    if (Game.IsKeyDown(System.Windows.Forms.Keys.J))
                        state = State.Failure;
                    break;
                }
                if (Pumps >= MaxPumps)
                {
                    state = State.Failure;
                    break;
                }
                state = State.Pump;
                break;
            case State.Pump:
                if (animationAlmostFinished(adminTask))
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
                if (animationAlmostFinished(adminTask))
                {
                    Administrant.Tasks.Clear();
                    if (Patient)
                    {
                        NativeFunction.Natives.SetEntityNoCollisionEntity(Patient, Administrant, true);
                        NativeFunction.Natives.SetEntityNoCollisionEntity(Administrant, Patient, true);
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
                    IsFinished = true;
                }
                break;
            }
        }

        private bool animationAlmostFinished(AnimationTask t)
        {
            return t != null && t.CurrentTimeRatio > 0.95f;
        }

        private static int getMaxPumps(int required)
        {
            return required + MathHelper.GetRandomInteger(required == 0 ? 5 : 0, 15);
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
                break;
            case State.Idle:
                if (Administrant.IsLocalPlayer) Game.DisplayHelp("Press " + System.Windows.Forms.Keys.Space + " to pump their chest.", 2000);
                adminTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);
                patientTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);
                break;
            case State.Pump:
                adminTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                patientTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                Pumps++;
                break;
            case State.Success:
                adminTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_success", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                patientTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_success", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                NativeFunction.Natives.ClearFacialIdleAnimOverride(Patient);
                NativeFunction.Natives.SetFacialIdleAnimOverride(Patient, "mood_Happy_1", 0);
                break;
            case State.Failure:
                adminTask = Administrant.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_fail", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                patientTask = Patient.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_fail", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                break;
            }
        }

        
    }
}

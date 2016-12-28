namespace EmergencyV
{
    using System;
    using System.Collections.Generic;

    using Rage;
    using Rage.Native;

    // NOTE: credit to alexguirre for the basis of this class.
    internal class CPR
    {
        private static CPR instance;
        public static CPR Instance
        {
            get
            {
                if (instance == null)
                    instance = new CPR();
                return instance;
            }
        }

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
            private State From { get; }
            private State To { get; }

            public StateChangeArgs(State from, State to)
            {
                From = from;
                To = to;
            }
        }

        private float lookupRangeSq = 3.0625f; // 1.75m
        public float LookupRange
        {
            get { return (float) Math.Sqrt(lookupRangeSq); }
            set { lookupRangeSq = value * value; }
        }

        internal Dictionary<Ped, bool> TreatedPeds = new Dictionary<Ped, bool>();
        
        private Ped Victim { get; set; }

        private bool Reviving { get { return state != State.None; } }

        private int pumpsRequired;
        private int pumpCount;

        private AnimationTask playerTask = null;
        private AnimationTask victimTask = null;

        public CPR()
        {
            StateChange += OnStateChange;
        }

        public void Update()
        {
            if (Reviving)
                goto revive;

            Victim = getClosestDeadPed();

            if (!Victim)
                return;
            
            Game.DisplayHelp("Press ~INPUT_CONTEXT~ to attempt CPR", 20);

            if (!Game.IsControlJustPressed(0, GameControl.Context))
                return;
            
            pumpsRequired = getRequiredPumps(Victim);

            Victim.IsPositionFrozen = true;
            Victim.BlockPermanentEvents = true;
            Victim.CanPlayAmbientAnimations = false;
            Victim.CanPlayGestureAnimations = false;
            Victim.CanPlayVisemeAnimations = false;
            Victim.CollisionIgnoredEntity = Plugin.LocalPlayerCharacter;
            Plugin.LocalPlayerCharacter.CollisionIgnoredEntity = Victim;

            NativeFunction.Natives.SetFacialIdleAnimOverride(Victim, "dead_1", 0); // close the eyes of the victim
            NativeFunction.Natives.StopPedSpeaking(Victim, true);

            Victim.Resurrect();
            Victim.Tasks.ClearImmediately();
            
            Victim.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);

            state = State.Intro;
revive:
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
                    if (pumpsRequired > 0 && pumpCount >= pumpsRequired)
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
                    Game.LocalPlayer.Character.Tasks.Clear();

                    if (Victim)
                    {
                        NativeFunction.Natives.SetEntityNoCollisionEntity(Victim, Plugin.LocalPlayerCharacter, true);
                        NativeFunction.Natives.SetEntityNoCollisionEntity(Plugin.LocalPlayerCharacter, Victim, true);
                        NativeFunction.Natives.StopPedSpeaking(Victim, false);
                        NativeFunction.Natives.ClearFacialIdleAnimOverride(Victim);
                        Victim.IsPositionFrozen = false;
                        Victim.IsCollisionEnabled = true; // request collision, otherwise the ped falls through the ground
                        Victim.NeedsCollision = true;
                        Victim.Tasks.Clear();

                        if (!TreatedPeds.ContainsKey(Victim))
                            TreatedPeds.Add(Victim, state == State.Success);

                        if (state == State.Failure)
                            Victim.Kill();
                    }

                    pumpCount = 0;
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
            switch (state)
            {
            case State.Intro:
                Vector3 side = Victim.GetOffsetPosition(new Vector3(-0.99f, -0.01f, 0f));
                float h = Victim.Heading;
                Task t = Game.LocalPlayer.Character.Tasks.GoStraightToPosition(side, 1.0f, side.GetHeadingTowards(Victim), 0.1225f, -1);
                while (t.IsActive) // wait for completion
                {
                    GameFiber.Yield();
                    // sometimes the victim gets up while the player is completing the task, this makes him play the anim again
                    if (!NativeFunction.Natives.IsEntityPlayingAnim<bool>(Victim, "mini@cpr@char_b@cpr_str", "cpr_pumpchest_idle", 3))
                    {
                        Victim.Heading = h; // set the victim's heading to the original heading in case he turned around
                        Victim.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);
                    }
                }

                victimTask = Victim.Tasks.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_intro", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                playerTask = Game.LocalPlayer.Character.Tasks.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_intro", -1, 4.0f, -8.0f, 0, AnimationFlags.None);

                Game.DisplayHelp("Press " + System.Windows.Forms.Keys.J + " to delare victim dead.", 2000);
                break;
            case State.Idle:
                Game.DisplayHelp("Press " + System.Windows.Forms.Keys.Space + " to pump their chest.", 2000);
                playerTask = Game.LocalPlayer.Character.Tasks.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);
                victimTask = Victim.Tasks.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_pumpchest_idle", -1, 4.0f, -8.0f, 0, AnimationFlags.Loop);
                break;
            case State.Pump:
                playerTask = Game.LocalPlayer.Character.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                victimTask = Victim.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                pumpCount++;
                break;
            case State.Success:
                playerTask = Game.LocalPlayer.Character.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_success", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                victimTask = Victim.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_success", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                NativeFunction.Natives.ClearFacialIdleAnimOverride(Victim);
                NativeFunction.Natives.SetFacialIdleAnimOverride(Victim, "mood_Happy_1", 0);
                break;
            case State.Failure:
                playerTask = Game.LocalPlayer.Character.Tasks.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_fail", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                victimTask = Victim.Tasks.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_fail", -1, 4.0f, -8.0f, 0, AnimationFlags.None);
                break;
            }
        }

        private Ped getClosestDeadPed()
        {
            Ped victim = null;
            float closestDist = float.MaxValue;
            foreach (Ped p in World.EnumeratePeds())
            {
                if (!p || p.IsPlayer || !p.IsHuman || p.IsAlive || TreatedPeds.ContainsKey(p))
                    continue;
                float dist = Vector3.DistanceSquared(p.Position, Plugin.LocalPlayer.Character.Position);
                if (dist > lookupRangeSq)
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

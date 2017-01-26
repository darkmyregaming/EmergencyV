namespace EmergencyV.API
{
    // System
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    public class FireEx
    {
        public uint NativeHandle { get; }
        public Fire Fire { get; }

        internal FireEx(uint nativeHandle, Fire fire)
        {
            NativeHandle = nativeHandle;
            Fire = fire;

            RegisterFireEx(this);
        }

        protected virtual void Remove()
        {
            NativeFunction.Natives.RemoveScriptFire(NativeHandle);
        }


        private static void RegisterFireEx(FireEx f)
        {
            if (!UpdateInstancesFibersManager.Instance.IsUpdateDataSetForType<FireEx>())
            {
                UpdateInstancesFibersManager.Instance.SetUpdateDataForType<FireEx>(
                    canDoUpdateCallback: (p) => p.Fire,
                    onInstanceUpdateCallback: null,
                    onInstanceUnregisteredCallback: (p) => p.Remove(),
                    updatesInterval: 250);
            }

            UpdateInstancesFibersManager.Instance.RegisterInstance(f);
        }
    }
}

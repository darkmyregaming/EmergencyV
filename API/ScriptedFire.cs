namespace EmergencyV.API
{
    // RPH
    using Rage;
    using Rage.Native;

    public class ScriptedFire
    {
        public uint NativeHandle { get; }
        public Fire Fire { get; }

        internal ScriptedFire(uint nativeHandle, Fire fire)
        {
            NativeHandle = nativeHandle;
            Fire = fire;
        }

        public void Remove()
        {
            NativeFunction.Natives.RemoveScriptFire(NativeHandle);
        }
    }
}

namespace EmergencyV.API
{
    public abstract class Addon
    {
        public abstract string Name { get; }

        protected Addon()
        {
        }

        public abstract void OnStart();
        public abstract void OnCleanUp();
    }
}

namespace EmergencyV.API
{
    public abstract class Addon
    {
        protected Addon()
        {
        }

        public abstract void OnStart();
        public abstract void OnCleanUp();
    }
}

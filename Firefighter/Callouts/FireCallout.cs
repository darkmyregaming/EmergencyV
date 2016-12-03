namespace EmergencyV
{
    public abstract class FireCallout
    {
        public virtual string DisplayName { get; set; } = "[DISPLAY NAME NOT SET]";

        public abstract void ExecuteSomething();
    }
}

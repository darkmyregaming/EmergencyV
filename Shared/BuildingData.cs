namespace EmergencyV
{
    // System
    using System.Runtime.Serialization;

    using static Settings;

    [DataContract(Name = "Building", Namespace = "EmergencyV")]
    internal abstract class BuildingData
    {
        [DataMember(Order = 0)]
        public string Name;

        [DataMember(Order = 1)]
        public XYZ Entrance;

        [DataMember(Order = 2)]
        public float ActivationRange;
    }
}

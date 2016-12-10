namespace EmergencyV
{
    // RPH
    using Rage;

    internal abstract class BuildingsManager<TBuilding, TBuildingData> where TBuilding : Building<TBuildingData>
                                                                       where TBuildingData : BuildingData
    {
        public readonly TBuilding[] Buildings;
        
        protected BuildingsManager()
        {
            Buildings = LoadBuildings();
            
            for (int i = 0; i < Buildings.Length; i++)
            {
                Buildings[i].PlayerEntered += OnPlayerEnteredBuildingPrivate;
            }
        }

        public virtual void Update()
        {
            Vector3 playerPos = Plugin.LocalPlayerCharacter.Position;
            for (int i = 0; i < Buildings.Length; i++)
            {
                TBuilding building = Buildings[i];

                if (building.IsInActivationRangeFrom(playerPos))
                {
                    if (!building.IsCreated)
                        building.Create();
                }
                else if (building.IsCreated)
                {
                    building.Delete();
                }

                building.Update();
            }
        }

        protected virtual void OnPlayerEnteredBuilding(TBuilding building)
        {
        }

        private void OnPlayerEnteredBuildingPrivate(Building<TBuildingData> building)
        {
            Game.LogTrivial("Player Entered " + building);
            OnPlayerEnteredBuilding((TBuilding)building);
        }

        public virtual void CleanUp(bool isTerminating)
        {
            if (!isTerminating)
            {
                foreach (TBuilding b in Buildings)
                    b.CleanUp();
            }
        }

        protected abstract TBuilding[] LoadBuildings();
    }
}

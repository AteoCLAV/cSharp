namespace Traffic.Models
{
    public class EmergencyService : IEmergencyService
    {
        public void ArriveAtCrosswalk(CrosswalkModel crosswalk)
        {
            crosswalk.SpawnEmergencyCar();
        }
    }
}


using CommunityToolkit.Mvvm.ComponentModel;

namespace Traffic.Models
{
    public partial class Pedestrian : ObservableObject
    {
        [ObservableProperty]
        private double positionX;

        [ObservableProperty]
        private double positionY;

        [ObservableProperty]
        private double speed;

        [ObservableProperty]
        private bool isCrossing;

        [ObservableProperty]
        private bool isAccidentParticipant;
    }
}


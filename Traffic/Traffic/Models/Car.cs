using CommunityToolkit.Mvvm.ComponentModel;

namespace Traffic.Models
{
    public partial class Car : ObservableObject
    {
        [ObservableProperty]
        private double positionX;

        [ObservableProperty]
        private double positionY;

        [ObservableProperty]
        private double speed;

        [ObservableProperty]
        private bool isEmergency;

        [ObservableProperty]
        private bool isAccidentParticipant;
    }
}


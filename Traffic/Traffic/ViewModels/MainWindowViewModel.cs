using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Traffic.Models;

namespace Traffic.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public ObservableCollection<CrosswalkViewModel> Crosswalks { get; } = new();

        public MainWindowViewModel()
        {
            AddCrosswalk();
        }

        [RelayCommand]
        private void AddCrosswalk()
        {
            var model = new CrosswalkModel(
                emergencyProbability: 0.008,
                tickIntervalMs: 120,
                carPhaseTicks: 34,
                pedestrianPhaseTicks: 28);

            model.Cars.Add(new Car { PositionX = 40, PositionY = 82, Speed = 4.2 });
            model.Cars.Add(new Car { PositionX = 140, PositionY = 82, Speed = 3.8 });
            model.Cars.Add(new Car { PositionX = 260, PositionY = 82, Speed = 4.5 });

            model.Pedestrians.Add(new Pedestrian { PositionX = 356, PositionY = 6, Speed = 4.3 });
            model.Pedestrians.Add(new Pedestrian { PositionX = 384, PositionY = 6, Speed = 3.9 });
            model.Pedestrians.Add(new Pedestrian { PositionX = 412, PositionY = 6, Speed = 4.1 });

            Crosswalks.Add(new CrosswalkViewModel(model));
        }

        public void Dispose()
        {
            foreach (var vm in Crosswalks)
            {
                vm.Dispose();
            }
        }
    }
}

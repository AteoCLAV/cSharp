using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Traffic.Models;

namespace Traffic.ViewModels
{
    public partial class CrosswalkViewModel : ViewModelBase, IDisposable
    {
        public CrosswalkModel Model { get; }

        public ObservableCollection<Car> Cars => Model.Cars;
        public ObservableCollection<Car> EmergencyCars => Model.EmergencyCars;
        public ObservableCollection<Pedestrian> Pedestrians => Model.Pedestrians;

        [ObservableProperty] private TrafficLightState _trafficLightState;
        [ObservableProperty] private string _statusText = string.Empty;
        [ObservableProperty] private string _emergencyActiveText = string.Empty;
        [ObservableProperty] private bool _isAccidentActive;
        [ObservableProperty] private double _canvasWidth;
        [ObservableProperty] private double _canvasHeight;

        private readonly CancellationTokenSource _cts = new();
        private readonly Task _simulationTask;

        public CrosswalkViewModel(CrosswalkModel model)
        {
            Model = model;

            _trafficLightState = model.TrafficLight.State;
            _statusText = "Симуляция запущена.";
            _canvasWidth = model.CanvasWidth;
            _canvasHeight = model.CanvasHeight;

            Model.TrafficLight.GreenForPedestrians += OnGreenForPedestrians;
            Model.TrafficLight.RedForPedestrians += OnRedForPedestrians;
            Model.EmergencyOccured += OnEmergencyOccured;
            Model.EmergencyArrived += OnEmergencyArrived;

            _simulationTask = Task.Run(() => RunSimulationAsync(_cts.Token), _cts.Token);
        }

        private void OnGreenForPedestrians(object? sender, EventArgs e)
        {
            TrafficLightState = Model.TrafficLight.State;
            StatusText = "Зелёный свет для пешеходов.";
        }

        private void OnRedForPedestrians(object? sender, EventArgs e)
        {
            TrafficLightState = Model.TrafficLight.State;
            StatusText = "Красный свет для пешеходов.";
        }

        private void OnEmergencyOccured(object? sender, EventArgs e)
        {
            StatusText = "ДТП на переходе: машина сбила пешехода. Вызвана аварийная служба.";
            EmergencyActiveText = "Аварийная служба активна";
            IsAccidentActive = true;
        }

        private void OnEmergencyArrived(object? sender, EventArgs e)
        {
            StatusText = "Аварийная служба прибыла и освободила переход.";
            EmergencyActiveText = string.Empty;
            IsAccidentActive = false;
        }

        private async Task RunSimulationAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Dispatcher.UIThread.Post(() => Model.ApplyTick());

                await Task.Delay(Model.TickIntervalMs, token);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();

            try
            {
                _simulationTask.Wait(500);
            }
            catch
            {
            }

            _cts.Dispose();
        }
    }
}


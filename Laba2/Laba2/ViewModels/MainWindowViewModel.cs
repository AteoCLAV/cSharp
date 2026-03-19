using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Laba2.Models;

namespace Laba2.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FlyingVehicle> _vehicles;
        private FlyingVehicle _selectedVehicle;
        private string _newVehicleName;
        private bool _isAirplaneSelected;
        private string _runwayLength;
        private string _statusMessage;

        public MainWindowViewModel()
        {
            // Сначала инициализируем коллекции
            _vehicles = new ObservableCollection<FlyingVehicle>();
            FlightEvents = new ObservableCollection<string>();

            // Инициализируем команды ПЕРВЫМИ (до установки свойств)
            CreateCommand = new RelayCommand(CreateVehicle, CanCreateVehicle);
            DeleteCommand = new RelayCommand(DeleteVehicle, (param) => SelectedVehicle != null);
            TakeOffCommand = new RelayCommand(TakeOff, (param) => SelectedVehicle != null && !SelectedVehicle.IsFlying);
            LandCommand = new RelayCommand(Land, (param) => SelectedVehicle != null && SelectedVehicle.IsFlying);
            ClearEventsCommand = new RelayCommand(ClearEvents);

            // Потом устанавливаем свойства
            NewVehicleName = "Boeing 737";
            IsAirplaneSelected = true;
            RunwayLength = "800";

            StatusMessage = "Создайте летательное средство";
        }

        public ObservableCollection<FlyingVehicle> Vehicles
        {
            get => _vehicles;
            set
            {
                _vehicles = value;
                OnPropertyChanged();
            }
        }

        public FlyingVehicle SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (_selectedVehicle != value)
                {
                    _selectedVehicle = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedVehicleName));
                    OnPropertyChanged(nameof(SelectedVehicleType));
                    OnPropertyChanged(nameof(SelectedVehicleAltitude));
                    OnPropertyChanged(nameof(SelectedVehicleStatus));
                    OnPropertyChanged(nameof(SelectedVehicleStatusColor));

                    // Обновляем состояние команд
                    UpdateCommands();
                }
            }
        }

        public ObservableCollection<string> FlightEvents { get; }

        public string NewVehicleName
        {
            get => _newVehicleName;
            set
            {
                _newVehicleName = value;
                OnPropertyChanged();
                CreateCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool IsAirplaneSelected
        {
            get => _isAirplaneSelected;
            set
            {
                if (_isAirplaneSelected != value)
                {
                    _isAirplaneSelected = value;
                    OnPropertyChanged();
                    CreateCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string RunwayLength
        {
            get => _runwayLength;
            set
            {
                _runwayLength = value;
                OnPropertyChanged();
                CreateCommand?.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsAirplaneSelectedForVisibility => IsAirplaneSelected;

        public string SelectedVehicleName => SelectedVehicle?.Name ?? "не выбрано";

        public string SelectedVehicleType
        {
            get
            {
                if (SelectedVehicle is Airplane)
                    return "Самолет";
                else if (SelectedVehicle is Helicopter)
                    return "Вертолет";
                else
                    return "не выбрано";
            }
        }

        public double SelectedVehicleAltitude => SelectedVehicle?.Altitude ?? 0;

        public string SelectedVehicleStatus => SelectedVehicle?.IsFlying == true ? "В ПОЛЕТЕ" : "НА ЗЕМЛЕ";

        public string SelectedVehicleStatusColor => SelectedVehicle?.IsFlying == true ? "Green" : "Gray";

        public RelayCommand CreateCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand TakeOffCommand { get; }
        public RelayCommand LandCommand { get; }
        public RelayCommand ClearEventsCommand { get; }

        private bool CanCreateVehicle(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewVehicleName))
                return false;

            if (IsAirplaneSelected)
            {
                if (!double.TryParse(RunwayLength, out double length) || length <= 0)
                    return false;
            }

            return true;
        }

        private void CreateVehicle(object parameter)
        {
            try
            {
                FlyingVehicle newVehicle;

                if (IsAirplaneSelected)
                {
                    double length = double.Parse(RunwayLength);
                    newVehicle = new Airplane(NewVehicleName, length);
                    AddEvent($"Создан самолет {NewVehicleName} с длиной полосы {length} м");
                    StatusMessage = $"Создан самолет {NewVehicleName}";
                }
                else
                {
                    newVehicle = new Helicopter(NewVehicleName);
                    AddEvent($"Создан вертолет {NewVehicleName}");
                    StatusMessage = $"Создан вертолет {NewVehicleName}";
                }

                // Подписываемся на события
                newVehicle.TookOff += OnFlightEvent;
                newVehicle.Landed += OnFlightEvent;
                newVehicle.AltitudeChanged += OnAltitudeChanged;

                Vehicles.Add(newVehicle);
                SelectedVehicle = newVehicle;

                NewVehicleName = "";

                // Обновляем команды
                UpdateCommands();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }

        private void DeleteVehicle(object parameter)
        {
            if (SelectedVehicle != null)
            {
                string name = SelectedVehicle.Name;

                // Отписываемся от событий
                SelectedVehicle.TookOff -= OnFlightEvent;
                SelectedVehicle.Landed -= OnFlightEvent;
                SelectedVehicle.AltitudeChanged -= OnAltitudeChanged;

                Vehicles.Remove(SelectedVehicle);
                SelectedVehicle = Vehicles.FirstOrDefault();
                AddEvent($"Удален {name}");
                StatusMessage = $"Удален {name}";

                UpdateCommands();
            }
        }

        private void TakeOff(object parameter)
        {
            if (SelectedVehicle != null)
            {
                SelectedVehicle.TakeOff();
            }
        }

        private void Land(object parameter)
        {
            if (SelectedVehicle != null)
            {
                SelectedVehicle.Land();
            }
        }

        private void UpdateSelectedVehicleInfo()
        {
            OnPropertyChanged(nameof(SelectedVehicleAltitude));
            OnPropertyChanged(nameof(SelectedVehicleStatus));
            OnPropertyChanged(nameof(SelectedVehicleStatusColor));
        }

        private void OnFlightEvent(object sender, FlightEventArgs e)
        {
            AddEvent(e.Message);
            if (SelectedVehicle == sender)
            {
                UpdateSelectedVehicleInfo();
            }
            UpdateCommands();
        }

        private void OnAltitudeChanged(object sender, AltitudeChangedEventArgs e)
        {
            AddEvent($"{e.VehicleName}: высота {e.OldAltitude} -> {e.NewAltitude} м");
            if (SelectedVehicle == sender)
            {
                UpdateSelectedVehicleInfo();
            }
        }

        private void AddEvent(string message)
        {
            FlightEvents.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            if (FlightEvents.Count > 50)
                FlightEvents.RemoveAt(0);
        }

        private void ClearEvents(object parameter)
        {
            FlightEvents.Clear();
            AddEvent("Журнал очищен");
        }

        private void UpdateCommands()
        {
            CreateCommand?.RaiseCanExecuteChanged();
            DeleteCommand?.RaiseCanExecuteChanged();
            TakeOffCommand?.RaiseCanExecuteChanged();
            LandCommand?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        private EventHandler _canExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { _canExecuteChanged += value; }
            remove { _canExecuteChanged -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
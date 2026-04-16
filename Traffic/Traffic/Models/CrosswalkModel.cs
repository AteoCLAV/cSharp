using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Traffic.Models
{
    public class CrosswalkModel
    {
        private const double CanvasWidthValue = 760;
        private const double CanvasHeightValue = 180;
        private const double CrosswalkStartX = 340;
        private const double CrosswalkEndX = 420;
        private const double CarStopX = 310;
        private const double CarResetX = -90;
        private const double CarRoadEndX = 820;
        private const double CarLaneY = 82;
        private const double PedestrianStartY = 18;
        private const double PedestrianEndY = 150;
        private const double PedestrianResetY = 6;
        private const double CollisionX = 372;
        private const double CollisionY = 88;
        private const double EmergencyStartX = -80;
        private const double EmergencyDestinationX = 250;
        private const double EmergencyLaneY = 36;
        private const double EmergencySpeed = 5.5;

        private readonly IEmergencyService _emergencyService;
        private readonly Random _random = new();
        private readonly int _carPhaseTicks;
        private readonly int _pedestrianPhaseTicks;

        private int _ticksInCurrentPhase;
        private Car? _crashedCar;
        private Pedestrian? _crashedPedestrian;

        public TrafficLight TrafficLight { get; }

        public ObservableCollection<Car> Cars { get; } = new();
        public ObservableCollection<Car> EmergencyCars { get; } = new();
        public ObservableCollection<Pedestrian> Pedestrians { get; } = new();

        public double EmergencyProbability { get; }
        public int TickIntervalMs { get; }
        public double CanvasWidth => CanvasWidthValue;
        public double CanvasHeight => CanvasHeightValue;
        public bool IsAccidentActive => _crashedCar != null && _crashedPedestrian != null;

        public DateTime? LastEmergencyTime { get; private set; }

        public event EventHandler? EmergencyOccured;
        public event EventHandler? EmergencyArrived;

        public CrosswalkModel(
            double emergencyProbability,
            int tickIntervalMs,
            int carPhaseTicks,
            int pedestrianPhaseTicks)
        {
            if (emergencyProbability < 0 || emergencyProbability > 1)
                throw new ArgumentOutOfRangeException(nameof(emergencyProbability));

            if (tickIntervalMs < 50)
                throw new ArgumentOutOfRangeException(nameof(tickIntervalMs));

            if (carPhaseTicks < 2)
                throw new ArgumentOutOfRangeException(nameof(carPhaseTicks));

            if (pedestrianPhaseTicks < 2)
                throw new ArgumentOutOfRangeException(nameof(pedestrianPhaseTicks));

            EmergencyProbability = emergencyProbability;
            TickIntervalMs = tickIntervalMs;
            _carPhaseTicks = carPhaseTicks;
            _pedestrianPhaseTicks = pedestrianPhaseTicks;

            TrafficLight = new TrafficLight();
            _emergencyService = CreateEmergencyServiceByReflection();
            TrafficLight.SwitchTo(TrafficLightState.RedForPedestrians);
        }

        public void ApplyTick()
        {
            if (EmergencyCars.Count > 0)
            {
                UpdateEmergencyMovement();
                return;
            }

            AdvanceTrafficLightIfNeeded();
            MoveCars();
            MovePedestrians();
            TryCreateAccident();
            _ticksInCurrentPhase++;
        }

        internal void SpawnEmergencyCar()
        {
            if (EmergencyCars.Count > 0)
                return;

            EmergencyCars.Add(new Car
            {
                PositionX = EmergencyStartX,
                PositionY = EmergencyLaneY,
                Speed = EmergencySpeed,
                IsEmergency = true
            });
        }

        private void AdvanceTrafficLightIfNeeded()
        {
            var phaseLength = TrafficLight.State == TrafficLightState.RedForPedestrians
                ? _carPhaseTicks
                : _pedestrianPhaseTicks;

            if (_ticksInCurrentPhase < phaseLength)
                return;

            var nextState = TrafficLight.State == TrafficLightState.RedForPedestrians
                ? TrafficLightState.GreenForPedestrians
                : TrafficLightState.RedForPedestrians;

            TrafficLight.SwitchTo(nextState);
            _ticksInCurrentPhase = 0;
        }

        private void MoveCars()
        {
            var pedestriansCanCross = TrafficLight.State == TrafficLightState.GreenForPedestrians;

            foreach (var car in Cars)
            {
                car.PositionY = CarLaneY;

                if (pedestriansCanCross && car.PositionX < CarStopX)
                {
                    car.PositionX = Math.Min(car.PositionX + car.Speed, CarStopX);
                }
                else if (pedestriansCanCross && car.PositionX >= CarStopX && car.PositionX < CrosswalkStartX)
                {
                }
                else
                {
                    car.PositionX += car.Speed;
                }

                if (car.PositionX > CarRoadEndX)
                    ResetCar(car);
            }
        }

        private void MovePedestrians()
        {
            var pedestriansCanCross = TrafficLight.State == TrafficLightState.GreenForPedestrians;

            foreach (var pedestrian in Pedestrians)
            {
                if (pedestriansCanCross && pedestrian.PositionY <= PedestrianStartY)
                    pedestrian.IsCrossing = true;

                if (!pedestrian.IsCrossing)
                    continue;

                pedestrian.PositionY += pedestrian.Speed;

                if (pedestrian.PositionY >= PedestrianEndY)
                    ResetPedestrian(pedestrian);
            }
        }

        private void TryCreateAccident()
        {
            if (TrafficLight.State != TrafficLightState.GreenForPedestrians)
                return;

            if (_random.NextDouble() >= EmergencyProbability)
                return;

            var pedestrian = Pedestrians.FirstOrDefault(p => p.IsCrossing && p.PositionY > 50 && p.PositionY < 118);
            var car = Cars
                .Where(c => c.PositionX <= CarStopX + 2)
                .OrderByDescending(c => c.PositionX)
                .FirstOrDefault();

            if (pedestrian == null || car == null)
                return;

            car.PositionX = CollisionX;
            pedestrian.PositionY = CollisionY;

            car.IsAccidentParticipant = true;
            pedestrian.IsAccidentParticipant = true;

            _crashedCar = car;
            _crashedPedestrian = pedestrian;
            LastEmergencyTime = DateTime.Now;

            EmergencyOccured?.Invoke(this, EventArgs.Empty);
            _emergencyService.ArriveAtCrosswalk(this);
        }

        private void UpdateEmergencyMovement()
        {
            foreach (var emergencyCar in EmergencyCars)
                emergencyCar.PositionX += emergencyCar.Speed;

            var completed = EmergencyCars.FirstOrDefault(c => c.PositionX >= EmergencyDestinationX);
            if (completed == null)
                return;

            EmergencyCars.Clear();
            ClearAccident();
            EmergencyArrived?.Invoke(this, EventArgs.Empty);
        }

        private void ClearAccident()
        {
            if (_crashedCar != null)
            {
                _crashedCar.IsAccidentParticipant = false;
                ResetCar(_crashedCar);
            }

            if (_crashedPedestrian != null)
            {
                _crashedPedestrian.IsAccidentParticipant = false;
                ResetPedestrian(_crashedPedestrian);
            }

            _crashedCar = null;
            _crashedPedestrian = null;
        }

        private static void ResetCar(Car car)
        {
            car.PositionX = CarResetX;
            car.PositionY = CarLaneY;
        }

        private static void ResetPedestrian(Pedestrian pedestrian)
        {
            pedestrian.PositionY = PedestrianResetY;
            pedestrian.IsCrossing = false;
        }

        private static IEmergencyService CreateEmergencyServiceByReflection()
        {
            var assembly = typeof(IEmergencyService).Assembly;

            var serviceType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(IEmergencyService).IsAssignableFrom(t) &&
                                     t.IsClass &&
                                     !t.IsAbstract);

            if (serviceType == null)
                throw new InvalidOperationException("Не найдена реализация IEmergencyService.");

            return (IEmergencyService)Activator.CreateInstance(serviceType)!;
        }
    }
}


using System;

namespace Laba2.Models
{
    public abstract class FlyingVehicle
    {
        private double _altitude;

        public double Altitude
        {
            get => _altitude;
            protected set
            {
                if (value < 0)
                    throw new ArgumentException("Высота не может быть отрицательной");
                _altitude = value;
            }
        }

        public string Name { get; protected set; }
        public bool IsFlying => Altitude > 0;

        public abstract bool TakeOff();
        public abstract bool Land();

        public event EventHandler<FlightEventArgs> TookOff;
        public event EventHandler<FlightEventArgs> Landed;
        public event EventHandler<AltitudeChangedEventArgs> AltitudeChanged;

        protected virtual void OnTookOff(string message)
        {
            TookOff?.Invoke(this, new FlightEventArgs(Name, message, Altitude));
        }

        protected virtual void OnLanded(string message)
        {
            Landed?.Invoke(this, new FlightEventArgs(Name, message, Altitude));
        }

        protected virtual void OnAltitudeChanged(double oldAltitude, double newAltitude)
        {
            AltitudeChanged?.Invoke(this, new AltitudeChangedEventArgs(Name, oldAltitude, newAltitude));
        }

        protected void SetAltitude(double newAltitude)
        {
            double oldAltitude = Altitude;
            Altitude = newAltitude;
            OnAltitudeChanged(oldAltitude, newAltitude);
        }
    }

    public class Airplane : FlyingVehicle
    {
        private double _runwayLength;

        public double RunwayLength
        {
            get => _runwayLength;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Длина взлетной полосы должна быть положительной");
                _runwayLength = value;
            }
        }

        public Airplane(string name, double runwayLength)
        {
            Name = name;
            RunwayLength = runwayLength;
            Altitude = 0;
        }

        public override bool TakeOff()
        {
            try
            {
                if (IsFlying)
                {
                    OnTookOff($"Самолет {Name} уже в воздухе на высоте {Altitude} м");
                    return false;
                }

                if (RunwayLength < 500)
                {
                    OnTookOff($"Самолет {Name} не может взлететь: слишком короткая полоса ({RunwayLength} м)");
                    return false;
                }

                SetAltitude(1000);
                OnTookOff($"Самолет {Name} успешно взлетел. Высота: {Altitude} м, длина полосы: {RunwayLength} м");
                return true;
            }
            catch (Exception ex)
            {
                OnTookOff($"Ошибка при взлете самолета {Name}: {ex.Message}");
                return false;
            }
        }

        public override bool Land()
        {
            try
            {
                if (!IsFlying)
                {
                    OnLanded($"Самолет {Name} уже на земле");
                    return false;
                }

                if (RunwayLength < 400)
                {
                    OnLanded($"Самолет {Name} не может сесть: слишком короткая полоса ({RunwayLength} м)");
                    return false;
                }

                double oldAltitude = Altitude;
                SetAltitude(0);
                OnLanded($"Самолет {Name} успешно приземлился. Высота снижения: {oldAltitude} м, длина полосы: {RunwayLength} м");
                return true;
            }
            catch (Exception ex)
            {
                OnLanded($"Ошибка при посадке самолета {Name}: {ex.Message}");
                return false;
            }
        }
    }

    public class Helicopter : FlyingVehicle
    {
        public Helicopter(string name)
        {
            Name = name;
            Altitude = 0;
        }

        public override bool TakeOff()
        {
            try
            {
                if (IsFlying)
                {
                    OnTookOff($"Вертолет {Name} уже в воздухе на высоте {Altitude} м");
                    return false;
                }

                SetAltitude(500);
                OnTookOff($"Вертолет {Name} успешно взлетел вертикально. Высота: {Altitude} м");
                return true;
            }
            catch (Exception ex)
            {
                OnTookOff($"Ошибка при взлете вертолета {Name}: {ex.Message}");
                return false;
            }
        }

        public override bool Land()
        {
            try
            {
                if (!IsFlying)
                {
                    OnLanded($"Вертолет {Name} уже на земле");
                    return false;
                }

                double oldAltitude = Altitude;
                SetAltitude(0);
                OnLanded($"Вертолет {Name} успешно приземлился вертикально. Высота снижения: {oldAltitude} м");
                return true;
            }
            catch (Exception ex)
            {
                OnLanded($"Ошибка при посадке вертолета {Name}: {ex.Message}");
                return false;
            }
        }
    }

    public class FlightEventArgs : EventArgs
    {
        public string VehicleName { get; }
        public string Message { get; }
        public double Altitude { get; }
        public DateTime Time { get; }

        public FlightEventArgs(string vehicleName, string message, double altitude)
        {
            VehicleName = vehicleName;
            Message = message;
            Altitude = altitude;
            Time = DateTime.Now;
        }
    }

    public class AltitudeChangedEventArgs : EventArgs
    {
        public string VehicleName { get; }
        public double OldAltitude { get; }
        public double NewAltitude { get; }
        public DateTime Time { get; }

        public AltitudeChangedEventArgs(string vehicleName, double oldAltitude, double newAltitude)
        {
            VehicleName = vehicleName;
            OldAltitude = oldAltitude;
            NewAltitude = newAltitude;
            Time = DateTime.Now;
        }
    }
}
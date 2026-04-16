using System;

namespace Traffic.Models
{
    public class TrafficLight
    {
        public TrafficLightState State { get; private set; } = TrafficLightState.RedForPedestrians;

        public event EventHandler? GreenForPedestrians;
        public event EventHandler? RedForPedestrians;

        public void SwitchTo(TrafficLightState newState)
        {
            if (State == newState)
                return;

            State = newState;

            if (State == TrafficLightState.GreenForPedestrians)
                GreenForPedestrians?.Invoke(this, EventArgs.Empty);
            else
                RedForPedestrians?.Invoke(this, EventArgs.Empty);
        }
    }
}


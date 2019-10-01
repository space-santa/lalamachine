using System;

namespace LibLala.DomainPrimitives
{
    public class TrackLength
    {
        private TimeSpan _duration;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (value.TotalSeconds < 1) { throw new ArgumentException(paramName: nameof(value), message: "Track can't be less than 1 second."); }
                if (value.TotalSeconds > 10 * 60 * 60) { throw new ArgumentException(paramName: nameof(value), message: "Track can't be longer than 10 hours."); }
                _duration = value;
            }
        }

        public TrackLength(TimeSpan duration)
        {
            Duration = duration;
        }

        public TrackLength(int totalSeconds)
        {
            Duration = new TimeSpan(0, 0, totalSeconds);
        }

        public override string ToString()
        {
            var seconds = Duration.Seconds;
            var minutesValue = (int)Duration.TotalMinutes;
            var secondsString = $"{seconds}";
            if (secondsString.Length == 1)
            {
                secondsString = $"0{secondsString}";
            }
            return $"{minutesValue}:{secondsString}";
        }

        public int TotalSeconds => (int)Duration.TotalSeconds;
    }
}

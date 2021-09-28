using System;

namespace TagReporter.Models
{
    public class Measurement
    {
        public DateTime DateTime { get; set; }
        public Double TemperatureValue { get; set; }
        public Double Cap { get; set; }

        public Measurement() {}

        public Measurement(DateTime dateTime, Double temperature, Double cap)
        {
            DateTime = dateTime;
            TemperatureValue = temperature;
            Cap = cap;
        }
    }
}

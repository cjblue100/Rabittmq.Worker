using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherReport.WorkerService1.Model
{
    class WeatherReport
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF { get; set; }

        public string Summary { get; set; }
    }
}

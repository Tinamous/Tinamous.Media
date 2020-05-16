using System;

namespace AnalysisUK.Tinamous.Media.Domain.Documents
{
    public class LocationDetails
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Friendly name of the user/devices current location.
        /// </summary>
        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Elevation { get; set; }
    }
}
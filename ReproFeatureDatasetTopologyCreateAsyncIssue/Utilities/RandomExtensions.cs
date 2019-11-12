using System;
using ArcGIS.Core.Geometry;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities
{
    /// <summary>
    /// Extension methods to generate random coordinates within a given extent.
    /// </summary>
    public static class RandomExtension
    {
        /// <summary>
        /// Generate a random double number between the min and max values.
        /// </summary>
        /// <param name="random">Instance of a random class.</param>
        /// <param name="minValue">The min value for the potential range.</param>
        /// <param name="maxValue">The max value for the potential range.</param>
        /// <returns>Random number between min and max</returns>
        /// <remarks>The random result number will always be less than the max number.</remarks>
        public static double NextDouble(this Random random, double minValue, double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Generate a random coordinate (only x,y values)  within the provided envelope.
        /// </summary>
        /// <param name="random">Instance of a random class.</param>
        /// <param name="withinThisExtent">Area of interest in which the random coordinate will be created.</param>
        /// <returns>A coordinate with random values (only x,y values) within the extent.</returns>
        public static Coordinate2D NextCoordinate2D(this Random random, Envelope withinThisExtent)
        {
            return new Coordinate2D(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
                                random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax));
        }

        /// <summary>
        /// /Generate a random coordinate 3D (containing x,y,z values) within the provided envelope.
        /// </summary>
        /// <param name="random">Instance of a random class.</param>
        /// <param name="withinThisExtent">Area of interest in which the random coordinate will be created.</param>
        /// <returns>A coordinate with random values 3D (containing x,y,z values) within the extent.</returns>
        public static Coordinate3D NextCoordinate3D(this Random random, Envelope withinThisExtent)
        {
            return new Coordinate3D(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
                random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax), 0);
        }
    }
}
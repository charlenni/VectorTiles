﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace VectorTiles.MapboxGL.Json
{
    /// <summary>
    /// Class holding StoppedFloat data in Json format
    /// </summary>
    public class StoppedFloatArray
    {
        [JsonProperty("base")]
        public float Base { get; set; } = 1f;

        [JsonProperty("stops")]
        public IList<KeyValuePair<float, float[]>> Stops { get; set; }

        public float[] SingleVal { get; set; }

        /// <summary>
        /// Calculate the correct value for a stopped function
        /// No Bezier type up to now
        /// </summary>
        /// <param name="contextZoom">Zoom factor for calculation </param>
        /// <param name="stoppsType">Type of calculation (interpolate, exponential, categorical)</param>
        /// <returns>Value for this stopp respecting zoom factor and type</returns>
        public float[] Evaluate(float? contextZoom, StopsType stoppsType = StopsType.Exponential)
        {
            // Are there no stopps, but a single value?
            // !=
            if (SingleVal != null)
                return SingleVal;

            // Are there no stopps in array
            if (Stops.Count == 0)
                return null;

            float zoom = contextZoom ?? 0f;

            var lastZoom = Stops[0].Key;
            var lastValue = Stops[0].Value;

            if (lastZoom > zoom)
                return lastValue;

            for (int i = 1; i < Stops.Count; i++)
            {
                var nextZoom = Stops[i].Key;
                var nextValue = Stops[i].Value;

                if (zoom == nextZoom)
                    return nextValue;

                if (lastZoom <= zoom && zoom < nextZoom)
                {
                    switch (stoppsType)
                    {
                        case StopsType.Interval:
                            return lastValue;
                        case StopsType.Categorical:
                            if (nextZoom - zoom < float.Epsilon)
                                return nextValue;
                            break;
                    }
                }

                lastZoom = nextZoom;
                lastValue = nextValue;
            }

            return lastValue;
        }
    }
}

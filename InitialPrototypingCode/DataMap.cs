using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class holds all data-changing functions
//This version contains a mapping function for converting raw data to Unity-friendly ranges
namespace Data.Namespace
{
    public static class DataMap
    {

        public static float Map(this float value, float inputFrom, float inputTo, float outputFrom, float outputTo)
        {
            return (value - inputFrom) / (outputFrom - inputFrom) * (outputTo - outputFrom) + outputFrom;
        }

    }
}

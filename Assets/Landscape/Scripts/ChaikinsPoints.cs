using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuiteSensible
{
    /// <summary>
    /// For creating smooth path from 
    /// a listof 3D points using 
    /// Chaikins algorithm
    /// </summary>
    public class ChaikinsPoints
    {
        public Vector3[] Points;

        private readonly Vector3[] buffer; // backbuffer
        private readonly int pointCount;
        private readonly int iterations;
        private readonly int controlPoints;

        public ChaikinsPoints(int controlPointCount = 3, int iterationCount = 4)
        {
            controlPoints = controlPointCount;
            iterations = iterationCount;
            pointCount = CalcPointsForIterations(iterations);
            Points = new Vector3[pointCount];
            buffer = new Vector3[pointCount];
        }

        public void CalcPoints(Vector3[] controlPoints)
        {
            int dataSize = controlPoints.Length;
            Array.Copy(controlPoints, Points, controlPoints.Length);

            var input = Points;
            var output = buffer;

            for (int i = 0; i < iterations; i++)
            {
                dataSize = ApplyChaikins(input, dataSize, output);
                var temp = input;
                input = output;
                output = temp;
            }

            if (Points != input)
                Array.Copy(input, Points, pointCount);
        }

        private int ApplyChaikins(Vector3[] path, int dataSize, Vector3[] output)
        {
            int index = 0;
            output[index++] = path[0];

            for (var i = 0; i < dataSize - 1; i++)
            {
                var p0 = path[i];
                var p1 = path[i + 1];
                var q = (p0 * .75f) + (p1 * .25f);
                var r = (p0 * .25f) + (p1 * .75f);

                output[index++] = q;
                output[index++] = r;
            }

            output[index++] = path[dataSize - 1];

            return index;
        }

        /// <summary>
        /// The result is equivalent to 
        /// (.75 multiplied by 2 iteration-1 times) * 8
        /// </summary>
        /// <param name="iterations">Smoothness. 4 is good.</param>
        /// <returns>How many points we need to allocate</returns>
        private int CalcPointsForIterations(int iterations)
        {
            int dataSize = controlPoints;

            for (int i = 0; i < iterations; i++)
                dataSize = CalcDataSize(dataSize);

            return dataSize;
        }

        private int CalcDataSize(int dataSize)
        {
            int index = 2;

            for (var i = 0; i < dataSize - 1; i++)
                index += 2;

            return index;
        }

    }
}
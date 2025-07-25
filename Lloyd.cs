using Rewired.Utils.Classes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BazaarIsMyHome
{
    public class Lloyd
    {

        public static List<Vector2> GenerateCirclePoints(float radius, float startAngle, float endAngle, float orientation, int numberOfPoints)
        {
            List<Vector2> points = new List<Vector2>();
            float angleStep = (endAngle - startAngle) / (numberOfPoints - 1);

            for (int i = 0; i < numberOfPoints; i++)
            {
                float angleInDegrees = startAngle + i * angleStep + orientation;
                float angleInRadians = angleInDegrees * Mathf.Deg2Rad; // Convert to radians
                float x = radius * Mathf.Cos(angleInRadians);
                float y = radius * Mathf.Sin(angleInRadians);
                points.Add(new Vector2(x, y));
            }
            return points;
        }

        public static List<Vector2> LloydsAlgorithm(List<Vector2> dataPoints, int k)
        {
            // Step 1: Initialize centroids
            Vector2[] centroids = new Vector2[k];
            HashSet<int> selectedIndices = new HashSet<int>();
            System.Random rand = new System.Random();

            for (int i = 0; i < k; i++)
            {
                int index;
                do
                {
                    index = rand.Next(dataPoints.Count);
                } while (selectedIndices.Contains(index));
                selectedIndices.Add(index);
                centroids[i] = dataPoints[index];
            }

            List<List<Vector2>> clusters = new List<List<Vector2>>();
            bool changed;

            do
            {
                // Step 2: Assign clusters
                clusters = new List<List<Vector2>>(new List<Vector2>[k].Select(x => new List<Vector2>()));
                foreach (var point in dataPoints)
                {
                    int closestCentroidIndex = FindClosestCentroid(point, centroids);
                    clusters[closestCentroidIndex].Add(point);
                }

                // Step 3: Update centroids
                changed = false;
                for (int i = 0; i < k; i++)
                {
                    if (clusters[i].Count > 0)
                    {
                        Vector2 newCentroid = CalculateMean(clusters[i]);
                        if (newCentroid != centroids[i])
                        {
                            centroids[i] = newCentroid;
                            changed = true;
                        }
                    }
                }
            } while (changed);

            return new List<Vector2>(centroids);
        }
        static int FindClosestCentroid(Vector2 point, Vector2[] centroids)
        {
            float minDistance = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < centroids.Length; i++)
            {
                float distance = Vector2.Distance(point, centroids[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
        static Vector2 CalculateMean(List<Vector2> points)
        {
            Vector2 mean = Vector2.zero;

            foreach (var point in points)
            {
                mean += point;
            }

            mean /= points.Count;
            return mean;
        }

    }
}

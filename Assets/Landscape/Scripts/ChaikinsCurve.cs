using UnityEngine;
using System;

/// <summary>
/// Tester for curve gen
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class ChaikinsCurve : MonoBehaviour
{
    public Vector3[] controlPoints;
    [Range(0, 8)]
    public int iterations = 4; // Note even numbers are quicker as no need to copy buffer

    private LineRenderer lineRenderer;
    private Vector3[] points;
    private int pointCount;

    private Vector3[] buffer; // backbuffer

    private void OnValidate()
    {
        Init();
    }

    private void Init()
    {
        lineRenderer = GetComponent<LineRenderer>();

        pointCount = CalcPointsForIterations(iterations);
        points = new Vector3[pointCount];
        buffer = new Vector3[pointCount];        
        lineRenderer.positionCount = pointCount;

        Draw();
    }

    void Start()
    {
        Init();

#if TIME_TEST
        if (curveType == CurveType.Chaikins)
        {
            // The result is equivalent to 
            // (.75 multiplied by 2 iteration-1 times) * 8
            pointCount = CalcPointsForIterations(iterations);
            buffer = new Vector3[pointCount];

            Debug.Log("PointCount: " + pointCount);
        }
        else
            pointCount = 24;

        points = new Vector3[pointCount];        

        lineRenderer.positionCount = points.Length;


        float start = Time.realtimeSinceStartup;

        for (int i = 0; i < 60000; i++)
        {
            ChaikinsCurveBuffered(Vector3.zero, new Vector3(0, 0, 5f), 5f);
        }

        float elapsedChaikins = Time.realtimeSinceStartup - start;
        Debug.Log("Elapsed Chaikins: " + elapsedChaikins);

        start = Time.realtimeSinceStartup;
        for (int i = 0; i < 60000; i++)
        {
            SineCurve(Vector3.zero, new Vector3(0, 0, 5f), 5f);
        }

        float elapsedSine = Time.realtimeSinceStartup - start;
        Debug.Log("Elapsed Sine: " + elapsedSine);

        if (curveType == CurveType.Chaikins)
            ChaikinsCurveBuffered(Vector3.zero, new Vector3(0, 0, 5f), 5f);
        else
            SineCurve(Vector3.zero, new Vector3(0, 0, 5f), 5f);
#endif

    }


    void Update()
    {
        MessWithPoints();
        Draw();
    }

    private void Draw()
    {
        ChaikinsCurveBuffered();
        lineRenderer.SetPositions(points);
    }

    private void ChaikinsCurveBuffered()
    {
        Array.Copy(controlPoints, points, controlPoints.Length);

        int dataSize = controlPoints.Length;

        var input = points;
        var output = buffer;

        for (int i = 0; i < iterations; i++)
        {
            dataSize = ApplyChaikins(input, dataSize, output);
            var temp = input;
            input = output;
            output = temp;
        }

        if (points != input)
            Array.Copy(input, points, pointCount);
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
        int dataSize = controlPoints.Length;

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

    private void MessWithPoints()
    {
        for (int i = 0; i < controlPoints.Length; i++)
        {
            controlPoints[i].x += (UnityEngine.Random.value - .5f) / 100f;
            controlPoints[i].y += (UnityEngine.Random.value - .5f) / 100f;
            controlPoints[i].z += (UnityEngine.Random.value - .5f) / 100f;
        }
    }
}

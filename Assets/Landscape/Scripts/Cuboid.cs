using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cuboid : MonoBehaviour
{
    public int divisions = 8;
    public float size = 1f;

    private Vector3[] vertices;

    void Start()
    {
        if (divisions % 2 != 0)
            divisions += 1;

        float cellSize = size / (float)divisions;
        float halfSize = size / 2f;
        int halfDivisions = divisions / 2;
        int stride = divisions + 1;
        vertices = new Vector3[stride * stride * 6];

        
        // top
        float y = halfSize;

        Vector3 leftTriV1 = new Vector3(0, 0, 0);
        Vector3 leftTriV2 = new Vector3(0, 0, 1);
        Vector3 leftTriV3 = new Vector3(1, 0, 0);
        // duplicate of left3 as we want to be able to break it up
        Vector3 rightTriV1 = new Vector3(1, 0, 0);
        Vector3 rightTriV2 = new Vector3(0, 0, 1);
        Vector3 rightTriV3 = new Vector3(1, 0, 1);

        int vx = 0;
        for (int z = 0; z < stride; z++)
        {
            for (int x = 0; x < stride; x++)
            {
                vertices[vx++] = new Vector3(-halfSize + cellSize * x, halfSize, -halfSize + cellSize * z);
            }
        }
        // front
        for (int z = 0; z < stride; z++)
        {
            for (int x = 0; x < stride; x++)
            {
                vertices[vx++] = new Vector3(-halfSize + cellSize * x, -halfSize + y * stride, -halfSize);
            }
        }
        //left
    }

    void Update()
    {
        
    }
}

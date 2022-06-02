using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gfx : MonoBehaviour
{
    public float blowupScale = .01f;
    public float blowupTimeScale = 4f;

    private Mesh mesh;
    private Vector3[] origVertices;
    private System.Action currentPhase;
    private float currentPhaseLength;
    private float currentPhaseCounter;

    public void Blowup(float time = 2f)
    {
        currentPhaseLength = time;
        currentPhaseCounter = 0f;
        currentPhase = BlowupStep;
    }

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        origVertices = mesh.vertices;

        Blowup();
    }

    void Update()
    {
        currentPhase?.Invoke();
    }

    private void BlowupStep()
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        bool flip = true;
        for (int j = 0; j < mesh.triangles.Length; j += 3)
        {
            if (flip)
            {
                int ix0 = mesh.triangles[j];
                int ix1 = mesh.triangles[j + 1];
                int ix2 = mesh.triangles[j + 2];

                float sin = Mathf.Sin(Time.time * blowupTimeScale) * blowupScale;
                vertices[ix0] += normals[ix0] * sin;
                vertices[ix1] += normals[ix1] * sin;
                vertices[ix2] += normals[ix2] * sin;
            }

            flip = !flip;
        }

        mesh.vertices = vertices;

        currentPhaseCounter += Time.deltaTime;
        if (currentPhaseCounter >= currentPhaseLength)
        {
            mesh.vertices = origVertices;
            currentPhase = null;
        }
    }
}

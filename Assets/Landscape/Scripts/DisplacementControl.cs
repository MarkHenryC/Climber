using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplacementControl : MonoBehaviour
{
    public float size = 1f;

    MeshRenderer meshRenderer;
    float displacementAmount;
    Material material;
    List<Material> materials = new List<Material>();
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.GetMaterials(materials);
        material = materials[0];
        displacementAmount = 0f;
    }

    void Update()
    {
        if (displacementAmount > 0)
        {
            displacementAmount = Mathf.Lerp(displacementAmount, 0f, Time.deltaTime);
            material.SetFloat("_Amount", displacementAmount);
        }
        if (Input.GetButtonDown("Jump"))
            displacementAmount += size;
    }
}

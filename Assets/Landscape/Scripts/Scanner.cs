using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuiteSensible
{
    [RequireComponent(typeof(LineRenderer))]
    public class Scanner : MonoBehaviour
    {
        public float rotationsPerSecond = .25f;
        public float scanDistance = 10f;
        public Vector3 scanSize = Vector3.one;
        public LayerMask targetMask;
        public GameData gameData;
        public float damageStrength = 10f;
        public float speedVariance = 0f;
        public float distanceVariance = 0f;
        public Transform scanGuide;
        public GameObject beamMaterialContainer;
        public float uvXAnim = .1f;
        public float uvXAnimMultiplier = 4f;

        private Vector3 scanHalfExtents;
        private float currentAngle;
        private Vector3 currentDirection = Vector3.zero;
        private LineRenderer scanLine;
        private bool haveTarget;
        private RaycastHit raycastHit;
        private Material beamMaterial;
        private Vector2 uv = Vector2.zero;
        int rotationDirection = 1;

        void Start()
        {
            scanGuide.localScale = scanSize;
            scanHalfExtents = scanSize / 2f;
            
            scanLine = GetComponent<LineRenderer>();
            scanLine.positionCount = 2;
            scanLine.SetPosition(0, Vector3.zero);
            
            // LineRenderer is not pretty but handy for guide
            // Related code could be used if we replace LR
            // with something nicer
            // scanLine.enabled = gameData.Playing;
            
            scanGuide.gameObject.SetActive(gameData.Playing);
        
            if (speedVariance != 0f)
                rotationsPerSecond += Random.Range(-speedVariance, speedVariance);
            if (distanceVariance != 0f)
                scanDistance += Random.Range(-distanceVariance, distanceVariance);

            var r = beamMaterialContainer.GetComponent<Renderer>();
            beamMaterial = r.sharedMaterial;
            rotationDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        }

        void Update()
        {
            if (gameData.Playing)
            {
                if (!scanGuide.gameObject.activeSelf)
                    scanGuide.gameObject.SetActive(true);

                float rad = Mathf.Deg2Rad * currentAngle;
                currentDirection.x = Mathf.Sin(rad);
                currentDirection.z = Mathf.Cos(rad);

                //scanLine.SetPosition(1, currentDirection * scanDistance);
                
                if (Physics.BoxCast(transform.position, scanHalfExtents, currentDirection,
                    out raycastHit, Quaternion.LookRotation(currentDirection), scanDistance, targetMask))
                {
                    gameData.TakePlayerHealth(Time.deltaTime, damageStrength);                    
                    
                    haveTarget = true;
                }
                else if (haveTarget)
                {                                        
                    haveTarget = false;
                }                

                if (haveTarget)
                {
                    scanGuide.position = transform.position + currentDirection * raycastHit.distance / 2f;
                    scanSize.z = raycastHit.distance;                    
                    uv.y += uvXAnim * Time.deltaTime * uvXAnimMultiplier;
                }
                else
                {
                    scanGuide.position = transform.position + currentDirection * scanDistance / 2f;
                    scanSize.z = scanDistance;
                    uv.y -= uvXAnim * Time.deltaTime;
                }

                beamMaterial.mainTextureOffset = uv;

                scanGuide.rotation = Quaternion.LookRotation(currentDirection);
                scanGuide.localScale = scanSize;

                if (!haveTarget)
                    currentAngle += Time.deltaTime * (360f * rotationsPerSecond * rotationDirection);
            }
            else if (scanGuide.gameObject.activeSelf)
                scanGuide.gameObject.SetActive(true);
        }
    }
}
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
        public LayerMask targetMask;
        public GameData gameData;
        public float damageStrength = 10f;
        public float speedVariance = 0f;
        public float distanceVariance = 0f;
        public Transform scanGuide;
        public Transform rotator;
        public float uvXAnim = .1f;
        public float uvXAnimMultiplier = 4f;
        public ParticleSystem particles;
        public AnimationCurve lengthMapping;        
        public Vector3 scanHalfExtents;
        public float lengthParticleDivisor = 25f; // divisor to absolute length to get corresponding particle length via startLifetime
        public bool rotate = true;
        public Plasma plasma;

        private float currentAngle;
        private Vector3 currentDirection = Vector3.zero;
        private LineRenderer scanLine;
        private bool haveTarget;
        private RaycastHit raycastHit;
        private Vector2 uv = Vector2.zero;
        private int rotationDirection = 1;
        private float scanLength;

        void Start()
        {
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
                
                scanLength = scanDistance;
                if (Physics.BoxCast(transform.position, scanHalfExtents, currentDirection,
                    out raycastHit, Quaternion.LookRotation(currentDirection), scanDistance, targetMask))
                {
                    if (raycastHit.transform.gameObject.CompareTag("Player"))
                    {
                        gameData.TakePlayerHealth(Time.deltaTime, damageStrength);
                        haveTarget = true;
                    }
                    scanLength = raycastHit.distance;
                    
                }
                else if (haveTarget)
                {                                        
                    haveTarget = false;
                }

                Debug.DrawLine(scanGuide.position, scanGuide.position + scanGuide.forward * scanLength);

                var main = particles.main;

                main.startLifetime = scanLength / lengthParticleDivisor;

                plasma.SetLength(scanLength / lengthParticleDivisor);

                rotator.rotation = Quaternion.LookRotation(currentDirection);

                if (!haveTarget && rotate)
                    currentAngle += Time.deltaTime * (360f * rotationsPerSecond * rotationDirection);
            }
            else if (scanGuide.gameObject.activeSelf)
                scanGuide.gameObject.SetActive(true);
        }
    }
}
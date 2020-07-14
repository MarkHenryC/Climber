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
        public float scanLength, scanWidth, scanHeight;
        public LayerMask targetMask;
        public GameData gameData;
        public float damageStrength = 10f;
        public float speedVariance = 0f;
        public float distanceVariance = 0f;
        private Vector3 scanHalfExtents;
        private float currentAngle;
        private Vector3 currentDirection = Vector3.zero;
        private LineRenderer scanLine;
        private bool haveTarget;

        void Start()
        {
            scanHalfExtents = new Vector3(scanWidth / 2f, scanHeight / 2f, scanLength / 2f);
            scanLine = GetComponent<LineRenderer>();
            scanLine.positionCount = 2;
            scanLine.SetPosition(0, Vector3.zero);
            scanLine.enabled = gameData.Playing;
            if (speedVariance != 0f)
                rotationsPerSecond += Random.Range(-speedVariance, speedVariance);
            if (distanceVariance != 0f)
                scanDistance += Random.Range(-distanceVariance, distanceVariance);
        }

        void Update()
        {
            if (gameData.Playing)
            {
                if (!scanLine.enabled)
                    scanLine.enabled = true;

                float rad = Mathf.Deg2Rad * currentAngle;
                currentDirection.x = Mathf.Sin(rad);
                currentDirection.z = Mathf.Cos(rad);

                scanLine.SetPosition(1, currentDirection * scanDistance);

                if (Physics.BoxCast(transform.position, scanHalfExtents, currentDirection,
                    out RaycastHit hit, Quaternion.identity, scanDistance, targetMask))
                {
                    gameData.TakePlayerHealth(Time.deltaTime, damageStrength);

                    haveTarget = true;
                }
                else if (haveTarget)
                {
                    haveTarget = false;
                }

                if (!haveTarget)
                    currentAngle += Time.deltaTime * (360f * rotationsPerSecond);
            }
            else if (scanLine.enabled)
                scanLine.enabled = false;
        }
    }
}
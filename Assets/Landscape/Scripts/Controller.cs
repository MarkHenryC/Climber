using QuiteSensible;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuiteSensible
{
    public class Controller : MonoBehaviour
    {
        public LevelCreator levelCreator;
        public Transform player;
        public float moveSpeed = 1f;
        public float rayCastDistance = 10f;
        public float castAngleIncrement = 15f;
        public float sphereCastRadius = 1f;

        private Coroutine currentTravelRoutine;
        private PositionData currentPositionData;
        private Camera cam;
        private ChaikinsPoints curve;

        // Start is called before the first frame update
        void Start()
        {
            cam = Camera.main;
            curve = new ChaikinsPoints();
            levelCreator.CreateLandscapeMesh();
        }

        // Update is called once per frame
        void Update()
        {
            currentPositionData = levelCreator.Scan(cam.transform.position, cam.transform.forward, rayCastDistance, sphereCastRadius, true);
            if (Input.GetMouseButtonDown(0))
            {
                //currentPositionData = levelCreator.Scan(cam.transform.position, cam.transform.forward, rayCastDistance, sphereCastRadius, true);
                HandleMouseDown();
            }
        }

        private void HandleMouseDown()
        {
            if (currentPositionData != null) // We have a valid currentPositionData
            {
                Debug.Log("Travel to " + currentPositionData.globalCentrePos);

                if (currentTravelRoutine != null)
                {
                    if (currentPositionData != null)
                    {
                        currentPositionData.obstructed = false;
                        currentPositionData.occupant = null;
                    }
                    StopCoroutine(currentTravelRoutine);
                }

                if (currentPositionData.obstructed)
                {
                    bool clearPath = GetPathAround(cam.transform.position, cam.transform.forward,
                        currentPositionData.landingQuad.startTriangleIndex, rayCastDistance, out Vector3 clearDirection);
                    float distance = Vector3.Distance(player.transform.position, currentPositionData.globalCentrePos);
                    Vector3 midPos = player.transform.position + clearDirection * (distance / 2f);
                    curve.CalcPoints(new Vector3[] { player.transform.position, midPos, currentPositionData.globalCentrePos });
                    currentTravelRoutine = StartCoroutine(MoveToDest(currentPositionData.globalCentrePos, curve.Points));

                    if (!clearPath)
                    {
                        Debug.LogWarning("No clear forward path found.");
                        currentTravelRoutine = StartCoroutine(MoveToDest(currentPositionData.globalCentrePos));
                    }
                }
                else
                    currentTravelRoutine = StartCoroutine(MoveToDest(currentPositionData.globalCentrePos));
            }
        }

        private IEnumerator MoveToDest(Vector3 dest)
        {
            float distance = Vector3.Distance(player.transform.position, dest);
            float t = 0f;
            float incr = 1f / distance;
            var wait = new WaitForEndOfFrame();
            Vector3 startPos = player.transform.position;

            while (t < 1f)
            {
                player.transform.position = Vector3.Lerp(startPos, dest, t);
                t += incr * Time.deltaTime * moveSpeed;
                yield return wait;
            }

            currentTravelRoutine = null;
        }

        private IEnumerator MoveToDest(Vector3 dest, Vector3[] points)
        {
            Vector3 start = player.transform.position;
            float distance = Vector3.Distance(start, dest);
            float section = distance / (points.Length + 1);            
            var wait = new WaitForEndOfFrame();

            for (int i = 0; i < (points.Length - 1); i++)
            {
                float t = 0f;
                float incr = 1f / section;
                while (t < 1f)
                {
                    player.transform.position = Vector3.Lerp(points[i], points[i + 1], t);
                    t += incr * Time.deltaTime * moveSpeed;
                    yield return wait;
                }
            }

            currentTravelRoutine = null;
        }

        /// <summary>
        /// We assume there is an obstruction, so
        /// start casting each side of centre
        /// </summary>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        /// <param name="destinationQuadIndex">The returned triangle index converted to start tri of quad that's our destination</param>
        /// <param name="distance"></param>
        /// <param name="points"></param>
        private bool GetPathAround(Vector3 start, Vector3 direction, int destinationFirstTriIndex, float distance, out Vector3 clearDirection)
        {
            bool clearSpace = false;
            float castAngle = 0f;

            clearDirection = Vector3.zero;

            while (!clearSpace)
            {
                castAngle += castAngleIncrement;
                clearDirection = Quaternion.Euler(0f, castAngle, 0f) * direction;

                if (Physics.SphereCast(start, sphereCastRadius, clearDirection, out RaycastHit rightHit, distance) &&
                    LandingQuad.StartTri(rightHit.triangleIndex) != destinationFirstTriIndex)
                {
                    clearDirection = Quaternion.Euler(0f, -castAngle, 0f) * direction;
                    if (!(Physics.SphereCast(start, sphereCastRadius, clearDirection, out RaycastHit leftHit, distance)) ||
                        (LandingQuad.StartTri(leftHit.triangleIndex) == destinationFirstTriIndex))
                    {
                        clearSpace = true;
                    }
                }
                else
                    clearSpace = true;

                if (castAngle >= 90f) // Going sideways not a good idea
                    return false;

            }

            return true;
        }
    }
}
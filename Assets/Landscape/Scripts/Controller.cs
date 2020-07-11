using System.Collections;
using UnityEngine;

namespace QuiteSensible
{
    public class Controller : MonoBehaviour
    {
        public LevelCreator levelCreator;
        public TouchController camController;
        public Transform player;
        public float moveSpeed = 3f;
        public float rayCastDistance = 10f;
        public Transform sphereMarker;
        public float curveHeight = 1f;

        private Coroutine currentTravelRoutine;
        private Camera cam;
        private ChaikinsPoints curve;
        PositionData positionOfInterest;

        void Start()
        {
            cam = Camera.main;
            curve = new ChaikinsPoints();
            camController.ButtonUp = HandleTrigger;

            CreateLandscape();
        }

        void Update()
        {
            positionOfInterest = levelCreator.Scan(cam.transform.position, cam.transform.forward, rayCastDistance);
        }

        private void CreateLandscape()
        {
            levelCreator.CreateLandscapeMesh();
            levelCreator.SetObjectAt(player.transform, PositionData.OccupantType.Player, levelCreator.LowestPanel);
            levelCreator.SetObjectAt(sphereMarker.transform, PositionData.OccupantType.Boss, levelCreator.HighestPanel);
        }

        private void HandleTrigger()
        {
            PositionData pd = levelCreator.GetCurrentTarget();
            Debug.Assert(pd == positionOfInterest);

            if (pd != null)
            {
                switch (pd.occupant)
                {
                    case PositionData.OccupantType.None:
                        if (currentTravelRoutine != null)
                            StopCoroutine(currentTravelRoutine);

                        Vector3 mid = Vector3.Lerp(player.transform.position, levelCreator.transform.TransformPoint(pd.centrePos), .5f);
                        Vector3 cross = Vector3.Cross(player.transform.forward, player.transform.right);
                        Vector3 apex = mid + cross * curveHeight;
                        curve.CalcPoints(new Vector3[] { player.transform.position, apex, levelCreator.transform.TransformPoint(pd.centrePos) });
                        currentTravelRoutine = StartCoroutine(FollowPath(curve.Points));

                        break;
                    case PositionData.OccupantType.Boss:

                        break;
                }
            }
        }

        private IEnumerator FollowPath(Vector3[] points)
        {
            Vector3 startPos = points[0];
            float distance = Vector3.Distance(startPos, points[points.Length - 1]);
            float section = distance / points.Length;
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
    }
}
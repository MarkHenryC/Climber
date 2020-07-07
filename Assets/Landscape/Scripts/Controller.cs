using System.Collections;
using UnityEngine;

namespace QuiteSensible
{
    public class Controller : MonoBehaviour
    {
        public LevelCreator levelCreator;
        public Transform player;
        public float moveSpeed = 3f;
        public float rayCastDistance = 10f;
        public float obstructionCastDistance = 1f;
        public float castAngleIncrement = 5f;
        public float sphereCastRadius = .25f;
        public Transform sphereMarker;
        public float curveHeight = 1f;

        private Coroutine currentTravelRoutine;
        private Camera cam;
        private ChaikinsPoints curve;

        void Start()
        {
            cam = Camera.main;
            curve = new ChaikinsPoints();
            levelCreator.CreateLandscapeMesh();
            levelCreator.SetObjectAt(player.transform, PositionData.OccupantType.Player, levelCreator.LowestPanel);
            levelCreator.SetObjectAt(sphereMarker.transform, PositionData.OccupantType.Object, levelCreator.HighestPanel);
        }

        void Update()
        {
            levelCreator.Scan(cam.transform.position, cam.transform.forward, rayCastDistance);
            if (Input.GetMouseButtonDown(0))
                HandleMouseDown();
        }

        private void HandleMouseDown()
        {
            PositionData pd = levelCreator.SetCurrentTarget();

            if (pd != null)
            {
                if (currentTravelRoutine != null)
                    StopCoroutine(currentTravelRoutine);

                Vector3 mid = Vector3.Lerp(player.transform.position, levelCreator.transform.TransformPoint(pd.centrePos), .5f);
                Vector3 cross = Vector3.Cross(player.transform.forward, player.transform.right);
                Vector3 apex = mid + cross * curveHeight;
                curve.CalcPoints(new Vector3[] { player.transform.position, apex, levelCreator.transform.TransformPoint(pd.centrePos) });
                currentTravelRoutine = StartCoroutine(FollowPath(curve.Points));
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
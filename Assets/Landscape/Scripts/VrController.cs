using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace QuiteSensible
{
   [System.Serializable]
    public class PassFloat : UnityEvent<Vector3, Vector3, float> { }

    public class VrController : MonoBehaviour
    {
        [SerializeField]
        private Transform playerTransform;
        [SerializeField]
        private LevelCreator levelCreator;
        [SerializeField]
        private InputActionReference inputActionRef;
        [SerializeField]
        private Transform leftController, rightController;
        [SerializeField]
        private float moveSpeed = 3f;
        [SerializeField]
        private float rayCastDistance = 10f;
        [SerializeField]
        private GameObject boss;
        [SerializeField]
        private float curveHeight = 1f;
        [SerializeField]
        private GameObject templateNPC;
        [SerializeField]
        private GameData gameData;
        [SerializeField]
        private int npcProbability;
        [SerializeField]
        private Fader fader;        
        [SerializeField]
        private PassFloat UpdateScanDistance;
        [SerializeField]
        private GameObject startMarker, endMarker; // test LH controller pos & angle

        private Coroutine currentTravelRoutine;
        private Camera cam;
        private ChaikinsPoints curve;
        private Vector3 pointerPosition, pointerDirection;
        PositionData positionOfInterest;
        private PlayerUi playerUi;

        void Start()
        {
            cam = Camera.main;
            curve = new ChaikinsPoints();
            playerUi = FindObjectOfType<PlayerUi>();
            inputActionRef.action.performed += InputActionPerformed;
            gameData.PlayerHealthNotification = (h) => { playerUi.SetLife(h); };
            gameData.StartGameAction = SetupGame;
            gameData.PlayerDeathNotification = LoseGame;
            gameData.StartGame();
        }

        void Update()
        {
            if (gameData.Playing)
            {
                float scanLength = rayCastDistance;
                var globalPoint = rightController.position; // cam.transform.parent.TransformPoint(pointerPosition);
                var globalDir = rightController.forward; // cam.transform.parent.TransformDirection(pointerDirection);
                positionOfInterest = levelCreator.Scan(globalPoint, globalDir, ref scanLength);
                UpdateScanDistance?.Invoke(globalPoint, globalDir, scanLength);

                startMarker.transform.position = globalPoint;
                endMarker.transform.position = globalPoint + globalDir * scanLength;
            }
        }


        void InputActionPerformed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                HandleTrigger();
            }
        }

        private void SetupGame()
        {
            fader.FadeIn();

            gameData.ReturnObjects();

            levelCreator.CreateLandscapeMesh();

            levelCreator.SetObjectAt(playerTransform, PositionData.OccupantType.Player, levelCreator.LowestPanel);
            levelCreator.CreateObjectAt(boss, PositionData.OccupantType.Boss, levelCreator.HighestPanel);

            int[] emptyIndices = levelCreator.GetEmptyPositions();
            Shuffle(emptyIndices);

            for (int i = 0; i< emptyIndices.Length; i++)
            {
                if (i % npcProbability == 0)
                    levelCreator.CreateObjectAt(templateNPC, PositionData.OccupantType.NPC, emptyIndices[i]);
            }

            playerUi.SetLife(1f); // Maybe should take from gameData but we know it's always going to be 1f unitised at this point
        }

        public void Aim(Vector3 direction)
        {
            pointerDirection = direction;
        }

        public void Position(Vector3 position)
        {
            pointerPosition = position;
        }

        /// <summary>
        /// Called as UnityEvent by input
        /// </summary>
        public void HandleTrigger()
        {
            PositionData pd = levelCreator.GetCurrentTarget();
            Debug.Assert(pd == positionOfInterest);

            if (pd != null)
            {
                switch (pd.occupant)
                {
                    case PositionData.OccupantType.None:
                        //Debug.LogFormat("Hit empty panel {0}", pd.landingQuad.startTriangleIndex);

                        if (currentTravelRoutine != null)
                            StopCoroutine(currentTravelRoutine);

                        Vector3 mid = Vector3.Lerp(playerTransform.position, levelCreator.transform.TransformPoint(pd.centrePos), .5f);
                        Vector3 cross = Vector3.Cross(playerTransform.forward, playerTransform.right);
                        Vector3 apex = mid + cross * curveHeight;
                        curve.CalcPoints(new Vector3[] { playerTransform.position, apex, levelCreator.transform.TransformPoint(pd.centrePos) });
                        currentTravelRoutine = StartCoroutine(FollowPath(curve.Points));

                        break;
                    case PositionData.OccupantType.Boss:
                        Debug.LogFormat("Hit boss panel");
                        gameData.WonGame();
                        StartCoroutine(WinGame());
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
                    if (!gameData.Playing)
                    {
                        currentTravelRoutine = null;
                        yield break;
                    }

                    playerTransform.position = Vector3.Lerp(points[i], points[i + 1], t);
                    t += incr * Time.deltaTime * moveSpeed;
                    yield return wait;
                }
            }

            currentTravelRoutine = null;
        }

        private IEnumerator WinGame()
        {
            float yInc = .01f;
            Vector3 pos = boss.transform.position;
            float seconds = 5f;
            var w = new WaitForEndOfFrame();

            while (seconds > 0f)
            {
                pos.y += yInc;
                boss.transform.position = pos;
                seconds -= Time.deltaTime;
                yield return w;
            }

            gameData.StartGame();
        }

        private void LoseGame()
        {
            fader.FadeOut(() => { gameData.StartGame(); });
        }

        private void PlayScan()
        {
            float scanLength = rayCastDistance;
            positionOfInterest = levelCreator.Scan(cam.transform.position, cam.transform.forward, ref scanLength);
        }

        private void Shuffle(int[] ar)
        {
            for (int i = 0; i < ar.Length; i++)
            {
                int tempVal = ar[i];
                int randomIx = Random.Range(0, ar.Length);
                ar[i] = ar[randomIx];
                ar[randomIx] = tempVal;
            }
        }
    }
}
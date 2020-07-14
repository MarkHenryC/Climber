﻿using System.Collections;
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
        public GameObject boss;
        public float curveHeight = 1f;
        public GameObject templateNPC;
        public GameData gameData;
        public int npcProbability;
        public Fader fader;

        private Coroutine currentTravelRoutine;
        private Camera cam;
        private ChaikinsPoints curve;
        PositionData positionOfInterest;

        void Start()
        {
            cam = Camera.main;
            curve = new ChaikinsPoints();
            camController.ButtonUp = HandleTrigger;

            gameData.StartGameAction = CreateLandscape;
            gameData.PlayerDeathNotification = LoseGame;
            gameData.StartGame();
        }

        void Update()
        {
            if (gameData.Playing)
                positionOfInterest = levelCreator.Scan(cam.transform.position, cam.transform.forward, rayCastDistance);
        }

        private void CreateLandscape()
        {
            fader.FadeIn();

            gameData.ReturnObjects();

            levelCreator.CreateLandscapeMesh();
            levelCreator.SetObjectAt(player.transform, PositionData.OccupantType.Player, levelCreator.LowestPanel);
            levelCreator.SetObjectAt(boss.transform, PositionData.OccupantType.Boss, levelCreator.HighestPanel);

            int[] emptyIndices = levelCreator.GetEmptyPositions();
            Shuffle(emptyIndices);

            for (int i = 0; i< emptyIndices.Length; i++)
            {
                if (i % npcProbability == 0)
                    levelCreator.CreateObjectAt(templateNPC, PositionData.OccupantType.NPC, emptyIndices[i]);
            }
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
                    player.transform.position = Vector3.Lerp(points[i], points[i + 1], t);
                    t += incr * Time.deltaTime * moveSpeed;
                    yield return wait;
                }
            }

            currentTravelRoutine = null;
        }

        private IEnumerator WinGame(bool won = true)
        {
            float yInc = (won ? -.1f : .1f);
            Vector3 pos = boss.transform.position;
            float seconds = 2f;
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
            positionOfInterest = levelCreator.Scan(cam.transform.position, cam.transform.forward, rayCastDistance);
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
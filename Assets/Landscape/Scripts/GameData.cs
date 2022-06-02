using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuiteSensible
{
    [CreateAssetMenu()]
    public class GameData : ScriptableObject
    {
        public float startPlayerHealth = 100f;
        public PlayerUi player 
        { 
            get { return _player; }
            set { _player = value; } 
        }

        private float playerHealth;
        private int wins, losses;
        public enum EnLevelStatus { Starting, Playing, Finished };
        public enum EnPlayStatus { None, Won, Lost };

        public EnLevelStatus LevelStatus;
        public EnPlayStatus PlayStatus;

        private readonly Stack<GameObject> objectsAvailable = new Stack<GameObject>();
        private readonly Stack<GameObject> objectsInUse = new Stack<GameObject>();
        private PlayerUi _player;

        private void Awake()
        {
            LevelStatus = EnLevelStatus.Starting;
            playerHealth = startPlayerHealth;
        }

        public void TakePlayerHealth(float deltaTime, float magnitude)
        {
            playerHealth -= deltaTime * magnitude;
            PlayerHealthNotification?.Invoke(Mathf.Clamp(playerHealth, 0f, startPlayerHealth) / startPlayerHealth);

            if (playerHealth <= 0f)
            {
                Debug.Log("Player DEAD");

                LostGame();
            }
        }

        public void PlayerUiText(string text)
        {
            if (_player)
                _player.SetText(text);
        }

        public void StartGame()
        {
            playerHealth = startPlayerHealth;

            PlayStatus = EnPlayStatus.None;
            LevelStatus = EnLevelStatus.Playing;

            StartGameAction?.Invoke();
        }

        public void WonGame()
        {
            Debug.Log("Game WON");

            wins++;
            LevelStatus = EnLevelStatus.Finished;
            PlayStatus = EnPlayStatus.Won;
        }

        public void LostGame()
        {
            Debug.Log("Game LOST");

            losses++;
            LevelStatus = EnLevelStatus.Finished;
            PlayStatus = EnPlayStatus.Lost;
            PlayerDeathNotification?.Invoke();
        }

        public bool Playing
        {
            get
            {
                return LevelStatus == EnLevelStatus.Playing &&
                  PlayStatus == EnPlayStatus.None;
            }
        }

        public float PlayerHealth => playerHealth;
        public bool PlayerAlive => playerHealth > 0f;
        public int Wins => wins;
        public int Losses => losses;

        public System.Action StartGameAction;
        public System.Action PlayerDeathNotification;
        public System.Action<float> PlayerHealthNotification;

        public GameObject GetObject(GameObject template)
        {
            GameObject ob;

            if (objectsAvailable.Count > 0)
                ob = objectsAvailable.Pop();
            else
                ob = GameObject.Instantiate(template);

            objectsInUse.Push(ob);
            ob.SetActive(true);
            return ob;
        }

        public void ReturnObjects()
        {
            while (objectsInUse.Count > 0)
            {
                GameObject ob = objectsInUse.Pop();
                ob.SetActive(false);
                objectsAvailable.Push(ob);
            }
        }
    }
}
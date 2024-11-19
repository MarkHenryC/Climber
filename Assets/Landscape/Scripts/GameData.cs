using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuiteSensible
{
    [CreateAssetMenu()]
    public class GameData : ScriptableObject
    {
        [SerializeField]
        float startPlayerHealth = 100f;

        public PlayerUi player 
        { 
            get { return _player; }
            set { _player = value; } 
        }

        private float _playerHealth;
        private int _wins, _losses;
        public enum LevelStatus { Starting, Playing, Finished };
        public enum PlayStatus { None, Won, Lost };

        private LevelStatus _levelStatus;
        private PlayStatus _playStatus;

        private readonly Stack<GameObject> objectsAvailable = new Stack<GameObject>();
        private readonly Stack<GameObject> objectsInUse = new Stack<GameObject>();
        private PlayerUi _player;

        private void Awake()
        {
            _levelStatus = LevelStatus.Starting;
            _playerHealth = startPlayerHealth;
        }

        public void TakePlayerHealth(float deltaTime, float magnitude)
        {
            _playerHealth -= deltaTime * magnitude;
            PlayerHealthNotification?.Invoke(Mathf.Clamp(_playerHealth, 0f, startPlayerHealth) / startPlayerHealth);

            if (_playerHealth <= 0f)
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
            _playerHealth = startPlayerHealth;

            _playStatus = PlayStatus.None;
            _levelStatus = LevelStatus.Playing;

            StartGameAction?.Invoke();
        }

        public void WonGame()
        {
            Debug.Log("Game WON");

            _wins++;
            _levelStatus = LevelStatus.Finished;
            _playStatus = PlayStatus.Won;
        }

        public void LostGame()
        {
            Debug.Log("Game LOST");

            _losses++;
            _levelStatus = LevelStatus.Finished;
            _playStatus = PlayStatus.Lost;
            PlayerDeathNotification?.Invoke();
        }

        public bool Playing
        {
            get
            {
                return _levelStatus == LevelStatus.Playing &&
                  _playStatus == PlayStatus.None;
            }
        }

        public float PlayerHealth => _playerHealth;
        public bool PlayerAlive => _playerHealth > 0f;
        public int Wins => _wins;
        public int Losses => _losses;

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
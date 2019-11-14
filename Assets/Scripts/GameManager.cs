using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Text _gameTimer;

    [SerializeField]
    private GameObject _mainMenu;

    [SerializeField]
    private GameObject _enemies;

    [SerializeField]
    private GameObject _character;

    [SerializeField]
    private GameObject _endGameModal;

    [SerializeField]
    private Text _endGameTime;

    [SerializeField]
    private Text _endGameEnemies;

    [SerializeField]
    private Text _endGameHits;

    [SerializeField]
    private GameObject _pauseMenu;

    private float _gameTime = 0;

    private bool _gameActive;

    public bool gameActive { get { return _gameActive; } }

    public static GameManager Instance;

    private Coroutine _gameTimerCR;

    private int _enemiesKilled = 0;
    private int _hitsTaken = 0;

    private bool _isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _isPaused = !_isPaused;
                if (_isPaused)
                {
                    UnityEngine.Cursor.visible = true;
                    _pauseMenu.SetActive(true);
                    Time.timeScale = 0;
                }
                else
                {
                    UnityEngine.Cursor.visible = false;
                    _pauseMenu.SetActive(false);
                    Time.timeScale = 1;
                }
                
            }
        }
    }

    public void ResetTimescale()
    {
        Time.timeScale = 1;
        _isPaused = false;
    }

    public void KilledEnemy()
    {
        _enemiesKilled++;
    }

    public void GotHit()
    {
        _hitsTaken++;
    }

    public void StartGame()
    {
        //Start the timer and remove main menu
        _mainMenu.SetActive(false);
        _gameActive = true;

        _enemies.SetActive(true);
        _character.SetActive(true);
        UnityEngine.Cursor.visible = false;
        if (_gameTimerCR != null)
        {
            StopCoroutine(_gameTimerCR);
        }
        _gameTimerCR = StartCoroutine(StartTimer());
    }

    public void EndGame()
    {
        _endGameTime.text = _gameTimer.text;
        _endGameEnemies.text = _enemiesKilled.ToString();
        _endGameHits.text = _hitsTaken.ToString();

        _enemies.SetActive(false);
        _character.SetActive(false);
        UnityEngine.Cursor.visible = true;
        if (_gameTimerCR != null)
        {
            StopCoroutine(_gameTimerCR);
        }

        _endGameModal.SetActive(true);
    }

    private IEnumerator StartTimer()
    {
        _gameTime = 0;
        while (_gameActive)
        {
            _gameTime += Time.deltaTime;
            string timerString = _gameTime.ToString("F2");
            if(_gameTime <= 60)
            {
                timerString = _gameTime.ToString("F2") + "s";
            }else if(_gameTime > 60)
            {
                float timeInMinutes = _gameTime / 60;
                timerString = timeInMinutes.ToString("F2") + " m";
            }else if(_gameTime > 3600)
            {
                float timeInHours = _gameTime / 60 / 60;
                timerString = timeInHours.ToString("F2") + "h";
            }
            _gameTimer.text = timerString;
            yield return new WaitForEndOfFrame();
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(0); 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

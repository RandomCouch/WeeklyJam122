using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Button _restartButton;
    // Start is called before the first frame update
    void Start()
    {
        if(_restartButton != null)
        {
            _restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }

    private void OnRestartButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

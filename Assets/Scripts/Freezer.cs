using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freezer : MonoBehaviour {

    public static Freezer Instance;

    private bool _canFreezeTime = true;
    private float _freezeTimeCooldown = 0.02f;

	// Use this for initialization
	void Awake () {
		if(Instance == null)
        {
            Instance = this;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Freeze()
    {
        StartCoroutine(FreezeTime());
    }

    private IEnumerator FreezeTime()
    {
        if (_canFreezeTime)
        {

            _canFreezeTime = false;
            //Time.timeScale = 2f;
            //yield return new WaitForSecondsRealtime(0.05f);

            Time.timeScale = 0.05f;

            yield return new WaitForSecondsRealtime(0.3f);

            Time.timeScale = 1f;
            StartCoroutine(FreezeTimeCooldown());
            //_canFreezeTime = true;
        }
    }

    private IEnumerator FreezeTimeCooldown()
    {
        yield return new WaitForSecondsRealtime(_freezeTimeCooldown);
        _canFreezeTime = true;
    }
}

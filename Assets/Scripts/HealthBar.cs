using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    [SerializeField]
    private Image _healthBar;
	
	public void SetBar(float t)
    {
        _healthBar.fillAmount = t;
    }
}

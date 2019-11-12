using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField]
    private GameObject _damageText;

    [SerializeField]
    private GameObject _healthBarPrefab;

    [SerializeField]
    private Text _damageIndicatorText;

    [SerializeField]
    private Transform _player;

    public static UIManager Instance;

    private RectTransform _rect;
    private Dictionary<RectTransform, Transform> _healthBars;
    private bool _damageIndicatorVisible = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        _rect = GetComponent<RectTransform>();
        _healthBars = new Dictionary<RectTransform, Transform>();
    }

    public void SpawnDamageText(Vector3 targetPos, float damage)
    {
        Vector2 screenPos = Camera.main.WorldToViewportPoint(targetPos);
        Vector2 damagePos = new Vector2(((screenPos.x * _rect.sizeDelta.x) - (_rect.sizeDelta.x * 0.5f)), ((screenPos.y * _rect.sizeDelta.y) - (_rect.sizeDelta.y * 0.5f)));
        //screenPos.y += 5f;
        damagePos.y -= 500f;
        GameObject damageText = Instantiate(_damageText);
        damageText.transform.SetParent(transform);
        damageText.transform.localScale = Vector3.one;
        damageText.transform.position = Vector3.zero;
        RectTransform damageRect = damageText.GetComponent<RectTransform>();
        damageRect.anchoredPosition = damagePos;

        DamageText dmgScript = damageText.GetComponent<DamageText>();
        string damageTextString = ((int)damage).ToString();
        dmgScript.SetText(damageTextString); ;
    }

    public void SpawnHealthbar(Transform entity, float fillAmount)
    {
        GameObject healthBar = Instantiate(_healthBarPrefab);
        healthBar.transform.SetParent(transform);
        healthBar.transform.localScale = Vector3.one;
        healthBar.transform.position = Vector3.zero;
        RectTransform healthBarRect = healthBar.GetComponent<RectTransform>();

        Vector2 screenPos = Camera.main.WorldToViewportPoint(entity.position);
        Vector2 hpPos = new Vector2(((screenPos.x * _rect.sizeDelta.x) - (_rect.sizeDelta.x * 0.5f)), ((screenPos.y * _rect.sizeDelta.y) - (_rect.sizeDelta.y * 0.5f)));
        hpPos.y += 50f;
        healthBarRect.anchoredPosition = hpPos;
        HealthBar hp = healthBar.GetComponent<HealthBar>();
        hp.SetBar(fillAmount);

        _healthBars.Add(healthBarRect, entity);
        StartCoroutine(fadeHPBar(healthBarRect, 2f));
    }

    private IEnumerator fadeHPBar(RectTransform hpBar, float t)
    {
        yield return new WaitForSeconds(t);
        Animator hpAnim = hpBar.GetComponent<Animator>();
        hpAnim.SetTrigger("Fade");
        yield return new WaitForSeconds(0.5f);
        _healthBars.Remove(hpBar);
        Destroy(hpBar.gameObject);
    }

    public void ReloadDamageIndicatorCharge()
    {
        Animator dmgAnim = _damageIndicatorText.gameObject.GetComponent<Animator>();
        dmgAnim.SetTrigger("ReloadCharge");
    }

    public void UpdateDamageIndicator(string text)
    {
        _damageIndicatorText.text = text;
        Animator dmgAnim = _damageIndicatorText.gameObject.GetComponent<Animator>();
        if(!_damageIndicatorVisible)
        {
            dmgAnim.SetBool("Charging", true);
            _damageIndicatorVisible = true;
        }
    }

    public void FadeOutDamageIndicator()
    {
        Animator dmgAnim = _damageIndicatorText.gameObject.GetComponent<Animator>();
        if (_damageIndicatorVisible)
        {
            dmgAnim.SetBool("Charging", false);
            _damageIndicatorVisible = false;
        }
    }

    private void Update()
    {
        if(_healthBars.Count > 0)
        {
            foreach (KeyValuePair<RectTransform, Transform> healthBar in _healthBars)
            {
                if (healthBar.Key != null && healthBar.Value != null)
                {
                    Vector2 screenPos = Camera.main.WorldToViewportPoint(healthBar.Value.transform.position);
                    Vector2 hpPos = new Vector2(((screenPos.x * _rect.sizeDelta.x) - (_rect.sizeDelta.x * 0.5f)), ((screenPos.y * _rect.sizeDelta.y) - (_rect.sizeDelta.y * 0.5f)));
                    hpPos.y += 120f;
                    healthBar.Key.anchoredPosition = hpPos;
                }
            }
        }

        if(_damageIndicatorText != null)
        {
            Vector2 screenPos = Camera.main.WorldToViewportPoint(_player.position);
            Vector2 dmgIndicatorPos = new Vector2(((screenPos.x * _rect.sizeDelta.x) - (_rect.sizeDelta.x * 0.5f)), ((screenPos.y * _rect.sizeDelta.y) - (_rect.sizeDelta.y * 0.5f)));
            dmgIndicatorPos.y += 180f;
            _damageIndicatorText.rectTransform.anchoredPosition = dmgIndicatorPos;
        }
    }
}

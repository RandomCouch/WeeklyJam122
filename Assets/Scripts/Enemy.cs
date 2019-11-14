using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _visionRange;

    [SerializeField]
    private float _attackRange;

    [SerializeField]
    private bool _locksOn;

    [SerializeField]
    private int _maxHP;

    [SerializeField]
    private GameObject _hitEffectPrefab;

    [SerializeField]
    private GameObject _dieEffectPrefab;

    private int _curHP;

    private Transform _player;

    private Rigidbody2D _rb;
    private bool _disabled = false;
    private SpriteRenderer _sr;

    private Color _initialColor;

    
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _initialColor = _sr.color;
        _curHP = _maxHP;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, _visionRange);
    }

    // Update is called once per frame
    void Update()
    {
        if (_disabled)
        {
            return;
        }
        if(_player != null && _locksOn)
        {
            _rb.AddForce(_player.position - transform.position);
            return;
        }
        List<Collider2D> hits = Physics2D.OverlapCircleAll(transform.position, _visionRange).ToList();
        //Debug.Log("Enemy scan : " + hits.Count + " objects");
        if(hits.Count > 0)
        {
            foreach(Collider2D hit in hits)
            {
                if (hit.tag == "Player") {
                    _player = hit.transform;
                    //Debug.Log("Enemy scan: Found player");
                    //Move towards player
                    _rb.AddForce( hit.transform.position - transform.position);
                    //_rb.MovePosition(hit.transform.position);
                }
            }
        }
    }

    public bool IsDead()
    {
        return _curHP <= 0;
    }

    public bool IsDisabled()
    {
        return _disabled;
    }

    public void TakeDamage(int damage)
    {
        Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
        _curHP -= damage;
        if(_curHP <= 0)
        {
            Die();
        }
        else
        {
            DisableEnemy(1.25f);
        }
    }


    public void DisableEnemy(float t)
    {
        StartCoroutine(DisableEnemyCR(t));
    }

    private void Die()
    {
        StartCoroutine(DieCR());
    }

    private IEnumerator DieCR()
    {
        _sr.color = Color.red;
        _disabled = true;
        _rb.gravityScale = 2f;
        yield return new WaitForSeconds(1f);
        StopAllCoroutines();
        Instantiate(_dieEffectPrefab, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayEnemyDieFX();
        GameManager.Instance.KilledEnemy();
        Destroy(gameObject);
    }

    private IEnumerator DisableEnemyCR(float t)
    {
        _disabled = true;
        _sr.color = Color.grey;
        yield return new WaitForSeconds(t);
        if(_curHP > 0)
        {
            _disabled = false;
            _sr.color = _initialColor;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScript : MonoBehaviour {
    /// <summary>
    /// Analyze swipe direction and play appropriate animations
    /// </summary>
	// Use this for initialization

    private Animator _anim;

    private Vector2 _initialTapLocation;
    private bool _charging = false;

    [SerializeField]
    private Fist _fist;

    private float _chargeTime = 0f;

    public float damage = 10f;

    public enum CharacterDirection
    {
        Right,
        Left
    }

    private CharacterDirection _dir = CharacterDirection.Right;

    public CharacterDirection direction
    {
        get
        {
            return _dir;
        }
        set
        {
            if(_dir != value)
            {
                _dir = value;
                OnDirectionChanged();
            }
        }
    }

	void Start () {
        _anim = GetComponent<Animator>();
        _fist.OnHitEnemy += OnHitEnemy;
	}

    private void OnHitEnemy(EnemyScript enemy, Vector3 hitPos)
    {
        //enemy knockback and damage based on charge time
        Debug.Log("Hit enemy with a charged punch of " + _chargeTime + " seconds");
        float damageTaken = damage * _chargeTime;
        enemy.TakeDamage(damageTaken, hitPos);
        Rigidbody2D rb2d = enemy.gameObject.GetComponent<Rigidbody2D>();
        if(enemy.curHP <= 0)
        {
            //If it was a kill punch
            rb2d.AddForceAtPosition((enemy.gameObject.transform.position - transform.position) * 20f, _fist.transform.position, ForceMode2D.Impulse);
        }
        else
        {
            rb2d.AddForceAtPosition((enemy.gameObject.transform.position - transform.position) * _chargeTime * 100f, _fist.transform.position, ForceMode2D.Impulse);
        }
        
    }

    private void OnDirectionChanged()
    {
        _chargeTime = 0f;
        switch (_dir)
        {
            case CharacterDirection.Right:
                if(transform.localEulerAngles.y != 0)
                {
                    Vector3 newAngles = transform.localEulerAngles;
                    newAngles.y = 0;
                    transform.localEulerAngles = newAngles;
                    //if was charging restart the charge
                    if (_charging)
                    {
                        _anim.SetBool("RestartCharge", true);
                    }
                }
                break;
            case CharacterDirection.Left:
                if (transform.localEulerAngles.y != 180)
                {
                    Vector3 newAngles = transform.localEulerAngles;
                    newAngles.y = 180;
                    transform.localEulerAngles = newAngles;
                    //if was charging restart the charge
                    if (_charging)
                    {
                        _anim.SetBool("RestartCharge", true);
                    }
                }
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            _initialTapLocation = Input.mousePosition;
            _chargeTime = 0f;
        }else if (Input.GetMouseButton(0))
        {
            //While holding down the button guage what level we are pulling
            float distance = Vector2.Distance(Input.mousePosition, _initialTapLocation);
            //Debug.Log("Distance from initial tap : " + distance);
            //Figure out direction
            if(Input.mousePosition.x < _initialTapLocation.x)
            {
                //Debug.Log("Pulling to the left");
                direction = CharacterDirection.Right;
            }
            else if(Input.mousePosition.x > _initialTapLocation.x)
            {
                //Debug.Log("Pulling to the right");
                direction = CharacterDirection.Left;
            }

            if(distance >= 20f)
            {
                _anim.SetBool("Charging", true);
                //_anim.SetTrigger("Charge");
                _charging = true;
                _chargeTime += Time.deltaTime;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_charging)
            {
                _charging = false;
                _anim.SetBool("Charging", false);
                _anim.SetTrigger("Punch");
            }
        }
	}
}

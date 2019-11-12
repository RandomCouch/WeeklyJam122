using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puncher : MonoBehaviour {

    [SerializeField]
    private Transform _fist;
    [SerializeField]
    private Transform _head;
    [SerializeField]
    private Transform _punchTrail;

    [SerializeField]
    private GameObject _chargeEffect;

    [SerializeField]
    private float _maxVelocity;

    [SerializeField]
    private float _punchStrength;

    [SerializeField]
    private float _baseDamage = 15f;



    private Animator _anim;
    private Rigidbody2D _rb;

    private Vector2 _initialTapLocation;
    private bool _charging = false;
    private bool _inAir = false;
    private bool _canPunch = true;
    private bool _canAirPunch = true;

    private float _chargeTime = 0f;

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
            if (_dir != value)
            {
                _dir = value;
                OnDirectionChanged();
                if(_chargeTime >= 0.25f)
                {
                    _anim.ResetTrigger("ReloadCharge");
                    _anim.SetTrigger("ReloadCharge");
                }
                _chargeTime = 0.1f;
                
                Debug.Log("Set trigger reload charge");
                UIManager.Instance.ReloadDamageIndicatorCharge();
            }
        }
    }
    // Use this for initialization
    void Start () {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _fist.gameObject.GetComponent<Fist>().OnHitEnemy += OnEnemyHit;
        _punchTrail.gameObject.SetActive(false);
	}
	
    void OnEnemyHit(EnemyScript enemy, Vector3 hitPos)
    {
        if(enemy != null)
        {
            Rigidbody2D rb2d = enemy.gameObject.GetComponent<Rigidbody2D>();
            Vector3 currentVelocity = _rb.velocity;
            rb2d.velocity = currentVelocity * 10.75f;

            float DamageDealt = _baseDamage * _chargeTime;
            enemy.TakeDamage(DamageDealt, hitPos);
            if (enemy.curHP <= 0)
            {
                //If it was a kill punch
                //rb2d.AddForceAtPosition((enemy.gameObject.transform.position - transform.position) * 20f, _fist.transform.position, ForceMode2D.Impulse);
                
            }
            else
            {
                //rb2d.AddForceAtPosition((enemy.gameObject.transform.position - transform.position) * 100f, _fist.transform.position, ForceMode2D.Impulse);

                
                rb2d.velocity = currentVelocity * 10;
                rb2d.AddForceAtPosition(currentVelocity.normalized * _chargeTime * _punchStrength * 150f, hitPos);
                rb2d.angularVelocity = currentVelocity.magnitude * 50f;
            }
            currentVelocity -= currentVelocity * 0.15f;
            //_rb.velocity = currentVelocity;
            
            
        }
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && _canPunch)
        {
            _initialTapLocation = Input.mousePosition;
            _chargeTime = 0.1f;
        }
        else if (Input.GetMouseButton(0) && _charging)
        {
            if (Input.mousePosition.x < _initialTapLocation.x)
            {
                direction = CharacterDirection.Left;
            }
            else if(Input.mousePosition.x > _initialTapLocation.x)
            {
                direction = CharacterDirection.Right;
            }
        }else if (Input.GetMouseButtonUp(0))
        {

        }

        if (_rb.velocity.magnitude >= _maxVelocity)
        {
            //_rb.velocity = _rb.velocity.normalized * _maxVelocity;
        }

        if(_rb.velocity.y < 0)
        {
            _rb.gravityScale = 5.5f;
        }else
        {
            _rb.gravityScale = 2f;
        }


        
    }

	// Update is called once per frame
	void LateUpdate () {
        Vector3 mousePos;
        if (( Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) && _canPunch)
        {
#if UNITY_EDITOR
            _initialTapLocation = Input.mousePosition;
#else
            _initialTapLocation = Input.GetTouch(0).position;
#endif

            _charging = true;
            _canPunch = false;
            _chargeTime = 0.1f;
            _chargeEffect.SetActive(true);
            _anim.ResetTrigger("ReloadCharge");
            if (_inAir && _canAirPunch)
            {
                _canAirPunch = false;
            }
        }else if ((Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary))) && _charging)
        {
            //Charging
#if UNITY_EDITOR
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _head.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1f), _initialTapLocation - (Vector2)Input.mousePosition);
#else
            mousePos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            _head.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1f), _initialTapLocation - Input.GetTouch(0).position);
#endif

            Vector3 newHeadAngles = _head.localEulerAngles;
            //newHeadAngles.z -= 90f;
            //Vector3 newFistAngles = _fist.localEulerAngles;

            _chargeTime += Time.deltaTime;
            if (direction == CharacterDirection.Left)
            {
                //Debug.Log("Pulling to the left");
                
                newHeadAngles.y += 180f;
                //newHeadAngles.x -= 180f;
                newHeadAngles.z *= -1;
                //newFistAngles.x += 180f;

                //newFistAngles.z *= -1;
                //newFistAngles.z += 180f;
                //Debug.Log("Changed direction to left");
            }
            else
            {
                //Debug.Log("Pulling to the right");
                
                //Debug.Log("Changed direction to right");
            }

            float DamageDealt = _baseDamage * _chargeTime;
            UIManager.Instance.UpdateDamageIndicator(((int)DamageDealt).ToString());

            _head.localEulerAngles = newHeadAngles;
            //_fist.localEulerAngles = newFistAngles;

        }else if ((Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)) && _charging && _chargeTime > 0.1f)
        {
            //Punch
#if UNITY_EDITOR
            transform.rotation = Quaternion.LookRotation(Vector3.forward, _initialTapLocation - (Vector2)Input.mousePosition);
#else
            transform.rotation = Quaternion.LookRotation(Vector3.forward, _initialTapLocation - Input.GetTouch(0).position);
#endif

            Vector3 newAngles = transform.eulerAngles;
            newAngles.z -= 90f;

            if (direction == CharacterDirection.Left)
            {
                newAngles.y += 180f;
                newAngles.z *= -1f;
                newAngles.z += 180f;
            }

            transform.eulerAngles = newAngles;
            _charging = false;

            Vector3 chargeMovement = transform.TransformDirection(Vector3.left) * (_punchStrength * _chargeTime + 15);
            //_rb.AddForce(chargeMovement, ForceMode2D.Impulse);
            //StartCoroutine(ResetRotation(0.5f));
            StartCoroutine(Charge(chargeMovement));
            _chargeEffect.SetActive(false);
            if(_chargeTime >= 3f)
            {
                _punchTrail.gameObject.SetActive(true);
            }
            UIManager.Instance.FadeOutDamageIndicator();
            _anim.SetTrigger("Punch");
            StartCoroutine(ReloadPunch(0.15f));
        }

       

        if (transform.position.y > -3f)
        {
            _inAir = true;
        }
        else
        {
            _inAir = false;
            _canAirPunch = true;
        }

        if(_inAir && _charging)
        {
            Vector3 newVelocity = _rb.velocity;
            newVelocity.y = newVelocity.y * 0.85f;
            _rb.velocity = newVelocity;

            _rb.gravityScale = 0.5f;
        }

        _anim.SetBool("Charging", _charging);
        _anim.SetBool("inAir", _inAir);
        

        _punchTrail.localPosition = _fist.localPosition;

        
    }

    private IEnumerator Charge(Vector3 chargeMovement)
    {
        yield return new WaitForSecondsRealtime(0.05f);
        _rb.AddForce(chargeMovement, ForceMode2D.Impulse);
        StartCoroutine(ResetRotation(0.5f));
    }

    private IEnumerator ReloadPunch(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime);
        _canPunch = true;
    }

    private IEnumerator ResetRotation(float t)
    {
        yield return new WaitForSeconds(0.4f);
        float elapsedTime = 0;
        Vector3 newAngles = transform.eulerAngles;
        newAngles.z = 0;
        while (elapsedTime < t)
        {
            //transform.eulerAngles = Vector3.Lerp(startingAngles, newAngles, (elapsedTime / t));
            float angle = Mathf.LerpAngle(transform.eulerAngles.z, 0, (elapsedTime / t));
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angle);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.eulerAngles = newAngles;
    }
    

    private void OnDirectionChanged()
    {
        switch (_dir)
        {
            case CharacterDirection.Right:
                if (transform.eulerAngles.y != 0)
                {
                    Vector3 newAngles = transform.eulerAngles;
                    newAngles.y = 0;
                    transform.eulerAngles = newAngles;
                }
                break;
            case CharacterDirection.Left:
                if (transform.eulerAngles.y != 180)
                {
                    Vector3 newAngles = transform.eulerAngles;
                    newAngles.y = 180;
                    transform.eulerAngles = newAngles;
                }
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            //_inAir = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            //_inAir = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            //_inAir = true;
        }
    }
}

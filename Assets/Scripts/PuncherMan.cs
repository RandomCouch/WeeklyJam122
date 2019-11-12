using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PuncherMan : MonoBehaviour
{
    private bool _canPunch = true;
    private Vector2 _initialTapLocation;
    private float _chargeTime;
    private bool _charging;
    [SerializeField]
    private bool _inAir = true;

    private bool _onWall = false;

    [SerializeField]
    private CharState _state;

    [SerializeField]
    private Transform _cursor;

    [SerializeField]
    private Transform _cursorTarget;

    
    public CharState state
    {
        get
        {
            return _state;
        }
        set
        {
            if(state != value)
            {
                _state = value;
                OnStateChanged();
            }
        }
    }

    private CharacterDirection _dir = CharacterDirection.Right;

    [SerializeField]
    private Transform _torso;

    [SerializeField]
    private Transform _forearm;

    [SerializeField]
    private float _punchStrength;

    private Animator _anim;
    private Rigidbody2D _rb;

    [SerializeField]
    private float _rotateCharge = 0f;

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
                
                if (_chargeTime >= 0.25f)
                {
                    //_anim.ResetTrigger("ReloadCharge");
                    //_anim.SetTrigger("ReloadCharge");
                }
                _chargeTime = 0f;

                //Debug.Log("Set trigger reload charge");
                //UIManager.Instance.ReloadDamageIndicatorCharge();
            }
            OnDirectionChanged();
        }
    }

    void Start()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnStateChanged()
    {
        switch (_state)
        {
            case CharState.Idle:
                _anim.SetBool("Charging", false);
                _anim.SetBool("Falling", false);
                break;
            case CharState.Charging:
                _anim.SetBool("Charging", true);
                _anim.SetBool("Falling", false);
                break;
            case CharState.Punching:
                _anim.SetTrigger("Punch");
                _anim.SetBool("Charging", false);
                _anim.SetBool("Falling", false);
                break;
            case CharState.Falling:
                _anim.SetBool("Charging", false);
                _anim.SetBool("Falling", true);
                break;
            case CharState.Landing:
                _anim.SetTrigger("Land");
                _anim.SetBool("Charging", false);
                _anim.SetBool("Falling", false);
                break;
        }
    }

    private void OnDirectionChanged()
    {
        //Debug.Log("Switching direction to : " + _dir.ToString());
        switch (_dir)
        {
            case CharacterDirection.Right:
                if (transform.localEulerAngles.y != 0)
                {
                    Vector3 newAngles = transform.localEulerAngles;
                    newAngles.y = 0;
                    if (!_onWall)
                    {
                        transform.localEulerAngles = newAngles;
                    }
                    else
                    {
                        Vector3 newScale = new Vector3(1, -1, 1);
                        //_torso.localScale = newScale;
                    }
                    transform.localEulerAngles = newAngles;
                    //if was charging restart the charge
                    if (_charging)
                    {
                        //_anim.SetBool("RestartCharge", true);
                    }
                }
                break;
            case CharacterDirection.Left:
                if (transform.localEulerAngles.y != 180)
                {
                    Vector3 newAngles = transform.localEulerAngles;
                    newAngles.y = 180;
                    
                    if (!_onWall)
                    {
                        transform.localEulerAngles = newAngles;
                    }
                    else
                    {
                        Vector3 newScale = new Vector3(1, -1, 1);
                        //_torso.localScale = newScale;
                        //_torso.localEulerAngles = newAngles;
                    }
                    transform.localEulerAngles = newAngles;
                    //if was charging restart the charge
                    if (_charging)
                    {
                        //_anim.SetBool("RestartCharge", true);
                    }
                }
                break;
        }
    }

    // Start is called before the first frame update
    

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && (_state == CharState.Idle || _state == CharState.Falling))
        {
            state = CharState.Charging;
            _initialTapLocation = Input.mousePosition;
            _chargeTime = 0f;
            _rotateCharge = 0f;
        }
        else if (Input.GetMouseButton(0) && _state == CharState.Charging)
        {
            if (_cursor.position.x < _cursorTarget.position.x)
            {
                //Debug.Log("Changing direction to RIGHT");
                direction = CharacterDirection.Right;
            }
            else if (_cursor.position.x > _cursorTarget.position.x)
            {
                //Debug.Log("Changing direction to LEFT");
                direction = CharacterDirection.Left;
            }
        }
        else if (Input.GetMouseButtonUp(0) && _state == CharState.Charging)
        {
            state = CharState.Punching;
        }

        if(state == CharState.Punching && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Char_Punch"))
        {
            //Debug.Log("I just Finished punching");
            StartCoroutine(CheckIfInAir());
        }

        if(state == CharState.Falling && !_inAir)
        {
            state = CharState.Landing;
        }
        if(state == CharState.Landing && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Char_Land"))
        {
            state = CharState.Idle;
        }
    }

    private IEnumerator CheckIfInAir()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        if (_inAir)
        {
            //Debug.Log("I'm falling after punching!");
            state = CharState.Falling;
        }
        else
        {
            //Debug.Log("I'm on the ground after punching!");
            state = CharState.Idle;
        }
    }

    void LateUpdate()
    {
        if ((Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) && _canPunch)
        {
#if UNITY_EDITOR
            _initialTapLocation = Input.mousePosition;
#else
            _initialTapLocation = Input.GetTouch(0).position;
#endif

            _charging = true;
            _canPunch = false;
            _chargeTime = 0f;
            _rotateCharge = 0f;
            //_chargeEffect.SetActive(true);
            //_anim.ResetTrigger("ReloadCharge");
            /*
            if (_inAir && _canAirPunch)
            {
                _canAirPunch = false;
            }
            */
        }
        else if ((Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary))) && _charging)
        {
            //Charging
            //Debug.Log("Rotating torso");

            Vector2 difference = (Vector2)_cursor.position  - (Vector2)_cursorTarget.position;
            float sign = (((Vector2)_cursor.position).y < ((Vector2)_cursorTarget.position).y) ? -1.0f : 1.0f;
            float angle = Vector2.Angle(Vector2.right, difference) * sign;

            Vector3 newHeadAngles = _torso.localEulerAngles;

            _chargeTime += Time.deltaTime;

            newHeadAngles.z = -angle;
            
            if (direction == CharacterDirection.Left)
            {
                
            }
            else
            {
                newHeadAngles.z *= -1;
                newHeadAngles.z += 180f;
            }

            float DamageDealt = 15 * _chargeTime;
            //UIManager.Instance.UpdateDamageIndicator(((int)DamageDealt).ToString());

            _torso.localEulerAngles = newHeadAngles;

            Vector3 forearmAngles = _forearm.localEulerAngles;
            forearmAngles.z += 50 + Time.smoothDeltaTime;
            //_forearm.localEulerAngles += forearmAngles * _chargeTime;
            _rotateCharge += Mathf.Pow(15, _chargeTime);

            _forearm.Rotate(0, 0, _rotateCharge);

        }
        else if ((Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)) && _charging && _chargeTime > 0.1f)
        {
            //Punch
            Vector2 difference = (Vector2)_cursor.position - (Vector2)_cursorTarget.position;
            float sign = (((Vector2)_cursor.position).y < ((Vector2)_cursorTarget.position).y) ? -1.0f : 1.0f;
            float angle = Vector2.Angle(Vector2.right, difference) * sign;

            Vector3 newAngles = transform.eulerAngles;

            newAngles.z = -angle; 

            if (direction == CharacterDirection.Left)
            {

            }
            else
            {
                newAngles.z *= -1;
                newAngles.z += 180f;
            }

            transform.eulerAngles = newAngles;
            _charging = false;

            Vector3 chargeMovement = -_torso.up * (_punchStrength * _chargeTime + 15);
            Debug.DrawRay(_torso.position, chargeMovement, Color.green);
            //_rb.AddForce(chargeMovement, ForceMode2D.Impulse);
            //StartCoroutine(ResetRotation(0.5f));
            StartCoroutine(Charge(chargeMovement));
            /*
            _chargeEffect.SetActive(false);
            if (_chargeTime >= 3f)
            {
                _punchTrail.gameObject.SetActive(true);
            }
            UIManager.Instance.FadeOutDamageIndicator();
            _anim.SetTrigger("Punch");
            */
            StartCoroutine(ReloadPunch(0.5f));
        }



        if (transform.position.y > -3f)
        {
           // _inAir = true;
        }
        else
        {
           // _inAir = false;
           // _canAirPunch = true;
        }
        
        if (_inAir && _charging)
        {
            Vector3 newVelocity = _rb.velocity;
            newVelocity.y = newVelocity.y * 0.85f;
            _rb.velocity = newVelocity;

            _rb.gravityScale = 0.5f;
        }else if (!_inAir || !_charging)
        {
            _rb.gravityScale = 2f;
        }
        
        //_anim.SetBool("Charging", _charging);
        //_anim.SetBool("inAir", _inAir);


        //_punchTrail.localPosition = _fist.localPosition;
        

    }


    private IEnumerator Charge(Vector3 chargeMovement)
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.enabled = false;
        _rb.isKinematic = false;
        yield return new WaitForSecondsRealtime(0.05f);
        yield return new WaitForEndOfFrame();
        _rb.AddForce(chargeMovement, ForceMode2D.Impulse);
        StartCoroutine(ResetRotation(0.5f));
        _rotateCharge = 0f;
        yield return new WaitForSecondsRealtime(0.25f);
        collider.enabled = true;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Walls" && state != CharState.Punching)
        {
            _rb.isKinematic = true;
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            collider.enabled = false;
            //_rb.gravityScale = 0f;
            _rb.velocity = Vector3.zero;

            _anim.SetBool("OnWall", true);
            _onWall = true;
        }
        if (collision.gameObject.tag == "Stage")
        {
            Debug.Log("I'm grounded!");
            _inAir = false;
            //_rb.gravityScale = 0f;
            //_rb.velocity = Vector3.zero;
            //_rb.bodyType = RigidbodyType2D.Kinematic;
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Walls")
        {
            //_rb.velocity *= 0.25f;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Walls")
        {
            _rb.gravityScale = 2f;
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            collider.enabled = true;
            _onWall = false;
            _anim.SetBool("OnWall", false);
        }
        if (collision.gameObject.tag == "Stage")
        {
            Debug.Log("I'm in the air!!");
            _inAir = true;
            //_rb.gravityScale = 2f;
            //_rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

}

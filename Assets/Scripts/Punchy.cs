using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharState
{
    Idle,
    Charging,
    Punching,
    Falling,
    Landing
}

public enum CharacterDirection
{
    Right,
    Left
}

public class Punchy : MonoBehaviour
{
    [SerializeField]
    private Transform _arm;

    [SerializeField]
    private Transform _cursor;

    [SerializeField]
    private Transform _cursorTarget;

    [SerializeField]
    private CharState _state;
    public CharState state
    {
        get
        {
            return _state;
        }
        set
        {
            if (state != value)
            {
                _state = value;
                OnStateChanged();
            }
        }
    }

    private CharacterDirection _dir;
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
            }
            OnDirectionChanged();
        }
    }

    private Animator _anim;
    [SerializeField]
    private bool _inAir = true;
    private Rigidbody2D _rb;
    private bool _charging;
    private bool _canPunch = true;
    private bool _maintainPosition;
    private Vector3 _punchAngles;

    private void OnStateChanged()
    {
        switch (_state)
        {
            case CharState.Idle:
                _anim.SetBool("Charging", false);
                _anim.SetBool("Falling", false);
                _anim.SetBool("Punch", false);
                _maintainPosition = false;
                break;
            case CharState.Charging:
                _anim.SetBool("Charging", true);
                _anim.SetBool("Falling", false);
                break;
            case CharState.Punching:
                _anim.SetBool("Punch", true);
                _anim.SetBool("Charging", false);
                _anim.SetBool("Falling", false);
                _maintainPosition = true;
                break;
            case CharState.Falling:
                _anim.SetBool("Punch", false);
                _anim.SetBool("Charging", false);
                _anim.SetBool("Falling", true);
                _maintainPosition = false;
                break;
            case CharState.Landing:
                _anim.SetTrigger("Land");
                _anim.SetBool("Charging", false);
                _anim.SetBool("Falling", false);
                _anim.SetBool("Punch", false);
                break;
        }
    }

    private void OnDirectionChanged()
    {
        Debug.Log("Switching direction to : " + _dir.ToString());
        Vector3 newScale;
        Vector3 newArmAngles;
        switch (_dir)
        {
            case CharacterDirection.Right:
                newScale = transform.localScale;
                newScale.x = 2;
                transform.localScale = newScale;
                newArmAngles = _arm.localEulerAngles;
                newArmAngles.y = 0;
                _arm.localEulerAngles = newArmAngles;
                break;
            case CharacterDirection.Left:
                newScale = transform.localScale;
                newScale.x = -2;
                transform.localScale = newScale;
                newArmAngles = _arm.localEulerAngles;
                newArmAngles.y = 180;
                _arm.localEulerAngles = newArmAngles;
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _state = CharState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && (_state == CharState.Idle || _state == CharState.Falling))
        {
            state = CharState.Charging;
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

        if (state == CharState.Punching && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Punch"))
        {
            //Debug.Log("I just Finished punching");
            StartCoroutine(CheckIfInAir());
        }

        if (state == CharState.Falling && !_inAir)
        {
            state = CharState.Landing;
        }
        if (state == CharState.Landing && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Land"))
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
            _charging = true;
            _canPunch = false;
            Debug.Log("Starting punch");
        }
        else if ((Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary))) && _charging)
        {

            Vector2 difference = (Vector2)_cursor.position - (Vector2)_cursorTarget.position;
            float sign = (((Vector2)_cursor.position).y < ((Vector2)_cursorTarget.position).y) ? -1.0f : 1.0f;
            float angle = Vector2.Angle(Vector2.right, difference) * sign;

            Vector3 newHeadAngles = _arm.localEulerAngles;
            newHeadAngles.z = angle + 180;
            
            _arm.localEulerAngles = newHeadAngles;
        }
        else if ((Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)) && _charging)
        {
            //Punch
            Vector2 difference = (Vector2)_cursor.position - (Vector2)_cursorTarget.position;
            float sign = (((Vector2)_cursor.position).y < ((Vector2)_cursorTarget.position).y) ? -1.0f : 1.0f;
            float angle = Vector2.Angle(Vector2.right, difference) * sign;

            Vector3 newAngles = transform.localEulerAngles;

            newAngles.z = angle;

            if (direction == CharacterDirection.Left)
            {

            }
            else
            {
               // newAngles.z *= -1;
                //newAngles.z += 180f;
            }

            _punchAngles = newAngles;
            _charging = false;
            newAngles.z += 180;
            
            transform.eulerAngles = newAngles;
            Vector3 chargeMovement = transform.right * 20;
            Debug.DrawRay(_arm.position, chargeMovement, Color.green);
            StartCoroutine(Charge(chargeMovement));
            StartCoroutine(ReloadPunch(0.5f));
        }
        

        if (_inAir && _charging)
        {
            Vector3 newVelocity = _rb.velocity;
            newVelocity.y = newVelocity.y * 0.85f;
            _rb.velocity = newVelocity;

            _rb.gravityScale = 0.5f;
        }
        else if (!_inAir || !_charging)
        {
            _rb.gravityScale = 2f;
        }

        if (_maintainPosition)
        {
            Vector3 newAngles = new Vector3(0,0, _punchAngles.z);
            if (direction == CharacterDirection.Right)
            {
                newAngles.z += 180;
            }
            else
            {
            }
            transform.eulerAngles = newAngles;
        }
    }

    private IEnumerator Charge(Vector3 chargeMovement)
    {
        yield return new WaitForEndOfFrame();
        _arm.localEulerAngles = Vector3.zero;
        _rb.AddForce(chargeMovement, ForceMode2D.Impulse);
        //StartCoroutine(ResetRotation(0.5f));
        
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
        _arm.localEulerAngles = Vector3.zero;
        transform.eulerAngles = newAngles;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //check if ground
        if(collision.gameObject.tag == "Stage")
        {
            if(collision.contacts[0].point.y < transform.position.y)
            {
                //we hit the ground
                
                _inAir = false;
                _anim.SetTrigger("Land");
                if(state == CharState.Falling)
                {
                    state = CharState.Landing;
                    
                }
            }
            else
            {
                //we hit a ceiling
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //check if ground
        if (collision.gameObject.tag == "Stage")
        {
            _inAir = true;
            _anim.SetBool("Falling", true);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Transform _wallFrontDetector;

    [SerializeField]
    private Transform _wallBackDetector;

    [SerializeField]
    private SpriteRenderer _sr;

    [SerializeField]
    private SpriteRenderer _sr2;

    public static Punchy Instance;

    private Coroutine _disableCR;

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
            if (!_onWall)
            {
                OnDirectionChanged();
            }
            
        }
    }

    private bool _disabled = false;

    private Animator _anim;
    [SerializeField]
    private bool _inAir = true;
    private Rigidbody2D _rb;
    private bool _charging;
    private bool _canPunch = true;
    private bool _maintainPosition;
    private Vector3 _punchAngles;
    private Vector2 _wallSlidePos;
    private float _angle = 0f;
    [SerializeField]
    private bool _onWall;
    [SerializeField]
    private float _punchStrength;
    [SerializeField]
    private float _punchReloadSpeed;

    private void OnStateChanged()
    {
        switch (_state)
        {
            case CharState.Idle:
                //_anim.SetBool("Charging", false);
                _anim.SetBool("Falling", false);
                _anim.SetBool("Punch", false);
                _maintainPosition = false;
                _anim.ResetTrigger("Land");
                break;
            case CharState.Charging:
                _anim.SetBool("Charging", true);
                //_anim.SetBool("Falling", false);
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
                //_anim.SetBool("Charging", false);
                _anim.SetBool("Falling", false);
                //_anim.SetBool("Punch", false);
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
        if(Instance == null)
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_disabled)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && (_state != CharState.Punching) && _canPunch)
        {
            state = CharState.Charging;
        }
        else if (Input.GetMouseButton(0))
        {
            state = CharState.Charging;
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
            state = CharState.Charging;
        }
        else if (Input.GetMouseButtonUp(0) && _state == CharState.Charging)
        {
            state = CharState.Punching;
        }

        if (state == CharState.Punching && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Punch"))
        {
            //Debug.Log("I just Finished punching");
            StartCoroutine(CheckIfInAir(0.3f));
        }

        if (state == CharState.Falling && !_inAir)
        {
            state = CharState.Landing;
        }
        if (state == CharState.Landing && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Land"))
        {
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

        if (_inAir)
        {
            //Check wall
            /*
            if(Physics2D.OverlapCircle(transform.position, 10, 8))
            {
                //We are on a wall

            }
            */
        }
        else
        {
            _onWall = false;
        }

        _anim.SetBool("OnWall", _onWall);
        _anim.SetBool("Falling", _inAir);
        Debug.DrawRay(_wallFrontDetector.position, _wallFrontDetector.right, Color.blue);
        Debug.DrawRay(_wallBackDetector.position, -_wallBackDetector.right, Color.blue);

        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Stage"));

        Vector3 rayDir = _wallFrontDetector.right;
        if(direction == CharacterDirection.Left)
        {
            rayDir = -_wallFrontDetector.right;
        }

        List<Collider2D> hits = Physics2D.OverlapCircleAll(_wallFrontDetector.position, 0.25f).ToList();
        //hits.AddRange(Physics2D.RaycastAll(_wallBackDetector.position, -_wallBackDetector.right, 1f).ToList());
        if (hits.Contains(GetComponent<Collider2D>()))
        {
            hits.Remove(GetComponent<Collider2D>());
        }

        //Debug.Log("Ray hit " + hits.Count + " things");
        List<Collider2D> hitsToRemove = new List<Collider2D>();
        foreach(Collider2D hit in hits)
        {
            if (hit.transform != transform && hit.tag != "Enemy")
            {
                if (_inAir && state != CharState.Charging)
                {
                    //we wanna be stuck on this wall
                    //Debug.Log("I'm on a wall!");
                    Vector2 newVelocity = new Vector2(0, _rb.velocity.y);
                    newVelocity.y *= 0.5f;

                    _rb.velocity = newVelocity;
                    _wallSlidePos = transform.position;
                    _anim.SetBool("OnWall", true);
                    _onWall = true;
                }
            }
            else
            {
                hitsToRemove.Add(hit);
            }
        }

        foreach(Collider2D hit  in hitsToRemove)
        {
            hits.Remove(hit);
        }
        if(hits.Count == 0)
        {
            _onWall = false;
        }
        /*
        if(Physics2D.OverlapBox(_wallFrontDetector.position, new Vector2(2,2), 0f, contactFilter, results))
        {
            foreach(Collider2D result in results)
            {
                Debug.Log("Hitting " + result.name);
            }
            if (_inAir && state != CharState.Charging)
            {
                //we wanna be stuck on this wall
                Debug.Log("I'm on a wall!");
                Vector2 newVelocity = new Vector2(0, _rb.velocity.y);
                newVelocity.y *= 0.5f;

                _rb.velocity = newVelocity;
                _wallSlidePos = transform.position;
                _anim.SetBool("OnWall", true);
                _onWall = true;
            }
        }
        */
    }

    private IEnumerator CheckIfInAir(float t)
    {
        yield return new WaitForSecondsRealtime(t);
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
        if (_disabled)
        {
            return;
        }

        if (_onWall)
        {
            transform.position = new Vector2(_wallSlidePos.x, transform.position.y);
        }
        if ((Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) && _canPunch)
        {
            _charging = true;
            
            //Debug.Log("Starting punch");
        }
        else if ((Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary))) && state == CharState.Charging)
        {

            Vector2 difference = (Vector2)_cursor.position - (Vector2)_cursorTarget.position;
            float sign = (((Vector2)_cursor.position).y < ((Vector2)_cursorTarget.position).y) ? -1.0f : 1.0f;
            _angle = Vector2.Angle(Vector2.right, difference) * sign;

            Vector3 newHeadAngles = _arm.localEulerAngles;
            if (_onWall)
            {
                if(direction == CharacterDirection.Left)
                {
                    newHeadAngles.z = _angle + 360;
                }
                else
                {
                    newHeadAngles.z = -_angle + 180;
                }
                
            }
            else
            {
                newHeadAngles.z = _angle + 180;
            }
            
            
            _arm.localEulerAngles = newHeadAngles;
        }
        else if ((Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)) && state == CharState.Punching)
        {
            _canPunch = false;
            _onWall = false;
            //Punch

            Vector3 newAngles = transform.localEulerAngles;

            newAngles.z = _angle;

            if (direction == CharacterDirection.Left)
            {

            }
            else
            {
               // newAngles.z *= -1;
                //newAngles.z += 180f;
            }

            OnDirectionChanged();


            _punchAngles = newAngles;
            _charging = false;
            newAngles.z += 180;
            
            transform.eulerAngles = newAngles;
            float dragDistance = Vector3.Distance(_cursor.transform.position, _cursorTarget.transform.position);
            Debug.Log("Drag distance was :" + dragDistance);
            Vector3 chargeMovement = transform.right * dragDistance * _punchStrength;
            Debug.DrawRay(_arm.position, chargeMovement, Color.green);
            StartCoroutine(Charge(chargeMovement));
            StartCoroutine(ReloadPunch(_punchReloadSpeed));
        }
        


        if (_inAir && state == CharState.Charging)
        {
            Vector3 newVelocity = _rb.velocity;
            newVelocity.y = newVelocity.y * 0.85f;
            _rb.velocity = newVelocity;

            _rb.gravityScale = 0.5f;
        }
        else if (!_inAir || state != CharState.Charging)
        {
            _rb.gravityScale = 2f;
        }

        if (_onWall && _inAir)
        {
            Vector3 newVelocity = _rb.velocity;
            newVelocity.y = newVelocity.y * 0.85f;
            _rb.velocity = newVelocity;

            _rb.gravityScale = 1f;
        }
        else if (state != CharState.Charging)
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

    public void DisablePlayer(float t)
    {
        if(_disableCR != null)
        {
            StopCoroutine(_disableCR);
        }
        _disableCR = StartCoroutine(DisablePlayerCR(t));
    }

    private IEnumerator DisablePlayerCR(float t)
    {
        _disabled = true;
        _sr.color = Color.red;
        _sr2.color = Color.red;
        yield return new WaitForSeconds(t);
        _sr.color = Color.white;
        _sr2.color = Color.white;
        _disabled = false;
    }

    private IEnumerator Charge(Vector3 chargeMovement)
    {
        _rb.isKinematic = false;
        yield return new WaitForEndOfFrame();
        AudioManager.Instance.PlayPunchFX();
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
        yield return new WaitForSeconds(0.5f);
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
        if (collision.gameObject.tag == "Walls")
        {
            
            if(state == CharState.Falling)
            {
                //BoxCollider2D collider = GetComponent<BoxCollider2D>();
                //collider.enabled = false;
                //_rb.gravityScale = 0f;
                Vector2 newVelocity = new Vector2(0, _rb.velocity.y);
                newVelocity.y *= 0.5f;
                
                _rb.velocity = newVelocity;
                _wallSlidePos = transform.position;
                _anim.SetBool("OnWall", true);
                _onWall = true;
            }
            
        }
        //check if ground
        if (collision.gameObject.tag == "Stage")
        {
            if(collision.contacts[0].point.y < transform.position.y)
            {
                //we hit the ground
                
                _inAir = false;
                if(state != CharState.Charging)
                {
                    state = CharState.Landing;
                }
                
                _onWall = false;
            }
            else
            {
                //we hit a ceiling
            }
        }
        if(collision.gameObject.tag == "Enemy")
        {
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (state == CharState.Punching)
            {
                
                //enemyRb.velocity = _rb.velocity * 5f;
                enemyRb.AddForceAtPosition(new Vector2(6, 6), transform.position, ForceMode2D.Impulse);
                _rb.AddForce(transform.up * 10, ForceMode2D.Impulse);
               
                enemyScript.TakeDamage(1);

                AudioManager.Instance.PlayEnemyHitFX();
            }
            else
            {
                if (!enemyScript.IsDead() && !enemyScript.IsDisabled())
                {
                    enemyRb.AddForceAtPosition(new Vector2(5, 5), transform.position, ForceMode2D.Impulse);
                    _rb.AddForceAtPosition(new Vector2(5, -5), collision.transform.position, ForceMode2D.Impulse);
                    _rb.AddForce(-transform.up * 20, ForceMode2D.Impulse);
                    DisablePlayer(0.35f);
                    GameManager.Instance.GotHit();

                    AudioManager.Instance.PlayHitFX();
                }
                
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Walls")
        {


            //BoxCollider2D collider = GetComponent<BoxCollider2D>();
            //collider.enabled = false;
            //_rb.gravityScale = 0f;
            Vector2 newVelocity = new Vector2(0, _rb.velocity.y);
            if (_rb.velocity.y < 0)
            {
                newVelocity.y *= 0.25f;
            }
            else
            {
                newVelocity.y *= 1.25f;
            }
            _rb.velocity = newVelocity;
            transform.position = new Vector2(_wallSlidePos.x, transform.position.y);
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
        if (collision.gameObject.tag == "Walls")
        {
            _anim.SetBool("OnWall", false);
            _onWall = false;
        }
    }
}

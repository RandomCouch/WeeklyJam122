using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour {
    //Simple enemy, moves towards the character

    //Enemy can spawn on either left or right side of the screen
    //Once a certain range is reached, enemy stops moving and starts attacking character
    //Enemy has basic HP and attack properties

    // Use this for initialization

    [SerializeField]
    private Transform _player;

    [SerializeField]
    private BoxCollider2D _selfCollider;

    [SerializeField]
    private GameObject _hitEffect;

    [SerializeField]
    private GameObject _explodeEffect;

    [SerializeField]
    private GameObject _bleedEffect;

    [SerializeField]
    private Rigidbody2D[] _bonesRB;
    

    public float attackRange = 10f;
    public float attackSpeed = 1f;
    public float attackDamage = 5f;

    public float moveSpeed = 5f;

    public float maxHP = 100f;
    public float curHP = 100f;

    private bool _canWalk = true;
    private bool _grounded = false;
    private bool _rotating = false;
    private bool _touchingPlayer = false;
    private BoxCollider2D _collider;
    private SpriteRenderer _spRenderer;
    private Vector2 _moveDir;

    public Action OnDestroyed;

    private bool _collidingWithOtherEnemy = false;
    private Animator _anim;

    public float _groundLevel = 3.26f;

    Rigidbody2D rb2d = null;

    public enum DieMode
    {
        Ragdoll,
        Explode
    }

	void Start () {
        _collider = GetComponent<BoxCollider2D>();
        _spRenderer = GetComponent<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();

        if (GetComponent<Animator>() != null)
            _anim = GetComponent<Animator>();

        if(_player == null)
        {
            _player = FindObjectOfType<Puncher>().transform;
        }
        //StartCoroutine(StandupStraight());
    }

    private IEnumerator StandupStraight()
    {
        //while alive
        if (_grounded && !_rotating && curHP > 0)
        {
                
            Vector3 newAngles = transform.localEulerAngles;
            newAngles.z = 0;
            //StartCoroutine(RotateSelf(newAngles));
            while (transform.localEulerAngles.z != 0)
            {
                //transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, newAngles, Time.deltaTime * 15);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(newAngles), Time.deltaTime * 10);
                yield return new WaitForEndOfFrame();
            }
            //transform.eulerAngles = newAngles;
            //Debug.Log("Rotating to zero");
        }
        yield return new WaitForSeconds(1.5f);
    }

    private void TurnOnRagdoll()
    {
        foreach(Rigidbody2D boneRb in _bonesRB)
        {
            boneRb.bodyType = RigidbodyType2D.Dynamic;
            BoxCollider2D boneCol = boneRb.gameObject.GetComponent<BoxCollider2D>();
            boneCol.enabled = true;
        }
        _bonesRB[0].velocity = rb2d.velocity * 2;
        Instantiate(_explodeEffect, transform.position, Quaternion.identity);
        _bonesRB[0].transform.SetParent(null);
    }

    private void ExplodeParts()
    {
        foreach (Rigidbody2D boneRb in _bonesRB)
        {
            boneRb.transform.SetParent(transform);
            boneRb.transform.localScale = Vector3.one;
            boneRb.bodyType = RigidbodyType2D.Dynamic;
            HingeJoint2D hingeJoint = boneRb.gameObject.GetComponent<HingeJoint2D>();
            if(hingeJoint != null)
            {
                Destroy(hingeJoint);
            }
            BoxCollider2D boneCol = boneRb.gameObject.GetComponent<BoxCollider2D>();
            boneCol.enabled = true;
            boneRb.velocity = rb2d.velocity * 0.05f;
            System.Random randForce = new System.Random();
            boneRb.AddForceAtPosition((boneRb.transform.position - _bonesRB[0].transform.position) * rb2d.velocity.magnitude * randForce.Next(2,10), _bonesRB[0].transform.position);
            GameObject bleedEffect = Instantiate(_bleedEffect, boneRb.transform.position, boneRb.transform.rotation);
            bleedEffect.transform.SetParent(boneRb.transform);
            bleedEffect.transform.localScale = Vector3.one;
            
        }
        System.Random rand = new System.Random();
        _bonesRB[0].angularVelocity = rand.Next(0,360);
        Instantiate(_explodeEffect, transform.position, Quaternion.identity);
        rb2d.velocity = Vector3.zero;
        Destroy(rb2d);
    }


    private IEnumerator RotateSelf(Vector3 newAngles)
    {
        _canWalk = false;
        _rotating = true;
        while (transform.localEulerAngles.z != 0)
        {
            //transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, newAngles, Time.deltaTime * 15);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(newAngles), Time.deltaTime * 10);
            yield return new WaitForEndOfFrame();
        }
        transform.localEulerAngles = newAngles;
        rb2d.velocity = Vector2.zero;
        rb2d.angularVelocity = 0f;
        _canWalk = true;
        _rotating = false;
    }
	
    public void TakeDamage(float damage, Vector3 hitPos)
    {
        if(curHP > 0)
        {
            curHP -= damage;
        }
        
        if(curHP <= 0)
        {
            curHP = 0;
            if(damage >= maxHP)
            {
                Die(DieMode.Explode);
                Freezer.Instance.Freeze();
            }
            else
            {
                Die(DieMode.Ragdoll);
            }
            
        }
        else
        {
            StartCoroutine(GetStunned(0.5f));
            _collider.enabled = false;
            StartCoroutine(ReactivateCollider(0.2f));

            float lifePercent = curHP / maxHP;
            UIManager.Instance.SpawnHealthbar(transform, lifePercent);
            //StartCoroutine(StandupStraight());
        }
        UIManager.Instance.SpawnDamageText(transform.position, damage);

        GameObject hitEffect = Instantiate(_hitEffect, hitPos, Quaternion.identity);
        hitEffect.transform.SetParent(transform);
        hitEffect.transform.localScale = Vector3.one;
        
    }
    
    private IEnumerator ReactivateCollider(float t)
    {
        yield return new WaitForSeconds(0.2f);
        if(_collider != null)
        {
            _collider.enabled = true;
        }
    }

    private IEnumerator GetStunned(float t)
    {
        _canWalk = false;
        yield return new WaitForSeconds(t);
        _canWalk = true;
    }

    private IEnumerator Disappear(float t)
    {
        yield return new WaitForSeconds(t);
        if(OnDestroyed != null)
        {
            OnDestroyed();
        }
        Destroy(_bonesRB[0].gameObject);
        Destroy(gameObject);
    }

    private void Die(DieMode dieMode)
    {
        //Destroy(gameObject);
        Destroy(_anim);
        _canWalk = false;
        _spRenderer.sortingOrder = 0;
        //_selfCollider.enabled = false;
        _collider.enabled = false;
        if(dieMode == DieMode.Explode)
        {
            ExplodeParts();
        }
        else
        {
            TurnOnRagdoll();
        }
        StartCoroutine(Disappear(4f));
    }

	// Update is called once per frame
	void Update () {
		if(_player != null  && _grounded && rb2d != null)
        {
            //Move towards player if no in range of attack
            if(Vector2.Distance(transform.position, _player.position) > attackRange && _canWalk)
            {
                //move enemy slowly towards player
                //figure out what direction player is
                float dir = 1;
                if(transform.position.x < _player.position.x)
                {
                    //is to the left of the player so move right
                    dir = 1;
                    //Change euler angles too
                    //if (transform.localEulerAngles.y != 0)
                    //{
                        Vector3 newAngles = transform.localEulerAngles;
                        newAngles.z = 0;
                        newAngles.y = 0;
                        newAngles.x = 0;
                        transform.localEulerAngles = newAngles;
                    //}
                }
                else if(transform.position.x > _player.position.x) 
                {
                    dir = -1;
                    //Change euler angles too
                    //if(transform.localEulerAngles.y != 180)
                    //{
                        Vector3 newAngles = transform.localEulerAngles;
                        newAngles.x = 0;
                        newAngles.z = 0;
                        newAngles.y = 180;
                        transform.localEulerAngles = newAngles;
                    //}
                    
                }
                _moveDir = dir == 1 ? Vector2.right : Vector2.left;
                //transform.position = Vector3.Lerp(transform.position, transform.position + (moveDir * moveSpeed), Time.deltaTime);
                //rb2d.AddRelativeForce(_moveDir * moveSpeed, ForceMode2D.Force);
                rb2d.MovePosition(Vector3.Lerp(transform.position, (Vector2)transform.position + (_moveDir * moveSpeed), Time.deltaTime));
                
                if (_anim != null)
                    _anim.SetBool("Walking", true);
                //transform.Translate(moveDir * moveSpeed * Time.deltaTime);
            }
            else
            {
                

                if (Vector2.Distance(transform.position, _player.position) < attackRange)
                {
                    rb2d.MovePosition(Vector3.Lerp(transform.position, (Vector2)transform.position - (_moveDir * moveSpeed * 0.25f), Time.deltaTime));
                    if (_anim != null)
                        _anim.SetTrigger("Attack");
                }
            }
        }
        else if(!_canWalk)
        {
            if (_anim != null)
                _anim.SetBool("Walking", false);
        }
        /*
        if(transform.position.y >= _groundLevel)
        {
            if (_anim != null)
                _anim.SetBool("InAir", true);

            _grounded = false;
            
        }
        else
        {
            if (_anim != null)
                _anim.SetBool("InAir", false);

            _grounded = true;
            float angle = Mathf.LerpAngle(transform.eulerAngles.z, 0, Time.deltaTime * 15f);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angle);
        }
        */
        if(rb2d != null)
        {
            if (rb2d.velocity.y < -0.2f)
            {
                rb2d.gravityScale = 3.5f;
            }
            else
            {
                rb2d.gravityScale = 1f;
            }

            //Debug.Log("Enemy Velocity: " + rb2d.velocity);

            if (rb2d.velocity.magnitude >= 6f)
            {
                rb2d.velocity = rb2d.velocity.normalized * 6f;
            }
        }
        
        if(transform.position.y > _groundLevel)
        {
            _grounded = false;
        }
        else
        {
            _grounded = true;
        }
        if (_grounded)
        {
            if (_anim != null)
                _anim.SetBool("InAir", false);
        }
        else
        {
            if (_anim != null)
                _anim.SetBool("InAir", true);
        }
    }

    private void OnAttackHitFrame()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if we are hitting another enemy, stop moving
        //Debug.Log("Enemy colliding with " + collision.gameObject.name);
        if(collision.collider.gameObject.tag == "Enemy")
        {
            //Debug.Log("Enemy colliding with enemy");
            //Check if this enemy is behind the colliding enemy
            if(_moveDir == Vector2.right)
            {
                if(transform.position.x < collision.transform.position.x)
                {
                    _canWalk = false;
                }
            }
            else
            {
                if (transform.position.x > collision.transform.position.x)
                {
                    _canWalk = false;
                }
            }
           // _canWalk = false;
            //_collidingWithOtherEnemy = true;
            if (!_grounded)
            {
                //_canWalk = false;
                if (_anim != null)
                    _anim.SetBool("InAir", true);
                //bounce to help zombie get back on floor
                if(rb2d.velocity.magnitude <= 1.5f)
                    rb2d.AddForceAtPosition(((Vector2)transform.position - (Vector2)collision.collider.transform.position) * 1.5f, collision.contacts[0].point, ForceMode2D.Impulse);
            }
            else
            {
                if(_anim != null)
                    _anim.SetBool("InAir", false);
            }
        }

        if(collision.collider.gameObject.tag == "Floor")
        {
            //grounded
            _grounded = true;
        }
        if (collision.gameObject.tag == "Player")
        {
            _touchingPlayer = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            _grounded = true;
        }
        
        if(collision.gameObject.tag == "Enemy")
        {
            if (_moveDir == Vector2.right)
            {
                if (transform.position.x < collision.transform.position.x)
                {
                    _canWalk = false;
                }
            }
            else
            {
                if (transform.position.x > collision.transform.position.x)
                {
                    _canWalk = false;
                }
            }
        }
        
        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.gameObject.tag == "Enemy")
        {
            _canWalk = true;
            //_collidingWithOtherEnemy = false;
            StartCoroutine(CheckCollision());
        }
        if(collision.collider.gameObject.tag == "Floor")
        {
            _grounded = false;
        }

        if (collision.gameObject.tag == "Player")
        {
            _touchingPlayer = false;
        }
    }

    private IEnumerator CheckCollision()
    {
        yield return new WaitForSeconds(0.4f);
        if (!_collidingWithOtherEnemy)
        {
            _canWalk = true;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollowCam : MonoBehaviour
{
    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _followSpeed;

    private Vector3 _initialOffset;
    // Start is called before the first frame update
    void Start()
    {
        if(_target != null)
        {
            _initialOffset =  transform.position - _target.position;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_target != null)
        {
            Vector3 newPos = _target.position + _initialOffset;
            transform.position = Vector3.Lerp(transform.position, newPos, Time.smoothDeltaTime * _followSpeed);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField]
    private int zPlane = -2;

    [SerializeField]
    private GameObject _cursorStart;

    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private GameObject _cursor;

    private Vector2 _startMousePos;
    private Vector3 _oldStartPos = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.visible = false; 
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = zPlane;

        _cursor.transform.position = mousePos;

        if (Input.GetMouseButtonDown(0))
        {
            _startMousePos = Input.mousePosition;
            _cursorStart.transform.position = mousePos;
            _oldStartPos = mousePos;

            _lineRenderer.SetPosition(0, mousePos);
            _lineRenderer.SetPosition(1, mousePos);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 mouseStartPos = Camera.main.ScreenToWorldPoint(_startMousePos);
            mouseStartPos.z = zPlane;
            _cursorStart.transform.position = mouseStartPos;
            _lineRenderer.SetPosition(0, mouseStartPos);
            _lineRenderer.SetPosition(1, mousePos);
        }
    }


    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = zPlane;
        
        if (Input.GetMouseButtonDown(0))
        {
            _lineRenderer.gameObject.SetActive(true);
            _cursorStart.gameObject.SetActive(true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _cursorStart.SetActive(false);
            _lineRenderer.gameObject.SetActive(false);
        }
    }
}

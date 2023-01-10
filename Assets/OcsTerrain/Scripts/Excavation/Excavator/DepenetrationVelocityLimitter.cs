using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepenetrationVelocityLimitter : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private float _velocity;

    [SerializeField]
    private Transform _targetVelocty;

    [SerializeField]
    private float _targetVelocityCoeff = 1.0f;

    [SerializeField]
    private float _offsetVelocity;

    private Rigidbody rb;
    private Vector3 _pos_old;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.maxDepenetrationVelocity = _velocity;
    }

    private void Update()
    {
        if (!_targetVelocty) return;
        Vector3 pos = _targetVelocty.position;
        _velocity = (pos - _pos_old).magnitude*_targetVelocityCoeff + _offsetVelocity;

        rb.maxDepenetrationVelocity = _velocity;
        
        _pos_old = pos;
    }
}

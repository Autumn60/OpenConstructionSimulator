using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : MonoBehaviour
{
    [SerializeField] private float _minRadius = 0.0f;
    [SerializeField] private float _maxRadius = 1.0f;
    [SerializeField] private float _ballooningTime = 1.0f;

    [SerializeField] private int _static_layerNumber;
    [SerializeField] private int _dynamic_layerNumber;

    private Transform _transform;

    private Rigidbody _rb;

    private float _activateTime;

    public bool isUsed { get => this.gameObject.activeSelf; }
    public bool isActive { get => !this._rb.isKinematic; }

    private void Awake()
    {
        _transform = this.transform;
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _transform.localScale = new Vector3(_minRadius, _minRadius, _minRadius);
        _rb.isKinematic = true;
        this.gameObject.layer = _static_layerNumber;
        for (int i = 0; i < _transform.childCount; i++) _transform.GetChild(i).gameObject.layer = _static_layerNumber;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (_rb.isKinematic) return;
        if (Time.time > _activateTime + _ballooningTime) return;
        float t = Mathf.Clamp01((Time.time - _activateTime)/ _ballooningTime);
        float scale = (1 - t) * _minRadius + t * _maxRadius;
        _transform.localScale = new Vector3(scale, scale, scale);
    }

    public void Activate()
    {
        if (!_rb.isKinematic) return;
        _rb.isKinematic = false;
        _activateTime = Time.time;
        this.gameObject.layer = _dynamic_layerNumber;
        for (int i = 0; i < _transform.childCount; i++) _transform.GetChild(i).gameObject.layer = _dynamic_layerNumber;
    }

    public void SetPosition(float x, float y, float z)
    {
        _transform.position = new Vector3(x, y, z);
        _transform.rotation = Quaternion.identity;
    }

    public void SetPosition(float x, float z)
    {
        Vector3 pos = _transform.position;
        pos.x = x;
        pos.z = z;
        _transform.position = pos;
        _transform.rotation = Quaternion.identity;
    }
}

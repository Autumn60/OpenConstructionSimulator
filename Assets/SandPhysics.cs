using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandPhysics : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _mass = 1.0f;
    public float mass { get => this._mass; }
    [SerializeField] private float _drag = 0.0f;
    [SerializeField] private float _angularDrag = 0.05f;

    [SerializeField] private float _coef_rollingFriction;
    [SerializeField] private float _coef_cohesionForce;

    private Transform _transform;
    [SerializeField] private string _tag;

    public float radius { get => this._transform.localScale.x * 0.5f; set => this._transform.localScale = new Vector3(value, value, value) * 2.0f; }
    public Vector3 position { get => this._transform.position; }

    public Vector3 GetVelocity(ContactPoint contactPoint)
    {
        Vector3 velocity = GetLinearVelocity() + GetSurfaceVelocity(contactPoint.point);
        float normalVelocity = Vector3.Dot(velocity, contactPoint.normal);
        velocity -= normalVelocity * contactPoint.normal;
        return velocity;
    }

    private Vector3 GetLinearVelocity()
    {
        return _rigidbody.velocity;
    }

    private Vector3 GetSurfaceVelocity(Vector3 point)
    {
        point -= _transform.position;
        return Quaternion.Euler(_rigidbody.angularVelocity * Mathf.Rad2Deg) * point - point;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _transform = this.transform;
    }

    private void Start()
    {
        _rigidbody.mass = _mass;
        _rigidbody.drag = _drag;
        _rigidbody.angularDrag = _angularDrag;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _tag = _rigidbody.gameObject.tag;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (_rigidbody.angularVelocity.magnitude == 0) return;

        Vector3 relativeVelocity;
        if (collision.gameObject.tag == _tag)
        {
            SandPhysics otherParticle = collision.gameObject.GetComponent<SandPhysics>();
            relativeVelocity = GetVelocity(collision.contacts[0]) - otherParticle.GetVelocity(collision.contacts[0]);
        }
        else
        {
            relativeVelocity = GetVelocity(collision.contacts[0]);
        }

        float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;

        Vector3 torque = -_coef_rollingFriction * relativeVelocity.magnitude * collisionForce * _rigidbody.angularVelocity / _rigidbody.angularVelocity.magnitude;
        _rigidbody.AddTorque(torque, ForceMode.Force);
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.tag != _tag) return;
        SandPhysics soilParticle = collider.gameObject.GetComponent<SandPhysics>();
        Vector3 position_diff = _transform.position - soilParticle.position;
        Vector3 cohesion = position_diff * _mass * soilParticle.mass / (position_diff.magnitude * position_diff.magnitude * position_diff.magnitude);
        _rigidbody.AddForce(-_coef_cohesionForce * cohesion);
    }
}
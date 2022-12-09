using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocs.Vehicle.DriveTrain
{
    public class RotationalJoint : MonoBehaviour
    {
        enum Axis
        {
            Roll,
            Pitch,
            Yaw
        };

        [SerializeField] private Transform _target;
        [SerializeField] private Axis _axis = Axis.Roll;
        [SerializeField] private bool _reverse = false;

        [SerializeField] private bool _limit = false;
        [SerializeField] private float _limit_min = -179.0f;
        public float limit_min { get => this._limit_min; }
        [SerializeField] private float _limit_max = 179.0f;
        public float limit_max { get => this._limit_max; }

        [SerializeField] private float T;

        [SerializeField] private float _targetAngle;
        [SerializeField] private float _angle;

        private float _startTime;
        private float _startAngle;

        [SerializeField] private float _velocity;

        private void Awake()
        {
            if (!_target) _target = this.transform;
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (_velocity != 0f) SetTargetAngle(_targetAngle + _velocity * Time.deltaTime);
            /*
            float t = (1.0f - Mathf.Exp(-(Time.time - _startTime) / T));
            float nextAngle = t * _targetAngle + (1 - t) * _startAngle;
            float deltaAngle = nextAngle - _angle;
            */
            float t, nextAngle, deltaAngle;
            if (_velocity == 0f)
            {
                t = (1.0f - Mathf.Exp(-(Time.time - _startTime) / T));
                nextAngle = t * _targetAngle + (1 - t) * _startAngle;
                deltaAngle = nextAngle - _angle;
            }
            else
            {
                SetTargetAngle(_targetAngle + _velocity * Time.deltaTime);
                t = (1.0f - Mathf.Exp(-(Time.deltaTime) / T));
                nextAngle = t * _targetAngle + (1 - t) * _startAngle;
                deltaAngle = nextAngle - _angle;
            }
            if (_limit)
            {
                if (nextAngle > _limit_max) deltaAngle = _limit_max - _angle;
                else if (nextAngle < _limit_min) deltaAngle = _limit_min - _angle;
            }
            _angle += deltaAngle;

            Vector3 axis = new Vector3();
            switch (_axis)
            {
                case Axis.Roll:
                    axis = Vector3.forward;
                    break;
                case Axis.Pitch:
                    axis = Vector3.right;
                    break;
                case Axis.Yaw:
                    axis = Vector3.up;
                    break;
            }
            _target.rotation *= Quaternion.AngleAxis(deltaAngle, axis);
        }

        private void SetTargetAngle(float targetAngle)
        {
            this._targetAngle = targetAngle;
            if (_limit)
            {
                if (_targetAngle > _limit_max) _targetAngle = _limit_max;
                else if (_targetAngle < _limit_min) _targetAngle = _limit_min;
            }
            _startAngle = _angle;
            _startTime = Time.time;
        }

        public void SetVelocity(float velocity)
        {
            _velocity = _reverse ? -velocity : velocity;
        }
    }
}

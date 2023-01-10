using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ocs.Vehicle.DriveTrain;

namespace Ocs.Vehicle
{
    public class Crawler : Vehicle
    {
        public float LeftCrawlerInput { get; set; }
        public bool LeftReverse { get; set; }
        public float RightCrawlerInput { get; set; }
        public bool RightReverse { get; set; }

        [SerializeField] private float _turnTorque = 1000000f;
        [SerializeField] private float _driveTorque = 400f;
        [SerializeField] private float _brakeTorque = 300f;
        [SerializeField] private float _maxSpeed = 1.5f;
        [SerializeField] private float _maxRotSpeed = 0.75f;

        [SerializeField] private float _sidewaysFriction_move = 2.2f;
        [SerializeField] private float _sidewaysFriction_stiff = 0.8f;

        [SerializeField] private float _maxHeight = 1.5f;
        [SerializeField] private float _centerOfMassYOffset = 1.0f;

        [SerializeField] private WheelCollider[] _leftWheelColliders;
        [SerializeField] private WheelCollider[] _rightWheelColliders;

        [SerializeField] private MeshRenderer _leftTrackMesh;
        [SerializeField] private MeshRenderer _rightTrackMesh;
        [SerializeField] private float _animationSpeedGain = 0.001f;

        private Transform _transform;
        private Rigidbody _rigidbody;

        private WheelFrictionCurve[] _leftWCFriction;
        private WheelFrictionCurve[] _rightWCFriction;

        private float _climbAngle_old;

        [SerializeField] private bool _use_debugInput = false;
        [SerializeField] [Range(-1.0f, 1.0f)] private float _debug_leftInput = 0.0f;
        [SerializeField] [Range(-1.0f, 1.0f)] private float _debug_rightInput = 0.0f;

        private void Awake()
        {
            _transform = this.transform;
            _rigidbody = GetComponent<Rigidbody>();

            _leftWCFriction = new WheelFrictionCurve[_leftWheelColliders.Length];
            _rightWCFriction = new WheelFrictionCurve[_rightWheelColliders.Length];
        }

        private void Start()
        {
            for (int i = 0; i < _leftWheelColliders.Length; i++)
            {
                _leftWCFriction[i] = _leftWheelColliders[i].sidewaysFriction;
                _leftWCFriction[i].stiffness = _sidewaysFriction_stiff;
                _leftWheelColliders[i].sidewaysFriction = _leftWCFriction[i];
            }

            for (int i = 0; i < _rightWheelColliders.Length; i++)
            {
                _rightWCFriction[i] = _rightWheelColliders[i].sidewaysFriction;
                _rightWCFriction[i].stiffness = _sidewaysFriction_stiff;
                _rightWheelColliders[i].sidewaysFriction = _rightWCFriction[i];
            }

            _rigidbody.maxAngularVelocity = _maxRotSpeed;
        }

        protected virtual void Update()
        {
            vehicleStateUpdate();
        }

        protected virtual void FixedUpdate()
        {
            _rigidbody.centerOfMass = new Vector3(0, _centerOfMassYOffset, 0);
            float localZVelocity = transform.InverseTransformDirection(_rigidbody.velocity).z;

            float leftInput, rightInput;

            if (_use_debugInput)
            {
                leftInput = _debug_leftInput;
                rightInput = _debug_rightInput;
            }
            else
            {
                leftInput = LeftReverse ? -LeftCrawlerInput : LeftCrawlerInput;
                rightInput = RightReverse ? -RightCrawlerInput : RightCrawlerInput;
            }

            float driveInput, turnInput;
            (driveInput, turnInput) = ConvertInput(leftInput, rightInput);

            SetTrackTorque(leftInput * _driveTorque, rightInput * _driveTorque);
            _leftTrackMesh.material.SetTextureOffset("_MainTex", new Vector2(0, _leftTrackMesh.material.mainTextureOffset.y + (leftInput * -1.0f * _rigidbody.velocity.magnitude * _animationSpeedGain * Mathf.Sign(localZVelocity))));
            _rightTrackMesh.material.SetTextureOffset("_MainTex", new Vector2(0, _rightTrackMesh.material.mainTextureOffset.y + (rightInput * +1.0f * _rigidbody.velocity.magnitude * _animationSpeedGain * Mathf.Sign(localZVelocity))));

            bool isGrounded = IsGrounded();
            if (isGrounded)
            {
                _rigidbody.AddTorque(_transform.up * turnInput * _turnTorque * Time.fixedDeltaTime);
                if (IsZero(turnInput))
                    _rigidbody.AddTorque(transform.up * _turnTorque * Time.fixedDeltaTime * -_rigidbody.angularVelocity.y);
            }

            _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, _maxSpeed);
            _rigidbody.angularVelocity = Vector3.ClampMagnitude(_rigidbody.angularVelocity, _maxRotSpeed);

            if (IsZero(driveInput) && IsZero(turnInput) ||
                (driveInput < 0.0f && _rigidbody.velocity.magnitude > 0.5f) ||
                (driveInput > 0.0f && _rigidbody.velocity.magnitude < -0.5f)
                )
            {
                _rigidbody.drag = 2.0f;
                SetBrakeTorque(_brakeTorque);
            }
            else
            {
                _rigidbody.drag = 0.1f;
                SetBrakeTorque(0.0f);
            }

            float climbAngle = GetClimbAngle();
            if (climbAngle > 20.0f && isGrounded && _climbAngle_old < climbAngle)
                _rigidbody.drag = climbAngle * 0.03f;
            _climbAngle_old = climbAngle;
        }

        private void LateUpdate()
        {
            bool isStop = (_rigidbody.velocity.magnitude <= 1.0f || GetClimbAngle() > 22.0f);

            for (int i = 0; i < _leftWheelColliders.Length; i++)
            {
                _leftWCFriction[i] = _leftWheelColliders[i].sidewaysFriction;
                _leftWCFriction[i].stiffness = isStop ? _sidewaysFriction_stiff : _sidewaysFriction_move;
                _leftWheelColliders[i].sidewaysFriction = _leftWCFriction[i];
            }

            for (int i = 0; i < _rightWheelColliders.Length; i++)
            {
                _rightWCFriction[i] = _rightWheelColliders[i].sidewaysFriction;
                _rightWCFriction[i].stiffness = isStop ? _sidewaysFriction_stiff : _sidewaysFriction_move;
                _rightWheelColliders[i].sidewaysFriction = _rightWCFriction[i];
            }
        }

        void SetTrackTorque(float speed_l, float speed_r)
        {
            foreach (WheelCollider wc in _leftWheelColliders)
                wc.motorTorque = speed_l;
            foreach (WheelCollider wc in _rightWheelColliders)
                wc.motorTorque = speed_r;
        }

        void SetBrakeTorque(float torque)
        {
            foreach (WheelCollider wc in _leftWheelColliders)
                wc.brakeTorque = torque;
            foreach (WheelCollider wc in _rightWheelColliders)
                wc.brakeTorque = torque;
        }

        private (float, float) ConvertInput(float leftInput, float rightInput)
        {
            return ((leftInput + rightInput) * 0.5f, (leftInput - rightInput) * 0.5f);
        }

        private bool IsZero(float val)
        {
            //return (Mathf.Abs(val) < 0.05f);
            return (val == 0);
        }

        private float GetClimbAngle()
        {
            return Vector3.Angle(Vector3.up, _transform.up);
        }

        private bool IsGrounded()
        {
            RaycastHit hit;
            Ray ray = new Ray(_transform.position + _transform.up * 1.0f, -_transform.up);
            if (Physics.Raycast(ray, out hit))
            {
                return (hit.distance < _maxHeight);
            }
            return false;
        }
    }
}

using System;
using InputSystem.Interfaces;
using PlayerController.Interfaces;
using PoolManager.Interfaces;
using UnityEngine;

namespace PlayerController
{
    public class Player : MonoBehaviour, IPlayer, IPoolObject, IKillable, ITarget
    {
        [SerializeField] private float accelerationMultiplier = 1f;
        [SerializeField] private float maxVelocityMagnitude = 10f;
        [Range(0.1f, 1f)] [SerializeField] private float breakMultiplier = 0.5f;

        private Rigidbody2D _rigidbody2D;
        private IInputSystem _inputSystem;

        public bool IsDead { get; private set; } = true;

        public Action OnFire { get; set; } = () => { };
        public Action OnActivate { get; set; } = () => { };
        public Action OnDeactivate { get; set; } = () => { };

        public void Initialize(IInputSystem inputSystem)
        {
            _inputSystem = inputSystem;
            _rigidbody2D = transform.GetComponent<Rigidbody2D>();
        }

        private void SetupControl(IInputSystem inputSystem)
        {
            inputSystem.OnUp += OnUp;
            inputSystem.OnDown += OnDown;
            inputSystem.OnRight += OnRight;
            inputSystem.OnLeft += OnLeft;
            inputSystem.OnSpace += OnFire;
        }

        private void ResetControl(IInputSystem inputSystem)
        {
            inputSystem.OnUp -= OnUp;
            inputSystem.OnDown -= OnDown;
            inputSystem.OnRight -= OnRight;
            inputSystem.OnLeft -= OnLeft;
            inputSystem.OnSpace -= OnFire;
        }

        private void OnLeft()
        {
            transform.Rotate(Vector3.forward);
        }

        private void OnRight()
        {
            transform.Rotate(Vector3.back);
        }

        private void OnUp()
        {
            _rigidbody2D.AddForce(transform.localRotation * Vector3.up * accelerationMultiplier);
            _rigidbody2D.velocity = Vector2.ClampMagnitude(_rigidbody2D.velocity, maxVelocityMagnitude);
        }

        private void OnDown()
        {
            _rigidbody2D.AddForce(_rigidbody2D.velocity * -breakMultiplier);
        }

        public void Activate()
        {
            //TODO: remove multiple IsDead check (Bullet, Enemy, Player)
            if (!IsDead)
            {
                return;
            }

            SetupControl(_inputSystem);
            OnActivate();

            IsDead = false;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            if (IsDead)
            {
                return;
            }

            OnDeactivate();

            ResetControl(_inputSystem);
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            OnActivate = () => { };
            OnDeactivate = () => { };

            IsDead = true;
            gameObject.SetActive(false);
        }

        public void DirectUpdate()
        {
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(gameObject.tag))
            {
                return;
            }

            TakeDamage();
        }

        public void TakeDamage()
        {
            Deactivate();
        }

        public Vector2 GetCurrentPosition()
        {
            return transform.position;
        }

        bool ITarget.IsDead()
        {
            return IsDead;
        }
    }
}
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Units.Objects
{
    public class MovingPlatform : MonoBehaviour
    {
        public enum ActivationType
        {
            None,         // moves immediately, no trigger required
            Continuous,   // loops indefinitely
            Lever,        // triggered by lever
            OnUnitDeath   // triggered externally (enemy death, etc.)
        }

        [Header("Movement Settings")]
        public float speed = 5f;
        public float distance = 5f;
        [FormerlySerializedAs("movingUp")] public bool vertical = true;
        public bool movingUp = false;

        [Header("Activation Settings")]
        public ActivationType activationType = ActivationType.Lever;
        [Tooltip("If true, platform can be reactivated by player or event")]
        public bool canReactivate = true;

        private Vector3 _startPosition;
        private bool _isMoving = false;
        private float _traveledDistance = 0f;
        private bool _queuedFlip = false;


        private void Start()
        {
            _startPosition = transform.position;

            // If set to None, start moving immediately
            if (activationType == ActivationType.None)
            {
                _isMoving = true;
            }
        }

        private void Update()
        {
            switch (activationType)
            {
                case ActivationType.None:
                    MoveOnce(); // one-shot but restarts every frame if set to None
                    break;

                case ActivationType.Continuous:
                    MoveContinuous();
                    break;

                case ActivationType.Lever:
                case ActivationType.OnUnitDeath:
                    if (_isMoving)
                        MoveOnce();
                    break;
            }
        }

        // ---------- Movement Behaviors ----------

        private void MoveOnce()
        {
            Vector3 direction = vertical
                ? (movingUp ? Vector3.down : Vector3.up)
                : (movingUp ? Vector3.left : Vector3.right);

            float step = speed * Time.deltaTime;
            transform.Translate(direction * step);
            _traveledDistance += step;

            if (_traveledDistance >= distance)
            {
                _isMoving = false;
                _traveledDistance = 0f;
                _startPosition = transform.position;

                if (_queuedFlip)
                {
                    _queuedFlip = false;
                    movingUp = !movingUp;
                    _isMoving = true;
                    Debug.Log($"{name} auto-flipped due to queued activation.");
                }
                else
                {
                    Debug.Log($"{name} stopped after moving {distance:F2} units ({(movingUp ? "down" : "up")}).");
                }
            }

        }

        private void MoveContinuous()
        {
            Vector3 direction = vertical
                ? (movingUp ? Vector3.down : Vector3.up)
                : (movingUp ? Vector3.left : Vector3.right);

            float step = speed * Time.deltaTime;
            transform.Translate(direction * step);
            _traveledDistance += step;

            if (_traveledDistance >= distance)
            {
                _traveledDistance = 0f;
                _startPosition = transform.position;
                movingUp = !movingUp; // flip direction automatically
            }
        }

        // ---------- External Triggers ----------

        public void Activate()
        {
            // Prevent mid-movement flipping exploit
            if (_isMoving)
            {
                if (canReactivate)
                {
                    _queuedFlip = true;
                    Debug.Log($"{name} queued direction flip after current move.");
                }
                return;
            }

            if (!canReactivate && _traveledDistance > 0f)
            {
                Debug.Log($"{name} cannot be reactivated (one-time only).");
                return;
            }

            _isMoving = true;
            movingUp = !movingUp; // flip direction only when stopped
            _traveledDistance = 0f;
            _startPosition = transform.position;

            Debug.Log($"Activating {name}, moving {(movingUp ? "down" : "up")}");
        }


        public void Deactivate()
        {
            _isMoving = false;
        }
    }
}

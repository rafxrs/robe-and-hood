using UnityEngine;
using Unity.Cinemachine;

public class CameraLookahead : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The single Cinemachine camera used for the whole game.")]
    public CinemachineCamera cinemachineCamera;

    [Tooltip("The player's CharacterController2D, used for facing direction and vertical velocity.")]
    public CharacterController2D controller;

    [Header("Horizontal Lookahead")]
    [Tooltip("How far the offset reaches when fully facing a direction")]
    public float maxOffsetX = 1f;

    [Tooltip("Approx. seconds to settle into the target offset — lower is snappier")]
    public float smoothTime = 0.25f;

    [Header("Vertical Damping")]
    [Tooltip("Composer Y damping while rising — higher feels heavier/slower")]
    public float risingYDamping = 1f;
    [Tooltip("Composer Y damping while falling — lower feels snappier/more aggressive")]
    public float fallingYDamping = 0.3f;
    [Tooltip("Approx. seconds to transition between rising/falling damping values")]
    public float dampingSmoothTime = 0.15f;

    private float _currentYDamping;
    private float _dampingVelocity; // SmoothDamp's internal velocity state for the damping lerp

    private float _currentOffsetX;
    private float _offsetVelocity; // required out-param SmoothDamp uses internally between calls

    private Rigidbody2D _rb;
    private CinemachinePositionComposer _composer;

    void Start()
    {
        _currentYDamping = risingYDamping;

        if (controller != null)
            _rb = controller.GetComponent<Rigidbody2D>();

        if (cinemachineCamera != null)
            _composer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();
    }

    void Update()
    {
        if (controller == null || _composer == null || _rb == null) return;

        float faceDirection = controller._mFacingRight ? 1f : -1f;

        float targetX = faceDirection * maxOffsetX;
        _currentOffsetX = Mathf.SmoothDamp(_currentOffsetX, targetX, ref _offsetVelocity, smoothTime);

        Vector3 offset = _composer.TargetOffset;
        offset.x = _currentOffsetX;
        _composer.TargetOffset = offset;

        float targetYDamping = _rb.linearVelocity.y < 0f ? fallingYDamping : risingYDamping;
        _currentYDamping = Mathf.SmoothDamp(_currentYDamping, targetYDamping, ref _dampingVelocity, dampingSmoothTime);

        Vector3 damping = _composer.Damping;
        damping.y = _currentYDamping;
        _composer.Damping = damping;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterController2D : MonoBehaviour
{
	[FormerlySerializedAs("m_JumpForce")][SerializeField] private float mJumpForce = 400f;                          // Amount of force added when the player jumps.
	[FormerlySerializedAs("m_CrouchSpeed")][Range(0, 1.5f)][SerializeField] private float mCrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
	[FormerlySerializedAs("m_MovementSmoothing")][Range(0, .3f)][SerializeField] private float mMovementSmoothing = .05f;   // How much to smooth out the movement
	[FormerlySerializedAs("m_AirControl")][SerializeField] private bool mAirControl = false;                            // Whether or not a player can steer while jumping;
	[FormerlySerializedAs("m_WhatIsGround")][SerializeField] private LayerMask mWhatIsGround;                           // A mask determining what is ground to the character
	[FormerlySerializedAs("m_GroundCheck")][SerializeField] private Transform mGroundCheck;                         // A position marking where to check if the player is grounded.
	[FormerlySerializedAs("m_CeilingCheck")][SerializeField] private Transform mCeilingCheck;                           // A position marking where to check for ceilings
	[FormerlySerializedAs("m_CrouchDisableCollider")][SerializeField] private Collider2D mCrouchDisableCollider;                // A collider that will be disabled when crouching

	[Header("Variable Jump Height")]
	[Tooltip("Multiplier applied to the player's upward velocity if the jump button is released early (0 = hard cut, 1 = no cut).")]
	[Range(0f, 1f)][SerializeField] private float mJumpCutMultiplier = 0.5f;
	[Tooltip("Minimum upward velocity guaranteed even on the shortest tap of the jump button.")]
	[SerializeField] private float mMinJumpVelocity = 4f;

	const float KGroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	const float KCeilingRadius = .2f; // Radius of the overlap circle to determine if there is a ceiling
	[FormerlySerializedAs("m_Grounded")] public bool mGrounded;            // Whether or not the player is grounded.
	public bool Grounded { get; private set; }

	// Coyote time
	private const float CoyoteTime = 0.12f;
	private float _lastGroundedTime = -1f;

	// Variable jump height state
	private bool _isJumping = false;      // true from the moment the jump is triggered until it's grounded again
	private bool _jumpCutApplied = false; // ensures we only cut the jump once per jump

	private Rigidbody2D _mRigidbody2D;
	public bool _mFacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 _mVelocity = Vector3.zero;


	[FormerlySerializedAs("OnLandEvent")]
	[Header("Events")]
	[Space]

	public UnityEvent onLandEvent;
	[FormerlySerializedAs("OnCrouchEvent")] public UnityEvent onCrouchEvent;
	private bool _mWasCrouching = false;

	private void Awake()
	{
		_mRigidbody2D = GetComponent<Rigidbody2D>();

		if (onLandEvent == null)
			onLandEvent = new UnityEvent();

		if (onCrouchEvent == null)
			onCrouchEvent = new UnityEvent();
	}

	private void FixedUpdate()
	{
		bool wasGrounded = mGrounded;
		mGrounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		Collider2D[] colliders = Physics2D.OverlapCircleAll(mGroundCheck.position, KGroundedRadius, mWhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				mGrounded = true;
				if (!wasGrounded)
					onLandEvent.Invoke();
			}
		}

		if (mGrounded)
		{
			_lastGroundedTime = Time.time;
			// Reset jump state once we're back on the ground
			_isJumping = false;
			_jumpCutApplied = false;
		}

		// Expose unified grounded state
		Grounded = mGrounded;
	}


	// Return true if a jump force was actually applied this frame
	public bool Move(float move, bool crouch, bool jump)
	{
		bool didJump = false;

		// If crouching, check to see if the character can stand up
		if (crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(mCeilingCheck.position, KCeilingRadius, mWhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (mGrounded || mAirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!_mWasCrouching)
				{
					_mWasCrouching = true;

				}

				// Reduce the speed by the crouchSpeed multiplier
				move *= mCrouchSpeed;

				// Disable one of the colliders when crouching
				if (mCrouchDisableCollider != null)
					mCrouchDisableCollider.enabled = false;
			}
			else
			{
				onCrouchEvent.Invoke();
				// Enable the collider when not crouching
				if (mCrouchDisableCollider != null)
					mCrouchDisableCollider.enabled = true;

				if (_mWasCrouching)
				{
					_mWasCrouching = false;
					// OnCrouchEvent.Invoke();
				}
			}

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, _mRigidbody2D.linearVelocity.y);
			// And then smoothing it out and applying it to the character
			_mRigidbody2D.linearVelocity = Vector3.SmoothDamp(_mRigidbody2D.linearVelocity, targetVelocity, ref _mVelocity, mMovementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !_mFacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && _mFacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump (grounded or within coyote time)...
		if ((mGrounded || Time.time - _lastGroundedTime <= CoyoteTime) && jump)
		{
			// Add a vertical force to the player.
			_mRigidbody2D.AddForce(new Vector2(0f, mJumpForce));
			didJump = true;

			// Mark as airborne until next ground check updates it again
			mGrounded = false;
			Grounded = mGrounded;

			// Begin tracking this jump for variable height purposes
			_isJumping = true;
			_jumpCutApplied = false;
		}

		return didJump;
	}
	public void EndJump()
	{
		if (!_isJumping || _jumpCutApplied)
			return;

		Vector2 velocity = _mRigidbody2D.linearVelocity;

		if (velocity.y > 0f)
		{
			float cutVelocity = velocity.y * mJumpCutMultiplier;
			// Never cut below the guaranteed minimum jump velocity
			velocity.y = Mathf.Max(cutVelocity, mMinJumpVelocity);
			_mRigidbody2D.linearVelocity = velocity;
		}

		_jumpCutApplied = true;
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		_mFacingRight = !_mFacingRight;

		Vector3 scale = transform.localScale;
		scale.x *= -1;        // <-- this is now consistent with EnemyDir
		transform.localScale = scale;
		transform.Find("MissingMana").localScale = scale;
		transform.Find("MissingKey").localScale = scale;

	}
}
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 스크립트를 플랫폼에 추가하면 플레이 가능한 캐릭터가 걸어갈 때 쓰러집니다.
    /// 플랫폼에 AutoRespawn 구성 요소를 추가하면 캐릭터가 죽을 때 재설정됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Falling Platform 2D")]
	public class FallingPlatform2D : TopDownMonoBehaviour 
	{
		/// the possible states for the falling platform
		public enum FallingPlatformStates { Idle, Shaking, Falling, ColliderOff }

		/// the current state of the falling platform
		[MMReadOnly]
		[Tooltip("떨어지는 플랫폼의 현재 상태")]
		public FallingPlatformStates State;

		/// if this is true, the platform will fall inevitably once touched
		[Tooltip("이것이 사실이라면 플랫폼을 건드리면 필연적으로 떨어질 것입니다.")]
		public bool InevitableFall = false;
		/// the time (in seconds) before the fall of the platform
		[Tooltip("플랫폼이 추락하기 전의 시간(초)")]
		public float TimeBeforeFall = 2f;
		/// the time (in seconds) before the collider turns itself off once the fall has started
		[Tooltip("낙하가 시작된 후 충돌체가 꺼지기 전까지의 시간(초)")]
		public float DelayBetweenFallAndColliderOff = 0.5f;

		// private stuff
		protected Animator _animator;
		protected Vector2 _newPosition;
		protected Bounds _bounds;
		protected Collider2D _collider;
		protected Vector3 _initialPosition;
		protected float _timeLeftBeforeFall;
		protected float _fallStartedAt;
		protected bool _contact = false;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start()
		{
			Initialization ();
		}

		/// <summary>
		/// Grabs components and saves initial position and timer
		/// </summary>
		protected virtual void Initialization()
		{
			// we get the animator
			State = FallingPlatformStates.Idle;
			_animator = GetComponent<Animator>();
			_collider = GetComponent<Collider2D> ();
			_collider.enabled = true;
			_bounds =LevelManager.Instance.LevelBounds;
			_initialPosition = this.transform.position;
			_timeLeftBeforeFall = TimeBeforeFall;

		}

		/// <summary>
		/// This is called every frame.
		/// </summary>
		protected virtual void FixedUpdate()
		{		
			// we send our various states to the animator.		
			UpdateAnimator ();	

			if (_contact)
			{
				_timeLeftBeforeFall -= Time.deltaTime;
			}

			if (_timeLeftBeforeFall < 0)
			{
				if (State != FallingPlatformStates.Falling)
				{
					_fallStartedAt = Time.time;
				}
				State = FallingPlatformStates.Falling;
			}

			if (State == FallingPlatformStates.Falling)
			{
				if (Time.time - _fallStartedAt >= DelayBetweenFallAndColliderOff)
				{
					_collider.enabled = false;
				}
			}            
		}

		/// <summary>
		/// Disables the falling platform. We're not destroying it, so we can revive it on respawn
		/// </summary>
		protected virtual void DisableFallingPlatform()
		{
			this.gameObject.SetActive (false);					
			this.transform.position = _initialPosition;		
			_timeLeftBeforeFall = TimeBeforeFall;
			State = FallingPlatformStates.Idle;
		}

		/// <summary>
		/// Updates the block's animator.
		/// </summary>
		protected virtual void UpdateAnimator()
		{				
			if (_animator!=null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Idle", (State == FallingPlatformStates.Idle));
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Shaking", (State == FallingPlatformStates.Shaking));
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Falling", (State == FallingPlatformStates.Falling));
			}
		}

		/// <summary>
		/// Triggered when a TopDownController touches the platform
		/// </summary>
		/// <param name="controller">The TopDown controller that collides with the platform.</param>		
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			TopDownController2D controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController2D>();
			if (controller == null)
			{
				return;
			}

			if (State == FallingPlatformStates.Falling)
			{
				return;
			}

			if (TimeBeforeFall>0)
			{
				_contact = true;
				State = FallingPlatformStates.Shaking;
			}	
			else
			{
				if (!InevitableFall)
				{
					_contact = false;
					State = FallingPlatformStates.Idle;
				}
			}
		}
		/// <summary>
		/// Triggered when a TopDownController exits the platform
		/// </summary>
		/// <param name="controller">The TopDown controller that collides with the platform.</param>
		protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			if (InevitableFall)
			{
				return;
			}

			TopDownController controller = collider.gameObject.GetComponent<TopDownController>();
			if (controller==null)
				return;

			_contact = false;
			if (State == FallingPlatformStates.Shaking)
			{
				State = FallingPlatformStates.Idle;
			}
		}

		/// <summary>
		/// On Revive, we restore this platform's state
		/// </summary>
		protected virtual void OnRevive()
		{
			this.transform.position = _initialPosition;		
			_timeLeftBeforeFall = TimeBeforeFall;
			State = FallingPlatformStates.Idle;

		}
	}
}
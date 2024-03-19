using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// CharacterMovement 능력이 필요합니다.
    /// 캐릭터가 경로에서 장애물을 발견할 때까지 무작위로 움직이게 하며, 이 경우 무작위로 새로운 방향을 선택합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMoveRandomly3D")]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionMoveRandomly3D : AIAction
	{
		[Header("Duration")]
        /// 캐릭터가 방향을 바꾸지 않고 이동하는 데 소비할 수 있는 최대 시간
        [Tooltip("캐릭터가 방향을 바꾸지 않고 이동하는 데 소비할 수 있는 최대 시간")]
		public float MaximumDurationInADirection = 2f;
		[Header("Obstacles")]
        /// 캐릭터가 피하려고 하는 레이어
        [Tooltip("캐릭터가 피하려고 하는 레이어")]
		public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
        /// 이 캐릭터가 도달할 수 있는 대상으로부터의 최소 거리입니다.
        [Tooltip("이 캐릭터가 도달할 수 있는 대상으로부터의 최소 거리입니다.")]
		public float ObstaclesDetectionDistance = 1f;
        /// 장애물을 확인하는 빈도(초)
        [Tooltip("the장애물을 확인하는 빈도(초)")]
		public float ObstaclesCheckFrequency = 1f;
        /// 무작위로 추출할 최소 무작위 방향
        [Tooltip("무작위로 추출할 최소 무작위 방향")]
		public Vector2 MinimumRandomDirection = new Vector2(-1f, -1f);
        /// 무작위화할 최대 무작위 방향
        [Tooltip("무작위화할 최대 무작위 방향")]
		public Vector2 MaximumRandomDirection = new Vector2(1f, 1f);

		protected CharacterMovement _characterMovement;
		protected Vector2 _direction;
		protected Collider _collider;
		protected float _lastObstacleDetectionTimestamp = 0f;
		protected float _lastDirectionChangeTimestamp = 0f;
		protected Vector3 _rayDirection;
                
		/// <summary>
		/// On start we grab our character movement component and pick a random direction
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
			_collider = this.gameObject.GetComponentInParent<Collider>();
			PickRandomDirection();
		}

		/// <summary>
		/// On PerformAction we move
		/// </summary>
		public override void PerformAction()
		{
			CheckForObstacles();
			CheckForDuration();
			Move();
		}

		/// <summary>
		/// Moves the character
		/// </summary>
		protected virtual void Move()
		{
			_characterMovement.SetMovement(_direction);
		}

		/// <summary>
		/// Checks for obstacles by casting a ray
		/// </summary>
		protected virtual void CheckForObstacles()
		{
			if (Time.time - _lastObstacleDetectionTimestamp < ObstaclesCheckFrequency)
			{
				return;
			}

			_rayDirection = Vector3.zero;
			_rayDirection.x = _direction.x;
			_rayDirection.z = _direction.y;
			bool hit = Physics.BoxCast(_collider.bounds.center, _collider.bounds.extents, _rayDirection.normalized, this.transform.rotation, ObstaclesDetectionDistance, ObstacleLayerMask);
			if (hit)
			{
				PickRandomDirection();
			}

			_lastObstacleDetectionTimestamp = Time.time;
		}

		/// <summary>
		/// Checks whether or not we should pick a new direction at random
		/// </summary>
		protected virtual void CheckForDuration()
		{
			if (Time.time - _lastDirectionChangeTimestamp > MaximumDurationInADirection)
			{
				PickRandomDirection();
			}
		}

		/// <summary>
		/// Picks a random direction
		/// </summary>
		protected virtual void PickRandomDirection()
		{
			_direction.x = UnityEngine.Random.Range(MinimumRandomDirection.x, MaximumRandomDirection.x);
			_direction.y = UnityEngine.Random.Range(MinimumRandomDirection.y, MaximumRandomDirection.y);
			_lastDirectionChangeTimestamp = Time.time;
		}

		/// <summary>
		/// On exit state we stop our movement
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();

			_characterMovement?.SetHorizontalMovement(0f);
			_characterMovement?.SetVerticalMovement(0f);
		}
	}
}
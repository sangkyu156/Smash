using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// CharacterMovement 능력이 필요합니다. 캐릭터가 대상 방향으로 지정된 최소 거리만큼 이동하도록 합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMoveTowardsTarget3D")]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionMoveTowardsTarget3D : AIAction
	{
        /// 이 캐릭터가 도달할 수 있는 대상으로부터의 최소 거리입니다.
        [Tooltip("이 캐릭터가 도달할 수 있는 대상으로부터의 최소 거리입니다.")]
		public float MinimumDistance = 1f;

		protected Vector3 _directionToTarget;
		protected CharacterMovement _characterMovement;
		protected int _numberOfJumps = 0;
		protected Vector2 _movementVector;

		/// <summary>
		/// On init we grab our CharacterMovement ability
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
		}

		/// <summary>
		/// On PerformAction we move
		/// </summary>
		public override void PerformAction()
		{
			Move();
		}

		/// <summary>
		/// Moves the character towards the target if needed
		/// </summary>
		protected virtual void Move()
		{
			if (_brain.Target == null)
			{
				return;
			}
            
			_directionToTarget = _brain.Target.position - this.transform.position;
			_movementVector.x = _directionToTarget.x;
			_movementVector.y = _directionToTarget.z;
			_characterMovement.SetMovement(_movementVector);


            if (Mathf.Abs(this.transform.position.x - _brain.Target.position.x) < MinimumDistance)
			{
				_characterMovement.SetHorizontalMovement(0f);
			}

			if (Mathf.Abs(this.transform.position.z - _brain.Target.position.z) < MinimumDistance)
			{
				_characterMovement.SetVerticalMovement(0f);
			}
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
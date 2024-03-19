using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// RequiresCharacterMovement 능력. 캐릭터가 대상 방향으로 지정된 최소 거리만큼 이동하도록 합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMoveTowardsTarget2D")]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionMoveTowardsTarget2D : AIAction
	{
        /// 이것이 사실이라면, x축의 대상까지의 특정 거리를 넘지 않도록 이동이 제한됩니다.
        [Tooltip("이것이 사실이라면, x축의 대상까지의 특정 거리를 넘지 않도록 이동이 제한됩니다.")]
		public bool UseMinimumXDistance = true;
        /// 이 캐릭터가 x축에서 도달할 수 있는 대상으로부터의 최소 거리입니다.
        [FormerlySerializedAs("MinimumDistance")] [Tooltip("이 캐릭터가 x축에서 도달할 수 있는 대상으로부터의 최소 거리입니다.")]
		public float MinimumXDistance = 1f;
		
		protected Vector2 _direction;
		protected CharacterMovement _characterMovement;
		protected int _numberOfJumps = 0;

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

			if (UseMinimumXDistance)
			{
				if (this.transform.position.x < _brain.Target.position.x)
				{
					_characterMovement.SetHorizontalMovement(1f);
				}
				else
				{
					_characterMovement.SetHorizontalMovement(-1f);
				}

				if (this.transform.position.y < _brain.Target.position.y)
				{
					_characterMovement.SetVerticalMovement(1f);
				}
				else
				{
					_characterMovement.SetVerticalMovement(-1f);
				}
            
				if (Mathf.Abs(this.transform.position.x - _brain.Target.position.x) < MinimumXDistance)
				{
					_characterMovement.SetHorizontalMovement(0f);
				}

				if (Mathf.Abs(this.transform.position.y - _brain.Target.position.y) < MinimumXDistance)
				{
					_characterMovement.SetVerticalMovement(0f);
				}
			}
			else
			{
				_direction = (_brain.Target.position - this.transform.position).normalized;
				_characterMovement.SetMovement(_direction);
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
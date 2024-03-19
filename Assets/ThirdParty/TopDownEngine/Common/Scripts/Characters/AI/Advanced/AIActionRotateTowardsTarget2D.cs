using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 AI 작업을 사용하면 CharacterRotation2D 기능(ForcedRotation:true로 설정)이 있는 에이전트가 대상을 향하도록 회전할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionRotateTowardsTarget2D")]
	//[RequireComponent(typeof(CharacterRotation2D))]
	public class AIActionRotateTowardsTarget2D : AIAction
	{
		[Header("Lock Rotation")]
        /// whetherX 회전을 잠글지 여부입니다. false로 설정하면 모델이 x축을 기준으로 회전하여 위 또는 아래를 조준합니다.
        [Tooltip("X 회전을 잠글지 여부입니다. false로 설정하면 모델이 x축을 기준으로 회전하여 위 또는 아래를 조준합니다.")]
		public bool LockRotationX = false;

		protected CharacterRotation2D _characterRotation2D;
		protected Vector3 _targetPosition;

		/// <summary>
		/// On init we grab our CharacterOrientation3D ability
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterRotation2D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterRotation2D>();
		}

		/// <summary>
		/// On PerformAction we move
		/// </summary>
		public override void PerformAction()
		{
			Rotate();
		}

		/// <summary>
		/// Makes the orientation 3D ability rotate towards the brain target
		/// </summary>
		protected virtual void Rotate()
		{
			if (_brain.Target == null)
			{
				return;
			}
			_targetPosition = _brain.Target.transform.position;
			if (LockRotationX)
			{
				_targetPosition.y = this.transform.position.y;
			}
			_characterRotation2D.ForcedRotationDirection = (_targetPosition - this.transform.position).normalized;
		}
	}
}
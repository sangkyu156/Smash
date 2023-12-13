using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionRotateTowardsTarget3D")]
	//[RequireComponent(typeof(CharacterOrientation3D))]
	public class AIActionRotateTowardsTarget3D : AIAction
	{
		[Header("Lock Rotation")]
        /// X 회전을 잠글지 여부입니다. false로 설정하면 모델이 x축을 기준으로 회전하여 위 또는 아래를 조준합니다.
        [Tooltip("X 회전을 잠글지 여부입니다. false로 설정하면 모델이 x축을 기준으로 회전하여 위 또는 아래를 조준합니다.")]
		public bool LockRotationX = false;

		protected CharacterOrientation3D _characterOrientation3D;
		protected Vector3 _targetPosition;
		protected bool _originalForcedRotation;

        /// <summary>
        /// 초기화 시 CharacterOrientation3D 기능을 확보합니다.
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterOrientation3D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterOrientation3D>();
		}

        /// <summary>
        /// PerformAction에서 우리는 움직입니다.
        /// </summary>
        public override void PerformAction()
		{
			Rotate();
		}

        /// <summary>
        /// 방향 3D 능력이 brain 목표를 향해 회전하도록 만듭니다.
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
			_characterOrientation3D.ForcedRotationDirection = (_targetPosition - this.transform.position).normalized;
        }

		/// <summary>
		/// On enter state we reset our flag
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			if (_characterOrientation3D == null)
			{
				_characterOrientation3D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterOrientation3D>();
			}
			if (_characterOrientation3D != null)
			{
				_originalForcedRotation = _characterOrientation3D.ForcedRotation;
				_characterOrientation3D.ForcedRotation = true;
			}
		}

		/// <summary>
		/// On enter state we reset our flag
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			if (_characterOrientation3D != null)
			{
				_characterOrientation3D.ForcedRotation = _originalForcedRotation;
			}
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 트리거 충돌체에 추가하면 클래스에 들어가는 캐릭터에 이동 속도 배율을 적용할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Movement Zone")]
	public class MovementZone : ButtonActivated
	{
        /// <summary>
        /// 이동 영역의 설정을 저장하는 데 사용되는 클래스 
        /// </summary>
        public class MovementZoneCollidingEntity
		{
			public Character TargetCharacter;
			public CharacterMovement TargetCharacterMovement;
		}

		[Header("Movement Zone")]
		/// the new movement multiplier to apply
		[Tooltip("적용할 새로운 이동 승수")]
		public float MovementSpeedMultiplier = 0.5f;

		protected List<MovementZoneCollidingEntity> CollidingEntities;

		/// <summary>
		/// On init, we initialize our list
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			CollidingEntities = new List<MovementZoneCollidingEntity>();
		}

		/// <summary>
		/// When the button is pressed we start modifying the timescale
		/// </summary>
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction();
			ChangeSpeed();
		}

		/// <summary>
		/// Changes the speed of CharacterMovement equipped characters that enter the zone
		/// </summary>
		public virtual void ChangeSpeed()
		{
			if (_characterButtonActivation == null)
			{
				return;
			}

			MovementZoneCollidingEntity collidingEntity = new MovementZoneCollidingEntity();
			collidingEntity.TargetCharacter = _characterButtonActivation.gameObject.GetComponentInParent<Character>();

			foreach (MovementZoneCollidingEntity entity in CollidingEntities)
			{
				if (entity.TargetCharacter.gameObject == collidingEntity.TargetCharacter.gameObject)
				{
					return;
				}
			}

			collidingEntity.TargetCharacterMovement = _characterButtonActivation.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
			CollidingEntities.Add(collidingEntity);
			collidingEntity.TargetCharacterMovement.SetContextSpeedMultiplier(MovementSpeedMultiplier);
		}

		/// <summary>
		/// When exiting, and if needed, we reset the character's speed and remove it from our list
		/// </summary>
		/// <param name="collider"></param>
		public override void TriggerExitAction(GameObject collider)
		{
			foreach (MovementZoneCollidingEntity collidingEntity in CollidingEntities)
			{
				if (collidingEntity.TargetCharacter.gameObject == collider)
				{
					collidingEntity.TargetCharacterMovement.ResetContextSpeedMultiplier();
					CollidingEntities.Remove(collidingEntity);
					break;
				}
			}
		}
	}
}
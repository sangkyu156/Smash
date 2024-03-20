using System;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 유형화된 피해 영향을 저장하고 정의하는 데 사용되는 클래스: 발생한 피해, 조건 또는 이동 속도 변경 등
    /// </summary>
    [Serializable]
	public class TypedDamage 
	{
		/// the type of damage associated to this definition
		[Tooltip("이 정의와 관련된 손상 유형")]
		public DamageType AssociatedDamageType;
		/// The min amount of health to remove from the player's health
		[Tooltip("플레이어의 체력에서 제거할 최소 체력입니다.")]
		public float MinDamageCaused = 10f;
		/// The max amount of health to remove from the player's health
		[Tooltip("플레이어의 체력에서 제거할 최대 체력")]
		public float MaxDamageCaused = 10f;
		
		/// whether or not this damage, when applied, should force the character into a specified condition
		[Tooltip("이 피해가 적용될 때 캐릭터를 특정 상태로 강제해야 하는지 여부")] 
		public bool ForceCharacterCondition = false;
		/// when in forced character condition mode, the condition to which to swap
		[Tooltip("강제 캐릭터 조건 모드일 때 스왑할 조건")]
		[MMCondition("ForceCharacterCondition", true)]
		public CharacterStates.CharacterConditions ForcedCondition;
		/// when in forced character condition mode, whether or not to disable gravity
		[Tooltip("강제 캐릭터 조건 모드일 때 중력을 비활성화할지 여부")]
		[MMCondition("ForceCharacterCondition", true)]
		public bool DisableGravity = false;
		/// when in forced character condition mode, whether or not to reset controller forces
		[Tooltip("강제 캐릭터 조건 모드에 있을 때 컨트롤러 강제 재설정 여부")]
		[MMCondition("ForceCharacterCondition", true)]
		public bool ResetControllerForces = false;
		/// when in forced character condition mode, the duration of the effect, after which condition will be reverted 
		[Tooltip("강제 캐릭터 상태 모드일 때 효과의 지속 시간, 그 이후에는 상태가 원래대로 돌아갑니다.")]
		[MMCondition("ForceCharacterCondition", true)]
		public float ForcedConditionDuration = 3f;
		
		/// whether or not to apply a movement multiplier to the damaged character
		[Tooltip("손상된 캐릭터에 이동 승수를 적용할지 여부")] 
		public bool ApplyMovementMultiplier = false;
		/// the movement multiplier to apply when ApplyMovementMultiplier is true 
		[Tooltip("ApplyMovementMultiplier가 true일 때 적용할 이동 승수")]
		[MMCondition("ApplyMovementMultiplier", true)]
		public float MovementMultiplier = 0.5f;
		/// the duration of the movement multiplier, if ApplyMovementMultiplier is true
		[Tooltip("ApplyMovementMultiplier가 true인 경우 이동 승수의 지속 시간")]
		[MMCondition("ApplyMovementMultiplier", true)]
		public float MovementMultiplierDuration = 2f;
		
		

		protected int _lastRandomFrame = -1000;
		protected float _lastRandomValue = 0f;

		public virtual float DamageCaused
		{
			get
			{
				if (Time.frameCount != _lastRandomFrame)
				{
					_lastRandomValue = Random.Range(MinDamageCaused, MaxDamageCaused);
					_lastRandomFrame = Time.frameCount;
				}
				return _lastRandomValue;
			}
		} 
	}	
}


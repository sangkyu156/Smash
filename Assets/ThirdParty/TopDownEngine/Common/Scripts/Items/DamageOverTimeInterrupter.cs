using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 선택기를 사용하여 선택하는 캐릭터에 대한 지정된 유형의 모든 피해를 차단합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/DamageOverTimeInterrupter")]
	public class DamageOverTimeInterrupter : PickableItem
	{
		[Header("Damage Over Time Interrupter")]
		/// whether interrupted damage over time should be of a specific type, or if all damage should be interrupted 
		[Tooltip("시간이 지남에 따라 중단된 손상이 특정 유형이어야 하는지 아니면 모든 손상이 중단되어야 하는지 여부")]
		public bool InterruptByTypeOnly = false;
		/// The type of damage over time this should interrupt
		[Tooltip("시간이 지남에 따라 중단되어야 하는 손상 유형")]
		[MMCondition("InterruptByTypeOnly", true)]
		public DamageType TargetDamageType;
		/// if this is true, only player characters can pick this up
		[Tooltip("이것이 사실이라면 플레이어 캐릭터만이 이것을 선택할 수 있습니다.")]
		public bool OnlyForPlayerCharacter = true;

		/// <summary>
		/// Triggered when something collides with the stimpack
		/// </summary>
		/// <param name="collider">Other.</param>
		protected override void Pick(GameObject picker)
		{
			Character character = picker.gameObject.MMGetComponentNoAlloc<Character>();
			if (OnlyForPlayerCharacter && (character != null) && (_character.CharacterType != Character.CharacterTypes.Player))
			{
				return;
			}

			Health characterHealth = picker.gameObject.MMGetComponentNoAlloc<Health>();
			// else, we give health to the player
			if (characterHealth != null)
			{
				if (InterruptByTypeOnly)
				{
					characterHealth.InterruptAllDamageOverTimeOfType(TargetDamageType);	
				}
				else
				{
					characterHealth.InterruptAllDamageOverTime();	
				}
			}            
		}
	}
}
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 선택 시 체력을 돌려주는 스팀팩/건강 보너스
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Stimpack")]
	public class Stimpack : PickableItem
	{
		[Header("Stimpack")]
		/// The amount of points to add when collected
		[Tooltip("적립 시 추가되는 포인트 금액")]
		public float HealthToGive = 10f;
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
				characterHealth.ReceiveHealth(HealthToGive, gameObject);
			}            
		}
	}
}
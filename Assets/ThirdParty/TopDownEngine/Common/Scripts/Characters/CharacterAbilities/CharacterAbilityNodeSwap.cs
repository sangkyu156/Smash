using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 능력을 사용하면 전체 능력 노드를 매개변수에 설정된 능력 노드로 교체할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/CharacterAbilityNodeSwap")]
	public class CharacterAbilityNodeSwap : CharacterAbility
	{
		[Header("Ability Node Swap")]

        /// 능력이 실행될 때 이 캐릭터의 능력 노드 세트를 대체할 GameObject 목록
        [Tooltip("능력이 실행될 때 이 캐릭터의 능력 노드 세트를 대체할 GameObject 목록")]
		public List<GameObject> AdditionalAbilityNodes;

        /// <summary>
        /// 플레이어가 SwitchCharacter 버튼을 누르면 능력을 교체합니다.
        /// 이 기능은 SwitchCharacter 입력을 재사용하여 입력 항목의 곱셈을 방지하지만 이 메서드를 재정의하여 전용 항목을 추가해도 됩니다.
        /// </summary>
        protected override void HandleInput()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            
			if (_inputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwapAbilityNodes();
			}
		}

		/// <summary>
		/// Disables the old ability nodes, swaps with the new, and enables them
		/// </summary>
		public virtual void SwapAbilityNodes()
		{
			foreach (GameObject node in _character.AdditionalAbilityNodes)
			{
				node.gameObject.SetActive(false);
			}
            
			_character.AdditionalAbilityNodes = AdditionalAbilityNodes;

			foreach (GameObject node in _character.AdditionalAbilityNodes)
			{
				node.gameObject.SetActive(true);
			}

			_character.CacheAbilities();
		}
	}
}
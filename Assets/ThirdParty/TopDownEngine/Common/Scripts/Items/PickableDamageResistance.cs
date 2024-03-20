using System;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 선택기를 사용하여 선택하는 캐릭터에 새로운 저항을 생성하거나 기존 저항을 활성화/비활성화합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Pickable Damage Resistance")]
	public class PickableDamageResistance : PickableItem
	{
		public enum Modes { Create, ActivateByLabel, DisableByLabel }
		
		[Header("Damage Resistance")]
		/// The chosen mode to interact with resistance, either creating one, activating one or disabling one
		[Tooltip("저항 생성, 활성화 또는 비활성화 등 저항과 상호 작용하기 위해 선택한 모드")]
		public Modes Mode = Modes.ActivateByLabel;
		
		/// If activating or disabling by label, the exact label of the target resistance
		[Tooltip("라벨로 활성화 또는 비활성화하는 경우 목표 저항의 정확한 라벨")]
		[MMEnumCondition("Mode", (int)Modes.ActivateByLabel, (int)Modes.DisableByLabel)]
		public string TargetLabel = "SomeResistance";
		
		/// in create mode, the name of the new game object to create to host the new resistance
		[Tooltip("생성 모드에서 새로운 저항을 호스팅하기 위해 생성할 새 게임 개체의 이름")]
		[MMEnumCondition("Mode", (int)Modes.Create)]
		public string NewResistanceNodeName = "NewResistance";
		/// in create mode, a DamageResistance to copy and give to the new node. Usually you'll want to create a new DamageResistance component on your picker, and drag it in this slot
		[Tooltip("생성 모드에서는 DamageResistance를 복사하여 새 노드에 제공합니다. 일반적으로 선택기에 새 DamageResistance 구성 요소를 생성하고 이 슬롯에 드래그하려고 합니다.")]
		[MMEnumCondition("Mode", (int)Modes.Create)]
		public DamageResistance DamageResistanceToGive;
		
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
			if (characterHealth == null)
			{
				return;
			}          
			DamageResistanceProcessor processor = characterHealth.TargetDamageResistanceProcessor;
			if (processor == null)
			{
				return;
			}

			switch (Mode)
			{
				case Modes.ActivateByLabel:
					processor.SetResistanceByLabel(TargetLabel, true);
					break;
				case Modes.DisableByLabel:
					processor.SetResistanceByLabel(TargetLabel, false);
					break;
				case Modes.Create:
					if (DamageResistanceToGive == null) { return; }
					GameObject newResistance = new GameObject();
					newResistance.transform.SetParent(processor.transform);
					newResistance.name = NewResistanceNodeName;
					DamageResistance newResistanceComponent = MMHelpers.CopyComponent<DamageResistance>(DamageResistanceToGive, newResistance);
					processor.DamageResistanceList.Add(newResistanceComponent);
					break;
			}
		}
	}
}
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 트리거 상자 충돌기 2D가 있는 개체에 이 클래스를 추가하면 선택 가능한 개체가 되어 캐릭터의 능력을 허용하거나 금지할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Pickable Ability")]
	public class PickableAbility : PickableItem
	{
		public enum Methods
		{
			Permit,
			Forbid
		}

		[Header("Pickable Ability")] 
		/// whether this object should permit or forbid an ability when picked
		[Tooltip("이 개체를 선택할 때 능력을 허용할지 금지할지 여부")]
		public Methods Method = Methods.Permit;
		/// whether or not only characters of Player type should be able to pick this 
		[Tooltip("플레이어 유형의 캐릭터만 선택할 수 있는지 여부")]
		public bool OnlyPickableByPlayerCharacters = true;

		[HideInInspector] public string AbilityTypeAsString;

		/// <summary>
		/// Checks if the object is pickable 
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		protected override bool CheckIfPickable()
		{
			_character = _collidingObject.GetComponent<Character>();

			// if what's colliding with the coin ain't a characterBehavior, we do nothing and exit
			if (_character == null)
			{
				return false;
			}

			if (OnlyPickableByPlayerCharacters && (_character.CharacterType != Character.CharacterTypes.Player))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// on pick, we permit or forbid our target ability
		/// </summary>
		protected override void Pick(GameObject picker)
		{
			if (_character == null)
			{
				return;
			}
			bool newState = (Method == Methods.Permit);
			CharacterAbility ability = _character.FindAbilityByString(AbilityTypeAsString);
			if (ability != null)
			{
				ability.PermitAbility(newState);
			}
		}
	}
}
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 다시 로드가 필요한 경우 이 결정은 true를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionReloadNeeded")]
	public class AIDecisionReloadNeeded : AIDecision
	{
		protected CharacterHandleWeapon _characterHandleWeapon;

		/// <summary>
		/// On Init we store our CharacterHandleWeapon
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
		}

		/// <summary>
		/// On Decide we return true if a reload is needed
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			if (_characterHandleWeapon == null)
			{
				return false;
			}

			if (_characterHandleWeapon.CurrentWeapon == null)
			{
				return false;
			}

			return _characterHandleWeapon.CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadNeeded;
		}
	}
}
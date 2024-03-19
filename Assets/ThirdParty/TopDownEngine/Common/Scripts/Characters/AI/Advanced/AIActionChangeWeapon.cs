using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;


namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 동작은 캐릭터가 다른 무기로 전환하도록 강제하는 데 사용됩니다. 무기 구조물을 NewWeapon 슬롯으로 드래그하기만 하면 됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionChangeWeapon")]
	//[RequireComponent(typeof(CharacterHandleWeapon))]
	public class AIActionChangeWeapon : AIAction
	{
        /// 해당 작업을 수행할 때 장착할 새로운 무기
        [Tooltip("해당 작업을 수행할 때 장착할 새로운 무기")]
		public Weapon NewWeapon;

		protected CharacterHandleWeapon _characterHandleWeapon;
		protected int _change = 0;

		/// <summary>
		/// On init we grab our CharacterHandleWeapon ability
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
		}

		/// <summary>
		/// On PerformAction we change weapon
		/// </summary>
		public override void PerformAction()
		{
			ChangeWeapon();
		}

		/// <summary>
		/// Performs the weapon change
		/// </summary>
		protected virtual void ChangeWeapon()
		{
			if (_change < 1)
			{
				if (NewWeapon == null)
				{
					_characterHandleWeapon.ChangeWeapon(NewWeapon, "");
				}
				else
				{
					_characterHandleWeapon.ChangeWeapon(NewWeapon, NewWeapon.name);
				}
                
				_change++;
			}
		}

		/// <summary>
		/// Resets our counter
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_change = 0;
		}
	}
}
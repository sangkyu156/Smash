using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 기절 구역은 CharacterStun 능력이 있는 모든 캐릭터를 기절시킵니다.
    /// </summary>
    public class StunZone : TopDownMonoBehaviour
	{
        /// 가능한 기절 모드: 
		/// Forever: CharacterStun 구성 요소에서 StunExit가 호출될 때까지 기절합니다. 
		/// ForDuration: 일정 기간 동안 기절한 다음 캐릭터가 스스로 기절을 종료합니다.
        public enum StunModes { Forever, ForDuration }

		[Header("Stun Zone")]
		// the layers that will be stunned by this object
		[Tooltip("이 개체에 의해 기절할 레이어")]
		public LayerMask TargetLayerMask;
		/// the chosen stun mode (Forever : stuns until StunExit is called on the CharacterStun component, ForDuration : stuns for a duration, and then the character will exit stun on its own)
		[Tooltip("선택한 기절 모드(영원히: CharacterStun 구성 요소에서 StunExit가 호출될 때까지 기절하고, ForDuration: 일정 기간 동안 기절한 다음 캐릭터가 스스로 기절을 종료합니다)")] 
		public StunModes StunMode = StunModes.ForDuration;
		/// if in ForDuration mode, the duration of the stun in seconds
		[Tooltip("ForDuration 모드인 경우 기절 지속 시간(초)")]
		[MMEnumCondition("StunMode", (int)StunModes.ForDuration)]
		public float StunDuration = 2f;
		/// whether or not to disable the zone after the stun has happened
		[Tooltip("기절이 발생한 후 구역을 비활성화할지 여부")]
		public bool DisableZoneOnStun = true;

		protected Character _character;
		protected CharacterStun _characterStun;
        
		/// <summary>
		/// When colliding with a gameobject, we make sure it's a target, and if yes, we stun it
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void Colliding(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{

				return;
			}

			_character = collider.GetComponent<Character>();
			if (_character != null) { _characterStun = _character.FindAbility<CharacterStun>(); }

			if (_characterStun == null)
			{
				return;
			}
            
			if (StunMode == StunModes.ForDuration)
			{
				_characterStun.StunFor(StunDuration);
			}
			else
			{
				_characterStun.Stun();
			}
            
			if (DisableZoneOnStun)
			{
				this.gameObject.SetActive(false);
			}
		}
        
		/// <summary>
		/// When a collision with the player is triggered, we give damage to the player and knock it back
		/// </summary>
		/// <param name="collider">what's colliding with the object.</param>
		// public virtual void OnTriggerStay2D(Collider2D collider)
		// {
		//     Colliding(collider.gameObject);
		// }

		/// <summary>
		/// On trigger enter 2D, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>S
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger stay, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		// public virtual void OnTriggerStay(Collider collider)
		// {
		//     Colliding(collider.gameObject);
		// }

		/// <summary>
		/// On trigger enter, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerEnter(Collider collider)
		{
			Colliding(collider.gameObject);
		}
	}    
}
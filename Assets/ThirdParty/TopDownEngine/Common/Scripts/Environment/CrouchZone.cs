using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 영역을 트리거 충돌체에 추가하면 입장 시 3D 캐릭터가 자동으로 웅크리는 동작이 발생합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
	[AddComponentMenu("TopDown Engine/Environment/Crouch Zone")]
	public class CrouchZone : TopDownMonoBehaviour
	{
		protected CharacterCrouch _characterCrouch;

		/// <summary>
		/// On start we make sure our collider is set to trigger
		/// </summary>
		protected virtual void Start()
		{
			this.gameObject.MMGetComponentNoAlloc<Collider>().isTrigger = true;
		}

		/// <summary>
		/// On enter we force crouch if we can
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter(Collider collider)
		{
			_characterCrouch = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterCrouch>();
			if (_characterCrouch != null)
			{
				_characterCrouch.StartForcedCrouch();
			}
		}

		/// <summary>
		/// On exit we stop force crouching
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerExit(Collider collider)
		{
			_characterCrouch = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterCrouch>();
			if (_characterCrouch != null)
			{
				_characterCrouch.StopForcedCrouch();
			}
		}
	}
}
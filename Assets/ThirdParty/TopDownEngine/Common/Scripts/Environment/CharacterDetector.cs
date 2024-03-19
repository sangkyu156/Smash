using System;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 **TRIGGER** collider2D에 추가하면 캐릭터가 이 클래스에 들어올 때 이를 알려주고 결과적으로 액션을 트리거할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Character Detector")]
	[RequireComponent(typeof(Collider2D))]
	public class CharacterDetector : TopDownMonoBehaviour
	{
        /// 이것이 사실입니다. 이 기능이 작동하려면 캐릭터에 Player 태그가 지정되어야 합니다.
        [Tooltip("이것이 사실입니다. 이 기능이 작동하려면 캐릭터에 Player 태그가 지정되어야 합니다.")]
		public bool RequiresPlayer = true;
        /// 이것이 사실이라면 캐릭터(및 위 설정에 따른 플레이어)가 해당 영역에 있는 것입니다.
        [MMReadOnly]
		[Tooltip("이것이 사실이라면 캐릭터(및 위 설정에 따른 플레이어)가 해당 영역에 있는 것입니다.")]
		public bool CharacterInArea = false;
        /// 대상 캐릭터가 해당 지역에 들어올 때 발생하는 UnityEvent
        [Tooltip("대상 캐릭터가 해당 지역에 들어올 때 발생하는 UnityEvent")]
		public UnityEvent OnEnter;
        /// 대상 캐릭터가 해당 영역에 머무르는 동안 발생하는 UnityEvent
        [Tooltip("대상 캐릭터가 해당 영역에 머무르는 동안 발생하는 UnityEvent")]
		public UnityEvent OnStay;
        /// 대상 캐릭터가 영역을 벗어날 때 발생하는 UnityEvent
        [Tooltip("대상 캐릭터가 영역을 벗어날 때 발생하는 UnityEvent")]
		public UnityEvent OnExit;

		protected Collider2D _collider2D;
		protected Collider _collider;
		protected Character _character;

		/// <summary>
		/// On Start we grab our collider2D and set it to trigger in case we forgot AGAIN to set it to trigger
		/// </summary>
		protected virtual void Start()
		{
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			_collider = this.gameObject.GetComponent<Collider>();
			if (_collider2D != null) { _collider2D.isTrigger = true; }
			if (_collider != null) { _collider.isTrigger = true; }
		}        

		/// <summary>
		/// When a character enters we turn our state to true
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter2D(Collider2D collider) { OnTriggerEnterProxy(collider.gameObject); }
		protected void OnTriggerEnter(Collider collider) { OnTriggerEnterProxy(collider.gameObject); }

		protected virtual void OnTriggerEnterProxy(GameObject collider)
		{
			if (!TargetFound(collider))
			{
				return;
			}

			CharacterInArea = true;

			if (OnEnter != null)
			{
				OnEnter.Invoke();
			}
		}

		/// <summary>
		/// While a character stays we keep our boolean true
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerStay2D(Collider2D collider) { OnTriggerStayProxy(collider.gameObject); }
		protected void OnTriggerStay(Collider collider) { OnTriggerStayProxy(collider.gameObject); }

		protected virtual void OnTriggerStayProxy(GameObject collider)
		{
			if (!TargetFound(collider))
			{
				return;
			}
            
			CharacterInArea = true;

			if (OnStay != null)
			{
				OnStay.Invoke();
			}
		}

		/// <summary>
		/// When a character exits we reset our boolean
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerExit2D(Collider2D collider) { OnTriggerExitProxy(collider.gameObject); }
		protected void OnTriggerExit(Collider collider) { OnTriggerExitProxy(collider.gameObject); }

		protected virtual void OnTriggerExitProxy(GameObject collider)
		{
			if (!TargetFound(collider))
			{
				return;
			}
            
			CharacterInArea = false;

			if (OnExit != null)
			{
				OnExit.Invoke();
			}
		}

		/// <summary>
		/// Returns true if the collider set in parameter is the targeted type, false otherwise
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual bool TargetFound(GameObject collider)
		{
			_character = collider.gameObject.MMGetComponentNoAlloc<Character>();
            
			if (_character == null)
			{
				return false;
			}

			if (RequiresPlayer && (_character.CharacterType != Character.CharacterTypes.Player))
			{
				return false;
			}

			return true;
		}
	}
}
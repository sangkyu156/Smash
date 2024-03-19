using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 도우미 클래스는 프로젝트가 이전 입력 시스템을 사용하는지 아니면 새로운 입력 시스템을 사용하는지에 따라 적절한 입력 모듈 추가를 처리합니다.
    /// </summary>
    public class MMAutoInputModule : MonoBehaviour
	{
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		protected InputSystemUIInputModule _module;
		#endif

		protected GameObject _eventSystemGameObject;
		
		/// <summary>
		/// On Awake, we initialize the input module
		/// </summary>
		protected virtual void Awake()
		{
			StartCoroutine(InitializeInputModule());
		}

		/// <summary>
		/// We add the appropriate input module
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator InitializeInputModule()
		{
			EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();

			if (eventSystem == null)
			{
				yield break;
			}
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				_eventSystemGameObject = eventSystem.gameObject;
				_module = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
				// thanks new input system.
				yield return null;
				_module.enabled = false;
				yield return null;
				_module.enabled = true;
			#else
			eventSystem.gameObject.AddComponent<StandaloneInputModule>();
			#endif
			yield return null;
		}
	}	
}
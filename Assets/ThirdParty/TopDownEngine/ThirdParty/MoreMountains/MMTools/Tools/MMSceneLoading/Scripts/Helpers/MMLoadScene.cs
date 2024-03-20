using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 객체에 이 구성 요소를 추가하고 해당 인스펙터에서 장면 이름을 지정한 다음 LoadScene()을 호출하여 원하는 장면을 로드합니다.
    /// </summary>
    public class MMLoadScene : MonoBehaviour 
	{
        /// 장면을 로드할 수 있는 모드. Unity의 기본 API 또는 MoreMountains의 LoadingSceneManager
        public enum LoadingSceneModes { UnityNative, MMSceneLoadingManager, MMAdditiveSceneLoadingManager }

		/// the name of the scene that needs to be loaded when LoadScene gets called
		[Tooltip("LoadScene이 호출될 때 로드되어야 하는 장면의 이름")]
		public string SceneName;
        /// 장면이 Unity의 기본 API를 사용하여 로드되는지 아니면 MoreMountains의 방식을 사용하여 로드되는지 정의합니다.
        [Tooltip("장면이 Unity의 기본 API를 사용하여 로드되는지 아니면 MoreMountains의 방식을 사용하여 로드되는지 정의합니다.")]
		public LoadingSceneModes LoadingSceneMode = LoadingSceneModes.UnityNative;

		/// <summary>
		/// Loads the scene specified in the inspector
		/// </summary>
		public virtual void LoadScene()
		{
			switch (LoadingSceneMode)
			{
				case LoadingSceneModes.UnityNative:
					SceneManager.LoadScene (SceneName);
					break;
				case LoadingSceneModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene (SceneName);
					break;
				case LoadingSceneModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(SceneName);
					break;
			}
		}
	}
}
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
	/// Add this component on an object, specify a scene name in its inspector, and call LoadScene() to load the desired scene.
	/// </summary>
	public class MMLoadScene : MonoBehaviour 
	{
        /// 장면을 로드할 수 있는 모드. Unity의 기본 API 또는 MoreMountains의 LoadingSceneManager
        public enum LoadingSceneModes { UnityNative, MMSceneLoadingManager, MMAdditiveSceneLoadingManager }

		/// the name of the scene that needs to be loaded when LoadScene gets called
		[Tooltip("the name of the scene that needs to be loaded when LoadScene gets called")]
		public string SceneName;
        /// 장면이 Unity의 기본 API를 사용하여 로드되는지 아니면 MoreMountains의 방식을 사용하여 로드되는지 정의합니다.
        [Tooltip("defines whether the scene will be loaded using Unity's native API or MoreMountains' way")]
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
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성요소를 사용하면 액세스하고 로드할 수 있는 레벨을 정의할 수 있습니다. 주로 레벨 맵 장면에서 사용됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/LevelSelector")]
	public class LevelSelector : TopDownMonoBehaviour
	{
		/// the exact name of the target level
		[Tooltip("목표 레벨의 정확한 이름")]
		public string LevelName;

		/// if this is true, GoToLevel will ignore the LevelManager and do a direct call
		[Tooltip("이것이 사실이라면 GoToLevel은 LevelManager를 무시하고 직접 호출을 수행합니다.")]
		public bool DoNotUseLevelManager = false;

		/// if this is true, any persistent character will be destroyed when loading the new level
		[Tooltip("이것이 사실이라면 새 레벨을 로드할 때 모든 영구 캐릭터가 삭제됩니다.")]
		public bool DestroyPersistentCharacter = false;

		/// <summary>
		/// Loads the level specified in the inspector
		/// </summary>
		public virtual void GoToLevel()
		{
			LoadScene(LevelName);
		}

		/// <summary>
		/// Loads a new scene, either via the LevelManager or not
		/// </summary>
		/// <param name="newSceneName"></param>
		protected virtual void LoadScene(string newSceneName)
		{
			if (DestroyPersistentCharacter)
			{
				GameManager.Instance.DestroyPersistentCharacter();
			}
			
			if (GameManager.Instance.Paused)
			{
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			}
				
			if (DoNotUseLevelManager)
			{
				MMAdditiveSceneLoadingManager.LoadScene(newSceneName);    
			}
			else
			{
				LevelManager.Instance.GotoLevel(newSceneName);   
			}
		}

		/// <summary>
		/// Restarts the current level, without reloading the whole scene
		/// </summary>
		public virtual void RestartLevel()
		{
			if (GameManager.Instance.Paused)
			{
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			}            
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnStarted, null);
		}

		/// <summary>
		/// Reloads the current level
		/// </summary>
		public virtual void ReloadLevel()
		{
			// we trigger an unPause event for the GameManager (and potentially other classes)
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			LoadScene(SceneManager.GetActiveScene().name);
		}
		
	}
}
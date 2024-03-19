using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 선택한 캐릭터를 저장하고 선택적으로 다른 장면으로 이동할 수 있도록 버튼에 이 구성 요소를 추가하세요(예:)
    /// DeadlineCharacterSelection 데모 장면에서 사용 예를 볼 수 있습니다.
    /// </summary>
    public class CharacterSelector : TopDownMonoBehaviour 
	{
		/// The name of the scene to go to when calling LoadNextScene()
		[Tooltip("LoadNextScene() 호출 시 이동할 씬 이름")]
		public string DestinationSceneName;
		/// The character prefab to store in the GameManager
		[Tooltip("GameManager에 저장할 캐릭터 프리팹")]
		public Character CharacterPrefab;

		/// <summary>
		/// Stores the selected character prefab in the Game Manager
		/// </summary>
		public virtual void StoreCharacterSelection()
		{
			GameManager.Instance.StoreSelectedCharacter (CharacterPrefab);
		}

		/// <summary>
		/// Loads the next scene after having stored the selected character in the Game Manager.
		/// </summary>
		public virtual void LoadNextScene()
		{
			StoreCharacterSelection ();
			MMSceneLoadingManager.LoadScene(DestinationSceneName);
		}
	}
}
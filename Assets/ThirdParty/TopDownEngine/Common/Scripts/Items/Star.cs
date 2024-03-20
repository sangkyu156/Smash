using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 선택할 수 있는 별, 선택하면 TopDownEngineStarEvent를 트리거합니다.
    /// 해당 이벤트를 처리할 무언가를 구현하는 것은 사용자에게 달려 있습니다.
    /// 이에 대한 예는 DeadlineStar 및 DeadlineProgressManager를 참조하세요.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Star")]
	public class Star : PickableItem
	{
		/// the ID of this star, used by the progress manager to know which one got unlocked
		[Tooltip("어느 별이 잠금 해제되었는지 알기 위해 진행 관리자가 사용하는 이 별의 ID")]
		public int StarID;

		/// <summary>
		/// Triggered when something collides with the star
		/// </summary>
		/// <param name="collider">Other.</param>
		protected override void Pick(GameObject picker) 
		{
			// we send a new star event for anyone to catch 
			TopDownEngineStarEvent.Trigger(SceneManager.GetActiveScene().name, StarID);
		}
	}

	public struct TopDownEngineStarEvent
	{
		public string SceneName;
		public int StarID;

		public TopDownEngineStarEvent(string sceneName, int starID)
		{
			SceneName = sceneName;
			StarID = starID;
		}

		static TopDownEngineStarEvent e;
		public static void Trigger(string sceneName, int starID)
		{
			e.SceneName = sceneName;
			e.StarID = starID;
			MMEventManager.TriggerEvent(e);
		}
	}
}
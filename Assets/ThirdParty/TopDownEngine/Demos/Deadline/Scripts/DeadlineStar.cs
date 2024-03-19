using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 선택하면 이벤트를 트리거하고 이전에 수집된 경우 자체적으로 비활성화되는 선택 가능한 별입니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Deadline Star")]
	public class DeadlineStar : Star
	{
		/// <summary>
		/// On Start we disable our star if needed
		/// </summary>
		protected override void Start()
		{
			base.Start ();
			DisableIfAlreadyCollected ();
		}

		/// <summary>
		/// Disables the star if it's already been collected in the past.
		/// </summary>
		protected virtual void DisableIfAlreadyCollected ()
		{
			foreach (DeadlineScene scene in DeadlineProgressManager.Instance.Scenes)
			{
				if (scene.SceneName == SceneManager.GetActiveScene().name)
				{
					if (scene.CollectedStars.Length >= StarID)
					{
						if (scene.CollectedStars[StarID])
						{
							Disable ();
						}
					}
				}
			}
		}

		/// <summary>
		/// Disable this star.
		/// </summary>
		protected virtual void Disable()
		{
			this.gameObject.SetActive (false);
		}
	}
}
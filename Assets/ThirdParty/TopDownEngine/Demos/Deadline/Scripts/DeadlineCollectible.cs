using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스는 Deadline 데모에서 수집품을 표시하고 이전 레벨 방문에서 수집한 경우 비활성화하는 데 사용됩니다.
    /// </summary>
    public class DeadlineCollectible : TopDownMonoBehaviour
	{
		public string CollectibleName = "";
		
		/// <summary>
		/// On Start we disable our game object if needed
		/// </summary>
		protected virtual void Start()
		{
			DisableIfAlreadyCollected ();
		}

		/// <summary>
		/// Call this to collect this collectible and keep track of it in the future
		/// </summary>
		public virtual void Collect()
		{
			DeadlineProgressManager.Instance.FindCollectible (CollectibleName);
		}

		/// <summary>
		/// Disables the game object if it's already been collected in the past.
		/// </summary>
		protected virtual void DisableIfAlreadyCollected ()
		{
			if (DeadlineProgressManager.Instance.FoundCollectibles == null)
			{
				return;
			}
			foreach (string collectible in DeadlineProgressManager.Instance.FoundCollectibles)
			{
				if (collectible == this.CollectibleName)
				{
					Disable ();
				}
			}
		}

		/// <summary>
		/// Disable this game object.
		/// </summary>
		protected virtual void Disable()
		{
			this.gameObject.SetActive (false);
		}
	    
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스는 Explodude 데모 장면의 시작 화면을 처리합니다.
    /// </summary>
    public class ExplodudesStartScreen : TopDownMonoBehaviour
	{
		/// <summary>
		/// On start, enables all its children
		/// </summary>
		protected virtual void Start()
		{
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Called by an animator to turn the startscreen off after it plays
		/// </summary>
		public virtual void DisableStartScreen()
		{
			this.gameObject.SetActive(false);
		}
	}
}
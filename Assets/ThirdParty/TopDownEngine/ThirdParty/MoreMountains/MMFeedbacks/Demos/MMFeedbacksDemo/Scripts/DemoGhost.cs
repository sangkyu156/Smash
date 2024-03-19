using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// MMFeedback의 데모 고스트에 사용되는 클래스
    /// </summary>
    public class DemoGhost : MonoBehaviour
	{
		/// <summary>
		/// Called via animation event, disables the object
		/// </summary>
		public virtual void OnAnimationEnd()
		{
			this.gameObject.SetActive(false);
		}
	}
}
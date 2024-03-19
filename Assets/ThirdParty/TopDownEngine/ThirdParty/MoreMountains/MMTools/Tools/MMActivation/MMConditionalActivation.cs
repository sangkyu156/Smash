using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 게임 개체에 추가하면 다른 모든 대상이 비활성화된 후에도 대상 모노를 활성화할 수 있습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Activation/MMConditionalActivation")]
	public class MMConditionalActivation : MonoBehaviour
	{
		/// a list of monos to enable
		public MonoBehaviour[] EnableThese;
		/// a list of all the monos that have to have been disabled first
		public MonoBehaviour[] AfterTheseAreAllDisabled;

		protected bool _enabled = false;

		/// <summary>
		/// On update, we check if we should disable
		/// </summary>
		protected virtual void Update()
		{
			if (_enabled)
			{
				return;
			}

			bool allDisabled = true;
			foreach (MonoBehaviour component in AfterTheseAreAllDisabled)
			{
				if (component.isActiveAndEnabled)
				{
					allDisabled = false;
				}
			}
			if (allDisabled)
			{
				foreach (MonoBehaviour component in EnableThese)
				{
					component.enabled = true;
				}
				_enabled = true;
			}
		}
	}
}
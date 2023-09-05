using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 도우미를 개체에 추가하면 활성화 시 포커스가 해당 개체로 설정됩니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/GUI/MMGetFocusOnEnable")]
	public class MMGetFocusOnEnable : MonoBehaviour
	{
		protected virtual void OnEnable()
		{
			EventSystem.current.SetSelectedGameObject(this.gameObject, null);
		}
	}
}
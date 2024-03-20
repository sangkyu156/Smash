using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 매우 간단한 클래스는 현재 있는 GameObject(또는 인스펙터에서 비어 있는 경우 대상 객체)를 활성 또는 비활성으로 전환하는 메서드를 노출합니다.
    /// </summary>
    public class MMToggleActive : MonoBehaviour
	{
		[Header("Target - leave empty for self")]
		/// the target gameobject to toggle. Leave blank for auto grab
		public GameObject TargetGameObject;

		/// a test button
		[MMInspectorButton("ToggleActive")]        
		public bool ToggleActiveButton;

		/// <summary>
		/// On awake, grabs self if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (TargetGameObject == null)
			{
				TargetGameObject = this.gameObject;
			}
		}

		/// <summary>
		/// Toggles the target gameobject's active state
		/// </summary>
		public virtual void ToggleActive()
		{
			TargetGameObject.SetActive(!TargetGameObject.activeInHierarchy);
		}
	}
}
using System;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 근접 관리자가 관리하는 것으로 표시하기 위해 장면의 개체에 추가하는 클래스입니다.
    /// </summary>
    public class ProximityManaged : TopDownMonoBehaviour
	{
		[Header("Thresholds")]
		/// the distance from the proximity center (the player) under which the object should be enabled
		[Tooltip("개체를 활성화해야 하는 근접 중심(플레이어)으로부터의 거리")]
		public float EnableDistance = 35f;
		/// the distance from the proximity center (the player) after which the object should be disabled
		[Tooltip("객체가 비활성화되어야 하는 근접 중심(플레이어)으로부터의 거리")]
		public float DisableDistance = 45f;

		/// whether or not this object was disabled by the ProximityManager
		[MMReadOnly]
		[Tooltip("ProximityManager에 의해 이 객체가 비활성화되었는지 여부")]
		public bool DisabledByManager;

		[Header("Debug")] 
		/// a debug manager to add this object to, only used for debug
		[Tooltip("이 개체를 추가할 디버그 관리자이며 디버그에만 사용됩니다.")]
		public ProximityManager DebugProximityManager;
		/// a debug button to add this object to the debug manager
		[MMInspectorButton("DebugAddObject")]
		public bool AddButton;

		public ProximityManager Manager { get; set; }
		
		/// <summary>
		/// A debug method used to add this object to a proximity manager
		/// </summary>
		public virtual void DebugAddObject()
		{
			DebugProximityManager.AddControlledObject(this);
		}

		/// <summary>
		/// On enable, we register to our manager
		/// </summary>
		private void OnEnable()
		{
			if (Manager != null)
			{
				return;
			}
			
			ProximityManager proximityManager = ProximityManager.Current;
			if (proximityManager != null) 
			{
				var targets = proximityManager.ControlledObjects;
				if (targets != null && targets.Count > 0) 
				{
					if (Manager == null)
					{
						Manager = proximityManager;
						proximityManager.ControlledObjects.Add(this);
					}
				}
			}
		}

		/// <summary>
		/// On Destroy we let our manager know we're gone
		/// </summary>
		private void OnDestroy()
		{
			if (Manager != null && Manager.ControlledObjects != null)
			{
				Manager.ControlledObjects.Remove(this);
			}
		}
	}
}
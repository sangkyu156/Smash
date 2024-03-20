using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 객체에 추가하면 선택 시 지정된 작업이 트리거됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Pickable Action")]
	public class PickableAction : PickableItem
	{
		/// the action(s) to trigger when picked
		[Tooltip("선택 시 트리거할 작업")]
		public UnityEvent PickEvent;

		/// <summary>
		/// Triggered when something collides with the object
		/// </summary>
		/// <param name="collider">Other.</param>
		protected override void Pick(GameObject picker)
		{
			base.Pick(picker);
			if (PickEvent != null)
			{
				PickEvent.Invoke();
			}
		}
	}
}
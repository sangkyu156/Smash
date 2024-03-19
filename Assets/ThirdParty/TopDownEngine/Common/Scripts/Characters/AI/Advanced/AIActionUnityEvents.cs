using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 액션은 UnityEvent를 트리거하는 데 사용됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionUnityEvents")]
	public class AIActionUnityEvents : AIAction
	{
        /// AIBrain이 이 작업을 수행할 때 트리거할 UnityEvent입니다.
        [Tooltip("AIBrain이 이 작업을 수행할 때 트리거할 UnityEvent입니다.")]
		public UnityEvent TargetEvent;
        /// 이것이 false인 경우 Unity 이벤트는 PerformAction마다(기본적으로 이 상태에 있는 동안 모든 프레임) 트리거되고, 그렇지 않으면 상태에 들어갈 때 한 번만 재생됩니다.
        [Tooltip("이것이 false인 경우 Unity 이벤트는 PerformAction마다(기본적으로 이 상태에 있는 동안 모든 프레임) 트리거되고, 그렇지 않으면 상태에 들어갈 때 한 번만 재생됩니다.")]
		public bool OnlyPlayWhenEnteringState = true;

		protected bool _played = false;

		/// <summary>
		/// On PerformAction we trigger our event
		/// </summary>
		public override void PerformAction()
		{
			TriggerEvent();
		}

		/// <summary>
		/// Triggers the target event
		/// </summary>
		protected virtual void TriggerEvent()
		{
			if (OnlyPlayWhenEnteringState && _played)
			{
				return;
			}

			if (TargetEvent != null)
			{
				TargetEvent.Invoke();
				_played = true;
			}
		}

		/// <summary>
		/// On enter state we initialize our _played bool
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_played = false;
		}
	}
}
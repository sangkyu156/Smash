using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;


namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 작업은 MMFeedbacks를 재생하는 데 사용됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMMFeedbacks")]
	public class AIActionMMFeedbacks : AIAction
	{
        /// AIBrain이 이 작업을 수행할 때 재생할 MMFeedbacks
        [Tooltip("AIBrain이 이 작업을 수행할 때 재생할 MMFeedbacks")]
		public MMFeedbacks TargetFeedbacks;
        /// false인 경우 피드백은 PerformAction마다(기본적으로 이 상태에 있는 동안 프레임마다) 재생되고, 그렇지 않으면 상태에 들어갈 때 한 번만 재생됩니다.
        [Tooltip("false인 경우 피드백은 PerformAction마다(기본적으로 이 상태에 있는 동안 프레임마다) 재생되고, 그렇지 않으면 상태에 들어갈 때 한 번만 재생됩니다.")]
		public bool OnlyPlayWhenEnteringState = true;
        /// 이것이 true인 경우 이 작업을 수행할 때 TargetFeedbacks가 있는 대상 게임 개체가 활성 상태로 설정됩니다.
        [Tooltip("이것이 true인 경우 이 작업을 수행할 때 TargetFeedbacks가 있는 대상 게임 개체가 활성 상태로 설정됩니다.")]
		public bool SetTargetGameObjectActive = false;

		protected bool _played = false;

		/// <summary>
		/// On PerformAction we play our MMFeedbacks
		/// </summary>
		public override void PerformAction()
		{
			PlayFeedbacks();
		}

		/// <summary>
		/// Plays the target MMFeedbacks
		/// </summary>
		protected virtual void PlayFeedbacks()
		{
			if (OnlyPlayWhenEnteringState && _played)
			{
				return;
			}

			if (TargetFeedbacks != null)
			{
				if (SetTargetGameObjectActive)
				{
					TargetFeedbacks.gameObject.SetActive(true);
				}
				TargetFeedbacks.PlayFeedbacks();
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
using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 원하는 수의 대상 MMF 플레이어를 연결하고 전후에 선택적 지연을 적용하여 순서대로 재생할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 원하는 수의 대상 MMF 플레이어를 연결하고 전후에 선택적 지연을 적용하여 순서대로 재생할 수 있습니다.")]
	[FeedbackPath("Feedbacks/MMF Player Chain")]
	public class MMF_PlayerChain : MMF_Feedback
	{
		/// <summary>
		/// A class used to store and define items in a chain of MMF Players
		/// </summary>
		[Serializable]
		public class PlayerChainItem
		{
			/// the target MMF Player 
			[Tooltip("the target MMF Player")]
			public MMF_Player TargetPlayer;
			/// a delay in seconds to wait for before playing this MMF Player (x) and after (y)
			[Tooltip("이 MMF 플레이어를 재생하기 전(x)과 재생 후(y)를 기다리는 초 단위 지연입니다.")]
			[MMVector("Before", "After")]
			public Vector2 Delay;
			/// whether this player is active in the list or not. Inactive players will be skipped when playing the chain of players
			[Tooltip("이 플레이어가 목록에서 활성 상태인지 여부입니다. 플레이어 체인을 플레이할 때 비활성 플레이어는 건너뜁니다.")]
			public bool Inactive = false;
		}
		
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		#endif
		/// the duration of this feedback is the duration of the chain
		public override float FeedbackDuration 
		{
			get
			{
				if ((Players == null) || (Players.Count == 0))
				{
					return 0f;
				}
				
				float totalDuration = 0f;
				foreach (PlayerChainItem item in Players)
				{
					if ((item == null) || (item.TargetPlayer == null) || item.Inactive)
					{
						continue;
					}

					totalDuration += item.Delay.x;
					totalDuration += item.TargetPlayer.TotalDuration;
					totalDuration += item.Delay.y; 
				}
				return totalDuration;
			} 
		}

		[MMFInspectorGroup("Feedbacks", true, 79)]
		/// the list of MMF Player that make up the chain. The chain's items will be played from index 0 to the last in the list
		[Tooltip("체인을 구성하는 MMF 플레이어 목록입니다. 체인의 항목은 인덱스 0부터 목록의 마지막 항목까지 재생됩니다.")]
		public List<PlayerChainItem> Players;

		/// <summary>
		/// On Play we start our chain
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if ((Players == null) || (Players.Count == 0))
			{
				return;
			}
			
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			Owner.StartCoroutine(PlayChain());
		}

		/// <summary>
		/// Plays all players in the chain in sequence
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator PlayChain()
		{
			foreach (PlayerChainItem item in Players)
			{
				if ((item == null) || (item.TargetPlayer == null) || item.Inactive)
				{
					continue;
				}

				if (item.Delay.x > 0) { yield return WaitFor(item.Delay.x); }
				
				item.TargetPlayer.PlayFeedbacks();
				yield return WaitFor(item.TargetPlayer.TotalDuration);
				
				if (item.Delay.y > 0) { yield return WaitFor(item.Delay.y); }
			}
		}

		/// <summary>
		/// On skip to the end, we skip for all players in our chain
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			foreach (PlayerChainItem item in Players)
			{
				if ((item == null) || (item.TargetPlayer == null) || item.Inactive)
				{
					continue;
				}

				item.TargetPlayer.PlayFeedbacks();
				item.TargetPlayer.SkipToTheEnd();
			}
		}
		
		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			foreach (PlayerChainItem item in Players)
			{
				item.TargetPlayer.RestoreInitialValues();
			}
		}
	}
}
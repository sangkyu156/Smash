using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 MMFeedbacks 시퀀스의 현재 "헤드"를 목록 위의 다른 피드백으로 다시 이동합니다.
    /// 헤드가 도달하는 피드백은 설정에 따라 다릅니다. 마지막 일시 중지 또는 목록의 마지막 LoopStart 피드백(또는 둘 다)에서 루프하도록 결정할 수 있습니다.
    /// 또한 여러 번 반복하여 충족되면 일시 중지하도록 결정할 수도 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 MMFeedbacks 시퀀스의 현재 '헤드'를 목록의 위의 다른 피드백으로 다시 이동합니다. " +
"헤드가 어떤 피드백을 받는지는 설정에 따라 다릅니다. 마지막 일시 중지 시 반복되도록 결정할 수 있습니다." +
"또는 목록의 마지막 LoopStart 피드백(또는 둘 다)에서. 또한 여러 번 반복하고 충족되면 일시 중지하도록 결정할 수 있습니다.")]
	[FeedbackPath("Loop/Looper")]
	public class MMF_Looper : MMF_Pause
	{
		[MMFInspectorGroup("Loop", true, 34)]
        
		[Header("Loop conditions")]
		/// if this is true, this feedback, when met, will cause the MMFeedbacks to reposition its 'head' to the first pause found above it (going from this feedback to the top), or to the start if none is found
		[Tooltip("이것이 사실인 경우 이 피드백이 충족되면 MMFeedbacks는 '헤드'를 위에 있는 첫 번째 일시 중지(이 피드백에서 맨 위로 이동)로 위치를 변경하거나, 아무 것도 발견되지 않으면 시작 위치로 이동합니다.")]
		public bool LoopAtLastPause = true;
		/// if this is true, this feedback, when met, will cause the MMFeedbacks to reposition its 'head' to the first LoopStart feedback found above it (going from this feedback to the top), or to the start if none is found
		[Tooltip("이것이 사실인 경우, 이 피드백이 충족되면 MMFeedbacks는 '헤드'를 위에 있는 첫 번째 LoopStart 피드백(이 피드백에서 맨 위로 이동)으로 위치를 변경하거나, 아무것도 발견되지 않으면 시작 위치로 이동하게 됩니다.")]
		public bool LoopAtLastLoopStart = true;

		[Header("Loop")]
		/// if this is true, the looper will loop forever
		[Tooltip("이것이 사실이라면 루퍼는 영원히 반복될 것입니다.")]
		public bool InfiniteLoop = false;
		/// how many times this loop should run
		[Tooltip("이 루프를 몇 번이나 실행해야 하는지")]
		public int NumberOfLoops = 2;
		/// the amount of loops left (updated at runtime)
		[Tooltip("남은 루프의 양(런타임에 업데이트됨)")]
		[MMFReadOnly]
		public int NumberOfLoopsLeft = 1;
		/// whether we are in an infinite loop at this time or not
		[Tooltip("지금 우리가 무한 루프에 빠져 있는지 아닌지")]
		[MMFReadOnly]
		public bool InInfiniteLoop = false;

		/// sets the color of this feedback in the inspector
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LooperColor; } }
		#endif
		public override bool LooperPause { get { return true; } }

		/// the duration of this feedback is the duration of the pause
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

		/// <summary>
		/// On init we initialize our number of loops left
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			InInfiniteLoop = InfiniteLoop;
			NumberOfLoopsLeft = NumberOfLoops;
		}

		/// <summary>
		/// On play we decrease our counter and play our pause
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active)
			{
				InInfiniteLoop = InfiniteLoop;
				NumberOfLoopsLeft--;
				Owner.StartCoroutine(PlayPause());
			}
		}

		/// <summary>
		/// On custom stop, we exit our infinite loop
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			InInfiniteLoop = false;
		}

		/// <summary>
		/// On reset we reset our amount of loops left
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();
			InInfiniteLoop = InfiniteLoop;
			NumberOfLoopsLeft = NumberOfLoops;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백이 충족되면 일시 중지가 발생하여 완료될 때까지 시퀀스의 하위 피드백이 실행되지 않도록 합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백이 충족되면 일시 중지가 발생하여 완료될 때까지 시퀀스의 하위 피드백이 실행되지 않도록 합니다.")]
	[FeedbackPath("Pause/Pause")]
	public class MMF_Pause : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PauseColor; } }
		public override bool DisplayFullHeaderColor => true;
		#endif
		public override IEnumerator Pause { get { return PauseWait(); } }
        
		[MMFInspectorGroup("Pause", true, 32)]
		/// the duration of the pause, in seconds
		[Tooltip("일시 중지 기간(초)")]
		public float PauseDuration = 1f;

		public bool RandomizePauseDuration = false;

		[MMFCondition("RandomizePauseDuration", true)]
		public float MinPauseDuration = 1f;
		[MMFCondition("RandomizePauseDuration", true)]
		public float MaxPauseDuration = 3f;
		[MMFCondition("RandomizePauseDuration", true)]
		public bool RandomizeOnEachPlay = true;
        
		/// if this is true, you'll need to call the Resume() method on the host MMFeedbacks for this pause to stop, and the rest of the sequence to play
		[Tooltip("이것이 사실인 경우 일시 중지를 중지하고 나머지 시퀀스를 재생하려면 호스트 MMFeedbacks에서 Resume() 메서드를 호출해야 합니다.")]
		public bool ScriptDriven = false;
		/// if this is true, a script driven pause will resume after its AutoResumeAfter delay, whether it has been manually resumed or not 
		[Tooltip("이것이 사실이라면 수동 재개 여부에 관계없이 스크립트 기반 일시 중지는 AutoResumeAfter 지연 후에 재개됩니다.")] 
		[MMFCondition("ScriptDriven", true)]
		public bool AutoResume = false;
		/// the duration after which to auto resume, regardless of manual resume calls beforehand
		[Tooltip("사전 수동 재개 호출에 관계없이 자동 재개되기까지의 시간")] 
		[MMFCondition("AutoResume", true)]
		public float AutoResumeAfter = 0.25f;
        
		/// the duration of this feedback is the duration of the pause
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

		/// <summary>
		/// An IEnumerator used to wait for the duration of the pause, on scaled or unscaled time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator PauseWait()
		{
			yield return WaitFor(PauseDuration);
		}

		/// <summary>
		/// On init we cache our wait for seconds
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			ScriptDrivenPause = ScriptDriven;
			ScriptDrivenPauseAutoResume = AutoResume ? AutoResumeAfter : -1f;
			if (RandomizePauseDuration)
			{
				PauseDuration = Random.Range(MinPauseDuration, MaxPauseDuration);
			}
		}

		/// <summary>
		/// On play we trigger our pause
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (RandomizePauseDuration && RandomizeOnEachPlay)
			{
				PauseDuration = Random.Range(MinPauseDuration, MaxPauseDuration);
			}
			Owner.StartCoroutine(PlayPause());
		}

		/// <summary>
		/// Pause coroutine
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator PlayPause()
		{
			yield return Pause;
		}
	}
}
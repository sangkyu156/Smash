using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// MMSoundManager에서 재생되는 모든 사운드를 한 번에 제어하는 ​​데 사용되는 피드백입니다. 사운드를 일시 중지, 재생, 중지하고 해제(중지하고 오디오 소스를 풀로 반환)할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager All Sounds Control")]
	[FeedbackHelp("MMSoundManager에서 재생되는 모든 사운드를 한 번에 제어하는 ​​데 사용되는 피드백입니다. 사운드를 일시 중지, 재생, 중지하고 해제(중지하고 오디오 소스를 풀로 반환)할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.")]
	public class MMF_MMSoundManagerAllSoundsControl : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return ControlMode.ToString();  } }
		#endif
        
		[MMFInspectorGroup("MMSoundManager All Sounds Control", true, 30)]
		/// The selected control mode. 
		[Tooltip("선택한 제어 모드")]
		public MMSoundManagerAllSoundsControlEventTypes ControlMode = MMSoundManagerAllSoundsControlEventTypes.Pause;

		/// <summary>
		/// On Play, we call the specified event, to be caught by the MMSoundManager
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			switch (ControlMode)
			{
				case MMSoundManagerAllSoundsControlEventTypes.Pause:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Pause);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Play:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Play);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Stop:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Stop);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Free:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Free);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.FreeAllButPersistent:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllButPersistent);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping);
					break;
			}
		}
	}
}
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
    /// 이 피드백을 통해 MMSoundManager 설정에 대한 저장, 로드 및 재설정을 실행할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Save and Load")]
	[FeedbackHelp("이 피드백을 통해 MMSoundManager 설정에 대한 저장, 로드 및 재설정을 실행할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.")]
	public class MMFeedbackMMSoundManagerSaveLoad : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif

		/// the possible modes you can use to interact with save settings
		public enum Modes { Save, Load, Reset }

		[Header("MMSoundManager Save and Load")] 
		/// the selected mode to interact with save settings on the MMSoundManager
		[Tooltip("MMSoundManager의 저장 설정과 상호 작용하기 위해 선택한 모드")]
		public Modes Mode = Modes.Save;
        
		/// <summary>
		/// On Play, saves, loads or resets settings
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			switch (Mode)
			{
				case Modes.Save:
					MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.SaveSettings);
					break;
				case Modes.Load:
					MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.LoadSettings);
					break;
				case Modes.Reset:
					MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.ResetSettings);
					break;
			}
		}
	}
}
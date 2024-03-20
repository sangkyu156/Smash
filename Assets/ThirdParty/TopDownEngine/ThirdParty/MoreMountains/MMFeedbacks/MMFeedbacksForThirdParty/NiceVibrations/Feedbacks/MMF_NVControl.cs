using UnityEngine;
using MoreMountains.Feedbacks;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 글로벌 수준에서 햅틱과 상호 작용하고, 모두 중지하고, 활성화 또는 비활성화하고, 글로벌 수준을 조정하거나 햅틱 엔진을 초기화/해제하려면 이 피드백을 추가하세요.
    /// </summary>
    [AddComponentMenu("")]
	#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
	[FeedbackPath("Haptics/Haptic Control")]
	#endif
	[FeedbackHelp("글로벌 수준에서 햅틱과 상호 작용하고, 모두 중지하고, 활성화 또는 비활성화하고, 글로벌 수준을 조정하거나 햅틱 엔진을 초기화/해제하려면 이 피드백을 추가하세요.")]
	public class MMF_NVControl : MMF_Feedback
	{
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HapticsColor; } }
		public override string RequiredTargetText { get { return ControlType.ToString();  } }
		#endif
    
		public enum ControlTypes { Stop, EnableHaptics, DisableHaptics, AdjustHapticsLevel, Initialize, Release }

		[MMFInspectorGroup("Haptic Control", true, 24)]
		/// the type of control order to trigger when playing this feedback - check Nice Vibrations' documentation for the exact behaviour of these 
		[Tooltip("the type of control order to trigger when playing this feedback - check Nice Vibrations' documentation for the exact behaviour of these")]
		public ControlTypes ControlType = ControlTypes.Stop;
		/// the output level when in AdjustHapticsLevel mode
		[Tooltip("the output level when in AdjustHapticsLevel mode")]
		[MMFEnumCondition("ControlType", (int)ControlTypes.AdjustHapticsLevel)]
		public float OutputLevel = 1f;
        
		/// <summary>
		/// On play we apply the specified order
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			switch (ControlType)
			{
				case ControlTypes.Stop:
					HapticController.Stop();
					break;
				case ControlTypes.EnableHaptics:
					HapticController.hapticsEnabled = true;
					break;
				case ControlTypes.DisableHaptics:
					HapticController.hapticsEnabled = false;
					break;
				case ControlTypes.AdjustHapticsLevel:
					HapticController.outputLevel = OutputLevel;
					break;
				case ControlTypes.Initialize:
					LofeltHaptics.Initialize();
					HapticController.Init();
					break;
				case ControlTypes.Release:
					LofeltHaptics.Release();
					break;
			}
		}
		#else
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }
		#endif
	}    
}
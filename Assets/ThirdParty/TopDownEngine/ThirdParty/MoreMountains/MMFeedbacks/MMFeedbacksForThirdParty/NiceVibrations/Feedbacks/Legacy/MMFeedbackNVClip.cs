using UnityEngine;
using MoreMountains.Feedbacks;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// .haptic 클립을 재생하려면 이 피드백을 추가하고 선택적으로 레벨과 빈도를 무작위로 지정합니다.
    /// </summary>
    [AddComponentMenu("")]
	#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
	[FeedbackPath("Haptics/Haptic Clip")]
	#endif
	[FeedbackHelp("이 피드백을 사용하면 햅틱 클립을 재생하고 해당 클립의 레벨과 주파수를 무작위로 지정할 수 있습니다.")]
	public class MMFeedbackNVClip : MMFeedback
	{
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HapticsColor; } }
		#endif
        
		[Header("Haptic Clip")]
		/// the haptic clip to play with this feedback
		[Tooltip("the haptic clip to play with this feedback")]
		public HapticClip Clip;
		/// a preset to play should the device you're running your game on doesn't support playing haptic clips
		[Tooltip("a preset to play should the device you're running your game on doesn't support playing haptic clips")]
		public HapticPatterns.PresetType FallbackPreset = HapticPatterns.PresetType.LightImpact;
		/// whether or not this clip should play on a loop, until stopped (won't work on gamepads)
		[Tooltip("whether or not this clip should play on a loop, until stopped (won't work on gamepads)")]
		public bool Loop = false;
		/// at what timestamp this clip should start playing
		[Tooltip("at what timestamp this clip should start playing")]
		public float SeekTime = 0f;

		[Header("Level")]
		/// the minimum level at which this clip should play (level will be randomized between MinLevel and MaxLevel)
		[Tooltip("the minimum level at which this clip should play (level will be randomized between MinLevel and MaxLevel)")]
		[Range(0f, 5f)]
		public float MinLevel = 1f;
		/// the maximum level at which this clip should play (level will be randomized between MinLevel and MaxLevel)
		[Tooltip("the maximum level at which this clip should play (level will be randomized between MinLevel and MaxLevel)")]
		[Range(0f, 5f)]
		public float MaxLevel = 1f;
        
		[Header("Frequency Shift")]
		/// the minimum frequency shift at which this clip should play (frequency shift will be randomized between MinFrequencyShift and MaxLevel)
		[Tooltip("the minimum frequency shift at which this clip should play (frequency shift will be randomized between MinFrequencyShift and MaxFrequencyShift)")]
		[Range(-1f, 1f)]
		public float MinFrequencyShift = 0f;
		/// the maximum frequency shift at which this clip should play (frequency shift will be randomized between MinFrequencyShift and MaxLevel)
		[Tooltip("the maximum frequency shift at which this clip should play (frequency shift will be randomized between MinFrequencyShift and MaxFrequencyShift)")]
		[Range(-1f, 1f)]
		public float MaxFrequencyShift = 0f;

		[Header("Settings")] 
		/// a set of settings you can tweak to specify how and when exactly this haptic should play
		[Tooltip("a set of settings you can tweak to specify how and when exactly this haptic should play")]
		public MMFeedbackNVSettings HapticSettings;
        
		/// <summary>
		/// On play, we load our clip, set its settings and play it
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !HapticSettings.CanPlay() || (Clip == null))
			{
				return;
			}

			HapticController.Load(Clip);
			HapticSettings.SetGamepad();
			HapticController.fallbackPreset = FallbackPreset;
			HapticController.Loop(Loop);
			HapticController.Seek(SeekTime);
			HapticController.clipLevel = Random.Range(MinLevel, MaxLevel);
			HapticController.clipFrequencyShift = Random.Range(MinFrequencyShift, MaxFrequencyShift);
			HapticController.Play();
		}
        
		/// <summary>
		/// On stop we stop haptics
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!FeedbackTypeAuthorized)
			{
				return;
			}
            
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			HapticController.Stop();
		}
		#else
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }
		#endif
	}    
}
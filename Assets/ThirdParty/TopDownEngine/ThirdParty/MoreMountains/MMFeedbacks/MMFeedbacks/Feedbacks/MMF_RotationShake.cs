using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 RotationShake 이벤트를 내보낼 수 있습니다. 이는 MMRotationShakers(지정된 채널)에 의해 포착됩니다.
    /// 회전 셰이커는 이름에서 알 수 있듯이 선택적 노이즈 및 기타 미세 제어 옵션을 사용하여 방향을 따라 변환의 회전을 셰이크하는 데 사용됩니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Transform/Rotation Shake")]
	[FeedbackHelp("이 피드백을 사용하면 RotationShake 이벤트를 내보낼 수 있습니다. 이는 지정된 채널의 MMRotationShakers에 의해 포착됩니다." +
"이름에서 알 수 있듯이 회전 셰이커는 선택적 노이즈 및 기타 미세 제어 옵션을 사용하여 방향을 따라 변환의 회전을 셰이크하는 데 사용됩니다.")]
	public class MMF_RotationShake : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override string RequiredTargetText { get { return RequiredChannelText; } }
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		public override bool HasRange => true;

		[MMFInspectorGroup("Optional Target", true, 33)]
		/// a specific (and optional) shaker to target, regardless of its channel
		[Tooltip("채널에 관계없이 대상으로 삼을 특정(및 선택 사항) 셰이커")]
		public MMRotationShaker TargetShaker;
		
		[MMFInspectorGroup("Rotation Shake", true, 28)]
		/// the duration of the shake, in seconds
		[Tooltip("흔들림의 지속 시간(초)")]
		public float Duration = 0.5f;
		/// whether or not to reset shaker values after shake
		[Tooltip("흔들기 후 셰이커 값을 재설정할지 여부")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("흔들기 후 대상의 값을 재설정할지 여부")]
		public bool ResetTargetValuesAfterShake = true;
		
		[MMFInspectorGroup("Shake Settings", true, 42)]
		/// the speed at which the transform should shake
		[Tooltip("the speed at which the transform should shake")]
		public float ShakeSpeed = 20f;
		/// the maximum distance from its initial rotation the transform will move to during the shake
		[Tooltip("흔들림 중에 변환이 이동하게 될 초기 회전으로부터의 최대 거리")]
		public float ShakeRange = 50f;
        
		[MMFInspectorGroup("Direction", true, 43)]
		/// the direction along which to shake the transform's rotation
		[Tooltip("변환의 회전을 흔드는 방향")]
		public Vector3 ShakeMainDirection = Vector3.up;
		/// if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection
		[Tooltip("이것이 사실이라면 ShakeMainDirection을 흔들기 방향으로 사용하는 대신 무작위 벡터3가 생성되어 ShakeMainDirection과 ShakeAltDirection 사이에서 무작위로 생성됩니다.")]
		public bool RandomizeDirection = false;
		/// when in RandomizeDirection mode, a vector against which to randomize the main direction
		[Tooltip("RandomizeDirection 모드에 있을 때 주 방향을 무작위화할 벡터")]
		[MMFCondition("RandomizeDirection", true)]
		public Vector3 ShakeAltDirection = Vector3.up;
		/// if this is true, a new direction will be randomized every time a shake happens
		[Tooltip("이것이 사실이라면 흔들릴 때마다 새로운 방향이 무작위로 지정됩니다.")]
		public bool RandomizeDirectionOnPlay = false;

		[MMFInspectorGroup("Directional Noise", true, 47)]
		/// whether or not to add noise to the main direction
		[Tooltip("whether or not to add noise to the main direction")]
		public bool AddDirectionalNoise = true;
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax
		[Tooltip("방향성 노이즈를 추가할 때 노이즈 강도는 이 값과 DirectionalNoiseStrengthMax 사이에서 무작위로 지정됩니다.")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMin = new Vector3(0.25f, 0.25f, 0.25f);
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin
		[Tooltip("방향성 노이즈를 추가할 때 노이즈 강도는 이 값과 DirectionalNoiseStrengthMin 사이에서 무작위로 지정됩니다.")]
		[MMFCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMax = new Vector3(0.25f, 0.25f, 0.25f);
        
		[MMFInspectorGroup("Randomness", true, 44)]
		/// a unique seed you can use to get different outcomes when shaking more than one transform at once
		[Tooltip("한 번에 둘 이상의 변환을 흔들 때 다른 결과를 얻는 데 사용할 수 있는 고유한 시드")]
		public Vector3 RandomnessSeed;
		/// whether or not to generate a unique seed automatically on every shake
		[Tooltip("흔들릴 때마다 고유한 시드를 자동으로 생성할지 여부")]
		public bool RandomizeSeedOnShake = true;

		[MMFInspectorGroup("One Time", true, 45)]
		/// whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve
		[Tooltip("정의된 곡선을 따라 흔들림의 진폭에 영향을 미치는 감쇠를 사용할지 여부")]
		public bool UseAttenuation = true;
		/// the animation curve used to define attenuation, impacting the amplitude of the shake
		[Tooltip("감쇠를 정의하는 데 사용되는 애니메이션 곡선으로 흔들림의 진폭에 영향을 미칩니다.")]
		[MMFCondition("UseAttenuation", true)]
		public AnimationCurve AttenuationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// <summary>
		/// Triggers the corresponding coroutine
		/// </summary>
		/// <param name="rotation"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);

			if (TargetShaker == null)
			{
				MMRotationShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
			}
			else
			{
				TargetShaker?.OnMMRotationShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve,
					UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
					intensityMultiplier, TargetShaker.ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
			}
		}
        
		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="rotation"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			
			if (TargetShaker == null)
			{
				MMRotationShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, stop:true);
			}
			else
			{
				TargetShaker?.OnMMRotationShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData: TargetShaker.ChannelData, stop:true);
			}
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if (TargetShaker == null)
			{
				MMRotationShakeEvent.Trigger(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, restore:true);
			}
			else
			{
				TargetShaker?.OnMMRotationShakeEvent(Duration, ShakeSpeed,  ShakeRange,  ShakeMainDirection,  RandomizeDirection,  ShakeAltDirection,  RandomizeDirectionOnPlay,  AddDirectionalNoise, 
					DirectionalNoiseStrengthMin,  DirectionalNoiseStrengthMax,  RandomnessSeed,  RandomizeSeedOnShake,  UseAttenuation,  AttenuationCurve, channelData: TargetShaker.ChannelData, restore:true);
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 재생 시 조명의 색상과 강도를 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 통해 특정 기간 동안(또는 즉시) 장면의 조명 색상과 강도를 제어할 수 있습니다.")]
	[FeedbackPath("Light")]
	public class MMFeedbackLight : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LightColor; } }
		#endif

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant, ShakerEvent }

		[Header("Light")]
		/// the light to affect when playing the feedback
		[Tooltip("피드백을 재생할 때 영향을 미치는 조명")]
		public Light BoundLight;
		/// whether the feedback should affect the light instantly or over a period of time
		[Tooltip("피드백이 조명에 즉시 영향을 미치는지 또는 일정 기간 동안 영향을 미치는지 여부")]
		public Modes Mode = Modes.OverTime;
		/// how long the light should change over time
		[Tooltip("시간이 지남에 따라 빛이 얼마나 오랫동안 변해야 하는지")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float Duration = 0.2f;
		/// whether or not that light should be turned off on start
		[Tooltip("시작 시 조명을 꺼야 하는지 여부")]
		public bool StartsOff = true;
		/// whether or not the values should be relative or not
		[Tooltip("값이 상대적이어야 하는지 여부")]
		public bool RelativeValues = true;
		/// the channel to broadcast on
		[Tooltip("the channel to broadcast on")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public int Channel = 0;
		/// whether or not to reset shaker values after shake
		[Tooltip("흔들기 후 셰이커 값을 재설정할지 여부")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("흔들기 후 대상의 값을 재설정할지 여부")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to broadcast a range to only affect certain shakers
		[Tooltip("특정 셰이커에만 영향을 미치도록 범위를 방송할지 여부")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public bool UseRange = false;
		/// the range of the event, in units
		[Tooltip("이벤트 범위(단위)")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public float EventRange = 100f;
		/// the transform to use to broadcast the event as origin point
		[Tooltip("이벤트를 원점으로 방송하는 데 사용할 변환")]
		[MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
		public Transform EventOriginTransform;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, the light will be disabled when this feedbacks is stopped
		[Tooltip("이것이 사실이라면 이 피드백이 중지되면 조명이 비활성화됩니다.")] 
		public bool DisableOnStop = true;

		[Header("Color")]
		/// whether or not to modify the color of the light
		[Tooltip("빛의 색상을 수정할지 여부")]
		public bool ModifyColor = true;
		/// the colors to apply to the light over time
		[Tooltip("시간이 지남에 따라 빛에 적용할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("인스턴트 모드에서 이동할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ShakerEvent)]
		public Color InstantColor;

		[Header("Intensity")]
		/// the curve to tween the intensity on
		[Tooltip("강도를 트위닝하는 곡선")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public AnimationCurve IntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the intensity curve's 0 to
		[Tooltip("the value to remap the intensity curve's 0 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapIntensityZero = 0f;
		/// the value to remap the intensity curve's 1 to
		[Tooltip("the value to remap the intensity curve's 1 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapIntensityOne = 1f;
		/// the value to move the intensity to in instant mode
		[Tooltip("인스턴트 모드에서 강도를 이동할 값")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantIntensity;

		[Header("Range")]
		/// the range to apply to the light over time
		[Tooltip("인스턴트 모드에서 강도를 이동하기 위해 시간 경과에 따라 빛에 적용할 범위 값")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public AnimationCurve RangeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the range curve's 0 to
		[Tooltip("the value to remap the range curve's 0 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapRangeZero = 0f;
		/// the value to remap the range curve's 0 to
		[Tooltip("the value to remap the range curve's 0 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapRangeOne = 10f;
		/// the value to move the intensity to in instant mode
		[Tooltip("인스턴트 모드에서 강도를 이동할 값")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantRange;

		[Header("Shadow Strength")]
		/// the range to apply to the light over time
		[Tooltip("인스턴트 모드에서 강도를 이동하기 위해 시간 경과에 따라 빛에 적용할 범위 값")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public AnimationCurve ShadowStrengthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the shadow strength's curve's 0 to
		[Tooltip("the value to remap the shadow strength's curve's 0 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapShadowStrengthZero = 0f;
		/// the value to remap the shadow strength's curve's 1 to
		[Tooltip("the value to remap the shadow strength's curve's 1 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public float RemapShadowStrengthOne = 1f;
		/// the value to move the shadow strength to in instant mode
		[Tooltip("인스턴트 모드에서 그림자 강도를 이동할 값입니다.")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantShadowStrength;

		protected float _initialRange;
		protected float _initialShadowStrength;
		protected float _initialIntensity;
		protected Coroutine _coroutine;

		/// the duration of this feedback is the duration of the light, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// <summary>
		/// On init we turn the light off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

			_initialRange = BoundLight.range;
			_initialShadowStrength = BoundLight.shadowStrength;
			_initialIntensity = BoundLight.intensity;

			if (EventOriginTransform == null)
			{
				EventOriginTransform = this.transform;
			}

			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}
		}

		/// <summary>
		/// On Play we turn our light on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					BoundLight.intensity = InstantIntensity * intensityMultiplier;
					BoundLight.shadowStrength = InstantShadowStrength;
					BoundLight.range = InstantRange;
					if (ModifyColor)
					{
						BoundLight.color = InstantColor;
					}                        
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(LightSequence(intensityMultiplier));

					break;
				case Modes.ShakerEvent:
					MMLightShakeEvent.Trigger(FeedbackDuration, RelativeValues, ModifyColor, ColorOverTime, IntensityCurve,
						RemapIntensityZero, RemapIntensityOne, RangeCurve, RemapRangeZero * intensityMultiplier, RemapRangeOne * intensityMultiplier,
						ShadowStrengthCurve, RemapShadowStrengthZero, RemapShadowStrengthOne, feedbacksIntensity,
						ChannelData(Channel), ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
						UseRange, EventRange, EventOriginTransform.position);
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the intensity and color of the light over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator LightSequence(float intensityMultiplier)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetLightValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetLightValues(FinalNormalizedTime, intensityMultiplier);
			if (StartsOff)
			{
				Turn(false);
			}            
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the light on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetLightValues(float time, float intensityMultiplier)
		{
			float intensity = MMFeedbacksHelpers.Remap(IntensityCurve.Evaluate(time), 0f, 1f, RemapIntensityZero, RemapIntensityOne);
			float range = MMFeedbacksHelpers.Remap(RangeCurve.Evaluate(time), 0f, 1f, RemapRangeZero, RemapRangeOne);
			float shadowStrength = MMFeedbacksHelpers.Remap(ShadowStrengthCurve.Evaluate(time), 0f, 1f, RemapShadowStrengthZero, RemapShadowStrengthOne);        

			if (RelativeValues)
			{
				intensity += _initialIntensity;
				shadowStrength += _initialShadowStrength;
				range += _initialRange;
			}

			BoundLight.intensity = intensity * intensityMultiplier;
			BoundLight.range = range;
			BoundLight.shadowStrength = Mathf.Clamp01(shadowStrength);
			if (ModifyColor)
			{
				BoundLight.color = ColorOverTime.Evaluate(time);
			}
		}

		/// <summary>
		/// Turns the light off on stop
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
			if (Active && (_coroutine != null))
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
			if (Active && DisableOnStop)
			{
				Turn(false);
			}
		}

		/// <summary>
		/// Turns the light on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			BoundLight.gameObject.SetActive(status);
			BoundLight.enabled = status;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 장면 안개의 밀도, 색상, 끝 및 시작 거리를 애니메이션화할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 장면 안개의 밀도, 색상, 끝 및 시작 거리를 애니메이션화할 수 있습니다.")]
	[FeedbackPath("Renderer/Fog")]
	public class MMF_Fog : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override string RequiredTargetText { get { return Mode.ToString();  } }
		#endif
		public override bool HasRandomness => true;

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Fog", true, 24)]
        /// 피드백이 스프라이트 렌더러에 즉시 영향을 미칠지, 아니면 일정 기간 동안 영향을 미칠지 여부
        [Tooltip("피드백이 스프라이트 렌더러에 즉시 영향을 미칠지, 아니면 일정 기간 동안 영향을 미칠지 여부")]
		public Modes Mode = Modes.OverTime;
		/// how long the sprite renderer should change over time
		[Tooltip("스프라이트 렌더러가 시간에 따라 변경되어야 하는 시간")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 2f;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;

		[MMFInspectorGroup("Fog Density", true, 25)]
		/// whether or not to modify the fog's density
		[Tooltip("안개의 밀도를 수정할지 여부")]
		public bool ModifyFogDensity = true;
		/// a curve to use to animate the fog's density over time
		[Tooltip("시간이 지남에 따라 안개의 밀도를 애니메이션화하는 데 사용하는 곡선")]
		public MMTweenType DensityCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the fog's density curve zero value to
		[Tooltip("안개의 밀도 곡선 0 값을 다시 매핑할 값")]
		public float DensityRemapZero = 0.01f;
		/// the value to remap the fog's density curve one value to
		[Tooltip("안개의 밀도 곡선을 한 값으로 다시 매핑할 값")]
		public float DensityRemapOne = 0.05f;
		/// the value to change the fog's density to when in instant mode
		[Tooltip("순간 모드일 때 안개의 밀도를 변경하는 값")]
		public float DensityInstantChange;
        
		[MMFInspectorGroup("Fog Start Distance", true, 26)]
		/// whether or not to modify the fog's start distance
		[Tooltip("안개의 시작 거리를 수정할지 여부")]
		public bool ModifyStartDistance = true;
		/// a curve to use to animate the fog's start distance over time
		[Tooltip("시간에 따른 안개의 시작 거리를 애니메이션화하는 데 사용하는 곡선")]
		public MMTweenType StartDistanceCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the fog's start distance curve zero value to
		[Tooltip("안개의 시작 거리 곡선 0 값을 다시 매핑할 값입니다.")]
		public float StartDistanceRemapZero = 0f;
		/// the value to remap the fog's start distance curve one value to
		[Tooltip("안개의 시작 거리 곡선을 한 값으로 다시 매핑할 값")]
		public float StartDistanceRemapOne = 0f;
		/// the value to change the fog's start distance to when in instant mode
		[Tooltip("인스턴트 모드일 때 안개의 시작 거리를 변경하는 값")]
		public float StartDistanceInstantChange;
        
		[MMFInspectorGroup("Fog End Distance", true, 27)]
		/// whether or not to modify the fog's end distance
		[Tooltip("안개의 끝 거리를 수정할지 여부")]
		public bool ModifyEndDistance = true;
		/// a curve to use to animate the fog's end distance over time
		[Tooltip("시간에 따른 안개의 끝 거리를 애니메이션화하는 데 사용하는 곡선")]
		public MMTweenType EndDistanceCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the fog's end distance curve zero value to
		[Tooltip("안개의 끝 거리 곡선 0 값을 다시 매핑할 값입니다.")]
		public float EndDistanceRemapZero = 0f;
		/// the value to remap the fog's end distance curve one value to
		[Tooltip("안개의 끝 거리 곡선을 한 값으로 다시 매핑할 값")]
		public float EndDistanceRemapOne = 300f;
		/// the value to change the fog's end distance to when in instant mode
		[Tooltip("순간 모드일 때 안개의 끝 거리를 변경하는 값")]
		public float EndDistanceInstantChange;
        
		[MMFInspectorGroup("Fog Color", true, 28)]
		/// whether or not to modify the fog's color
		[Tooltip("안개의 색상을 수정할지 여부")]
		public bool ModifyColor = true;
		/// the colors to apply to the sprite renderer over time
		[Tooltip("시간이 지남에 따라 스프라이트 렌더러에 적용할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("인스턴트 모드에서 이동할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Color InstantColor;

		/// the duration of this feedback is the duration of the sprite renderer, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { if (Mode != Modes.Instant) { Duration = value; } } }
        
		protected Coroutine _coroutine;
		protected Color _initialColor;
		protected float _initialStartDistance;
		protected float _initialEndDistance;
		protected float _initialDensity;

		/// <summary>
		/// On Play we change the values of our fog
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			_initialColor = RenderSettings.fogColor;
			_initialStartDistance = RenderSettings.fogStartDistance;
			_initialEndDistance = RenderSettings.fogEndDistance;
			_initialDensity = RenderSettings.fogDensity;
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						RenderSettings.fogColor = InstantColor;
					}

					if (ModifyStartDistance)
					{
						RenderSettings.fogStartDistance = StartDistanceInstantChange;
					}

					if (ModifyEndDistance)
					{
						RenderSettings.fogEndDistance = EndDistanceInstantChange;
					}

					if (ModifyFogDensity)
					{
						RenderSettings.fogDensity = DensityInstantChange * intensityMultiplier;
					}
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(FogSequence(intensityMultiplier));
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the fog settings
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator FogSequence(float intensityMultiplier)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetFogValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetFogValues(FinalNormalizedTime, intensityMultiplier);    
			_coroutine = null;      
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the fog on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetFogValues(float time, float intensityMultiplier)
		{
			if (ModifyColor)
			{
				RenderSettings.fogColor = ColorOverTime.Evaluate(time); 
			}

			if (ModifyFogDensity)
			{
				RenderSettings.fogDensity = MMTween.Tween(time, 0f, 1f, DensityRemapZero, DensityRemapOne, DensityCurve) * intensityMultiplier;
			}

			if (ModifyStartDistance)
			{
				RenderSettings.fogStartDistance = MMTween.Tween(time, 0f, 1f, StartDistanceRemapZero, StartDistanceRemapOne, StartDistanceCurve);
			}

			if (ModifyEndDistance)
			{
				RenderSettings.fogEndDistance = MMTween.Tween(time, 0f, 1f, EndDistanceRemapZero, EndDistanceRemapOne, EndDistanceCurve);
			}
		}
        
		/// <summary>
		/// Stops this feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			Owner.StopCoroutine(_coroutine);
			_coroutine = null;
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

			RenderSettings.fogColor = _initialColor;
			RenderSettings.fogStartDistance = _initialStartDistance;
			RenderSettings.fogEndDistance = _initialEndDistance;
			RenderSettings.fogDensity = _initialDensity;
		}
	}
}
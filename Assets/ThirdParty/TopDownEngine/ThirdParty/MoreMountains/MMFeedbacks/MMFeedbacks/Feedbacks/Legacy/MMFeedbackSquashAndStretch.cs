using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 한 축의 개체 크기를 수정하는 동시에 다른 두 축(또는 하나만)은 자동으로 수정되어 질량을 보존할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Transform/Squash and Stretch")]
	[FeedbackHelp("이 피드백을 사용하면 한 축의 개체 크기를 수정하는 동시에 다른 두 축(또는 하나만)은 자동으로 수정되어 질량을 보존할 수 있습니다.")]
	public class MMFeedbackSquashAndStretch : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible modes this feedback can operate on
		public enum Modes { Absolute, Additive, ToDestination }
		/// the various axis on which to apply the squash and stretch
		public enum PossibleAxis { XtoYZ, XtoY, XtoZ, YtoXZ, YtoX, YtoZ, ZtoXZ, ZtoX, ZtoY }
		/// the possible timescales for the animation of the scale
		public enum TimeScales { Scaled, Unscaled }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		#endif

		[Header("Squash & Stretch")]
        
		/// the object to animate
		[Tooltip("the object to animate")]
		public Transform SquashAndStretchTarget;
        /// 이 피드백이 작동해야 하는 모드
        /// Absolute : 곡선을 따른다
        /// Additive : 대상의 현재 규모에 추가
        /// ToDestination : 현재 스케일이 무엇이든 대상 대상으로 스케일을 설정합니다.
        [Tooltip("이 피드백이 작동해야 하는 모드" +
                 "Absolute : 곡선을 따른다" +
                 "Additive : 대상의 현재 규모에 추가" +
                 "ToDestination : 현재 스케일이 무엇이든 대상 대상으로 스케일을 설정합니다.")]
		public Modes Mode = Modes.Absolute;

		public PossibleAxis Axis = PossibleAxis.YtoXZ;
		/// whether this feedback should play in scaled or unscaled time
		[Tooltip("이 피드백이 확장된 시간에 재생되어야 하는지 또는 확장되지 않은 시간에 재생되어야 하는지 여부")]
		public TimeScales TimeScale = TimeScales.Scaled;
		/// the duration of the animation
		[Tooltip("애니메이션 지속 시간")]
		public float AnimateScaleDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("the value to remap the curve's 0 value to")]
		public float RemapCurveZero = 1f;
		/// the value to remap the curve's 1 value to
		[Tooltip("the value to remap the curve's 1 value to")]
		[FormerlySerializedAs("Multiplier")]
		public float RemapCurveOne = 2f;
		/// how much should be added to the curve
		[Tooltip("곡선에 얼마나 추가해야 할까요?")]
		public float Offset = 0f;
        
		/// the curve along which to animate the scale
		[Tooltip("스케일에 애니메이션을 적용할 곡선")]
		public AnimationCurve AnimateCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0));
       
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination scales will be recomputed on every play
		[Tooltip("이것이 사실이라면 매 플레이마다 초기 및 대상 스케일이 다시 계산됩니다.")]
		public bool DetermineScaleOnPlay = false;

		[Header("To Destination")]
		/// the scale to reach when in ToDestination mode
		[Tooltip("ToDestination 모드에 있을 때 도달할 스케일")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationScale = 2f;

		/// the duration of this feedback is the duration of the scale animation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateScaleDuration); } set { AnimateScaleDuration = value; } }

		protected Vector3 _initialScale;
		protected float _initialAxisScale;
		protected Vector3 _newScale;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial scale
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			if (Active && (SquashAndStretchTarget != null))
			{
				GetInitialScale();
			}
		}

		/// <summary>
		/// Stores initial scale for future use
		/// </summary>
		protected virtual void GetInitialScale()
		{
			_initialScale = SquashAndStretchTarget.localScale;
		}

		protected virtual void GetAxisScale()
		{
			switch (Axis)
			{
				case PossibleAxis.XtoYZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.x;
					break;
				case PossibleAxis.XtoY:
					_initialAxisScale = SquashAndStretchTarget.localScale.x;
					break;
				case PossibleAxis.XtoZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.x;
					break;
				case PossibleAxis.YtoXZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.y;
					break;
				case PossibleAxis.YtoX:
					_initialAxisScale = SquashAndStretchTarget.localScale.y;
					break;
				case PossibleAxis.YtoZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.y;
					break;
				case PossibleAxis.ZtoXZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.z;
					break;
				case PossibleAxis.ZtoX:
					_initialAxisScale = SquashAndStretchTarget.localScale.z;
					break;
				case PossibleAxis.ZtoY:
					_initialAxisScale = SquashAndStretchTarget.localScale.z;
					break;
			}
		}

		/// <summary>
		/// On Play, triggers the scale animation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (SquashAndStretchTarget == null)) 
			{
				return;
			}
            
			if (DetermineScaleOnPlay && NormalPlayDirection)
			{
				GetInitialScale();
			}
            
			GetAxisScale();
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			if (isActiveAndEnabled || _hostMMFeedbacks.AutoPlayOnEnable)
			{
				if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(AnimateScale(SquashAndStretchTarget, FeedbackDuration, AnimateCurve, Axis, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
				}
				if (Mode == Modes.ToDestination)
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(ScaleToDestination());
				}                   
			}
		}

		/// <summary>
		/// An internal coroutine used to scale the target to its destination scale
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ScaleToDestination()
		{
			if (SquashAndStretchTarget == null)
			{
				yield break;
			}

			if (FeedbackDuration == 0f)
			{
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			_initialScale = SquashAndStretchTarget.localScale;
			_newScale = _initialScale;
			GetAxisScale();
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);

				float newScale = Mathf.LerpUnclamped(_initialAxisScale, DestinationScale, AnimateCurve.Evaluate(percent) + Offset);
				newScale = MMFeedbacksHelpers.Remap(newScale, 0f, 1f, RemapCurveZero, RemapCurveOne);

				ApplyScale(newScale);
                
				SquashAndStretchTarget.localScale = _newScale;

				if (TimeScale == TimeScales.Scaled)
				{
					journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				}
				else
				{
					journey += NormalPlayDirection ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
				}
				yield return null;
			}

			ApplyScale(DestinationScale);
			SquashAndStretchTarget.localScale = NormalPlayDirection ? _newScale : _initialScale;
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// An internal coroutine used to animate the scale over time
		/// </summary>
		/// <param name="targetTransform"></param>
		/// <param name="duration"></param>
		/// <param name="curveX"></param>
		/// <param name="curveY"></param>
		/// <param name="curveZ"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		protected virtual IEnumerator AnimateScale(Transform targetTransform, float duration, AnimationCurve curve, PossibleAxis axis, float remapCurveZero = 0f, float remapCurveOne = 1f)
		{
			if (targetTransform == null)
			{
				yield break;
			}

			if (duration == 0f)
			{
				yield break;
			}
            
			float journey = NormalPlayDirection ? 0f : duration;
            
			_initialScale = targetTransform.localScale;
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);

				float newScale = curve.Evaluate(percent) + Offset;
				newScale = MMFeedbacksHelpers.Remap(newScale, 0f, 1f, remapCurveZero, remapCurveOne);
				if (Mode == Modes.Additive)
				{
					newScale += _initialAxisScale;
				}

				ApplyScale(newScale);
				targetTransform.localScale = _newScale;
                
				if (TimeScale == TimeScales.Scaled)
				{
					journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				}
				else
				{
					journey += NormalPlayDirection ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
				}
				yield return null;
			}

			float finalScale;
            
			finalScale = curve.Evaluate(FinalNormalizedTime) + Offset;
			finalScale = MMFeedbacksHelpers.Remap(finalScale, 0f, 1f, remapCurveZero, remapCurveOne);
			if (Mode == Modes.Additive)
			{
				finalScale += _initialAxisScale;
			}
            
			ApplyScale(finalScale);
            
			targetTransform.localScale = _newScale;
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Applies the new scale on the selected axis
		/// </summary>
		/// <param name="newScale"></param>
		protected virtual void ApplyScale(float newScale)
		{
			float invertScale = 1 / Mathf.Sqrt(newScale);
			switch (Axis)
			{
				case PossibleAxis.XtoYZ:
					_newScale.x = newScale;
					_newScale.y = invertScale;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.XtoY:
					_newScale.x = newScale;
					_newScale.y = invertScale;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.XtoZ:
					_newScale.x = newScale;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoXZ:
					_newScale.x = invertScale;
					_newScale.y = newScale;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoX:
					_newScale.x = invertScale;
					_newScale.y = newScale;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.YtoZ:
					_newScale.x = newScale;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.ZtoXZ:
					_newScale.x = invertScale;
					_newScale.y = invertScale;
					_newScale.z = newScale;
					break;
				case PossibleAxis.ZtoX:
					_newScale.x = invertScale;
					_newScale.y = _initialScale.y;
					_newScale.z = newScale;
					break;
				case PossibleAxis.ZtoY:
					_newScale.x = _initialScale.x;
					_newScale.y = invertScale;
					_newScale.z = newScale;
					break;
			}
		}

		/// <summary>
		/// On stop, we interrupt movement if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && (_coroutine != null))
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
				IsPlaying = false;
			}
		}

		/// <summary>
		/// On disable we reset our coroutine
		/// </summary>
		protected virtual void OnDisable()
		{
			_coroutine = null;
		}
	}
}
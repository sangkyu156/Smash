﻿using MoreMountains.Tools;
using System.Collections;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 장면의 모든 개체에 대한 (거의) 모든 속성을 대상으로 지정할 수 있습니다.
    /// 스크립트 가능한 객체에서도 작동합니다. 개체를 드래그하고 속성을 선택하고 피드백을 설정하세요. " +
    /// 시간이 지남에 따라 해당 속성을 업데이트합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 장면의 모든 개체에 대한 (거의) 모든 속성을 대상으로 지정할 수 있습니다. " +
"스크립트 가능한 개체에서도 작동합니다. 개체를 드래그하고 속성을 선택하고 피드백을 설정합니다. " +
"시간이 지남에 따라 해당 속성을 업데이트합니다.")]
	[FeedbackPath("GameObject/Property")]
	public class MMFeedbackProperty : MMFeedback
	{
		/// the duration of this feedback is the duration of the target property, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { if (Mode != Modes.Instant) { Duration = value; } } }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif
        
		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant } 
        
		[Header("Target Property")]
		/// the receiver to write the level to
		[Tooltip("레벨을 쓸 수신기")]
		public MMPropertyReceiver Target;

		[Header("Mode")]
		/// whether the feedback should affect the target property instantly or over a period of time
		[Tooltip("피드백이 대상 속성에 즉시 영향을 미치는지 또는 일정 기간에 걸쳐 영향을 미치는지 여부")]
		public Modes Mode = Modes.OverTime;
		/// how long the target property should change over time
		[Tooltip("시간이 지남에 따라 대상 속성이 변경되어야 하는 기간")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not that target property should be turned off on start
		[Tooltip("시작 시 해당 대상 속성을 꺼야 하는지 여부")]
		public bool StartsOff = false;
		/// whether or not the values should be relative or not
		[Tooltip("값이 상대적이어야 하는지 여부")]
		public bool RelativeValues = true;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;

		[Header("Level")]
		/// the curve to tween the intensity on
		[Tooltip("강도를 트위닝하는 곡선")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public MMTweenType LevelCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the intensity curve's 0 to
		[Tooltip("the value to remap the intensity curve's 0 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float RemapLevelZero = 0f;
		/// the value to remap the intensity curve's 1 to
		[Tooltip("the value to remap the intensity curve's 1 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float RemapLevelOne = 1f;
		/// the value to move the intensity to in instant mode
		[Tooltip("인스턴트 모드에서 강도를 이동할 값")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantLevel;

		protected float _initialIntensity;
		protected Coroutine _coroutine; 

		/// <summary>
		/// On init we turn the target property off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

			Target.Initialization(this.gameObject);
			_initialIntensity = Target.Level; 
            
			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}

		}

		/// <summary>
		/// On Play we turn our target property on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active)
			{
				Turn(true);
                
				float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                
				switch (Mode)
				{
					case Modes.Instant:
						Target.SetLevel(InstantLevel);
						break;
					case Modes.OverTime:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						_coroutine = StartCoroutine(UpdateValueSequence(intensityMultiplier));
						break;
				}
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the target property
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator UpdateValueSequence(float intensityMultiplier)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetValues(FinalNormalizedTime, intensityMultiplier);
			if (StartsOff)
			{
				Turn(false);
			}

			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the target property on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetValues(float time, float intensityMultiplier)
		{
			float intensity = MMTween.Tween(time, 0f, 1f, RemapLevelZero, RemapLevelOne, LevelCurve);

			intensity *= intensityMultiplier;
            
			if (RelativeValues)
			{
				intensity += _initialIntensity;
			}

			Target.SetLevel(intensity);
		}

		/// <summary>
		/// Turns the target property object off on stop if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active)
			{
				if (_coroutine != null)
				{
					StopCoroutine(_coroutine);
					_coroutine = null;
					SetValues(_initialIntensity, 1f);
				}

				if (StartsOff)
				{
					Turn(false);    
				}
			}
		}

		/// <summary>
		/// Turns the target object on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			if (Target.TargetComponent.gameObject != null)
			{
				Target.TargetComponent.gameObject.SetActive(status);
			}
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간이 지남에 따라 대상 스프라이트 렌더러의 색상을 변경하고 X 또는 Y로 뒤집을 수 있습니다. 또한 이를 사용하여 하나 이상의 MMSpriteRendererShaker를 명령할 수도 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 대상 스프라이트 렌더러의 색상을 변경하고 X 또는 Y로 뒤집을 수 있습니다. 또한 이를 사용하여 하나 이상의 MMSpriteRendererShaker를 명령할 수도 있습니다.")]
	[FeedbackPath("Renderer/SpriteRenderer")]
	public class MMFeedbackSpriteRenderer : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		#endif

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant, ShakerEvent, ToDestinationColor, ToDestinationColorAndBack }
		/// the different ways to grab initial color
		public enum InitialColorModes { InitialColorOnInit, InitialColorOnPlay }

		[Header("Sprite Renderer")]
		/// the SpriteRenderer to affect when playing the feedback
		[Tooltip("피드백을 재생할 때 영향을 미치는 SpriteRenderer")]
		public SpriteRenderer BoundSpriteRenderer;
		/// whether the feedback should affect the sprite renderer instantly or over a period of time
		[Tooltip("피드백이 스프라이트 렌더러에 즉시 영향을 미칠지, 아니면 일정 기간 동안 영향을 미칠지 여부")]
		public Modes Mode = Modes.OverTime;
		/// how long the sprite renderer should change over time
		[Tooltip("스프라이트 렌더러가 시간에 따라 변경되어야 하는 시간")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
		public float Duration = 0.2f;
		/// whether or not that sprite renderer should be turned off on start
		[Tooltip("시작 시 스프라이트 렌더러를 꺼야 하는지 여부")]
		public bool StartsOff = false;
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
		/// whether to grab the initial color to (potentially) go back to at init or when the feedback plays
		[Tooltip("(잠재적으로) 초기화 시로 돌아가기 위해 초기 색상을 가져올지 아니면 피드백이 재생될 때할지 여부")] 
		public InitialColorModes InitialColorMode = InitialColorModes.InitialColorOnPlay;
        
		[Header("Color")]
		/// whether or not to modify the color of the sprite renderer
		[Tooltip("스프라이트 렌더러의 색상을 수정할지 여부")]
		public bool ModifyColor = true;
		/// the colors to apply to the sprite renderer over time
		[Tooltip("시간이 지남에 따라 스프라이트 렌더러에 적용할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("인스턴트 모드에서 이동할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ShakerEvent)]
		public Color InstantColor;
        
		/// the color to move to in ToDestination mode
		[Tooltip("인스턴트 모드에서 이동할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
		public Color ToDestinationColor = Color.red;
		/// the color to move to in ToDestination mode
		[Tooltip("인스턴트 모드에서 이동할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
		public AnimationCurve ToDestinationColorCurve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 1f));
        
		[Header("Flip")]
		/// whether or not to flip the sprite on X
		[Tooltip("X에서 스프라이트를 뒤집을지 여부")]
		public bool FlipX = false;
		/// whether or not to flip the sprite on Y
		[Tooltip("Y에서 스프라이트를 뒤집을지 여부")]
		public bool FlipY = false;

		/// the duration of this feedback is the duration of the sprite renderer, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		protected Coroutine _coroutine;
		protected Color _initialColor;
        
		/// <summary>
		/// On init we turn the sprite renderer off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

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

			if ((BoundSpriteRenderer != null) && (InitialColorMode == InitialColorModes.InitialColorOnInit))
			{
				_initialColor = BoundSpriteRenderer.color;
			}
		}

		/// <summary>
		/// On Play we turn our sprite renderer on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if ((BoundSpriteRenderer != null) && (InitialColorMode == InitialColorModes.InitialColorOnPlay))
			{
				_initialColor = BoundSpriteRenderer.color;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						BoundSpriteRenderer.color = InstantColor;
					}
					Flip();
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(SpriteRendererSequence());
					break;
				case Modes.ShakerEvent:
					MMSpriteRendererShakeEvent.Trigger(FeedbackDuration, ModifyColor, ColorOverTime, 
						FlipX, FlipY,   
						intensityMultiplier,
						ChannelData(Channel), ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
						UseRange, EventRange, EventOriginTransform.position);
					break;
				case Modes.ToDestinationColor:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(SpriteRendererToDestinationSequence(false));
					break;
				case Modes.ToDestinationColorAndBack:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(SpriteRendererToDestinationSequence(true));
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the SpriteRenderer
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SpriteRendererSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			Flip();
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetSpriteRendererValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetSpriteRendererValues(FinalNormalizedTime);
			if (StartsOff)
			{
				Turn(false);
			}            
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// This coroutine will modify the values on the SpriteRenderer
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SpriteRendererToDestinationSequence(bool andBack)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			Flip();
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				if (andBack)
				{
					remappedTime = (remappedTime < 0.5f)
						? MMFeedbacksHelpers.Remap(remappedTime, 0f, 0.5f, 0f, 1f)
						: MMFeedbacksHelpers.Remap(remappedTime, 0.5f, 1f, 1f, 0f);
				}
                
				float evalTime = ToDestinationColorCurve.Evaluate(remappedTime);
                
				if (ModifyColor)
				{
					BoundSpriteRenderer.color = Color.LerpUnclamped(_initialColor, ToDestinationColor, evalTime);
				}

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			if (ModifyColor)
			{
				BoundSpriteRenderer.color = andBack ? _initialColor : ToDestinationColor;
			}
			if (StartsOff)
			{
				Turn(false);
			}            
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Flips the sprite on X or Y based on the FlipX/FlipY settings
		/// </summary>
		protected virtual void Flip()
		{
			if (FlipX)
			{
				BoundSpriteRenderer.flipX = !BoundSpriteRenderer.flipX;
			}
			if (FlipY)
			{
				BoundSpriteRenderer.flipY = !BoundSpriteRenderer.flipY;
			}
		}

		/// <summary>
		/// Sets the various values on the sprite renderer on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetSpriteRendererValues(float time)
		{
			if (ModifyColor)
			{
				BoundSpriteRenderer.color = ColorOverTime.Evaluate(time);
			}
		}

		/// <summary>
		/// Stops the transition on stop if needed
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
            
			StopCoroutine(_coroutine);
			IsPlaying = false;
			_coroutine = null;
		}

		/// <summary>
		/// Turns the sprite renderer on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			BoundSpriteRenderer.gameObject.SetActive(status);
			BoundSpriteRenderer.enabled = status;
		}
        
		/// <summary>
		/// On disable, 
		/// </summary>
		protected virtual void OnDisable()
		{
			_coroutine = null;
		}
	}
}
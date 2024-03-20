using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// SpriteRenderer에 이를 추가하여 피드백에서 MMSpriteRendererShakeEvents를 수신하거나 로컬에서 흔들도록 합니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Renderer/MMSpriteRendererShaker")]
	[RequireComponent(typeof(SpriteRenderer))]
	public class MMSpriteRendererShaker : MMShaker
	{
		[MMInspectorGroup("SpriteRenderer", true, 39)]
		/// the SpriteRenderer to affect when playing the feedback
		[Tooltip("피드백을 재생할 때 영향을 미치는 SpriteRenderer")]
		public SpriteRenderer BoundSpriteRenderer;
		/// whether or not that SpriteRenderer should be turned off on start
		[Tooltip("시작 시 SpriteRenderer를 꺼야 하는지 여부")]
		public bool StartsOff = true;

		[MMInspectorGroup("Color", true, 40)]
		/// whether or not this shaker should modify color 
		[Tooltip("이 셰이커가 colo를 수정해야 하는지 여부r")]
		public bool ModifyColor = true;
		/// the colors to apply to the SpriteRenderer over time
		[Tooltip("시간이 지남에 따라 SpriteRenderer에 적용할 색상")]
		public Gradient ColorOverTime;

		[MMInspectorGroup("Flip", true, 41)]
		/// whether or not to flip the sprite on X
		[Tooltip("whether or not to flip the sprite on X")]
		public bool FlipX = false;
		/// whether or not to flip the sprite on Y
		[Tooltip("whether or not to flip the sprite on Y")]
		public bool FlipY = false;

		protected Color _initialColor;
		protected bool _originalModifyColor;
		protected float _originalShakeDuration;
		protected Gradient _originalColorOverTime;
		protected bool _originalFlipX;
		protected bool _originalFlipY;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			if (BoundSpriteRenderer == null)
			{
				BoundSpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
			}
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 1f;
		}
               
		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			if (ModifyColor)
			{
				_remappedTimeSinceStart = MMFeedbacksHelpers.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
				BoundSpriteRenderer.color = ColorOverTime.Evaluate(_remappedTimeSinceStart);
			}            
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialColor = BoundSpriteRenderer.color;
			_originalFlipX = BoundSpriteRenderer.flipX;
			_originalFlipY = BoundSpriteRenderer.flipY;
		}

		/// <summary>
		/// Resets the target's values
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			BoundSpriteRenderer.color = _initialColor;
			BoundSpriteRenderer.flipX = _originalFlipX;
			BoundSpriteRenderer.flipY = _originalFlipY;
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ModifyColor = _originalModifyColor;
			ShakeDuration = _originalShakeDuration;
			ColorOverTime = _originalColorOverTime;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMSpriteRendererShakeEvent.Register(OnMMSpriteRendererShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMSpriteRendererShakeEvent.Unregister(OnMMSpriteRendererShakeEvent);
		}

		public virtual void OnMMSpriteRendererShakeEvent(float shakeDuration, bool modifyColor, Gradient colorOverTime,
			bool flipX, bool flipY,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!CheckEventAllowed(channelData, useRange, eventRange, eventOriginPosition) ||  (!Interruptible && Shaking))
			{
				return;
			}

			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalModifyColor = ModifyColor;
				_originalShakeDuration = ShakeDuration;
				_originalColorOverTime = ColorOverTime;
			}

			if (!OnlyUseShakerValues)
			{
				ModifyColor = modifyColor;
				ShakeDuration = shakeDuration;
				ColorOverTime = colorOverTime;
				FlipX = flipX;
				FlipY = flipY;
			}

			if (FlipX)
			{
				BoundSpriteRenderer.flipX = !BoundSpriteRenderer.flipX;
			}
			if (FlipY)
			{
				BoundSpriteRenderer.flipY = !BoundSpriteRenderer.flipY;
			}            

			Play();
		}
	}

	/// <summary>
	/// An event used (usually from MMFeeedbackSpriteRenderer) to shake the values of a SpriteRenderer
	/// </summary>
	public struct MMSpriteRendererShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(float shakeDuration, bool modifyColor, Gradient colorOverTime,
			bool flipX, bool flipY,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3));

		static public void Trigger(float shakeDuration, bool modifyColor, Gradient colorOverTime,
			bool flipX, bool flipY,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			OnEvent?.Invoke(shakeDuration, modifyColor, colorOverTime,
				flipX, flipY,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, 
				useRange, eventRange, eventOriginPosition);
		}
	}
}
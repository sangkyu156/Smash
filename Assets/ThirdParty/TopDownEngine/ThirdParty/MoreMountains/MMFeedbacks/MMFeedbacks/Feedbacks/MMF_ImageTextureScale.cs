using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간 경과에 따라 대상 UI 이미지의 텍스처 크기를 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간 경과에 따라 대상 UI 이미지의 텍스처 크기를 제어할 수 있습니다.")]
	[FeedbackPath("UI/Image Texture Scale")]
	public class MMF_ImageTextureScale : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetImage == null); }
		public override string RequiredTargetText { get { return TargetImage != null ? TargetImage.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetImage be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetImage = FindAutomatedTarget<Image>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant }
		//
		public enum MaterialPropertyTypes { Main, TextureID }

		[MMFInspectorGroup("Texture Scale", true, 63, true)]
		/// the UI Image on which to change texture scale on
		[Tooltip("the UI Image on which to change texture scale on")]
		public Image TargetImage;
		/// whether to target the main texture property, or one specified in MaterialPropertyName
		[Tooltip("기본 텍스처 속성을 대상으로 할지 아니면 MaterialPropertyName에 지정된 속성을 대상으로 할지 여부")]
		public MaterialPropertyTypes MaterialPropertyType = MaterialPropertyTypes.Main;
		/// the property name, for example _MainTex_ST, or _MainTex if you don't have UseMaterialPropertyBlocks set to true
		[Tooltip("속성 이름(예: _MainTex_ST 또는 UseMaterialPropertyBlocks가 true로 설정되지 않은 경우 _MainTex)")]
		[MMEnumCondition("MaterialPropertyType", (int)MaterialPropertyTypes.TextureID)]
		public string MaterialPropertyName = "_MainTex_ST";
		/// whether the feedback should affect the material instantly or over a period of time
		[Tooltip("피드백이 자료에 즉시 영향을 미칠지, 아니면 일정 기간에 걸쳐 영향을 미칠지 여부")]
		public Modes Mode = Modes.OverTime;
		/// how long the material should change over time
		[Tooltip("시간이 지남에 따라 재료가 얼마나 오랫동안 변해야 하는지")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not the values should be relative 
		[Tooltip("값이 상대적이어야 하는지 여부")]
		public bool RelativeValues = true;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;
        
		[MMFInspectorGroup("Intensity", true, 65)]
		/// the curve to tween the scale on
		[Tooltip("스케일을 트위닝하는 곡선")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public AnimationCurve ScaleCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the scale curve's 0 to
		[Tooltip("the value to remap the scale curve's 0 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Vector2 RemapZero = Vector2.zero;
		/// the value to remap the scale curve's 1 to
		[Tooltip("the value to remap the scale curve's 1 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Vector2 RemapOne = Vector2.one;
		/// the value to move the intensity to in instant mode
		[Tooltip("인스턴트 모드에서 강도를 이동할 값")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Vector2 InstantScale;

		protected Vector2 _initialValue;
		protected Coroutine _coroutine;
		protected Vector2 _newValue;
		protected Material _material;

		/// the duration of this feedback is the duration of the transition
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// <summary>
		/// On init we store our initial texture scale
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			_material = TargetImage.material;

			switch (MaterialPropertyType)
			{
				case MaterialPropertyTypes.Main:
					_initialValue = _material.mainTextureScale;
					break;
				case MaterialPropertyTypes.TextureID:
					_initialValue = _material.GetTextureScale(MaterialPropertyName);
					break;
			}
		}

		/// <summary>
		/// On Play we initiate our scale change
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
            
			switch (Mode)
			{
				case Modes.Instant:
					ApplyValue(InstantScale * intensityMultiplier);
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(TransitionCo(intensityMultiplier));
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the scale value over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator TransitionCo(float intensityMultiplier)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetMaterialValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetMaterialValues(FinalNormalizedTime, intensityMultiplier);
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Applies scale to the target material 
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetMaterialValues(float time, float intensityMultiplier)
		{
			_newValue.x = MMFeedbacksHelpers.Remap(ScaleCurve.Evaluate(time), 0f, 1f, RemapZero.x, RemapOne.x);
			_newValue.y = MMFeedbacksHelpers.Remap(ScaleCurve.Evaluate(time), 0f, 1f, RemapZero.y, RemapOne.y);

			if (RelativeValues)
			{
				_newValue += _initialValue;
			}

			ApplyValue(_newValue * intensityMultiplier);
		}

		/// <summary>
		/// Applies the specified value to the material
		/// </summary>
		/// <param name="newValue"></param>
		protected virtual void ApplyValue(Vector2 newValue)
		{
			switch (MaterialPropertyType)
			{
				case MaterialPropertyTypes.Main:
					_material.mainTextureScale = newValue;
					break;
				case MaterialPropertyTypes.TextureID:
					_material.SetTextureScale(MaterialPropertyName, newValue);
					break;
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
	}
}
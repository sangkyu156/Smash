using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간이 지남에 따라 대상 재질의 텍스처 크기를 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 대상 재질의 텍스처 크기를 제어할 수 있습니다.")]
	[FeedbackPath("Renderer/Texture Scale")]
	public class MMF_TextureScale : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRenderer == null); }
		public override string RequiredTargetText { get { return TargetRenderer != null ? TargetRenderer.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetRenderer be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRenderer = FindAutomatedTarget<Renderer>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Texture Scale", true, 62, true)]
		/// the renderer on which to change texture scale on
		[Tooltip("텍스처 스케일을 변경할 렌더러")]
		public Renderer TargetRenderer;
		/// the material index
		[Tooltip("the material index")]
		public int MaterialIndex = 0;
		/// the property name, for example _MainTex_ST, or _MainTex if you don't have UseMaterialPropertyBlocks set to true
		[Tooltip("속성 이름(예: _MainTex_ST 또는 UseMaterialPropertyBlocks가 true로 설정되지 않은 경우 _MainTex)")]
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
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("이것이 사실이라면 이 구성요소는 재료의 인스턴스에서 작업하는 대신 재료 특성 블록을 사용합니다.")] 
		public bool UseMaterialPropertyBlocks = false;

		[MMFInspectorGroup("Intensity", true, 64)]
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
		protected MaterialPropertyBlock _propertyBlock;
		protected Vector4 _propertyBlockVector;

		/// the duration of this feedback is the duration of the transition
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasRandomness => true;

		/// <summary>
		/// On init we store our texture scale
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (UseMaterialPropertyBlocks)
			{
				_propertyBlock = new MaterialPropertyBlock();
				TargetRenderer.GetPropertyBlock(_propertyBlock);
				_propertyBlockVector.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).w;
				_propertyBlockVector.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).z;
				_initialValue.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).x;
				_initialValue.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).y;    
			}
			else
			{
				_initialValue = TargetRenderer.materials[MaterialIndex].GetTextureScale(MaterialPropertyName);    
			}
		}

		/// <summary>
		/// On Play we modify our texture scale
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
		/// This coroutine will animate the target material's scale over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator TransitionCo(float intensityMultiplier)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetMaterialValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetMaterialValues(FinalNormalizedTime, intensityMultiplier);
			IsPlaying = true;
			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Applies the scale to the target material
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
			if (UseMaterialPropertyBlocks)
			{
				TargetRenderer.GetPropertyBlock(_propertyBlock);
				_propertyBlockVector.x = newValue.x;
				_propertyBlockVector.y = newValue.y;
				_propertyBlock.SetVector(MaterialPropertyName, _propertyBlockVector);
				TargetRenderer.SetPropertyBlock(_propertyBlock, MaterialIndex);
			}
			else
			{
				TargetRenderer.materials[MaterialIndex].SetTextureScale(MaterialPropertyName, newValue);    
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
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			ApplyValue(_initialValue);
		}
	}
}
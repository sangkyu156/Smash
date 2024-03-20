using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 재생 시 설정된 기간 동안 바운드 렌더러를 깜박이게 합니다(그리고 중지되면 초기 색상을 복원합니다).
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 특정 기간 동안 지정된 옥타브에서 지정된 색상으로 지정된 렌더러(스프라이트, 메시 등)의 색상을 깜박일 수 있습니다. 예를 들어, 캐릭터가 맞았을 때 유용합니다(하지만 훨씬 더 많습니다!).")]
	[FeedbackPath("Renderer/Flicker")]
	public class MMFeedbackFlicker : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
#endif

        /// the possible modes
        /// Color : Material.color를 제어합니다.
        /// PropertyName : 이름으로 특정 셰이더 속성을 타겟팅합니다.
        public enum Modes { Color, PropertyName }

		[Header("Flicker")]
		/// the renderer to flicker when played
		[Tooltip("재생할 때 렌더러가 깜박입니다.")]
		public Renderer BoundRenderer;
		/// the selected mode to flicker the renderer 
		[Tooltip("렌더러를 깜박이게 하기 위해 선택한 모드")]
		public Modes Mode = Modes.Color;
		/// the name of the property to target
		[MMFEnumCondition("Mode", (int)Modes.PropertyName)]
		[Tooltip("타겟팅할 속성의 이름")]
		public string PropertyName = "_Tint";
		/// the duration of the flicker when getting damage
		[Tooltip("피해를 입을 때 깜박이는 지속 시간")]
		public float FlickerDuration = 0.2f;
		/// the frequency at which to flicker
		[Tooltip("깜박이는 빈도")]
		public float FlickerOctave = 0.04f;
		/// the color we should flicker the sprite to 
		[Tooltip("스프라이트를 깜박여야 하는 색상")]
		[ColorUsage(true, true)]
		public Color FlickerColor = new Color32(255, 20, 20, 255);
		/// the list of material indexes we want to flicker on the target renderer. If left empty, will only target the material at index 0 
		[Tooltip("대상 렌더러에서 깜박이려는 머티리얼 인덱스 목록입니다. 비워두면 인덱스 0의 재료만 대상으로 합니다.")]
		public int[] MaterialIndexes;
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("이것이 사실이라면 이 구성요소는 재료의 인스턴스에서 작업하는 대신 재료 특성 블록을 사용합니다.")] 
		public bool UseMaterialPropertyBlocks = false;

		/// the duration of this feedback is the duration of the flicker
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlickerDuration); } set { FlickerDuration = value; } }

		protected const string _colorPropertyName = "_Color";
        
		protected Color[] _initialFlickerColors;
		protected int[] _propertyIDs;
		protected bool[] _propertiesFound;
		protected Coroutine[] _coroutines;
		protected MaterialPropertyBlock _propertyBlock;

		/// <summary>
		/// On init we grab our initial color and components
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			if (MaterialIndexes.Length == 0)
			{
				MaterialIndexes = new int[1];
				MaterialIndexes[0] = 0;
			}

			_coroutines = new Coroutine[MaterialIndexes.Length];

			_initialFlickerColors = new Color[MaterialIndexes.Length];
			_propertyIDs = new int[MaterialIndexes.Length];
			_propertiesFound = new bool[MaterialIndexes.Length];
			_propertyBlock = new MaterialPropertyBlock();
            
			if (Active && (BoundRenderer == null) && (owner != null))
			{
				if (owner.MMFGetComponentNoAlloc<Renderer>() != null)
				{
					BoundRenderer = owner.GetComponent<Renderer>();
				}
				if (BoundRenderer == null)
				{
					BoundRenderer = owner.GetComponentInChildren<Renderer>();
				}
			}

			if (BoundRenderer == null)
			{
				Debug.LogWarning("[MMFeedbackFlicker] The flicker feedback on "+this.name+" doesn't have a bound renderer, it won't work. You need to specify a renderer to flicker in its inspector.");    
			}

			if (Active)
			{
				if (BoundRenderer != null)
				{
					BoundRenderer.GetPropertyBlock(_propertyBlock);    
				}
			}            

			for (int i = 0; i < MaterialIndexes.Length; i++)
			{
				_propertiesFound[i] = false;

				if (Active && (BoundRenderer != null))
				{
					if (Mode == Modes.Color)
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[i].HasProperty(_colorPropertyName) : BoundRenderer.materials[i].HasProperty(_colorPropertyName);
						if (_propertiesFound[i])
						{
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[i].color : BoundRenderer.materials[i].color;
						}
					}
					else
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[i].HasProperty(PropertyName) : BoundRenderer.materials[i].HasProperty(PropertyName); 
						if (_propertiesFound[i])
						{
							_propertyIDs[i] = Shader.PropertyToID(PropertyName);
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[i].GetColor(_propertyIDs[i]) : BoundRenderer.materials[i].GetColor(_propertyIDs[i]);
						}
					}
				}
			}
		}

		/// <summary>
		/// On play we make our renderer flicker
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (BoundRenderer == null))
			{
				return;
			}
			for (int i = 0; i < MaterialIndexes.Length; i++)
			{
				_coroutines[i] = StartCoroutine(Flicker(BoundRenderer, i, _initialFlickerColors[i], FlickerColor, FlickerOctave, FeedbackDuration));
			}
		}

		/// <summary>
		/// On reset we make our renderer stop flickering
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			if (Active && FeedbackTypeAuthorized && (BoundRenderer != null))
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					SetColor(i, _initialFlickerColors[i]);
				}
			}
		}

		public virtual IEnumerator Flicker(Renderer renderer, int materialIndex, Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
		{
			if (renderer == null)
			{
				yield break;
			}

			if (!_propertiesFound[materialIndex])
			{
				yield break;
			}

			if (initialColor == flickerColor)
			{
				yield break;
			}

			float flickerStop = FeedbackTime + flickerDuration;
			IsPlaying = true;
            
			while (FeedbackTime < flickerStop)
			{
				SetColor(materialIndex, flickerColor);
				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
				}
				else
				{
					yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
				}
				SetColor(materialIndex, initialColor);
				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
				}
				else
				{
					yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
				}
			}

			SetColor(materialIndex, initialColor);
			IsPlaying = false;
		}

		protected virtual void SetColor(int materialIndex, Color color)
		{
			if (!_propertiesFound[materialIndex])
			{
				return;
			}
            
			if (Mode == Modes.Color)
			{
				if (UseMaterialPropertyBlocks)
				{
					BoundRenderer.GetPropertyBlock(_propertyBlock);
					_propertyBlock.SetColor(_colorPropertyName, color);
					BoundRenderer.SetPropertyBlock(_propertyBlock, materialIndex);
				}
				else
				{
					BoundRenderer.materials[materialIndex].color = color;
				}
			}
			else
			{
				if (UseMaterialPropertyBlocks)
				{
					BoundRenderer.GetPropertyBlock(_propertyBlock);
					_propertyBlock.SetColor(_propertyIDs[materialIndex], color);
					BoundRenderer.SetPropertyBlock(_propertyBlock, materialIndex);
				}
				else
				{
					BoundRenderer.materials[materialIndex].SetColor(_propertyIDs[materialIndex], color);
				}
			}            
		}
        
		/// <summary>
		/// Stops this feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			IsPlaying = false;
			for (int i = 0; i < _coroutines.Length; i++)
			{
				if (_coroutines[i] != null)
				{
					StopCoroutine(_coroutines[i]);    
				}
				_coroutines[i] = null;    
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 재생 시 설정된 기간 동안 바운드 렌더러를 깜박이게 합니다(그리고 중지되면 초기 색상을 복원합니다).
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 특정 기간 동안 지정된 옥타브에서 지정된 색상으로 지정된 렌더러(스프라이트, 메시 등)의 색상을 깜박일 수 있습니다. 예를 들어, 캐릭터가 맞았을 때 유용합니다(하지만 훨씬 더 많습니다!).")]
	[FeedbackPath("Renderer/Flicker")]
	public class MMF_Flicker : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() => (BoundRenderer == null);
		public override string RequiredTargetText => BoundRenderer != null ? BoundRenderer.name : "";
		public override string RequiresSetupText => "This feedback requires that a BoundRenderer be set to be able to work properly. You can set one below.";
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundRenderer = FindAutomatedTarget<Renderer>();

		/// the possible modes
		/// Color : will control material.color
		/// PropertyName : will target a specific shader property by name
		public enum Modes { Color, PropertyName }

		[MMFInspectorGroup("Flicker", true, 61, true)]
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
		/// if using material property blocks on a sprite renderer, you'll want to make sure the sprite texture gets passed to the block when updating it. For that, you need to specify your sprite's material's shader's texture property name. If you're not working with a sprite renderer, you can safely ignore this.
		[Tooltip("스프라이트 렌더러에서 재료 속성 블록을 사용하는 경우 업데이트할 때 스프라이트 텍스처가 블록에 전달되는지 확인하는 것이 좋습니다. 이를 위해서는 스프라이트 머티리얼 셰이더의 텍스처 속성 이름을 지정해야 합니다. 스프라이트 렌더러로 작업하지 않는 경우에는 이를 무시해도 됩니다.")]
		[MMCondition("UseMaterialPropertyBlocks", true)]
		public string SpriteRendererTextureProperty = "_MainTex";

		/// the duration of this feedback is the duration of the flicker
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlickerDuration); } set { FlickerDuration = value; } }

		protected const string _colorPropertyName = "_Color";
        
		protected Color[] _initialFlickerColors;
		protected int[] _propertyIDs;
		protected bool[] _propertiesFound;
		protected Coroutine[] _coroutines;
		protected MaterialPropertyBlock _propertyBlock;
		protected SpriteRenderer _spriteRenderer;
		protected Texture2D _spriteRendererTexture;
		protected bool _spriteRendererIsNull;
		

		/// <summary>
		/// On init we grab our initial color and components
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
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
				if (Owner.gameObject.MMFGetComponentNoAlloc<Renderer>() != null)
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
				Debug.LogWarning("[MMFeedbackFlicker] The flicker feedback on "+Owner.name+" doesn't have a bound renderer, it won't work. You need to specify a renderer to flicker in its inspector.");    
			}
			
			_spriteRenderer = BoundRenderer.GetComponent<SpriteRenderer>();
			_spriteRendererIsNull = _spriteRenderer == null;
			StoreSpriteRendererTexture();

			for (int i = 0; i < MaterialIndexes.Length; i++)
			{
				_propertiesFound[i] = false;
				int index = MaterialIndexes[i];

				if (Active && (BoundRenderer != null))
				{
					if (Mode == Modes.Color)
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].HasProperty(_colorPropertyName) : BoundRenderer.materials[index].HasProperty(_colorPropertyName);
						if (_propertiesFound[i])
						{
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].color : BoundRenderer.materials[index].color;
						}
					}
					else
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].HasProperty(PropertyName) : BoundRenderer.materials[index].HasProperty(PropertyName); 
						if (_propertiesFound[i])
						{
							_propertyIDs[i] = Shader.PropertyToID(PropertyName);
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].GetColor(_propertyIDs[i]) : BoundRenderer.materials[index].GetColor(_propertyIDs[i]);
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
				_coroutines[i] = Owner.StartCoroutine(Flicker(BoundRenderer, i, _initialFlickerColors[i], FlickerColor, FlickerOctave, FeedbackDuration));
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

		protected virtual void StoreSpriteRendererTexture()
		{
			if (_spriteRendererIsNull)
			{
				return;
			}
			_spriteRendererTexture = _spriteRenderer.sprite.texture;
		}
		
		protected virtual void SetStoredSpriteRendererTexture(MaterialPropertyBlock block)
		{
			if (_spriteRendererIsNull)
			{
				return;
			}
			block.SetTexture(SpriteRendererTextureProperty, _spriteRendererTexture);
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
			
			StoreSpriteRendererTexture();
            
			while (FeedbackTime < flickerStop)
			{
				SetColor(materialIndex, flickerColor);
				yield return WaitFor(flickerSpeed);
				SetColor(materialIndex, initialColor);
				yield return WaitFor(flickerSpeed);
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
					BoundRenderer.GetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
					_propertyBlock.SetColor(_colorPropertyName, color);
					SetStoredSpriteRendererTexture(_propertyBlock);
					BoundRenderer.SetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
				}
				else
				{
					BoundRenderer.materials[MaterialIndexes[materialIndex]].color = color;
				}
			}
			else
			{
				if (UseMaterialPropertyBlocks)
				{
					BoundRenderer.GetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
					_propertyBlock.SetColor(_propertyIDs[materialIndex], color);
					SetStoredSpriteRendererTexture(_propertyBlock);
					BoundRenderer.SetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
				}
				else
				{
					BoundRenderer.materials[MaterialIndexes[materialIndex]].SetColor(_propertyIDs[materialIndex], color);
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
					Owner.StopCoroutine(_coroutines[i]);    
				}
				_coroutines[i] = null;    
			}
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

			CustomReset();
		}
	}
}
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

namespace MoreMountains.Tools
{

    /// <summary>
    /// 시간이 지남에 따라 다른 클래스의 부동 소수점을 제어하는 ​​데 사용되는 클래스
    /// 이를 사용하려면 대상 필드에 단일 동작을 드래그하고 제어 모드(핑퐁 또는 무작위)를 선택한 다음 설정을 조정하면 됩니다.
    /// </summary>
    [MMRequiresConstantRepaint]
	[AddComponentMenu("More Mountains/Tools/Property Controllers/ShaderController")]
	public class ShaderController : MMMonoBehaviour
	{
		/// the possible types of targets
		public enum TargetTypes { Renderer, Image, RawImage, Text }
		/// the possible types of properties
		public enum PropertyTypes { Bool, Float, Int, Vector, Keyword, Color }
		/// the possible control modes
		public enum ControlModes { PingPong, Random, OneTime, AudioAnalyzer, ToDestination, Driven, Loop }
		/// the possible color modes on which to interpolate colors
		public enum ColorModes { TwoColors, ColorRamp }

		[Header("Target")]
		/// the type of renderer to pilot
		[Tooltip("파일럿할 렌더러 유형")]
		public TargetTypes TargetType = TargetTypes.Renderer;
		/// the renderer with the shader you want to control
		[Tooltip("제어하려는 셰이더가 포함된 렌더러")]
		[MMEnumCondition("TargetType",(int)TargetTypes.Renderer)]
		public Renderer TargetRenderer;
		/// the ID of the material in the Materials array on the target renderer (usually 0)
		[Tooltip("대상 렌더러의 Materials 배열에 있는 재질의 ID(일반적으로 0)")]
		[MMEnumCondition("TargetType", (int)TargetTypes.Renderer)]
		public int TargetMaterialID = 0;
		/// the Image with the shader you want to control
		[Tooltip("제어하려는 셰이더가 포함된 이미지")]
		[MMEnumCondition("TargetType", (int)TargetTypes.Image)]
		public Image TargetImage;
		/// if this is true, the 'materialForRendering' for this Image will be used, instead of the regular material
		[Tooltip("이것이 사실이라면 일반 재질 대신 이 이미지의 'materialForRendering'이 사용됩니다.")]
		[MMEnumCondition("TargetType", (int)TargetTypes.Image)]
		public bool UseMaterialForRendering = false;
		/// the RawImage with the shader you want to control
		[Tooltip("제어하려는 셰이더가 포함된 RawImage")]
		[MMEnumCondition("TargetType", (int)TargetTypes.RawImage)]
		public RawImage TargetRawImage;
		/// the Text with the shader you want to control
		[Tooltip("제어하려는 셰이더가 포함된 텍스트")]
		[MMEnumCondition("TargetType", (int)TargetTypes.Text)]
		public Text TargetText;
		/// if this is true, material will be cached on Start
		[Tooltip("이것이 사실이라면 시작 시 자료가 캐시됩니다.")]
		public bool CacheMaterial = true;
		/// if this is true, an instance of the material will be created on start so that this controller only affects its target
		[Tooltip("이것이 사실이라면, 이 컨트롤러가 대상에만 영향을 미치도록 머티리얼 인스턴스가 시작 시 생성됩니다.")]
		public bool CreateMaterialInstance = false;
		/// the EXACT name of the property to affect
		[Tooltip("영향을 미칠 부동산의 정확한 이름")]
		public string TargetPropertyName;
		/// the type of the property to affect
		[Tooltip("영향을 미치는 속성의 유형")]
		public PropertyTypes PropertyType = PropertyTypes.Float;
		/// whether or not to affect its x component
		[Tooltip("X 구성요소에 영향을 미칠지 여부")]
		[MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
		public bool X;
		/// whether or not to affect its y component
		[Tooltip("Y 구성요소에 영향을 미칠지 여부")]
		[MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
		public bool Y;
		/// whether or not to affect its z component
		[Tooltip("Z 구성요소에 영향을 미칠지 여부")]
		[MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
		public bool Z;
		/// whether or not to affect its w component
		[Tooltip("w 구성요소에 영향을 미칠지 여부")]
		[MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
		public bool W;

		[Header("Color")]
		/// whether to move from a color to another, or to evalute colors on a ramp
		[Tooltip("한 색상에서 다른 색상으로 이동할지 아니면 경사로에서 색상을 평가할지 여부")]
		public ColorModes ColorMode = ColorModes.TwoColors;
		/// the ramp along which to lerp when in ramp color mode
		[Tooltip("램프 색상 모드에 있을 때 따라갈 램프")]
		[GradientUsage(true)]
		public Gradient ColorRamp;
		/// the color to lerp from	
		[Tooltip("lerp할 색상")]
		[ColorUsage(true, true)]
		public Color FromColor = Color.black;
		/// the color to lerp to	
		[Tooltip("lerp 할 색상")]
		[ColorUsage(true, true)]
		public Color ToColor = Color.white;

		[Header("Global Settings")]
		/// the control mode (ping pong or random)
		[Tooltip("제어 모드(탁구 또는 무작위)")]
		public ControlModes ControlMode;
		/// whether or not the updated value should be added to the initial one
		[Tooltip("업데이트된 값을 초기 값에 추가해야 하는지 여부")]
		public bool AddToInitialValue = false;
		/// whether or not to use unscaled time
		[Tooltip("크기 조정되지 않은 시간을 사용할지 여부")]
		public bool UseUnscaledTime = true;
		/// whether or not you want to revert to the InitialValue after the control ends
		[Tooltip("컨트롤이 끝난 후 초기 값으로 되돌릴지 여부")]
		public bool RevertToInitialValueAfterEnd = true;
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("이것이 사실이라면 이 구성요소는 재료의 인스턴스에서 작업하는 대신 재료 특성 블록을 사용합니다.")] 
		[MMEnumCondition("TargetType", (int)TargetTypes.Renderer)]
		public bool UseMaterialPropertyBlocks = false;
		/// if using material property blocks on a sprite renderer, you'll want to make sure the sprite texture gets passed to the block when updating it. For that, you need to specify your sprite's material's shader's texture property name. If you're not working with a sprite renderer, you can safely ignore this.
		[Tooltip("스프라이트 렌더러에서 재료 속성 블록을 사용하는 경우 업데이트할 때 스프라이트 텍스처가 블록에 전달되는지 확인하는 것이 좋습니다. 이를 위해서는 스프라이트 머티리얼 셰이더의 텍스처 속성 이름을 지정해야 합니다. 스프라이트 렌더러로 작업하지 않는 경우에는 이를 무시해도 됩니다.")]
		[MMCondition("UseMaterialPropertyBlocks", true)]
		public string SpriteRendererTextureProperty = "_MainTex";
		/// whether or not to perform extra safety checks (safer, more costly)
		[Tooltip("추가 안전 점검 수행 여부(더 안전하고 비용이 더 많이 듭니다)")]
		public bool SafeMode = false;

		[Header("Ping Pong")]
		/// the curve to apply to the tween
		[Tooltip("트윈에 적용할 곡선")]
		public MMTweenType Curve;
		/// the minimum value for the ping pong
		[Tooltip("탁구의 최소값")]
		public float MinValue = 0f;
		/// the maximum value for the ping pong
		[Tooltip("탁구의 최대값")]
		public float MaxValue = 5f;
		/// the duration of one ping (or pong)
		[Tooltip("한 번의 핑(또는 퐁)의 지속 시간")]
		public float Duration = 1f;
		/// the duration of the pause between two ping (or pongs) (in seconds)
		[Tooltip("두 개의 핑(또는 퐁) 사이의 일시 중지 기간(초)")]
		public float PingPongPauseDuration = 1f;

		[Header("Loop")]
		/// the curve to apply to the tween
		[Tooltip("트윈에 적용할 곡선")]
		public MMTweenType LoopCurve;
		/// the start value for the loop tween
		[Tooltip("루프 트윈의 시작 값")]
		public float LoopStartValue = 0f;
		/// the end value for the loop tween
		[Tooltip("루프 트윈의 최종 값")]
		public float LoopEndValue = 5f;
		/// the duration of one loop
		[Tooltip("한 루프의 지속 시간")]
		public float LoopDuration = 1f;
		/// the duration of the pause between two loops (in seconds)
		[Tooltip("두 루프 사이의 일시 중지 기간(초)")]
		public float LoopPauseDuration = 1f;

		[Header("Driven")]
		/// the value that will be applied to the controlled float in driven mode 
		[Tooltip("구동 모드에서 제어된 플로트에 적용될 값")]
		public float DrivenLevel = 0f;

		[Header("Random")]
		/// the noise amplitude
		[Tooltip("소음 진폭")]
		[MMVector("Min", "Max")]
		public Vector2 Amplitude = new Vector2(0f,5f);
		/// the noise frequency
		[Tooltip("소음 주파수")]
		[MMVector("Min", "Max")]
		public Vector2 Frequency = new Vector2(1f, 1f);
		/// the noise shift
		[Tooltip("소음 변화")]
		[MMVector("Min", "Max")]
		public Vector2 Shift = new Vector2(0f, 1f);

		/// if this is true, will let you remap the noise value (without amplitude) to the bounds you've specified
		[Tooltip("이것이 사실이라면 노이즈 값(진폭 제외)을 지정한 경계로 다시 매핑할 수 있습니다.")]
		public bool RemapNoiseValues = false;
		/// the value to which to remap the random's zero bound
		[Tooltip("무작위의 영점 경계를 다시 매핑할 값")]
		[MMCondition("RemapNoiseValues", true)]
		public float RemapNoiseZero = 0f;
		/// the value to which to remap the random's one bound
		[Tooltip("무작위의 한 경계를 다시 매핑할 값")]
		[MMCondition("RemapNoiseValues", true)]
		public float RemapNoiseOne = 1f;
        
		[Header("OneTime")]
		/// the duration of the One Time shake
		[Tooltip("One Time Shake의 지속 시간")]
		public float OneTimeDuration = 1f;
		/// the amplitude of the One Time shake (this will be multiplied by the curve's height)
		[Tooltip("일회성 흔들림의 진폭(이에 곡선 높이를 곱함)")]
		public float OneTimeAmplitude = 1f;
		/// the low value to remap the normalized curve value to 
		[Tooltip("정규화된 곡선 값을 다시 매핑할 낮은 값")]
		public float OneTimeRemapMin = 0f;
		/// the high value to remap the normalized curve value to 
		[Tooltip("정규화된 곡선 값을 다시 매핑할 높은 값")]
		public float OneTimeRemapMax = 1f;
		/// the curve to apply to the one time shake
		[Tooltip("일회성 흔들기에 적용할 곡선")]
		public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		[MMInspectorButton("OneTime")]
		/// a test button for the one time shake
		[Tooltip("일회성 흔들림을 위한 테스트 버튼")]
		public bool OneTimeButton;
		/// whether or not this controller should go back to sleep after a OneTime
		[Tooltip("이 컨트롤러가 OneTime 이후에 다시 절전 모드로 전환되어야 하는지 여부")]
		public bool DisableAfterOneTime = false;
		/// whether or not this controller should go back to sleep after a OneTime
		[Tooltip("이 컨트롤러가 OneTime 이후에 다시 절전 모드로 전환되어야 하는지 여부")]
		public bool DisableGameObjectAfterOneTime = false;

		[Header("AudioAnalyzer")]
		/// the bound audio analyzer used to drive this controller
		[Tooltip("이 컨트롤러를 구동하는 데 사용되는 바운드 오디오 분석기")]
		public MMAudioAnalyzer AudioAnalyzer;
		/// the ID of the selected beat on the analyzer
		[Tooltip("분석기에서 선택한 비트의 ID")]
		public int BeatID;
		/// the multiplier to apply to the value out of the analyzer
		[Tooltip("분석기의 값에 적용할 승수")]
		public float AudioAnalyzerMultiplier = 1f;
		/// the offset to apply to the value out of the analyzer
		[Tooltip("분석기의 값에 적용할 오프셋")]
		public float AudioAnalyzerOffset = 0f;
		/// the speed at which to lerp the value
		[Tooltip("값을 lerp하는 속도")]
		public float AudioAnalyzerLerp = 60f;

		[Header("ToDestination")]
		/// the value to go to when in ToDestination mode
		[Tooltip("ToDestination 모드에 있을 때 이동할 값")]
		public float ToDestinationValue = 1f;
		/// the duration of the ToDestination tween
		[Tooltip("ToDestination 트윈의 지속 시간")]
		public float ToDestinationDuration = 1f;
		/// the curve to use to tween to the ToDestination value
		[Tooltip("ToDestination 값으로 트위닝하는 데 사용할 곡선")]
		public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.6f), new Keyframe(1f, 1f));
		/// a test button for the one time shake
		[Tooltip("일회성 흔들림을 위한 테스트 버튼")]
		[MMInspectorButton("ToDestination")]
		public bool ToDestinationButton;
		/// whether or not this controller should go back to sleep after a OneTime
		[Tooltip("이 컨트롤러가 OneTime 이후에 다시 절전 모드로 전환되어야 하는지 여부")]
		public bool DisableAfterToDestination = false;

		[Header("Debug")]
		/// the initial value of the controlled float
		[Tooltip("제어되는 플로트의 초기값")]
		[MMReadOnly]
		public float InitialValue;
		/// the current value of the controlled float
		[Tooltip("제어되는 플로트의 현재 값")]
		[MMReadOnly]
		public float CurrentValue;
		/// the current value of the controlled float, normalized
		[Tooltip("제어된 플로트의 현재 값, 정규화됨")]
		[MMReadOnly]
		public float CurrentValueNormalized = 0f;
		/// the current value of the controlled float	
		[Tooltip("제어되는 플로트의 현재 값")]
		[MMReadOnly]
		public Color InitialColor;

		/// the ID of the property
		[Tooltip("부동산의 ID")]
		[MMReadOnly]
		public int PropertyID;
		/// whether or not the property got found
		[Tooltip("부동산이 발견되었는지 여부")]
		[MMReadOnly]
		public bool PropertyFound = false;
		/// the target material
		[Tooltip("the target material")]
		[MMReadOnly]
		public Material TargetMaterial;

		/// internal use only
		[HideInInspector]
		public float PingPong;
		/// internal use only
		[HideInInspector]
		public float LoopTime;
        
		protected float _randomAmplitude;
		protected float _randomFrequency;
		protected float _randomShift;
		protected float _elapsedTime = 0f;
		protected bool _shaking = false;
		protected float _startedTimestamp = 0f;
		protected float _remappedTimeSinceStart = 0f;
		protected Color _currentColor;
		protected Vector4 _vectorValue;
		protected float _pingPongDirection = 1f;
		protected float _lastPingPongPauseAt = 0f;
		protected float _lastLoopPauseAt = 0f;
		protected float _initialValue = 0f;
		protected Color _fromColorStorage;
		protected bool _activeLastFrame = false;
		protected MaterialPropertyBlock _propertyBlock;
		protected SpriteRenderer _spriteRenderer;
		protected Texture2D _spriteRendererTexture;
		protected bool SpriteRendererIsNull;

		/// <summary>
		/// Finds an attribute (property or field) on the target object
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public virtual bool FindShaderProperty(string propertyName)
		{
			if (TargetType == TargetTypes.Renderer)
			{
				if (CreateMaterialInstance)
				{
					TargetRenderer.materials[TargetMaterialID] = new Material(TargetRenderer.materials[TargetMaterialID]);
				}
				TargetMaterial = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterials[TargetMaterialID] : TargetRenderer.materials[TargetMaterialID];
			}
			else if (TargetType == TargetTypes.Image)
			{
				if (CreateMaterialInstance)
				{
					TargetImage.material = new Material(TargetImage.material);
				}
				TargetMaterial = TargetImage.material;
			}
			else if (TargetType == TargetTypes.RawImage)
			{
				if (CreateMaterialInstance)
				{
					TargetRawImage.material = new Material(TargetRawImage.material);
				}
				TargetMaterial = TargetRawImage.material;
			}
			else if (TargetType == TargetTypes.Text)
			{
				if (CreateMaterialInstance)
				{
					TargetText.material = new Material(TargetText.material);
				}
				TargetMaterial = TargetText.material;
			}

			if (PropertyType == PropertyTypes.Keyword)
			{
				PropertyFound = true;
				return true;
			}
			if (TargetMaterial.HasProperty(propertyName))
			{                
				PropertyID = Shader.PropertyToID(propertyName);
				PropertyFound = true;
				return true;
			}
			return false;
		}

		/// <summary>
		/// On start we initialize our controller
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On enable, grabs the initial value
		/// </summary>
		protected virtual void OnEnable()
		{
			InitialValue = GetInitialValue();
			if (PropertyType == PropertyTypes.Color)
			{
				InitialColor = TargetMaterial.GetColor(PropertyID);
			}
		}

		/// <summary>
		/// Returns true if the renderer is null, false otherwise
		/// </summary>
		/// <returns></returns>
		protected virtual bool RendererIsNull()
		{
			if ((TargetType == TargetTypes.Renderer) && (TargetRenderer == null))
			{
				return true;
			}
			if ((TargetType == TargetTypes.Image) && (TargetImage == null))
			{
				return true;
			}
			if ((TargetType == TargetTypes.RawImage) && (TargetRawImage == null))
			{
				return true;
			}
			if ((TargetType == TargetTypes.Text) && (TargetText == null))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Grabs the target property and initializes stuff
		/// </summary>
		public virtual void Initialization()
		{
			if (RendererIsNull() || (string.IsNullOrEmpty(TargetPropertyName)))
			{
				return;
			}
			if (TargetType != TargetTypes.Renderer)
			{
				UseMaterialPropertyBlocks = false;
			}

			StoreSpriteRenderer();
            
			PropertyFound = FindShaderProperty(TargetPropertyName);
			if (!PropertyFound)
			{
				return;
			}

			_elapsedTime = 0f;
			_randomAmplitude = Random.Range(Amplitude.x, Amplitude.y);
			_randomFrequency = Random.Range(Frequency.x, Frequency.y);
			_randomShift = Random.Range(Shift.x, Shift.y);
            
			if ((TargetType == TargetTypes.Renderer) && UseMaterialPropertyBlocks)
			{
				_propertyBlock = new MaterialPropertyBlock();
				TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
			}

			InitialValue = GetInitialValue();
			if (PropertyType == PropertyTypes.Color)
			{
				InitialColor = TargetMaterial.GetColor(PropertyID);
			}
                
			_shaking = false;
			if (ControlMode == ControlModes.OneTime)
			{
				this.enabled = false;
			}
			StoreSpriteRendererTexture();
		}

		/// <summary>
		/// Stores the sprite renderer and a test for it
		/// </summary>
		public virtual void StoreSpriteRenderer()
		{
			_spriteRenderer = (TargetRenderer != null) ? TargetRenderer.GetComponent<SpriteRenderer>() : null;
			SpriteRendererIsNull = _spriteRenderer == null;
		}

		/// <summary>
		/// Stores the SpriteRenderer's texture if found
		/// </summary>
		public virtual void StoreSpriteRendererTexture()
		{
			if (SpriteRendererIsNull)
			{
				return;
			}
			_spriteRendererTexture = _spriteRenderer.sprite.texture;
		}

		/// <summary>
		/// Sets the texture associated with the sprite renderer to the specified block
		/// </summary>
		/// <param name="block"></param>
		protected virtual void SetStoredSpriteRendererTexture(MaterialPropertyBlock block)
		{
			if (SpriteRendererIsNull)
			{
				return;
			}
			block.SetTexture(SpriteRendererTextureProperty, _spriteRendererTexture);
		}

		/// <summary>
		/// Sets the level to the value passed in parameters
		/// </summary>
		/// <param name="level"></param>
		public virtual void SetDrivenLevelAbsolute(float level)
		{
			DrivenLevel = level;
		}

		/// <summary>
		/// Sets the level to the remapped value passed in parameters
		/// </summary>
		/// <param name="normalizedLevel"></param>
		/// <param name="remapZero"></param>
		/// <param name="remapOne"></param>
		public virtual void SetDrivenLevelNormalized(float normalizedLevel, float remapZero, float remapOne)
		{
			DrivenLevel = MMMaths.Remap(normalizedLevel, 0f, 1f, remapZero, remapOne);
		}

		/// <summary>
		/// Triggers a one time shake of the shader controller
		/// </summary>
		public virtual void OneTime()
		{
			if (!CacheMaterial)
			{
				Initialization();
			}

			if (RendererIsNull() || (!PropertyFound))
			{
				return;
			}
			else
			{
				this.gameObject.SetActive(true);
				this.enabled = true;
				ControlMode = ControlModes.OneTime;
				_startedTimestamp = GetTime();
				_shaking = true;
			}
		}

		/// <summary>
		/// Triggers a one time shake of the controller to a specified destination value
		/// </summary>
		public virtual void ToDestination()
		{
			if (!CacheMaterial)
			{
				Initialization();
			}
			if (RendererIsNull() || (!PropertyFound))
			{
				return;
			}
			else
			{
				this.enabled = true;
				if (PropertyType == PropertyTypes.Color)
				{
					_fromColorStorage = FromColor;
					FromColor = TargetMaterial.GetColor(PropertyID);
				}                
				ControlMode = ControlModes.ToDestination;
				_startedTimestamp = GetTime();
				_shaking = true;
				_initialValue = GetInitialValue();
			}
		}

		/// <summary>
		/// Use this method to change the FromColor value
		/// </summary>
		/// <param name="newColor"></param>
		public void SetFromColor(Color newColor) { FromColor = newColor; }

		/// <summary>
		/// Use this method to change the ToColor value
		/// </summary>
		/// <param name="newColor"></param>
		public void SetToColor(Color newColor) { ToColor = newColor; }

		/// <summary>
		/// Use this method to change the OneTimeRemapMin value
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetRemapOneTimeMin(float newValue) { OneTimeRemapMin = newValue; }

		/// <summary>
		/// Use this method to change the OneTimeRemapMax value
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetRemapOneTimeMax(float newValue) { OneTimeRemapMax = newValue; }

		/// <summary>
		/// Use this method to change the ToDestinationValue 
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetToDestinationValue(float newValue) { ToDestinationValue = newValue; }

		/// <summary>
		/// Returns the relevant delta time
		/// </summary>
		/// <returns></returns>
		protected float GetDeltaTime()
		{
			return UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
		}

		/// <summary>
		/// Returns the relevant time
		/// </summary>
		/// <returns></returns>
		protected float GetTime()
		{
			return UseUnscaledTime ? Time.unscaledTime : Time.time;
		}

		/// <summary>
		/// On Update, we move our value based on the defined settings
		/// </summary>
		protected virtual void Update()
		{
			UpdateValue();
		}

		protected virtual void OnDisable()
		{
			if (RevertToInitialValueAfterEnd)
			{
				CurrentValue = InitialValue;
				_currentColor = InitialColor;
				SetValue(CurrentValue);
			}
		}

		/// <summary>
		/// Updates the value over time based on the selected options
		/// </summary>
		protected virtual void UpdateValue()
		{
			if (SafeMode)
			{
				if (RendererIsNull() || (!PropertyFound))
				{
					return;
				}    
			}

			switch (ControlMode)
			{
				case ControlModes.PingPong:
					if (GetTime() - _lastPingPongPauseAt < PingPongPauseDuration)
					{
						return;
					}
					PingPong += GetDeltaTime() * _pingPongDirection;
					if (PingPong < 0f)
					{
						PingPong = 0f;
						_pingPongDirection = -_pingPongDirection;
						_lastPingPongPauseAt = GetTime();
					}

					if (PingPong > Duration)
					{
						PingPong = Duration;
						_pingPongDirection = -_pingPongDirection;
						_lastPingPongPauseAt = GetTime();
					}
					CurrentValue = MMTween.Tween(PingPong, 0f, Duration, MinValue, MaxValue, Curve);
					CurrentValueNormalized = MMMaths.Remap(CurrentValue, MinValue, MaxValue, 0f, 1f);
					break;
				case ControlModes.Loop:
					if (GetTime() - _lastLoopPauseAt < LoopPauseDuration)
					{
						return;
					}
					LoopTime += GetDeltaTime();
					if (LoopTime > LoopDuration)
					{
						LoopTime = 0f;
						_lastLoopPauseAt = GetTime();
					}
					CurrentValue = MMTween.Tween(LoopTime, 0f, LoopDuration, LoopStartValue, LoopEndValue, LoopCurve);
					CurrentValueNormalized = MMMaths.Remap(CurrentValue, LoopStartValue, LoopEndValue, 0f, 1f);
					break;
				case ControlModes.Random:
					_elapsedTime += GetDeltaTime();
					CurrentValueNormalized = Mathf.PerlinNoise(_randomFrequency * _elapsedTime, _randomShift); 
					if (RemapNoiseValues)
					{
						CurrentValue = CurrentValueNormalized;
						CurrentValue = MMMaths.Remap(CurrentValue, 0f, 1f, RemapNoiseZero, RemapNoiseOne);
					}
					else
					{
						CurrentValue = (CurrentValueNormalized * 2.0f - 1.0f) * _randomAmplitude;
					}
					break;
				case ControlModes.OneTime:
					if (!_shaking)
					{
						return;
					}
					_remappedTimeSinceStart = MMMaths.Remap(GetTime() - _startedTimestamp, 0f, OneTimeDuration, 0f, 1f);
					CurrentValueNormalized = OneTimeCurve.Evaluate(_remappedTimeSinceStart);
					CurrentValue = MMMaths.Remap(CurrentValueNormalized, 0f, 1f, OneTimeRemapMin, OneTimeRemapMax);
					CurrentValue *= OneTimeAmplitude;
					break;
				case ControlModes.AudioAnalyzer:
					CurrentValue = Mathf.Lerp(CurrentValue, AudioAnalyzer.Beats[BeatID].CurrentValue * AudioAnalyzerMultiplier + AudioAnalyzerOffset, AudioAnalyzerLerp * GetDeltaTime());
					CurrentValueNormalized = Mathf.Clamp(AudioAnalyzer.Beats[BeatID].CurrentValue, 0f, 1f);
					break;
				case ControlModes.Driven:
					CurrentValue = DrivenLevel;
					CurrentValueNormalized = Mathf.Clamp(CurrentValue, 0f, 1f);
					break;
				case ControlModes.ToDestination:
					if (!_shaking)
					{
						return;
					}
					_remappedTimeSinceStart = MMMaths.Remap(GetTime() - _startedTimestamp, 0f, ToDestinationDuration, 0f, 1f);
					float time = ToDestinationCurve.Evaluate(_remappedTimeSinceStart);
					CurrentValue = Mathf.LerpUnclamped(_initialValue, ToDestinationValue, time);
					CurrentValueNormalized = MMMaths.Remap(CurrentValue, _initialValue, ToDestinationValue, 0f, 1f);
					break;
			}

			if (PropertyType == PropertyTypes.Color)
			{
				if (ColorMode == ColorModes.TwoColors)
				{
					_currentColor = Color.Lerp(FromColor, ToColor, CurrentValue);	
				}
				else
				{
					_currentColor = ColorRamp.Evaluate(CurrentValue);
				}
			}

			if (AddToInitialValue)
			{
				CurrentValue += InitialValue;
			}

			if ((ControlMode == ControlModes.OneTime) && _shaking && (GetTime() - _startedTimestamp > OneTimeDuration))
			{
				_shaking = false;
				if (RevertToInitialValueAfterEnd)
				{
					CurrentValue = InitialValue;
					if (PropertyType == PropertyTypes.Color)
					{
						_currentColor = InitialColor;
					}
				}
				else
				{
					CurrentValue = OneTimeCurve.Evaluate(1f);
					CurrentValue = MMMaths.Remap(CurrentValue, 0f, 1f, OneTimeRemapMin, OneTimeRemapMax);
					CurrentValue *= OneTimeAmplitude;
				}
				SetValue(CurrentValue);
				if (DisableAfterOneTime)
				{
					this.enabled = false;
				}     
				if (DisableGameObjectAfterOneTime)
				{
					this.gameObject.SetActive(false);
				}
				return;
			}

			if ((ControlMode == ControlModes.ToDestination) && _shaking && (GetTime() - _startedTimestamp > ToDestinationDuration))
			{
				_shaking = false;
				FromColor = _fromColorStorage;
				if (RevertToInitialValueAfterEnd)
				{
					CurrentValue = InitialValue;
					if (PropertyType == PropertyTypes.Color)
					{
						_currentColor = InitialColor;
					}
				}
				else
				{
					CurrentValue = ToDestinationValue;
				}
				SetValue(CurrentValue);
				if (DisableAfterToDestination)
				{
					this.enabled = false;
				}
				return;
			}

			SetValue(CurrentValue);
		}

		/// <summary>
		/// Grabs and stores the initial value
		/// </summary>
		protected virtual float GetInitialValue()
		{
			if (TargetMaterial == null)
			{
				Debug.LogWarning("Material is null", this);
				return 0f;
			}

			switch (PropertyType)
			{
				case PropertyTypes.Bool:
					return TargetMaterial.GetInt(PropertyID);                    

				case PropertyTypes.Int:
					return TargetMaterial.GetInt(PropertyID);

				case PropertyTypes.Float:
					return TargetMaterial.GetFloat(PropertyID);

				case PropertyTypes.Vector:
					return TargetMaterial.GetVector(PropertyID).x;                    

				case PropertyTypes.Keyword:
					return TargetMaterial.IsKeywordEnabled(TargetPropertyName) ? 1f : 0f;

				case PropertyTypes.Color:
					if (ControlMode != ControlModes.ToDestination)
					{
						InitialColor = TargetMaterial.GetColor(PropertyID);
					}                    
					return 0f;

				default:
					return 0f;
			}
		}

		/// <summary>
		/// Sets the value in the shader
		/// </summary>
		/// <param name="newValue"></param>
		protected virtual void SetValue(float newValue)
		{
			if (TargetType == TargetTypes.Image && UseMaterialForRendering)
			{
				if (SafeMode)
				{
					if (TargetImage == null)
					{
						return;    
					}
				}
				TargetMaterial = TargetImage.materialForRendering;
			}

			switch (PropertyType)
			{
				case PropertyTypes.Bool:
					newValue = (newValue > 0f) ? 1f : 0f;
					int newBool = Mathf.RoundToInt(newValue);
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						StoreSpriteRendererTexture();
						_propertyBlock.SetInt(PropertyID, newBool);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetInt(PropertyID, newBool);    
					}
					break;

				case PropertyTypes.Keyword:
					newValue = (newValue > 0f) ? 1f : 0f;
					if (newValue == 0f)
					{
						TargetMaterial.DisableKeyword(TargetPropertyName);
					}
					else
					{
						TargetMaterial.EnableKeyword(TargetPropertyName);
					}
					break;

				case PropertyTypes.Int:
					int newInt = Mathf.RoundToInt(newValue);
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						StoreSpriteRendererTexture();
						_propertyBlock.SetInt(PropertyID, newInt);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetInt(PropertyID, newInt);    
					}
					break;

				case PropertyTypes.Float:
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						StoreSpriteRendererTexture();
						_propertyBlock.SetFloat(PropertyID, newValue);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetFloat(PropertyID, newValue);
					}
					break;

				case PropertyTypes.Vector:
					_vectorValue = TargetMaterial.GetVector(PropertyID);
					if (X)
					{
						_vectorValue.x = newValue;
					}
					if (Y)
					{
						_vectorValue.y = newValue;
					}
					if (Z)
					{
						_vectorValue.z = newValue;
					}
					if (W)
					{
						_vectorValue.w = newValue;
					}
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						_propertyBlock.SetVector(PropertyID, _vectorValue);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetVector(PropertyID, _vectorValue);
					}
					break;
                    
				case PropertyTypes.Color:
					if (UseMaterialPropertyBlocks)
					{
						if (TargetRenderer == null) { return; }
						TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
						StoreSpriteRendererTexture();
						_propertyBlock.SetColor(PropertyID, _currentColor);
						SetStoredSpriteRendererTexture(_propertyBlock);
						TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
					}
					else
					{
						TargetMaterial.SetColor(PropertyID, _currentColor);
					}
					break;
			}
		}

		/// <summary>
		/// Interrupts any tween in progress, and disables itself
		/// </summary>
		public virtual void Stop()
		{
			_shaking = false;
			this.enabled = false;
		}

		public virtual void RestoreInitialValues()
		{
			_currentColor = InitialColor;
			SetValue(InitialValue);
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	#region Events
	/// <summary>
	/// An event used (usually by feedbacks) to trigger the spawn of a new floating text
	/// </summary>
	public struct MMFloatingTextSpawnEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMChannelData channelData, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
			bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false);
		static public void Trigger(MMChannelData channelData, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
			bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false)
		{
			OnEvent?.Invoke(channelData, spawnPosition, value, direction, intensity, forceLifetime, lifetime, forceColor, animateColorGradient, useUnscaledTime);
		} 
	}
    #endregion

    /// <summary>
    /// 이 클래스를 사용하면 일반적으로 손상 정보를 표시하기 위해 부동 텍스트를 모으고, 재활용하고 생성할 수 있습니다.
    /// 입력으로 MMFloatingText 개체가 필요합니다.
    /// </summary>
    public class MMFloatingTextSpawner : MMMonoBehaviour
	{
		/// whether to spawn a single prefab or one at random 
		public  enum PoolerModes { Simple, Multiple }
		/// whether the spawned text should have a fixed alignment, orient to match the initial spawn direction, or its movement curve
		public enum AlignmentModes { Fixed, MatchInitialDirection, MatchMovementDirection }

		[MMInspectorGroup("General Settings", true, 10)]
        
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("int 또는 MMChannel 스크립트 가능 개체로 정의된 채널을 수신할지 여부입니다. Int는 설정이 간단하지만 지저분해질 수 있으며 int가 무엇에 해당하는지 기억하기 어렵게 만들 수 있습니다. " +
"MMChannel 스크립트 가능 개체를 미리 생성해야 하지만 읽기 쉬운 이름이 제공되고 확장성이 더 뛰어납니다.")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("들을 채널 - 피드백에 있는 채널과 일치해야 합니다.")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("이벤트를 수신하는 데 사용할 MMChannel 정의 자산입니다. 이 셰이커를 대상으로 하는 피드백은 이벤트를 수신하기 위해 동일한 MMChannel 정의를 참조해야 합니다. MMChannel을 생성하려면 " +
"프로젝트(일반적으로 Data 폴더)의 아무 곳이나 마우스 오른쪽 버튼으로 클릭하고 MoreMountains > MMChannel로 이동한 다음 고유한 이름으로 이름을 지정합니다.")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// whether or not this spawner can spawn at this time
		[Tooltip("이 생성기가 현재 생성될 수 있는지 여부")]
		public bool CanSpawn = true;
		/// whether or not this spawner should spawn objects on unscaled time
		[Tooltip("이 생성자가 크기 조정되지 않은 시간에 객체를 생성해야 하는지 여부")]
		public bool UseUnscaledTime = false;
        
		[MMInspectorGroup("Pooler", true, 24)]

		/// the selected pooler mode (single prefab or multiple ones)
		[Tooltip("선택한 풀러 모드(단일 조립식 또는 다중 조립식)")]
		public PoolerModes PoolerMode = PoolerModes.Simple;
		/// the prefab to spawn (ignored if in multiple mode)
		[Tooltip("생성할 프리팹(다중 모드인 경우 무시됨)")]
		public MMFloatingText PooledSimpleMMFloatingText;
		/// the prefabs to spawn (ignored if in simple mode)
		[Tooltip("스폰할 프리팹(간단 모드에서는 무시됨)")]
		public List<MMFloatingText> PooledMultipleMMFloatingText;
		/// the amount of objects to pool to avoid having to instantiate them at runtime. Should be bigger than the max amount of texts you plan on having on screen at any given moment
		[Tooltip("런타임에 인스턴스화할 필요가 없도록 풀링할 개체의 양. 특정 순간에 화면에 표시할 최대 텍스트 양보다 커야 합니다.")]
		public int PoolSize = 20;
		/// whether or not to nest the waiting pools
		[Tooltip("대기 풀을 중첩할지 여부")]
		public bool NestWaitingPool = true;
		/// whether or not to mutualize the waiting pools
		[Tooltip("대기 풀을 상호화할지 여부")]
		public bool MutualizeWaitingPools = true;
		/// whether or not the text pool can expand if the pool is empty
		[Tooltip("텍스트 풀이 비어 있으면 텍스트 풀을 확장할 수 있는지 여부")]
		public bool PoolCanExpand = true;

		[MMInspectorGroup("Spawn Settings", true, 14)]

		/// the random min and max lifetime duration for the spawned texts (in seconds)
		[Tooltip("생성된 텍스트의 임의 최소 및 최대 수명 기간(초)")]
		[MMVector("Min", "Max")] 
		public Vector2 Lifetime = Vector2.one;
        
		[Header("Spawn Position Offset")]
		/// the random min position at which to spawn the text, relative to its intended spawn position
		[Tooltip("의도한 생성 위치를 기준으로 텍스트를 생성할 임의의 최소 위치")]
		public Vector3 SpawnOffsetMin = Vector3.zero;
		/// the random max position at which to spawn the text, relative to its intended spawn position
		[Tooltip("의도한 생성 위치를 기준으로 텍스트를 생성할 임의의 최대 위치")]
		public Vector3 SpawnOffsetMax = Vector3.zero;

		[MMInspectorGroup("Animate Position", true, 15)] 
        
		[Header("Movement")]

		/// whether or not to animate the movement of spawned texts
		[Tooltip("생성된 텍스트의 움직임에 애니메이션을 적용할지 여부s")]
		public bool AnimateMovement = true;
		/// whether or not to animate the X movement of spawned texts
		[Tooltip("생성된 텍스트의 X 움직임에 애니메이션을 적용할지 여부")]
		public bool AnimateX = false;
		/// the value to which the x movement curve's zero should be remapped to
		[Tooltip("x 이동 곡선의 0이 다시 매핑되어야 하는 값")]
		[MMCondition("AnimateX", true)] 
		public Vector2 RemapXZero = Vector2.zero;
		/// the value to which the x movement curve's one should be remapped to
		[Tooltip("x 이동 곡선의 값을 다시 매핑해야 하는 값")]
		[MMCondition("AnimateX", true)] 
		public Vector2 RemapXOne = Vector2.one;
		/// the curve on which to animate the x movement
		[Tooltip("x 움직임을 애니메이션화할 곡선")]
		[MMCondition("AnimateX", true)]
		public AnimationCurve AnimateXCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		/// whether or not to animate the Y movement of spawned texts
		[Tooltip("생성된 텍스트의 Y 움직임에 애니메이션을 적용할지 여부")]
		public bool AnimateY = true;
		/// the value to which the y movement curve's zero should be remapped to
		[Tooltip("y 이동 곡선의 0이 다시 매핑되어야 하는 값")]
		[MMCondition("AnimateY", true)] 
		public Vector2 RemapYZero = Vector2.zero;
		/// the value to which the y movement curve's one should be remapped to
		[Tooltip("y 이동 곡선의 값을 다시 매핑해야 하는 값")]
		[MMCondition("AnimateY", true)]
		public Vector2 RemapYOne = new Vector2(5f, 5f);
		/// the curve on which to animate the y movement
		[Tooltip("y 움직임을 애니메이션화할 곡선")]
		[MMCondition("AnimateY", true)]
		public AnimationCurve AnimateYCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		/// whether or not to animate the Z movement of spawned texts
		[Tooltip("생성된 텍스트의 Z 이동에 애니메이션을 적용할지 여부")]
		public bool AnimateZ = false;
		/// the value to which the z movement curve's zero should be remapped to
		[Tooltip("z 이동 곡선의 0이 다시 매핑되어야 하는 값")]
		[MMCondition("AnimateZ", true)] 
		public Vector2 RemapZZero = Vector2.zero;
		/// the value to which the z movement curve's one should be remapped to
		[Tooltip("z 이동 곡선의 이동 곡선을 다시 매핑해야 하는 값")]
		[MMCondition("AnimateZ", true)] 
		public Vector2 RemapZOne = Vector2.one;
		/// the curve on which to animate the z movement
		[Tooltip("z 움직임을 애니메이션화할 곡선")]
		[MMCondition("AnimateZ", true)]
		public AnimationCurve AnimateZCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        
		[MMInspectorGroup("Facing Directions", true, 16)]
        
		[Header("Alignment")]

		/// the selected alignment mode (whether the spawned text should have a fixed alignment, orient to match the initial spawn direction, or its movement curve)
		[Tooltip("선택한 정렬 모드(생성된 텍스트가 고정 정렬을 가져야 하는지, 초기 생성 방향과 일치하도록 방향을 지정해야 하는지 또는 해당 이동 곡선)")]
		public AlignmentModes AlignmentMode = AlignmentModes.Fixed;
		/// when in fixed mode, the direction in which to keep the spawned texts
		[Tooltip("고정 모드에서 생성된 텍스트를 유지할 방향")]
		[MMEnumCondition("AlignmentMode", (int)AlignmentModes.Fixed)]
		public Vector3 FixedAlignment = Vector3.up;

		[Header("Billboard")]

		/// whether or not spawned texts should always face the camera
		[Tooltip("생성된 텍스트가 항상 카메라를 향해야 하는지 여부")]
		public bool AlwaysFaceCamera;
		/// whether or not this spawner should automatically grab the main camera on start
		[Tooltip("이 생성자가 시작 시 자동으로 기본 카메라를 잡아야 하는지 여부")]
		[MMCondition("AlwaysFaceCamera", true)]
		public bool AutoGrabMainCameraOnStart = true;
		/// if not in auto grab mode, the camera to use for billboards
		[Tooltip("자동 잡기 모드가 아닌 경우 광고판에 사용할 카메라")]
		[MMCondition("AlwaysFaceCamera", true)]
		public Camera TargetCamera;
                
		[MMInspectorGroup("Animate Scale", true, 46)]

		/// whether or not to animate the scale of spawned texts
		[Tooltip("생성된 텍스트의 크기에 애니메이션을 적용할지 여부")]
		public bool AnimateScale = true;
		/// the value to which the scale curve's zero should be remapped to
		[Tooltip("스케일 곡선의 0이 다시 매핑되어야 하는 값")]
		[MMCondition("AnimateScale", true)]
		public Vector2 RemapScaleZero = Vector2.zero;
		/// the value to which the scale curve's one should be remapped to
		[Tooltip("스케일 곡선의 값을 다시 매핑해야 하는 값")]
		[MMCondition("AnimateScale", true)]
		public Vector2 RemapScaleOne = Vector2.one;
		/// the curve on which to animate the scale
		[Tooltip("스케일을 애니메이션화할 곡선")]
		[MMCondition("AnimateScale", true)]
		public AnimationCurve AnimateScaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 1f), new Keyframe(0.85f, 1f), new Keyframe(1f, 0f));
        
		[MMInspectorGroup("Animate Color", true, 55)]

		/// whether or not to animate the spawned text's color over time
		[Tooltip("시간이 지남에 따라 생성된 텍스트의 색상에 애니메이션을 적용할지 여부")]
		public bool AnimateColor = false;
		/// the gradient over which to animate the spawned text's color over time
		[Tooltip("시간이 지남에 따라 생성된 텍스트의 색상에 애니메이션을 적용할 그라데이션")]
		[GradientUsage(true)]
		public Gradient AnimateColorGradient = new Gradient();

		[MMInspectorGroup("Animate Opacity", true, 45)]

		/// whether or not to animate the opacity of the spawned texts
		[Tooltip("생성된 텍스트의 불투명도에 애니메이션을 적용할지 여부")]
		public bool AnimateOpacity = true;
		/// the value to which the opacity curve's zero should be remapped to
		[Tooltip("불투명도 곡선의 0이 다시 매핑되어야 하는 값")]
		[MMCondition("AnimateOpacity", true)]
		public Vector2 RemapOpacityZero = Vector2.zero;
		/// the value to which the opacity curve's one should be remapped to
		[Tooltip("불투명도 곡선의 값을 다시 매핑해야 하는 값")]
		[MMCondition("AnimateOpacity", true)]
		public Vector2 RemapOpacityOne = Vector2.one;
		/// the curve on which to animate the opacity
		[Tooltip("불투명도에 애니메이션을 적용할 곡선")]
		[MMCondition("AnimateOpacity", true)]
		public AnimationCurve AnimateOpacityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.2f, 1f), new Keyframe(0.8f, 1f), new Keyframe(1f, 0f));

		[MMInspectorGroup("Intensity Multipliers", true, 45)]

		/// whether or not the intensity multiplier should impact lifetime
		[Tooltip("강도 승수가 수명에 영향을 미치는지 여부")]
		public bool IntensityImpactsLifetime = false;
		/// when getting an intensity multiplier, the value by which to multiply the lifetime
		[Tooltip("강도 승수를 얻을 때 수명에 곱할 값")]
		[MMCondition("IntensityImpactsLifetime", true)]
		public float IntensityLifetimeMultiplier = 1f;
		/// whether or not the intensity multiplier should impact movement
		[Tooltip("강도 승수가 움직임에 영향을 미치는지 여부")]
		public bool IntensityImpactsMovement = false;
		/// when getting an intensity multiplier, the value by which to multiply the movement values
		[Tooltip("강도 승수를 얻을 때 이동 값에 곱할 값")]
		[MMCondition("IntensityImpactsMovement", true)]
		public float IntensityMovementMultiplier = 1f;
		/// whether or not the intensity multiplier should impact scale
		[Tooltip("강도 승수가 규모에 영향을 미치는지 여부")]
		public bool IntensityImpactsScale = false;
		/// when getting an intensity multiplier, the value by which to multiply the scale values
		[Tooltip("강도 승수를 얻을 때 스케일 값에 곱할 값")]
		[MMCondition("IntensityImpactsScale", true)]
		public float IntensityScaleMultiplier = 1f;

		[MMInspectorGroup("Debug", true, 12)]

		/// a random value to display when pressing the TestSpawnOne button
		[Tooltip("TestSpawnOne 버튼을 누를 때 표시할 임의의 값")]
		public Vector2Int DebugRandomValue = new Vector2Int(100, 500);
		/// the min and max bounds within which to pick a value to output when pressing the TestSpawnMany button
		[Tooltip("TestSpawnMany 버튼을 눌렀을 때 출력할 값을 선택하는 최소 및 최대 경계")]
		[MMVector("Min", "Max")] 
		public Vector2 DebugInterval = new Vector2(0.3f, 0.5f);
		/// a button used to test the spawn of one text
		[Tooltip("하나의 텍스트 생성을 테스트하는 데 사용되는 버튼")]
		[MMInspectorButton("TestSpawnOne")]
		public bool TestSpawnOneBtn;
		/// a button used to start/stop the spawn of texts at regular intervals
		[Tooltip("일정한 간격으로 텍스트 생성을 시작/중지하는 데 사용되는 버튼")]
		[MMInspectorButton("TestSpawnMany")]
		public bool TestSpawnManyBtn;
        
		protected MMObjectPooler _pooler;
		protected MMFloatingText _floatingText;
		protected Coroutine _testSpawnCoroutine;
        
		protected float _lifetime;
		protected float _speed;
		protected Vector3 _spawnOffset;
		protected Vector3 _direction;
		protected Gradient _colorGradient;
		protected bool _animateColor;

		#region Initialization

		/// <summary>
		/// On awake we initialize our spawner
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init, we instantiate our object pool and grab the main camera
		/// </summary>
		protected virtual void Initialization()
		{
			InstantiateObjectPool();
			GrabMainCamera();
		}

		/// <summary>
		/// Instantiates the specified type of object pool
		/// </summary>
		protected virtual void InstantiateObjectPool()
		{
			if (_pooler == null)
			{
				if (PoolerMode == PoolerModes.Simple)
				{
					InstantiateSimplePool();
				}
				else
				{
					InstantiateMultiplePool();
				}
			}
		}

		/// <summary>
		/// Instantiates a simple object pooler and sets it up
		/// </summary>
		protected virtual void InstantiateSimplePool()
		{
			if (PooledSimpleMMFloatingText == null)
			{
				Debug.LogError(this.name + " : no PooledSimpleMMFloatingText prefab has been set.");
				return;
			}
			GameObject newPooler = new GameObject();
			SceneManager.MoveGameObjectToScene(newPooler, this.gameObject.scene);
			newPooler.name = PooledSimpleMMFloatingText.name + "_Pooler";
			newPooler.transform.SetParent(this.transform);
			MMSimpleObjectPooler simplePooler = newPooler.AddComponent<MMSimpleObjectPooler>();
			simplePooler.PoolSize = PoolSize;
			simplePooler.GameObjectToPool = PooledSimpleMMFloatingText.gameObject;
			simplePooler.NestWaitingPool = NestWaitingPool;
			simplePooler.MutualizeWaitingPools = MutualizeWaitingPools;
			simplePooler.PoolCanExpand = PoolCanExpand;
			simplePooler.FillObjectPool();
			_pooler = simplePooler;
		}

		/// <summary>
		/// Instantiates a multiple object pooler and sets it up
		/// </summary>
		protected virtual void InstantiateMultiplePool()
		{
			GameObject newPooler = new GameObject();
			SceneManager.MoveGameObjectToScene(newPooler, this.gameObject.scene);
			newPooler.name = this.name + "_Pooler";
			newPooler.transform.SetParent(this.transform);
			MMMultipleObjectPooler multiplePooler = newPooler.AddComponent<MMMultipleObjectPooler>();
			multiplePooler.Pool = new List<MMMultipleObjectPoolerObject>();
			foreach (MMFloatingText obj in PooledMultipleMMFloatingText)
			{
				MMMultipleObjectPoolerObject item = new MMMultipleObjectPoolerObject();
				item.GameObjectToPool = obj.gameObject;
				item.PoolCanExpand = PoolCanExpand;
				item.PoolSize = PoolSize;
				item.Enabled = true;
				multiplePooler.Pool.Add(item);
			}
			multiplePooler.NestWaitingPool = NestWaitingPool;
			multiplePooler.MutualizeWaitingPools = MutualizeWaitingPools;
			multiplePooler.FillObjectPool();
			_pooler = multiplePooler;
		}

		/// <summary>
		/// Grabs the main camera if needed
		/// </summary>
		protected virtual void GrabMainCamera()
		{
			if (AutoGrabMainCameraOnStart)
			{
				TargetCamera = Camera.main;
			}
		}

		#endregion
        
		/// <summary>
		/// Spawns a new floating text
		/// </summary>
		/// <param name="value"></param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="intensity"></param>
		/// <param name="forceLifetime"></param>
		/// <param name="lifetime"></param>
		/// <param name="forceColor"></param>
		/// <param name="animateColorGradient"></param>
		protected virtual void Spawn(string value, Vector3 position, Vector3 direction, float intensity = 1f,
			bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null)
		{
			if (!CanSpawn)
			{
				return;
			}

			_direction = (direction != Vector3.zero) ? direction + this.transform.up : this.transform.up;

			this.transform.position = position;

			GameObject nextGameObject = _pooler.GetPooledGameObject();

			float lifetimeMultiplier = IntensityImpactsLifetime ? intensity * IntensityLifetimeMultiplier : 1f;
			float movementMultiplier = IntensityImpactsMovement ? intensity * IntensityMovementMultiplier : 1f;
			float scaleMultiplier = IntensityImpactsScale ? intensity * IntensityScaleMultiplier : 1f;

			_lifetime = UnityEngine.Random.Range(Lifetime.x, Lifetime.y) * lifetimeMultiplier;
			_spawnOffset = MMMaths.RandomVector3(SpawnOffsetMin, SpawnOffsetMax);
			_animateColor = AnimateColor;
			_colorGradient = AnimateColorGradient;

			float remapXZero = UnityEngine.Random.Range(RemapXZero.x, RemapXZero.y);
			float remapXOne = UnityEngine.Random.Range(RemapXOne.x, RemapXOne.y) * movementMultiplier;
			float remapYZero = UnityEngine.Random.Range(RemapYZero.x, RemapYZero.y);
			float remapYOne = UnityEngine.Random.Range(RemapYOne.x, RemapYOne.y) * movementMultiplier;
			float remapZZero = UnityEngine.Random.Range(RemapZZero.x, RemapZZero.y);
			float remapZOne = UnityEngine.Random.Range(RemapZOne.x, RemapZOne.y) * movementMultiplier;
			float remapOpacityZero = UnityEngine.Random.Range(RemapOpacityZero.x, RemapOpacityZero.y);
			float remapOpacityOne = UnityEngine.Random.Range(RemapOpacityOne.x, RemapOpacityOne.y);
			float remapScaleZero = UnityEngine.Random.Range(RemapScaleZero.x, RemapOpacityZero.y);
			float remapScaleOne = UnityEngine.Random.Range(RemapScaleOne.x, RemapScaleOne.y) * scaleMultiplier;

			if (forceLifetime)
			{
				_lifetime = lifetime;
			}

			if (forceColor)
			{
				_animateColor = true;
				_colorGradient = animateColorGradient;
			}

			// mandatory checks
			if (nextGameObject==null) { return; }
            
			// we activate the object
			nextGameObject.gameObject.SetActive(true);
			nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();

			// we position the object
			nextGameObject.transform.position = this.transform.position + _spawnOffset;

			_floatingText = nextGameObject.MMGetComponentNoAlloc<MMFloatingText>();
			_floatingText.SetUseUnscaledTime(UseUnscaledTime, true);
			_floatingText.ResetPosition();
			_floatingText.SetProperties(value, _lifetime, _direction, AnimateMovement, 
				AlignmentMode, FixedAlignment, AlwaysFaceCamera, TargetCamera,
				AnimateX, AnimateXCurve, remapXZero, remapXOne,
				AnimateY, AnimateYCurve, remapYZero, remapYOne,
				AnimateZ, AnimateZCurve, remapZZero, remapZOne,
				AnimateOpacity, AnimateOpacityCurve, remapOpacityZero, remapOpacityOne,
				AnimateScale, AnimateScaleCurve, remapScaleZero, remapScaleOne,
				_animateColor, _colorGradient);            
		}

		/// <summary>
		/// When we get a floating text event on this spawner's Channel, we spawn a new floating text
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="spawnPosition"></param>
		/// <param name="value"></param>
		/// <param name="direction"></param>
		/// <param name="intensity"></param>
		/// <param name="forceLifetime"></param>
		/// <param name="lifetime"></param>
		/// <param name="forceColor"></param>
		/// <param name="animateColorGradient"></param>
		public virtual void OnMMFloatingTextSpawnEvent(MMChannelData channelData, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
			bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}

			UseUnscaledTime = useUnscaledTime;
			Spawn(value, spawnPosition, direction, intensity, forceLifetime, lifetime, forceColor, animateColorGradient);
		}
    
		/// <summary>
		/// On enable we start listening for floating text events
		/// </summary>
		protected virtual void OnEnable()
		{
			MMFloatingTextSpawnEvent.Register(OnMMFloatingTextSpawnEvent);
		}
    
		/// <summary>
		/// On disable we stop listening for floating text events
		/// </summary>
		protected virtual void OnDisable()
		{
			MMFloatingTextSpawnEvent.Unregister(OnMMFloatingTextSpawnEvent);
		}

		// Test methods ----------------------------------------------------------------------------------------

		#region TestMethods

		/// <summary>
		/// A test method that spawns one floating text
		/// </summary>
		protected virtual void TestSpawnOne()
		{
			string test = UnityEngine.Random.Range(DebugRandomValue.x, DebugRandomValue.y).ToString();
			Spawn(test, this.transform.position, Vector3.zero);
		}

		/// <summary>
		/// A method used to start/stop the regular spawning of debug floating texts 
		/// </summary>
		protected virtual void TestSpawnMany()
		{
			if (_testSpawnCoroutine == null)
			{
				_testSpawnCoroutine = StartCoroutine(TestSpawnManyCo());    
			}
			else
			{
				StopCoroutine(_testSpawnCoroutine);
				_testSpawnCoroutine = null;
			}
		}

		/// <summary>
		/// A coroutine used to spawn debug floating texts until stopped 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator TestSpawnManyCo()
		{
			float lastSpawnAt = Time.time;
			float interval = UnityEngine.Random.Range(DebugInterval.x, DebugInterval.y);
			while (true)
			{
				if (Time.time - lastSpawnAt > interval)
				{
					TestSpawnOne();
					lastSpawnAt = Time.time;
					interval = UnityEngine.Random.Range(DebugInterval.x, DebugInterval.y);
				}
				yield return null;
			}
		}
        
		#endregion
	}
}
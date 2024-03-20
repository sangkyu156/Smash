using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using System;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	public struct MMFlashEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(Color flashColor, float duration, float alpha, int flashID, MMChannelData channelData, TimescaleModes timescaleMode, bool stop = false);

		static public void Trigger(Color flashColor, float duration, float alpha, int flashID, MMChannelData channelData, TimescaleModes timescaleMode, bool stop = false)
		{
			OnEvent?.Invoke(flashColor, duration, alpha, flashID, channelData, timescaleMode, stop);
		}
	}

	[Serializable]
	public class MMFlashDebugSettings
	{
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("int 또는 MMChannel 스크립트 가능 개체로 정의된 채널을 수신할지 여부입니다. Int는 설정이 간단하지만 지저분해질 수 있으며 int가 무엇에 해당하는지 기억하기 어렵게 만들 수 있습니다. " +
"MMChannel 스크립트 가능 개체를 미리 생성해야 하지만 읽기 쉬운 이름이 제공되고 확장성이 더 뛰어납니다.")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("들을 채널 - 피드백에 있는 채널과 일치해야 합니다.")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("이벤트를 수신하는 데 사용할 MMChannel 정의 자산입니다. 이 셰이커를 대상으로 하는 피드백은 이벤트를 수신하기 위해 동일한 MMChannel 정의를 참조해야 합니다. MMChannel을 생성하려면 " +
"프로젝트(일반적으로 Data 폴더)의 아무 곳이나 마우스 오른쪽 버튼으로 클릭하고 MoreMountains > MMChannel로 이동한 다음 고유한 이름으로 이름을 지정합니다.")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// the color of the flash
		public Color FlashColor = Color.white;
		/// the flash duration (in seconds)
		public float FlashDuration = 0.2f;
		/// the alpha of the flash
		public float FlashAlpha = 1f;
		/// the ID of the flash (usually 0). You can specify on each MMFlash object an ID, allowing you to have different flash images in one scene and call them separately (one for damage, one for health pickups, etc)
		public int FlashID = 0;
	}
    
	[RequireComponent(typeof(Image))]
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MMFlash")]
    /// <summary>
    /// 이 클래스를 이미지에 추가하면 MMFlashEvent를 받을 때 깜박입니다.
    /// </summary>
    public class MMFlash : MonoBehaviour
	{
		[Header("Flash")]
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("int 또는 MMChannel 스크립트 가능 개체로 정의된 채널을 수신할지 여부입니다. Int는 설정이 간단하지만 지저분해질 수 있으며 int가 무엇에 해당하는지 기억하기 어렵게 만들 수 있습니다. " +
"MMChannel 스크립트 가능 개체를 미리 생성해야 하지만 읽기 쉬운 이름이 제공되고 확장성이 더 뛰어납니다.")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("들을 채널 - 피드백에 있는 채널과 일치해야 합니다.")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("이벤트를 수신하는 데 사용할 MMChannel 정의 자산입니다. 이 셰이커를 대상으로 하는 피드백은 이벤트를 수신하기 위해 동일한 MMChannel 정의를 참조해야 합니다. MMChannel을 생성하려면 " +
"프로젝트(일반적으로 Data 폴더)의 아무 곳이나 마우스 오른쪽 버튼으로 클릭하고 MoreMountains > MMChannel로 이동한 다음 고유한 이름으로 이름을 지정합니다.")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// the ID of this MMFlash object. When triggering a MMFlashEvent you can specify an ID, and only MMFlash objects with this ID will answer the call and flash, allowing you to have more than one flash object in a scene
		[Tooltip("이 MMFlash 객체의 ID입니다. MMFlashEvent를 트리거할 때 ID를 지정할 수 있으며 이 ID가 있는 MMFlash 개체만 호출에 응답하고 플래시하므로 장면에 두 개 이상의 플래시 개체가 있을 수 있습니다.")]
		public int FlashID = 0;
		/// if this is true, the MMFlash will stop before playing on every new event received
		[Tooltip("이것이 사실이라면 MMFlash는 수신된 모든 새 이벤트를 재생하기 전에 중지됩니다.")]
		public bool Interruptable = false;
		
		[Header("Interpolation")]
		/// the animation curve to use when flashing in
		[Tooltip("플래싱할 때 사용할 애니메이션 곡선")]
		public MMTweenType FlashInTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);
		/// the animation curve to use when flashing out
		[Tooltip("플래시 아웃할 때 사용할 애니메이션 곡선")]
		public MMTweenType FlashOutTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);

		[Header("Debug")]
		/// the set of test settings to use when pressing the DebugTest button
		[Tooltip("DebugTest 버튼을 누를 때 사용할 테스트 설정 세트")]
		public MMFlashDebugSettings DebugSettings;
		/// a test button that calls the DebugTest method
		[Tooltip("DebugTest 메서드를 호출하는 테스트 버튼")]
		[MMFInspectorButton("DebugTest")]
		public bool DebugTestButton;

		public virtual float GetTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		protected Image _image;
		protected CanvasGroup _canvasGroup;
		protected bool _flashing = false;
		protected float _targetAlpha;
		protected Color _initialColor;
		protected float _delta;
		protected float _flashStartedTimestamp;
		protected int _direction = 1;
		protected float _duration;
		protected TimescaleModes _timescaleMode;
		protected MMTweenType _currentTween;

		/// <summary>
		/// On start we grab our image component
		/// </summary>
		protected virtual void Start()
		{
			_image = GetComponent<Image>();
			_canvasGroup = GetComponent<CanvasGroup>();
			_initialColor = _image.color;
		}

		/// <summary>
		/// On update we flash our image if needed
		/// </summary>
		protected virtual void Update()
		{
			if (_flashing)
			{
				_image.enabled = true;

				_currentTween = FlashInTween;
				if (GetTime() - _flashStartedTimestamp > _duration / 2f)
				{
					_direction = -1;
					_currentTween = FlashOutTween;
				}
				
				if (_direction == 1)
				{
					_delta += GetDeltaTime() / (_duration / 2f);
				}
				else
				{
					_delta -= GetDeltaTime() / (_duration / 2f);
				}
                
				if (GetTime() - _flashStartedTimestamp > _duration)
				{
					_flashing = false;
				}
				
				float percent = MMMaths.Remap(_delta, 0f, _duration/2f, 0f, 1f);
				float tweenValue = _currentTween.Evaluate(percent);

				_canvasGroup.alpha = Mathf.Lerp(0f, _targetAlpha, tweenValue);
			}
			else
			{
				_image.enabled = false;
			}
		}

		public virtual void DebugTest()
		{
			MMFlashEvent.Trigger(DebugSettings.FlashColor, DebugSettings.FlashDuration, DebugSettings.FlashAlpha, DebugSettings.FlashID, new MMChannelData(DebugSettings.ChannelMode, DebugSettings.Channel, DebugSettings.MMChannelDefinition), TimescaleModes.Unscaled);
		}

		/// <summary>
		/// When getting a flash event, we turn our image on
		/// </summary>
		public virtual void OnMMFlashEvent(Color flashColor, float duration, float alpha, int flashID, MMChannelData channelData, TimescaleModes timescaleMode, bool stop = false)
		{
			if (flashID != FlashID) 
			{
				return;
			}
            
			if (stop)
			{
				_flashing = false;
				return;
			}

			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}

			if (_flashing && Interruptable)
			{
				_flashing = false;
			}

			if (!_flashing)
			{
				_flashing = true;
				_direction = 1;
				_canvasGroup.alpha = 0;
				_targetAlpha = alpha;
				_delta = 0f;
				_image.color = flashColor;
				_duration = duration;
				_timescaleMode = timescaleMode;
				_flashStartedTimestamp = GetTime();
			}
		} 

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			MMFlashEvent.Register(OnMMFlashEvent);
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			MMFlashEvent.Unregister(OnMMFlashEvent);
		}		
	}
}
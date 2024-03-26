using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using System.Linq;
using MoreMountains.Tools;
using UnityEditor.Experimental;
using UnityEngine.Events;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 모두 재생되도록 만들어진 MMFeedback 모음입니다.
    /// 이 클래스는 피드백을 추가 및 사용자 정의하는 사용자 정의 검사기와 이를 트리거하고 중지하는 등의 공개 메소드를 제공합니다.
    /// 자체적으로 사용하거나 다른 클래스에서 바인딩하여 거기에서 트리거할 수 있습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/MMFeedbacks")]
	public class MMFeedbacks : MonoBehaviour
	{
        /// MMFeedback이 재생될 수 있는 가능한 방향
        public enum Directions { TopToBottom, BottomToTop }
        /// 가능한 안전 모드(직렬화 오류로 인해 손상되지 않았는지 확인하기 위해 검사를 수행함)
        /// - nope : 안전하지 않다
        /// - editor only : 활성화 확인을 수행합니다.
        /// - runtime only : Awake에 대한 검사를 수행합니다.
        /// - full : 편집기와 런타임 검사를 모두 수행합니다. 권장 설정
        public enum SafeModes { Nope, EditorOnly, RuntimeOnly, Full }

        /// 트리거할 MMFeedback 목록
        public List<MMFeedback> Feedbacks = new List<MMFeedback>();

		/// 가능한 초기화 모드. 스크립트를 사용하는 경우 초기화 메서드를 호출하고 소유자를 전달하여 수동으로 초기화해야 합니다.
        /// 그렇지 않으면 이 구성 요소가 Awake 또는 Start에서 자체적으로 초기화되도록 할 수 있으며 이 경우 소유자는 MMFeedbacks 자체가 됩니다.
        public enum InitializationModes { Script, Awake, Start }
        /// 선택한 초기화 모드
        [Tooltip("선택한 초기화 모드. 스크립트를 사용하는 경우 다음을 호출하여 수동으로 초기화해야 합니다. " +
                 "초기화 방법 및 소유자 전달 그렇지 않으면 이 구성 요소를 초기화할 수 있습니다. " +
                 "Awake 또는 Start에서 자체적으로, 이 경우 소유자는 MMFeedbacks 자체가 됩니다.")]
		public InitializationModes InitializationMode = InitializationModes.Start;
        /// 이를 true로 설정하면 시스템은 재생 전에 항상 초기화가 발생하도록 변경합니다.
        [Tooltip("이를 true로 설정하면 시스템은 재생 전에 항상 초기화가 발생하도록 변경합니다.")]
		public bool AutoInitialization = true;
        /// 선택한 안전 모드
        [Tooltip("선택한 안전 모드")]
		public SafeModes SafeMode = SafeModes.Full;
        /// 선택한 방향
        [Tooltip("이러한 피드백이 재생되어야 하는 선택된 방향")]
		public Directions Direction = Directions.TopToBottom;
        /// 모든 피드백이 재생되었을 때 이 MMFeedback이 방향을 반전해야 하는지 여부
        [Tooltip("모든 피드백이 재생되었을 때 이 MMFeedback이 방향을 반전해야 하는지 여부")]
		public bool AutoChangeDirectionOnEnd = false;
        /// 시작 시 이 피드백을 자동으로 재생할지 여부
        [Tooltip("시작 시 이 피드백을 자동으로 재생할지 여부")]
		public bool AutoPlayOnStart = false;
        /// 활성화 시 이 피드백을 자동으로 재생할지 여부
        [Tooltip("활성화 시 이 피드백을 자동으로 재생할지 여부")]
		public bool AutoPlayOnEnable = false;

        /// 이것이 사실이라면 해당 플레이어 내의 모든 피드백은 개별 설정에 관계없이 지정된 ForcedTimescaleMode에서 작동합니다. 
        [Tooltip("이것이 사실이라면 해당 플레이어 내의 모든 피드백은 개별 설정에 관계없이 지정된 ForcedTimescaleMode에서 작동합니다.")] 
		public bool ForceTimescaleMode = false;
        /// ForceTimescaleMode가 true인 경우 이 플레이어에 대한 모든 피드백이 작동해야 하는 시간 척도 모드입니다.
        [Tooltip("ForceTimescaleMode가 true인 경우 이 플레이어에 대한 모든 피드백이 작동해야 하는 시간 척도 모드입니다.")] 
		[MMFCondition("ForceTimescaleMode", true)]
		public TimescaleModes ForcedTimescaleMode = TimescaleModes.Unscaled;
        /// 모든 피드백 기간(초기 지연, 기간, 반복 간 지연...)에 적용되는 시간 승수
        [Tooltip("모든 피드백 기간(초기 지연, 기간, 반복 간 지연...)에 적용되는 시간 승수")]
		public float DurationMultiplier = 1f;
        /// 이것이 사실이라면 RandomDurationMultiplier가 노출됩니다. 각 피드백의 최종 지속 시간은 다음과 같습니다: 기본 지속 시간 * DurationMultiplier * RandomDurationMultiplier.x와 RandomDurationMultiplier.y 사이의 임의 값
        [Tooltip("이것이 사실이라면 RandomDurationMultiplier가 노출됩니다. 각 피드백의 최종 지속 시간은 다음과 같습니다: 기본 지속 시간 * DurationMultiplier * RandomDurationMultiplier.x와 RandomDurationMultiplier.y 사이의 임의 값")]
		public bool RandomizeDuration = false;
        /// RandomizeDuration이 true인 경우 무작위 기간 승수의 최소(x) 및 최대(y) 값
        [Tooltip("RandomizeDuration이 true인 경우 무작위 기간 승수의 최소(x) 및 최대(y) 값")]
		[MMCondition("RandomizeDuration", true)]
		public Vector2 RandomDurationMultiplier = new Vector2(0.5f, 1.5f);
        /// 이것이 사실이라면 더 많은 편집자 전용 세부 정보가 기간 슬롯의 피드백별로 표시됩니다.
        [Tooltip("이것이 사실이라면 더 많은 편집자 전용 세부 정보가 기간 슬롯의 피드백별로 표시됩니다.")]
		public bool DisplayFullDurationDetails = false;
        /// 플레이어 자체가 작동하는 시간 척도. 이는 특히 순서에 영향을 미치고 기간 평가를 일시 중지합니다.
        [Tooltip("플레이어 자체가 작동하는 시간 척도. 이는 특히 순서에 영향을 미치고 기간 평가를 일시 중지합니다.")]
		public TimescaleModes PlayerTimescaleMode = TimescaleModes.Unscaled;

        /// 이것이 사실이라면 이 피드백은 RangeCenter까지의 거리가 RangeDistance보다 낮거나 같은 경우에만 재생됩니다.
        [Tooltip("이것이 사실이라면 이 피드백은 RangeCenter까지의 거리가 RangeDistance보다 낮거나 같은 경우에만 재생됩니다.")]
		public bool OnlyPlayIfWithinRange = false;
        /// OnlyPlayIfWithinRange 모드에서 범위의 중심으로 간주되는 변환
        [Tooltip("OnlyPlayIfWithinRange 모드에서 범위의 중심으로 간주되는 변환")]
		public Transform RangeCenter;
        /// OnlyPlayIfWithinRange 모드에서 피드백이 재생되는 중심까지의 거리
        [Tooltip("OnlyPlayIfWithinRange 모드에서 피드백이 재생되는 중심까지의 거리")]
		public float RangeDistance = 5f;
        /// OnlyPlayIfWithinRange 모드에서 RangeFallOff 곡선을 기반으로 피드백 강도를 수정할지 여부  
        [Tooltip("OnlyPlayIfWithinRange 모드에서 RangeFallOff 곡선을 기반으로 피드백 강도를 수정할지 여부")]
		public bool UseRangeFalloff = false;
        /// 폴오프를 정의하는 데 사용할 애니메이션 곡선(x에서 0은 범위 중심을 나타내고 1은 최대 거리를 나타냄)
        [Tooltip("폴오프를 정의하는 데 사용할 애니메이션 곡선(x에서 0은 범위 중심을 나타내고 1은 최대 거리를 나타냄)")]
		[MMFCondition("UseRangeFalloff", true)]
		public AnimationCurve RangeFalloff = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        /// 폴오프 곡선의 y축을 다시 매핑하는 값 '0과 1'
        [Tooltip("폴오프 곡선의 y축을 다시 매핑하는 값 '0과 1'")]
		[MMFVector("Zero","One")]
		public Vector2 RemapRangeFalloff = new Vector2(0f, 1f);
        /// 어디서나 RangeCenter를 설정하는 데 사용되는 MMSetFeedbackRangeCenterEvent를 무시할지 여부
        [Tooltip("어디서나 RangeCenter를 설정하는 데 사용되는 MMSetFeedbackRangeCenterEvent를 무시할지 여부")]
		public bool IgnoreRangeEvents = false;

        /// 이 MMFeedbacks가 한 번 재생된 후 새로운 재생을 트리거하는 기간(초)
        [Tooltip("이 MMFeedbacks가 한 번 재생된 후 새로운 재생을 트리거하는 기간(초)")]
		public float CooldownDuration = 0f;
        /// 이 MMFeedbacks의 콘텐츠 재생 시작을 지연하는 기간(초)
        [Tooltip("이 MMFeedbacks의 콘텐츠 재생 시작을 지연하는 기간(초)")]
		public float InitialDelay = 0f;
        /// 이 플레이어를 플레이할 수 있는지 여부. 예를 들어 다른 클래스의 플레이를 일시적으로 방지하는 데 유용합니다.
        [Tooltip("이 플레이어를 플레이할 수 있는지 여부. 예를 들어 다른 클래스의 플레이를 일시적으로 방지하는 데 유용합니다.")]
		public bool CanPlay = true;
        /// 이것이 사실이라면 이 피드백이 이미 재생되는 동안 새 재생을 시작할 수 있습니다. 그렇지 않으면 다음을 수행할 수 없습니다.
        [Tooltip("이것이 사실이라면 이 피드백이 이미 재생되는 동안 새 재생을 시작할 수 있습니다. 그렇지 않으면 다음을 수행할 수 없습니다.")]
		public bool CanPlayWhileAlreadyPlaying = true;
        /// 이 시퀀스가 ​​발생할 확률(퍼센트: 100: 항상 발생, 0: 절대 발생하지 않음, 50: 두 번의 호출마다 한 번 발생 등)
        [Tooltip("이 시퀀스가 ​​발생할 확률(퍼센트: 100: 항상 발생, 0: 절대 발생하지 않음, 50: 두 번의 호출마다 한 번 발생 등)")]
		[Range(0,100)]
		public float ChanceToPlay = 100f;

        /// 이 피드백을 재생할 강도입니다. 해당 값은 대부분의 피드백에서 진폭을 조정하는 데 사용됩니다. 1은 정상, 0.5는 절반 전력, 0은 효과 없음을 나타냅니다.
        /// 이 값이 제어하는 ​​내용은 피드백마다 다르므로 주저하지 말고 코드를 확인하여 정확히 무엇을 하는지 확인하세요.  
        [Tooltip("이 피드백을 재생할 강도입니다. 해당 값은 대부분의 피드백에서 진폭을 조정하는 데 사용됩니다. 1은 정상, 0.5는 절반 전력, 0은 효과 없음을 나타냅니다." +
                 "이 값이 제어하는 ​​내용은 피드백마다 다르므로 주저하지 말고 코드를 확인하여 정확히 무엇을 하는지 확인하세요.")]
		public float FeedbacksIntensity = 1f;

        /// 이 MMFeedback의 다양한 단계에서 트리거될 수 있는 다수의 UnityEvent
        [Tooltip("이 MMFeedback의 다양한 단계에서 트리거될 수 있는 다수의 UnityEvent")] 
		public MMFeedbacksEvents Events;

        /// 모든 피드백을 전체적으로 켜거나 끄는 데 사용되는 전역 스위치
        [Tooltip("모든 피드백을 전체적으로 켜거나 끄는 데 사용되는 전역 스위치")]
		public static bool GlobalMMFeedbacksActive = true;
        
		[HideInInspector]
        /// 이 MMFeedbacks가 디버그 모드인지 여부
        public bool DebugActive = false;
        /// 이 MMFeedbacks가 지금 재생 중인지 여부 - 아직 중지되지 않았음을 의미합니다.
        /// MMFeedback을 중지하지 않으면 물론 그대로 유지됩니다.
        public bool IsPlaying { get; protected set; }
        /// 이 MMFeedbacks가 재생을 시작한 이후의 시간을 재생하는 경우
        public float ElapsedTime => IsPlaying ? GetTime() - _lastStartAt : 0f;
        /// 이 MMFeedback이 재생된 횟수
        public int TimesPlayed { get; protected set; }
        /// 이 MMFeedbacks' 시퀀스의 실행이 금지되고 Resume() 호출을 기다리는지 여부
        public bool InScriptDrivenPause { get; set; }
        /// 이 MMFeedbacks에 루프가 하나 이상 포함되어 있으면 true입니다.
        public bool ContainsLoop { get; set; }
        /// 이 피드백이 다음에 재생될 때 재생 방향을 바꿔야 한다면 true입니다.
        public bool ShouldRevertOnNextPlay { get; set; }
        /// 이 플레이어가 확장되지 않은 모드를 강제하는 경우 true입니다.
        public bool ForcingUnscaledTimescaleMode { get { return (ForceTimescaleMode && ForcedTimescaleMode == TimescaleModes.Unscaled);  } }
        /// 이 MMFeedbacks에 있는 모든 활성 피드백의 총 지속 시간(초)입니다.
        public virtual float TotalDuration
		{
			get
			{
				float total = 0f;
				foreach (MMFeedback feedback in Feedbacks)
				{
					if ((feedback != null) && (feedback.Active))
					{
						if (total < feedback.TotalDuration)
						{
							total = feedback.TotalDuration;    
						}
					}
				}
				return InitialDelay + total;
			}
		}
        
		public virtual float GetTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
		protected float _startTime = 0f;
		protected float _holdingMax = 0f;
		protected float _lastStartAt = -float.MaxValue;
		protected int _lastStartFrame = -1;
		protected bool _pauseFound = false;
		protected float _totalDuration = 0f;
		protected bool _shouldStop = false;
		protected const float _smallValue = 0.001f;
		protected float _randomDurationMultiplier = 1f;
		protected float _lastOnEnableFrame = -1;

        #region INITIALIZATION

        /// <summary>
        /// Awake에서 자동 모드에 있으면 피드백을 초기화합니다.
        /// </summary>
        protected virtual void Awake()
		{
			// if our MMFeedbacks is in AutoPlayOnEnable mode, we add a little helper to it that will re-enable it if needed if the parent game object gets turned off and on again
			if (AutoPlayOnEnable)
			{
				MMFeedbacksEnabler enabler = GetComponent<MMFeedbacksEnabler>(); 
				if (enabler == null)
				{
					enabler = this.gameObject.AddComponent<MMFeedbacksEnabler>();
				}
				enabler.TargetMMFeedbacks = this;
			}
            
			if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
			{
				Initialization(this.gameObject);
			}
			CheckForLoops();
		}

        /// <summary>
        /// 시작 시 자동 모드에 있는 경우 피드백을 초기화합니다.
        /// </summary>
        protected virtual void Start()
		{
			if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
			{
				Initialization(this.gameObject);
			}
			if (AutoPlayOnStart && Application.isPlaying)
			{
				PlayFeedbacks();
			}
			CheckForLoops();
		}

        /// <summary>
        /// 활성화하면 자동 모드에 있는 경우 피드백을 초기화합니다.
        /// </summary>
        protected virtual void OnEnable()
		{
			if (AutoPlayOnEnable && Application.isPlaying)
			{
				PlayFeedbacks();
			}
		}

        /// <summary>
        /// MMFeedbacks를 초기화하고 이 MMFeedbacks를 소유자로 설정합니다.
        /// </summary>
        public virtual void Initialization()
		{
			Initialization(this.gameObject);
		}

        /// <summary>
        /// 피드백에 의한 위치 및 계층 구조에 대한 참조로 사용될 소유자를 지정하여 피드백을 초기화하는 공개 방법
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="feedbacksOwner"></param>
        public virtual void Initialization(GameObject owner)
		{
			if ((SafeMode == MMFeedbacks.SafeModes.RuntimeOnly) || (SafeMode == MMFeedbacks.SafeModes.Full))
			{
				AutoRepair();
			}

			IsPlaying = false;
			TimesPlayed = 0;
			_lastStartAt = -float.MaxValue;

			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					Feedbacks[i].Initialization(owner);
				}                
			}
		}

        #endregion

        #region PLAY

        /// <summary>
        /// MMFeedbacks의 위치를 ​​참조로 사용하고 감쇠 없이 모든 피드백을 재생합니다.
        /// </summary>
        public virtual void PlayFeedbacks()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity);
		}

        /// <summary>
        /// 모든 피드백을 재생하고 완료될 때까지 기다립니다.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="forceRevert"></param>
        public virtual async System.Threading.Tasks.Task PlayFeedbacksTask(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			while (IsPlaying)
			{
				await System.Threading.Tasks.Task.Yield();
			}
		}

        /// <summary>
        /// 위치와 강도를 지정하여 모든 피드백을 재생합니다. 위치는 각 피드백에 의해 사용될 수 있으며 예를 들어 입자를 활성화하거나 사운드를 재생하는 데 고려됩니다.
        /// 피드백 강도는 각 피드백이 강도를 낮추는 데 사용할 수 있는 요소입니다. 
		/// 일반적으로 시간이나 거리를 기준으로 감쇠를 정의하고 싶을 것입니다(더 낮은 값 사용). 플레이어로부터 멀리 떨어진 곳에서 발생하는 피드백의 강도 값)
        /// 또한 현재 상태를 무시하고 피드백을 역방향으로 재생하도록 강제할 수 있습니다.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksOwner"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

        /// <summary>
        /// MMFeedbacks의 위치를 ​​참조로 사용하고 감쇠 없이 역방향(아래에서 위로)으로 모든 피드백을 재생합니다.
        /// </summary>
        public virtual void PlayFeedbacksInReverse()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true);
		}

        /// <summary>
        /// MMFeedbacks의 위치를 ​​참조로 사용하고 감쇠 없이 역방향(아래에서 위로)으로 모든 피드백을 재생합니다.
        /// </summary>
        public virtual void PlayFeedbacksInReverse(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

        /// <summary>
        /// 모든 피드백을 순서대로 재생합니다. 단, 이 MMFeedback이 역순으로 재생되는 경우에만 해당됩니다.
        /// </summary>
        public virtual void PlayFeedbacksOnlyIfReversed()
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks();
			}
		}

        /// <summary>
        /// 모든 피드백을 순서대로 재생합니다. 단, 이 MMFeedback이 역순으로 재생되는 경우에만 해당됩니다.
        /// </summary>
        public virtual void PlayFeedbacksOnlyIfReversed(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}

        /// <summary>
        /// 모든 피드백을 순서대로 재생합니다. 단, 이 MMFeedback이 정상적인 순서로 재생되는 경우에만 해당됩니다.
        /// </summary>
        public virtual void PlayFeedbacksOnlyIfNormalDirection()
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks();
			}
		}

        /// <summary>
        /// 모든 피드백을 순서대로 재생합니다. 단, 이 MMFeedback이 정상적인 순서로 재생되는 경우에만 해당됩니다.
        /// </summary>
        public virtual void PlayFeedbacksOnlyIfNormalDirection(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}

        /// <summary>
        /// MMFeedbacks 재생이 중지될 때까지 자신의 코루틴에서 양보하려는 경우 외부에서 호출할 수 있는 공개 코루틴
        /// 일반적으로: yield return myFeedback.PlayFeedbacksCoroutine(this.transform.position, 1.0f, false);
        /// </summary>
        /// <param name="position">The position at which the MMFeedbacks should play</param>
        /// <param name="feedbacksIntensity">The intensity of the feedback</param>
        /// <param name="forceRevert">Whether or not the MMFeedbacks should play in reverse or not</param>
        /// <returns></returns>
        public virtual IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			while (IsPlaying)
			{
				yield return null;    
			}
		}

        #endregion

        #region SEQUENCE

        /// <summary>
        /// 피드백을 재생하는 데 사용되는 내부 메서드는 외부에서 호출하면 안 됩니다.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual void PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
            if (!CanPlay)
			{
                return;
			}
			
			if (IsPlaying && !CanPlayWhileAlreadyPlaying)
			{
                return;
			}

			if (!EvaluateChance())
			{
                return;
			}

            // 쿨다운이 있으면 필요한 경우 실행을 방지합니다.
            if (CooldownDuration > 0f)
			{
				if (GetTime() - _lastStartAt < CooldownDuration)
				{
                    return;
				}
			}

            // 모든 MMFeedback이 전역적으로 비활성화되면 중지하고 재생하지 않습니다.
            if (!GlobalMMFeedbacksActive)
			{
                return;
			}

			if (!this.gameObject.activeInHierarchy)
			{
                return;
			}
            
			if (ShouldRevertOnNextPlay)
			{
				Revert();
				ShouldRevertOnNextPlay = false;
			}

			if (forceRevert)
			{
				Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
			}
            
			ResetFeedbacks();
			this.enabled = true;
			TimesPlayed++;
			IsPlaying = true;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			_totalDuration = TotalDuration;
			CheckForPauses();
            
			if (InitialDelay > 0f)
			{
                StartCoroutine(HandleInitialDelayCo(position, feedbacksIntensity, forceRevert));
			}
			else
			{
				PreparePlay(position, feedbacksIntensity, forceRevert);
			}
		}

		protected virtual void PreparePlay(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
            Events.TriggerOnPlay(this);

			_holdingMax = 0f;
			CheckForPauses();
			
			if (!_pauseFound)
			{
                PlayAllFeedbacks(position, feedbacksIntensity, forceRevert);
			}
			else
			{
                // 일시 중지가 하나 이상 발견된 경우
                StartCoroutine(PausedFeedbacksCo(position, feedbacksIntensity));
			}
		}

		protected virtual void CheckForPauses()
		{
			_pauseFound = false;
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}
					if ((Feedbacks[i].HoldingPause == true) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}    
				}
			}
		}

		protected virtual void PlayAllFeedbacks(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
            // 일시 중지가 발견되지 않으면 모든 피드백을 한 번에 재생합니다.
            for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (FeedbackCanPlay(Feedbacks[i]))
				{
                    Feedbacks[i].Play(position, feedbacksIntensity);
				}
			}
		}

		protected virtual IEnumerator HandleInitialDelayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
            Debug.Log("9");
            IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitFor(InitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}
        
		protected virtual void Update()
		{
			if (_shouldStop)
			{
				if (HasFeedbackStillPlaying())
				{
					return;
				}
				IsPlaying = false;
				Events.TriggerOnComplete(this);
				ApplyAutoRevert();
				this.enabled = false;
				_shouldStop = false;
			}
			if (IsPlaying)
			{
				if (!_pauseFound)
				{
					if (GetTime() - _startTime > _totalDuration)
					{
						_shouldStop = true;
					}    
				}
			}
			else
			{
				this.enabled = false;
			}
		}

        /// <summary>
        /// 피드백이 아직 재생 중이면 true를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public virtual bool HasFeedbackStillPlaying()
		{
			int count = Feedbacks.Count;
			for (int i = 0; i < count; i++)
			{
				if ((Feedbacks[i] != null) && (Feedbacks[i].IsPlaying))
				{
					return true;
				}
			}
			return false;
		}

        /// <summary>
        /// 일시 중지가 포함된 경우 일련의 피드백을 처리하는 데 사용되는 코루틴
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
		{
			IsPlaying = true;

			int i = (Direction == Directions.TopToBottom) ? 0 : Feedbacks.Count-1;

			while ((i >= 0) && (i < Feedbacks.Count))
			{
				if (!IsPlaying)
				{
					yield break;
				}

				if (Feedbacks[i] == null)
				{
					yield break;
				}
                
				if (((Feedbacks[i].Active) && (Feedbacks[i].ScriptDrivenPause)) || InScriptDrivenPause)
				{
					InScriptDrivenPause = true;

					bool inAutoResume = (Feedbacks[i].ScriptDrivenPauseAutoResume > 0f); 
					float scriptDrivenPauseStartedAt = GetTime();
					float autoResumeDuration = Feedbacks[i].ScriptDrivenPauseAutoResume;
                    
					while (InScriptDrivenPause)
					{
						if (inAutoResume && (GetTime() - scriptDrivenPauseStartedAt > autoResumeDuration))
						{
							ResumeFeedbacks();
						}
						yield return null;
					} 
				}

				// handles holding pauses
				if ((Feedbacks[i].Active)
				    && ((Feedbacks[i].HoldingPause == true) || (Feedbacks[i].LooperPause == true))
				    && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
				{
					Events.TriggerOnPause(this);
					// we stay here until all previous feedbacks have finished
					while (GetTime() - _lastStartAt < _holdingMax)
					{
						yield return null;
					}
					_holdingMax = 0f;
					_lastStartAt = GetTime();
				}

				// plays the feedback
				if (FeedbackCanPlay(Feedbacks[i]))
				{
					Feedbacks[i].Play(position, feedbacksIntensity);
				}

				// Handles pause
				if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
				{
					bool shouldPause = true;
					if (Feedbacks[i].Chance < 100)
					{
						float random = Random.Range(0f, 100f);
						if (random > Feedbacks[i].Chance)
						{
							shouldPause = false;
						}
					}

					if (shouldPause)
					{
						yield return Feedbacks[i].Pause;
						Events.TriggerOnResume(this);
						_lastStartAt = GetTime();
						_holdingMax = 0f;
					}
				}

				// updates holding max
				if (Feedbacks[i].Active)
				{
					if ((Feedbacks[i].Pause == null) && (Feedbacks[i].ShouldPlayInThisSequenceDirection) && (!Feedbacks[i].Timing.ExcludeFromHoldingPauses))
					{
						float feedbackDuration = Feedbacks[i].TotalDuration;
						_holdingMax = Mathf.Max(feedbackDuration, _holdingMax);
					}
				}

				// handles looper
				if ((Feedbacks[i].LooperPause == true)
				    && (Feedbacks[i].Active)
				    && (Feedbacks[i].ShouldPlayInThisSequenceDirection)
				    && (((Feedbacks[i] as MMFeedbackLooper).NumberOfLoopsLeft > 0) || (Feedbacks[i] as MMFeedbackLooper).InInfiniteLoop))
				{
					// we determine the index we should start again at
					bool loopAtLastPause = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastPause;
					bool loopAtLastLoopStart = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastLoopStart;

					int newi = 0;

					int j = (Direction == Directions.TopToBottom) ? i - 1 : i + 1;

					while ((j >= 0) && (j <= Feedbacks.Count))
					{
						// if we're at the start
						if (j == 0)
						{
							newi = j - 1;
							break;
						}
						if (j == Feedbacks.Count)
						{
							newi = j ;
							break;
						}
						// if we've found a pause
						if ((Feedbacks[j].Pause != null)
						    && (Feedbacks[j].FeedbackDuration > 0f)
						    && loopAtLastPause && (Feedbacks[j].Active))
						{
							newi = j;
							break;
						}
						// if we've found a looper start
						if ((Feedbacks[j].LooperStart == true)
						    && loopAtLastLoopStart
						    && (Feedbacks[j].Active))
						{
							newi = j;
							break;
						}

						j += (Direction == Directions.TopToBottom) ? -1 : 1;
					}
					i = newi;
				}
				i += (Direction == Directions.TopToBottom) ? 1 : -1;
			}
			float unscaledTimeAtEnd = GetTime();
			while (GetTime() - unscaledTimeAtEnd < _holdingMax)
			{
				yield return null;
			}
            
			while (HasFeedbackStillPlaying())
			{
				yield return null;
			}
            
			IsPlaying = false;
			Events.TriggerOnComplete(this);
			ApplyAutoRevert();
		}

        #endregion

        #region STOP

        /// <summary>
        /// 개별 피드백을 중지하지 않고 추가 피드백 재생을 모두 중지합니다.
        /// </summary>
        public virtual void StopFeedbacks()
		{
			StopFeedbacks(true);
		}

        /// <summary>
        /// 개별 피드백을 중지할 수 있는 옵션과 함께 모든 피드백 재생을 중지합니다.
        /// </summary>
        public virtual void StopFeedbacks(bool stopAllFeedbacks = true)
		{
			StopFeedbacks(this.transform.position, 1.0f, stopAllFeedbacks);
		}

        /// <summary>
        /// 피드백에서 사용할 수 있는 위치와 강도를 지정하여 모든 피드백 재생을 중지합니다.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool stopAllFeedbacks = true)
		{
			if (stopAllFeedbacks)
			{
				for (int i = 0; i < Feedbacks.Count; i++)
				{
					if (Feedbacks[i] != null)
					{
						Feedbacks[i].Stop(position, feedbacksIntensity);	
					}
				}    
			}
			IsPlaying = false;
			StopAllCoroutines();
		}

        #endregion

        #region CONTROLS

        /// <summary>
        /// 정의된 경우 각 피드백의 Reset 메서드를 호출합니다. 그 예로 깜박이는 렌더러의 초기 색상을 재설정할 수 있습니다.
        /// </summary>
        public virtual void ResetFeedbacks()
		{
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if ((Feedbacks[i] != null) && (Feedbacks[i].Active))
				{
					Feedbacks[i].ResetFeedback();    
				}
			}
			IsPlaying = false;
		}

        /// <summary>
        /// 이 MMFeedback의 방향을 변경합니다.
        /// </summary>
        public virtual void Revert()
		{
			Events.TriggerOnRevert(this);
			Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
		}

        /// <summary>
        /// 이 플레이어의 플레이를 승인하거나 방지하려면 이 방법을 사용하세요.
        /// </summary>
        /// <param name="newState"></param>
        public virtual void SetCanPlay(bool newState)
		{
			CanPlay = newState;
		}

        /// <summary>
        /// ResumeFeedbacks()를 호출하여 재개할 수 있는 시퀀스 실행을 일시 중지합니다.
        /// </summary>
        public virtual void PauseFeedbacks()
		{
			Events.TriggerOnPause(this);
			InScriptDrivenPause = true;
		}

        /// <summary>
        /// 스크립트 기반 일시 중지가 진행 중인 경우 시퀀스 실행을 재개합니다.
        /// </summary>
        public virtual void ResumeFeedbacks()
		{
			Events.TriggerOnResume(this);
			InScriptDrivenPause = false;
		}

		#endregion
        
		#region MODIFICATION
        
		public virtual MMFeedback AddFeedback(System.Type feedbackType, bool add = true)
		{
			MMFeedback newFeedback;
            
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				newFeedback = Undo.AddComponent(this.gameObject, feedbackType) as MMFeedback;
			}
			else
			{
				newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
			}
			#else 
                newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
			#endif
            
			newFeedback.hideFlags = HideFlags.HideInInspector;
			newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(feedbackType);

			AutoRepair();
            
			return newFeedback;
		}
        
		public virtual void RemoveFeedback(int id)
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Undo.DestroyObjectImmediate(Feedbacks[id]);
			}
			else
			{
				DestroyImmediate(Feedbacks[id]);
			}
			#else
                DestroyImmediate(Feedbacks[id]);
			#endif
            
			Feedbacks.RemoveAt(id);
			AutoRepair();
		}

        #endregion MODIFICATION

        #region HELPERS

        /// <summary>
        /// 이 피드백이 재생될 가능성을 평가하고, 이 피드백이 재생될 수 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateChance()
		{
			if (ChanceToPlay == 0f)
			{
				return false;
			}
			if (ChanceToPlay != 100f)
			{
				// determine the odds
				float random = Random.Range(0f, 100f);
				if (random > ChanceToPlay)
				{
					return false;
				}
			}

			return true;
		}

        /// <summary>
        /// 이 MMFeedbacks에 하나 이상의 루퍼 피드백이 포함되어 있는지 확인합니다.
        /// </summary>
        protected virtual void CheckForLoops()
		{
			ContainsLoop = false;
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					if (Feedbacks[i].LooperPause && Feedbacks[i].Active)
					{
						ContainsLoop = true;
						return;
					}
				}                
			}
		}

        /// <summary>
        /// 지정된 피드백의 Timing 섹션에 정의된 조건이 이 MMFeedbacks의 현재 재생 방향으로 재생되도록 허용하는 경우 true를 반환합니다.
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        protected bool FeedbackCanPlay(MMFeedback feedback)
		{
			if (feedback == null)
			{
				return false;
			}
			
			if (feedback.Timing == null)
			{
				return false;
			}
			
			if (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.Always)
			{
				return true;
			}
			else if (((Direction == Directions.TopToBottom) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards))
			         || ((Direction == Directions.BottomToTop) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards)))
			{
				return true;
			}
			return false;
		}

        /// <summary>
        /// 다음 플레이에서 방향을 되돌리도록 MMFeedback을 준비합니다.
        /// </summary>
        protected virtual void ApplyAutoRevert()
		{
			if (AutoChangeDirectionOnEnd)
			{
				ShouldRevertOnNextPlay = true;
			}
		}

        /// <summary>
        /// 이 피드백의 시간 승수를 기간(초)에 적용합니다.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public virtual float ApplyTimeMultiplier(float duration)
		{
			return duration * Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
		}

        /// <summary>
        /// Unity에는 때때로 직렬화 문제가 있습니다.
        /// 이 방법은 발생할 수 있는 잘못된 동기화를 수정하여 문제를 해결합니다.
        /// </summary>
        public virtual void AutoRepair()
		{
			List<Component> components = components = new List<Component>();
			components = this.gameObject.GetComponents<Component>().ToList();
			foreach (Component component in components)
			{
				if (component is MMFeedback)
				{
					bool found = false;
					for (int i = 0; i < Feedbacks.Count; i++)
					{
						if (Feedbacks[i] == (MMFeedback)component)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						Feedbacks.Add((MMFeedback)component);
					}
				}
			}
		}

        #endregion

        #region EVENTS

        /// <summary>
        /// On Disable 우리는 모든 피드백을 중지합니다
        /// </summary>
        protected virtual void OnDisable()
		{
			/*if (IsPlaying)
			{
			    StopFeedbacks();
			    StopAllCoroutines();
			}*/
		}

        /// <summary>
        /// On validate, DurationMultiplier가 양수로 유지되는지 확인합니다.
        /// </summary>
        protected virtual void OnValidate()
		{
			DurationMultiplier = Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
		}

        /// <summary>
        /// On Destroy, 남은 부분을 방지하기 위해 이 MMFeedbacks에서 모든 피드백을 제거합니다.
        /// </summary>
        protected virtual void OnDestroy()
		{
			IsPlaying = false;
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{            
				// we remove all binders
				foreach (MMFeedback feedback in Feedbacks)
				{
					EditorApplication.delayCall += () =>
					{
						DestroyImmediate(feedback);
					};                    
				}
			}
			#endif
		}     
        
		#endregion EVENTS
	}
}
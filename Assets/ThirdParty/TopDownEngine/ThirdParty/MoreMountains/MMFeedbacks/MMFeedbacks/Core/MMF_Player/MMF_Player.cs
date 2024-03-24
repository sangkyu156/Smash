using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Feedbacks/MMF Player")]
	[DisallowMultipleComponent] 
	public class MMF_Player : MMFeedbacks
	{
		#region PROPERTIES
        
		[SerializeReference]
		public List<MMF_Feedback> FeedbacksList;
        
		public override float TotalDuration
		{
			get
			{
				return _cachedTotalDuration;
			}
		}

		public bool KeepPlayModeChanges = false;
        /// 이것이 사실이라면 피드백이 재생되는 동안 검사기가 새로 고쳐지지 않습니다. 이렇게 하면 성능이 절약되지만 예를 들어 피드백 검사기의 진행률 표시줄이 매끄럽게 보이지는 않습니다.
        [Tooltip("이것이 사실이라면 피드백이 재생되는 동안 검사기가 새로 고쳐지지 않습니다. 이렇게 하면 성능이 절약되지만 예를 들어 피드백 검사기의 진행률 표시줄이 매끄럽게 보이지는 않습니다.")]
		public bool PerformanceMode = false;
        /// 이것이 사실이라면, 비활성화에 대한 모든 피드백에 대해 StopFeedbacks가 호출됩니다.
        [Tooltip("이것이 사실이라면, 비활성화에 대한 모든 피드백에 대해 StopFeedbacks가 호출됩니다.")]
		public bool StopFeedbacksOnDisable = false;
        /// 이 플레이어가 플레이를 시작한 횟수
        [Tooltip("이 플레이어가 플레이를 시작한 횟수")]
		[MMReadOnly]
		public int PlayCount = 0;

		public bool SkippingToTheEnd { get; protected set; }
        
		protected Type _t;
		protected float _cachedTotalDuration;
		protected bool _initialized = false;

        #endregion

        #region INITIALIZATION

        /// <summary>
        /// Awake에서 자동 모드에 있으면 피드백을 초기화합니다.'
        /// </summary>
        protected override void Awake()
		{
			if (AutoInitialization && (AutoPlayOnEnable || AutoPlayOnStart))
			{
				InitializationMode = InitializationModes.Awake;
			}
			
			// if our MMFeedbacks is in AutoPlayOnEnable mode, we add a little helper to it that will re-enable it if needed if the parent game object gets turned off and on again
			if (AutoPlayOnEnable)
			{
				MMF_PlayerEnabler playerEnabler = GetComponent<MMF_PlayerEnabler>(); 
				if (playerEnabler == null)
				{
					playerEnabler = this.gameObject.AddComponent<MMF_PlayerEnabler>();
				}
				playerEnabler.TargetMmfPlayer = this; 
			}
            
			if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
			{
				Initialization();
			}

			InitializeFeedbackList();
			ExtraInitializationChecks();
			CheckForLoops();
			ComputeCachedTotalDuration();
			PreInitialization();
		}

        /// <summary>
        /// On Start 자동 모드에 있으면 피드백을 초기화합니다.
        /// </summary>
        protected override void Start()
		{
			if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
			{
				Initialization();
			}
			if (AutoPlayOnStart && Application.isPlaying)
			{
				PlayFeedbacks();
			}
			CheckForLoops();
		}

        /// <summary>
        /// 피드백 목록을 초기화합니다.
        /// </summary>
        protected virtual void InitializeFeedbackList()
		{
			if (FeedbacksList == null)
			{
				FeedbacksList = new List<MMF_Feedback>();
			}
		}

        /// <summary>
        /// 주로 동적 생성 사례를 다루기 위해 추가 검사를 수행합니다.
        /// </summary>
        protected virtual void ExtraInitializationChecks()
		{
			if (Events == null)
			{
				Events = new MMFeedbacksEvents();
				Events.Initialization();
			}
		}

        /// <summary>
        /// On Enable 자동 모드에 있으면 피드백을 초기화합니다.
        /// </summary>
        protected override void OnEnable()
		{
			if (OnlyPlayIfWithinRange)
			{
				MMSetFeedbackRangeCenterEvent.Register(OnMMSetFeedbackRangeCenterEvent);	
			}
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				feedback.CacheRequiresSetup();
			}
			if (AutoPlayOnEnable && Application.isPlaying)
			{
				if (_lastOnEnableFrame == Time.frameCount)
				{
					return;
				}
				
				// if we're in the very first frames, we delay our play for 2 frames to avoid Unity bugs
				if (Time.frameCount < 2)
				{
					_lastOnEnableFrame = 2;
					StartCoroutine(PlayFeedbacksAfterFrames(2));
				}
				else
				{
					PlayFeedbacks();
				}
			}
		}

        /// <summary>
        /// X 프레임 후에 이 플레이어의 피드백 재생을 시작할 수 있는 코루틴
        /// </summary>
        /// <param name="framesAmount"></param>
        /// <returns></returns>
        public virtual IEnumerator PlayFeedbacksAfterFrames(int framesAmount)
		{
			yield return MMFeedbacksCoroutine.WaitForFrames(framesAmount);
			PlayFeedbacks();
		}

		public virtual void PreInitialization()
		{
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					FeedbacksList[i].PreInitialization(this, i);
				}                
			}
		}

        /// <summary>
        /// 피드백에 의한 위치 및 계층 구조에 대한 참조로 사용될 소유자를 지정하여 피드백을 초기화하는 공개 방법
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="feedbacksOwner"></param>
        public override void Initialization()
		{
			SkippingToTheEnd = false;
			IsPlaying = false;
			_lastStartAt = -float.MaxValue;

			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					FeedbacksList[i].Initialization(this, i);
				}
			}

			_initialized = true;
		}

        /// <summary>
        /// 소유자를 지정하는 데 사용된 레거시 init 메소드를 호출하면 MMF Player init가 강제로 실행됩니다.
        /// </summary>
        /// <param name="owner"></param>
        public override void Initialization(GameObject owner)
		{
			Initialization();
		}

        #endregion

        #region PLAY

        /// <summary>
        /// MMFeedbacks의 위치를 ​​참조로 사용하고 감쇠 없이 모든 피드백을 재생합니다.
        /// </summary>
        public override void PlayFeedbacks()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity);
		}

        /// <summary>
        /// 위치와 강도를 지정하여 모든 피드백을 재생합니다. 위치는 각 피드백에 의해 사용될 수 있으며 예를 들어 입자를 활성화하거나 사운드를 재생하는 데 고려됩니다.
        /// 피드백 강도는 각 피드백이 강도를 낮추는 데 사용할 수 있는 요소입니다. 일반적으로 시간이나 거리를 기준으로 감쇠를 정의하고 싶을 것입니다(플레이어로부터 더 멀리서 발생하는 피드백에는 더 낮은 강도 값 사용).
        /// 또한 현재 상태를 무시하고 피드백을 역방향으로 재생하도록 강제할 수 있습니다.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksOwner"></param>
        /// <param name="feedbacksIntensity"></param>
        public override void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

        /// <summary>
        /// MMFeedbacks의 위치를 ​​참조로 사용하고 감쇠 없이 역방향(아래에서 위로)으로 모든 피드백을 재생합니다.
        /// </summary>
        public override void PlayFeedbacksInReverse()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true);
		}

        /// <summary>
        /// MMFeedbacks의 위치를 ​​참조로 사용하고 감쇠 없이 역방향(아래에서 위로)으로 모든 피드백을 재생합니다.
        /// </summary>
        public override void PlayFeedbacksInReverse(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

        /// <summary>
        /// 모든 피드백을 순서대로 재생합니다. 단, 이 MMFeedback이 역순으로 재생되는 경우에만 해당됩니다.
        /// </summary>
        public override void PlayFeedbacksOnlyIfReversed()
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
        public override void PlayFeedbacksOnlyIfReversed(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
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
        public override void PlayFeedbacksOnlyIfNormalDirection()
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks();
			}
		}

        /// <summary>
        /// 모든 피드백을 순서대로 재생합니다. 단, 이 MMFeedback이 정상적인 순서로 재생되는 경우에만 해당됩니다.
        /// </summary>
        public override void PlayFeedbacksOnlyIfNormalDirection(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}

        /// <summary>
        /// MMFeedbacks 재생이 중지될 때까지 자신의 코루틴에서 양보하려는 경우 외부에서 호출할 수 있는 공개 코루틴
        /// 일반적으로 : yield return myFeedback.PlayFeedbacksCoroutine(this.transform.position, 1.0f, false);
        /// </summary>
        /// <param name="position">The position at which the MMFeedbacks should play</param>
        /// <param name="feedbacksIntensity">The intensity of the feedback</param>
        /// <param name="forceRevert">Whether or not the MMFeedbacks should play in reverse or not</param>
        /// <returns></returns>
        public override IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
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
        protected override void PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			if (AutoInitialization)
			{
				if (!_initialized)
				{
					Initialization();
				}
			}
			
			if (!IsAllowedToPlay(position))
			{
				return;
			}
            
			SkippingToTheEnd = false;
            
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
			_lastStartFrame = Time.frameCount;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			this.enabled = true;
			IsPlaying = true;
			PlayCount++;
			ComputeNewRandomDurationMultipliers();
			CheckForPauses();
            
			if (Time.frameCount < 2)
			{
				this.enabled = false;
				StartCoroutine(FrameOnePlayCo(position, feedbacksIntensity, forceRevert));
				return;
			}

			if (InitialDelay > 0f)
			{
				StartCoroutine(HandleInitialDelayCo(position, feedbacksIntensity, forceRevert));
			}
			else
			{
				PreparePlay(position, feedbacksIntensity, forceRevert);
			}
		}

        /// <summary>
        /// 이 피드백의 재생이 허용되면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public virtual bool IsAllowedToPlay(Vector3 position)
		{
			// if CanPlay is false, we're not allowed to play
			if (!CanPlay)
			{
				return false;
			}
			
			// if we're already playing and can't play while already playing, we're not allowed to play 
			if (IsPlaying && !CanPlayWhileAlreadyPlaying)
			{
				return false;
			}

			if (AutoPlayOnEnable && (_lastStartFrame == Time.frameCount))
			{
				return false;
			}

			// if we roll a dice and are below our chance rate, we're not allowed to play
			if (!EvaluateChance())
			{
				return false;
			}

			// if we are in cooldown, we're not allowed to play
			if (CooldownDuration > 0f)
			{
				if (GetTime() - _lastStartAt < CooldownDuration)
				{
					return false;
				}
			}

			// if all MMFeedbacks are disabled globally, we're not allowed to play
			if (!GlobalMMFeedbacksActive)
			{
				return false;
			}

			// if the game object this player is on disabled, we're not allowed to play
			if (!this.gameObject.activeInHierarchy)
			{
				return false;
			}
			
			// if we're using range and are not within range, we're not allowed to play
			if (OnlyPlayIfWithinRange)
			{
				if (RangeCenter == null)
				{
					return false;
				}
				float distanceToCenter = Vector3.Distance(position, RangeCenter.position);
				if (distanceToCenter > RangeDistance)
				{
					return false;
				}
			}

			return true;
		}
        
		protected virtual IEnumerator FrameOnePlayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			yield return null;
			this.enabled = true;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitForUnscaled(InitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}

		protected override void PreparePlay(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
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
				// if at least one pause was found
				StartCoroutine(PausedFeedbacksCo(position, feedbacksIntensity));
			}
		}
		
		protected override void CheckForPauses()
		{
			_pauseFound = false;
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					if ((FeedbacksList[i].Pause != null) && (FeedbacksList[i].Active) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}
					if ((FeedbacksList[i].HoldingPause == true) && (FeedbacksList[i].Active) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}    
				}
			}
		}

		protected override void PlayAllFeedbacks(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			// if no pause was found, we just play all feedbacks at once
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbackCanPlay(FeedbacksList[i]))
				{
					FeedbacksList[i].Play(position, feedbacksIntensity);
				}
			}
		}

		protected override IEnumerator HandleInitialDelayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitForUnscaled(InitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}
        
		protected override void Update()
		{
			if (_shouldStop)
			{
				if (HasFeedbackStillPlaying())
				{
					return;
				}
				IsPlaying = false;
				ApplyAutoRevert();
				this.enabled = false;
				_shouldStop = false;
				Events.TriggerOnComplete(this);
			}
			if (IsPlaying)
			{
				if (!_pauseFound)
				{
					if (GetTime() - _startTime > TotalDuration)
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
        /// 일시 중지가 포함된 경우 일련의 피드백을 처리하는 데 사용되는 코루틴
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected override IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
		{
			IsPlaying = true;

			int i = (Direction == Directions.TopToBottom) ? 0 : FeedbacksList.Count-1;

			int count = FeedbacksList.Count;
			while ((i >= 0) && (i < count))
			{
				if (!IsPlaying)
				{
					yield break;
				}

				if (FeedbacksList[i] == null)
				{
					yield break;
				}
                
				if (((FeedbacksList[i].Active) && (FeedbacksList[i].ScriptDrivenPause)) || InScriptDrivenPause)
				{
					InScriptDrivenPause = true;

					bool inAutoResume = (FeedbacksList[i].ScriptDrivenPauseAutoResume > 0f); 
					float scriptDrivenPauseStartedAt = GetTime();
					float autoResumeDuration = FeedbacksList[i].ScriptDrivenPauseAutoResume;
                    
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
				if ((FeedbacksList[i].Active)
				    && ((FeedbacksList[i].HoldingPause == true) || (FeedbacksList[i].LooperPause == true))
				    && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
				{
					Events.TriggerOnPause(this);
					// we stay here until all previous feedbacks have finished
					while ((GetTime() - _lastStartAt < _holdingMax) && !SkippingToTheEnd)
					{
						yield return null;
					}
					_holdingMax = 0f;
					_lastStartAt = GetTime();
				}

				// plays the feedback
				if (FeedbackCanPlay(FeedbacksList[i]))
				{
					FeedbacksList[i].Play(position, feedbacksIntensity);
				}

				// Handles pause
				if ((FeedbacksList[i].Pause != null) && (FeedbacksList[i].Active) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection) && !SkippingToTheEnd)
				{
					bool shouldPause = true;
					if (FeedbacksList[i].Chance < 100)
					{
						float random = Random.Range(0f, 100f);
						if (random > FeedbacksList[i].Chance)
						{
							shouldPause = false;
						}
					}

					if (shouldPause)
					{
						yield return FeedbacksList[i].Pause;
						Events.TriggerOnResume(this);
						_lastStartAt = GetTime();
						_holdingMax = 0f;
					}
				}

				// updates holding max
				if (FeedbacksList[i].Active)
				{
					if ((FeedbacksList[i].Pause == null) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection) && (!FeedbacksList[i].Timing.ExcludeFromHoldingPauses))
					{
						float feedbackDuration = FeedbacksList[i].TotalDuration;
						_holdingMax = Mathf.Max(feedbackDuration, _holdingMax);
					}
				}

				// handles looper
				if ((FeedbacksList[i].LooperPause == true)
				    && (FeedbacksList[i].Active)
				    && (FeedbacksList[i].ShouldPlayInThisSequenceDirection)
				    && (((FeedbacksList[i] as MMF_Looper).NumberOfLoopsLeft > 0) || (FeedbacksList[i] as MMF_Looper).InInfiniteLoop))
				{
					while (HasFeedbackStillPlaying() && !SkippingToTheEnd)
					{
						yield return null;
					}
	                
					// we determine the index we should start again at
					bool loopAtLastPause = (FeedbacksList[i] as MMF_Looper).LoopAtLastPause;
					bool loopAtLastLoopStart = (FeedbacksList[i] as MMF_Looper).LoopAtLastLoopStart;
                    
					int newi = 0;

					int j = (Direction == Directions.TopToBottom) ? i - 1 : i + 1;

					int listCount = FeedbacksList.Count;
					while ((j >= 0) && (j <= listCount))
					{
						// if we're at the start
						if (j == 0)
						{
							newi = j - 1;
							break;
						}
						if (j == listCount)
						{
							newi = j ;
							break;
						}
						// if we've found a pause
						if ((FeedbacksList[j].Pause != null)
						    && !SkippingToTheEnd
						    && (FeedbacksList[j].FeedbackDuration > 0f)
						    && loopAtLastPause && (FeedbacksList[j].Active))
						{
							newi = j;
							break;
						}
						// if we've found a looper start
						if ((FeedbacksList[j].LooperStart == true)
						    && !SkippingToTheEnd
						    && loopAtLastLoopStart
						    && (FeedbacksList[j].Active))
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
			while ((GetTime() - unscaledTimeAtEnd < _holdingMax) && !SkippingToTheEnd)
			{
				yield return null;
			}
			while (HasFeedbackStillPlaying() && !SkippingToTheEnd)
			{
				yield return null;
			}
			IsPlaying = false;
			Events.TriggerOnComplete(this);
			ApplyAutoRevert();
		}

		protected virtual IEnumerator SkipToTheEndCo()
		{
			if (_startTime == GetTime())
			{
				yield return null;
			}
			SkippingToTheEnd = true;
			Events.TriggerOnSkipToTheEnd(this);
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].SkipToTheEnd(this.transform.position);    
				}
			}
			yield return null;
			yield return null;
			SkippingToTheEnd = false;
			StopFeedbacks();
		}

        #endregion

        #region STOP

        /// <summary>
        /// 모든 추가 피드백 재생을 중지하고 개별 피드백을 중지합니다. 
        /// </summary>
        public override void StopFeedbacks()
		{
			StopFeedbacks(true);
		}

        /// <summary>
        /// 개별 피드백을 중지할 수 있는 옵션과 함께 모든 피드백 재생을 중지합니다.
        /// </summary>
        public override void StopFeedbacks(bool stopAllFeedbacks = true)
		{
			StopFeedbacks(this.transform.position, 1.0f, stopAllFeedbacks);
		}

        /// <summary>
        /// 피드백에서 사용할 수 있는 위치와 강도를 지정하여 모든 피드백 재생을 중지합니다.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public override void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool stopAllFeedbacks = true)
		{
			if (stopAllFeedbacks)
			{
				int count = FeedbacksList.Count;
				for (int i = 0; i < count; i++)
				{
					FeedbacksList[i].Stop(position, feedbacksIntensity);
				}    
			}
			IsPlaying = false;
			StopAllCoroutines();
		}

        #endregion

        #region CONTROLS

        /// <summary>
        /// 정의된 경우 각 피드백의 Reset 메서드를 호출합니다. 그 예로 깜박이는 렌더러의 초기 색상을 재설정할 수 있습니다. 일반적으로 재생하기 전에 자동으로 호출됩니다
        /// </summary>
        public override void ResetFeedbacks()
		{
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].ResetFeedback();    
				}
			}
			IsPlaying = false;
		}

        /// <summary>
        /// 이 MMFeedback의 방향을 변경합니다.
        /// </summary>
        public override void Revert()
		{
			Events.TriggerOnRevert(this);
			Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
		}

        /// <summary>
        /// 플레이어의 방향을 매개변수에 지정된 방향으로 설정합니다.
        /// </summary>
        public virtual void SetDirection(Directions newDirection)
		{
			Direction = newDirection;
		}

        /// <summary>
        /// 방향을 위에서 아래로 설정합니다.
        /// </summary>
        public void SetDirectionTopToBottom()
		{
			Direction = Directions.TopToBottom;
		}

        /// <summary>
        /// 방향을 아래에서 위로 설정합니다.
        /// </summary>
        public void SetDirectionBottomToTop()
		{
			Direction = Directions.BottomToTop;
		}

        /// <summary>
        /// ResumeFeedbacks()를 호출하여 재개할 수 있는 시퀀스 실행을 일시 중지합니다.
        /// </summary>
        public override void PauseFeedbacks()
		{
			Events.TriggerOnPause(this);
			InScriptDrivenPause = true;
		}

        /// <summary>
        /// ResumeFeedbacks()를 호출하여 재개할 수 있는 시퀀스 실행을 일시 중지합니다.
        /// 이는 설계상 피드백을 중지하지 않지만 대부분의 경우 StopFeedbacks()를 먼저 호출하는 것이 좋습니다.
        /// </summary>
        public virtual void RestoreInitialValues()
		{
			if (PlayCount <= 0)
			{
				return;
			}
			
			int count = FeedbacksList.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].RestoreInitialValues();    
				}
			}

			Events.TriggerOnRestoreInitialValues(this);
		}

        /// <summary>
        /// 일련의 피드백 끝으로 건너뜁니다. 설정에 따라 완료하는 데 최대 3프레임이 걸릴 수 있습니다. 플레이어를 즉시 비활성화하지 마십시오. 그렇지 않으면 건너뛰기가 완료되지 않습니다.
        /// </summary>
        public virtual void SkipToTheEnd()
		{
			StartCoroutine(SkipToTheEndCo());
		}

        /// <summary>
        /// 스크립트 기반 일시 중지가 진행 중인 경우 시퀀스 실행을 재개합니다.
        /// </summary>
        public override void ResumeFeedbacks()
		{
			Events.TriggerOnResume(this);
			InScriptDrivenPause = false;
		}

        #endregion

        #region MODIFICATION

        /// <summary>
        /// 지정된 MMF_Feedback을 플레이어에 추가합니다.
        /// </summary>
        /// <param name="newFeedback"></param>
        public virtual void AddFeedback(MMF_Feedback newFeedback)
		{
			InitializeFeedbackList();
			newFeedback.Owner = this;
			newFeedback.UniqueID = Guid.NewGuid().GetHashCode();
			FeedbacksList.Add(newFeedback);
			newFeedback.OnAddFeedback();
			newFeedback.CacheRequiresSetup();
			newFeedback.InitializeCustomAttributes();
		}

        /// <summary>
        /// 플레이어에 지정된 유형의 피드백을 추가합니다.
        /// </summary>
        /// <param name="feedbackType"></param>
        /// <returns></returns>
        public new MMF_Feedback AddFeedback(System.Type feedbackType, bool add = true)
		{
			InitializeFeedbackList();
			MMF_Feedback newFeedback = (MMF_Feedback)Activator.CreateInstance(feedbackType);
			newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(feedbackType);
			newFeedback.Owner = this;
			newFeedback.Timing = new MMFeedbackTiming();
			newFeedback.UniqueID = Guid.NewGuid().GetHashCode();
			if (add)
			{
				FeedbacksList.Add(newFeedback);	
			}
			newFeedback.OnAddFeedback();
			newFeedback.InitializeCustomAttributes();
			newFeedback.CacheRequiresSetup();
			return newFeedback;
		}

        /// <summary>
        /// 지정된 인덱스에서 피드백을 제거합니다.
        /// </summary>
        /// <param name="id"></param>
        public override void RemoveFeedback(int id)
		{
			if (FeedbacksList.Count < id)
			{
				return;
			}
			FeedbacksList.RemoveAt(id);
		}

        #endregion MODIFICATION

        #region HELPERS

        /// <summary>
        /// 피드백이 아직 재생 중이면 true를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public override bool HasFeedbackStillPlaying()
		{
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i].IsPlaying 
				     && !FeedbacksList[i].Timing.ExcludeFromHoldingPauses)
				    || FeedbacksList[i].Timing.RepeatForever)
				{
					return true;
				}
			}
			return false;
		}

        /// <summary>
        /// 이 MMFeedbacks에 하나 이상의 루퍼 피드백이 포함되어 있는지 확인합니다.
        /// </summary>
        protected override void CheckForLoops()
		{
			ContainsLoop = false;
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					if (FeedbacksList[i].LooperPause && FeedbacksList[i].Active)
					{
						ContainsLoop = true;
						return;
					}
				}                
			}
		}

        /// <summary>
        /// 필요한 경우 모든 피드백에 대해 새로운 무작위 기간 승수를 계산합니다.
        /// </summary>
        protected virtual void ComputeNewRandomDurationMultipliers()
		{
			if (RandomizeDuration)
			{
				_randomDurationMultiplier = Random.Range(RandomDurationMultiplier.x, RandomDurationMultiplier.y);
			}
			
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].RandomizeDuration))
				{
					FeedbacksList[i].ComputeNewRandomDurationMultiplier();
				}                
			}
		}

        /// <summary>
        /// 적용할 강도 승수를 결정합니다.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public virtual float ComputeRangeIntensityMultiplier(Vector3 position)
		{
			if (!OnlyPlayIfWithinRange)
			{
				return 1f;
			}

			if (RangeCenter == null)
			{
				return 0f;
			}

			float distanceToCenter = Vector3.Distance(position, RangeCenter.position);

			if (distanceToCenter > RangeDistance)
			{
				return 0f;
			}

			if (!UseRangeFalloff)
			{
				return 1f;
			}

			float normalizedDistance = MMFeedbacksHelpers.Remap(distanceToCenter, 0f, RangeDistance, 0f, 1f);
			float curveValue = RangeFalloff.Evaluate(normalizedDistance);
			float newIntensity = MMFeedbacksHelpers.Remap(curveValue, 0f, 1f, RemapRangeFalloff.x, RemapRangeFalloff.y);
			return newIntensity;
		}

        /// <summary>
        /// 지정된 피드백의 Timing 섹션에 정의된 조건이 이 MMFeedbacks의 현재 재생 방향으로 재생되도록 허용하는 경우 true를 반환합니다.
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        protected bool FeedbackCanPlay(MMF_Feedback feedback)
		{
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
        protected override void ApplyAutoRevert()
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
        public override float ApplyTimeMultiplier(float duration)
		{
			return duration * Mathf.Clamp(DurationMultiplier, _smallValue, float.MaxValue) * _randomDurationMultiplier;
		}

        /// <summary>
        /// 피드백에서 객체를 파괴할 수 있습니다.
        /// </summary>
        /// <param name="gameObjectToDestroy"></param>
        public virtual void ProxyDestroy(GameObject gameObjectToDestroy)
		{
			Destroy(gameObjectToDestroy);
		}

        /// <summary>
        /// 피드백으로 인해 지연된 후 객체를 파괴할 수 있습니다.
        /// </summary>
        /// <param name="gameObjectToDestroy"></param>
        /// <param name="delay"></param>
        public virtual void ProxyDestroy(GameObject gameObjectToDestroy, float delay)
		{
			Destroy(gameObjectToDestroy, delay);
		}

        /// <summary>
        /// 피드백에서 DestroyImmediate 객체를 사용할 수 있습니다.
        /// </summary>
        /// <param name="gameObjectToDestroy"></param>
        public virtual void ProxyDestroyImmediate(GameObject gameObjectToDestroy)
		{
			DestroyImmediate(gameObjectToDestroy);
		}
        
		#endregion

		#region ACCESS
		
		public enum AccessMethods { First, Previous, Closest, Next, Last }

        /// <summary>
        /// 선택한 방법 및 유형을 기반으로 이 플레이어 목록에서 발견된 첫 번째 피드백을 반환합니다.
        /// First : 목록에서 일치하는 유형의 첫 번째 피드백(위에서 아래로)
        /// Previous : 참조 인덱스의 피드백 이전(위)에 위치한 일치 유형의 첫 번째 피드백
        /// Closest : 기준 인덱스에서 피드백 이전 또는 이후에 위치한 일치 유형의 첫 번째 피드백
        /// Next : 참조 인덱스의 피드백 뒤에(아래와 같이) 위치한 일치 유형의 첫 번째 피드백
        /// First : 목록에 있는 일치 유형의 마지막 피드백(위에서 아래로)
        /// </summary>
        /// <param name="method"></param>
        /// <param name="referenceIndex"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T GetFeedbackOfType<T>(AccessMethods method, int referenceIndex) where T:MMF_Feedback
		{
			_t = typeof(T);

			referenceIndex = Mathf.Clamp(referenceIndex, 0, FeedbacksList.Count);
			
			switch (method)
			{
				case AccessMethods.First:
					for (int i = 0; i < FeedbacksList.Count; i++)
					{
						if (Check(i)) { return (T)FeedbacksList[i]; }
					}
					break;
				case AccessMethods.Previous:
					for (int i = referenceIndex; i >= 0; i--)
					{
						if (Check(i)) { return (T)FeedbacksList[i]; }
					}
					break;
				case AccessMethods.Closest:
					int closestIndexBack = referenceIndex;
					int closestIndexForward = referenceIndex;
					for (int i = referenceIndex; i >= 0; i--)
					{
						if (Check(i))
						{
							closestIndexBack = i;
							break;
						}
					}

					for (int i = referenceIndex; i < FeedbacksList.Count; i++)
					{
						if (Check(i))
						{
							closestIndexForward = i;
							break;
						}
					}

					int foundIndex;
					if ((closestIndexBack != referenceIndex) || (closestIndexForward != referenceIndex))
					{
						if (closestIndexBack == referenceIndex) { foundIndex = closestIndexForward; }
						else if (closestIndexForward == referenceIndex) { foundIndex = closestIndexBack; }
						else
						{
							int distanceBack = Mathf.Abs(referenceIndex - closestIndexBack);
							int distanceForward = Mathf.Abs(referenceIndex - closestIndexForward);
							foundIndex = (distanceBack > distanceForward) ? closestIndexForward : closestIndexBack;
						}
						return (T)FeedbacksList[foundIndex];
					}
					else
					{
						return null;
					}
				case AccessMethods.Next:
					for (int i = referenceIndex; i < FeedbacksList.Count; i++)
					{
						if (Check(i)) { return (T)FeedbacksList[i]; }
					}
					break;
				case AccessMethods.Last:
					for (int i = FeedbacksList.Count - 1; i >= 0; i--)
					{
						if (Check(i)) { return (T)FeedbacksList[i]; }
					}
					break;
			}
			return null;

			bool Check(int i)
			{
				return (FeedbacksList[i].GetType() == _t);
			}
		}

        /// <summary>
        /// 이 MMF_Player에서 검색된 유형의 첫 번째 피드백을 반환합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T GetFeedbackOfType<T>() where T:MMF_Feedback
		{
			_t = typeof(T);
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if (feedback.GetType() == _t)
				{
					return (T)feedback;
				}
			}
			return null;
		}

        /// <summary>
        /// 이 MMF_Player에서 검색된 유형의 모든 피드백 목록을 반환합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual List<T> GetFeedbacksOfType<T>() where T:MMF_Feedback
		{
			_t = typeof(T);
			List<T> list = new List<T>();
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if (feedback.GetType() == _t)
				{
					list.Add((T)feedback);
				}
			}
			return list;
		}

        /// <summary>
        /// 이 MMF_Player에서 검색된 유형의 첫 번째 피드백을 반환합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T GetFeedbackOfType<T>(string searchedLabel) where T:MMF_Feedback
		{
			_t = typeof(T);
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if ((feedback.GetType() == _t) && (feedback.Label == searchedLabel))
				{
					return (T)feedback;
				}
			}
			return null;
		}

        /// <summary>
        /// 이 MMF_Player에서 검색된 유형의 모든 피드백 목록을 반환합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual List<T> GetFeedbacksOfType<T>(string searchedLabel) where T:MMF_Feedback
		{
			_t = typeof(T);
			List<T> list = new List<T>();
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if ((feedback.GetType() == _t) && (feedback.Label == searchedLabel))
				{
					list.Add((T)feedback);
				}
			}
			return list;
		}

        #endregion

        #region EVENTS

        /// <summary>
        /// MMSetFeedbackRangeCenterEvent를 받으면 새로운 범위 센터를 설정합니다.
        /// </summary>
        /// <param name="newTransform"></param>
        protected virtual void OnMMSetFeedbackRangeCenterEvent(Transform newTransform)
		{
			if (IgnoreRangeEvents)
			{
				return;
			}
			RangeCenter = newTransform;
		}

        /// <summary>
        /// On Disable 우리는 모든 피드백을 중지합니다
        /// </summary>
        protected override void OnDisable()
		{
			if (OnlyPlayIfWithinRange)
			{
				MMSetFeedbackRangeCenterEvent.Unregister(OnMMSetFeedbackRangeCenterEvent);	
			}
			
			if (IsPlaying)
			{
				if (StopFeedbacksOnDisable)
				{
					StopFeedbacks();    
				}
				StopAllCoroutines();
				for (int i = FeedbacksList.Count - 1; i >= 0; i--)
				{
					FeedbacksList[i].OnDisable();
				}
			}
		}

        /// <summary>
        /// On validate, DurationMultiplier가 양수로 유지되는지 확인합니다.
        /// </summary>
        protected override void OnValidate()
		{
			RefreshCache();

			if ((FeedbacksList != null) && (FeedbacksList.Count > 0))
			{
				for (int i = FeedbacksList.Count - 1; i >= 0; i--)
				{
					FeedbacksList[i].OnValidate();	
				}	
			}
		}

        /// <summary>
        /// 캐시된 피드백을 새로 고칩니다.
        /// </summary>
        public virtual void RefreshCache()
		{
			if (FeedbacksList == null)
			{
				return;
			}
            
			DurationMultiplier = Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
            
			for (int i = FeedbacksList.Count - 1; i >= 0; i--)
			{
				if (FeedbacksList[i] == null)
				{
					FeedbacksList.RemoveAt(i);
				}
				else
				{
					FeedbacksList[i].Owner = this;
					FeedbacksList[i].CacheRequiresSetup();
				}
			}

			ComputeCachedTotalDuration();
		}

        /// <summary>
        /// 플레이어 피드백 시퀀스의 총 지속 시간을 계산합니다.
        /// </summary>
        public virtual void ComputeCachedTotalDuration()
		{
			float total = 0f;
			if (FeedbacksList == null)
			{
				_cachedTotalDuration = InitialDelay;
				return;
			}
			
			CheckForPauses();

			if (!_pauseFound)
			{
				foreach (MMF_Feedback feedback in FeedbacksList)
				{
					feedback.ComputeTotalDuration();
					if ((feedback != null) && (feedback.Active) && feedback.ShouldPlayInThisSequenceDirection)
					{
						if (total < feedback.TotalDuration)
						{
							total = feedback.TotalDuration;    
						}
					}
				}
			}
			else
			{
				int lastLooperStart = 0;
				int lastLoopFoundAt = 0;
				int lastPauseFoundAt = 0;
				int loopsLeft = 0;
				int iterations = 0;
				int maxIterationsSafety = 1000;
				float currentPauseDelay = 0f;
				int i = (Direction == Directions.TopToBottom) ? 0 : Feedbacks.Count-1;
				float intermediateTotal = 0f;
				while ((i >= 0) && (i < FeedbacksList.Count) && (iterations < maxIterationsSafety))
				{
					iterations++;
					
					if ((FeedbacksList[i] != null) && FeedbacksList[i].Active && FeedbacksList[i].ShouldPlayInThisSequenceDirection)
					{
						FeedbacksList[i].ComputeTotalDuration();
						if (FeedbacksList[i].Pause != null)
						{
							// pause
							if (FeedbacksList[i].HoldingPause)
							{
								intermediateTotal += (FeedbacksList[i] as MMF_Pause).PauseDuration;
								total += intermediateTotal;
								intermediateTotal = 0f;
							}
							else
							{
								currentPauseDelay += (FeedbacksList[i] as MMF_Pause).PauseDuration;
							}
							
							//loops
							if (FeedbacksList[i].LooperStart)
							{
								lastLooperStart = i;
							}

							if (!FeedbacksList[i].LooperPause)
							{
								lastPauseFoundAt = i;
							}

							if (FeedbacksList[i].LooperPause && ((FeedbacksList[i] as MMF_Looper).NumberOfLoops > 0))
							{
								if (i == lastLoopFoundAt)
								{
									loopsLeft--;
									if (loopsLeft <= 0)
									{
										i += (Direction == Directions.TopToBottom) ? 1 : -1;
										continue;
									}
								}
								else
								{
									lastLoopFoundAt = i;
									loopsLeft = (FeedbacksList[i] as MMF_Looper).NumberOfLoops - 1;
								}
								
								if ((FeedbacksList[i] as MMF_Looper).InfiniteLoop)
								{
									_cachedTotalDuration = 999f; 
									return;
								}

								if ((FeedbacksList[i] as MMF_Looper).LoopAtLastPause)
								{
									i = lastPauseFoundAt;
									total += intermediateTotal;
									intermediateTotal = 0f;
									currentPauseDelay = 0f;
									continue;
								}
								else if ((FeedbacksList[i] as MMF_Looper).LoopAtLastLoopStart)
								{
									i = lastLooperStart;
									total += intermediateTotal;
									intermediateTotal = 0f;
									currentPauseDelay = 0f;
									continue;
								}
								else
								{
									i = 0;
									total += intermediateTotal;
									intermediateTotal = 0f;
									currentPauseDelay = 0f;
									continue;
								}
							}	
						}
						else
						{
							float feedbackDuration = FeedbacksList[i].TotalDuration + currentPauseDelay;
							if (intermediateTotal < feedbackDuration)
							{
								intermediateTotal = feedbackDuration;    
							}
						}
					}
					
					i += (Direction == Directions.TopToBottom) ? 1 : -1;
				}
				total += intermediateTotal;
			}
			_cachedTotalDuration = InitialDelay + total;
		}

        /// <summary>
        /// On Destroy, 남은 부분을 방지하기 위해 이 MMFeedbacks에서 모든 피드백을 제거합니다.
        /// </summary>
        protected override void OnDestroy()
		{
			IsPlaying = false;
            
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				feedback.OnDestroy();
			}
		}

        /// <summary>
        /// Draws gizmos, MMF_Player가 선택된 경우, 동일한 이름의 메소드를 구현하는 모든 피드백에 대해
        /// </summary>
        protected void OnDrawGizmosSelected()
		{
			if (FeedbacksList == null)
			{
				return;
			}
            
			for (int i = FeedbacksList.Count - 1; i >= 0; i--)
			{
				FeedbacksList[i].OnDrawGizmosSelectedHandler();
			}
		}

		#endregion EVENTS
	}    
}
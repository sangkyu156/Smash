using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace MoreMountains.Tools
{	
	[System.Serializable]
	public class ProgressEvent : UnityEvent<float>{}

    /// <summary>
    /// 추가 로딩 설정을 저장하는 데 사용되는 간단한 클래스
    /// </summary>
    [Serializable]
	public class MMAdditiveSceneLoadingManagerSettings
	{
		
		/// the possible ways to unload scenes
		public enum UnloadMethods { None, ActiveScene, AllScenes };
		
		/// the name of the MMSceneLoadingManager scene you want to use when in additive mode
		[Tooltip("추가 모드에서 사용하려는 MMSceneLoadingManager 장면의 이름")]
		public string LoadingSceneName = "MMAdditiveLoadingScreen";
		/// when in additive loading mode, the thread priority to apply to the loading
		[Tooltip("추가 로딩 모드에 있을 때 로딩에 적용할 스레드 우선순위")]
		public ThreadPriority ThreadPriority = ThreadPriority.High;
		/// whether or not to make additional sanity checks (better leave this to true)
		[Tooltip("추가 온전성 검사를 수행할지 여부(true로 두는 것이 좋음)")]
		public bool SecureLoad = true;
		/// when in additive loading mode, whether or not to interpolate the progress bar's progress
		[Tooltip("추가 로딩 모드에서 진행률 표시줄의 진행률을 보간할지 여부")]
		public bool InterpolateProgress = true;
		/// when in additive loading mode, when in additive loading mode, the duration (in seconds) of the delay before the entry fade
		[Tooltip("추가 로딩 모드에 있는 경우, 추가 로딩 모드에 있는 경우 항목이 페이드되기 전의 지연 기간(초)입니다.")]
		public float BeforeEntryFadeDelay = 0f;
		/// when in additive loading mode, the duration (in seconds) of the entry fade
		[Tooltip("추가 로딩 모드에 있을 때 항목 페이드의 지속 시간(초)")]
		public float EntryFadeDuration = 0.25f;
		/// when in additive loading mode, the duration (in seconds) of the delay before the entry fade
		[Tooltip("추가 로딩 모드에 있을 때 항목이 페이드되기 전의 지연 기간(초)입니다.")]
		public float AfterEntryFadeDelay = 0.1f;
		/// when in additive loading mode, the duration (in seconds) of the delay before the exit fade
		[Tooltip("추가 로딩 모드에 있을 때 종료 페이드 전 지연 시간(초)입니다.")]
		public float BeforeExitFadeDelay = 0.25f;
		/// when in additive loading mode, the duration (in seconds) of the exit fade
		[Tooltip("추가 로딩 모드에 있을 때 종료 페이드 지속 시간(초)")]
		public float ExitFadeDuration = 0.2f;
		/// when in additive loading mode, when in additive loading mode, the tween to use to fade on entry
		[Tooltip("추가 로딩 모드에 있는 경우, 추가 로딩 모드에 있는 경우 항목 시 페이드에 사용할 트윈")]
		public MMTweenType EntryFadeTween = null;
		/// when in additive loading mode, the tween to use to fade on exit
		[Tooltip("추가 로딩 모드에 있을 때 종료 시 페이드하는 데 사용할 트윈")]
		public MMTweenType ExitFadeTween = null;
		/// when in additive loading mode, the speed at which the loader's progress bar should move
		[Tooltip("추가 로딩 모드에 있을 때 로더의 진행률 표시줄이 이동해야 하는 속도")]
		public float ProgressBarSpeed = 5f;
		/// when in additive loading mode, the selective additive fade mode
		[Tooltip("첨가제 로딩 모드에 있을 때 선택적 첨가제 페이드 모드")]
		public MMAdditiveSceneLoadingManager.FadeModes FadeMode = MMAdditiveSceneLoadingManager.FadeModes.FadeInThenOut;
		/// the chosen way to unload scenes (none, only the active scene, all loaded scenes)
		[Tooltip("장면을 언로드하기 위해 선택한 방법(없음, 활성 장면만, 로드된 모든 장면)")]
		public UnloadMethods UnloadMethod = UnloadMethods.AllScenes;
        /// 추가로 로드할 때 사용할 유출 방지 장면의 이름입니다.
        /// 비워두면 해당 장면이 자동으로 생성되지만 이에 사용할 장면을 지정할 수 있습니다. 일반적으로 유출 방지 장면을 빈 장면으로 만들고 싶지만 예를 들어 조명 설정을 사용자 정의할 수 있습니다.
        [Tooltip("추가로 로드할 때 사용할 유출 방지 장면의 이름입니다." +
                 "비워두면 해당 장면이 자동으로 생성되지만 이에 사용할 장면을 지정할 수 있습니다. 일반적으로 유출 방지 장면을 빈 장면으로 만들고 싶지만 예를 들어 조명 설정을 사용자 정의할 수 있습니다.")]
		public string AntiSpillSceneName = "";
	}

    /// <summary>
    /// 기본 API 대신 로딩 화면을 사용하여 장면을 로드하는 클래스
    /// 이는 기존 LoadingSceneManager의 새 버전입니다(일관성을 위해 이제 MMSceneLoadingManager로 이름이 변경됨).
    /// </summary>
    public class MMAdditiveSceneLoadingManager : MonoBehaviour 
	{
		/// The possible orders in which to play fades (depends on the fade you've set in your loading screen
		public enum FadeModes { FadeInThenOut, FadeOutThenIn }

		[Header("Audio Listener")] 
		public AudioListener LoadingAudioListener;
		
		[Header("Settings")]
		/// the ID on which to trigger a fade, has to match the ID on the fader in your scene
		[Tooltip("페이드를 트리거하는 ID는 장면의 페이더에 있는 ID와 일치해야 합니다.")]
		public int FaderID = 500;
		/// whether or not to output debug messages to the console
		[Tooltip("콘솔에 디버그 메시지를 출력할지 여부")]
		public bool DebugMode = false;

		[Header("Progress Events")] 
		/// an event used to update progress 
		[Tooltip("진행 상황을 업데이트하는 데 사용되는 이벤트")]
		public ProgressEvent SetRealtimeProgressValue;
		/// an event used to update progress with interpolation
		[Tooltip("보간을 통해 진행 상황을 업데이트하는 데 사용되는 이벤트")]
		public ProgressEvent SetInterpolatedProgressValue;

		[Header("State Events")]
		/// an event that will be invoked when the load starts
		[Tooltip("로드가 시작될 때 호출될 이벤트")]
		public UnityEvent OnLoadStarted;
		/// an event that will be invoked when the delay before the entry fade starts
		[Tooltip("항목 페이드가 시작되기 전 지연이 발생할 때 호출되는 이벤트")]
		public UnityEvent OnBeforeEntryFade;
		/// an event that will be invoked when the entry fade starts
		[Tooltip("항목 페이드가 시작될 때 호출될 이벤트")]
		public UnityEvent OnEntryFade;
		/// an event that will be invoked when the delay after the entry fade starts
		[Tooltip("항목 페이드가 시작된 후 지연이 시작될 때 호출되는 이벤트")]
		public UnityEvent OnAfterEntryFade;
		/// an event that will be invoked when the origin scene gets unloaded
		[Tooltip("원본 장면이 언로드될 때 호출될 이벤트")]
		public UnityEvent OnUnloadOriginScene;
		/// an event that will be invoked when the destination scene starts loading
		[Tooltip("대상 장면이 로드되기 시작할 때 호출될 이벤트")]
		public UnityEvent OnLoadDestinationScene;
		/// an event that will be invoked when the load of the destination scene is complete
		[Tooltip("대상 장면의 로드가 완료되면 호출될 이벤트")]
		public UnityEvent OnLoadProgressComplete;
		/// an event that will be invoked when the interpolated load of the destination scene is complete
		[Tooltip("대상 장면의 보간된 로드가 완료되면 호출될 이벤트")]
		public UnityEvent OnInterpolatedLoadProgressComplete;
		/// an event that will be invoked when the delay before the exit fade starts
		[Tooltip("종료 페이드가 시작되기 전의 지연이 시작될 때 호출될 이벤트")]
		public UnityEvent OnBeforeExitFade;
		/// an event that will be invoked when the exit fade starts
		[Tooltip("종료 페이드가 시작될 때 호출될 이벤트")]
		public UnityEvent OnExitFade;
		/// an event that will be invoked when the destination scene gets activated
		[Tooltip("대상 장면이 활성화될 때 호출될 이벤트")]
		public UnityEvent OnDestinationSceneActivation;
		/// an event that will be invoked when the scene loader gets unloaded
		[Tooltip("씬 로더가 언로드될 때 호출될 이벤트")]
		public UnityEvent OnUnloadSceneLoader;

		protected static bool _interpolateProgress;
		protected static float _progressInterpolationSpeed;
		protected static float _beforeEntryFadeDelay;
		protected static MMTweenType _entryFadeTween;
		protected static float _entryFadeDuration;
		protected static float _afterEntryFadeDelay;
		protected static float _beforeExitFadeDelay;
		protected static MMTweenType _exitFadeTween;
		protected static float _exitFadeDuration;
		protected static FadeModes _fadeMode;
		protected static string _sceneToLoadName = "";
		protected static string _loadingScreenSceneName;
		protected static List<string> _scenesInBuild;
		protected static Scene[] _initialScenes;
		protected float _loadProgress = 0f;
		protected float _interpolatedLoadProgress;
		protected static bool _loadingInProgress = false;
		protected AsyncOperation _unloadOriginAsyncOperation;
		protected AsyncOperation _loadDestinationAsyncOperation;
		protected AsyncOperation _unloadLoadingAsyncOperation;
		protected bool _setRealtimeProgressValueIsNull;
		protected bool _setInterpolatedProgressValueIsNull;
		protected const float _asyncProgressLimit = 0.9f;
		protected MMSceneLoadingAntiSpill _antiSpill = new MMSceneLoadingAntiSpill();
		protected static string _antiSpillSceneName = "";

		/// <summary>
		/// Call this static method to load a scene from anywhere (packed settings signature)
		/// </summary>
		/// <param name="sceneToLoadName"></param>
		/// <param name="settings"></param>
		public static void LoadScene(string sceneToLoadName, MMAdditiveSceneLoadingManagerSettings settings)
		{
			LoadScene(sceneToLoadName, settings.LoadingSceneName, settings.ThreadPriority, settings.SecureLoad, settings.InterpolateProgress,
				settings.BeforeEntryFadeDelay, settings.EntryFadeDuration, settings.AfterEntryFadeDelay, settings.BeforeExitFadeDelay,
				settings.ExitFadeDuration, settings.EntryFadeTween, settings.ExitFadeTween, settings.ProgressBarSpeed, settings.FadeMode, settings.UnloadMethod, settings.AntiSpillSceneName);
		}

        /// <summary>
        /// 어디에서나 장면을 로드하려면 이 정적 메서드를 호출하세요.
        /// </summary>
        /// <param name="sceneToLoadName">Level name.</param>
        public static void LoadScene(string sceneToLoadName, string loadingSceneName = "MMAdditiveLoadingScreen", 
			ThreadPriority threadPriority = ThreadPriority.High, bool secureLoad = true,
			bool interpolateProgress = true,
			float beforeEntryFadeDelay = 0f,
			float entryFadeDuration = 0.25f,
			float afterEntryFadeDelay = 0.1f,
			float beforeExitFadeDelay = 0.25f,
			float exitFadeDuration = 0.2f, 
			MMTweenType entryFadeTween = null, MMTweenType exitFadeTween = null,
			float progressBarSpeed = 5f, 
			FadeModes fadeMode = FadeModes.FadeInThenOut,
			MMAdditiveSceneLoadingManagerSettings.UnloadMethods unloadMethod = MMAdditiveSceneLoadingManagerSettings.UnloadMethods.AllScenes,
			string antiSpillSceneName = "")
		{
			if (_loadingInProgress)
			{
				Debug.LogError("MMLoadingSceneManagerAdditive : a request to load a new scene was emitted while a scene load was already in progress");  
				return;
			}

			if (entryFadeTween == null)
			{
				entryFadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
			}

			if (exitFadeTween == null)
			{
				exitFadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
			}

			if (secureLoad)
			{
				_scenesInBuild = MMScene.GetScenesInBuild();
	            
				if (!_scenesInBuild.Contains(sceneToLoadName))
				{
					Debug.LogError("MMLoadingSceneManagerAdditive : impossible to load the '"+sceneToLoadName+"' scene, " +
					               "there is no such scene in the project's build settings.");
					return;
				}
				if (!_scenesInBuild.Contains(loadingSceneName))
				{
					Debug.LogError("MMLoadingSceneManagerAdditive : impossible to load the '"+loadingSceneName+"' scene, " +
					               "there is no such scene in the project's build settings.");
					return;
				}
			}

			_loadingInProgress = true;
			_initialScenes = GetScenesToUnload(unloadMethod);

			Application.backgroundLoadingPriority = threadPriority;
			_sceneToLoadName = sceneToLoadName;					
			_loadingScreenSceneName = loadingSceneName;
			_beforeEntryFadeDelay = beforeEntryFadeDelay;
			_entryFadeDuration = entryFadeDuration;
			_entryFadeTween = entryFadeTween;
			_afterEntryFadeDelay = afterEntryFadeDelay;
			_progressInterpolationSpeed = progressBarSpeed;
			_beforeExitFadeDelay = beforeExitFadeDelay;
			_exitFadeDuration = exitFadeDuration;
			_exitFadeTween = exitFadeTween;
			_fadeMode = fadeMode;
			_interpolateProgress = interpolateProgress;
			_antiSpillSceneName = antiSpillSceneName;

			SceneManager.LoadScene(_loadingScreenSceneName, LoadSceneMode.Additive);
		}
        
		private static Scene[] GetScenesToUnload(MMAdditiveSceneLoadingManagerSettings.UnloadMethods unloaded)
		{
	        
			switch (unloaded) {
				case MMAdditiveSceneLoadingManagerSettings.UnloadMethods.None:
					_initialScenes = new Scene[0];
					break;
				case MMAdditiveSceneLoadingManagerSettings.UnloadMethods.ActiveScene:
					_initialScenes = new Scene[1] {SceneManager.GetActiveScene()};
					break;
				default:
				case MMAdditiveSceneLoadingManagerSettings.UnloadMethods.AllScenes:
					_initialScenes = MMScene.GetLoadedScenes();
					break;
			}
			return _initialScenes;
		}


		/// <summary>
		/// Starts loading the new level asynchronously
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

        /// <summary>
        /// 시간 단위를 초기화하고, null 검사를 계산하고, 로드 시퀀스를 시작합니다.
        /// </summary>
        protected virtual void Initialization()
		{
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : Initialization");

			if (DebugMode)
			{
				foreach (Scene scene in _initialScenes)
				{
					MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : Initial scene : " + scene.name);
				}    
			}

			_setRealtimeProgressValueIsNull = SetRealtimeProgressValue == null;
			_setInterpolatedProgressValueIsNull = SetInterpolatedProgressValue == null;
			Time.timeScale = 1f;

			if ((_sceneToLoadName == "") || (_loadingScreenSceneName == ""))
			{
				return;
			}
            
			StartCoroutine(LoadSequence());
		}

		/// <summary>
		/// Every frame, we fill the bar smoothly according to loading progress
		/// </summary>
		protected virtual void Update()
		{
			UpdateProgress();
		}

		/// <summary>
		/// Sends progress value via UnityEvents
		/// </summary>
		protected virtual void UpdateProgress()
		{
			if (!_setRealtimeProgressValueIsNull)
			{
				SetRealtimeProgressValue.Invoke(_loadProgress);
			}

			if (_interpolateProgress)
			{
				_interpolatedLoadProgress = MMMaths.Approach(_interpolatedLoadProgress, _loadProgress, Time.unscaledDeltaTime * _progressInterpolationSpeed);
				if (!_setInterpolatedProgressValueIsNull)
				{
					SetInterpolatedProgressValue.Invoke(_interpolatedLoadProgress);	
				}
			}
			else
			{
				SetInterpolatedProgressValue.Invoke(_loadProgress);	
			}
		}

		/// <summary>
		/// Loads the scene to load asynchronously.
		/// </summary>
		protected virtual IEnumerator LoadSequence()
		{
			_antiSpill?.PrepareAntiFill(_sceneToLoadName, _antiSpillSceneName);
			InitiateLoad();
			yield return ProcessDelayBeforeEntryFade();
			yield return EntryFade();
			yield return ProcessDelayAfterEntryFade();
			yield return UnloadOriginScenes();
			yield return LoadDestinationScene();
			yield return ProcessDelayBeforeExitFade();
			yield return DestinationSceneActivation();
			yield return ExitFade();
			yield return UnloadSceneLoader();
		}

		/// <summary>
		/// Initializes counters and timescale
		/// </summary>
		protected virtual void InitiateLoad()
		{
			_loadProgress = 0f;
			_interpolatedLoadProgress = 0f;
			Time.timeScale = 1f;
			SetAudioListener(false);
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : Initiate Load");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadStarted);
			OnLoadStarted?.Invoke();
		}

		/// <summary>
		/// Waits for the specified BeforeEntryFadeDelay duration
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ProcessDelayBeforeEntryFade()
		{
			if (_beforeEntryFadeDelay > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : delay before entry fade, duration : " + _beforeEntryFadeDelay);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.BeforeEntryFade);
				OnBeforeEntryFade?.Invoke();
				
				yield return MMCoroutine.WaitForUnscaled(_beforeEntryFadeDelay);
			}
		}

		/// <summary>
		/// Calls a fader on entry
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator EntryFade()
		{
			if (_entryFadeDuration > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : entry fade, duration : " + _entryFadeDuration);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.EntryFade);
				OnEntryFade?.Invoke();
				
				if (_fadeMode == FadeModes.FadeOutThenIn)
				{
					yield return null;
					MMFadeOutEvent.Trigger(_entryFadeDuration, _entryFadeTween, FaderID, true);
				}
				else
				{
					yield return null;
					MMFadeInEvent.Trigger(_entryFadeDuration, _entryFadeTween, FaderID, true);
				}           

				yield return MMCoroutine.WaitForUnscaled(_entryFadeDuration);
			}
		}

		/// <summary>
		/// Waits for the specified AfterEntryFadeDelay
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ProcessDelayAfterEntryFade()
		{
			if (_afterEntryFadeDelay > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : delay after entry fade, duration : " + _afterEntryFadeDelay);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.AfterEntryFade);
				OnAfterEntryFade?.Invoke();
				
				yield return MMCoroutine.WaitForUnscaled(_afterEntryFadeDelay);
			}
		}

		/// <summary>
		/// Unloads the original scene(s) and waits for the unload to complete
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator UnloadOriginScenes()
		{
			foreach (Scene scene in _initialScenes)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : unload scene " + scene.name);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.UnloadOriginScene);
				OnUnloadOriginScene?.Invoke();
				
				if (!scene.IsValid() || !scene.isLoaded)
				{
					Debug.LogWarning("MMLoadingSceneManagerAdditive : invalid scene : " + scene.name);
					continue;
				}
				
				_unloadOriginAsyncOperation = SceneManager.UnloadSceneAsync(scene);
				SetAudioListener(true);
				while (_unloadOriginAsyncOperation.progress < _asyncProgressLimit)
				{
					yield return null;
				}
			}
		}

		/// <summary>
		/// Loads the destination scene
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator LoadDestinationScene()
		{
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : load destination scene");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadDestinationScene);
			OnLoadDestinationScene?.Invoke();

			_loadDestinationAsyncOperation = SceneManager.LoadSceneAsync(_sceneToLoadName, LoadSceneMode.Additive );
			_loadDestinationAsyncOperation.completed += OnLoadOperationComplete;

			_loadDestinationAsyncOperation.allowSceneActivation = false;
            
			while (_loadDestinationAsyncOperation.progress < _asyncProgressLimit)
			{
				_loadProgress = _loadDestinationAsyncOperation.progress;
				yield return null;
			}
            
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : load progress complete");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadProgressComplete);
			OnLoadProgressComplete?.Invoke();

			// when the load is close to the end (it'll never reach it), we set it to 100%
			_loadProgress = 1f;

			// we wait for the bar to be visually filled to continue
			if (_interpolateProgress)
			{
				while (_interpolatedLoadProgress < 1f)
				{
					yield return null;
				}
			}			

			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : interpolated load complete");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.InterpolatedLoadProgressComplete);
			OnInterpolatedLoadProgressComplete?.Invoke();
		}

		/// <summary>
		/// Waits for BeforeExitFadeDelay seconds
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ProcessDelayBeforeExitFade()
		{
			if (_beforeExitFadeDelay > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : delay before exit fade, duration : " + _beforeExitFadeDelay);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.BeforeExitFade);
				OnBeforeExitFade?.Invoke();
				
				yield return MMCoroutine.WaitForUnscaled(_beforeExitFadeDelay);
			}
		}

		/// <summary>
		/// Requests a fade on exit
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ExitFade()
		{
			SetAudioListener(false);
			if (_exitFadeDuration > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : exit fade, duration : " + _exitFadeDuration);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.ExitFade);
				OnExitFade?.Invoke();
				
				if (_fadeMode == FadeModes.FadeOutThenIn)
				{
					MMFadeInEvent.Trigger(_exitFadeDuration, _exitFadeTween, FaderID, true);
				}
				else
				{
					MMFadeOutEvent.Trigger(_exitFadeDuration, _exitFadeTween, FaderID, true);
				}
				yield return MMCoroutine.WaitForUnscaled(_exitFadeDuration);
			}
		}

		/// <summary>
		/// Activates the destination scene
		/// </summary>
		protected virtual IEnumerator DestinationSceneActivation()
		{
			yield return MMCoroutine.WaitForFrames(1);
			_loadDestinationAsyncOperation.allowSceneActivation = true;
			while (_loadDestinationAsyncOperation.progress < 1.0f)
			{
				yield return null;
			}
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : activating destination scene");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.DestinationSceneActivation);
			OnDestinationSceneActivation?.Invoke();
		}

		/// <summary>
		/// A method triggered when the async operation completes
		/// </summary>
		/// <param name="obj"></param>
		protected virtual void OnLoadOperationComplete(AsyncOperation obj)
		{
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneToLoadName));
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : set active scene to " + _sceneToLoadName);

		}

		/// <summary>
		/// Unloads the scene loader
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator UnloadSceneLoader()
		{
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : unloading scene loader");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.UnloadSceneLoader);
			OnUnloadSceneLoader?.Invoke();
			
			yield return null; // mandatory yield to avoid an unjustified warning
			_unloadLoadingAsyncOperation = SceneManager.UnloadSceneAsync(_loadingScreenSceneName);
			while (_unloadLoadingAsyncOperation.progress < _asyncProgressLimit)
			{
				yield return null;
			}
		}

		/// <summary>
		/// Turns the loading audio listener on or off
		/// </summary>
		/// <param name="state"></param>
		protected virtual void SetAudioListener(bool state)
		{
			if (LoadingAudioListener != null)
			{
				//LoadingAudioListener.gameObject.SetActive(state);
			}
		}

		/// <summary>
		/// On Destroy we reset our state
		/// </summary>
		protected virtual void OnDestroy()
		{
			_loadingInProgress = false;
		}

		/// <summary>
		/// A debug method used to output console messages, for this class only
		/// </summary>
		/// <param name="message"></param>
		protected virtual void MMLoadingSceneDebug(string message)
		{
			if (!DebugMode)
			{
				return;
			}
			
			string output = "";
			output += "<color=#82d3f9>[" + Time.frameCount + "]</color> ";
			output += "<color=#f9a682>[" + MMTime.FloatToTimeString(Time.time, false, true, true, true) + "]</color> ";
			output +=  message;
			Debug.Log(output);
		}
	}
}
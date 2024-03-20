using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 선택한 방법을 사용하여 새 장면의 로드를 요청합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 선택한 방법을 사용하여 새 장면의 로드를 요청합니다.")]
	[FeedbackPath("Scene/Load Scene")]
	public class MMFeedbackLoadScene : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SceneColor; } }
#endif

        /// 새로운 장면을 로드하는 가능한 방법:
        /// - direct : Unity의 SceneManager API를 사용합니다.
        /// - MMSceneLoadingManager : 장면을 로딩하는 간단하고 독창적인 MM 방식
        /// - MMAdditiveSceneLoadingManager : 더 많은 옵션을 갖춘 고급 장면 로딩 방식
        public enum LoadingModes { Direct, MMSceneLoadingManager, MMAdditiveSceneLoadingManager }

		[Header("Scene Names")]
		/// the name of the loading screen scene to use
		[Tooltip("사용할 로딩 화면 장면의 이름은 빌드 설정에 추가되어야 합니다. Sy의 SceneManager API를 사용합니다.")]
		public string LoadingSceneName = "MMAdditiveLoadingScreen";
		/// the name of the destination scene
		[Tooltip("대상 장면의 이름 - 빌드 설정에 추가해야 합니다.")]
		public string DestinationSceneName = "";

		[Header("Mode")] 
		/// the loading mode to use
		[Tooltip("대상 장면을 로드하는 데 사용할 로딩 모드: " +
"- 직접: Unity의 SceneManager API를 사용합니다." +
"- MMSceneLoadingManager : 장면을 로드하는 간단하고 독창적인 MM 방식" +
"- MMAdditiveSceneLoadingManager: 더 많은 옵션을 갖춘 고급 장면 로드 방법")]
		public LoadingModes LoadingMode = LoadingModes.MMAdditiveSceneLoadingManager;
        
		[Header("Loading Scene Manager")]
		/// the priority to use when loading the new scenes
		[Tooltip("새 장면을 로드할 때 사용할 우선순위")]
		public ThreadPriority Priority = ThreadPriority.High;
		/// whether or not to interpolate progress (slower, but usually looks better and smoother)
		[Tooltip("진행 상황을 보간할지 여부(느리지만 일반적으로 더 좋고 부드러워 보입니다)")]
		public bool InterpolateProgress = true;
		/// whether or not to perform extra checks to make sure the loading screen and destination scene are in the build settings
		[Tooltip("로딩 화면과 대상 장면이 빌드 설정에 있는지 확인하기 위해 추가 검사를 수행할지 여부\r\n")]
		public bool SecureLoad = true;

		[Header("Loading Scene Delays")]
		/// a delay (in seconds) to apply before the first fade plays
		[Tooltip("첫 번째 페이드가 재생되기 전에 적용할 지연(초)")]
		public float BeforeEntryFadeDelay = 0f;
		/// the duration (in seconds) of the entry fade
		[Tooltip("항목 페이드의 지속 시간(초)")]
		public float EntryFadeDuration = 0.2f;
		/// a delay (in seconds) to apply after the first fade plays
		[Tooltip("첫 번째 페이드 재생 후 적용할 지연(초)")]
		public float AfterEntryFadeDelay = 0f;
		/// a delay (in seconds) to apply before the exit fade plays
		[Tooltip("종료 페이드가 재생되기 전에 적용할 지연(초)")]
		public float BeforeExitFadeDelay = 0f;
		/// the duration (in seconds) of the exit fade
		[Tooltip("출구 페이드의 지속 시간(초)")]
		public float ExitFadeDuration = 0.2f;
        
		[Header("Transitions")]
		/// the speed at which the progress bar should move if interpolated
		[Tooltip("보간된 경우 진행률 표시줄이 이동해야 하는 속도")]
		public float ProgressInterpolationSpeed = 5f;
		/// the order in which to play fades (really depends on the type of fader you have in your loading screen
		[Tooltip("페이드 재생 순서(실제로 로딩 화면에 있는 페이더 유형에 따라 다름)")]
		public MMAdditiveSceneLoadingManager.FadeModes FadeMode = MMAdditiveSceneLoadingManager.FadeModes.FadeInThenOut;
		/// the tween to use on the entry fade
		[Tooltip("항목 페이드에 사용할 트윈")]
		public MMTweenType EntryFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the tween to use on the exit fade
		[Tooltip("종료 페이드에 사용할 트윈")]
		public MMTweenType ExitFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));

		/// <summary>
		/// On play, we request a load of the destination scene using hte specified method
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			switch (LoadingMode)
			{
				case LoadingModes.Direct:
                    Debug.Log("SceneManager(유니티에서 제공하는) 를 이용하여 씬전환함");
                    SceneManager.LoadScene(DestinationSceneName);
					break;
				case LoadingModes.MMSceneLoadingManager:
                    Debug.Log("MMSceneLoadingManager 를 이용하여 씬전환함");
                    MMSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName);
					break;
				case LoadingModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName, 
						Priority, SecureLoad, InterpolateProgress, 
						BeforeEntryFadeDelay, EntryFadeDuration,
						AfterEntryFadeDelay,
						BeforeExitFadeDelay, ExitFadeDuration,
						EntryFadeTween, ExitFadeTween,
						ProgressInterpolationSpeed, FadeMode);
					break;
			}
		}
	}
}
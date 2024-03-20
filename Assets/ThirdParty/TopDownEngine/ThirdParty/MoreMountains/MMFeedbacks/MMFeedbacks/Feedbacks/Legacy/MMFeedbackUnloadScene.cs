using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 이름으로 장면을 언로드하거나 인덱스를 만들 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 이름으로 장면을 언로드하거나 인덱스를 만들 수 있습니다.")]
	[FeedbackPath("Scene/Unload Scene")]
	public class MMFeedbackUnloadScene : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum ColorModes { Instant, Gradient, Interpolate }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SceneColor; } }
		#endif
        
		public enum Methods { BuildIndex, SceneName }

		[Header("Unload Scene")] 
        
		/// whether to unload a scene by build index or by name
		[Tooltip("빌드 인덱스 또는 이름으로 장면을 언로드할지 여부")]
		public Methods Method = Methods.SceneName;

		/// the build ID of the scene to unload, find it in your Build Settings
		[Tooltip("언로드할 장면의 빌드 ID는 빌드 설정에서 찾으세요.")]
		[MMFEnumCondition("Method", (int)Methods.BuildIndex)]
		public int BuildIndex = 0;

		/// the name of the scene to unload
		[Tooltip("언로드할 장면의 이름")]
		[MMFEnumCondition("Method", (int)Methods.SceneName)]
		public string SceneName = "";

        
		/// whether or not to output warnings if the scene doesn't exist or can't be loaded
		[Tooltip("장면이 존재하지 않거나 로드할 수 없는 경우 경고를 출력할지 여부")]
		public bool OutputWarningsIfNeeded = true;
        
		protected Scene _sceneToUnload;

		/// <summary>
		/// On play we change the text of our target TMPText
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Method == Methods.BuildIndex)
			{
				_sceneToUnload = SceneManager.GetSceneByBuildIndex(BuildIndex);
			}
			else
			{
				_sceneToUnload = SceneManager.GetSceneByName(SceneName);
			}

			if ((_sceneToUnload != null) && (_sceneToUnload.isLoaded))
			{
				SceneManager.UnloadSceneAsync(_sceneToUnload);    
			}
			else
			{
				if (OutputWarningsIfNeeded)
				{
					Debug.LogWarning("Unload Scene Feedback : you're trying to unload a scene that hasn't been loaded.");    
				}
			}
		}
	}
}
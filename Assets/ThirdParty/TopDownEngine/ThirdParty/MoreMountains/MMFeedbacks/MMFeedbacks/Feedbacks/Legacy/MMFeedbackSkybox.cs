using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 재생 중인 장면의 스카이박스를 다른 스카이박스(특정 스카이박스 또는 여러 스카이박스 중에서 무작위로 선택한 스카이박스)로 변경할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 재생 중인 장면의 스카이박스를 다른 스카이박스(특정 스카이박스 또는 여러 스카이박스 중에서 무작위로 선택한 스카이박스)로 변경할 수 있습니다.")]
	[FeedbackPath("Renderer/Skybox")]
	public class MMFeedbackSkybox : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		#endif
        
		/// whether skyboxes are selected at random or not
		public enum Modes { Single, Random }

		[Header("Skybox")]
		/// the selected mode 
		public Modes Mode = Modes.Single;
		/// the skybox to assign when in Single mode
		public Material SingleSkybox;
		/// the skyboxes to pick from when in Random mode
		public Material[] RandomSkyboxes;
        
		/// <summary>
		/// On play, we set the scene's skybox to a new one
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (Mode == Modes.Single)
			{
				RenderSettings.skybox = SingleSkybox;
			}
			else if (Mode == Modes.Random)
			{
				RenderSettings.skybox = RandomSkyboxes[Random.Range(0, RandomSkyboxes.Length)];
			}
		}
	}    
}
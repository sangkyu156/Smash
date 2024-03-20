using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 대상 이미지의 RaycastTarget 매개변수를 제어하여 재생 시 켜거나 끌 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 대상 이미지의 RaycastTarget 매개변수를 제어하여 재생 시 켜거나 끌 수 있습니다.")]
	[FeedbackPath("UI/Image RaycastTarget")]
	public class MMFeedbackImageRaycastTarget : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif
        
		[Header("Image")]
		/// the target Image we want to control the RaycastTarget parameter on
		[Tooltip("RaycastTarget 매개변수를 제어하려는 대상 이미지")]
		public Image TargetImage;
		/// if this is true, when played, the target image will become a raycast target
		[Tooltip("이것이 사실이라면 재생 시 대상 이미지가 레이캐스트 대상이 됩니다.")]
		public bool ShouldBeRaycastTarget = true;
        
		/// <summary>
		/// On play we turn raycastTarget on or off
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetImage == null)
			{
				return;
			}

			TargetImage.raycastTarget = NormalPlayDirection ? ShouldBeRaycastTarget : !ShouldBeRaycastTarget;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
#if MM_CINEMACHINE
using Cinemachine;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	[AddComponentMenu("")]
	#if MM_CINEMACHINE
	[FeedbackPath("Camera/Cinemachine Impulse Clear")]
	#endif
	[FeedbackHelp("이 피드백을 통해 Cinemachine Impulse 클리어를 트리거하여 재생 중인 모든 임펄스를 즉시 중지할 수 있습니다.")]
	public class MMF_CinemachineImpulseClear : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			#if MM_CINEMACHINE
			CinemachineImpulseManager.Instance.Clear();
			#endif
		}
	}
}
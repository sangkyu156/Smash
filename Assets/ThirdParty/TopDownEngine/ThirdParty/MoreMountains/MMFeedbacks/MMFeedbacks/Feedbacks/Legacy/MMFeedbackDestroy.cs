using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 Destroy, DestroyImmediate 또는 SetActive:False를 통해 대상 게임 개체를 파괴할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 Destroy, DestroyImmediate 또는 SetActive:False를 통해 대상 게임 개체를 파괴할 수 있습니다.")]
	[FeedbackPath("GameObject/Destroy")]
	public class MMFeedbackDestroy : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		/// the possible ways to destroy an object
		public enum Modes { Destroy, DestroyImmediate, Disable }

		[Header("Destroy")]
		/// the gameobject we want to change the active state of
		[Tooltip("활성 상태를 변경하려는 게임 개체")]
		public GameObject TargetGameObject;
		/// the selected destruction mode 
		[Tooltip("선택한 파괴 모드")]
		public Modes Mode;

		/// <summary>
		/// On Play we change the state of our Behaviour if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetGameObject == null))
			{
				return;
			}
			ProceedWithDestruction(TargetGameObject);
		}
        
		/// <summary>
		/// Changes the status of the Behaviour
		/// </summary>
		/// <param name="state"></param>
		protected virtual void ProceedWithDestruction(GameObject go)
		{
			switch (Mode)
			{
				case Modes.Destroy:
					Destroy(go);
					break;
				case Modes.DestroyImmediate:
					DestroyImmediate(go);
					break;
				case Modes.Disable:
					go.SetActive(false);
					break;
			}
		}
	}
}
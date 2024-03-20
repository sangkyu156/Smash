using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 변환의 상위를 변경하는 데 사용되는 피드백
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 통해 변환의 상위를 변경할 수 있습니다.")]
	[FeedbackPath("Transform/Set Parent")]
	public class MMF_SetParent : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (ObjectToParent == null); }
		public override string RequiredTargetText { get { return ObjectToParent != null ? ObjectToParent.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires an ObjectToParent, that will be reparented to NewParent"; } } 
		#endif
		
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => ObjectToParent = FindAutomatedTarget<Transform>(); 

		[MMFInspectorGroup("Parenting", true, 12, true)]
		/// the object we want to change the parent of
		[Tooltip("부모를 변경하려는 객체")]
		public Transform ObjectToParent;
		/// the object ObjectToParent should now be parented to after playing this feedback
		[Tooltip("이제 이 피드백을 재생한 후 ObjectToParent 개체의 부모가 되어야 합니다.")]
		public Transform NewParent;
		/// if true, the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before
		[Tooltip("true인 경우 상위 상대 위치, 스케일 및 회전은 객체가 이전과 동일한 월드 공간 위치, 회전 및 스케일을 유지하도록 수정됩니다.")]
		public bool WorldPositionStays = true;

		/// <summary>
		/// On Play, changes the parent of the target transform
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (ObjectToParent == null)
			{
				Debug.LogWarning("No object to parent was set for " + Owner.name);
				return;
			}
			ObjectToParent.SetParent(NewParent, WorldPositionStays);
		}
	}
}
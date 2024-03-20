using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 기본적으로 아무 작업도 수행하지 않으며 단지 설명을 의미하며 나중에 참조할 수 있도록 텍스트를 저장할 수 있으며 특정 MMFeedback을 설정하는 방법을 기억할 수도 있습니다. 선택적으로 해당 설명을 Play 콘솔에 출력할 수도 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 기본적으로 아무 작업도 수행하지 않으며 단지 설명을 의미하며 나중에 참조할 수 있도록 텍스트를 저장할 수 있으며 특정 MMFeedback을 설정하는 방법을 기억할 수도 있습니다. 선택적으로 해당 설명을 Play 콘솔에 출력할 수도 있습니다.")]
	[FeedbackPath("Debug/Comment")]
	public class MMF_DebugComment : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
		#endif
     
		[MMFInspectorGroup("Comment", true, 61)]
		/// the comment / note associated to this feedback 
		[Tooltip("이 피드백과 관련된 댓글/메모")]
		[TextArea(10,30)] 
		public string Comment;

		/// if this is true, the comment will be output to the console on Play 
		[Tooltip("이것이 사실이라면 댓글은 Play의 콘솔에 출력됩니다.")]
		public bool LogComment = false;
		/// the color of the message when in DebugLogTime mode
		[Tooltip("DebugLogTime 모드일 때 메시지의 색상")]
		[MMCondition("LogComment", true)]
		public Color DebugColor = Color.gray;
        
		/// <summary>
		/// On Play we output our message to the console if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !LogComment)
			{
				return;
			}
            
			Debug.Log(Comment);
		}
	}
}
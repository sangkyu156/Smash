using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 사용자 정의 MM 디버그 방법이나 로그, 어설션, 오류 또는 경고 로그를 사용하여 콘솔에 메시지를 출력할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 사용자 정의 MM 디버그 방법이나 로그, 어설션, 오류 또는 경고 로그를 사용하여 콘솔에 메시지를 출력할 수 있습니다.")]
	[FeedbackPath("Debug/Log")]
	public class MMFeedbackDebugLog : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the duration of this feedback is 0
		public override float FeedbackDuration { get { return 0f; } }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
		#endif
        
		/// the possible debug modes
		public enum DebugLogModes { DebugLogTime, Log, Assertion, Error, Warning }

		[Header("Debug")] 
		/// the selected debug mode
		[Tooltip("선택한 디버그 모드")]
		public DebugLogModes DebugLogMode = DebugLogModes.DebugLogTime;

		/// the message to display 
		[Tooltip("표시할 메시지")]
		[TextArea] 
		public string DebugMessage;
		/// the color of the message when in DebugLogTime mode
		[Tooltip("DebugLogTime 모드일 때 메시지의 색상")]
		[MMFEnumCondition("DebugLogMode", (int) DebugLogModes.DebugLogTime)]
		public Color DebugColor = Color.cyan;
		/// whether or not to display the frame count when in DebugLogTime mode
		[Tooltip("DebugLogTime 모드에서 프레임 수를 표시할지 여부")]
		[MMFEnumCondition("DebugLogMode", (int) DebugLogModes.DebugLogTime)]
		public bool DisplayFrameCount = true;

		/// <summary>
		/// On Play we output our message to the console using the selected mode
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			switch (DebugLogMode)
			{
				case DebugLogModes.Assertion:
					Debug.LogAssertion(DebugMessage);
					break;
				case DebugLogModes.Log:
					Debug.Log(DebugMessage);
					break;
				case DebugLogModes.Error:
					Debug.LogError(DebugMessage);
					break;
				case DebugLogModes.Warning:
					Debug.LogWarning(DebugMessage);
					break;
				case DebugLogModes.DebugLogTime:
					string color = "#" + ColorUtility.ToHtmlStringRGB(DebugColor);
					MMDebug.DebugLogTime(DebugMessage, color, 3, DisplayFrameCount);
					break;
			}
		}
	}
}
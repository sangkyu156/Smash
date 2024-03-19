using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// MMF_PlayerDemo 및 MMFeedbacksDemo 장면에서 데모 버튼을 처리하는 데 사용되는 간단한 클래스
    /// </summary>
    [ExecuteAlways]
	public class DemoButton : MonoBehaviour
	{
		[Header("Behaviour")]
		public bool NotSupportedInWebGL = false;

		[Header("Bindings")]
		public Button TargetButton;
		public Text ButtonText;
		public Text WebGL;
		public MMF_Player TargetMMF_Player;
		public MMFeedbacks TargetMMFeedbacks;
		protected Color _disabledColor = new Color(255, 255, 255, 0.5f);
        
		//[Header("Debug")]
		//[MMInspectorButton("ConvertButtonToMMFPlayerDemo")]
		//public bool ConvertButtonToMMFPlayerDemoButton;
		
		protected virtual void OnEnable()
		{
			HandleWebGL();
		}

		protected virtual void ConvertButtonToMMFPlayerDemo()
		{
			#if UNITY_EDITOR
	        
			if (TargetMMF_Player != null)
			{
				TargetButton.onClick = new Button.ButtonClickedEvent();
				UnityAction action = new UnityAction(TargetMMF_Player.PlayFeedbacks);
				UnityEventTools.AddVoidPersistentListener(TargetButton.onClick, action);
				EditorUtility.SetDirty(TargetButton);
				PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject.transform);
			}
	        
			#endif
		}
        
		public void OnClickEvent()
		{
			TargetMMF_Player.PlayFeedbacks();
		}

		protected virtual void HandleWebGL()
		{
			if (WebGL != null)
			{
				#if UNITY_WEBGL
                TargetButton.interactable = !NotSupportedInWebGL;    
                    WebGL.gameObject.SetActive(NotSupportedInWebGL);   
                ButtonText.color = NotSupportedInWebGL ? _disabledColor : Color.white;
				#else
				WebGL.gameObject.SetActive(false);
				TargetButton.interactable = true;
				ButtonText.color = Color.white;
				#endif
			}
		}
	}
}
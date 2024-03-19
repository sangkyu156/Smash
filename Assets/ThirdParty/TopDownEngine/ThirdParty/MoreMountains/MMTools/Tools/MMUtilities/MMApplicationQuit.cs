using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 매우 간단한 모노를 개체에 추가하여 Quit 메서드를 호출하면 응용 프로그램이 강제로 종료됩니다.
    /// </summary>
    public class MMApplicationQuit : MonoBehaviour
	{
		[Header("Debug")]
		[MMInspectorButton("Quit")] 
		public bool QuitButton;
        
		/// <summary>
		/// Forces the application to quit
		/// </summary>
		public virtual void Quit()
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
                Application.Quit();
			#endif
		}
	}
}
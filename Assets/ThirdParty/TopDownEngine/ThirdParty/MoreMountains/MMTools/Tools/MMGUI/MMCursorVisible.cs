using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 객체에 추가하면 커서가 보이거나 보이지 않게 됩니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/GUI/MMCursorVisible")]
	public class MMCursorVisible : MonoBehaviour
	{
		/// The possible states of the cursor
		public enum CursorVisibilities { Visible, Invisible }
		/// Whether that cursor should be visible or invisible
		public CursorVisibilities CursorVisibility = CursorVisibilities.Visible;

		/// <summary>
		/// On Update we change the status of our cursor accordingly
		/// </summary>
		protected virtual void Update()
		{
			if (CursorVisibility == CursorVisibilities.Visible)
			{
				Cursor.visible = true;
			}
			else
			{
				Cursor.visible = false;
			}
		}
	}
}
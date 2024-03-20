using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 충돌체에 연결하면 사용자가 충돌체를 클릭/드래그/입력/등할 때 이벤트를 트리거할 수 있습니다.
    /// </summary>
    public class MMOnMouse : MonoBehaviour
	{
		/// OnMouseDown is called when the user has pressed the mouse button while over the Collider.
		[Tooltip("OnMouseDown은 사용자가 Collider 위에서 마우스 버튼을 눌렀을 때 호출됩니다.")]
		public UnityEvent OnMouseDownEvent;
		/// OnMouseDrag is called when the user has clicked on a Collider and is still holding down the mouse.
		[Tooltip("OnMouseDrag는 사용자가 Collider를 클릭하고 여전히 마우스를 누르고 있을 때 호출됩니다.")]
		public UnityEvent OnMouseDragEvent;
		/// Called when the mouse enters the Collider.
		[Tooltip("마우스가 Collider에 들어갈 때 호출됩니다.")]
		public UnityEvent OnMouseEnterEvent;
		/// Called when the mouse is not any longer over the Collider.
		[Tooltip("마우스가 더 이상 Collider 위에 있지 않을 때 호출됩니다.")]
		public UnityEvent OnMouseExitEvent;
		/// Called every frame while the mouse is over the Collider.
		[Tooltip("마우스가 Collider 위에 있는 동안 매 프레임마다 호출됩니다.")]
		public UnityEvent OnMouseOverEvent;
		/// OnMouseUp is called when the user has released the mouse button.
		[Tooltip("OnMouseUp은 사용자가 마우스 버튼을 놓을 때 호출됩니다.")]
		public UnityEvent OnMouseUpEvent;
		/// OnMouseUpAsButton is only called when the mouse is released over the same Collider as it was pressed.
		[Tooltip("OnMouseUpAsButton은 마우스를 눌렀을 때와 동일한 Collider 위에서 마우스를 놓을 때만 호출됩니다.")]
		public UnityEvent OnMouseUpAsButtonEvent;

		protected virtual void OnMouseDown()
		{
			OnMouseDownEvent.Invoke();
		}
		
		protected virtual void OnMouseDrag()
		{
			OnMouseDragEvent.Invoke();
		}
		
		protected virtual void OnMouseEnter()
		{
			OnMouseEnterEvent.Invoke();
		}
		
		protected virtual void OnMouseExit()
		{
			OnMouseExitEvent.Invoke();
		}
		
		protected virtual void OnMouseOver()
		{
			OnMouseOverEvent.Invoke();
		}
		
		protected virtual void OnMouseUp()
		{
			OnMouseUpEvent.Invoke();
		}
		
		protected virtual void OnMouseUpAsButton()
		{
			OnMouseUpAsButtonEvent.Invoke();
		}
		
	}	
}
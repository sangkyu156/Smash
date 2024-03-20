using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 사용하면 UI 개체가 마우스 위치를 따르도록 할 수 있습니다.
    /// </summary>
    public class MMUIFollowMouse : MonoBehaviour
	{
		public Canvas TargetCanvas { get; set; }
		protected Vector2 _newPosition;
		protected Vector2 _mousePosition;
        
		protected virtual void LateUpdate()
		{
			#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
			_mousePosition = Input.mousePosition;
			#else
			_mousePosition = Mouse.current.position.ReadValue();
			#endif
			RectTransformUtility.ScreenPointToLocalPointInRectangle(TargetCanvas.transform as RectTransform, _mousePosition, TargetCanvas.worldCamera, out _newPosition);
			transform.position = TargetCanvas.transform.TransformPoint(_newPosition);
		}
	}
}
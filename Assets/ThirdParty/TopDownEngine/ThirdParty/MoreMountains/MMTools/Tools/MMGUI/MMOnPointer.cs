using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Unity의 포인터 이벤트에 대한 메서드를 트리거하는 데 사용할 수 있는 간단한 도우미 클래스
    /// 일반적으로 UI 이미지에 사용됩니다.
    /// </summary>
    public class MMOnPointer : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
	{
		[Header("Pointer movement")]
		/// an event to trigger when the pointer enters the associated game object
		[Tooltip("포인터가 관련 게임 개체에 들어갈 때 트리거되는 이벤트")]
		public UnityEvent PointerEnter;
		/// an event to trigger when the pointer exits the associated game object
		[Tooltip("포인터가 관련 게임 개체를 종료할 때 트리거되는 이벤트")]
		public UnityEvent PointerExit;
		
		[Header("Clicks")]
		/// an event to trigger when the pointer is pressed down on the associated game object
		[Tooltip("관련 게임 개체에 포인터를 눌렀을 때 트리거되는 이벤트")]
		public UnityEvent PointerDown;
		/// an event to trigger when the pointer is pressed up on the associated game object
		[Tooltip("관련 게임 개체에서 포인터를 위로 눌렀을 때 트리거되는 이벤트")]
		public UnityEvent PointerUp;
		/// an event to trigger when the pointer is clicked on the associated game object
		[Tooltip("연결된 게임 개체에서 포인터를 클릭할 때 트리거되는 이벤트")]
		public UnityEvent PointerClick;
		
		/// <summary>
		/// IPointerEnterHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerEnter(PointerEventData eventData)
		{
			PointerEnter?.Invoke();
		}

		/// <summary>
		/// IPointerExitHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerExit(PointerEventData eventData)
		{
			PointerExit?.Invoke();
		}
		
		/// <summary>
		/// IPointerDownHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerDown(PointerEventData eventData)
		{
			PointerDown?.Invoke();
		}

		/// <summary>
		/// IPointerUpHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerUp(PointerEventData eventData)
		{
			PointerUp?.Invoke();
		}

		/// <summary>
		/// IPointerClickHandler implementation
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerClick(PointerEventData eventData)
		{
			PointerClick?.Invoke();
		}
	}
}

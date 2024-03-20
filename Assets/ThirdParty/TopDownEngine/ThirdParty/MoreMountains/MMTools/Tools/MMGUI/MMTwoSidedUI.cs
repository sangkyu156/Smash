using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 사용하면 "양면" UI 요소(두 개의 개별적이고 일반적으로 정렬된 개체로 구성되어 컨테이너의 크기가 특정 임계값을 초과/미달할 때마다 효과적으로 각 측면을 켜거나 끄는)를 뒤집을 축을 정의할 수 있습니다.
    /// </summary>
    [ExecuteAlways]
	public class MMTwoSidedUI : MonoBehaviour
	{
		/// the possible axis on which to flip the double object
		public enum Axis { x, y, z }
        
		[Header("Bindings")]
		/// the object to consider as the "front" of the two sided element. Will be visible if the scale is above the threshold
		public GameObject Front;
		/// the object to consider as the "back" of the two sided element. Will be visible if the scale is below the threshold
		public GameObject Back;
        
		[Header("Axis")]
		/// the axis on which to flip this object
		public Axis FlipAxis;
		/// the scale threshold at which the flip should occur
		public float ScaleThreshold = 0f;

		[Header("Events")]
		/// an event to invoke on flip
		public UnityEvent OnFlip;

		[Header("Debug")] 
		/// whether or not we're in debug mode
		public bool DebugMode;
		/// the value to apply to the scale when in debug mode
		[Range(-1f, 1f)] 
		public float ScaleValue;
		/// whether or not our object is flipped right now
		[MMReadOnly] 
		public bool BackVisible = false;

		protected RectTransform _rectTransform;
		protected bool _initialized = false;

		/// <summary>
		/// On Start we initialize our object
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init we grab our rect transform and initialize visibility
		/// </summary>
		protected virtual void Initialization()
		{
			_rectTransform = this.gameObject.GetComponent<RectTransform>();
			_initialized = true;
            
			float axis = GetScaleValue();
			BackVisible = (axis < ScaleThreshold);
            
			Front.SetActive(!BackVisible);
			Back.SetActive(BackVisible);
		}
        
		/// <summary>
		/// On Update we update visibility if needed
		/// </summary>
		protected virtual void Update()
		{
			#if UNITY_EDITOR
			IfEditor();
			#endif
            
			float axis = GetScaleValue();

			if ((axis < ScaleThreshold) != BackVisible)
			{
				Front.SetActive(BackVisible);
				Back.SetActive(!BackVisible);
				OnFlip?.Invoke();
			}
			BackVisible = (axis < ScaleThreshold);
		}

		/// <summary>
		/// If in editor, we initialize if needed, and apply the debug scale value if needed
		/// </summary>
		protected virtual void IfEditor()
		{
			if (!_initialized)
			{
				Initialization();
			}

			if (DebugMode)
			{
				switch (FlipAxis)
				{
					case Axis.x:
						_rectTransform.localScale = new Vector3(ScaleValue, _rectTransform.localScale.y, _rectTransform.localScale.z);
						break;
					case Axis.y:
						_rectTransform.localScale = new Vector3(_rectTransform.localScale.x, ScaleValue, _rectTransform.localScale.z);
						break;
					case Axis.z:
						_rectTransform.localScale = new Vector3(_rectTransform.localScale.x, _rectTransform.localScale.y, ScaleValue);
						break;
				}
			}
		}

		/// <summary>
		/// Returns the scale of the selected axis
		/// </summary>
		/// <returns></returns>
		protected virtual float GetScaleValue()
		{
			switch (FlipAxis)
			{
				case Axis.x:
					return _rectTransform.localScale.x;
				case Axis.y:
					return _rectTransform.localScale.y;
				case Axis.z:
					return _rectTransform.localScale.z;
			}

			return 0f;
		}
	}
}
using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
	using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 사용하여 여러 UI 레이어를 마우스 커서 또는 모바일 장치 자이로스코프의 움직임에 바인딩하거나 다른 스크립트로 조종하도록 할 수도 있습니다.
    /// 각 UI 레이어에 대해 서로 다른 속도/진폭 값을 설정하면 멋진 시차 효과를 만들 수 있습니다.
    /// </summary>
    public class MMParallaxUI : MonoBehaviour
	{
		/// <summary>
		/// A class used to store layer settings
		/// </summary>
		[Serializable]
		public class ParallaxLayer
		{
			/// the rect transform for this layer   
			public RectTransform Rect;
			/// the speed at which this layer should move
			public float Speed = 2f;
			/// the maximum distance this layer can travel from its starting position
			public float Amplitude = 50f;
			/// the starting position for this layer
			[HideInInspector] 
			public Vector2 StartPosition;
			/// if this is false, this layer won't move
			public bool Active = true;
		}
        
		/// the possible modes used to pilot this parallax rig
		public enum Modes { Mouse, Gyroscope, Script }
		/// the selected mode for this parallax setup. note that gyroscope mode is only available on mobile devices
		public Modes Mode = Modes.Mouse;
		/// a multiplier to apply to all layers' amplitudes
		public float AmplitudeMultiplier = 1f;
		/// a speed multiplier to apply to all layers' speeds
		public float SpeedMultiplier = 1f;
		/// a list of all the layers to pilot
		public List<ParallaxLayer> ParallaxLayers;
        
		protected Vector2 _referencePosition;
		protected Vector3 _newPosition;
		protected Vector2 _mousePosition;
        
		/// <summary>
		/// On Start we initialize our reference position
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Initializes the start position of all layers
		/// </summary>
		public virtual void Initialization()
		{
			foreach (ParallaxLayer layer in ParallaxLayers)
			{
				layer.StartPosition = layer.Rect.position;
			}
		}
        
		/// <summary>
		/// On Update, moves all layers according to the selected mode
		/// </summary>
		protected virtual void Update()
		{
			MoveLayers();
		}

		/// <summary>
		/// Computes the input data according to the selected mode, and moves the layers accordingly
		/// </summary>
		protected virtual void MoveLayers()
		{
			switch (Mode)
			{
				case Modes.Gyroscope:
					_referencePosition = MMGyroscope.CalibratedInputAcceleration; 
					break;
				case Modes.Mouse:
					#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
						_mousePosition = Mouse.current.position.ReadValue();
					#else
					_mousePosition = Input.mousePosition;
					#endif
					_referencePosition = Camera.main.ScreenToViewportPoint(_mousePosition);
					break;
			}
            
			foreach (ParallaxLayer layer in ParallaxLayers)
			{
				if (layer.Active)
				{
					_newPosition.x = Mathf.Lerp(layer.Rect.position.x, layer.StartPosition.x + _referencePosition.x * layer.Amplitude * AmplitudeMultiplier, layer.Speed * SpeedMultiplier * Time.deltaTime);
					_newPosition.y = Mathf.Lerp(layer.Rect.position.y, layer.StartPosition.y + _referencePosition.y * layer.Amplitude * AmplitudeMultiplier, layer.Speed * SpeedMultiplier * Time.deltaTime);
					_newPosition.z = 0;

					layer.Rect.position = _newPosition;    
				}
			}
		}
        
		/// <summary>
		/// Sets a new reference position, to use when in Script mode
		/// </summary>
		/// <param name="newReferencePosition"></param>
		public virtual void SetReferencePosition(Vector3 newReferencePosition)
		{
			_referencePosition = newReferencePosition;
		}
	}   
}
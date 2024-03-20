using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	[RequireComponent(typeof(LineRenderer))]
	public class MMLineRendererCircle : MonoBehaviour
	{
		/// the possible axis 
		public enum DrawAxis { X, Y, Z };
     
		[Header("Draw Axis")]
		/// the axis on which to draw the circle
		[Tooltip("원을 그릴 축")]
		public DrawAxis Axis = DrawAxis.Z;
		/// the distance by which to push the circle on the draw axis
		[Tooltip("그리기 축에서 원을 밀어야 하는 거리")]
		public float NormalOffset = 0;
        
		[Header("Geometry")]
		/// the amount of segments on the line renderer. More segments, more smoothness, more performance cost
		[Tooltip("라인 렌더러의 세그먼트 양. 더 많은 세그먼트, 더 많은 부드러움, 더 많은 성능 비용")]
		[Range(0, 2000)]
		public int PositionsCount = 60;
     
		[Header("Shape")]
		/// the length of the circle's horizontal radius
		[Tooltip("원의 수평 반지름의 길이")]
		public float HorizontalRadius = 10;
		/// the length of the circle's vertical radius
		[Tooltip("원의 수직 반지름의 길이")]
		public float VerticalRadius = 10;

		[Header("Debug")]
		/// if this is true, the circle will be redrawn every time you change a value in the inspector, otherwise you'll have to call the DrawCircle method (or press the debug button below)
		[Tooltip("이것이 사실이라면 인스펙터에서 값을 변경할 때마다 원이 다시 그려질 것입니다. 그렇지 않으면 DrawCircle 메소드를 호출해야 합니다(또는 아래 디버그 버튼을 눌러야 합니다).")]
		public bool AutoRedrawOnValuesChange = false;
		/// a test button used to call the DrawCircle method
		[MMInspectorButton("DrawCircle")]
		public bool DrawCircleButton;
        
		protected LineRenderer _line;
		protected Vector3 _newPosition;
		protected float _angle, _x, _y, _z;
     
		/// <summary>
		/// On Awake we initialize our line renderer and draw our circle
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			DrawCircle();
		}

		/// <summary>
		/// Grabs the line renderer and sets it up
		/// </summary>
		protected virtual void Initialization()
		{
			_line = gameObject.GetComponent<LineRenderer>();
			_line.positionCount = PositionsCount + 1;
			_line.useWorldSpace = false;
		}
     
		/// <summary>
		/// Sets all point positions for our line renderer
		/// </summary>
		public virtual void DrawCircle()
		{
			_angle = 0f;
			_z = NormalOffset;
            
			switch(Axis)
			{
				case DrawAxis.X: 
					DrawCircleX();
					break;
				case DrawAxis.Y: 
					DrawCircleY();
					break;
				case DrawAxis.Z: 
					DrawCircleZ();
					break;
			}
		}

		/// <summary>
		/// Computes the x position of the new point
		/// </summary>
		/// <returns></returns>
		protected virtual float ComputeX()
		{
			return Mathf.Cos (Mathf.Deg2Rad * _angle) * HorizontalRadius;
		}

		/// <summary>
		/// Computes the y position of the new point
		/// </summary>
		/// <returns></returns>
		protected virtual float ComputeY()
		{
			return Mathf.Sin (Mathf.Deg2Rad * _angle) * VerticalRadius;
		}

		/// <summary>
		/// Draws a circle on the x axis
		/// </summary>
		protected virtual void DrawCircleX()
		{
			for (int i = 0; i < (PositionsCount + 1); i++)
			{
				_x = ComputeX();
				_y = ComputeY();
                
				_newPosition.x = _z;
				_newPosition.y = _y;
				_newPosition.z = _x;
				_line.SetPosition(i, _newPosition);
     
				_angle += (360f / PositionsCount);
			}
		}

		/// <summary>
		/// Draws a circle on the y axis
		/// </summary>
		protected virtual void DrawCircleY()
		{
			for (int i = 0; i < (PositionsCount + 1); i++)
			{
				_x = ComputeX();
				_y = ComputeY();
                
				_newPosition.x = _y;
				_newPosition.y = _z;
				_newPosition.z = _x;
				_line.SetPosition(i, _newPosition);
     
				_angle += (360f / PositionsCount);
			}
		}

		/// <summary>
		/// Draws a circle on the z axis
		/// </summary>
		protected virtual void DrawCircleZ()
		{
			for (int i = 0; i < (PositionsCount + 1); i++)
			{
				_x = ComputeX();
				_y = ComputeY();
                
				_newPosition.x = _x;
				_newPosition.y = _y;
				_newPosition.z = _z;
				_line.SetPosition(i, _newPosition);
     
				_angle += (360f / PositionsCount);
			}
		}

		/// <summary>
		/// On Validate we redraw our circle if needed
		/// </summary>
		protected virtual void OnValidate()
		{
			if (AutoRedrawOnValuesChange)
			{
				DrawCircle();
			}
		}
	}
}
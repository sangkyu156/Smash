using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 비율 표시 정보를 저장하는 클래스
    /// </summary>
    [Serializable]    
	public class Ratio
	{
		/// whether or not that ratio should be drawn
		public bool DrawRatio = true;
		/// the ratio's size (4:3, 16:9, etc)
		public Vector2 Size;
		/// the color of the handle to draw
		public Color RatioColor;

		public Ratio(bool drawRatio, Vector2 size, Color ratioColor)
		{
			DrawRatio = drawRatio;
			Size = size;
			RatioColor = ratioColor;
		}
	}

    /// <summary>
    ///인스펙터에서 설정된 다양한 비율에 대한 안전 영역의 자동 표시를 처리하는 클래스
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Camera/MMAspectRatioSafeZones")]
	public class MMAspectRatioSafeZones : MonoBehaviour
	{
		[Header("Center")]
		/// whether or not to draw the center crosshair
		public bool DrawCenterCrosshair = true;
		/// the size of the center crosshair
		public float CenterCrosshairSize = 1f;
		/// the color of the center crosshair
		public Color CenterCrosshairColor = MMColors.Wheat;

		[Header("Ratios")]
		/// whether or not to draw any ratio
		public bool DrawRatios = true;
		/// the size of the projected ratios
		public float CameraSize = 5f;
		/// the opacity to apply to the dead zones
		public float UnsafeZonesOpacity = 0.2f;
		/// the list of ratios to draw
		public List<Ratio> Ratios;

		[MMInspectorButton("AutoSetup")]
		public bool AutoSetupButton;

		public virtual void AutoSetup()
		{
			Ratios.Clear();
			Ratios.Add(new Ratio(true, new Vector2(16, 9), MMColors.DeepSkyBlue));
			Ratios.Add(new Ratio(true, new Vector2(16, 10), MMColors.GreenYellow));
			Ratios.Add(new Ratio(true, new Vector2(4, 3), MMColors.HotPink));
		}
	}
}
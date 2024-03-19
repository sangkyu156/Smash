using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 안개 속성을 저장하는 데 사용되는 간단한 클래스
    /// </summary>
    [Serializable]
	public class FogSettings
	{
		public bool FogEnabled = true;
		public Color FogColor = Color.white;
		public float FogDensity = 0.01f;
		public UnityEngine.FogMode FogMode = FogMode.ExponentialSquared;
	}

    /// <summary>
    /// 이 클래스를 카메라에 추가하면 활성화되면 안개 설정이 재정의됩니다.
    /// </summary>
    [ExecuteAlways]
	public class MMCameraFog : MonoBehaviour
	{
		/// the settings to use to override fog settings 
		public FogSettings Settings;

		protected FogSettings _previousSettings;

		protected void Awake()
		{
			_previousSettings = new FogSettings();
		}

		/// <summary>
		/// On pre render we store our current fog settings and override them
		/// </summary>
		protected virtual void OnPreRender()
		{
			_previousSettings.FogEnabled = RenderSettings.fog;
			_previousSettings.FogColor = RenderSettings.fogColor;
			_previousSettings.FogDensity = RenderSettings.fogDensity;
			_previousSettings.FogMode = RenderSettings.fogMode;

			RenderSettings.fog = Settings.FogEnabled;
			RenderSettings.fogColor = Settings.FogColor;
			RenderSettings.fogDensity = Settings.FogDensity;
			RenderSettings.fogMode = Settings.FogMode;
		}

		/// <summary>
		/// On post render we restore fog settings
		/// </summary>
		protected virtual void OnPostRender()
		{
			RenderSettings.fog = _previousSettings.FogEnabled;
			RenderSettings.fogColor = _previousSettings.FogColor;
			RenderSettings.fogDensity = _previousSettings.FogDensity;
			RenderSettings.fogMode = _previousSettings.FogMode;
		}
	}
}
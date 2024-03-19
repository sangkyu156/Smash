using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 빛의 세기를 조절하는데 사용되는 클래스
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Property Controllers/LightController")]
	public class LightController : MonoBehaviour
	{
		[Header("Binding")]
		[MMInformation("런타임에 하나 이상의 조명 속성을 제어하려면 이 구성요소를 사용합니다. FloatController와 잘 작동합니다. " +
                       "이 구성 요소는 이 객체에 Light 구성 요소가 있는 경우 TargetLight를 자동 설정하려고 시도합니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// the light to control 
		public Light TargetLight;
		/// the lights to control
		public List<Light> TargetLights;

		[Header("Light Settings")]
		/// the new intensity
		public float Intensity = 1f;
		/// the multiplier to apply
		public float Multiplier = 1f;
		/// the new range
		public float Range = 1f;

		[Header("Color")]
		/// the new color
		public Color LightColor;
        
		/// <summary>
		/// On Start, we initialize our light
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs the light, sets initial range and color
		/// </summary>
		protected virtual void Initialization()
		{
			if (TargetLight == null)
			{
				TargetLight = this.gameObject.GetComponent<Light>();
			}

			if (TargetLight != null)
			{
				TargetLight.range = Range;
				TargetLight.color = LightColor;
			}

			if (TargetLights.Count > 0)
			{
				foreach (Light light in TargetLights)
				{
					if (light != null)
					{
						light.range = Range;
						light.color = LightColor;
					}
				}
			}
		}

		/// <summary>
		/// On Update we apply our light settings
		/// </summary>
		protected virtual void Update()
		{
			ApplyLightSettings();           
		}

		/// <summary>
		/// Applys the new intensity, range and color to the light
		/// </summary>
		protected virtual void ApplyLightSettings()
		{
			if (TargetLight != null)
			{
				TargetLight.intensity = Intensity * Multiplier;
				TargetLight.range = Range;
				TargetLight.color = LightColor;
			}

			if (TargetLights.Count > 0)
			{
				foreach (Light light in TargetLights)
				{
					if (light != null)
					{
						light.intensity = Intensity * Multiplier;
						light.range = Range;
						light.color = LightColor;
					}
				}
			}
		}
	}
}
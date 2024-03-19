using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	[ExecuteAlways]
    /// <summary>
    /// 이 클래스를 UnityStandardAssets.ImageEffects.GlobalFog에 추가하여 색상을 변경합니다.
    /// 왜 이것이 네이티브가 아닌지 모르겠습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Particles/MMChangeFogColor")]
	public class MMChangeFogColor : MonoBehaviour 
	{
		/// Adds this class to a UnityStandardAssets.ImageEffects.GlobalFog to change its color
		[MMInformation("이 클래스를 UnityStandardAssets.ImageEffects.GlobalFog에 추가하여 색상을 변경합니다.", MMInformationAttribute.InformationType.Info,false)]
		public Color FogColor;

		/// <summary>
		/// Sets the fog's color to the one set in the inspector
		/// </summary>
		protected virtual void SetupFogColor () 
		{
			RenderSettings.fogColor = FogColor;
			RenderSettings.fog = true;
		}

		/// <summary>
		/// On Start(), we set the fog's color
		/// </summary>
		protected virtual void Start()
		{
			SetupFogColor();
		}

		/// <summary>
		/// Whenever there's a change in the camera's inspector, we change the fog's color
		/// </summary>
		protected virtual void OnValidate()
		{
			SetupFogColor();
		}
	}
}
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 플랫폼에 추가하고 그 위를 걷는 모든 TopDownController에 적용될 새로운 마찰 또는 힘을 정의합니다.
    /// TODO, 아직 작업이 진행 중입니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Surface Modifier")]
	public class SurfaceModifier : TopDownMonoBehaviour 
	{
		[Header("Friction")]
		[MMInformation("미끄러운 표면을 얻으려면 마찰력을 0.01에서 0.99 사이로 설정하세요(0에 가까울수록 매우 미끄럽고 1에 가까울수록 덜 미끄럽습니다).\n또는 끈적한 표면을 얻으려면 1보다 높게 설정하세요. 값이 높을수록 표면이 더 끈적해집니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the amount of friction to apply to a TopDownController walking over this surface		
		[Tooltip("the amount of friction to apply to a TopDownController walking over this surface")]
		public float Friction;

		[Header("Force")]
		[MMInformation("이를 사용하여 이 표면에 접지된 TopDownController에 X 또는 Y(또는 둘 다) 힘을 추가합니다. X 힘을 추가하면 런닝머신이 생성됩니다(음수 값 > 런닝머신은 왼쪽, 양수 ​​값 > 런닝머신은 오른쪽). 예를 들어 양수 y 값은 트램폴린, 탄력 있는 표면 또는 점퍼를 생성합니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the amount of force to add to a TopDownController walking over this surface
		[Tooltip("the amount of force to add to a TopDownController walking over this surface")]
		public Vector3 AddedForce=Vector3.zero;

		/// <summary>
		/// Triggered when a TopDownController collides with the surface
		/// </summary>
		/// <param name="collider">Collider.</param>
		/*public virtual void OnTriggerStay2D(Collider2D collider)
		{
			TODO
		}*/
	}
}
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 무기에 추가하면 장애물에 가까울 때 사격을 방지합니다(ObstacleLayerMask에 의해 정의됨).
    /// </summary>
    [RequireComponent(typeof(Weapon))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Prevent Shooting when Close to Walls 3D")]
	public class WeaponPreventShootingWhenCloseToWalls3D : WeaponPreventShooting
	{
		[Header("Raycast Settings")]
		/// the angle to consider when deciding whether or not there's a wall in front of the weapon (usually 5 degrees is fine)
		[Tooltip("무기 앞에 벽이 있는지 없는지 판단할 때 고려해야 할 각도(보통 5도 정도가 적당함)")]
		public float Angle = 5f;
		/// the max distance to the wall we want to prevent shooting from
		[Tooltip("총격을 방지하려는 벽까지의 최대 거리")]
		public float Distance = 2f;
		/// the offset to apply to the detection (in addition and relative to the weapon's position)
		[Tooltip("탐지에 적용할 오프셋(무기 위치에 추가로)")]
		public Vector3 RaycastOriginOffset = Vector3.zero;
		/// the layers to consider as obstacles
		[Tooltip("장애물로 간주되는 레이어")]
		public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;

		protected RaycastHit _hitLeft;
		protected RaycastHit _hitMiddle;
		protected RaycastHit _hitRight;
   

		/// <summary>
		/// Casts rays in front of the weapon to check for obstacles
		/// Returns true if an obstacle was found
		/// </summary>
		/// <returns></returns>
		protected virtual bool CheckForObstacles()
		{
			_hitLeft = MMDebug.Raycast3D(this.transform.position + this.transform.rotation * RaycastOriginOffset, Quaternion.Euler(0f, -Angle/2f, 0f) * this.transform.forward, Distance, ObstacleLayerMask, Color.yellow, true);
			_hitMiddle = MMDebug.Raycast3D(this.transform.position + this.transform.rotation * RaycastOriginOffset, this.transform.forward, Distance, ObstacleLayerMask, Color.yellow, true);
			_hitRight = MMDebug.Raycast3D(this.transform.position + this.transform.rotation * RaycastOriginOffset, Quaternion.Euler(0f, Angle / 2f, 0f) * this.transform.forward, Distance, ObstacleLayerMask, Color.yellow, true);

			if ((_hitLeft.collider == null) && (_hitMiddle.collider == null) && (_hitRight.collider == null))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Shooting is allowed if no obstacle is in front of the weapon
		/// </summary>
		/// <returns></returns>
		public override bool ShootingAllowed()
		{
			return !CheckForObstacles();
		}
	}
}
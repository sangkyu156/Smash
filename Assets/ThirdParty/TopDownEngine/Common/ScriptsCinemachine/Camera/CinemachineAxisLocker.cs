using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// CinemachineExtension을 사용하면 하나 이상의 축에서 Cinemachine을 잠글 수 있습니다.
    /// </summary>
    [ExecuteInEditMode]
	[SaveDuringPlay]
	[AddComponentMenu("")]
	public class CinemachineAxisLocker : CinemachineExtension
	{
		/// the possible methods to lock axis on
		public enum Methods { ForcedPosition, InitialPosition, ColliderBoundsCenter, Collider2DBoundsCenter }
		/// whether or not axis should be locked on X
		[Tooltip("축을 X에 고정해야 하는지 여부")]
		public bool LockXAxis = false;
		/// whether or not axis should be locked on Y
		[Tooltip("축을 Y에 고정해야 하는지 여부")]
		public bool LockYAxis = false;
		/// whether or not axis should be locked on Z
		[Tooltip("축을 Z에 고정해야 하는지 여부")]
		public bool LockZAxis = false;
		/// the selected method to lock axis on 
		[Tooltip("축을 잠그기 위해 선택한 방법")]
		public Methods Method = Methods.InitialPosition;
		/// the position to lock axis based on
		[MMEnumCondition("Method", (int)Methods.ForcedPosition)]
		[Tooltip("축을 잠글 위치는 다음을 기준으로 합니다.")]
		public Vector3 ForcedPosition;
		/// the collider to lock axis on
		[MMEnumCondition("Method", (int)Methods.ColliderBoundsCenter)]
		[Tooltip("축을 잠글 충돌기")]
		public Collider TargetCollider;
		/// the 2D collider to lock axis on
		[MMEnumCondition("Method", (int)Methods.Collider2DBoundsCenter)]
		[Tooltip("축을 고정할 2D 충돌기")]
		public Collider2D TargetCollider2D;

		protected Vector3 _forcedPosition;

		/// <summary>
		/// On Start we initialize our forced position based on the selected choice
		/// </summary>
		protected virtual void Start()
		{
			switch (Method)
			{
				case Methods.ForcedPosition:
					_forcedPosition = ForcedPosition;
					break;

				case Methods.InitialPosition:
					_forcedPosition = this.transform.position;
					break;

				case Methods.ColliderBoundsCenter:
					_forcedPosition = TargetCollider.bounds.center;
					break;

				case Methods.Collider2DBoundsCenter:
					_forcedPosition = TargetCollider2D.bounds.center + (Vector3)TargetCollider2D.offset;
					break;
			}
		}

		/// <summary>
		/// Locks position
		/// </summary>
		/// <param name="vcam"></param>
		/// <param name="stage"></param>
		/// <param name="state"></param>
		/// <param name="deltaTime"></param>
		protected override void PostPipelineStageCallback(
			CinemachineVirtualCameraBase vcam,
			CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
		{
			if (enabled && stage == CinemachineCore.Stage.Body)
			{
				var pos = state.RawPosition;
				if (LockXAxis)
				{
					pos.x = _forcedPosition.x;
				}
				if (LockYAxis)
				{
					pos.y = _forcedPosition.y;
				}
				if (LockZAxis)
				{
					pos.z = _forcedPosition.z;
				}
				state.RawPosition = pos;
			}
		}
	}
}
#endif
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 3D 공간에서 노드 세트를 따라 이동하는 움직이는 플랫폼을 처리하는 클래스
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Moving Platform 3D")]
	public class MovingPlatform3D : MMPathMovement
	{
		/// The force to apply when pushing a character that'd be in the way of the moving platform
		[Tooltip("움직이는 플랫폼을 방해하는 캐릭터를 밀 때 적용되는 힘")]
		public float PushForce = 5f;       
	}
}
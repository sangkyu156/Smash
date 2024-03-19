using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 레이어 이름을 추적하고 가장 일반적인 레이어 및 레이어 마스크 조합에 레이어 마스크를 사용할 준비가 된 간단한 정적 클래스입니다.
    /// 물론 레이어 순서나 번호를 변경하는 경우 이 클래스를 업데이트하고 싶을 것입니다.
    /// </summary>
    public static class LayerManager
	{
		private static int ObstaclesLayer = 8;
		private static int GroundLayer = 9;
		private static int PlayerLayer = 10;
		private static int EnemiesLayer = 13;
		private static int HoleLayer = 15;
		private static int MovingPlatformLayer = 16;
		private static int FallingPlatformLayer = 17;
		private static int ProjectileLayer = 18;
        
		public static int ObstaclesLayerMask = 1 << ObstaclesLayer;
		public static int GroundLayerMask = 1 << GroundLayer;
		public static int PlayerLayerMask = 1 << PlayerLayer;
		public static int EnemiesLayerMask = 1 << EnemiesLayer;
		public static int HoleLayerMask = 1 << HoleLayer;
		public static int MovingPlatformLayerMask = 1 << MovingPlatformLayer;
		public static int FallingPlatformLayerMask = 1 << FallingPlatformLayer;
		public static int ProjectileLayerMask = 1 << ProjectileLayer;
	}
}
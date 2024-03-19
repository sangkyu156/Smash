using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 캐릭터가 현재 프레임에서 뭔가를 하고 있는지 확인하는 데 사용할 수 있는 다양한 상태
    /// </summary>    
    public class CharacterStates 
	{
		/// The possible character conditions
		public enum CharacterConditions
		{
			Normal,
			ControlledMovement,
			Frozen,
            Paused,
			Dead,
			Stunned
		}

        /// 캐릭터가 있을 수 있는 가능한 이동 상태. 이는 일반적으로 자신의 클래스에 해당하지만 필수는 아닙니다.
        public enum MovementStates 
		{
			Null,
			Idle,
			Falling,
			Walking,
			Running,
			Crouching,
			Crawling, 
			Dashing,
			Jetpacking,
			Jumping,
			Pushing,
			DoubleJumping,
			Attacking,
			FallingDownHole,
            Slow
        }
	}
}
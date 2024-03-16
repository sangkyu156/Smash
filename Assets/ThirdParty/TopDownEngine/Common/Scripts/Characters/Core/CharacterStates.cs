using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{	
	/// <summary>
	/// The various states you can use to check if your character is doing something at the current frame
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
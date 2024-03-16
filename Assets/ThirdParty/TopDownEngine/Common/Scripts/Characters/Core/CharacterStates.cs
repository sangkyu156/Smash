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

        /// ĳ���Ͱ� ���� �� �ִ� ������ �̵� ����. �̴� �Ϲ������� �ڽ��� Ŭ������ �ش������� �ʼ��� �ƴմϴ�.
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
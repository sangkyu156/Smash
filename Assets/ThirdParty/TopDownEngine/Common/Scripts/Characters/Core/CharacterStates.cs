using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// ĳ���Ͱ� ���� �����ӿ��� ������ �ϰ� �ִ��� Ȯ���ϴ� �� ����� �� �ִ� �پ��� ����
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    ///이 AI 작업을 통해 CharacterOrientation2D 능력이 있는 에이전트가 대상을 향하게 됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionFaceTowardsTarget2D")]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	public class AIActionFaceTowardsTarget2D : AIAction
	{
		/// the possible modes you can ask the AI to face (should - usually - match your CharacterOrientation2D settings)  
		public enum Modes { LeftRight, FacingDirections }

		[Header("Face Towards Target 2D")] 
		/// the selected facing mode
		public Modes Mode = Modes.LeftRight;
        
		protected CharacterOrientation2D _characterOrientation2D;
		protected Vector3 _targetPosition;
		protected Vector2 _distance;
		protected bool _chacterOrientation2DNotNull;
		protected Character.FacingDirections _newFacingDirection;
        
		/// <summary>
		/// On init we grab our CharacterOrientation2D ability
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterOrientation2D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterOrientation2D>();
			if (_characterOrientation2D != null)
			{
				_chacterOrientation2DNotNull = true;
				_characterOrientation2D.FacingMode = CharacterOrientation2D.FacingModes.None;    
			}
		}

		/// <summary>
		/// On PerformAction we face our target
		/// </summary>
		public override void PerformAction()
		{
			FaceTarget();
		}

		/// <summary>
		/// Makes the orientation 2D ability face towards the brain target
		/// </summary>
		protected virtual void FaceTarget()
		{
			if ((_brain.Target == null) || !_chacterOrientation2DNotNull)
			{
				return;
			}
			_targetPosition = _brain.Target.transform.position;

			if (Mode == Modes.LeftRight)
			{
				int facingDirection = (_targetPosition.x < this.transform.position.x) ? -1 : 1;
				_characterOrientation2D.FaceDirection(facingDirection);    
			}
			else
			{
				_distance = _targetPosition - this.transform.position;
				if (Mathf.Abs(_distance.y) > Mathf.Abs(_distance.x))
				{
					_newFacingDirection = (_distance.y > 0) ? Character.FacingDirections.North : Character.FacingDirections.South;
				}
				else
				{
					_newFacingDirection = (_distance.x > 0) ? Character.FacingDirections.East : Character.FacingDirections.West;
				}
				_characterOrientation2D.Face(_newFacingDirection);
			}
		}
	}
}
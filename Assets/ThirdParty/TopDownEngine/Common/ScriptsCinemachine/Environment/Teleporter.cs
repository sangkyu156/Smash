using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{	
	[AddComponentMenu("TopDown Engine/Environment/Teleporter")]
    /// <summary>
    /// 해당 개체에서 대상으로 개체를 순간 이동하려면 collider2D 또는 collider 트리거에 이 스크립트를 추가하세요.
    /// </summary>
    public class Teleporter : ButtonActivated 
	{
		/// the possible modes the teleporter can interact with the camera system on activation, either doing nothing, teleporting the camera to a new position, or blending between Cinemachine virtual cameras
		public enum CameraModes { DoNothing, TeleportCamera, CinemachinePriority }
		/// the possible teleportation modes (either 1-frame instant teleportation, or tween between this teleporter and its destination)
		public enum TeleportationModes { Instant, Tween }
		/// the possible time modes 
		public enum TimeModes { Unscaled, Scaled }

		[Header("Teleporter")]

		/// if true, this won't teleport non player characters
		[Tooltip("true인 경우 플레이어가 아닌 캐릭터는 순간이동하지 않습니다.")]
		public bool OnlyAffectsPlayer = true;
		/// the offset to apply when exiting this teleporter
		[Tooltip("이 텔레포터를 나갈 때 적용할 오프셋")]
		public Vector3 ExitOffset;
		/// the selected teleportation mode 
		[Tooltip("선택한 순간이동 모드")]
		public TeleportationModes TeleportationMode = TeleportationModes.Instant;
		/// the curve to apply to the teleportation tween 
		[MMEnumCondition("TeleportationMode", (int)TeleportationModes.Tween)]
		[Tooltip("순간 이동 트윈에 적용할 곡선")]
		public MMTween.MMTweenCurve TweenCurve = MMTween.MMTweenCurve.EaseInCubic;
		/// whether or not to maintain the x value of the teleported object on exit
		[Tooltip("종료 시 순간 이동된 개체의 x 값을 유지할지 여부")]
		public bool MaintainXEntryPositionOnExit = false;
		/// whether or not to maintain the y value of the teleported object on exit
		[Tooltip("종료 시 순간 이동된 개체의 Y 값을 유지할지 여부")]
		public bool MaintainYEntryPositionOnExit = false;
		/// whether or not to maintain the z value of the teleported object on exit
		[Tooltip("종료 시 순간 이동된 개체의 Z 값을 유지할지 여부")]
		public bool MaintainZEntryPositionOnExit = false;

		[Header("Destination")]

		/// the teleporter's destination
		[Tooltip("텔레포터의 목적지")]
		public Teleporter Destination;
		/// if this is true, the teleported object will be put on the destination's ignore list, to prevent immediate re-entry. If your 
		/// destination's offset is far enough from its center, you can set that to false
		[Tooltip("이것이 사실이라면 순간이동된 개체는 즉각적인 재진입을 방지하기 위해 대상의 무시 목록에 추가됩니다. 목적지의 오프셋이 중심에서 충분히 멀리 떨어져 있으면 이를 false로 설정할 수 있습니다.")]
		public bool AddToDestinationIgnoreList = true;

		[Header("Rooms")]

		/// the chosen camera mode
		[Tooltip("선택한 카메라 모드")]
		public CameraModes CameraMode = CameraModes.TeleportCamera;
		/// the room this teleporter belongs to
		[Tooltip("이 텔레포터가 속한 방")]
		public Room CurrentRoom;
		/// the target room
		[Tooltip("대상 방")]
		public Room TargetRoom;
        
		[Header("MMFader Transition")]

		/// if this is true, a fade to black will occur when teleporting
		[Tooltip("이것이 사실이라면 순간이동 시 검은색으로 변하는 현상이 발생합니다.")]
		public bool TriggerFade = false;
		/// the ID of the fader to target
		[MMCondition("TriggerFade", true)]
		[Tooltip("타겟으로 하는 페이더의 ID")]
		public int FaderID = 0;
		/// the curve to use to fade to black
		[Tooltip("검은색으로 페이드하는 데 사용할 곡선")]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		/// if this is true, fade events will ignore timescale
		[Tooltip("이것이 사실이라면 페이드 이벤트는 시간 척도를 무시합니다.")]
		public bool FadeIgnoresTimescale = false;

		[Header("Mask")]

		/// whether or not we should ask to move a MMSpriteMask on activation
		[Tooltip("활성화 시 MMSpriteMask 이동을 요청해야 하는지 여부")]
		public bool MoveMask = true;
		/// the curve to move the mask along to
		[MMCondition("MoveMask", true)]
		[Tooltip("마스크를 이동할 곡선")]
		public MMTween.MMTweenCurve MoveMaskCurve = MMTween.MMTweenCurve.EaseInCubic;
		/// the method to move the mask
		[MMCondition("MoveMask", true)]
		[Tooltip("마스크를 이동하는 데 사용되는 방법")]
		public MMSpriteMaskEvent.MMSpriteMaskEventTypes MoveMaskMethod = MMSpriteMaskEvent.MMSpriteMaskEventTypes.ExpandAndMoveToNewPosition;
		/// the duration of the mask movement (usually the same as the DelayBetweenFades
		[MMCondition("MoveMask", true)]
		[Tooltip("마스크 이동 기간(보통 DelayBetweenFades와 동일)")]
		public float MoveMaskDuration = 0.2f;

		[Header("Freeze")]
		/// whether or not time should be frozen during the transition
		[Tooltip("전환 중에 시간을 멈춰야 하는지 여부")]
		public bool FreezeTime = false;
		/// whether or not the character should be frozen (input blocked) for the duration of the transition
		[Tooltip("전환이 진행되는 동안 캐릭터가 정지(입력 차단)되어야 하는지 여부")]
		public bool FreezeCharacter = true;

		[Header("Teleport Sequence")]
		/// the timescale to use for the teleport sequence
		[Tooltip("텔레포트 시퀀스에 사용할 시간 척도")]
		public TimeModes TimeMode = TimeModes.Unscaled;
		/// the delay (in seconds) to apply before running the sequence
		[Tooltip("시퀀스를 실행하기 전에 적용할 지연(초)")]
		public float InitialDelay = 0.1f;
		/// the duration (in seconds) after the initial delay covering for the fade out of the scene
		[Tooltip("장면의 페이드 아웃을 덮는 초기 지연 이후의 지속 시간(초)")]
		public float FadeOutDuration = 0.2f;
		/// the duration (in seconds) to wait for after the fade out and before the fade in
		[Tooltip("페이드 아웃 후와 페이드 인 전에 대기하는 기간(초)")]
		public float DelayBetweenFades = 0.3f;
		/// the duration (in seconds) after the initial delay covering for the fade in of the scene
		[Tooltip("장면의 페이드 인을 위한 초기 지연 이후의 지속 시간(초)입니다.")]
		public float FadeInDuration = 0.2f;
		/// the duration (in seconds) to apply after the fade in of the scene
		[Tooltip("장면의 페이드 인 이후 적용할 지속 시간(초)")]
		public float FinalDelay = 0.1f;

		public float LocalTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledTime : Time.time;
		public float LocalDeltaTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledDeltaTime : Time.deltaTime;

		protected Character _player;
		protected Character _characterTester;
		protected CharacterGridMovement _characterGridMovement;
		protected List<Transform> _ignoreList;

		protected Vector3 _entryPosition;
		protected Vector3 _newPosition;

		/// <summary>
		/// On start we initialize our ignore list
		/// </summary>
		protected virtual void Awake()
		{
			InitializeTeleporter();
		}

		/// <summary>
		/// Grabs the current room in the parent if needed
		/// </summary>
		protected virtual void InitializeTeleporter()
		{
			_ignoreList = new List<Transform>();
			if (CurrentRoom == null)
			{
				CurrentRoom = this.gameObject.GetComponentInParent<Room>();
			}
		}

		/// <summary>
		/// Triggered when something enters the teleporter
		/// </summary>
		/// <param name="collider">Collider.</param>
		protected override void TriggerEnter(GameObject collider)
		{
			// if the object that collides with the teleporter is on its ignore list, we do nothing and exit.
			if (_ignoreList.Contains(collider.transform))
			{
				return;
			}

			_characterTester = collider.GetComponent<Character>();

			if (_characterTester != null)
			{
				if (RequiresPlayerType)
				{
					if (_characterTester.CharacterType != Character.CharacterTypes.Player)
					{
						return;
					}
				}

				_player = _characterTester;
				_characterGridMovement = _player.GetComponent<CharacterGridMovement>();
			}
            
			// if the teleporter is supposed to only affect the player, we do nothing and exit
			if (OnlyAffectsPlayer || !AutoActivation)
			{
				base.TriggerEnter(collider);
			}
			else
			{
				base.TriggerButtonAction();
				Teleport(collider);
			}
		}

		/// <summary>
		/// If we're button activated and if the button is pressed, we teleport
		/// </summary>
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction();
			Teleport(_player.gameObject);
		}

		/// <summary>
		/// Teleports whatever enters the portal to a new destination
		/// </summary>
		protected virtual void Teleport(GameObject collider)
		{
			_entryPosition = collider.transform.position;
			// if the teleporter has a destination, we move the colliding object to that destination
			if (Destination != null)
			{
				StartCoroutine(TeleportSequence(collider));         
			}
		}
        
		/// <summary>
		/// Handles the teleport sequence (fade in, pause, fade out)
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual IEnumerator TeleportSequence(GameObject collider)
		{
			SequenceStart(collider);

			for (float timer = 0, duration = InitialDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }
            
			AfterInitialDelay(collider);

			for (float timer = 0, duration = FadeOutDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterFadeOut(collider);
            
			for (float timer = 0, duration = DelayBetweenFades; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterDelayBetweenFades(collider);

			for (float timer = 0, duration = FadeInDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterFadeIn(collider);

			for (float timer = 0, duration = FinalDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }

			SequenceEnd(collider);
		}

		/// <summary>
		/// Describes the events happening before the initial fade in
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void SequenceStart(GameObject collider)
		{
			if (CameraMode == CameraModes.TeleportCamera)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);
			}

			if (FreezeTime)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
			}

			if (FreezeCharacter && (_player != null))
			{
				_player.Freeze();
			}
		}

		/// <summary>
		/// Describes the events happening after the initial delay has passed
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void AfterInitialDelay(GameObject collider)
		{            
			if (TriggerFade)
			{
				MMFadeInEvent.Trigger(FadeOutDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
			}
		}

		/// <summary>
		/// Describes the events happening once the initial fade in is complete
		/// </summary>
		protected virtual void AfterFadeOut(GameObject collider)
		{   
			#if MM_CINEMACHINE         
			TeleportCollider(collider);

			if (AddToDestinationIgnoreList)
			{
				Destination.AddToIgnoreList(collider.transform);
			}            
            
			if (CameraMode == CameraModes.CinemachinePriority)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
				MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, DelayBetweenFades);
			}

			if (CurrentRoom != null)
			{
				CurrentRoom.PlayerExitsRoom();
			}
            
			if (TargetRoom != null)
			{
				TargetRoom.PlayerEntersRoom();
				if (TargetRoom.VirtualCamera != null)
				{
					TargetRoom.VirtualCamera.Priority = 10;	
				}
				MMSpriteMaskEvent.Trigger(MoveMaskMethod, (Vector2)TargetRoom.RoomColliderCenter, TargetRoom.RoomColliderSize, MoveMaskDuration, MoveMaskCurve);
			}
			#endif
		}

		/// <summary>
		/// Teleports the object going through the teleporter, either instantly or by tween
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void TeleportCollider(GameObject collider)
		{
			_newPosition = Destination.transform.position + Destination.ExitOffset;
			if (MaintainXEntryPositionOnExit)
			{
				_newPosition.x = _entryPosition.x;
			}
			if (MaintainYEntryPositionOnExit)
			{
				_newPosition.y = _entryPosition.y;
			}
			if (MaintainZEntryPositionOnExit)
			{
				_newPosition.z = _entryPosition.z;
			}

			switch (TeleportationMode)
			{
				case TeleportationModes.Instant:
					collider.transform.position = _newPosition;
					_ignoreList.Remove(collider.transform);
					break;
				case TeleportationModes.Tween:
					StartCoroutine(TeleportTweenCo(collider, collider.transform.position, _newPosition));
					break;
			}
		}

		/// <summary>
		/// Tweens the object from origin to destination
		/// </summary>
		/// <param name="collider"></param>
		/// <param name="origin"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		protected virtual IEnumerator TeleportTweenCo(GameObject collider, Vector3 origin, Vector3 destination)
		{
			float startedAt = LocalTime;
			while (LocalTime - startedAt < DelayBetweenFades)
			{
				float elapsedTime = LocalTime - startedAt;
				collider.transform.position = MMTween.Tween(elapsedTime, 0f, DelayBetweenFades, origin, destination, TweenCurve);
				yield return null;
			}
			_ignoreList.Remove(collider.transform);
		}

		/// <summary>
		/// Describes the events happening after the pause between the fade in and the fade out
		/// </summary>
		protected virtual void AfterDelayBetweenFades(GameObject collider)
		{
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

			if (TriggerFade)
			{
				MMFadeOutEvent.Trigger(FadeInDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
			}
		}

		/// <summary>
		/// Describes the events happening after the fade in of the scene
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void AfterFadeIn(GameObject collider)
		{

		}

		/// <summary>
		/// Describes the events happening after the fade out is complete, so at the end of the teleport sequence
		/// </summary>
		protected virtual void SequenceEnd(GameObject collider)
		{
			if (FreezeCharacter && (_player != null))
			{
				_player.UnFreeze();
			}

			if (_characterGridMovement != null)
			{
				_characterGridMovement.SetCurrentWorldPositionAsNewPosition();
			}

			if (FreezeTime)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			}
		}

		/// <summary>
		/// When something exits the teleporter, if it's on the ignore list, we remove it from it, so it'll be considered next time it enters.
		/// </summary>
		/// <param name="collider">Collider.</param>
		public override void TriggerExitAction(GameObject collider)
		{
			if (_ignoreList.Contains(collider.transform))
			{
				_ignoreList.Remove(collider.transform);
			}
			base.TriggerExitAction(collider);
		}

		/// <summary>
		/// Adds an object to the ignore list, which will prevent that object to be moved by the teleporter while it's in that list
		/// </summary>
		/// <param name="objectToIgnore">Object to ignore.</param>
		public virtual void AddToIgnoreList(Transform objectToIgnore)
		{
			if (!_ignoreList.Contains(objectToIgnore))
			{
				_ignoreList.Add(objectToIgnore);
			}            
		}
        
		/// <summary>
		/// On draw gizmos, we draw arrows to the target destination and target room if there are any
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if (Destination != null)
			{
				// draws an arrow from this teleporter to its destination
				MMDebug.DrawGizmoArrow(this.transform.position, (Destination.transform.position + Destination.ExitOffset) - this.transform.position, Color.cyan, 1f, 25f);
				// draws a point at the exit position 
				MMDebug.DebugDrawCross(this.transform.position + ExitOffset, 0.5f, Color.yellow);
				MMDebug.DrawPoint(this.transform.position + ExitOffset, Color.yellow, 0.5f);
			}

			if (TargetRoom != null)
			{
				// draws an arrow to the destination room
				MMDebug.DrawGizmoArrow(this.transform.position, TargetRoom.transform.position - this.transform.position, MMColors.Pink, 1f, 25f);
			}
		}
	}
}
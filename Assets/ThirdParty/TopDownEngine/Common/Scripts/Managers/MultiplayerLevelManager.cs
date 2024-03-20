using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 멀티플레이어 장면(특히 스폰 및 카메라 모드)을 처리하기 위한 일반 레벨 관리자
    /// 자신만의 특정 게임플레이 규칙을 구현하도록 확장하는 것이 좋습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/MultiplayerLevelManager")]
	public class MultiplayerLevelManager : LevelManager
	{
		[Header("Multiplayer spawn")]
		/// the list of checkpoints (in order) to use to spawn characters
		[Tooltip("캐릭터를 생성하는 데 사용할 체크포인트 목록(순서대로)")]
		public List<CheckPoint> SpawnPoints;
		/// the types of cameras to choose from
		public enum CameraModes { Split, Group }

		[Header("Cameras")]
		/// the selected camera mode (either group, all targets in one screen, or split screen)
		[Tooltip("선택한 카메라 모드(그룹, 한 화면의 모든 대상 또는 분할 화면)")]
		public CameraModes CameraMode = CameraModes.Split;
		/// the group camera rig
		[Tooltip("그룹 카메라 장비")]
		public GameObject GroupCameraRig;
		/// the split camera rig
		[Tooltip("분할 카메라 장비")]
		public GameObject SplitCameraRig;

		[Header("GUI Manager")]
		/// the multiplayer GUI Manager
		[Tooltip("멀티플레이어 GUI 관리자")]
		public MultiplayerGUIManager MPGUIManager;

		/// <summary>
		/// On awake we handle our different camera modes
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			HandleCameraModes();
		}

		/// <summary>
		/// Sets up the scene to match the selected camera mode
		/// </summary>
		protected virtual void HandleCameraModes()
		{
			if (CameraMode == CameraModes.Split)
			{
				if (GroupCameraRig != null) { GroupCameraRig.SetActive(false); }
				if (SplitCameraRig != null) { SplitCameraRig.SetActive(true); }
				if (MPGUIManager != null)
				{
					MPGUIManager.SplitHUD?.SetActive(true);
					MPGUIManager.GroupHUD?.SetActive(false);
					MPGUIManager.SplittersGUI?.SetActive(true);
				}
			}
			if (CameraMode == CameraModes.Group)
			{
				if (GroupCameraRig != null) { GroupCameraRig?.SetActive(true); }
				if (SplitCameraRig != null) { SplitCameraRig?.SetActive(false); }
				if (MPGUIManager != null)
				{
					MPGUIManager.SplitHUD?.SetActive(false);
					MPGUIManager.GroupHUD?.SetActive(true);
					MPGUIManager.SplittersGUI?.SetActive(false);
				}
			}
		}

		/// <summary>
		/// Spawns all characters at the specified spawn points
		/// </summary>
		protected override void SpawnMultipleCharacters()
		{
			for (int i = 0; i < Players.Count; i++)
			{
				SpawnPoints[i].SpawnPlayer(Players[i]);
				if (AutoAttributePlayerIDs)
				{
					Players[i].SetPlayerID("Player" + (i + 1));
				}                
			}
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, null);
		}

		/// <summary>
		/// Kills the specified player 
		/// </summary>
		public override void PlayerDead(Character playerCharacter)
		{
			if (playerCharacter == null)
			{
				return;
			}
			Health characterHealth = playerCharacter.GetComponent<Health>();
			if (characterHealth == null)
			{
				return;
			}
			else
			{
				OnPlayerDeath(playerCharacter);
			}
		}
        
		/// <summary>
		/// Override this to specify what happens when a player dies
		/// </summary>
		/// <param name="playerCharacter"></param>
		protected virtual void OnPlayerDeath(Character playerCharacter)
		{

		}
	}
}
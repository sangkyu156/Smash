using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Managers/Multiplayer GUIManager")]
	public class MultiplayerGUIManager : GUIManager
	{
		[Header("Multiplayer GUI")]
		/// the HUD to display when in split screen mode
		[Tooltip("분할 화면 모드에서 표시할 HUD")]
		public GameObject SplitHUD;
		/// the HUD to display when in group camera mode
		[Tooltip("그룹 카메라 모드에 있을 때 표시할 HUD")]
		public GameObject GroupHUD;
		/// a UI object used to display the splitters UI images
		[Tooltip("스플리터 UI 이미지를 표시하는 데 사용되는 UI 객체")]
		public GameObject SplittersGUI;
	}
}
﻿using UnityEngine;
using MoreMountains.Tools;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 장면의 빈 구성 요소에 추가하면 SwapButton(기본적으로 P)을 누를 때 장면의 캐릭터를 바꿀 수 있습니다.
    /// 장면의 각 캐릭터에는 CharacterSwap 클래스와 해당 PlayerID가 있어야 합니다.
    /// MinimalCharacterSwap 데모 장면에서 이러한 설정의 예를 볼 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/CharacterSwapManager")]
	public class CharacterSwapManager : MMSingleton<CharacterSwapManager>, MMEventListener<TopDownEngineEvent>
	{
		[Header("Character Swap")]
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			/// the button to use to go up
			public Key SwapKey = Key.P;
		#else
		/// the name of the axis to use to catch input and trigger a swap on press
		[Tooltip("입력을 포착하고 누를 때 전환을 트리거하는 데 사용할 축의 이름")]
		public string SwapButtonName = "Player1_SwapCharacter";
		#endif
		/// the PlayerID set on the Characters you want to swap between
		[Tooltip("교체하려는 캐릭터에 설정된 PlayerID")]
		public string PlayerID = "Player1";

		protected CharacterSwap[] _characterSwapArray;
		protected MMCircularList<CharacterSwap> _characterSwapList;
		protected TopDownEngineEvent _swapEvent = new TopDownEngineEvent(TopDownEngineEventTypes.CharacterSwap, null);

		/// <summary>
		/// Grabs all CharacterSwap equipped characters in the scene and stores them in a list, sorted by Order
		/// </summary>
		public virtual void UpdateList()
		{
			_characterSwapArray = FindObjectsOfType<CharacterSwap>();
			_characterSwapList = new MMCircularList<CharacterSwap>();

			// stores the array into the list if the PlayerID matches
			for (int i = 0; i < _characterSwapArray.Length; i++)
			{
				if (_characterSwapArray[i].PlayerID == PlayerID)
				{
					_characterSwapList.Add(_characterSwapArray[i]);
				}
			}

			if (_characterSwapList.Count == 0)
			{
				return;
			}

			// sorts the list by order
			_characterSwapList.Sort(SortSwapsByOrder);
		}

		/// <summary>
		/// Static method to compare two CharacterSwaps
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		static int SortSwapsByOrder(CharacterSwap a, CharacterSwap b)
		{
			return a.Order.CompareTo(b.Order);
		}

		/// <summary>
		/// On Update, we watch for input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
		}

		/// <summary>
		/// If the user presses the Swap button, we swap characters
		/// </summary>
		protected virtual void HandleInput()
		{
			#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
			if (Input.GetButtonDown(SwapButtonName))
			{
				SwapCharacter();
			}
			#else
			if (Keyboard.current[SwapKey].wasPressedThisFrame)
			{
				SwapCharacter();
			}
			#endif
		}

		/// <summary>
		/// Changes the current character to the next one in line
		/// </summary>
		public virtual void SwapCharacter()
		{
			if (_characterSwapList.Count < 2)
			{
				return;
			}

			int currentIndex = GetCurrentIndex();
			_characterSwapList.CurrentIndex = currentIndex;
			_characterSwapList.IncrementCurrentIndex();
			int newIndex = currentIndex;

			int i = 0;
			while (i < _characterSwapList.Count)
			{
				if (_characterSwapList.Current.enabled)
				{
					newIndex = _characterSwapList.CurrentIndex;
					break;
				}

				_characterSwapList.IncrementCurrentIndex();
				i++;
			}
			
			_characterSwapList[currentIndex].ResetCharacterSwap();
			_characterSwapList[newIndex].SwapToThisCharacter();

			LevelManager.Instance.Players[0] = _characterSwapList[newIndex].gameObject.GetComponentInParent<Character>();
			MMEventManager.TriggerEvent(_swapEvent);
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
		}

		/// <summary>
		/// Finds which character is currently active and considered the current one
		/// </summary>
		/// <returns></returns>
		public virtual int GetCurrentIndex()
		{
			int currentIndex = -1;
			for (int i=0; i<_characterSwapList.Count; i++)
			{
				if (_characterSwapList[i].Current())
				{
					return i;
				}
			}
			return currentIndex;
		}

		/// <summary>
		/// On Level Start, we initialize our list
		/// </summary>
		/// <param name="eventType"></param>
		public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelStart:
					UpdateList();
					break;
			}
		}
        
		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
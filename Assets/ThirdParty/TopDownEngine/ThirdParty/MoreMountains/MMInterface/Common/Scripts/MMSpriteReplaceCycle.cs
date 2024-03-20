using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.MMInterface
{
    /// <summary>
    ///버튼을 눌렀을 때 스프라이트가 X 스프라이트를 순환하도록 버튼에 추가하는 클래스입니다.
    /// </summary>
    public class MMSpriteReplaceCycle : MonoBehaviour 
	{
		/// the list of sprites to cycle through
		public Sprite[] Sprites;
		/// the sprite index to start on
		public int StartIndex = 0;

		protected Image _image;
		protected MMTouchButton _mmTouchButton;
		protected int _currentIndex = 0;

		/// <summary>
		/// On Start we initialize our cycler
		/// </summary>
		protected virtual void Start()
		{
			Initialization ();
		}

		/// <summary>
		/// On init, we grab our image component, and set our first sprite as specified
		/// </summary>
		protected virtual void Initialization()
		{
			_mmTouchButton = GetComponent<MMTouchButton> ();
			if (_mmTouchButton != null)
			{
				_mmTouchButton.ReturnToInitialSpriteAutomatically = false;
			}
			_image = GetComponent<Image> ();
			if (_image == null) { return; }

			SwitchToIndex(StartIndex);
		}

		/// <summary>
		/// Changes the image's sprite to the next sprite in the list
		/// </summary>
		public virtual void Swap()
		{
			_currentIndex++;
			if (_currentIndex >= Sprites.Length)
			{
				_currentIndex = 0;
			}
			{
				SwitchToIndex (_currentIndex);
			}
		}

		/// <summary>
		/// A public method to set the sprite directly to the one specified in parameters
		/// </summary>
		/// <param name="index">Index.</param>
		public virtual void SwitchToIndex(int index)
		{
			if (_image == null) { return; }
			if (Sprites.Length <= index) { return; }
			if (Sprites[index] == null) { return; }
			_image.sprite = Sprites[index];
			_currentIndex = index;
		}


	}
}
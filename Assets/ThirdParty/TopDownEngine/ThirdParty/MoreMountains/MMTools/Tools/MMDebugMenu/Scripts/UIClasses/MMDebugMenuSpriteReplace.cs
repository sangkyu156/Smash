using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 켜기 및 끄기 상태에 대해 다른 스프라이트가 있는 버튼처럼 작동하도록 이미지에 추가하는 클래스입니다.
    /// </summary>
    public class MMDebugMenuSpriteReplace : MonoBehaviour 
	{
		/// the sprite to use when in the "on" state
		public Sprite OnSprite;
		/// the sprite to use when in the "off" state
		public Sprite OffSprite;
		/// if this is true, the button will start if "on" state
		public bool StartsOn = true;
		/// the current state of the button
		public bool CurrentValue { get { return (_image.sprite == OnSprite); } }

		protected Image _image;
		protected MMTouchButton _mmTouchButton;

		/// <summary>
		/// On Start we initialize our button
		/// </summary>
		protected virtual void Awake()
		{
			//Initialization ();
		}

		/// <summary>
		/// On init, we grab our image component, and set our sprite in its initial state
		/// </summary>
		public virtual void Initialization()
		{
			_image = this.gameObject.GetComponent<Image> ();
			_mmTouchButton = this.gameObject.GetComponent<MMTouchButton> ();
			if (_mmTouchButton != null)
			{
				_mmTouchButton.ReturnToInitialSpriteAutomatically = false;
			}

			if (_image == null) { return; }
			if ((OnSprite == null) || (OffSprite == null)) { return; }

			if (StartsOn)
			{
				_image.sprite = OnSprite;
			}
			else
			{
				_image.sprite = OffSprite;
			}
		}

		/// <summary>
		/// A public method to change the sprite 
		/// </summary>
		public virtual void Swap()
		{
			if (_image.sprite != OnSprite)
			{
				SwitchToOnSprite ();
			}
			else
			{
				SwitchToOffSprite ();
			}
		}

		/// <summary>
		/// a public method to switch to off sprite directly
		/// </summary>
		public virtual void SwitchToOffSprite()
		{
			if (_image == null) { return; }
			if (OffSprite == null) { return; }

			SpriteOff ();
		}

		/// <summary>
		/// sets the image's sprite to off
		/// </summary>
		protected virtual void SpriteOff()
		{
			_image.sprite = OffSprite;
		}

		/// <summary>
		/// a public method to switch to on sprite directly
		/// </summary>
		public virtual void SwitchToOnSprite()
		{
			if (_image == null) { return; }
			if (OnSprite == null) { return; }

			SpriteOn ();
		}	

		/// <summary>
		/// sets the image's sprite to on
		/// </summary>
		protected virtual void SpriteOn()
		{
			_image.sprite = OnSprite;
		}
	}
}
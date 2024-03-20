using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MoreMountains.MMInterface
{
    /// <summary>
    /// 라디오 버튼을 처리하는 클래스입니다.
    /// 그룹화하려면 그룹의 모든 라디오 버튼에 동일한 RadioButtonGroupName 문자열을 사용하면 됩니다.
    /// </summary>
    public class MMRadioButton : MMSpriteReplace 
	{
		/// The name of the radio button group. Use the same one for each buttons in the group
		public string RadioButtonGroupName;

		protected List<MMRadioButton> _group;

		/// <summary>
		/// On Init, we grab all buttons from the group
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization ();
			FindAllRadioButtonsFromTheSameGroup ();
		}

		/// <summary>
		/// Finds all radio buttons from the same group.
		/// </summary>
		protected virtual void FindAllRadioButtonsFromTheSameGroup ()
		{
			_group = new List<MMRadioButton> ();

			MMRadioButton[] radioButtons = FindObjectsOfType(typeof(MMRadioButton)) as MMRadioButton[];
			foreach (MMRadioButton radioButton in radioButtons) 
			{
				if ((radioButton.RadioButtonGroupName == RadioButtonGroupName)
				    && (radioButton != this))
				{
					_group.Add (radioButton);
				}
			}
		}

		/// <summary>
		/// When turning the button on, we turn off all other buttons in the group
		/// </summary>
		protected override void SpriteOn()
		{
			base.SpriteOn ();
			if (_group.Count >= 1)
			{
				foreach (MMRadioButton radioButton in _group) 
				{
					radioButton.SwitchToOffSprite ();
				}	
			}
		}
	}
}
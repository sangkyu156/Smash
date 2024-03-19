using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 사용하면 Start 또는 Awake에서 자동으로 다른 게임 객체를 활성화하거나 비활성화할 수 있습니다.
    /// </summary>

    [AddComponentMenu("More Mountains/Tools/Activation/MMActivationOnStart")]
	public class MMActivationOnStart : MonoBehaviour
	{
		/// The possible modes that define whether this should run at Awake or Start
		public enum Modes { Awake, Start }
		/// the selected mode for this instance
		public Modes Mode = Modes.Start;
		/// if true, objects will be activated on start, disabled otherwise
		public bool StateOnStart = true;
		/// the list of gameobjects whose active state will be affected on start
		public List<GameObject> TargetObjects;

		/// <summary>
		/// On Awake, we set our state if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (Mode != Modes.Awake)
			{
				return;
			}
			SetState();
		}

		/// <summary>
		/// On Start, we set our state if needed
		/// </summary>
		protected virtual void Start()
		{
			if (Mode != Modes.Start)
			{
				return;
			}
			SetState();
		}        

		/// <summary>
		/// Sets the state of all target objects
		/// </summary>
		protected virtual void SetState()
		{
			foreach (GameObject obj in TargetObjects)
			{
				obj.SetActive(StateOnStart);
			}
		}
	}
}
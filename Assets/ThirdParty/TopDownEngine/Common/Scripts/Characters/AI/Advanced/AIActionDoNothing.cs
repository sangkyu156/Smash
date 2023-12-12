using DG.Tweening;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이름에서 알 수 있듯이 아무것도 하지 않는 동작입니다. 그냥 거기서 기다리세요.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionDoNothing")]
	public class AIActionDoNothing : AIAction
	{
		public GameObject model;
        Vector3 rotationFix = new Vector3(100, 0, 100);
        protected CharacterOrientation3D _characterOrientation3D;

        new private void Awake()
        {
            _characterOrientation3D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterOrientation3D>();
        }


        /// <summary>
        /// On PerformAction we do nothing
        /// </summary>
        public override void PerformAction()
		{
			if(this.gameObject.layer == 24 && rotationFix != null)
            {
                _characterOrientation3D.ForcedRotationDirection = rotationFix;
                Debug.Log("들어옴");
			}
		}
	}
}
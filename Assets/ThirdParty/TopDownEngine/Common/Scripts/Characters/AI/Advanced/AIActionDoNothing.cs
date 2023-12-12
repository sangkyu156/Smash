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
		public Transform RrotationFix;
        /// <summary>
        /// On PerformAction we do nothing
        /// </summary>
        public override void PerformAction()
		{
			if(this.gameObject.layer == 24 && RrotationFix != null)
            {
				model.transform.LookAt(RrotationFix);
				Debug.Log("들어옴");
			}
		}
	}
}
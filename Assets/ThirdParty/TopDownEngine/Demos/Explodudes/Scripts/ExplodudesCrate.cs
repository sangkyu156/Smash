using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Explodude 데모 장면에서 상자의 y 크기를 수정하는 클래스
    /// </summary>
    public class ExplodudesCrate : TopDownMonoBehaviour
	{
		protected const float MinHeight = 0.8f;
		protected const float MaxHeight = 1.1f;
		protected Vector3 _newScale = Vector3.one;

		/// <summary>
		/// On Start we randomize our y scale for aesthetic considerations only
		/// </summary>
		protected virtual void Start()
		{
			_newScale.y = Random.Range(MinHeight, MaxHeight);
			this.transform.localScale = _newScale;
		}
	}
}
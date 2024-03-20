using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 UI 객체에 추가하면 이미지 구성 요소 없이 레이캐스트 대상으로 작동하게 됩니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/GUI/MMRaycastTarget")]
	public class MMRaycastTarget : Graphic
	{
		public override void SetVerticesDirty() { return; }
		public override void SetMaterialDirty() { return; }

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			return;
		}
	}
}
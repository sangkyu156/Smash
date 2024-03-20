using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 사용하면 Awake, Start 또는 Parent() 메서드를 호출할 때마다 대상 상위 항목(또는 아무것도 설정되지 않은 경우 루트)에 적용한 변환의 상위 항목을 지정할 수 있습니다.
    /// </summary>
    public class MMParentingOnStart : MonoBehaviour
	{
		/// the possible modes this can run on
		public enum Modes { Awake, Start, Script }
		/// the selected mode
		public Modes Mode = Modes.Awake;
		/// the parent to parent to, leave empty if you want to unparent completely
		public Transform TargetParent;

		/// <summary>
		/// On Awake we parent if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (Mode == Modes.Awake)
			{
				Parent();
			}
		}

		/// <summary>
		/// On Start we parent if needed
		/// </summary>
		protected virtual void Start()
		{
			if (Mode == Modes.Start)
			{
				Parent();
			}
		}

		/// <summary>
		/// Sets this transform's parent to the target
		/// </summary>
		public virtual void Parent()
		{
			this.transform.SetParent(TargetParent);
		}
	}
}
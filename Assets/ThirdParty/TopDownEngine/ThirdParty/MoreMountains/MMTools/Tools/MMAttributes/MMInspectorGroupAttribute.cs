using System;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 공통 드롭다운 아래에 검사기 필드를 그룹화하는 데 사용되는 속성
    /// Rodrigo Prinheiro의 작업에서 영감을 받은 구현, https://github.com/RodrigoPrinheiro/unityFoldoutAttribute에서 확인 가능
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
	public class MMInspectorGroupAttribute : PropertyAttribute
	{
		public string GroupName;
		public bool GroupAllFieldsUntilNextGroupAttribute;
		public int GroupColorIndex;

		public MMInspectorGroupAttribute(string groupName, bool groupAllFieldsUntilNextGroupAttribute = false, int groupColorIndex = 24)
		{
			if (groupColorIndex > 139) { groupColorIndex = 139; }

			this.GroupName = groupName;
			this.GroupAllFieldsUntilNextGroupAttribute = groupAllFieldsUntilNextGroupAttribute;
			this.GroupColorIndex = groupColorIndex;
		}
	}
}
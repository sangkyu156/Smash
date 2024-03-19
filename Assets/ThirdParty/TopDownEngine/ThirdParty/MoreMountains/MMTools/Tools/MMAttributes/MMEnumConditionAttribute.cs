using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// 열거형의 현재 선택을 기반으로 필드를 조건부로 숨기는 속성입니다.
    /// 사용법: [MMEnumCondition("rotationMode", (int)RotationMode.LookAtTarget, (int)RotationMode.RotateToAngles)]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
	public class MMEnumConditionAttribute : PropertyAttribute
	{
		public string ConditionEnum = "";
		public bool Hidden = false;

		BitArray bitArray = new BitArray(32);
		public bool ContainsBitFlag(int enumValue)
		{
			return bitArray.Get(enumValue);
		}

		public MMEnumConditionAttribute(string conditionBoolean, params int[] enumValues)
		{
			this.ConditionEnum = conditionBoolean;
			this.Hidden = true;

			for (int i = 0; i < enumValues.Length; i++)
			{
				bitArray.Set(enumValues[i], true);
			}
		}
	}
}
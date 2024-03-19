using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;


namespace MoreMountains.InventoryEngine
{	
	[CustomEditor(typeof(InventoryItem),true)]
    /// <summary>
    /// InventoryItem 구성 요소에 대한 사용자 정의 편집기
    /// </summary>
    public class InventoryItemEditor : Editor 
	{
		/// <summary>
		/// Gets the target inventory component.
		/// </summary>
		/// <value>The inventory target.</value>
		public InventoryItem ItemTarget 
		{ 
			get 
			{ 
				return (InventoryItem)target;
			}
		} 
	   
		/// <summary>
		/// Custom editor for the inventory panel.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			List<string> excludedProperties = new List<string>();
			if (!ItemTarget.Equippable)
			{
				excludedProperties.Add("TargetEquipmentInventoryName");
				excludedProperties.Add("EquippedSound");
			}
			if (!ItemTarget.Usable)
			{
				excludedProperties.Add("UsedSound");
			}
			Editor.DrawPropertiesExcluding(serializedObject, excludedProperties.ToArray());
			serializedObject.ApplyModifiedProperties();
		}
	}
}
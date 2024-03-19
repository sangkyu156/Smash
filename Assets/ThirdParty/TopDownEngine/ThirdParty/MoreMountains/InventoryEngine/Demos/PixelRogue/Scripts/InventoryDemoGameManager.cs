using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 게임 관리자의 예로서 유일하게 중요한 부분은 Start 메서드에서 모든 인벤토리의 로드를 한 곳에서 트리거하는 방법입니다.
    /// </summary>
    public class InventoryDemoGameManager : MMSingleton<InventoryDemoGameManager> 
	{
		public InventoryDemoCharacter Player { get; protected set; }

		protected override void Awake () 
		{
			base.Awake ();
			Player = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryDemoCharacter>()	;
		}

		/// <summary>
		/// On start, we trigger our load event, which will be caught by inventories so they try to load saved content
		/// </summary>
		protected virtual void Start()
		{
			MMGameEvent.Trigger("Load");
		}
	}
}
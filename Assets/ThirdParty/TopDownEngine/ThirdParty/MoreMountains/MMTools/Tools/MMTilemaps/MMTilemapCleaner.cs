using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 타일맵에 추가하여 모든 타일에서 타일을 정리하는 버튼을 얻을 수 있는 매우 간단한 구성 요소입니다.
    /// </summary>
    public class MMTilemapCleaner : MonoBehaviour
	{
		/// Debug buttons
		[MMInspectorButton("Clean")] 
		public bool CleanButton;
		[MMInspectorButton("CleanAllChildren")] 
		public bool CleanAllButton;
        
		protected Tilemap _tilemap;
		protected Tilemap[] _tilemaps;

		/// <summary>
		/// Cleans all tiles on the corresponding tilemap
		/// </summary>
		public virtual void Clean()
		{
			_tilemap = this.gameObject.GetComponent<Tilemap>();
			if (_tilemap != null)
			{
				_tilemap.ClearAllTiles();
			}
		}

		/// <summary>
		/// Cleans all tiles on all tilemaps that are set as children of this object
		/// </summary>
		public virtual void CleanAllChildren()
		{
			_tilemaps = GetComponentsInChildren<Tilemap>();

			foreach (Tilemap tilemap in _tilemaps)
			{
				tilemap.ClearAllTiles();
			}
                
		}
	}    
}
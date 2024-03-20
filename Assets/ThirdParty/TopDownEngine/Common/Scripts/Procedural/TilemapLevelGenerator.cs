using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  MoreMountains.Tools;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 레벨의 빈 개체에 추가된 이 구성 요소는 고유하고 무작위 타일맵 생성을 처리합니다.
    /// </summary>
    public class TilemapLevelGenerator : MMTilemapGenerator
	{
		[FormerlySerializedAs("GenerateOnStart")]
		[Header("TopDown Engine Settings")]
		/// Whether or not this level should be generated automatically on Awake
		[Tooltip("이 레벨이 Awake에서 자동으로 생성되어야 하는지 여부")]
		public bool GenerateOnAwake = false;

		[Header("Bindings")] 
		/// the Grid on which to work
		[Tooltip("작업할 그리드")]
		public Grid TargetGrid;
		/// the tilemap containing the walls
		[Tooltip("벽이 포함된 타일맵")]
		public Tilemap ObstaclesTilemap; 
		/// the tilemap containing the walls' shadows
		[Tooltip("벽의 그림자가 포함된 타일맵")]
		public MMTilemapShadow WallsShadowTilemap;
		/// the level manager
		[Tooltip("레벨 매니저")]
		public LevelManager TargetLevelManager;

		[Header("Spawn")] 
		/// the object at which the player will spawn
		[Tooltip("플레이어가 스폰될 객체")]
		public Transform InitialSpawn;
		/// the exit of the level
		[Tooltip("레벨의 출구")]
		public Transform Exit;
		/// the minimum distance that should separate spawn and exit.
		[Tooltip("생성과 종료를 분리해야 하는 최소 거리입니다.")]
		public float MinDistanceFromSpawnToExit = 2f;

		protected const int _maxIterationsCount = 100;
        
		/// <summary>
		/// On awake we generate our level if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (GenerateOnAwake)
			{
				Generate();
			}
		}

		/// <summary>
		/// Generates a new level
		/// </summary>
		public override void Generate()
		{
			base.Generate();
			HandleWallsShadow();
			PlaceEntryAndExit();
			ResizeLevelManager();
		}

		/// <summary>
		/// Resizes the level manager's bounds to match the new level
		/// </summary>
		protected virtual void ResizeLevelManager()
		{
			BoxCollider boxCollider = TargetLevelManager.GetComponent<BoxCollider>();
            
			Bounds bounds = ObstaclesTilemap.localBounds;
			boxCollider.center = bounds.center;
			boxCollider.size = new Vector3(bounds.size.x, bounds.size.y, boxCollider.size.z);
		}

		/// <summary>
		/// Moves the spawn and exit to empty places
		/// </summary>
		protected virtual void PlaceEntryAndExit()
		{
			UnityEngine.Random.InitState(GlobalSeed);
			int width = UnityEngine.Random.Range(GridWidth.x, GridWidth.y);
			int height = UnityEngine.Random.Range(GridHeight.x, GridHeight.y);
            
			Vector3 spawnPosition = MMTilemap.GetRandomPosition(ObstaclesTilemap, TargetGrid, width, height, false, width * height * 2);
			InitialSpawn.transform.position = spawnPosition;

			Vector3 exitPosition = spawnPosition;
			int iterationsCount = 0;
            
			while ((Vector3.Distance(exitPosition, spawnPosition) < MinDistanceFromSpawnToExit) && (iterationsCount < _maxIterationsCount))
			{
				exitPosition = MMTilemap.GetRandomPosition(ObstaclesTilemap, TargetGrid, width, height, false, width * height * 2);
				Exit.transform.position = exitPosition;
				iterationsCount++;
			}
		}
        
		/// <summary>
		/// Copies the contents of the Walls layer to the WallsShadows layer to get nice shadows automatically
		/// </summary>
		protected virtual void HandleWallsShadow()
		{
			if (WallsShadowTilemap != null)
			{
				WallsShadowTilemap.UpdateShadows();
			}
		}
	}    
}
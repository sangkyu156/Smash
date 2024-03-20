using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools 
{
    /// <summary>
    /// 이 클래스는 레이어 조합으로 생성된 데이터로 타일맵을 채웁니다.
    /// </summary>
    [ExecuteAlways]  
	public class MMTilemapGenerator : MonoBehaviour
	{
		[Header("Grid")] 
		/// The width of the grid, in cells
		[Tooltip("그리드의 너비(셀)")]
		[MMVector("Min","Max")]
		public Vector2Int GridWidth = new Vector2Int(50,50);
		/// the height of the grid, in cells
		[Tooltip("그리드의 높이(셀)")]
		[MMVector("Min","Max")]
		public Vector2Int GridHeight = new Vector2Int(50,50);

		[Header("Data")] 
		/// the list of layers that will be used to generate the tilemap
		[Tooltip("타일맵을 생성하는 데 사용될 레이어 목록")]
		public MMTilemapGeneratorLayerList Layers;

		/// a value between 0 and 1 that will be used by all layers as their random seed. If you generate another map using the same seed, it'll look the same 
		[Tooltip("모든 레이어에서 무작위 시드로 사용되는 0에서 1 사이의 값입니다. 동일한 시드를 사용하여 다른 맵을 생성하면 동일하게 보입니다.")]
		public int GlobalSeed = 0;
		/// whether or not to randomize the global seed every time a new map is generated
		[Tooltip("새 맵이 생성될 때마다 전역 시드를 무작위로 지정할지 여부")]
		public bool RandomizeGlobalSeed = true;
        
		[Header("Slow Render")] 
		/// turning this to true will (at runtime only) draw the map progressively. This is really just for fun.
		[Tooltip("이것을 true로 설정하면 (런타임에만) 지도가 점진적으로 그려집니다. 이것은 단지 재미를 위한 것입니다.")]
		public bool SlowRender = false;
		/// the duration of the slow render, in seconds
		[Tooltip("느린 렌더링 기간(초)")]
		public float SlowRenderDuration = 1f;
		/// the tween to use for the slow render
		[Tooltip("느린 렌더링에 사용할 트윈")]
		public MMTweenType SlowRenderTweenType = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
        
		protected int[,] _grid;

		protected int _width;
		protected int _height;

		/// <summary>
		/// the possible methods that can be used to generate a random grid
		/// </summary>
		public enum GenerateMethods
		{
			Full,
			Perlin,
			PerlinGround,
			Random,
			RandomWalk,
			RandomWalkAvoider,
			RandomWalkGround,
			Path,
			Copy
		}
        
		/// <summary>
		/// Generates and renders every layer in the data stack
		/// </summary>
		public virtual void Generate()
		{
			Random.InitState((int)System.DateTime.Now.Ticks);
			if (RandomizeGlobalSeed) { GlobalSeed = Mathf.Abs(Random.Range(int.MinValue, int.MaxValue)); }
            
			foreach (MMTilemapGeneratorLayer layer in Layers)
			{
				GenerateLayer(layer);   
			}
		}

		/// <summary>
		/// On reset, we initialize our list
		/// </summary>
		void Reset()
		{
			Layers = new MMTilemapGeneratorLayerList(){
				new MMTilemapGeneratorLayer()
			};
		}

		/// <summary>
		/// Generates a layer grid, and renders it
		/// </summary>
		/// <param name="layer"></param>
		protected virtual void GenerateLayer(MMTilemapGeneratorLayer layer)
		{
			if (!layer.Active)
			{
				return;
			}
            
			if (layer.TargetTilemap == null) { Debug.LogError("Tilemap Generator : you need to specify a Target Tilemap to paint on."); }
			if (layer.Tile == null) { Debug.LogError("Tilemap Generator : you need to specify a Tile to paint with."); }
			if (layer.GridWidth == 0) { Debug.LogError("Tilemap Generator : grid width can't be 0."); }
			if (layer.GridHeight == 0) { Debug.LogError("Tilemap Generator : grid height can't be 0."); }

			float seedFloat = 0f;
			float layerSeedFloat = 0f;
			float globalSeedFloat = 0f;
            
            
			UnityEngine.Random.InitState(GlobalSeed);
			int width = layer.OverrideGridSize ? layer.GridWidth : UnityEngine.Random.Range(GridWidth.x, GridWidth.y);
			int height = layer.OverrideGridSize ? layer.GridHeight : UnityEngine.Random.Range(GridHeight.x, GridHeight.y);
            
			globalSeedFloat = UnityEngine.Random.value;
            
			// random outside of the global seed 
			if (layer.DoNotUseGlobalSeed)
			{
				Random.InitState((int)System.DateTime.Now.Ticks);
				if (layer.RandomizeSeed)
				{
					layer.Seed = Mathf.Abs(Random.Range(int.MinValue, int.MaxValue));
				}
				UnityEngine.Random.InitState(layer.Seed);
				layerSeedFloat = UnityEngine.Random.value;
			}
            
			int seed = layer.DoNotUseGlobalSeed ? layer.Seed : GlobalSeed;
			seedFloat = layer.DoNotUseGlobalSeed ? layerSeedFloat : globalSeedFloat;

			switch (layer.GenerateMethod)
			{
				case GenerateMethods.Full:
					_grid = MMGridGeneratorFull.Generate(width, height, layer.FullGenerationFilled);
					layer.Grid = _grid;
					break;
				case GenerateMethods.Perlin:
					_grid = MMGridGeneratorPerlinNoise.Generate(width, height, seedFloat);
					layer.Grid = _grid;
					break;
				case GenerateMethods.PerlinGround:
					_grid = MMGridGeneratorPerlinNoiseGround.Generate(width, height, seedFloat);
					layer.Grid = _grid;
					break;
				case GenerateMethods.Random:
					_grid = MMGridGeneratorRandom.Generate(width, height, seed, layer.RandomFillPercentage);
					layer.Grid = _grid;
					break;
				case GenerateMethods.RandomWalk:
					_grid = MMGridGeneratorRandomWalk.Generate(width, height, seed, layer.RandomWalkPercent, layer.RandomWalkStartingPoint, layer.RandomWalkMaxIterations);
					layer.Grid = _grid;
					break;
				case GenerateMethods.RandomWalkAvoider:

					int[,] obstacleGrid = MMGridGenerator.TilemapToGrid(layer.RandomWalkAvoiderObstaclesTilemap, width, height);
					_grid = MMGridGeneratorRandomWalkAvoider.Generate(width, height, seed, layer.RandomWalkAvoiderPercent, layer.RandomWalkAvoiderStartingPoint, obstacleGrid, layer.RandomWalkAvoiderObstaclesDistance, layer.RandomWalkAvoiderMaxIterations);
					layer.Grid = _grid;
					break;
				case GenerateMethods.RandomWalkGround:
					_grid = MMGridGeneratorRandomWalkGround.Generate(width, height, seed, 
						layer.RandomWalkGroundMinHeightDifference, layer.RandomWalkGroundMaxHeightDifference, 
						layer.RandomWalkGroundMinFlatDistance, layer.RandomWalkGroundMaxFlatDistance, layer.RandomWalkGroundMaxHeight);
					layer.Grid = _grid;
					break;
				case GenerateMethods.Path:
					_grid = MMGridGeneratorPath.Generate(width, height, seed, layer.PathDirection, layer.PathStartPosition, layer.PathMinWidth,
						layer.PathMaxWidth, layer.PathDirectionChangeDistance, layer.PathWidthChangePercentage,
						layer.PathDirectionChangePercentage);
					layer.Grid = _grid;
					break;
				case GenerateMethods.Copy:
					layer.TargetTilemap.ClearAllTiles();
					DelayedCopy(layer);
					break;
			}

			if (layer.Smooth) { _grid = MMGridGenerator.SmoothenGrid(_grid); }
			if (layer.InvertGrid) { _grid = MMGridGenerator.InvertGrid(_grid); }

			_grid = MMGridGenerator.BindGrid(_grid, layer.BoundsTop, layer.BoundsBottom, layer.BoundsLeft, layer.BoundsRight);
			_grid = MMGridGenerator.ApplySafeSpots(_grid, layer.SafeSpots);

			RenderGrid(layer);
		}

		/// <summary>
		/// Copies the tilemap's content after a delay because Unity.
		/// </summary>
		/// <param name="layer"></param>
		async static void DelayedCopy(MMTilemapGeneratorLayer layer)
		{
			await Task.Delay(500);
			MMTilemapShadow.Copy(layer.CopyTilemap, layer.TargetTilemap);
		}
        
		/// <summary>
		/// Renders the grid with the selected modes to the specified target tilemap
		/// </summary>
		/// <param name="layer"></param>
		protected virtual void RenderGrid(MMTilemapGeneratorLayer layer)
		{
			MMTilemapGridRenderer.RenderGrid(_grid, layer, SlowRender, SlowRenderDuration, SlowRenderTweenType,this);
		}
        
		/// <summary>
		/// Sets default values for all layers
		/// </summary>
		protected virtual void OnValidate()
		{
			if ((Layers == null) || (Layers.Count <= 0))
			{
				return;
			}
			foreach (MMTilemapGeneratorLayer layer in Layers)
			{
				layer.SetDefaults();
			}
		}
	}    
}
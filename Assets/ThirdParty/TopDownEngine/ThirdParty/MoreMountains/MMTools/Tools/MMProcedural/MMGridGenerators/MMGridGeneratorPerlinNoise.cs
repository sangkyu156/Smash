using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 시드된 펄린 노이즈를 기반으로 지정된 크기의 그리드를 생성합니다. 시드가 작을수록 그리드는 더 차단됩니다.
    /// </summary>
    public class MMGridGeneratorPerlinNoise : MMGridGenerator 
	{
           
		/// <summary>
		/// Generates a grid of the specified size based on a seeded perlin noise, the smaller the seed, the blockier the grid
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		public static int[,] Generate(int width, int height, float seed)
		{
			int[,] grid = PrepareGrid(ref width, ref height);
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					int value = Mathf.RoundToInt(Mathf.PerlinNoise(i * seed, j * seed));
					SetGridCoordinate(grid, i, j, value);
				}
			}
			return grid;
		}
	}
}
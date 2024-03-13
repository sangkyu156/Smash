using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스는 MMSpawnAround 클래스에서 사용되는 생성 속성을 설명하는 데 사용됩니다.
    /// 일반적으로 전리품 시스템과 같은 개체를 생성하도록 설계된 클래스에 의해 노출되고 사용되도록 되어 있습니다.
    /// </summary>
    [System.Serializable]
	public class MMSpawnAroundProperties
	{
        /// 가능한 모양 개체가 생성될 수 있습니다.
        public enum MMSpawnAroundShapes { Sphere, Cube }
        /// 객체가 생성되어야 하는 모양
        [Header("Shape")] 
		[Tooltip("객체가 생성되어야 하는 모양")]
		public MMSpawnAroundShapes Shape = MMSpawnAroundShapes.Sphere;

		[Header("Position")]
        /// 객체를 생성하려는 평면에 대한 법선을 지정하는 Vector3(x/z 평면에 객체를 생성하려는 경우 해당 평면의 법선은 y축(0,1,0)이 됩니다.)
        [Tooltip("객체를 생성하려는 평면에 대한 법선을 지정하는 Vector3(x/z 평면에 객체를 생성하려는 경우 해당 평면의 법선은 y축(0,1,0)이 됩니다.)")]
		public Vector3 NormalToSpawnPlane = Vector3.up;
        /// 객체가 스폰될 수 있는 스폰 원점까지의 최소 거리
        [Tooltip("객체가 스폰될 수 있는 스폰 원점까지의 최소 거리")]
		[MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Sphere)]
		public float MinimumSphereRadius = 1f;
        /// 객체가 스폰될 수 있는 스폰 원점까지의 최대 거리
        [Tooltip("객체가 스폰될 수 있는 스폰 원점까지의 최대 거리")]
		[MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Sphere)]
		public float MaximumSphereRadius = 2f;
        /// 큐브 밑면의 최소 크기
        [Tooltip("큐브 밑면의 최소 크기")]
		[MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Cube)]
		public Vector3 MinimumCubeBaseSize = Vector3.one;
        /// 큐브 밑면의 최대 크기
        [Tooltip("큐브 밑면의 최대 크기")]
		[MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Cube)]
		public Vector3 MaximumCubeBaseSize = new Vector3(2f, 2f, 2f);

		[Header("NormalAxisOffset")]
        /// 법선 축에 적용할 최소 오프셋
        [Tooltip("법선 축에 적용할 최소 오프셋")]
		public float MinimumNormalAxisOffset = 0f;
        /// 일반 축에 적용할 최대 오프셋
        [Tooltip("일반 축에 적용할 최대 오프셋")]
		public float MaximumNormalAxisOffset = 0f;

		[Header("NormalAxisOffsetCurve")]
        /// 스폰 평면을 따라 오브젝트의 스폰 위치를 오프셋하기 위해 곡선을 사용할지 여부
        [Tooltip("스폰 평면을 따라 오브젝트의 스폰 위치를 오프셋하기 위해 곡선을 사용할지 여부")]
		public bool UseNormalAxisOffsetCurve = false;
        /// 원점까지의 거리를 어떻게 변경해야 하는지 정의하는 데 사용되는 곡선(잠재적으로 최소/최대 거리 이상)
        [Tooltip("원점까지의 거리를 어떻게 변경해야 하는지 정의하는 데 사용되는 곡선(잠재적으로 최소/최대 거리 이상)")]
		[MMCondition("UseNormalAxisOffsetCurve",true)]
		public AnimationCurve NormalOffsetCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 1f));
        /// 곡선의 0이 다시 매핑되어야 하는 값
        [Tooltip("곡선의 0이 다시 매핑되어야 하는 값")]
		[MMCondition("UseNormalAxisOffsetCurve",true)]
		public float NormalOffsetCurveRemapZero = 0f;
        /// 곡선의 값을 다시 매핑해야 하는 값
        [Tooltip("곡선의 값을 다시 매핑해야 하는 값")]
		[MMCondition("UseNormalAxisOffsetCurve",true)]
		public float NormalOffsetCurveRemapOne = 1f;
        /// 곡선을 반전할지 여부(수평)
        [Tooltip("곡선을 반전할지 여부(수평)")]
		[MMCondition("UseNormalAxisOffsetCurve",true)]
		public bool InvertNormalOffsetCurve = false;

		[Header("Rotation")]
        /// 적용할 최소 임의 회전(도 단위)
        [Tooltip("적용할 최소 임의 회전(도 단위)")]
		public Vector3 MinimumRotation = Vector3.zero;
        /// 적용할 최대 무작위 회전(도 단위)
        [Tooltip("적용할 최대 무작위 회전(도 단위)")]
		public Vector3 MaximumRotation = Vector3.zero;

		[Header("Scale")]
        /// 적용할 최소 무작위 척도
        [Tooltip("적용할 최소 무작위 척도")]
		public Vector3 MinimumScale = Vector3.one;
        /// 적용할 최대 무작위 스케일
        [Tooltip("적용할 최대 무작위 스케일")]
		public Vector3 MaximumScale = Vector3.one;
	}

    /// <summary>
    /// 이 정적 클래스는 개체를 인스턴스화해야 할 때 위치, 회전 및 배율을 무작위화하는 데 유용한 생성 도우미입니다.	/// </summary>
    public static class MMSpawnAround 
	{
		public static void ApplySpawnAroundProperties(GameObject instantiatedObj, MMSpawnAroundProperties props, Vector3 origin)
		{            
			// we randomize the position
			instantiatedObj.transform.position = SpawnAroundPosition(props, origin);
			// we randomize the rotation
			instantiatedObj.transform.rotation = SpawnAroundRotation(props);
			// we randomize the scale
			instantiatedObj.transform.localScale = SpawnAroundScale(props);
		}

        /// <summary>
        /// 객체가 생성되어야 하는 위치를 반환합니다.
        /// </summary>
        /// <param name="props"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static Vector3 SpawnAroundPosition(MMSpawnAroundProperties props, Vector3 origin)
		{
            // 정의된 평면과 거리를 기반으로 물체의 위치를 ​​얻습니다.
            Vector3 newPosition;
			if (props.Shape == MMSpawnAroundProperties.MMSpawnAroundShapes.Sphere)
			{
				float distance = Random.Range(props.MinimumSphereRadius, props.MaximumSphereRadius);
				newPosition = Vector3.Cross(Random.insideUnitSphere, props.NormalToSpawnPlane);
				newPosition.Normalize();
				newPosition *= distance;
			}
			else
			{
				float randomX = Random.Range(props.MinimumCubeBaseSize.x, props.MaximumCubeBaseSize.x);
				newPosition.x = Random.Range(-randomX, randomX) / 2f;
				float randomY = Random.Range(props.MinimumCubeBaseSize.y, props.MaximumCubeBaseSize.y);
				newPosition.y = Random.Range(-randomY, randomY) / 2f;
				float randomZ = Random.Range(props.MinimumCubeBaseSize.z, props.MaximumCubeBaseSize.z);
				newPosition.z = Random.Range(-randomZ, randomZ) / 2f;
				newPosition = Vector3.Cross(newPosition, props.NormalToSpawnPlane); 
			}

			float randomOffset = Random.Range(props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset);
            // NormalOffsetCurve를 기반으로 위치를 수정합니다.
            if (props.UseNormalAxisOffsetCurve)
			{
				float normalizedOffset = 0f;
				if (randomOffset != 0)
				{
					if (props.InvertNormalOffsetCurve)
					{
						normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 1f, 0f);
					}
					else
					{
						normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 0f, 1f);
					}
				}

				float offset = props.NormalOffsetCurve.Evaluate(normalizedOffset);
				offset = MMMaths.Remap(offset, 0f, 1f, props.NormalOffsetCurveRemapZero, props.NormalOffsetCurveRemapOne);

				newPosition *= offset;
			}
            // 일반 오프셋을 적용합니다
            newPosition += props.NormalToSpawnPlane.normalized * randomOffset;

            // 상대 위치
            newPosition += origin;

			return newPosition;
		}

        /// <summary>
        /// 객체가 생성되어야 하는 스케일을 반환합니다.
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static Vector3 SpawnAroundScale(MMSpawnAroundProperties props)
		{
			return MMMaths.RandomVector3(props.MinimumScale, props.MaximumScale);
		}

        /// <summary>
        /// 객체가 생성되어야 하는 회전을 반환합니다.
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static Quaternion SpawnAroundRotation(MMSpawnAroundProperties props)
		{
			return Quaternion.Euler(MMMaths.RandomVector3(props.MinimumRotation, props.MaximumRotation));
		}

        /// <summary>
        /// 생성 영역의 모양을 표시하는 장치를 그립니다.
        /// </summary>
        /// <param name="props"></param>
        /// <param name="origin"></param>
        /// <param name="quantity"></param>
        /// <param name="size"></param>
        public static void DrawGizmos(MMSpawnAroundProperties props, Vector3 origin, int quantity, float size, Color gizmosColor)
		{
			Gizmos.color = gizmosColor;
			for (int i = 0; i < quantity; i++)
			{
				Gizmos.DrawCube(SpawnAroundPosition(props, origin), SpawnAroundScale(props) * size);
			}
		}
	}
}
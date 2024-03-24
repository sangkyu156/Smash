using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
	[AddComponentMenu("More Mountains/Tools/Object Bounds/MMObjectBounds")]
	public class MMObjectBounds : MonoBehaviour
	{
		public enum WaysToDetermineBounds { Collider, Collider2D, Renderer, Undefined }

		[Header("Bounds")]
		public WaysToDetermineBounds BoundsBasedOn;  


		public Vector3 Size { get; set; }

        /// <summary>
        /// 이 구성 요소가 추가되면 해당 범위를 정의합니다.
        /// </summary>
        protected virtual void Reset() 
		{
			DefineBoundsChoice();
		}

        /// <summary>
        /// 경계의 기반이 무엇인지 자동으로 결정하려고 합니다.
        /// 이 순서대로 Collider2D, Collider 또는 Renderer 중 마지막으로 발견된 항목을 유지합니다.
        /// 이들 중 어느 것도 발견되지 않으면 정의되지 않음으로 설정됩니다.
        /// </summary>
        protected virtual void DefineBoundsChoice()
		{
			BoundsBasedOn = WaysToDetermineBounds.Undefined;
			if (GetComponent<Renderer>()!=null)
			{
				BoundsBasedOn = WaysToDetermineBounds.Renderer;
			}
			if (GetComponent<Collider>()!=null)
			{
				BoundsBasedOn = WaysToDetermineBounds.Collider;
			}
			if (GetComponent<Collider2D>()!=null)
			{
				BoundsBasedOn = WaysToDetermineBounds.Collider2D;
			}
		}

        /// <summary>
        /// 정의된 내용을 기반으로 객체의 경계를 반환합니다.
        /// </summary>
        public virtual Bounds GetBounds()
		{
			if (BoundsBasedOn==WaysToDetermineBounds.Renderer)
			{
				if (GetComponent<Renderer>()==null)
				{
					throw new Exception("The PoolableObject "+gameObject.name+" is set as having Renderer based bounds but no Renderer component can be found.");
				}
				return GetComponent<Renderer>().bounds;
			}

			if (BoundsBasedOn==WaysToDetermineBounds.Collider)
			{
				if (GetComponent<Collider>()==null)
				{
					throw new Exception("The PoolableObject "+gameObject.name+" is set as having Collider based bounds but no Collider component can be found.");
				}
				return GetComponent<Collider>().bounds;				
			}

			if (BoundsBasedOn==WaysToDetermineBounds.Collider2D)
			{
				if (GetComponent<Collider2D>()==null)
				{
					throw new Exception("The PoolableObject "+gameObject.name+" is set as having Collider2D based bounds but no Collider2D component can be found.");
				}
				return GetComponent<Collider2D>().bounds;				
			}

			return new Bounds(Vector3.zero,Vector3.zero);
		}



	}
}
using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	[RequireComponent(typeof(SpriteRenderer))]
    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 시작 시 레이어에서 새 순서를 선택하게 됩니다. 고유한 정렬 레이어 번호를 갖는 데 유용합니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Sprites/MMAutoOrderInLayer")]
	public class MMAutoOrderInLayer : MonoBehaviour 
	{
		static int CurrentMaxCharacterOrderInLayer = 0;

		[Header("Global Counter")]
		[MMInformation("스프라이트 렌더러가 있는 개체에 이 구성 요소를 추가하면 여기에 정의된 설정에 따라 레이어에 새로운 순서가 부여됩니다. 첫 번째는 전역 카운터 증분, 즉 동일한 레이어에 있는 두 객체 사이의 레이어 순서를 얼마나 증가시키려는지입니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the number by which to increment each new object's order in layer
		public int GlobalCounterIncrement = 5;

		[Header("Parent")]
		[MMInformation("또한 상위 스프라이트의 순서에 따라 새 레이어 순서를 결정할 수도 있습니다(동일한 레이어에 있어야 함).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// if this is true, the new order in layer value will be based on the highest order value found on a parent with a similar sorting layer
		public bool BasedOnParentOrder = false;
		/// if BasedOnParentOrder is true, the new value will be the parent's order value + this value
		public int ParentIncrement = 1;

		[Header("Children")]
		[MMInformation("그리고 여기에서 모든 하위 항목에 새 레이어 순서를 적용할지 결정할 수 있습니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// if this is true, the new order value will be passed to all children with a similar sorting layer
		public bool ApplyNewOrderToChildren = false;
		/// the value by which the new order value should be incremented to pass it to children
		public int ChildrenIncrement = 0;

		protected SpriteRenderer _spriteRenderer;

		/// <summary>
		/// On Start, we get our sprite renderer and determine the new order in layer
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
			AutomateLayerOrder();
		}

		/// <summary>
		/// Gets the sprite renderer component and stores it
		/// </summary>
		protected virtual void Initialization()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
		}

		/// <summary>
		/// Picks a new order in layer based on the inspector's settings
		/// </summary>
		protected virtual void AutomateLayerOrder()
		{
			int newOrder = 0;

			// if there's no sprite renderer on this object, we do nothing and exit
			if (_spriteRenderer == null)
			{
				return;
			}

			// if we're supposed to base our new order in layer value on the parent's value
			if (BasedOnParentOrder)
			{
				int maxLayerOrder = 0;
				Component[] spriteRenderers = GetComponentsInParent( typeof(SpriteRenderer) );

				// we look for all sprite renderers in parent objects
				if( spriteRenderers != null )
				{
					foreach( SpriteRenderer spriteRenderer in spriteRenderers )
					{
						// if we find a parent with a sprite renderer, on the same sorting layer and with a higher sorting value than previously found
						if ( (spriteRenderer.sortingLayerID == _spriteRenderer.sortingLayerID)
						     && (spriteRenderer.sortingOrder > maxLayerOrder))
						{							
							// we store the new value
							maxLayerOrder = spriteRenderer.sortingOrder;							
						}
					}	
					// we set our new value to the highest value found, plus our increment
					newOrder = maxLayerOrder + ParentIncrement;                
				}
			}
			else
			{
				// if we're not based on parent, we base our pick on the current max order in layer
				newOrder = CurrentMaxCharacterOrderInLayer + GlobalCounterIncrement;
				// we increment the global order index
				CurrentMaxCharacterOrderInLayer += GlobalCounterIncrement;
			}

			// we apply our new order value
			_spriteRenderer.sortingOrder = newOrder;

			// if we need to apply that new value to all children, we do it
			if (ApplyNewOrderToChildren)
			{
				Component[] childrenSpriteRenderers = GetComponentsInChildren( typeof(SpriteRenderer) );
				if( childrenSpriteRenderers != null )
				{
					foreach( SpriteRenderer childSpriteRenderer in childrenSpriteRenderers )
					{
						if (childSpriteRenderer.sortingLayerID == _spriteRenderer.sortingLayerID)
						{
							childSpriteRenderer.sortingOrder = newOrder + ChildrenIncrement;
						}
					}	              
				}
			}
		}
	}
}
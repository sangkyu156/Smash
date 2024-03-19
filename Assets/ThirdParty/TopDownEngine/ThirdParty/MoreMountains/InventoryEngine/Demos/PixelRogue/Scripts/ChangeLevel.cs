using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 한 레벨에서 다른 레벨로 이동하는 데모 수업
    /// </summary>
    public class ChangeLevel : MonoBehaviour 
	{
        /// ChangeLevel 존에 들어갈 때 갈 장면의 정확한 이름
        [MMInformation("이 데모 구성 요소를 BoxCollider2D에 추가하면 캐릭터가 충돌체에 들어갈 때 아래 필드에 지정된 장면으로 장면이 변경됩니다.", MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("ChangeLevel 존에 들어갈 때 갈 장면의 정확한 이름")]
		public string Destination;

		/// <summary>
		/// When a character enters the ChangeLevel zone, we trigger a general save and then load the destination scene
		/// </summary>
		/// <param name="collider">Collider.</param>
		public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			if ((Destination != null) && (collider.gameObject.GetComponent<InventoryDemoCharacter>() != null))
			{
				MMGameEvent.Trigger("Save");
				SceneManager.LoadScene(Destination);
			}
		}
	}
}
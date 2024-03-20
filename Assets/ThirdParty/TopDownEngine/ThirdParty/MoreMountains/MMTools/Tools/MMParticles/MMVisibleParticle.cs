using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 입자에 추가하여 정렬 레이어를 강제합니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Particles/MMVisibleParticle")]
	public class MMVisibleParticle : MonoBehaviour {

		/// <summary>
		/// Sets the particle system's renderer to the Visible Particles sorting layer
		/// </summary>
		protected virtual void Start () 
		{
			GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = "VisibleParticles";
		}		
	}
}
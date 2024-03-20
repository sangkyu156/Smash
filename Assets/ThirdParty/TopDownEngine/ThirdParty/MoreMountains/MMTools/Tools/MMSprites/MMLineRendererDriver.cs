using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 라인 렌더러에 추가된 이 구성 요소를 사용하면 변환 목록을 채우고 해당 위치를 라이너 렌더러의 위치에 바인딩할 수 있습니다.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
	public class MMLineRendererDriver : MonoBehaviour
	{
		[Header("Position Drivers")]

		/// the list of targets - their quantity has to match the LineRenderer's positions count
		public List<Transform> Targets;
		/// whether or not to keep both in sync at update
		public bool BindPositionsToTargetsAtUpdate = true;

		[Header("Binding")]

		/// a test button
		[MMInspectorButton("Bind")]
		public bool BindButton;

		protected LineRenderer _lineRenderer;
		protected bool _countsMatch = false;

		/// <summary>
		/// On Awake we initialize our driver
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs the line renderer, tests counts
		/// </summary>
		protected virtual void Initialization()
		{
			_lineRenderer = this.gameObject.GetComponent<LineRenderer>();
			_countsMatch = CheckPositionCounts();
			if (!_countsMatch)
			{
				Debug.LogWarning(this.name + ", MMLineRendererDriver's Targets list doesn't have the same amount of entries as the LineRender's Positions array. It won't work.");
			}
		}

		/// <summary>
		/// On Update we bind our positions to targets if needed
		/// </summary>
		protected virtual void Update()
		{
			if (BindPositionsToTargetsAtUpdate)
			{
				BindPositionsToTargets();
			}
		}

		/// <summary>
		/// A method meant to be called by the inspector button
		/// </summary>
		protected virtual void Bind()
		{
			Initialization();
			BindPositionsToTargets();
		}

		/// <summary>
		/// Goes through all the targets and assigns their positions to the LineRenderer's positions
		/// </summary>
		public virtual void BindPositionsToTargets()
		{
			if (!_countsMatch)
			{
				return;
			}

			for (int i = 0; i < Targets.Count; i++)
			{
				_lineRenderer.SetPosition(i, Targets[i].position);
			}
		}

		/// <summary>
		/// Makes sure the counts match
		/// </summary>
		/// <returns></returns>
		protected virtual bool CheckPositionCounts()
		{
			return Targets.Count == _lineRenderer.positionCount;
		}
	}
}
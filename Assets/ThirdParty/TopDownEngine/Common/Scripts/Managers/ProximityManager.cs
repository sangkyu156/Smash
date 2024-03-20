using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// ProximityManaged 클래스가 있는 개체를 찾고 해당 설정에 따라 개체를 활성화/비활성화하는 클래스입니다.
    /// 이 클래스는 객체가 많은 큰 장면을 처리하고 멀리 있는 객체를 비활성화하는 방법의 예를 보여줍니다.
    /// 성능을 절약하기 위해 작업에서 벗어났습니다.
    /// 이를 수행하는 방법은 여러 가지가 있습니다. 이 방법은 간단하고 일반적이므로 특정 사용 사례에 더 나은 선택이 있을 수 있습니다.
    /// </summary>
    public class ProximityManager : MMSingleton<ProximityManager>, MMEventListener<TopDownEngineEvent>
    {
        [Header("Target")]

        /// whether or not to automatically grab the player from the LevelManager once the scene loads
        [Tooltip("장면이 로드되면 LevelManager에서 플레이어를 자동으로 가져올지 여부")]
        public bool AutomaticallySetPlayerAsTarget = true;
        /// the target to detect proximity with
        [Tooltip("근접성을 감지할 대상")]
        public Transform ProximityTarget;
        /// in this mode, if there's no ProximityTarget, proximity managed objects will be disabled  
        [Tooltip("이 모드에서 ProximityTarget이 없으면 근접성 관리 개체가 비활성화됩니다.")]
        public bool RequireProximityTarget = true;

        [Header("EnableDisable")]

        /// whether or not to automatically grab all ProximityManaged objects in the scene
        [Tooltip("장면의 모든 ProximityManaged 개체를 자동으로 가져올지 여부")]
        public bool AutomaticallyGrabControlledObjects = true;
        /// the list of objects to check proximity with
        [Tooltip("근접성을 확인할 객체 목록")]
        public List<ProximityManaged> ControlledObjects;
        
        [Header("Tick")]

        /// the frequency, in seconds, at which to evaluate distances and enable/disable stuff
        [Tooltip("거리를 평가하고 항목을 활성화/비활성화하는 빈도(초)")]
        public float EvaluationFrequency = 0.5f;

        protected float _lastEvaluationAt = 0f;

        /// <summary>
        /// On start we grab our controlled objects
        /// </summary>
        protected virtual void Start()
        {
            GrabControlledObjects();
        }

        /// <summary>
        /// Grabs all proximity managed objects in the scene
        /// </summary>
        protected virtual void GrabControlledObjects()
        {
            if (AutomaticallyGrabControlledObjects)
            {
                var items = FindObjectsOfType<ProximityManaged>();
                foreach(ProximityManaged managed in items)
                {
                    managed.Manager = this;
                    ControlledObjects.Add(managed);
                }
            }
        }
        
        /// <summary>
        /// A public method used to add new controlled objects at runtime
        /// </summary>
        /// <param name="newObject"></param>
        public virtual void AddControlledObject(ProximityManaged newObject)
        {
            ControlledObjects.Add(newObject);
        }

        /// <summary>
        /// Grabs the player from the level manager
        /// </summary>
        protected virtual void SetPlayerAsTarget()
        {
            if (AutomaticallySetPlayerAsTarget)
            {
                ProximityTarget = LevelManager.Instance.Players[0].transform;
                _lastEvaluationAt = 0f;
            }            
        }

        /// <summary>
        /// On Update we check our distances
        /// </summary>
        protected virtual void Update()
        {
            EvaluateDistance();
        }

        /// <summary>
        /// Checks distances if needed
        /// </summary>
        protected virtual void EvaluateDistance()
        {
            if (ProximityTarget == null)
            {
                if (RequireProximityTarget)
                {
                    foreach (ProximityManaged proxy in ControlledObjects)
                    {
                        if (proxy.gameObject.activeInHierarchy)
                        {
                            proxy.gameObject.SetActive(false);
                            proxy.DisabledByManager = true;
                        }
                    }
                }
                return;
            }
            
            if (Time.time - _lastEvaluationAt > EvaluationFrequency)
            {
                _lastEvaluationAt = Time.time;
            }
            else
            {
                return;
            }
            foreach(ProximityManaged proxy in ControlledObjects)
            {
                float distance = Vector3.Distance(proxy.transform.position, ProximityTarget.position);
                if (proxy.gameObject.activeInHierarchy && (distance > proxy.DisableDistance))
                {
                    proxy.gameObject.SetActive(false);
                    proxy.DisabledByManager = true;
                }
                if (!proxy.gameObject.activeInHierarchy && proxy.DisabledByManager && (distance < proxy.EnableDistance))
                {
                    proxy.gameObject.SetActive(true);
                    proxy.DisabledByManager = false;
                }
            }
        }

        /// <summary>
        /// When we get a level start event, we assign our player as a target
        /// </summary>
        /// <param name="engineEvent"></param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            if ((engineEvent.EventType == TopDownEngineEventTypes.SpawnComplete)
                || (engineEvent.EventType == TopDownEngineEventTypes.CharacterSwap))
            {
                SetPlayerAsTarget();
            }
        }

        /// <summary>
        /// On enable we start listening for events
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
        }

        /// <summary>
        /// On disable we stop listening for events
        /// </summary>
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// ProximityManaged Ŭ������ �ִ� ��ü�� ã�� �ش� ������ ���� ��ü�� Ȱ��ȭ/��Ȱ��ȭ�ϴ� Ŭ�����Դϴ�.
    /// �� Ŭ������ ��ü�� ���� ū ����� ó���ϰ� �ָ� �ִ� ��ü�� ��Ȱ��ȭ�ϴ� ����� ���� �����ݴϴ�.
    /// ������ �����ϱ� ���� �۾����� ������ϴ�.
    /// �̸� �����ϴ� ����� ���� ������ �ֽ��ϴ�. �� ����� �����ϰ� �Ϲ����̹Ƿ� Ư�� ��� ��ʿ� �� ���� ������ ���� �� �ֽ��ϴ�.
    /// </summary>
    public class ProximityManager : MMSingleton<ProximityManager>, MMEventListener<TopDownEngineEvent>
    {
        [Header("Target")]

        /// whether or not to automatically grab the player from the LevelManager once the scene loads
        [Tooltip("����� �ε�Ǹ� LevelManager���� �÷��̾ �ڵ����� �������� ����")]
        public bool AutomaticallySetPlayerAsTarget = true;
        /// the target to detect proximity with
        [Tooltip("�������� ������ ���")]
        public Transform ProximityTarget;
        /// in this mode, if there's no ProximityTarget, proximity managed objects will be disabled  
        [Tooltip("�� ��忡�� ProximityTarget�� ������ ������ ���� ��ü�� ��Ȱ��ȭ�˴ϴ�.")]
        public bool RequireProximityTarget = true;

        [Header("EnableDisable")]

        /// whether or not to automatically grab all ProximityManaged objects in the scene
        [Tooltip("����� ��� ProximityManaged ��ü�� �ڵ����� �������� ����")]
        public bool AutomaticallyGrabControlledObjects = true;
        /// the list of objects to check proximity with
        [Tooltip("�������� Ȯ���� ��ü ���")]
        public List<ProximityManaged> ControlledObjects;
        
        [Header("Tick")]

        /// the frequency, in seconds, at which to evaluate distances and enable/disable stuff
        [Tooltip("�Ÿ��� ���ϰ� �׸��� Ȱ��ȭ/��Ȱ��ȭ�ϴ� ��(��)")]
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
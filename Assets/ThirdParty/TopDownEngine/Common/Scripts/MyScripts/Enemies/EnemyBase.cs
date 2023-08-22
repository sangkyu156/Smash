using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class EnemyBase : MonoBehaviour, IPoolObject
    {
        //ó������
        public virtual void OnCreatedInPool()
        {

        }

        //��Ȱ��ȭ
        public virtual void OnGettingFromPool()
        {

        }

        void Start()
        {

        }

        void Update()
        {

        }
    }
}
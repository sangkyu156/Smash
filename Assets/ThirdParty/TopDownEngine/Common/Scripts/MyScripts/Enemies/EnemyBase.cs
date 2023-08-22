using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class EnemyBase : MonoBehaviour, IPoolObject
    {
        //처음시작
        public virtual void OnCreatedInPool()
        {

        }

        //재활성화
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : EnemyBase
{
    public string idName = "Slime";

    void Start()
    {

    }

    void Update()
    {

    }

    //오브젝트 비활성화
    public void OnTargetReached()
    {
        if (gameObject.activeSelf)
        {
            CreateManager.Instance.ReturnPool(this);
            Debug.Log("실행됨1");
        }
    }

    public override void OnCreatedInPool()
    {
        base.OnCreatedInPool();

    }

    public override void OnGettingFromPool()
    {
        base.OnGettingFromPool();
        Debug.Log("실행됨2");
    }
}




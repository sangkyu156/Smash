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

    //������Ʈ ��Ȱ��ȭ
    public void OnTargetReached()
    {
        if (gameObject.activeSelf)
        {
            CreateManager.Instance.ReturnPool(this);
            Debug.Log("�����1");
        }
    }

    public override void OnCreatedInPool()
    {
        base.OnCreatedInPool();

    }

    public override void OnGettingFromPool()
    {
        base.OnGettingFromPool();
        Debug.Log("�����2");
    }
}




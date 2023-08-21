using Redcode.Pools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateManager : MonoBehaviour
{
    PoolManager poolManager; //������Ʈ Ǯ�� �Ŵ���

    static CreateManager c_instance; // ���ϼ��� ����ȴ�
    static public CreateManager Instance { get { /*Init();*/ return c_instance; } } // ������ �Ŵ����� ����´�
    
    float createTime = 0;

    private void Awake()
    {
        c_instance = this.GetComponent<CreateManager>();
        poolManager = GetComponent<PoolManager>();
    }

    void Start()
    {

    }

    private void Update()
    {
        createTime += Time.deltaTime;

        if (createTime > 1.5f)
        {
            createTime = 0;
            SlimeSpawn();
        }
    }

    static public void Init()
    {
        if (c_instance == null)
        {
            //GameObject go = GameObject.Find("@CreateManager");
            //if (go == null)
            //{
            //    go = Instantiate("Object/CreateManager");
            //}

            //DontDestroyOnLoad(go);
            //c_instance = go.GetComponent<CreateManager>();
        }
    }

    static public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    static public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }

    //������Ʈ ����
    public void SlimeSpawn()
    {
        Slime slime = poolManager.GetFromPool<Slime>();
    }

    //������Ʈ ȸ��
    public void ReturnPool(Slime clone)
    {
        poolManager.TakeToPool<Slime>(clone.idName, clone);
    }
}


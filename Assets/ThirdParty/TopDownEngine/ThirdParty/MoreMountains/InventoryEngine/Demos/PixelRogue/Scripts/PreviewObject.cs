using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewObject : MonoBehaviour
{
    private List<Collider> colliderList = new List<Collider>();

    [SerializeField]
    private int layerGround; 
    public MeshRenderer myMaterial;

    [SerializeField]
    private Material green;
    [SerializeField]
    private Material red;

    public float colliderSize;

    private void Awake()
    {
        myMaterial = GetComponent<MeshRenderer>();

        colliderSize = GetCapsuleColliderSizeFloat(this.gameObject.GetComponent<CapsuleCollider>());
    }

    void Update()
    {
        OnTriggerEnter2();
        //ChangeColor();
    }

    private void ChangeColor()
    {
        //if (colliderList.Count > 0)
        //    SetColor(red);
        //else
        //    SetColor(green);

        if (colliderList.Count > 0)
        {
            myMaterial.material = red;
            for (int i = 0; i < colliderList.Count; i++)
            {
                Debug.Log($"���� colliderList�� ����ִ� ������Ʈ = {colliderList[i].name}");
            }
        }
        else
            myMaterial.material = green;
    }

    private void SetColor(Material mat)
    {
        foreach (Transform tf_Child in this.transform)
        {
            Material[] newMaterials = new Material[tf_Child.GetComponent<Renderer>().materials.Length];

            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = mat;
            }

            tf_Child.GetComponent<Renderer>().materials = newMaterials;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.layer != layerGround)
    //        colliderList.Add(other);
    //}

    private void OnTriggerEnter2()
    {
        // ���� GameObject ������ �ݰ� 1.0f �ȿ� �ִ� Collider���� ��� ã���ϴ�.
        Collider[] colliders = Physics.OverlapSphere(transform.position, colliderSize);

        // ��� ã�� Collider���� ��ȸ�ϸ鼭 ó���մϴ�.
        foreach (Collider collider in colliders)
        {
            // �ڱ� �ڽŰ��� �浹�� �����մϴ�.
            if (collider == GetComponent<Collider>())
                continue;

            // ���⿡�� ���ϴ� ������ ����
            HandleCollision(collider);
        }
    }

    // �浹 ó���� ����ϴ� �Լ�
    void HandleCollision(Collider otherCollider)
    {
        if (otherCollider.gameObject.layer != layerGround)
            myMaterial.material = red;
        else
            myMaterial.material = green;
    }

    //�ݶ��̴� ������ ��ȭ�鿡 �׸���
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        // ���� ��ü�� ��ġ�� ��ü�� �׸��ϴ�.
        Gizmos.DrawWireSphere(transform.position, colliderSize);
    }

    //ĸ���ݶ��̴� ũ�⸦ float������ ��ȯ �ϴ� �Լ�
    float GetCapsuleColliderSizeFloat(CapsuleCollider capsuleCollider)
    {
        // CapsuleCollider�� ũ��� height�� radius�� ������ �����Ͽ� ���
        float size = capsuleCollider.radius * 2;

        return size;
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.layer != layerGround)
    //        colliderList.Remove(other);
    //}

    public bool isBuildable()
    {
        return colliderList.Count == 0;
    }
}
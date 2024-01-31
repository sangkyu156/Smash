using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewObject : MonoBehaviour
{
    [SerializeField]
    private int layerGround; 
    public MeshRenderer myMaterial;

    [SerializeField]
    private Material green;
    [SerializeField]
    private Material red;

    public float colliderSize;
    public bool isInstallable;

    private void Awake()
    {
        myMaterial = GetComponent<MeshRenderer>();

        colliderSize = GetCapsuleColliderSizeFloat(this.gameObject.GetComponent<CapsuleCollider>());
        layerGround = 9;
    }

    void Update()
    {
        OnTriggerEnter2();
    }

    private void OnTriggerEnter2()
    {
        // ���� GameObject ������ �ݰ� colliderSize �ȿ� �ִ� Collider���� ��� ã���ϴ�.
        Collider[] colliders = Physics.OverlapSphere(transform.position, colliderSize);

        // ��� ã�� Collider���� ��ȸ�ϸ鼭 ó���մϴ�.
        foreach (Collider collider in colliders)
        {
            // ���⿡�� ���ϴ� ������ ����
            HandleCollision(collider);
        }
    }

    // �浹 ó���� ����ϴ� �Լ�
    void HandleCollision(Collider otherCollider)
    {
        if (otherCollider.gameObject.layer != layerGround)
        {
            myMaterial.material = red;
            isInstallable = false;
        }
        else
        {
            myMaterial.material = green;
            isInstallable = true;
        }
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
}
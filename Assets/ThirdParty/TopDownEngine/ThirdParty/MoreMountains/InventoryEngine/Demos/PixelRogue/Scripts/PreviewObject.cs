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
    private CapsuleCollider capsuleCollider;

    private void Awake()
    {
        myMaterial = GetComponent<MeshRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider>();

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
        // ĸ�� �ݶ��̴��� �������� ���� ���
        Vector3 startPoint = transform.position + Vector3.up * capsuleCollider.height / 2 - Vector3.up * capsuleCollider.radius * 15;
        Vector3 endPoint = transform.position - Vector3.up * capsuleCollider.height / 2 + Vector3.up * capsuleCollider.radius * 15;

        // ĸ�� �ݶ��̴��� ��ġ�� ������Ʈ ã��
        Collider[] colliders = Physics.OverlapCapsule(startPoint, endPoint, capsuleCollider.radius * 15);

        //Collider[] colliders = Physics.OverlapSphere(transform.position, colliderSize);

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
        if (capsuleCollider != null)
        {
            // ĸ�� �ݶ��̴��� �������� ���� ���
            Vector3 startPoint = transform.position - Vector3.up * capsuleCollider.height / 2 + Vector3.up * capsuleCollider.radius * 15;

            // �������� ������ �������� ĸ�� ������ �� �׸���
            Gizmos.DrawWireSphere(startPoint, capsuleCollider.radius * 15);
        }

        // ���� ��ü�� ��ġ�� ��ü�� �׸��ϴ�.
        //Gizmos.DrawWireSphere(transform.position, colliderSize);
    }

    //ĸ���ݶ��̴� ũ�⸦ float������ ��ȯ �ϴ� �Լ�
    float GetCapsuleColliderSizeFloat(CapsuleCollider capsuleCollider)
    {
        // CapsuleCollider�� ũ��� height�� radius�� ������ �����Ͽ� ���
        float size = capsuleCollider.radius * 2;

        return size;
    }
}
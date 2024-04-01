using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ES3AutoSaveMgr;

public class PreviewObject2 : MonoBehaviour
{
    [SerializeField]
    private int layerGround;
    public MeshRenderer myMaterial;

    [SerializeField]
    private Material green;
    [SerializeField]
    private Material red;

    public Vector3 colliderSize;
    public bool isInstallable;
    private SphereCollider sphereCollider;

    private void Awake()
    {
        myMaterial = GetComponent<MeshRenderer>();
        sphereCollider = GetComponent<SphereCollider>();

        colliderSize = GetSphereColliderSize(sphereCollider);
        layerGround = 9;
    }

    void Update()
    {
        OnTriggerEnter2();
    }

    private void OnTriggerEnter2()
    {
        Vector3 center = transform.position + sphereCollider.center;

        // sphereCollider�� ��ġ�� ������Ʈ ã��
        Collider[] colliders = Physics.OverlapSphere(center, sphereCollider.radius);

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
        if (sphereCollider != null)
        {
            // SphereCollider�� �߽� ��ġ�� ����մϴ�.
            Vector3 center = transform.position + sphereCollider.center;

            // SphereCollider�� ũ�⸦ �׸��ϴ�.
            Gizmos.DrawWireSphere(center, sphereCollider.radius);
        }
    }

    //SphereCollider�� �������� �����ͼ� Vector3 ���·� ��ȯ �ϴ� �Լ�
    Vector3 GetSphereColliderSize(SphereCollider sphereCollider)
    {
        // SphereCollider�� �������� �����ͼ� Vector3 ���·� ��ȯ�մϴ�.
        Vector3 colliderSize = Vector3.one * sphereCollider.radius * 2f;
        return colliderSize;
    }
}
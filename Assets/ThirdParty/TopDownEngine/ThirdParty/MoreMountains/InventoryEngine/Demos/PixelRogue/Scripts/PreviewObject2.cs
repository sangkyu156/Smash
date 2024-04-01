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

        // sphereCollider와 겹치는 오브젝트 찾기
        Collider[] colliders = Physics.OverlapSphere(center, sphereCollider.radius);

        // 모든 찾은 Collider들을 순회하면서 처리합니다.
        foreach (Collider collider in colliders)
        {
            // 여기에서 원하는 동작을 수행
            HandleCollision(collider);
        }
    }

    // 충돌 처리를 담당하는 함수
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

    //콜라이더 사이즈 씬화면에 그리기
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (sphereCollider != null)
        {
            // SphereCollider의 중심 위치를 계산합니다.
            Vector3 center = transform.position + sphereCollider.center;

            // SphereCollider의 크기를 그립니다.
            Gizmos.DrawWireSphere(center, sphereCollider.radius);
        }
    }

    //SphereCollider의 반지름을 가져와서 Vector3 형태로 반환 하는 함수
    Vector3 GetSphereColliderSize(SphereCollider sphereCollider)
    {
        // SphereCollider의 반지름을 가져와서 Vector3 형태로 반환합니다.
        Vector3 colliderSize = Vector3.one * sphereCollider.radius * 2f;
        return colliderSize;
    }
}
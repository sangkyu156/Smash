using MoreMountains.TopDownEngine;
using UnityEngine;

namespace PolygonArsenal
{

    public class PolygonBeamStatic : MonoBehaviour
    {

        [Header("Prefabs")]
        public GameObject beamLineRendererPrefab; //여기에 라인 렌더러가 포함된 프리팹을 배치합니다
        public GameObject beamStartPrefab; //이는 빔의 시작 부분에 배치되는 프리팹입니다.
        public GameObject beamEndPrefab; //빔 끝에 조립식을 배치합니다.

        public Transform target;//쏠곳

        private GameObject beamStart;
        private GameObject beamEnd;
        private GameObject beam;
        private LineRenderer line;
        Vector3 end;

        [Header("Beam Options")]
        public bool alwaysOn = true; //스크립트가 로드될 때 빔을 생성하려면 이 옵션을 활성화하세요.
        public bool beamCollides = true; //빔이 충돌기에서 멈춤
        public float beamLength = 100; //게임 내 빔 길이
        public float beamEndOffset = 0f; //최종 효과가 레이캐스트 히트 포인트에서 얼마나 멀리 떨어져 있는지
        public float textureScrollSpeed = 0f; //텍스처가 빔을 따라 스크롤하는 속도는 음수 또는 양수일 수 있습니다.
        public float textureLengthScale = 1f;   //수직을 기준으로 텍스처의 수평 길이로 설정합니다.
                                                //예: 텍스처가 높이 200픽셀, 길이 600픽셀인 경우 이를 3으로 설정합니다.

        void Start()
        {
            
        }

        private void OnEnable()
        {
            if (alwaysOn) //이 스크립트가 연결된 개체가 활성화되면 빔을 생성합니다.
                SpawnBeam();
        }

        private void OnDisable() //이 스크립트가 연결된 개체가 비활성화된 경우 빔을 제거하십시오.
        {
            RemoveBeam();
        }

        void FixedUpdate()
        {
            if (beam) //빔을 업데이트합니다.
            {
                if(target !=  null)
                    ShootLaser(this.transform.position, GetDirection(transform.position, target.position + new Vector3(0, 0.5f, 0)));
            }
        }

        void ShootLaser(Vector3 startPos, Vector3 direction)
        {
            Ray ray = new Ray(startPos, direction);
            RaycastHit hitInfo;
            line.SetPosition(0, startPos);
            // 레이저를 발사하고 충돌 여부를 확인합니다.
            if (Physics.Raycast(ray, out hitInfo))
            {
                // 레이저가 어떤 물체와 충돌했을 때 처리할 내용을 작성합니다.
                //Debug.Log("Hit object: " + hitInfo.collider.gameObject.name);

                // 여기서 추가적인 처리를 할 수 있습니다. 예를 들어, 충돌 지점에 파티클을 생성하거나 피격 효과를 재생할 수 있습니다.
            }

            // 레이저 시각화를 위해 LineRenderer를 사용합니다.
            line.SetPosition(1, hitInfo.point);

            if (beamStart)
            {
                beamStart.transform.position = transform.position;
                beamStart.transform.LookAt(hitInfo.point);
            }
            if (beamEnd)
            {
                beamEnd.transform.position = hitInfo.point + new Vector3(0, 0.5f, 0);
                beamEnd.transform.LookAt(beamStart.transform.position);
            }

            float distance = Vector3.Distance(transform.position, hitInfo.point);
            line.material.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
            line.material.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
        }

        Vector3 GetDirection(Vector3 from, Vector3 to)
        {
            // from에서 to까지의 방향을 반환합니다.
            return (to - from).normalized;
        }

        public void SpawnBeam() //이 함수는 linerenderer를 사용하여 프리팹을 생성합니다.
        {
            if (beamLineRendererPrefab)
            {
                if (beamStartPrefab)
                    beamStart = Instantiate(beamStartPrefab); //이거 나중에 Instantiate이걸로 생성하지말고 SetActive로 끄고 켜야함 그래야 성능좋아짐
                if (beamEndPrefab)
                    beamEnd = Instantiate(beamEndPrefab);
                beam = Instantiate(beamLineRendererPrefab);
                beam.transform.position = transform.position;
                beam.transform.parent = transform;
                beam.transform.rotation = transform.rotation;
                line = beam.GetComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = 2;
            }
            else
                print("Add a hecking prefab with a line renderer to the SciFiBeamStatic script on " + gameObject.name + "! Heck!");
        }

        public void RemoveBeam() //이 기능은 linerenderer를 사용하여 프리팹을 제거합니다.
        {
            if (beam)
                Destroy(beam);
            if (beamStart)
                Destroy(beamStart);
            if (beamEnd)
                Destroy(beamEnd);
        }
    }
}
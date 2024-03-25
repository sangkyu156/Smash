using UnityEngine;

namespace PolygonArsenal
{

    public class PolygonBeamStatic : MonoBehaviour
    {

        [Header("Prefabs")]
        public GameObject beamLineRendererPrefab; //여기에 라인 렌더러가 포함된 프리팹을 배치합니다
        public GameObject beamStartPrefab; //이는 빔의 시작 부분에 배치되는 프리팹입니다.
        public GameObject beamEndPrefab; //빔 끝에 조립식을 배치합니다.

        public Transform DirectionMarker;//쏠곳

        private GameObject beamStart;
        private GameObject beamEnd;
        private GameObject beam;
        private LineRenderer line;

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
                line.SetPosition(0, transform.position);

                Vector3 end = DirectionMarker.position;
                RaycastHit hit;
                if (beamCollides && Physics.Raycast(transform.position, transform.forward, out hit)) //충돌 확인
                    end = hit.point - (transform.forward * beamEndOffset);
                else
                    end = transform.position + (transform.forward * beamLength);

                line.SetPosition(1, end);

                if (beamStart)
                {
                    beamStart.transform.position = transform.position;
                    beamStart.transform.LookAt(end);
                }
                if (beamEnd)
                {
                    beamEnd.transform.position = end;
                    beamEnd.transform.LookAt(beamStart.transform.position);
                }

                float distance = Vector3.Distance(transform.position, end);
                line.material.mainTextureScale = new Vector2(distance / textureLengthScale, 1); //텍스처의 크기를 설정하여 늘어나 보이지 않게 합니다.
                line.material.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0); //0으로 설정되지 않은 경우 빔을 따라 텍스처를 스크롤합니다.
            }
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
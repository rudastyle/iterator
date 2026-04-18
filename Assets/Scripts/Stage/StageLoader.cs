using UnityEngine;

namespace TimeLoop
{
    /// <summary>
    /// StageData 를 받아 씬에 오브젝트를 동적으로 빌드/교체.
    /// 반환값으로 GameManager 가 PressurePlate 와 ExitDoor 참조를 받음.
    /// </summary>
    public class StageLoader : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] GameObject _platformPrefab;
        [SerializeField] GameObject _pressurePlatePrefab;
        [SerializeField] GameObject _exitDoorPrefab;

        Transform _root;

        public void Build(StageData data,
                          out PressurePlate[] plates,
                          out ExitDoor        door)
        {
            // 이전 스테이지 오브젝트 제거
            if (_root != null) Destroy(_root.gameObject);
            _root = new GameObject("StageRoot").transform;

            // 카메라 배경색
            Camera.main.backgroundColor = data.backgroundColor;

            // 플랫폼 생성
            foreach (var p in data.platforms)
            {
                var go = Instantiate(_platformPrefab, p.center, Quaternion.identity, _root);
                go.transform.localScale = new Vector3(p.size.x, p.size.y, 1f);
            }

            // 버튼(PressurePlate) 생성
            plates = new PressurePlate[data.buttons.Length];
            for (int i = 0; i < data.buttons.Length; i++)
            {
                var b  = data.buttons[i];
                var go = Instantiate(_pressurePlatePrefab, b.center, Quaternion.identity, _root);
                var pp = go.GetComponent<PressurePlate>();
                pp.SetColor(b.indicatorColor);
                pp.SetHoldDuration(b.holdDuration);
                plates[i] = pp;
            }

            // 출구 문 생성
            var doorGO = Instantiate(_exitDoorPrefab, data.door.center, Quaternion.identity, _root);
            doorGO.transform.localScale = new Vector3(data.door.size.x, data.door.size.y, 1f);
            door = doorGO.GetComponent<ExitDoor>();
        }
    }
}

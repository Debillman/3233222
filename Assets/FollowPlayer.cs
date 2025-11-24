using UnityEngine;

public class FollowPlayerY : MonoBehaviour
{
    // 유니티 인스펙터에서 플레이어 오브젝트의 Transform을 연결할 변수
    public Transform playerTransform;

    // StarField가 플레이어보다 얼마나 위에 위치할지 결정하는 오프셋
    public float yOffset = 15f;

    void LateUpdate()
    {
        // 플레이어 트랜스폼이 연결되어 있는지 확인
        if (playerTransform != null)
        {
            // StarField의 새로운 위치를 계산합니다.
            Vector3 newPosition = new Vector3(
                transform.position.x, // 현재 X 좌표는 유지
                playerTransform.position.y + yOffset, // 플레이어의 Y 좌표에 오프셋을 더함
                transform.position.z  // 현재 Z 좌표는 유지
            );

            // StarField의 위치를 업데이트합니다.
            transform.position = newPosition;
        }
    }
}
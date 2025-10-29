using UnityEngine;
using UnityEngine.EventSystems; // 이 네임스페이스가 마우스 이벤트를 처리하는 데 필요합니다!

// IPointerClickHandler 인터페이스를 구현합니다.
public class RightClickButton : MonoBehaviour, IPointerClickHandler
{
    // 버튼을 클릭했을 때(마우스 버튼을 눌렀다 떼었을 때) 호출되는 함수입니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        // eventData.button을 사용하여 어떤 마우스 버튼이 눌렸는지 확인합니다.
        // 0: 좌클릭 (Left)
        // 1: 우클릭 (Right)
        // 2: 휠 클릭 (Middle)

        // 마우스 우클릭(1번)인지 확인합니다.
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("우클릭이 감지되어 버튼이 눌렸습니다!");

            //  여기에 **실제로 실행하고 싶은 기능**을 작성하면 됩니다.
            // 예를 들어:
            // GetComponent<Button>().onClick.Invoke(); // 일반 Button의 OnClick 이벤트를 강제 실행
            // GameManager.Instance.OpenContextMenu(); // 특정 게임 기능을 실행
        }

        // 이 스크립트는 좌클릭(0번)이 들어와도 아무것도 하지 않습니다.
    }

    // Start()와 Update()는 필요 없으므로 지워도 되지만, 남겨두셔도 상관없습니다.
}
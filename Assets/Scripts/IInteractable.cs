// 플레이어가 스페이스바로 상호작용할 수 있는 모든 대상(책상, 문 등)이 구현하는 공통 인터페이스.
// PlayerDeskInteractor가 구체 타입(DeskPuzzle, ExitDoor ...)을 몰라도 되게 해서
// 새 상호작용 오브젝트를 추가할 때 감지 로직을 다시 짤 필요가 없도록 한다.
public interface IInteractable
{
    // 상호작용 전에 연필로 치는 모션을 재생할지 여부 (책상: true, 문: false)
    bool PlaysHitMotion { get; }

    // 실제 상호작용 동작 (책상이면 퀴즈/규칙 열기, 문이면 탈출 조건 확인 등)
    void Interact();
}

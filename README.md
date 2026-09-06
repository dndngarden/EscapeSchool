# 🗝️ EscapeSchool

교실에 배치된 책상 10개 중 랜덤으로 배정된 문제 3개를 풀어 비밀번호 조각을 모으고, 다 모으면 문으로 탈출하는 2D 탑다운 방탈출 퍼즐 게임입니다.

## 기술 스택

| 항목 | 내용 |
|---|---|
| 엔진 | Unity 6 (2D URP) |
| 언어 | C# |
| 주요 시스템 | New Input System, Physics2D, TextMeshPro, Coroutine |

## 게임 루프

이동 → 근접 상호작용(Space) → 책상 치기(문제/규칙) → 문제 풀이 → 비밀번호 조각 획득 → 조각을 다 모으면 문에서 탈출

- 책상 10개 중 매 게임 시작 시 3개만 랜덤으로 문제 배정, 나머지는 교실 규칙 문구 표시 → **리플레이 가치**
- 오답 시 책상 복구(재도전 가능), 정답 시 책상 파괴 확정 + 비밀번호 조각 획득
- 모은 조각 수를 탈출문에서 검증해야 클리어 → **누적 보상이 실제 클리어 조건으로 연결**

## 아키텍처 하이라이트

- **역할별 클래스 분리**: 이동(`PlayerController`) / 타격 연출(`PlayerHitController`) / 상호작용 감지(`PlayerDeskInteractor`) / 책상 상태(`DeskPuzzle`) / 탈출 조건(`ExitDoor`) / 게임 흐름(`QuizManager`, `PasswordStorage`)으로 단일 책임 원칙을 적용했습니다.
- **`IInteractable` 인터페이스로 다형성 설계**: 상호작용 가능한 대상(책상, 탈출문)을 인터페이스로 추상화해서, 감지 로직(`PlayerDeskInteractor`)을 건드리지 않고도 새 상호작용 오브젝트를 추가할 수 있도록 설계했습니다. 처음엔 책상만 있었지만 탈출문을 추가하면서, 감지 코드를 복사하지 않고 인터페이스 구현만으로 확장했습니다.
- **중복 없는 랜덤 배정 알고리즘**: 리스트를 복사한 뒤 뽑아서 제거하는 방식(비복원추출)으로 매 플레이마다 다른 책상에 문제가 배정되도록 구현했습니다.
- **물리 기반 이동**: `transform.position +=` 대신 `Rigidbody2D.MovePosition()`을 `FixedUpdate`에서 호출해 콜라이더 관통(터널링)을 방지했습니다.
- **방어적 프로그래밍**: 모든 참조에 null 체크, `isBusy`/`isResolving` 플래그로 중복 입력·레이스 컨디션 방지.

더 자세한 설계 근거와 코드 스니펫은 [노션 아키텍처 문서](https://app.notion.com/p/3d3cc809b1e081ea8136fff109a10659)에 정리했습니다.

## 파일 구조

```
Assets/
├─ Scripts/
│  ├─ QuizQuestion.cs         # 문제 데이터 클래스
│  ├─ IInteractable.cs        # 상호작용 대상 공통 인터페이스
│  ├─ DeskPuzzle.cs           # 책상 상태 관리 (IInteractable)
│  ├─ ExitDoor.cs             # 탈출 조건 확인 (IInteractable)
│  ├─ QuizManger.cs           # 문제 배정 + 정답 검증 총괄
│  ├─ PasswordStorage.cs      # 비밀번호 누적 및 UI
│  ├─ PlayerController.cs     # 플레이어 이동
│  ├─ PlayerHitController.cs  # 연필 타격 모션
│  └─ PlayerDeskInteractor.cs # 근접 IInteractable 탐색 및 상호작용
├─ Prefabs/                   # desk1~3, Player
├─ Fonts/                     # 한글 표시용 TMP 폰트
└─ GameScene.unity
```

## 실행 방법

1. Unity Hub에서 `6000.3.6f1` 버전으로 프로젝트 열기
2. `Assets/GameScene.unity` 실행

## 개선 여지

- ScriptableObject로 문제 데이터 분리
- 정답/탈출 결과에 대한 이벤트 기반 알림 (`QuizManager.OnPuzzleSolved` 등)
- 저장 시스템, 사운드 피드백, EditMode 단위 테스트 추가

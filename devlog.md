# AI 협동 조형 게임 - 개발 일지 (DevLog)

## 📌 프로젝트 개요
* **프로젝트명**: AI 협동 조형 3D 게임 (AI Cooperative Sculpting Game)
* **엔진**: Unity 3D
* **핵심 기능**: 넓은 맵 탐색 + 3D 재료 수집 + 관성 플레이어 조작 + AI Vision 자동 채점 및 피드백

---

## 📅 개발 진행 상황 (Current Progress)

### 🟢 1단계: 기획 및 개발 계획 수립 (완료)
- [x] 게임 기본 컨셉 및 루프 정의 (주제 공개 -> 재료 탐색/수집 -> 시간 제한 조형 -> AI 채점 -> 결과 공개)
- [x] 기술 스택 선정 (Unity Netcode for GameObjects, LM Studio / Qwen2.5-VL -> OpenAI/Gemini API)
- [x] 개발 기획서 (`Assets/GameDesignDocument.md`) 작성 및 세부 시스템 명세 정의

### 🟢 2단계: 프로젝트 초기화 및 기반 마련 (완료)
- [x] 신규 Unity 3D 프로젝트 준비 및 개발 환경 구성
- [x] DevLog 문서 생성 및 관리 시작

### 🟢 3단계: 플레이어 & 관성 이동 시스템 (완료)
- [x] `PlayerController.cs`:
  - **부드러운 관성 가속/감속**: WASD 입력 시 미끄러지듯 자연스럽게 가속(`16.0`) 및 감속(`22.0`) 구현
  - **파묻힘 버그 완전 해결**: Visual 메쉬 피봇을 Y=+1.0으로 보정 → 발바닥이 바닥면에 밀착
  - **CS0266 컴파일 오류 수정**: `MeshRenderer` → `Renderer` 타입 불일치 버그 수정
- [x] `CameraController.cs`: 플레이어를 50도 각도로 내려다보는 탑다운 쿼터뷰 추적 카메라 및 마우스 휠 줌 지원

### 🟢 4단계: 맵 확장 & 재료 수집 시스템 (완료)
- [x] **맵 크기 대폭 확장**: 바닥 맵 **250m × 250m** (스폰 범위 220m 완전 커버)
- [x] **버그 수정**: 기존 씬에 `BuildingCanvasFloor`가 이미 존재할 때 스케일이 업데이트되지 않던 버그 수정 → `localScale` 설정을 `if(null)` 블록 바깥으로 이동하여 항상 강제 적용
- [x] `ResourceSpawner.cs`: 맵 외곽 **220m × 220m** 영역 곳곳에 60개 이상의 알록달록 3D 재료 스폰
  - 캔버스 중앙 내부 40m × 40m 구역에는 스폰 제외 (플레이어 시작 구역 확보)
- [x] `ResourceObject.cs` & `PlayerInventory.cs`: `E` 키로 재료를 수집하고 인벤토리에 보관/관리
- [x] `BuildingSystem.cs`: 소지품 재료 차감 캔버스 배치 & `우클릭`/`R` 키 분해 환수 지원

### 🟢 5단계: 라운드 시스템 (완료)
- [x] `TopicData.cs`: 라운드별 주제어, 목표 점수, 제한시간 데이터 구조화
- [x] `RoundManager.cs`: 라운드 진입, 카운트다운 타이머, 자동 채점 트리거
- [x] `GameManager.cs`: 실행 시 로비 패널 없이 **주제어 팝업(3초) ➔ 조형 샌드박스 직행** 루프 전환

### 🟢 6단계: UI & 폰트 시스템 (완료)
- [x] **글래스모피즘 HUD**: 반투명 어두운 블루/블랙 패널 스타일로 주제어, 타이머, 남은 개수, AI 채점 요청 버튼 시각화
- [x] `ObjectPaletteUI.cs`: 인벤토리 소지 수량 및 `[E] 재료 줍기` 하이라이트 안내
- [x] `AIResultUI.cs`: 캡처 캔버스 이미지, AI 점수, 통과/실패 도장, AI 평가 코멘트 연출 모달
- [x] **Galmuri11 폰트 전면 적용**: `GameSetupUtility.cs`에 `GetGalmuriFont()` / `ApplyGalmuriFont()` 헬퍼 추가
  - `Assets/Galmuri11 SDF.asset`을 `AssetDatabase`로 로드
  - 로비, 주제 발표, HUD, 채점 결과, 게임오버 등 **11개 전체 UI 텍스트**에 자동 적용
  - Auto Setup 실행 시 폰트가 자동으로 연결됨

### 🟢 7단계: AI 연동 시스템 (완료)
- [x] `AIVisionEvaluator.cs`:
  - 카메라 Render Texture 캡처 및 PNG/Base64 인코딩
  - UnityWebRequest 기반 HTTP REST API 통신 (LM Studio + Qwen2.5-VL / OpenAI Vision 연동)
  - 에디터 내 즉각적인 테스트를 위한 Mock 테스트 채점 모드 탑재

### 🟢 에디터 자동 셋업 유틸리티 (완료)
- [x] `GameSetupUtility.cs`:
  - `Tools > Sculpt Game > Auto Setup Scene & Managers` 원클릭 자동 구성
  - New Input System 전용 `InputSystemUIInputModule` 자동 바인딩
  - 바닥 스케일 강제 적용 버그 수정 (기존 오브젝트도 항상 업데이트)
  - 250m × 250m 대형 맵 자동 셋업

---

## 🐛 수정된 버그 목록

| 버그 | 원인 | 해결 |
|------|------|------|
| 플레이어가 바닥 절반에 파묻힘 | 캡슐 피봇이 중앙(0,0,0)이라 Y=0 착지 시 절반이 땅속에 들어감 | Visual 메쉬 자식 오브젝트를 Y=+1.0에 배치, CharacterController center도 (0,1,0) |
| CS0266 컴파일 오류 | `GetComponent<Renderer>()` 반환값을 `MeshRenderer` 변수에 할당 | `MeshRenderer` → `Renderer`로 변수 타입 수정 |
| 맵 스케일이 Auto Setup 재실행 시 업데이트 안됨 | `localScale` 설정이 `if(floorObj == null)` 블록 안에만 있어서 기존 오브젝트는 스킵됨 | `localScale` 설정을 블록 바깥으로 이동 → 항상 강제 적용 |
| 재료들이 바닥 맵 밖에 스폰됨 | 스폰 범위(220m)가 바닥 크기(60m/200m)보다 훨씬 넓었음 | 바닥을 250m × 250m으로 확장하여 스폰 범위 완전 커버 |

---

## 🎯 다음 작업 목표 (Next Up)

### 🟡 8단계: 멀티플레이 추가 (Unity NGO 연동)
- [ ] Unity Netcode for GameObjects (NGO) 패키지 셋업 및 NetworkManager 구성
- [ ] 플레이어 동기화 (`NetworkTransform` / `NetworkObject`)
- [ ] 흩어진 재료 수집 및 캔버스 오브젝트 배치 상태 멀티 동기화

---

## 📋 전체 개발 로드맵 & 체크리스트

- [x] **1단계: 기획 완료** (GDD 작성 완료)
- [x] **2단계: 프로젝트 생성** (기반 프로젝트 셋업 완료)
- [x] **3단계: 플레이어 & 관성 이동 제작** (관성 가속/감속, 발바닥 피봇 보정, 탑다운 쿼터뷰, CS0266 수정)
- [x] **4단계: 맵 확장 & 재료 수집** (250m × 250m 맵, 220m 스폰, E키 줍기, 인벤토리 차감/환수, 스케일 버그 수정)
- [x] **5단계: 라운드 시스템** (주제 관리, 카운트다운 타이머, 상태 제어)
- [x] **6단계: UI 제작 & 폰트** (세련된 HUD, 인벤토리 HUD, AI 채점 결과 모달, Galmuri11 전면 적용)
- [x] **7단계: AI 연동** (캔버스 스크린샷 캡처, LM Studio / Gemini API HTTP 연동, JSON 파싱)
- [ ] **8단계: 멀티플레이 추가** (Unity NGO 기반 플레이어, 오브젝트 배치, 라운드 동기화)
- [ ] **9단계: 테스트 및 버그 수정** (멀티플레이 동기화 검증, AI 응답 가속 및 패킷 최적화)
- [ ] **10단계: 밸런스 조정 및 폴리싱** (오브젝트 수 밸런싱, 효과음, 이펙트)

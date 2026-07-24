# AI 협동 조형 게임 - 개발 기획서 (Game Design Document)

## 1. 게임 컨셉 (Concept & Goals)

### 핵심 아이디어
* **협동 오브젝트 조형**: 플레이어들은 팀을 이루어 주어진 주제어(예: 자동차, 집, 동물 등)를 3D 오브젝트를 배치하고 조립하여 표현합니다.
* **AI 실시간/라운드 평가**: 제한 시간이 종료되면 현재 완성된 캔버스(작품)를 스크린샷 캡처하여 AI Vision 모델에 전달, AI가 주제와의 연관성 및 형태를 분석 및 평가합니다.
* **합격 / 불합격 판정**: AI의 채점 결과가 목표 점수 이상이면 라운드 통과, 미달 시 실패합니다.
* **라운드 반복 및 파티성**: 여러 라운드를 진행하며 난이도가 상승하거나 주제가 달라져, 친구들과 소통하며 웃고 즐길 수 있는 파티 게임을 목표로 합니다.

### 개발 목표
* **재미 요소**: 플레이어들의 우스꽝스럽거나 창의적인 조형물에 대한 AI의 재치 있는 평가 코멘트와 점수.
* **플레이 경험**: intuitive(직관적)한 3D 오브젝트 배치 방식과 멀티플레이 협동 요소.

---

## 2. 게임 플레이 흐름 및 규칙 (Game Flow & Rules)

### 게임 플레이 흐름
```
[메인 메뉴]
   ↓
[방 생성 / 참가 (로비)]
   ↓
[게임 시작 및 라운드 개시]
   ↓
[주제어 공개] (예: "자동차")
   ↓
[제한 시간 동안 캔버스 조형] (오브젝트 생성/이동/회전/삭제)
   ↓
[제한 시간 종료 & 캔버스 스크린샷 캡처]
   ↓
[AI 채점 및 코멘트 생성]
   ↓
[점수 및 AI 코멘트 연출 공개]
   ↓
[다음 라운드 진입 / 게임 종료 (최종 승리/패배)]
```

### 게임 규칙 (Game Rules)
* **제한 시간**: 라운드당 설정된 시간 (예: 180초).
* **오브젝트 개수 제한**: 성능 및 연산 최적화를 위해 라운드당/팀당 사용할 수 있는 총 오브젝트 수 제한.
* **통과 점수**: 라운드별 기준 점수 (예: 1라운드 60점, 2라운드 70점 등).
* **라운드 수**: 기본 3~5 라운드 구성.
* **오브젝트 종류**: 기본 도형(큐브, 구, 원통 등) 및 특수 프롭(바퀴, 눈장식, 지붕 등).
* **플레이어 수**: 2~4명 협동 플레이.

---

## 3. AI 채점 방식 (AI Scoring System Specification)

### Input (입력 데이터)
1. **캔버스 스크린샷 Image**: 카메라가 캔버스를 최적 각도에서 촬영한 Texture2D / PNG / Base64 이미지.
2. **주제어 Prompt**: 해당 라운드의 주제 (예: "자동차").

### Output (출력 데이터 - JSON Format)
* **점수 (Score)**: 0 ~ 100 점 사이의 정수.
* **통과 여부 (Passed)**: boolean (`true` / `false`).
* **평가 코멘트 (Comment)**: 작품에 대한 AI의 상세 묘사 및 평가 문장.

### AI 결과 예시
```json
{
  "score": 82,
  "passed": true,
  "comment": "바퀴와 차체 표현이 좋아 자동차처럼 보입니다."
}
```

---

## 4. 기술 스택 (Technical Stack)

| 구분 | 기술 / Tool | 비고 |
| :--- | :--- | :--- |
| **게임 엔진** | Unity 3D | 2022.3 LTS 이상 / 6 |
| **멀티플레이** | Unity Netcode for GameObjects (NGO) | RPC, NetworkVariable을 활용한 오브젝트/상태 동기화 |
| **개발 단계 AI** | LM Studio + Qwen2.5-VL | 로컬 환경에서 무료 테스트 및 빠른 반복 검증 |
| **출시/서비스 AI** | OpenAI API (GPT-4o Vision) 또는 Google Gemini API | 정식 버전용 고성능 비전 AI API 연동 |

---

## 5. 핵심 시스템 명세 (System Architecture)

### 1) 게임 및 라운드 시스템 (Game & Round System)
* **RoundManager**: 라운드 진행 상태(대기, 진행 중, 채점 중, 결과 표시, 종료) 제어.
* **TimerManager**: 동기화된 카운트다운 타이머.
* **ScoreManager**: 라운드별 점수 집계 및 통과 조건 판정.

### 2) 플레이어 시스템 (Player System)
* **PlayerController**: 3D 1인칭 또는 3인칭/쿼터뷰 이동 및 카메라 조작.
* **ObjectInteraction**: 오브젝트 선택, 레이캐스트(Raycast)를 통한 배치 위치 지정.

### 3) 오브젝트 배치 시스템 (Object Placement System)
* **Prefab Spawner**: 오브젝트 팝업 메뉴에서 선택하여 생성.
* **Transform Controller**: 배치 중인 오브젝트의 위치 이동, 회전, 스냅(Grid Snap), 삭제.
* **Collision Handler**: 오브젝트 간 겹침 방지 및 Physics/Grid 스냅 처리.

### 4) UI 시스템 (User Interface)
* **Main Menu / Lobby UI**: 방 생성/참가, 닉네임 설정.
* **HUD**: 제한 시간, 현재 주제어, 설치 가능 오브젝트 남은 개수.
* **Object Palette UI**: 사용 가능한 오브젝트 카테고리 및 선택 팔레트.
* **Result UI**: AI 캡처 화면, 점수 연출, AI 코멘트 텍스트 팝업.

### 5) 멀티플레이 시스템 (Multiplayer System)
* **NGO NetworkManager**: Host/Client 접속 및 세션 관리.
* **NetworkTransform / Custom RPC**: 배치된 오브젝트의 위치/회전/종류 실시간 동기화.

### 6) AI 연동 시스템 (AI Integration System)
* **Canvas Capture**: 지정된 Render Camera로 캔버스 캡처 후 Texture2D를 Base64/PNG로 변환.
* **HTTP Client (UnityWebRequest)**: LM Studio / OpenAI / Gemini API 서버로 비전 데이터 POST 요청.
* **JSON Parser**: 응답받은 JSON String을 DTO 객체로 디시리얼라이즈하여 UI에 전송.

---

## 6. MVP (최소 기능 제품) 정의

최초 프로토타입 검증을 위한 MVP 범위:
1. **단일/멀티 기본 플레이어 이동 및 카메라 조작**
2. **기본 3D 도형(큐브, 구 등) 배치 / 회전 / 삭제 기능**
3. **주제 표시 및 타이머 기능**
4. **제한시간 종료 시 화면 캡처 및 AI API (LM Studio 또는 OpenAI/Gemini) 채점 결과 수신**
5. **점수 및 AI 코멘트 UI 출력**

---

## 7. 추후 확장 아이디어 (Future Backlog)

* **게임성 강화**: 난이도 선택, 오브젝트 색상 변경 및 해금 요소, 다양한 AI 심사위원 페르소나(독설가, 칭찬봇 등).
* **커뮤니티 & 플랫폼**: 레플레이 기능, 창작물 갤러리, Steam Multiplayer 연동, Steam Workshop(창작마당) 지원, 랭킹 및 업적, 모드(Mod) 지원.

# Haderonia-Code
하데로니아 프로젝트 코드 저장소입니다.

## 코드 분류

### MainSystem
- **DI패턴**: 의존성 주입 컨테이너/인젝터 관련 코드
- **GameManager(전역으로 사용하는 기능들)**: 오디오, 입력, 시간, UI 등 전역 매니저 코드
- **NPC System**: NPC 기본 클래스와 모듈 구조
- **Player(플레이어 기능)**: 플레이어 본체, 컴포넌트, 스탯, 버프, UI 기능
- **RunTimePooling**: 런타임 오브젝트 풀링 시스템

### PlayerOutfitSystem
- **OutfitEditor (아웃핏 제작 에디터)**: 아웃핏 제작/편집 관련 기능
- **Player System (플레이어쪽 호환)**: 플레이어 시스템과의 연동 코드
- **PlayerOutFitBase (아웃핏에 필요한 것)**: 아웃핏 공통 베이스 구성
- **PlayerOutFitData (최종완성된 아웃핏들)**: 완성된 아웃핏 데이터

### 완성된 UI
- **Dialogue (NPC 대화)**: NPC 대화 UI
- **OutfitSelet (아웃핏 선택)**: 아웃핏 선택 UI

### Static
- **EnumTypes**: 프로젝트 공용 enum 타입 정의
- **DamageCalculator**: 데미지 계산 로직
- **AudioKeys / UIStringKeys**: 오디오 및 UI 문자열 키 상수

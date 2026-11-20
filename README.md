# Skul Exclusive Unlock Items (개발 초기 버전)

> **개발 현황 (2026-11-20):** 게임 내부 타입을 대상으로 한 Harmony 아이템 등록 실험 코드가 추가됐습니다. 실제 게임 빌드·실행 검증 전이며, 아이템별 전투 이벤트 효과와 자동 해금 훅은 아직 구현되지 않았습니다. 현재 프로젝트를 완성 모드로 설치하지 마세요.

> **상태: 실험적 아이템 등록 + 일부 기본 스탯. 아직 플레이 가능한 완성 모드가 아닙니다.**

원본 아이디어: [DC Inside 게시글](https://gall.dcinside.com/mgallery/board/view?id=skul&no=57505). 제공받은 6장의 스크린샷을 바탕으로 **스컬 6종 / 전용 아이템 12개**를 `data/items.ko.json`에 정리했습니다.

## 현재 포함된 것

- 12개 아이템의 이름, 등급, 각인, 설명, 알려진 수치와 6개 해금 조건
- BepInEx 5/Harmony 기반 C# 플러그인, 실험적인 아이템 데이터베이스 등록 및 해금 플래그 JSON 저장 코드
- 게임 버전과 실제 API를 파악한 뒤 연동해야 할 TODO

## 아직 미구현

- 알렉산더 사망 이벤트 감지, 처치 당시 스컬 판정, 해금 자동 발동
- 아이템 등록/드롭의 컴파일 및 실제 게임 검증, 상점 풀 편입, 정식 아이콘 표시
- 전투 효과 12개 및 각인 발동
- 게임 실행 테스트 및 배포 DLL/Thunderstore 패키징

**중요:** 스크린샷의 `완벽한 마석` 효과 설명은 보이지 않습니다. `파괴자의 검`의 증가 속도, `세 번째 눈`의 데미지/기절 지속시간 등 몇몇 구현 수치도 원문에 없습니다. 추측값을 사실처럼 사용하지 않았습니다.

## 소스 파일

- `data/items.ko.json`: 업로드된 이미지 6장에 근거한 아이템/해금 원문 사양
- `src/SkulExclusiveUnlocks/Plugin.cs`: BepInEx 플러그인 및 해금 저장 로직
- `src/SkulExclusiveUnlocks/SkulExclusiveUnlocks.csproj`: .NET Framework 4.7.2 프로젝트
- `docs/IMPLEMENTATION.md`: 실제 게임 연동 체크리스트

## 빌드(개발자용)

1. Windows에서 .NET Framework 4.7.2 타기팅 팩, C# 빌드 도구와 BepInEx 5 설치
2. `BEPINEX_CORE` 환경 변수를 해당 게임 설치의 `BepInEx/core` 폴더로 지정
3. `msbuild src/SkulExclusiveUnlocks/SkulExclusiveUnlocks.csproj /p:Configuration=Release`
4. **현재 빌드는 골격만 로드합니다.** DLL이 있어도 게임 내 새 아이템이나 해금은 구현되지 않았습니다.

BepInEx 모딩 선례: https://github.com/Tobi-Mob/Skul.Mod (해당 프로젝트는 이전 버전 기준이며 최신 호환을 보장하지 않음).

## 저작권

게시글의 기획과 시각 자료에 대한 권리는 원 작성자에게 있습니다. 이 저장소에는 원본 이미지 파일을 재배포하지 않습니다. 공개 배포 전 제작자의 허락과 게임 리소스 사용 조건을 확인하세요.

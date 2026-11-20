# 게임 기능 구현 현황 — v0.5.0 source snapshot

**중요:** GitHub 소스 업데이트만 완료. 이 환경에는 Skul 게임 `Assembly-CSharp.dll`, dotnet SDK 및 게임 실행 환경이 없으므로 **컴파일·런타임 검증을 전혀 하지 못했습니다.** 이 저장소의 DLL이 정상 동작한다고 보장하지 않습니다.

## 시도한 게임 연동

- Harmony `GearResource.Initialize` 커스텀 아이템 목록 주입
- `ItemReference.LoadAsync` / `GearReference.LoadAsync` / `LevelManager.DropItem` 게임 아이템 드롭 흐름 연동
- 커스텀 아이템의 기본 스탯·각인·한국어 표시 및 `AbilityAttacher` 부착
- `UnlockAllItemsForTesting` 옵션 (기본 false)
- `build.ps1`: Windows 게임 DLL 확인 후 빌드 명령 실행

## 새로 추가된 특수 전투 코드 (게임 미검증)

| 아이템 | 작성한 동작 | 소스 |
| --- | --- | --- |
| 은빛 모피 | 20초 쿨다운으로 유효한 한 번의 피해 무효화 | `SilverFurShield.cs` |
| 야생의 표식 | 추가 이동속도 %에 따른 물리/마법 공격력 동적 보너스 | `WildMarkSpeedPower.cs` |
| 거인의 식칼 | 회복 시 10초 동안 물리공격력 +75% | `HealTriggeredPhysicalBuff.cs` |
| 살점 핵 | 유효 피해 50% 감소 및 동일 플레이어에 대해 1회 사망 부활 | `FleshCoreGuard.cs` |
| 만능 플라스크 | 유효 공격 15%로 5가지 상태이상 중 1개 부여 | `UniversalFlaskProc.cs` |
| 루비 심장 | 교대 후 3초 상처 부여 및 출혈 피해로 3명 처치 시 HP 1 회복 | `RubyHeartWound.cs` |\n| 파괴자의 검 | 스킬 사용 중 물리 공격력/치명타 증가, 스킬 완료/취소 시 해제 (상승 속도 **임시 3초**) | `DestroyerBladeCharge.cs` |

**7종 모두 구현 코드가 저장소에 있는 상태이지 빌드/게임 정상작동이 입증된 상태가 아닙니다.**

## 여전히 구현되지 않은 특수 효과

- 살점 핵: 사망 시 최대 체력 100%로 1회 부활
- 험악한 가면: 교대 시 주변 적 둔화 6초
- 파괴자의 검: 스킬 시전 시간에 따라 물리 공격력 및 치명타 확률 누적
- 세 번째 눈: 5초마다 전방 적 공격·기절
- 쇠사슬: 스킬 시전 시 쿨타임에 비례한 다중 사슬 공격
- 루비 심장: 출혈로 3명 처치할 때마다 HP 1 회복
- 왕가의 휘장: 출혈 부여 시 최대 8회 추가 물리피해
- 완벽한 마석: **원본 스크린샷에 효과 설명이 나오지 않아 사양 미확정**
- 보스 알렉산더 처치 후 해당 스컬 판정과 해금 이벤트 훅
- 사용자 제공 이미지의 실제 아이콘 크롭·저작권 허가·프리팹 적용
- 게임 로딩, 상점, 드롭 풀, 세이브, 다른 모드 충돌 테스트

전투 스킬의 애니메이션·상태이상·보스 처치 판정은 실제 게임 어셈블리에서 시그니처와 리소스가 확인되지 않은 상태에서 임의 구현하지 않습니다.

## 검증 단계

1. 별도 Skul 프로필과 백업된 세이브 파일 준비
2. `pwsh -File ./build.ps1 -SkulRoot "C:\Program Files (x86)\Steam\steamapps\common\Skul"` 또는 Windows PowerShell의 `./build.ps1`
3. 빌드 로그로 공개화 대상/네임스페이스/필드·이벤트 시그니처 오류 수정
4. 별도 프로필에서 BepInEx로 로드 테스트; `LogOutput.log` 확인
5. [Testing] `UnlockAllItemsForTesting=true`로 각 아이템 등장/획득/해제/효과 검증
6. 실제 보스 처치 해금 구현 뒤 다시 옵션 false로 해금 저장·재실행 테스트
7. 모든 절차 통과 전 GitHub Release 배포 금지

## 참고한 커뮤니티 구현

- https://github.com/MrBacanudo/SkulHardModeMods/tree/main/CustomItems
- https://github.com/damku1214/VoiceOfTheCommunity

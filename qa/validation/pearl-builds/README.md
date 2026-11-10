# 진주도둑들 — 수정 전후 빌드 비교

2026.11.10 추가 검증. 기존 11월 6일 로직 검사 합계와 별도 회차다.

## 비교 대상과 상태

1. **패배 보상 지급 누락**: 실제 Git 수정 전 `b71d2db8d19e682f23a2713523131b9948b09d36`과 수정 후 `2cc97a7dabe9874a452bd703b99918f117c77c0e`를 각각 다시 빌드한다. 당시 배포 바이너리는 아니다. 본인이 확인한 개발 당시 역할은 문제 발견·문서 보고·수정 후 재검증이며, 수정 코드 작성자로 표시하지 않는다.
2. **DEF-001 음수 비용 입력 방어**: `2cc97a7`의 원본 `GoldSystem`과 해당 파일에만 `gold-input-guard.patch`를 적용한 수정 후보를 비교한다. 원본 파일은 현재 로컬 프로젝트의 같은 파일과 SHA-256이 일치했다. 수정 후보의 검증이며 원본 프로젝트·출시판에 반영한 결과는 아니다.

소스 저장소: https://github.com/Antang2/GameEngine_Pearl-Thieves

## 동일하게 적용한 테스트 환경

- Unity 6000.3.10f1, Windows 11 x64, Mono Development Player, D3D11, 1920×1080.
- 원래 Stage1·SelectStage 씬, eWarrior 프리팹, 재화 컴포넌트와 UI 사용.
- `PortfolioValidation / PearlRewardRebuild1110-{variant}`로 저장 공간을 분리한다. 플레이어의 기존 저장 데이터는 초기화하거나 변경하지 않는다.
- 자동 웨이브를 비활성화하고, 보상 검사는 실제 적 3마리에게 피해를 주어 사망 이벤트를 발생시킨다. 보상 메서드에 45를 직접 넣지 않는다.
- 클리어·패배는 상대 성·플레이어 성에 피해를 주는 경로로 진입한다. 중도 이탈은 SelectStage 씬 전환 호출이다. 버튼 클릭·수동 전투 전체를 검증했다고 집계하지 않는다.
- `StoveManager`를 빌드 씬에서 제거하여 외부 SDK 초기화·점수 제출을 제외한다. 플랫폼 연동 검사가 아니다.
- 에디터 자동 씬 변경 `AutoSetupStages.RunSetup`은 `PE_QA_BUILD=1`에서 건너뛴다.
- 과거 소스의 `LastBossAttack.ResolveStatsAttackRange`가 `UNITY_EDITOR` 안에 있어 Windows 컴파일이 실패했다. **양쪽 모두** `build-compatibility.patch`로 같은 함수 본문을 조건부 구역 밖에 두었다. 보스 전투의 통과 증거로 사용하지 않는다.
- 숨김 창의 화면 버퍼가 검게 나와 `SceneCapture`로 실제 게임 카메라와 Canvas를 RenderTexture에 렌더링한다. 캡처하는 동안 Overlay Canvas를 같은 카메라에 연결한 뒤 원상복구한다. 이미지로 UI를 다시 그리거나 재화 텍스트를 덮어쓰지 않는다.
- `RewardProbe`, `GoldProbe`, `SceneCapture`, 빌드 보정은 비교 양쪽에 동일하게 사용한다. 제품 이름과 버전 식별만 달라진다. 에디터 생성 폰트 캐시·빌드 설정 변경은 테스트 인프라에 포함된다.

## 검사와 기대값

### 패배 보상

매 조건 미네랄 100·진주 10으로 시작한다. 일반 적 3마리의 사망 이벤트와 각각 15 보상을 확인해 누적 45를 만든다. 결제 전 지갑 100도 확인한다.

| 조건 | 기대 미네랄 | 반복 |
|---|---:|---:|
| 클리어 | 145 | 3 |
| 중도 이탈 | 100 | 3 |
| 획득 0 상태에서 패배 | 100 | 3 |
| 45 획득 후 패배 결과 중복 호출 | 145, 중복 지급 없음 | 3 |
| 45 획득 후 패배 | 145 | 3 |

결과 UI의 획득량, 지갑, PlayerPrefs 저장값, 선택 화면 복귀 잔액을 대조한다. 마지막 패배 뒤 종료하여 **별도 프로세스**로 재실행하고 145 유지 여부를 추가 1회 검사한다. 수정 전후 같은 기대값을 사용한다.

### DEF-001 비용 입력

실제 Stage1의 `GoldSystem` 시간 수익을 중지하고 `currentGold`를 조건별로 초기화한다. `TrySpendGold` 반환값, 실제 잔액, 원래 `GoldUI` 표시를 대조한다.

| 초기 골드 | 비용 | 기대 반환 / 잔액 |
|---:|---|---|
| 0 | -5 | false / 0 |
| 15 | 0 | true / 15 (기존 무료 비용 동작 유지) |
| 15 | 15 | true / 0 |
| 0 | 15 | false / 0 |
| 20 | 5 | true / 15 |
| 30 | 15를 연속 2회 | 두 번 true / 0 |
| 15 | NaN | false / 15 |
| 15 | -Infinity | false / 15 |
| 15 | +Infinity | false / 15 |

각각 3회. 비정상 비용은 검사 도구에서 의도적으로 공급한다. 일반 UI에서 이런 값을 전달할 수 있다는 의미는 아니다. 잠정 심각도 S3·우선순위 P3는 실제 UI 영향도 확인 전까지 유지한다.

## 재실행

원본이 아닌 짧은 경로의 별도 복제본을 사용한다. Unity 에디터를 닫은 상태에서 버전을 전환한다.

1. 위 저장소의 `prototype3-2-2`를 별도 경로에 체크아웃한다. `git config core.longpaths true`가 필요할 수 있다.
2. `RewardBuild.cs`는 `Assets/Editor/`, 나머지 세 C# 파일은 `Assets/`에 복사한다. 공통 `build-compatibility.patch`를 적용하고 `AutoSetupStages.RunSetup` 첫 줄에 `if (System.Environment.GetEnvironmentVariable("PE_QA_BUILD") == "1") return;`를 넣는다.
3. 아래와 같이 빌드한다. before는 `b71d2db...`, after는 `2cc97a7...`. candidate는 after에 골드 패치만 추가한다. 버전을 되돌릴 때 골드 패치를 원본에 남겨두지 않는다.

```powershell
$env:PE_QA_BUILD = '1'
& $unity -batchmode -nographics -quit -projectPath $isolatedProject `
  -executeMethod RewardBuild.Run -qaOutput $buildDirectory `
  -qaVariant before -qaCommit b71d2db8d19e682f23a2713523131b9948b09d36 `
  -logFile $buildLog
```

4. before와 after에서 suite → 종료 확인 → reload 순서로 실행한다. 실제 저장 재로딩이므로 동시에 실행하지 않는다. after와 candidate는 gold 모드도 실행한다.

```powershell
Start-Process -WindowStyle Hidden -FilePath $playerExe -ArgumentList @(
  '-force-d3d11', '-screen-width', '1920', '-screen-height', '1080',
  '-qaPhase', 'suite', '-qaOutput', ('"' + $resultsDirectory + '"'),
  '-logFile', ('"' + $playerLog + '"')
)
```

`-qaPhase reload`는 기대 저장 미네랄 145, `-qaPhase gold`는 골드 경계 검사다. 결과 JSON의 `error` 또는 `FIXTURE_BLOCKED`는 준비 조건 실패이며 게임 결함의 PASS/FAIL 집계에서 제외한다. 기대값 불일치는 각 행의 FAIL로 남긴다.

## 결과 자료

- [빌드 해시·실행 파일 목록·결과 JSON 위치](../../evidence/2026-11-10/pearl-builds/manifest.json)
- [수정 전 보상 결과](../../evidence/2026-11-10/pearl-builds/before-suite.json) / [수정 후 보상 결과](../../evidence/2026-11-10/pearl-builds/after-suite.json)
- [원본 비용 결과](../../evidence/2026-11-10/pearl-builds/after-gold.json) / [수정 후보 비용 결과](../../evidence/2026-11-10/pearl-builds/candidate-gold.json)
- [엑셀 실행 보고서](../../reports/pearl-thieves.xlsx)

공개 로그는 케이스별 관찰 행을 추출한 파일이다. 전체 게임 바이너리는 배포하지 않으며 로컬 빌드 SHA-256으로 대상을 식별한다. 출시판 전체 회귀, 실제 UI 악용 가능성, 기기 호환성·장시간 안정성, STOVE 연동은 이번 검사 범위에 포함되지 않는다.

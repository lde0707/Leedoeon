# 2026.11.10 추가 검증

## 데굴데굴 퍼즐

- 대상: 로컬에 보관된 itch.io 업로드 패키지 1.2.1. 현재 온라인 서비스와 동일한 버전이라고 가정하지 않는다.
- 환경: Windows 11 / Chrome 157 / Unity WebGL / 900×900 / SwiftShader 소프트웨어 렌더링.
- 조작: Playwright가 마우스와 키보드 이벤트를 입력한다. 클리어·보상 함수를 직접 호출하지 않는다.
- 판정: 게임이 IndexedDB에 기록한 진행·보유 별·최고 기록을 읽기 전용으로 대조한다. 캡처는 화면 상태를 확인하는 보조 증거다.
- 격리: 회차마다 새 브라우저 컨텍스트를 생성하며 외부 요청을 차단한다. 실제 사용자 저장, 랭킹 계정이나 서버 데이터는 변경하지 않는다.

Node.js, Playwright, Chrome을 준비한 뒤:

```powershell
node puzzle-playflow.cjs "<패키지의 web 폴더>" "<결과 폴더>"
```

5개 시나리오 × 3회. 실패 시 출력 JSON의 `failure`와 `checks`를 읽는다. 정상 종료만으로 통과를 판단하지 않는다. 패키지는 별도로 필요하며 포함된 manifest 해시로 대상 버전을 구분한다.

## FlyEditor

- 대상: Unity 6000.5.3f1, 2026.11.10 로컬 작업 소스의 격리 복사본.
- 커밋 이후 미커밋 변경이 포함되어 있다. `fly-source-manifest.json`의 파일별 SHA-256이 실제 검증 대상 식별자다.
- 기존 `ResearchEconomyVerification.Run()`과 `PaperFoldingVerification.Run()`을 각 10회 실행한다.
- 연구 검사는 98개 노드의 가격·구매, 중복 구매 차단, 잔액 경계, 저장 실패·재시도·디스크 재로딩, 비행 정산과 연결 검사를 포함한다.
- 접기 검사는 형상·종이층·되돌리기·복원 등을 검사한다. GUI를 조작한 전체 플레이 테스트와 구분한다.

해당 소스가 있는 **작업용 복사본**의 `Assets/Editor`에 `PortfolioRegression.cs`를 넣고 실행:

```powershell
Unity.exe -batchmode -nographics -quit -projectPath "<작업용 복사본>" -executeMethod PortfolioRegression.Run -portfolioOutput "<결과 폴더>" -logFile "<로그 경로>"
```

배포한 실행기와 실제 실행기의 검사 순서는 같다. 실제 실행기에서 사용한 로컬 출력 경로만 `-portfolioOutput` 인자로 일반화했다. 게임 프로젝트와 기존 검사 코드는 별도로 필요하다. 반복 횟수는 두 검사 묶음의 실행 횟수이며 개별 테스트 케이스 수와 같지 않다. `elapsed_ms`는 검사 실행 시간이며 게임 FPS나 성능 향상 수치가 아니다.

## 결과 해석

11월 6일 검사와 이번 추가 검사는 별도 회차다. 수동 확인·실기기 호환성·장시간 안정성·모든 스테이지 통과를 의미하지 않는다. 이번 검증에서 발견하지 않은 결함을 수정 완료했다고 기록하지 않는다. 메이플 자동화는 원본 코드 부재로 재실행하지 않았으며 기존 시연 자료 범위로만 소개한다.

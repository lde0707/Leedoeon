# 이도언 · 게임 QA 포트폴리오

**게임을 만든 경험으로, 플레이가 막히는 순간을 찾습니다.**

여러 팀 프로젝트와 1인 개발 프로젝트에서 기획·구현·테스트를 경험했습니다. 게임 규칙과 실제 동작을 대조하고, 진행 차단·재화 손실·저장 오류부터 확인합니다.

## 먼저 볼 자료

| 자료 | 확인할 내용 |
|---|---|
| **[QA 포트폴리오](https://leedoeon.vercel.app/qa/index.html)** | 참여 역할, 게임 화면, 프로젝트별 테스트 결과, 게임 분석서 |
| **[대표 QA 검증 사례](docs/QA-HIGHLIGHTS.md)** | 결함 증상, 수정 내용, 같은 조건에서의 재검증 화면 |
| **[프로젝트별 QA 엑셀](#프로젝트별-qa-엑셀)** | 조작 조건·기대 결과·실제 관찰값·판정·검증 근거 |
| **[게임 실행 파일](https://github.com/lde0707/portfolio-game-builds)** | Unity 없이 실행하는 Windows 게임 6개와 진주도둑들 출시 링크 |

## 대표 프로젝트

| 프로젝트 | 참여·역할 | QA 확인 내용 |
|---|---|---|
| **데굴데굴 퍼즐** | 1인 개발 · 기획·구현·테스트·itch.io 공개 | 입력 복원·완료 보상·재실행 후 저장 |
| **진주도둑들** | 4인 팀 · 시스템 기획·출시 전후 QA·결함 보고·재검증 | 전투 보상·강화 구매·재화 부족 조건·화면 배율 |
| **Rushitadel** | 3인 팀 · 포탑 성장·전투·재화 시스템 기획 | 전투·라운드 전환·이동과 반동의 화면 경계 |
| **FlyEditor** | 1인 개발 중 · 접기·비행·연구 구현과 테스트 | 접기·되돌리기·연구 구매·형상과 재화 저장 |

쓰레기 집게 소년, StillDrawn, 접힌 빛, GreenWay와 AI Shorts 자동화도 [포트폴리오의 참여 프로젝트](https://leedoeon.vercel.app/qa/index.html#projects)에서 볼 수 있습니다.

## 프로젝트별 QA 엑셀

파일 이름을 누르면 엑셀을 다운로드합니다. 각 게임의 **플레이 재검증** 시트에는 실제 입력으로 확인한 추가 결과를 정리했습니다. 기존 시트의 검증 날짜와 과거 실행 결과를 함께 보존했습니다.

| 프로젝트 | 주요 확인 범위 | Excel 다운로드 |
|---|---|---|
| 데굴데굴 퍼즐 | 입력 복원·완료 보상·저장 | [QA 보고서](https://leedoeon.vercel.app/qa/reports/degul-degul.xlsx) |
| 진주도둑들 | 전투 보상·강화 구매·UI 수정 전후 | [QA 보고서](https://leedoeon.vercel.app/qa/reports/pearl-thieves.xlsx) |
| Rushitadel | 전투 진행·이동 및 반동 경계 | [QA 보고서](https://leedoeon.vercel.app/qa/reports/rushitadel.xlsx) |
| 쓰레기 집게 소년 | 시작 안정성·수집·분리수거 판정 | [QA 보고서](https://leedoeon.vercel.app/qa/reports/trash-boy.xlsx) |
| StillDrawn | 프롤로그 진입·지도·체크포인트 복원 | [QA 보고서](https://leedoeon.vercel.app/qa/reports/stilldrawn.xlsx) |
| 접힌 빛 | 퍼즐 로직·첫 방 조작·다리·일시정지 | [QA 보고서](https://leedoeon.vercel.app/qa/reports/booktower.xlsx) |
| GreenWay | Figma 프로토타입의 동의·경로·설정 | [QA 보고서](https://leedoeon.vercel.app/qa/reports/greenway.xlsx) |
| FlyEditor | 접기·비행 정산·연구 구매·재실행 | [QA 보고서](https://leedoeon.vercel.app/qa/reports/flyeditor.xlsx) |

[통합 실행 요약 다운로드](https://leedoeon.vercel.app/qa/reports/qa-summary.xlsx)

## 게임 플레이

- **[데굴데굴 퍼즐 · 브라우저에서 플레이](https://lde0707.itch.io/degul-degul)**
- **[진주도둑들 · STOVE 출시 페이지](https://store.onstove.com/games/104688)**
- **[Windows 실행 파일 모음](https://github.com/lde0707/portfolio-game-builds)**

게임별 Release의 `게임이름-Windows-x64.zip`을 받아 **전체 압축 해제 후 EXE**를 실행합니다. `Data` 폴더와 DLL도 함께 유지해야 합니다. `Source code`는 실행 파일이 아닙니다.

## 도구와 활용

- **Unity·C#**: 게임 규칙과 상태 전환을 확인하고 실제 플레이 결과와 비교
- **Excel**: 테스트 조건·관찰값·결함·재검증 결과 정리
- **GitHub**: 변경 이력·소스 버전·실행 파일 관리
- **Python·OpenCV·OCR·Anaconda**: 버튼 탐지·클릭, 재화 비교, 예외 처리 자동화
- **Jira**: 메이플키우기 관련 버그·개선사항을 간단히 정리한 경험

## 자료 안내

게임 이미지, 분석서 이미지, QA 엑셀은 이 저장소에 보관해 함께 배포합니다. 게임 개발 소스는 개인·팀 비공개 저장소에서 관리합니다.

검증 범위는 각 보고서에 적었습니다. 전체 스테이지 완주나 모든 PC 환경에서의 동작을 의미하지 않습니다. 진주도둑들 수정 빌드는 팀 비공개 Release에 있으며 STOVE 판매 빌드는 별도입니다.

연락: **[doun070789@gmail.com](mailto:doun070789@gmail.com)** · [GitHub 프로필](https://github.com/lde0707)

<details>
<summary>저장소 구조와 로컬 실행</summary>

| 경로 | 내용 |
|---|---|
| `qa/index.html` | 게임 QA 포트폴리오 |
| `qa/reports/` | 프로젝트별 실행 보고서와 다운로드 자료 |
| `qa/evidence/` | 검증 화면·로그·결과 근거 |
| `docs/` | GitHub에서 읽는 대표 사례 |
| `assets/` | 게임·분석서의 이미지와 영상 |

저장소 루트에서 `python -m http.server 8000`을 실행한 뒤 `http://localhost:8000/qa/index.html`을 엽니다. `main`에 푸시하면 연결된 Vercel에 배포됩니다.

</details>

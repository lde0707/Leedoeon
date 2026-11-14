# 이도언 · 게임 QA 포트폴리오

**[포트폴리오 보기](https://leedoeon.vercel.app/qa/index.html)** · [연락처·링크](https://leedoeon.vercel.app/links.html)

참여 프로젝트의 게임 화면, 테스트 사례, 분석서와 QA 실행 보고서를 모았습니다. 이미지와 분석서 자료는 이 저장소에 함께 보관합니다.

## 게임 다운로드

**[전체 게임 다운로드 모음](https://github.com/lde0707/portfolio-game-builds)**

Windows 게임은 Unity 설치 없이 실행할 수 있습니다. GitHub Releases에서 **게임이름-Windows-x64.zip**을 받고 **전체 압축을 푼 뒤 EXE**를 실행하세요. `Source code`는 개발 소스이며 게임 실행 패키지가 아닙니다.
동봉된 `Data`, DLL, 나머지 폴더는 EXE와 함께 유지하세요. 모바일·macOS용 실행 파일은 아닙니다.

| 프로젝트 | 실행 | 저장소 |
|---|---|---|
| 데굴데굴 퍼즐 | [다운로드·플레이](https://github.com/lde0707/portfolio-game-builds/releases/tag/deguldegul-2026-11-14) | 개발 소스 비공개 |
| 진주도둑들 | [다운로드·플레이](https://store.onstove.com/games/104688) | 팀 비공개 저장소 |
| 접힌 빛 | [다운로드·플레이](https://github.com/lde0707/portfolio-game-builds/releases/tag/afoldoflight-2026-11-14) | 개발 소스 비공개 |
| 쓰레기 집게 소년 | [다운로드·플레이](https://github.com/lde0707/portfolio-game-builds/releases/tag/trashboy-2026-11-14-r2) | 개발 소스 비공개 |
| FlyEditor | [다운로드·플레이](https://github.com/lde0707/portfolio-game-builds/releases/tag/flyeditor-2026-11-14) | 개발 소스 비공개 |
| Rushitadel | [다운로드·플레이](https://github.com/lde0707/portfolio-game-builds/releases/tag/rushitadel-2026-11-14-r2) | 개발 소스 비공개 |
| StillDrawn | [다운로드·플레이](https://github.com/lde0707/portfolio-game-builds/releases/tag/stilldrawn-2026-11-14) | 개발 소스 비공개 |

데굴데굴 퍼즐은 [itch.io 웹 플레이](https://lde0707.itch.io/degul-degul)도 제공합니다. GreenWay는 포트폴리오의 프로토타입 화면·영상으로 확인할 수 있습니다.

## 배포 파일 확인

2026-11-14에 게임 7개를 실제 키보드·마우스 입력으로 플레이해 시작부터 주요 기능까지 확인했습니다. 쓰레기 집게 소년의 그래픽 충돌, Rushitadel의 화면 상단 이탈, 진주도둑들의 메뉴·배경 배율 문제를 수정하고 다시 빌드했습니다.

[실제 화면·게임별 확인 범위·수정 전후 기록](https://leedoeon.vercel.app/qa/reports/windows-playtest-2026-11-14.html)

공개 Windows 패키지는 6개입니다. 진주도둑들 수정 파일은 팀 비공개 저장소 Release에 있으며 STOVE 판매 빌드는 별도입니다. 전체 완주·모든 PC 환경·이전 저장 파일의 모든 호환성은 검증 범위에 포함되지 않습니다. `SHA256SUMS.txt`로 다운로드 파일의 무결성을 대조할 수 있습니다.

## 포트폴리오 파일

- `qa/index.html`: 게임 QA 포트폴리오
- `qa/reports/`: 프로젝트별 다운로드 보고서
- `assets/`: 게임 화면·영상·보관 이미지
- `links.html`: 공개 링크 모음
- `outputs/`: 로컬 점검 산출물 (새 파일은 Git 제외)

로컬 확인은 저장소 루트에서 `python -m http.server 8000` 실행 후 `http://localhost:8000/qa/index.html`을 여세요. GitHub에 푸시하면 연결된 Vercel 배포에 반영됩니다.

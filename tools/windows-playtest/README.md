# Windows actual-input playtest

The probe is compiled only with PORTFOLIO_QA, activated with -playerQa, and records input, captures and observations. It does not invoke gameplay methods or modify gameplay state. PortfolioQaBuild uses an isolated company name for test saves and restores original project identity after building. Release builds must omit PORTFOLIO_QA.

Set PORTFOLIO_QA_EXE to a Windows output executable and run the matching Unity editor with -batchmode -nographics -executeMethod PortfolioQaBuild.Build. The source probe and builder belong under Assets/PortfolioQA and Assets/PortfolioQA/Editor respectively.

Use qa_client.py <repository> start, offscreen, buttons, button <name>, tap <key>, hold <seconds> <keys>, capture <name>, read <type> <member>, quit. Native Windows clicks account for per-window DPI and never use global mouse/keyboard events. Coordinate observations depend on the actual screen size. Wait for scene transitions before sending input.

Functional checks in PLAYTEST.md are limited to the paths actually exercised; internal fixtures and entire-game completion are separate.

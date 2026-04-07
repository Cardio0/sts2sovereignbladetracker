# Sovereign Blade Tracker

A mod that displays the current damage value of all Sovereign Blade cards on screen during combat. Only visible when playing as the Regent character.

## Features

- Tracks **all** Sovereign Blade cards simultaneously (multiple cards supported)
- Displays values in **descending order**: `55/19/10`
- Cards in the **exhaust zone** are shown in **gray**: `55/[gray]28[/gray]`
- **Transformed** cards are automatically removed from the display
- Real-time updates every frame — reflects changes from any game effect instantly

## Installation

1. Download the latest release from the [releases page](https://github.com/Cardio0/sts2sovereignbladetracker/releases/latest)
2. Extract the zip file
3. Copy `SovereignBladeTracker.dll`, `SovereignBladeTracker.pck`, and `sovereign_blade_tracker_manifest.json` to your STS2 mods folder (usually `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\SovereignBladeTracker\`). If the mods folder doesn't exist, create it.
4. Launch Slay the Spire 2
5. When you first install the mod and enter the game, a mod activation message will appear; activate it to enable the mod.

## Configuration

You can configure the mod in-game via the mod settings screen, or manually edit the JSON file:

**Location:** `%APPDATA%\SlaytheSpire2\SovereignBladeTracker.config.json`

The config file will be created automatically on first run with default values.

### Available Settings

```json
{
  "panelX": 177,
  "panelY": 845,
  "draggable": true,
  "rememberPosition": true
}
```

### Settings Explanation

- **panelX / panelY**: Position of the tracker panel on screen
- **draggable**: Whether the panel can be dragged with the mouse during combat
- **rememberPosition**: Save the panel position when exiting the game

### In-Combat Controls

- **Left-click + drag**: Move the panel
- **Right-click**: Reset panel to default position

### Cautions

- The tracker panel is only visible during combat when playing as the Regent character.
- If you delete the config file, the mod will create a new one with default values on next run.

## How to Build

You can build this mod using dotnet or Godot 4.5.1.

### Make DLL

1. Open the terminal and go to the project directory
2. Run `dotnet build sovereign_blade_tracker.csproj`
3. The DLL will be copied automatically to the STS2 mods folder

### Make PCK

1. Open the project in Godot 4.5.1
2. Export as PCK (`Project > Export`)
3. Copy the generated `.pck` file to the mods folder

<br>
<br>
<br>

# Sovereign Blade Tracker (한국어)

전투 중 Sovereign Blade 카드의 현재 데미지 수치를 화면에 표시해주는 모드입니다. Regent 캐릭터로 플레이할 때만 표시됩니다.

## 기능

- **모든** 군주의 칼날 카드를 동시에 추적 (복수 카드 지원)
- 수치를 **내림차순**으로 표시: `55/19/10`
- **소멸존**에 있는 카드는 **회색**으로 표시: `55/[회색]28[/회색]`
- **변화**된 카드는 자동으로 표시에서 제거
- 매 프레임 실시간 갱신 — 변화, 소멸 등 모든 게임 이펙트를 즉시 반영

## 설치 방법

1. [여기](https://github.com/Cardio0/sts2sovereignbladetracker/releases/latest)에서 최신 릴리스를 다운로드합니다.
2. zip 파일을 압축 해제합니다.
3. `SovereignBladeTracker.dll`, `SovereignBladeTracker.pck`, `sovereign_blade_tracker_manifest.json` 파일을 STS2 mods 폴더에 복사합니다. (보통 `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\SovereignBladeTracker\`입니다. 폴더가 없으면 생성하세요.)
4. Slay the Spire 2를 실행합니다.
5. 모드를 처음 설치하고 게임에 들어가면 모드 활성화 메시지가 나타납니다. 활성화하면 모드가 작동합니다.

## 설정 방법

설정은 인게임 모드 설정 화면에서 변경하거나, JSON 파일을 직접 편집할 수 있습니다:

**위치:** `%APPDATA%\SlaytheSpire2\SovereignBladeTracker.config.json`

설정 파일은 첫 실행 시 자동으로 생성됩니다.

### 사용 가능한 설정

```json
{
  "panelX": 177,
  "panelY": 845,
  "draggable": true,
  "rememberPosition": true
}
```

### 설정 설명

- **panelX / panelY**: 트래커 패널의 화면 위치
- **draggable**: 전투 중 마우스로 패널을 드래그할 수 있는지 여부
- **rememberPosition**: 게임 종료 시 패널 위치를 저장할지 여부

### 전투 중 조작법

- **좌클릭 + 드래그**: 패널 이동
- **우클릭**: 패널을 기본 위치로 초기화

### 주의사항

- 트래커 패널은 Regent 캐릭터로 전투 중일 때만 표시됩니다.
- 설정 파일을 삭제하면 다음 실행 시 기본값으로 새 파일이 생성됩니다.

## 빌드 방법

dotnet 또는 Godot 4.5.1로 빌드할 수 있습니다.

### DLL 빌드

1. 터미널에서 프로젝트 디렉토리로 이동합니다.
2. `dotnet build sovereign_blade_tracker.csproj` 를 실행합니다.
3. DLL이 자동으로 STS2 mods 폴더에 복사됩니다.

### PCK 빌드

1. Godot 4.5.1에서 프로젝트를 엽니다.
2. PCK로 익스포트합니다 (`Project > Export`)
3. 생성된 `.pck` 파일을 mods 폴더에 복사합니다.

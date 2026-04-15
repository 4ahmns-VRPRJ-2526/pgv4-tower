# 3DURP-Unity6057f1-Template

Base template for new Unity projects using the Universal Render Pipeline.

## Tech Stack

- **Unity:** 6000.0.57f1
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Project Type:** 3D
- **Target Platform:** Windows Desktop (default)

## Purpose

This repository provides a clean starting point for Unity projects with:

- a clear folder structure
- URP setup
- version control readiness
- scalable project organization

## Project Structure

```text
Assets/
  MyGame/
    Scenes/
    Scripts/

  Settings/
```

## Conventions

- All project-specific content goes into Assets/MyGame/
- Avoid placing custom files directly in Assets/
- Use clear, consistent names
- Keep scenes, scripts, and prefabs organized by purpose

## Getting Started

1. Clone the repository
2. Open the project in Unity 6000.0.57f1
3. Open the main scene:
   Assets/MyGame/Scenes/MainScene.unity
4. Press Play

## Build

1. Open File > Build Profiles
2. Add the required scenes
3. Select target platform
4. Build or Build and Run

## Version Control

This project is intended to be used with Git.

Do not commit:
- Library/
- Temp/
- Logs/
- Obj/
- Build/
- UserSettings/

## Coding Conventions

- PascalCase for classes and methods
- camelCase for variables
- One class per file
- File name matches class name
- Avoid magic numbers

## Scene Conventions

- Use one clear entry scene
- Keep hierarchy clean and structured
- Remove unused objects and assets

## Dependencies

Managed via Unity Package Manager.

Typical packages:
- Universal RP
- Input System
- TextMeshPro

## Collaboration

Recommended workflow:
1. Create feature branch
2. Implement changes
3. Review and merge into main

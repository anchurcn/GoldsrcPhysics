# GoldsrcPhysics
Welcome to the GoldsrcPhysics source code repository!
GoldsrcPhysics is a physics engine written in csharp for goldsrc which implemented ragdoll. 
The engine provides a set of APIs for mod developers to make ragdoll effect in their games.

> **NOTE:** The project is still under development, but spare time is short.

# How to use
For developers who want to use this library on their game.

## Basis
* You have gold source game mod dev experience.
* Since it based on .NET Framework, you must install .NET 4.6 first.
  - type .NET Framework in search engine and download on official site.
  - or run on first time, it will ask you if you would like to install .NET (may not work).
* Import GoldsrcPhysics SDK.
* Write some codes on goldsrc client side to call the API. It looks like (pseudo code):
```c
#include<physics.h>

//Represent GoldsrcPhysics API
physicsAPI_t g_physics;

void GameInit(){
  g_physics.Init();
}

void OnMapLoad(){
  g_physics.LoadScene(mapName);
}

void OnPlayerDie(int entityId){
  g_physics.StartRagdoll(entityId);
}

void OnPlayerRespawn(int entityId){
  g_physics.StopRagdoll(entityId);
}

...
```
* Add some data to describe the bones of each part of your player model.
* Have fun!

[Learn more](https://blog.csdn.net/u012779385/article/details/108901621)

# Building from source
For developers who want to contribute to this project.
## Prerequisite
1. Clone this repository to your computer.
1. VS2019 + .NET4.6.1 SDK 

## Build binary release
If you just want to build your own binary release, follow this guide.
1. Open \src\GoldsrcPhysics.sln with your VS2019.
1. Change build option to Release mode.
1. Right click your project on `Solution Resources Manager` and click build.

You will see the `\bin` folder on your repo root.

## Debugging
1. Goldsrc game which uses this library (GoldsrcPhysics) is needed.
1. Open project properties window.
 - edit the post build event on build event pannel for copying GoldsrcPhysics.dll to `"path\to\your\game\gsphysics\bin"`
 - select debug pannel, edit the path **Launch external program** to your game executable file. Usually named hl.exe or cstrick.exe.
 - maybe need some program arguments to specify your mod.
 1. Press F5, you'll see the game launched by VS2019.
 1. If you set a break point, VS2019 will tell you the break point is available.

# Contribution
If you found issues, don't hesitate to submit a new issue here. Also, pull request is welcomed!
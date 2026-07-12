# Project Overview
- **Game Title**: Flood Preparedness Low-Poly Game Prototype
- **High-Level Concept**: A first-stage prototype for a 3D flood preparedness educational game. The player chooses a role (Child, Teenager, or Adult) from a UI selection screen on startup, then spawns as a capsule representative of that role inside a greyboxed low-poly house while heavy rain falls outside.
- **Players**: Single player (3rd-person/isometric controller).
- **Inspiration / Reference Games**: Survival prototypes, flood rescue simulations, educational hazard games.
- **Tone / Art Direction**: Low-poly, clean greybox styling using basic shapes and color-coded roles.
- **Target Platform**: Standalone PC.
- **Screen Orientation / Resolution**: Landscape (1920x1080).
- **Render Pipeline**: Universal Render Pipeline (URP).

---

# Game Mechanics
## Core Gameplay Loop
1. **Startup / Selection**: The player starts with a clear Role Selection menu overlay.
2. **Role Choice**: Choosing a role changes the visual height, scale, and color of the player's 3D character capsule.
3. **Spawning**: The player is spawned at a designated Spawn Point inside a 3D greyboxed house.
4. **Heavy Rain Atmospheric Setup**: Heavy rain particles fall from above the house, and ambient grey clouds/fog are set up in URP.
5. **Exploration**: The player can walk around the house using WASD / Left Stick controls (driven by the active New Input System).

## Controls and Input Methods
- **Movement**: WASD keys / Keyboard Arrows or Left Stick on Gamepad (utilizing the pre-configured project-wide `Player/Move` Input Action).
- **UI Navigation**: Mouse click / Touch or Gamepad/Keyboard submit.

---

# UI
## Role Selection Screen
A full-screen Canvas overlay containing:
- Title text: "Select Your Role"
- Three buttons: 
  - **Child** (Visual: Small Orange Capsule)
  - **Teenager** (Visual: Medium Green Capsule)
  - **Adult** (Visual: Large Blue Capsule)
- Helper instruction: "Different roles represent different perspectives in flood preparedness."

## In-Game HUD Panel
A simple overlay showing:
- Active Role text: e.g., "Role: Teenager"
- Objective text: "It is raining heavily. Look around the house and prepare for flooding!"

---

# Key Asset & Context
### Materials
- `Assets/Materials/Mat_Adult.mat`: Blue URP Lit Material.
- `Assets/Materials/Mat_Teenager.mat`: Green URP Lit Material.
- `Assets/Materials/Mat_Child.mat`: Orange URP Lit Material.
- `Assets/Materials/Mat_HouseFloor.mat`: Dark grey/brown URP Lit Material.
- `Assets/Materials/Mat_HouseWalls.mat`: Light grey/beige URP Lit Material.

### Scripts
- `Assets/Scripts/GameManager.cs`: Main coordinator. Manages scene flow, spawns the player capsule, and configures the role appearance (color, scale) upon selection.
- `Assets/Scripts/PlayerController.cs`: Simple 3D movement script utilizing `CharacterController` and reading values from the pre-existing project-wide Input System asset `InputSystem_Actions.inputactions` (`Player/Move` map).
- `Assets/Scripts/CameraFollow.cs`: Smoothly follows the player's position to enable third-person/isometric perspective exploration of the house.

### Prefabs
- `Assets/Prefabs/PlayerPrefab.prefab`: A player GameObject root containing a CharacterController, the `PlayerController` script, a Child Visual capsule, and a Material reference.

---

# Implementation Steps

### Step 1: Create Materials & Folders
- **Description**: Create folder structures (`Assets/Scripts`, `Assets/Prefabs`, `Assets/Materials`) and the five key URP materials with appropriate colors.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Build the Greybox House Environment
- **Description**: In the `SampleScene`, construct a simple 3D low-poly house structure using Unity primitive Cubes for a floor plane, inner/outer walls, doors/windows openings, and a slanted roof. Assign the Floor and Wall materials. Create an empty GameObject `SpawnPoint` inside the house.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Implement Player Controller & Camera Follow
- **Description**: 
  - Create `PlayerController.cs` reading from `InputSystem.actions.FindAction("Move")` to drive movement via `CharacterController`. Include gravity and simple rotation towards the move direction.
  - Create `CameraFollow.cs` to smoothly position the camera behind the player with a set offset.
  - Create `PlayerPrefab` using a capsule, character controller, and the player controller script.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 4: Implement Game Manager and Role Selection UI
- **Description**:
  - Create `GameManager.cs` to coordinate game state: `Selection` -> `Gameplay`.
  - Design UI Canvas with `RoleSelectionPanel` (3 buttons) and `HUDPanel` (objective text).
  - Bind UI button click events in `GameManager` to instantiate the `PlayerPrefab` at `SpawnPoint`, set its visual height, scale, and material based on selection, activate the camera follower, and enable the gameplay HUD.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

### Step 5: Set up Rain Particle System & Weather Atmosphere
- **Description**: Add a standard Unity Particle System above the house. Configure it to emit downward-falling rain particles (stretched billboards) outside the house walls. Adjust ambient light/fog settings in URP global settings to evoke a stormy/rainy atmosphere.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: Yes

---

# Verification & Testing
- **Visual Validation**: Play in Editor, verify that the Role Selection Screen appears immediately and gameplay controls are disabled during selection.
- **Role Selection Test**: Click each button in turn (Adult, Teenager, Child) and verify:
  - Correct visual color and height scale is applied.
  - The correct character is instantiated at the Spawn Point.
  - Selection panel hides, HUD panel appears with active role name.
- **Movement & Collision Test**: Verify WASD/Arrow Keys move the player, the player is subject to gravity, the camera smoothly follows, and the player cannot walk through the house walls.
- **Rain Atmospheric Check**: Verify the rain particles are visible outside the house window/doors, but do not clip inappropriately through the roof if colliders are set up or bounds are correctly restricted.

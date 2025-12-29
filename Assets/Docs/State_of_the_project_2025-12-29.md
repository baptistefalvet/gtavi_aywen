# GTAVI Aywen - State of the Project
**Date:** December 29, 2025

---

## Project Overview

GTAVI Aywen is a GTA-style open-world game prototype built in Unity using the Universal Render Pipeline (URP). The project implements core open-world mechanics including player movement, combat, vehicles, AI NPCs, police system, and comprehensive UI.

---

## Implemented Features

### Player Systems

| Feature | Script | Description |
|---------|--------|-------------|
| Movement Controller | `PlayerController.cs` | Walk, sprint, jump with slope detection and state machine (Idle/Walk/Sprint/Air) |
| Third-Person Camera | `ThirdPersonCam.cs` | Three modes (Near, Far, FPS), camera shake, dynamic FOV for aiming |
| Aiming System | `PlayerAim.cs` | Raycast-based aiming with weapon-dependent behavior |
| Inverse Kinematics | `PlayerInversKinematics.cs` | Foot IK for terrain adaptation, hand IK for weapons |
| Aim IK | `PlayerAimIK.cs` | Hand targeting for weapons with smooth equip/unequip |
| Ragdoll System | `PlayerRagdoll.cs` | Toggle ragdoll physics (E key) with get-up animations |
| Vehicle Control | `PlayerCarControll.cs` | Enter/exit vehicles with door positioning |
| Speed Lines Effect | `PlayerSpeedLines.cs` | Dynamic speed lines based on velocity |

---

### Weapon & Combat System

| Feature | Script | Description |
|---------|--------|-------------|
| Weapon Controller | `WeaponController.cs` | Base weapon class with hand positioning |
| Gun Controller | `GunController.cs` | Shooting, recoil, spread, magazine system, muzzle flash |
| Bullet System | `BulletController.cs` | Projectile physics, collision detection, hit effects |
| Gun Data | `GunObject.cs` | ScriptableObject for configurable gun stats (damage, fire rate, magazine size, etc.) |
| Player Weapons | `PlayerWeaponController.cs` | Multi-weapon equipment, weapon switching, ammo UI, NPC panic trigger |

---

### Vehicle System

| Feature | Script | Description |
|---------|--------|-------------|
| Car Controller | `CarController.cs` | Spring-damper suspension, steering with turn curves, tire smoke, drift trails, speed lines |

---

### NPC System

| Feature | Script | Description |
|---------|--------|-------------|
| NPC Controller | `NpcController.cs` | AI patrolling via NavMesh, panic system, procedural randomization (clothing colors, body size), health system |
| NPC Ragdoll | `NpcRagdoll.cs` | Ragdoll physics on death, auto-get-up system with stand animations |

---

### World Systems

| Feature | Script | Description |
|---------|--------|-------------|
| Day/Night Cycle | `TimeCycleController.cs` | Sun/moon rotation, dynamic light color/intensity, rain system with probability |
| Police System | `PoliceSystem.cs` | Wanted levels (0-5 stars), police spawning around player, wanted decay when hidden |
| Crime Manager | `CrimeSeverityManager.cs` | Crime severity levels: Theft (1★), Assault (1★), Gun Firing (2★), Vehicle Theft (2★), Police Evasion (2★), Explosion (3★), Murder (4★), Massacre (5★) |

---

### UI Systems

| Feature | Script | Description |
|---------|--------|-------------|
| Minimap | `Minimap.cs` | Real-time minimap with zoom controls, rotation modes (follow/fixed north) |
| Full Map | `GrandeMap.cs` | Toggle with M key, WASD pan, mouse wheel zoom, waypoint placement |
| Compass | `Boussole.cs` | Cardinal direction display (N, NE, E, SE, S, SO, O, NO) |
| Waypoints | `MinimapWaypoints.cs` | Waypoint placement and clearing |
| Minimap Creator | `MinimapCreator.cs` | Automatic minimap setup utilities |
| Minimap Builder | `MinimapBuilder.cs` | Minimap building utilities |
| Minimap Marker | `MinimapMarker.cs` | Marker system for minimap |

---

### Rendering & Visual Effects

| Feature | Description |
|---------|-------------|
| Speed Lines Post-Processing | Motion blur effect at high speeds with chromatic aberration |
| Volumetric Clouds | Cloud rendering system with volume-based control |
| Screen Space Global Illumination | SSGI approximation with runtime adjustments |
| Posterization Effect | Fog-based color reduction effect |
| Water System | Bitgem stylized water with floating objects support |
| Procedural Terrain Painter | Height, slope, curvature, direction, and noise-based texture painting |

---

## Assets & Prefabs

### Vehicles
- Car 1, Car 2, Car 3, Car 4 (4 variants)

### NPCs
- Generic NPC prefab with procedural randomization

### Environment
- Trees
- Bushes (3 variants)
- Rocks (small, medium x3, large)
- Buildings

### Visual Effects
- Hit flash particles
- Muzzle flash effects
- Burst particle effects
- Speed lines rendering

### Items
- Bullet prefab

---

## Technical Systems

### Physics
- Rigidbody-based player movement
- Spring-damper vehicle suspension
- Ragdoll physics for players and NPCs
- Collision detection and response

### Animation
- Animator-driven character animations
- State machine integration
- Inverse Kinematics (IK) for hands and feet
- Ragdoll-to-standing transitions

### Input
- Keyboard: WASD (movement), Space (jump), E (ragdoll), M (map), V (camera mode)
- Mouse: Aim, look, camera control, scroll (weapon switch/zoom)

### Navigation
- NavMesh agent for NPC pathfinding
- Random walk point generation
- Panic fleeing pathfinding

### Post-Processing
- URP rendering pipeline
- Multiple post-processing effects
- Volume-based effects system
- Cinemachine camera shake

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Player/          # Player controllers and systems
│   ├── Weapon/          # Weapon and bullet systems
│   ├── Car/             # Vehicle controller
│   ├── Entity/          # NPC controllers
│   ├── World/           # Time cycle, police, crime systems
│   └── UI/              # Minimap, compass, map systems
├── Prefabs/             # Game object prefabs
├── Scenes/              # TestScene.unity (main scene)
└── ...
```

---

## Status Summary

The project is a **functional prototype** with all core GTA-style mechanics implemented:

- **Player**: Full movement, camera, aiming, weapons, vehicles, ragdoll
- **Combat**: Multi-weapon system with realistic gun mechanics
- **Vehicles**: Drivable cars with physics-based suspension
- **NPCs**: AI-driven pedestrians with panic behavior
- **World**: Day/night cycle, weather, police wanted system
- **UI**: Minimap, full map, compass, waypoints

---

## Next Steps (Suggested)

- [ ] Add more weapon types
- [ ] Implement mission system
- [ ] Add more vehicle types (motorcycles, boats, helicopters)
- [ ] Expand NPC behaviors (conversations, schedules)
- [ ] Add save/load system
- [ ] Implement radio/audio system for vehicles
- [ ] Add inventory system
- [ ] Create more detailed world environments

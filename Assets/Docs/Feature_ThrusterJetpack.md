# Thruster-Style Jetpack Feature Document

**Version:** 1.0
**Date:** December 30, 2025
**Status:** Design Phase

---

## Overview

A controllable, forgiving VTOL jetpack inspired by GTA Online's Mammoth Thruster. The jetpack uses Rigidbody-based physics with the Unity Input System and provides two distinct flight modes: Normal Flight and Upright/Strafe Mode.

---

## Table of Contents

1. [Core Concepts](#core-concepts)
2. [Input Mapping](#input-mapping)
3. [Flight Modes](#flight-modes)
4. [Boost System](#boost-system)
5. [Ground Cushion Safety](#ground-cushion-safety)
6. [State Machine](#state-machine)
7. [Physics Implementation](#physics-implementation)
8. [Tuning Parameters](#tuning-parameters)
9. [Debug & Verification](#debug--verification)
10. [Acceptance Tests](#acceptance-tests)
11. [Implementation Plan](#implementation-plan)
12. [File Structure](#file-structure)

---

## Core Concepts

### Design Philosophy
- **Forgiving Controls**: The jetpack should be easy to stabilize and hard to crash
- **Vectored Thrust Feel**: Forward motion comes from pitching forward (thrust vectoring)
- **Two Distinct Modes**: Normal flight for speed/maneuverability, Strafe mode for precision
- **Safety Systems**: Ground cushion prevents harsh landings when gear is retracted

### Physics Approach
- All movement via `Rigidbody.AddForce()` and `Rigidbody.AddTorque()`
- Prefer `ForceMode.Acceleration` for consistent tuning across mass changes
- PD controller for attitude stabilization (pitch/roll/yaw)
- Target velocity controller for strafe mode horizontal movement

---

## Input Mapping

### Input Actions Asset: `JetpackInputActions.inputactions`

| Action | Keyboard Primary | Keyboard Alt | Description |
|--------|------------------|--------------|-------------|
| Move | W/A/S/D | Arrow Keys | Strafe plane movement |
| Ascend | Space | - | Increase collective thrust |
| Descend | Left Ctrl | C | Decrease thrust |
| Yaw | Q/E | - | Rotate heading left/right |
| Pitch | Mouse Y | Up/Down Arrow | Pitch control |
| Roll | - | Z/C (configurable) | Roll control (optional) |
| StrafeMode | Left Shift (hold) | - | Activate upright/precision mode |
| Boost | Left Alt | - | Activate JATO-style burst |
| ToggleGear | G | - | Deploy/retract landing gear |
| Exit | X | - | Trigger safe landing/exit |

### Mouse Configuration
- **Mouse X**: Camera orbit (via existing ThirdPersonCam)
- **Mouse Y**: Pitch control (configurable sensitivity)
- Mouse influence on facing:
  - **Normal Mode**: Weak/optional camera influence
  - **Strafe Mode**: Strong camera-driven facing direction

---

## Flight Modes

### A) Normal Flight Mode

**Behavior:**
- Hover stable at zero input (no drift/oscillation)
- Strong angular damping prevents wobble
- Auto-level returns craft toward upright when releasing pitch/roll inputs

**Vertical Control:**
- `Space`: Increases collective thrust → climb
- `Ctrl/C`: Decreases thrust → descend
- Idle: Maintains approximate hover (gravity compensation)

**Horizontal Movement:**
- Forward motion via **pitch vectoring**: pitching forward redirects thrust
- Some thrust component becomes forward acceleration proportional to pitch angle
- W/S in normal mode affects pitch target
- A/D affects roll target (with clamped max angles)

**Constraints:**
- Max pitch angle clamped (e.g., ±30°) - not a stunt plane
- Max roll angle clamped (e.g., ±25°)
- Yaw control (Q/E) rotates heading without excessive roll coupling

**Auto-Stabilization:**
- PD controller drives toward target attitude
- Target pitch/roll = 0 when no input (auto-level)
- Configurable stabilization strength and damping

### B) Upright/Strafe Mode (Hold Left Shift)

**Activation:** Hold Left Shift modifier

**Behavior:**
- Craft becomes upright (strong roll/pitch stabilization toward 0°)
- Movement becomes camera-relative translation
- Quick acceleration/deceleration for dodge maneuvers

**Movement Mapping:**
| Input | Result |
|-------|--------|
| W | Move forward (camera direction) |
| S | Move backward (camera direction) |
| A | Strafe left (camera right) |
| D | Strafe right (camera right) |

**Characteristics:**
- **Reduced top speed** compared to normal flight
- **High acceleration/braking** for snappy starts/stops
- Facing direction follows camera yaw (with configurable smoothing)
- Strong damping when input released (quick stop)

**Implementation:**
```
targetVelocityXZ = (cameraForward * inputY + cameraRight * inputX) * strafeMaxSpeed
accelerate toward targetVelocityXZ with strafeAccel
apply strong braking when input released
```

---

## Boost System

### JATO-Style Burst

**Activation:** Press Left Alt

**Conditions for Activation:**
- Option A: Only when grounded OR altitude < `lowAltitudeThreshold`
- Option B: Only when landing gear is deployed
- **Selected**: Option A (altitude-based, more intuitive)

**Behavior:**
- Duration: ~3 seconds (configurable)
- Adds strong forward acceleration in facing direction
- Controls remain usable but slightly damped during boost
- Visual/audio feedback (exhaust effects, sound)

**Recharge:**
- Boost recharges **only upon landing**
- Landing detection: grounded for `minGroundedTime` with `verticalSpeed < maxGroundedSpeed`
- Cannot boost again until landing criteria met

**UI Feedback:**
- Boost meter showing remaining duration / availability
- Visual indicator when boost is available vs depleted

---

## Ground Cushion Safety

### Hover Cushion System (Gear Retracted)

**Purpose:** Prevent harsh slams when dropping from small heights

**Activation:** Automatic when landing gear is **retracted**

**Behavior:**
```
if (gearRetracted && raycastDistance < hoverTargetHeight):
    correctionForce = hoverSpring * (hoverTargetHeight - raycastDistance)
    dampingForce = hoverDamp * verticalVelocity
    apply upward force (correctionForce - dampingForce)
```

**Raycast Setup:**
- Cast downward from craft center
- `hoverRayLength`: Maximum detection distance
- Multiple rays optional for stability on uneven terrain

**Gear States:**
| Gear State | Ground Cushion | Landing Allowed |
|------------|----------------|-----------------|
| Deployed | Disabled | Yes (normal landing) |
| Retracted | Active | No (maintains hover) |

### Exit/Safe Landing Logic

**Trigger:** Press X (Exit action)

**Behavior:**
1. If near ground (`altitude < safeExitHeight`):
   - Auto-deploy gear
   - Reduce thrust gradually
   - Allow controlled touchdown
2. If high altitude:
   - Maintain controlled hover
   - Begin gradual descent
   - Auto-deploy gear when approaching ground

---

## State Machine

### States

```
┌─────────────┐
│  Grounded   │ ←─────────────────────────┐
└──────┬──────┘                           │
       │ (Ascend input)                   │
       ▼                                  │
┌─────────────┐                           │
│   Takeoff   │                           │
└──────┬──────┘                           │
       │ (altitude > takeoffThreshold)    │
       ▼                                  │
┌─────────────┐    (Shift held)    ┌──────────────┐
│   Flight    │ ◄────────────────► │  StrafeMode  │
└──────┬──────┘    (Shift released)└──────────────┘
       │                                  │
       │ (Both: Alt pressed + boost available)
       ▼                                  ▼
┌─────────────────────────────────────────────────┐
│                   Boosting                       │
└─────────────────────────────────────────────────┘
       │
       │ (boost depleted or released)
       ▼
┌─────────────┐
│   Flight    │ (or StrafeMode if Shift held)
└──────┬──────┘
       │ (descend + near ground + gear deployed)
       ▼
┌─────────────┐
│   Landing   │
└──────┬──────┘
       │ (velocity < threshold + grounded)
       ▼
┌─────────────┐
│  Grounded   │
└─────────────┘
```

### State Behaviors

| State | Thrust | Stabilization | Ground Cushion | Boost |
|-------|--------|---------------|----------------|-------|
| Grounded | Off | Full lock upright | Off | Recharges |
| Takeoff | Increasing | Strong | Off | Locked |
| Flight | Active | Normal (configurable) | If gear retracted | Available |
| StrafeMode | Active | Very strong (upright) | If gear retracted | Available |
| Boosting | Maximum + Boost | Slightly reduced | If gear retracted | Depleting |
| Landing | Reducing | Strong | Off | Locked |

---

## Physics Implementation

### Force Application Strategy

All forces use `ForceMode.Acceleration` for mass-independent tuning.

### Thrust System

```csharp
// Collective thrust (vertical)
float collectiveInput = ascendInput - descendInput; // -1 to +1
float targetThrust = baseThrustPower + collectiveInput * thrustPower;

// Gravity compensation
float gravityCompensation = Physics.gravity.magnitude * gravityCompensationFactor;
totalVerticalForce = targetThrust + gravityCompensation;

// Apply thrust along craft's up vector
rb.AddForce(transform.up * totalVerticalForce, ForceMode.Acceleration);
```

### Vectored Thrust (Normal Mode)

```csharp
// Forward acceleration from pitch angle
float pitchAngle = Vector3.SignedAngle(Vector3.up, transform.up, transform.right);
float forwardComponent = Mathf.Sin(pitchAngle * Mathf.Deg2Rad) * thrustPower;
rb.AddForce(transform.forward * forwardComponent, ForceMode.Acceleration);
```

### Attitude Control (PD Controller)

```csharp
// Target attitude from inputs
float targetPitch = pitchInput * maxPitchDegrees;
float targetRoll = rollInput * maxRollDegrees;
float targetYaw = currentYaw + yawInput * yawRate * Time.fixedDeltaTime;

// Current attitude
Vector3 currentEuler = transform.eulerAngles;
float currentPitch = NormalizeAngle(currentEuler.x);
float currentRoll = NormalizeAngle(currentEuler.z);

// PD controller for each axis
float pitchError = targetPitch - currentPitch;
float pitchTorque = pitchError * stabilizationP - angularVelocity.x * stabilizationD;

float rollError = targetRoll - currentRoll;
float rollTorque = rollError * stabilizationP - angularVelocity.z * stabilizationD;

// Apply torques in local space
rb.AddRelativeTorque(pitchTorque, yawTorque, rollTorque, ForceMode.Acceleration);
```

### Strafe Mode Velocity Control

```csharp
// Calculate target velocity from camera-relative input
Vector3 cameraForward = Vector3.ProjectOnPlane(camera.forward, Vector3.up).normalized;
Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);

Vector3 targetVelocity = (cameraForward * moveInput.y + cameraRight * moveInput.x) * strafeMaxSpeed;

// Current horizontal velocity
Vector3 currentHorizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

// Acceleration toward target
Vector3 velocityDiff = targetVelocity - currentHorizontalVel;
float accel = moveInput.magnitude > 0.1f ? strafeAccel : strafeBraking;
Vector3 force = Vector3.ClampMagnitude(velocityDiff, accel);

rb.AddForce(force, ForceMode.Acceleration);

// Apply damping
rb.AddForce(-currentHorizontalVel * strafeDamping, ForceMode.Acceleration);
```

### Ground Cushion

```csharp
if (gearRetracted && Physics.Raycast(transform.position, Vector3.down, out hit, hoverRayLength))
{
    float distance = hit.distance;
    if (distance < hoverTargetHeight)
    {
        float error = hoverTargetHeight - distance;
        float springForce = error * hoverSpring;
        float dampForce = rb.velocity.y * hoverDamp;
        float cushionForce = springForce - dampForce;
        rb.AddForce(Vector3.up * cushionForce, ForceMode.Acceleration);
    }
}
```

---

## Tuning Parameters

### Serialized Configuration (Inspector Exposed)

```csharp
[System.Serializable]
public class JetpackSettings
{
    [Header("Attitude Limits")]
    [Range(10f, 45f)] public float maxPitchDegrees = 30f;
    [Range(10f, 45f)] public float maxRollDegrees = 25f;

    [Header("Yaw Control")]
    public float yawRate = 90f;           // degrees per second
    public float yawDamping = 3f;

    [Header("Thrust")]
    public float thrustPower = 15f;       // m/s² acceleration
    public float verticalDamping = 2f;
    [Range(0.5f, 1.5f)] public float gravityCompensationFactor = 1.0f;

    [Header("Angular Stabilization (PD Gains)")]
    public float stabilizationP = 10f;    // Proportional gain
    public float stabilizationD = 5f;     // Derivative gain (damping)

    [Header("Strafe Mode")]
    public float strafeMaxSpeed = 15f;    // m/s (reduced from normal)
    public float strafeAccel = 25f;       // m/s² acceleration
    public float strafeBraking = 40f;     // m/s² braking when no input
    public float strafeDamping = 2f;
    public float strafeFacingSmoothTime = 0.2f;

    [Header("Hover Cushion")]
    public float hoverTargetHeight = 2f;  // meters
    public float hoverSpring = 50f;
    public float hoverDamp = 10f;
    public float hoverRayLength = 10f;

    [Header("Boost")]
    public float boostForce = 30f;        // m/s² additional acceleration
    public float boostDuration = 3f;      // seconds
    public float lowAltitudeThreshold = 5f; // meters (for boost activation)

    [Header("Landing Detection")]
    public float groundedRayLength = 1.5f;
    public float maxGroundedVerticalSpeed = 1f;
    public float minGroundedTime = 0.5f;

    [Header("Normal Flight Speed")]
    public float normalMaxSpeed = 40f;    // m/s
    public float pitchSpeedMultiplier = 1.5f;
}
```

### Recommended Starting Values

| Parameter | Value | Notes |
|-----------|-------|-------|
| maxPitchDegrees | 30° | Thruster-like, not acrobatic |
| maxRollDegrees | 25° | Stable banking |
| yawRate | 90°/s | Responsive but not twitchy |
| thrustPower | 15 m/s² | Strong lift |
| stabilizationP | 10 | Snappy correction |
| stabilizationD | 5 | Prevents overshoot |
| strafeMaxSpeed | 15 m/s | ~54 km/h, slower than normal |
| strafeAccel | 25 m/s² | Quick dodge response |
| hoverTargetHeight | 2m | Safe clearance |
| boostDuration | 3s | Short burst |

---

## Debug & Verification

### On-Screen Debug Display

```
┌─────────────────────────────────────┐
│ JETPACK DEBUG                       │
├─────────────────────────────────────┤
│ State: Flight                       │
│ Mode: Normal (Shift for Strafe)     │
│ Altitude: 12.4m                     │
│ Vertical Speed: +2.3 m/s            │
│ Horizontal Speed: 18.7 m/s          │
│ Pitch: 15.2° Roll: -3.1°            │
├─────────────────────────────────────┤
│ Boost: ████████░░ 2.4s remaining    │
│ Gear: Retracted                     │
│ Cushion: Active (dist: 1.8m)        │
└─────────────────────────────────────┘
```

### Console Logging

- State transitions
- Boost activation/depletion/recharge
- Landing detection events
- Warning on unusual physics values

### Gizmos Visualization

```csharp
void OnDrawGizmos()
{
    // Ground raycast
    Gizmos.color = Color.yellow;
    Gizmos.DrawRay(transform.position, Vector3.down * hoverRayLength);

    // Hover target height
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(transform.position + Vector3.down * hoverTargetHeight, 0.3f);

    // Current velocity vector
    Gizmos.color = Color.blue;
    Gizmos.DrawRay(transform.position, rb.velocity * 0.5f);

    // Target velocity (strafe mode)
    Gizmos.color = Color.cyan;
    Gizmos.DrawRay(transform.position, targetVelocity * 0.5f);

    // Thrust direction
    Gizmos.color = Color.red;
    Gizmos.DrawRay(transform.position, transform.up * 3f);
}
```

---

## Acceptance Tests

### Test 1: Basic Flight Cycle
1. Start grounded
2. Press Space → takeoff, hover at ~3m
3. Release all input → stable hover (no oscillation)
4. Pitch forward (W) → move forward
5. Release → stop reliably
6. Descend (Ctrl) → approach ground
7. Land with gear deployed → no bouncing

**Pass Criteria:** Smooth transitions, no physics glitches

### Test 2: Strafe Mode
1. While hovering, hold Shift
2. Craft becomes upright
3. Press A → strafe left quickly
4. Release A → stop quickly
5. Move camera → facing follows
6. Verify speed is lower than normal flight

**Pass Criteria:** Responsive dodging, stable upright, camera-following facing

### Test 3: Ground Cushion
1. Retract gear (G)
2. Hover at ~5m
3. Cut thrust entirely (Ctrl)
4. Observe: should slow descent as approaching ground
5. Should not slam into ground
6. Deploy gear → normal landing resumes

**Pass Criteria:** Soft "bounce" when dropping, no hard impact

### Test 4: Boost System
1. From grounded or low altitude
2. Press Alt → boost activates
3. Strong acceleration for ~3s
4. Boost ends → normal flight
5. Try boost again → should fail (depleted)
6. Land and wait → boost recharges
7. Takeoff → boost available again

**Pass Criteria:** Clear boost activation/depletion/recharge cycle

### Test 5: Performance Stability
1. Run at 30 FPS, 60 FPS, 120 FPS
2. No runaway forces or torques
3. No NaN values in transforms
4. Consistent behavior across framerates

**Pass Criteria:** Physics-stable at all framerates

---

## Implementation Plan

### Phase 1: Foundation
1. Create Input Actions asset (`JetpackInputActions.inputactions`)
2. Create base `JetpackController.cs` with Rigidbody setup
3. Implement basic thrust (up/down)
4. Implement gravity compensation

### Phase 2: Attitude Control
5. Implement PD attitude controller
6. Add pitch/roll limits
7. Implement yaw control
8. Add auto-level when no input

### Phase 3: Normal Flight Mode
9. Implement vectored thrust (forward from pitch)
10. Add horizontal speed limiting
11. Tune stabilization gains

### Phase 4: Strafe Mode
12. Implement strafe mode toggle
13. Add camera-relative velocity control
14. Implement facing-follows-camera
15. Add quick stop braking

### Phase 5: Boost System
16. Implement boost force application
17. Add boost timer/duration
18. Implement landing-based recharge
19. Add boost availability conditions

### Phase 6: Ground Systems
20. Implement gear toggle
21. Add ground cushion raycast
22. Implement cushion spring/damp
23. Add safe exit/landing logic

### Phase 7: Polish & Debug
24. Add debug UI overlay
25. Implement gizmos
26. Add console logging
27. Performance testing
28. Final tuning pass

---

## File Structure

```
Assets/
├── Docs/
│   └── Feature_ThrusterJetpack.md          # This document
├── Scripts/
│   └── Vehicles/
│       └── Jetpack/
│           ├── JetpackController.cs        # Main controller
│           ├── JetpackSettings.cs          # ScriptableObject settings
│           ├── JetpackStateMachine.cs      # State management
│           ├── JetpackDebugUI.cs           # Debug overlay
│           └── JetpackInputHandler.cs      # Input System wrapper
├── Settings/
│   └── Input/
│       └── JetpackInputActions.inputactions # Input mappings
└── Prefabs/
    └── Vehicles/
        └── Jetpack/
            └── Jetpack.prefab              # Complete jetpack prefab
```

---

## Integration Notes

### Camera Integration
- Works with existing `ThirdPersonCam.cs`
- Requires camera reference for strafe mode direction
- May need additional camera mode for jetpack flight

### Player Integration
- Similar to `PlayerCarControll.cs` pattern for enter/exit
- Disable `PlayerController` when mounting jetpack
- Transfer player model to jetpack or attach jetpack to player

### Input System Migration Note
The current project uses the legacy Input Manager. This feature requires:
1. Install/enable Input System package (already in project)
2. Create Input Actions asset for jetpack
3. Can coexist with legacy input (Player Settings → Both)

---

## References

- GTA Online Mammoth Thruster behavior
- Unity Rigidbody physics documentation
- PID Controller theory for games
- Unity Input System documentation

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-30 | Initial design document |

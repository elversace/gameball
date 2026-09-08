# Cat Ball Game — Phase 1 CLEAN MERGED CANDIDATE

This package contains one canonical implementation of each runtime class. The duplicate root/subfolder scripts from the previous merge are removed.

Canonical runtime scripts:
- GameEvents
- GameManager
- BallController
- TrajectoryPredictor
- CameraController
- AimInputController
- TargetController
- UIManager
- Phase1Bootstrap

Editor tool:
- Editor_PhaseOneSceneBuilder — optional scene generator for Unity Editor.

Phase 1 gameplay:
- Touch/mouse grab must begin near the ball.
- Drag controls launch power and direction.
- Trajectory uses 0.04 s sampling and SphereCast with actual ball radius.
- Ball has linear + angular settle checks and a flight timeout warning.
- Targets reset safely on Retry.
- Win/Fail UI panels are mutually exclusive.
- 60 FPS target and Landscape Left are configured at runtime.

Unity target: 2022.3 LTS (tested structurally against 2022.3 APIs; Unity Editor runtime/Play Mode was not available in this environment).


## PHASE1_FINAL fixes
- Trajectory preview now ignores the ball's own collider when performing SphereCastAll collision checks.
- Runtime Bootstrap now wires the Main Camera Controller's follow target to the active ball, matching the Editor scene builder.
- No runtime Unity compile/Play Mode test was performed in this environment.


## Final Release v2 validation
- Based directly on PHASE1_FINAL_REVIEWED.zip.
- TrajectoryPredictor keeps SphereCastNonAlloc with a reusable RaycastHit buffer; Unity documents this API as the non-allocating variant of SphereCastAll.
- Ball self-hit filtering remains component-based via BallController; no Layer dependency was introduced into trajectory logic.
- GameManager no longer directly resets BallController during Retry; BallController owns its reset through GameEvents.OnRetry.
- UIManager initializes panels hidden during Configure rather than hiding them in Start, preventing an early LevelFail event from being immediately concealed.
- Legacy InputManager.asset and TagManager.asset are included for project reproducibility.
- Unity editor/runtime compile and Play Mode were not executed in this environment.

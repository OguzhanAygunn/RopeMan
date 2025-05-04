__________________________________________________________________________________________

Package "Ragdoll Animator 2"
Version 1.0.3.3

Made by FImpossible Creations - Filip Moeglich
http://www.fimpossiblecreations.pl
FImpossibleGames@Gmail.com

__________________________________________________________________________________________

Asset Store: https://assetstore.unity.com/publishers/37262
Youtube: https://www.youtube.com/channel/UCDvDWSr6MAu1Qy9vX4w8jkw
Facebook: https://www.facebook.com/FImpossibleCreations
Twitter (@FimpossibleC): https://twitter.com/FImpossibleC

___________________________________________________

Ragdoll Animator 2 on the Asset Store: 

Check User Manual file for detailed description of the plugin.
Check tutorials: https://www.youtube.com/playlist?list=PL6MURe5By90nYgMbXHucsy8wUvuvPJGuT
Take look at `Helper Methods List` file and `Extra Features List` file as help for creating custom physical interactions.

__________________________________________________________________________________________

Reminder:
Unity joints are not supporting scaling during playmode, so just initial scale is supported!
If you encounter something like spine jittery, try lowering muscles spring power parameter and increase damping a bit.

__________________________________________________________________________________________
Changelog:

version 1.0.3.3
- Implemented 'myRagdoll.Handler.IsStandUpCoroutineRunning' property

version 1.0.3.2
- Optimize On Zero Blend + Kinematic Anchor On Max + Enable back from 'Optimize On Zero Blend' state anchor bone freeze fix
- Implemented 'ForceSyncRoot' method to sync dummy position, before running the ragdoll from the disabled state

version 1.0.3.1
- Default settings for fist/feet bones should be scaled with source bone lossy scale (to avoid generating default huge colliders if lossy scale is giant like 100)

version 1.0.3
- Added collission detection events support for attachables

version 1.0.2.11
- Fixed issue with re-attaching Attachables with multiple colliders

version 1.0.2.10
- Fixed auto get up layer mask field
- Fixed blend tree poser layer mask field
- Disabling scene icons of bone indicators
- Fixed Unity 6 Preview Warning

version 1.0.2.9
- Ragdoll Animator component's Motion Influence compensation effect will not be applied during fall mode.

version 1.0.2.8
- Added object pooling example scene in the demos .unitypackage

version 1.0.2.7
- Updated 'Pose Manipulator' Extra Feature with few more parameters
- Fix for rare error on first build when used Pre-Generated dummy, which was caused by OnValidate()

version 1.0.2.6
- Switching from Off to Fall mode will work properly now

version 1.0.2.5
- Calibration will not be applied on the anchor bone during fall animating mode
- OnEnable during fall mode will not force restoring character pose
- 'Optimize' extra feature now has 'Fade Speed' parameter and 'Store Pose' toggle

version 1.0.2.4
- Added link to Ragdoll Animator asset store when hiting "?" button on the right corner of the component
- Fixed few user methods (coroutines)

version 1.0.2.3
- Changed restore pose operations on turning Ragdoll Animator ON after being disabled

version 1.0.2.2
- Export all Ragdoll Animator Settings and read settings from the file feature
- Thickness Multiplier and Scale Multiplier parameters slider can exceed value 2 now
- 'Copy all extra features' (right mouse button on the extra features title text) is now copying .enabled state properly

version 1.0.2.1
- Changing collider types on pre-generated ragdoll dummy will be properly removed during edit mode
- Adjusting colliders on pre-generated ragdoll dummy using scene handles will update scene components
- Fixed mesh collider type support

version 1.0.2
- Added Motion Influence parameter
- Added Motion Influence parameter for Magnet Point
- Added Kinematic Anchor Unaffected switch
- Added Disable Mecanim On Sleep Mode switch
- Added Pose Manipulator Extra Feature
- Removed GUI warning for unity 6.0.9+ versions

version 1.0.1.2
- GUI null protections
- Get bone by id methods implemented (using ERagdollBoneID enum)

version 1.0.1.1
- Bones without child transforms now can be previewed under Construct -> Physics

version 1.0.1
- There was typo in Auto Get Up Feature property name (Freeze Source Animator Hips)
- Example Shooter Attach Demo bullets will not move already attached ragdolls
- New Utility Extra Feature: Velocity Solver Iterations

version 1.0.0.5
- Changing falling mode through inspector window will trigger "On Fall Mode Change" actions properly (helpful for testing and debugging)

version 1.0.0.4
- Added "Ragdoll Animator 2 - Demos - Unity Versions Below 2022 fix.unitypackage" file, since demo scenes was made in unity 2022 and opening them in lower versions is causing box colliders reset 

version 1.0.0.3
- Enabling Ragdoll Animator after being disabled, is teleporting body parts to the source position

version 1.0.0.2
- Fixed auto setup making thin colliders radius for scaled skeletons

version 1.0.0.1
- Added Colliders Thickness slider

Version 1.0.0
- Initial Release
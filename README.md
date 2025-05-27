# PhysAnim for Unity

<div align="center">
<img 
    src="Media~/ragdoll_gif.gif" 
    alt="Ragdoll gif">
</img>
</div>

PhysAnim is a Unity tool that enables physical animations through motor-based and keyframe-based ragdoll pose matching similar to those found in Star Wars: Fallen Order or Uncharted 4.

## Goal

Real-Time CGI are now great enough to make the environment, the characters feel almost life-like and allowing blending between physics and animations and overall putting an emphasis on interactions is the last missing piece to create truly immersive experiences. In VR/AR or high fidelity experiences, interactions are crucial as the lack of interactivity (or bugs) can easily break the player's immersion.
The goal of PhysAnim is to bridge that last gap.

## Quickstart Guide

### Installation

To add package to your project, you need to : 
1. Open the Unity Package Manager (via Window > Package Manager) 
2. You can then click "Add Package from Git URL" and copy the [url of this repository](https://github.com/lenoctambule/PhysAnim-Unity.git).

### Usage

:warning: First, you have to make sure that your animator's culling mode is set to "Always animate" and the update mode to animate physics.

1. Add Pose Match script component (via Add component > Scripts > PhysAnim > Pose Match) in any Game Object that is not one of the parents nor the object that you wish to physically animate.
2. Assign into reference field the root of the ragdoll you want to animate.
3. Hit "Auto-detect and add character joints" button to add all the joints to the Motor and Keyframed joints lists or/and add them manually.
4. Tweak the settings to your liking.

There are 3 modes :
    - **Ragdolling** which will match poses only using the motor-driven joints
    - **Partially keyframed** which will mix both motor-driven and keyframed driven.
    - **Fully keyframed** which will match the animation one to one (but it is better to just disable the script).

## Acknowledgments

- Michal Mach's GDC Talk : [Physics Animation in Uncharted 4: A Thief's End](https://www.youtube.com/watch?v=7S-_vuoKgR4)
- Bartlomiej Waszak's GDC Talk : [Physical Animation in Star Wars Jedi: Fallen Order](https://www.youtube.com/watch?v=TmAU8aPekEo)
- Michael Stevenson : For [ConfigurableJointsExtensions.cs](https://gist.github.com/mstevenson/7b85893e8caf5ca034e6)

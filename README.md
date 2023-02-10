# PhysAnim for Unity

<div align="center">
<img 
    src="Media/ragdoll_gif.gif" 
    alt="Ragdoll gif">
</img>
</div>

PhysAnim is a Unity tool that enables physical animations through motor-based and keyframe-based ragdoll pose matching similar to those found in Star Wars: Fallen Order or Uncharted 4.

## Goal

Real-Time CGI are now great enough to make the environment, the characters feel almost life-like and allowing blending between physics and animations and overall putting an emphasis on interactions is the last missing piece to create truly immersive experiences. In VR/AR or high fidelity experiences, interactions are crucial as the lack of interactivity (or bugs) can easily break the player's immersion.
The goal of PhysAnim is to bridge that last gap.

## Roadmap

- [X] Local motor-based pose-matching
- [X] Global keyframe-based pose-matching
- [X] Physics to regular animation transitioning

## Quickstart Guide

:warning: First, you have to make sure that your animator's culling mode is set to "Always animate" and the update mode to animate physics.

1. Slide the PoseMatch.cs script in any GameObject into the inspector.
2. Assign into reference the root of the ragdoll you want to animate.
3. Hit "Auto-detect and add character joints" button to add all the joints to the Motor-driven joints list or add and tweak the strengths manually.
4. You can also add Keyframe-driven limbs and tweak their stiffness.
5. You have 3 modes :
    - **Ragdolling** which will match poses only using the motor-driven joints
    - **Partially keyframed** which will mix both motor-driven and keyframed driven.
    - **Fully keyframed** which will match the animation one to one but it is better to just disable the script.

## Acknowledgments

- Michal Mach's GDC Talk : [Physics Animation in Uncharted 4: A Thief's End](https://www.youtube.com/watch?v=7S-_vuoKgR4)
- Bartlomiej Waszak's GDC Talk : [Physical Animation in Star Wars Jedi: Fallen Order](https://www.youtube.com/watch?v=TmAU8aPekEo)
- Michael Stevenson : For [ConfigurableJointsExtensions.cs](https://gist.github.com/mstevenson/7b85893e8caf5ca034e6)

This repository contains all the relevant material for this project:
# AR for Enhanced Video Viewing During Cardio Exercise

This project investigates the potential of Augmented Reality (AR) to enhance the video viewing experience during cardio exercise in gyms. It addresses common limitations of existing solutions, such as phone placement, which often lead to poor usability due to small screen sizes, uncomfortable viewing angles, and instability, especially during high-intensity workouts.

## The Problem

Current methods of consuming visual media during cardio exercise, primarily using phones, present several challenges:
* **Small Screen Sizes and Poor Positioning**: Phones placed on machine control panels result in small screens positioned below eye level, causing neck strain and reduced visibility.
* **Instability**: Phone setups become unstable during high-intensity exercise, leading most participants to watch videos only at low or medium intensity.
* **Lack of Personalization**: Large mounted TVs in gyms often lack personalization and are poorly placed.

## The Solution: AR Video System

An AR video system was developed utilizing the Meta Quest 3S headset, incorporating three distinct viewing modes:
* **Phone-like**: Simulates the current phone viewing experience.
* **Static AR**: A large, static virtual screen placed at a neutral eye level, simulating a TV-like experience.
* **Adaptive AR**: A large virtual screen that dynamically adjusts its size based on user movement and exercise intensity, aiming to match visual load with physical exertion.

The system anchors the video feed in the world to minimize mental workload and increase reading speed, based on prior research. The adaptive screen scales through three sizes: 60° horizontal FOV for low intensity, 45° for medium intensity, and 30° for high intensity, aligning with cinematic, THX, and SMPTE recommendations respectively.

## Methodology

The project involved:
1.  **Design Exploration**:
    * Observations of 86 gym-goers revealed that approximately half consumed video content.
    * Semi-structured interviews with 10 individuals highlighted issues with screen size, position, comfort, and intensity-dependent problems.
    * Investigation into the available space in the field of view (FOV) during cardio exercises was conducted to assess viable screen placement options.

2.  **User Study**:
    * A study with 16 participants evaluated the three viewing modes across three cardio machines: a treadmill, elliptical, and rowing machine.
    * Participants completed surveys on perceived exertion and their experience with each viewing mode, followed by semi-structured interviews.

## Key Findings

* **Preference for AR Modes**: Both Static AR and Adaptive AR modes were strongly preferred over the Phone-like mode. AR screens were perceived as significantly easier to view, more comfortable, and more engaging.
* **Adaptive AR for Rowing**: The Adaptive AR mode was particularly favored on the rowing machine due to its "follow effect," which mitigated motion sickness during vigorous movements.
* **Static AR for Treadmill/Elliptical**: The Static AR mode often proved more comfortable on the treadmill and elliptical due to less dynamic movement.
* **Impact of Intensity**: The Phone mode became significantly more difficult to watch as exercise intensity increased. Participants expressed a desire for screens to be less distracting at higher intensities.
* **Ergonomic Integration**: Participants emphasized the importance of natural integration of screens that allowed them to maintain proper posture and minimize head or eye movement.
* **User Customization**: There was a desire for personalization and control over screen placement and size adjustments, rather than fully automated systems.

## Limitations and Future Work

* **Headset Comfort**: The weight of the Meta Quest 3S headset caused neck discomfort during extended use, and bouncing during intense treadmill exercise reduced viewing stability.
* **Audio Issues**: Built-in speaker audio was sometimes obscured by gym noise, suggesting the need for subtitles or improved audio solutions.
* **RPE Assessment**: The subjective RPE scale used for intensity measurement showed variability, highlighting the need for more objective methods like heart rate tracking.
* **Adaptive Mode Feedback**: The adaptive mode's size-changing feature received mixed feedback, with some finding it distracting or the screen becoming too small at higher intensities.

Future research should focus on trying this on lighter and more stable headsets, investigating more accurate exertion tracking methods, and refining adaptive interfaces to better accommodate diverse user preferences and workout conditions.

## Prerequisites
1.  **Meta SDK**:
Add this to your Unity Assets: https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269657
After installing, inside Unity: Window -> Package Manager -> Select "My Assets" -> Install the Unity XR All-in-One SDK

2.  **TMPro (TextMeshPro)**:
Inside Unity: Window -> Package Manager -> Select "Unity Registry" -> Search "TextMeshPro" and install

3.  **NUnit**:
Inside Unity: Window -> Package Manager -> Select "Unity Registry" -> Search "Test Framework" and install

4. **Android Build Tool**
Inside Unity: File -> Build Settings -> Android -> Install

## Videos
As of right now, the implementation in VideoScript.cs is only set up for our study use case (playing a specific set of series).
In order to play a video, change the implementation in VideoScript.cs to support your video file(s).
Then, install an app like SideQuest to upload your video(s) to their respective file locations (which again, requires you to change the VideoScript.cs implementation).

## Build
The APK for this project is stored at ARVid/ProjectBuild.apk
Install it on your Meta Quest 3S by installing the [Android Debug Bridge (adb)](https://developer.android.com/tools/adb) and running "adb install ProjectBuild.apk" in the command prompt.

## Controls
* **Start / Pause video**: A
* **Change viewing method**: B
* **Restart video**: Shift + A

### Intensity
* **Change intensity**: Index button

### Video Time
* **Fast-forward (4x speed)**: Shift + Right
* **-10 seconds**: Shift + Left
* **+1 minute**: Shift + Up
* **-1 minute**: Shift + Down

### Other
* **Start / Stop recording**: Shift + B
* **Toggle settings**: Shift + Index
* **Increase recording time**: Up
* **Decrease recording time**: Down


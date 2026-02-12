# CRANE Simulation Tool

## Purpose

The Cross-domain Robotics Autonomous Navigation Engine (CRANE) proposes a generalized and highly modular control stack designed to support autonomous navigation across a wide range of robotic platforms operating in distinct environmental domains, including aerial, underwater, ground, and surface environments. Existing navigation systems are frequently tailored to specific hardware or conditions, limiting portability and reuse. CRANE addresses this challenge by defining universal abstraction layers within perception, planning, and control that enable core navigation behaviors such as state estimation, mapping, waypoint following, obstacle avoidance, recovery behaviors, and high-level decision-making to be reused with minimal platform-specific modification. The project integrates simulation-based development with real-world testing through a shared physics abstraction layer to evaluate the transferability of navigation strategies between different domains. Data collection and cross-platform performance analysis will quantify navigation accuracy, robustness, and efficiency. The overarching objective of CRANE is to demonstrate that a unified control architecture can significantly reduce hardware-dependent redesign and enable scalable, domain-agnostic autonomous navigation. 

## Usage

Clone the repo by running:
```bash
git clone https://github.com/1unarzDev/crane_sim
```

Then, start by installing [Unity Hub](https://docs.unity3d.com/hub/manual/InstallHub.html) for your OS. Add the project (the repo you cloned) by navigating to `Add > Add project from disk`, then selecting the appropriate folder. You will then be prompted to install Unity Engine. 

If your device is running OpenGL graphics, you may need to install [Vulkan](https://www.vulkan.org/tools#vulkan-gpu-resources). After installing Vulkan and Unity Engine, navigate to `Edit > Project Settings > Player > Other Settings > Graphics API for Linux` and make sure it is set to use Vulkan.

Finally, make sure you have selected the correct scene from `Assets/Scenes` and set the `ROSConnection` object IP to the one corresponding to the device you're running ROS on. For more details view the bottom section of the [mhseals_docker repo](https://github.com/mhseals/mhseals_docker).

The simulator has been build with customizability and modularity in mind, all physics scripts can be easily adapted in the `Assets/Scripts` folder. Importing URDFs can also be easily done by right clicking the hierarchy and selecting `3D Object > URDF Model (Import)`. 

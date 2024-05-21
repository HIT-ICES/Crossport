# Crossport

This repository contains most of the resources for reproducing Crossport, but some of the content is proprietary (by PICO or Unity) and therefore cannot be attached here.

File Structure：

- apps: Applications used for experiment
    - bistro: 
- docs: Brief documentation
- src: Source code of Toolchain and middleware
    - tools: Toolchain
        - gateway: Crossport Gateway
        - unity: Prefabs
    - middleware: Crossport Middleware, systemd services and configurations

## Compile Test Apps

1. Download necessary files from ORCA or Github
2. Copy `src\tools\unity` into `Assets`
3. Integrate necessary LinuxXR Layouts for proprietary devices
4. Compile using Unity 2022.3


If you have any trouble, feel free to contact me.
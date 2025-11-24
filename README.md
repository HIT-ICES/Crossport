# Crossport

> A out-of-box deployment-ready artifacts can be found [here](https://drive.google.com/file/d/1-2g9LcesghyS9UJQ-zDNjOUj6L_3Rc3T/view?usp=sharing).

This repository contains most of the resources for reproducing Crossport, but some of the content is proprietary (by PICO or Unity) or too large and therefore cannot be attached here.

## Citation

If you use this work in your research, please cite:

```bibtex
@ARTICLE{su2025crossport,
  author={Su, Zihang and He, Xiang and Wang, Wenrui and Wang, Zhongjie},
  journal={IEEE Transactions on Services Computing},
  title={Crossport: a Cloud-Edge-End Microservice Architecture for Collaborative Rendering in Metaverse Services},
  year={2025},
  volume={},
  number={},
  pages={1-13},
  keywords={Metaverse;Rendering (computer graphics);Microservice architectures;Collaboration;Cloud computing;Computer architecture;Resource management;Real-time systems;Costs;Servers;Metaverse Services;Microservices;Cloud-Edge End Collaboration;Collaborative Rendering},
  doi={10.1109/TSC.2025.3633668}
}
```

**Paper**: Z. Su, X. He, W. Wang and Z. Wang, "Crossport: a Cloud-Edge-End Microservice Architecture for Collaborative Rendering in Metaverse Services," in *IEEE Transactions on Services Computing*, doi: [10.1109/TSC.2025.3633668](https://doi.org/10.1109/TSC.2025.3633668).

File Structure：

- apps: Applications used for experiment
    - bistro: Get `Bistro_v5_2 and unzip from [ORCA](https://developer.nvidia.com/orca/); 
    - emerald-square-classroom: Get `EmeraldSquare_v4_1` and unzip from [ORCA](https://developer.nvidia.com/orca/); Get most of the resources from [origin author](https://github.com/Arduino-Projects/The-Virtual-Classroom), override the files with given ones.
    - hogwarts: Get most of the resources from [origin author](https://github.com/OpenHogwarts/hogwarts), override the files with given ones.
    - tales-of-evil-sword: Get most of the resources from [origin author](https://github.com/Uyouii/TalesOfEvilSword_Finished), override the files with given ones.
- docs: Brief documentation
- src: Source code of Toolchain and middleware
    - tools: Toolchain
        - gateway: Crossport Gateway
        - unity: Prefabs
    - middleware: Crossport Middleware, systemd services and configurations
        - services: Systemd services
        - shared: Shared resources, please download `virtualgl_3.1_amd64.deb` manually.
        - images: docker images

## Compile Test Apps

1. Download necessary files from ORCA or Github
2. Copy `src\tools\unity` into `Assets`
3. Integrate necessary LinuxXR Layouts for proprietary devices
4. Compile using Unity 2022.3


If you have any trouble, feel free to contact me.
---
sidebar_position: 1
---

# Containerize and Deployment

## Setup Runtime of Host

You should first set up the Linux host's environment (it might be a cluster node).

1. Make sure the machine has an attached monitor and NVIDIA GPU. Install the driver for your GPU, and run `nvidia-smi` to check if the driver works properly.
2. Configure the machine with VirtualGL following the [official documentation](https://rawcdn.githack.com/VirtualGL/virtualgl/3.1/doc/index.html) with GLX Back End. Remember **NOT** to restrict privileges (select `n` option for all three questions).
3. Install `xvfb` with the package manager.
4. Run `xvfb-run vglrun /opt/VirtualGL/bin/glxinfo |grep 'OpenGL'` and `xvfb-run vglrun /opt/VirtualGL/bin/glxspheres64` to see if the environment works properly.
5. Run `xhost +` to allow containers to connect to the 3D X Server on the host.
6. **Disattach the monitor.** This operation is essential. Otherwise, the program will be prolonged when the monitor is black.

## Build `vglbase-image`

To run Unity Application in a Linux headless server (or a container), we need VirtualGL and xvfb. So we need to build a base image with the required environment.

1. Download `virtualgl_3.1_amd64.deb` from [VirtualGL's official website](https://www.virtualgl.org/), and place it into the working directory.
2. Create `setupenv`.sh`. Remember to setup your DNS, timezone, and apt repository:
```bash
cp /etc/apt/sources.list /etc/apt/sources.list.old
echo nameserver 192.168.1.1 >> /etc/resolv.conf # Change to your own dns
DEBIAN_FRONTEND=noninteractive

ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime # Change to your local time zone
echo "YOUR_TIMEZONE" > /etc/timezone # Change to your local time zone

apt-get update
apt-get upgrade -y

apt-get install ca-certificates -y
apt-get install --no-install-recommends build-essential libc++1 apt-utils -y
apt-get install --no-install-recommends xvfb x11vnc -y #xserver-xorg libxtst6 libxv1 libglu1-mesa libegl1-mesa -y
apt-get install -y --no-install-recommends \
    ca-certificates curl dialog wget less sudo lsof git net-tools psmisc xz-utils nemo net-tools \
    lubuntu-core terminator zenity make cmake gcc libc6-dev \
    x11-xkb-utils xauth xfonts-base xkb-data \
    mesa-utils xvfb libgl1-mesa-dri libgl1-mesa-glx libglib2.0-0 libxext6 libsm6 libxrender1 \
    libglu1 libglu1:i386 libxv1 libxv1:i386 \
    libsuitesparse-dev libgtest-dev \
    libeigen3-dev libsdl1.2-dev libignition-math2-dev libarmadillo-dev libarmadillo${LIBARMADILLO_VERSION} libsdl-image1.2-dev libsdl-dev

dpkg -i virtualgl_3.1_amd64.deb

apt install -f -y

mv /etc/X11/xorg.conf /etc/X11/xorg.conf.old

cat > /etc/X11/xorg.conf <<EOF
Section "DRI"
        Mode 0666
EndSection

Section "ServerLayout"
    Identifier     "Layout0"
    Screen      0  "Screen0"
    InputDevice    "Keyboard0" "CoreKeyboard"
    InputDevice    "Mouse0" "CorePointer"
EndSection

Section "Files"
EndSection

Section "InputDevice"
    # generated from default
    Identifier     "Mouse0"
    Driver         "mouse"
    Option         "Protocol" "auto"
    Option         "Device" "/dev/psaux"
    Option         "Emulate3Buttons" "no"
    Option         "ZAxisMapping" "4 5"
EndSection

Section "InputDevice"
    # generated from default
    Identifier     "Keyboard0"
    Driver         "kbd"
EndSection

Section "Monitor"
    Identifier     "Monitor0"
    VendorName     "Unknown"
    ModelName      "Unknown"
    Option         "DPMS"
EndSection

Section "Device"
    Identifier     "Device0"
    Driver         "nvidia"
    VendorName     "NVIDIA Corporation"
EndSection

Section "Screen"
    Identifier     "Screen0"
    Device         "Device0"
    Monitor        "Monitor0"
    DefaultDepth    24
    SubSection     "Display"
        Depth       24
    EndSubSection
EndSection
EOF
```
3. Create `Dockerfile`:
```dockerfile
FROM nvidia/opengl:1.2-glvnd-devel-ubuntu20.04
ARG DEBIAN_FRONTEND=noninteractive
COPY . .
RUN /bin/bash setupenv.sh
```
4. Run `docker build -t <tag> .` to build the image.
5. Start the image with Nvidia container runtime and arguments `--gpus all -v /tmp/.X11-unix/X0:/tmp/.X11-unix/X0:ro`.
6. Attach to the container, Run `xvfb-run vglrun /opt/VirtualGL/bin/glxinfo |grep 'OpenGL'` and `xvfb-run vglrun /opt/VirtualGL/bin/glxspheres64` to see if the environment works properly.

## Containerize your Application

1. We have now created a Remote Renderer Application in [the last post](/docs/remote-rendering/start). Make a Linux build of the Remote Renderer, and put all the files into the `/app` folder of the container building workspace. Remember to include the crossport setting files.
2. Run `chmod +x /app/<your app>.x86_64` to make the application executable (assuming you have built it in Windows).
3. Create `Dockerfile`. The `<vglbase-image>` is built in the last section:
```dockerfile
FROM <vglbase-image>
COPY /app /app
WORKDIR /app
ENTRYPOINT xvfb-run -s "-screen 0 2560x1440x24" vglrun /app/<your app>.x86_64 -ph -logfile /dev/stdout
```
4. Run `docker build -t <tag> .` to build the image.

Notice:

- The `"-screen 0 2560x1440x24"` can be replaced with other display settings.
- `-logfile /dev/stdout` redirects the Unity Player logs to the container stdout so you can check the full logs with `docker/kubectl logs -f`.
- `--gpus all -v /tmp/.X11-unix/X0:/tmp/.X11-unix/X0:ro` is required for starting the container.

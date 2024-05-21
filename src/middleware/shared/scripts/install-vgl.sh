apt-get install --no-install-recommends xvfb x11vnc -y
apt-get install -y --no-install-recommends \
    ca-certificates curl dialog wget less sudo lsof git net-tools psmisc xz-utils nemo net-tools \
    terminator zenity make cmake gcc libc6-dev \
    x11-xkb-utils xauth xfonts-base xkb-data \
    mesa-utils xvfb libgl1-mesa-dri libgl1-mesa-glx libglib2.0-0 libxext6 libsm6 libxrender1 \
    libglu1 libxv1  \
    libsuitesparse-dev libgtest-dev \
    libeigen3-dev libsdl1.2-dev libarmadillo-dev libsdl-image1.2-dev libsdl-dev


dpkg -i virtualgl_3.1_amd64.deb

apt install -f -y

FROM nvidia/cudagl:11.4.2-devel-ubuntu20.04

ARG DEBIAN_FRONTEND=noninteractive

COPY shared/virtualgl_3.1_amd64.deb virtualgl_3.1_amd64.deb
COPY shared/scripts /scripts

WORKDIR /

RUN /bin/bash /scripts/apt-update.sh

RUN /bin/bash /scripts/install-vgl.sh


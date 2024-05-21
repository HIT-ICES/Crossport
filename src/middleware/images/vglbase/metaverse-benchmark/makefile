# Crossport App Conf.
APP_NAME = metaverse
CMP_NAME = benchmark
VERSION = 0.1
SIG_SRV ?= 10.123.3.1
SIG_URL ?= $(SIG_SRV)/sig

# Project Conf.
PROJECT = metaverse
PROJECT_ROOT ?= ../../../
DOCKERFILE_DIR = .
WORK_DIR = .
IMAGE_REPO = 10.123.3.1:5000/$(PROJECT)

# Image Conf.
IMAGE_NAME ?= $(APP_NAME)/$(CMP_NAME)
IMAGE_TAG ?= v$(VERSION)
CONTAINER_NAME = $(APP_NAME)-$(CMP_NAME)
SH_CONTAINER_NAME = $(CONTAINER_NAME)_sh
IMAGE_FULL_NAME = $(IMAGE_REPO)/$(IMAGE_NAME):$(IMAGE_TAG)

# K8s Conf.
NAMESPACE ?= $(PROJECT)
K8S = kubectl -n $(NAMESPACE)
K8S_REPLICA = 1
K8S_CONF_TMPL ?= $(PROJECT_ROOT)/images/apps/common/deployment.yaml

# Docker Conf.
DOCKER = docker
DEFAULT_DOCKER_RUN_OPTIONS = --cap-add=SYS_PTRACE --gpus all --shm-size=1024m --network=host

# Vgl Conf.
USE_VGL ?= true
X ?= $(shell find /tmp/.X11-unix/ -type s -print -quit | sed -e 's/.*X\([0-9]\+\)/\1/')

VGL_OPTIONS = --gpus all -e NVIDIA_DRIVER_CAPABILITIES=compute,utility,video,display -v /tmp/.X11-unix/X$(X):/tmp/.X11-unix/X0:ro -e VGL_DISPLAY=:0
ifeq ($(USE_VGL),true)
RUN_OPTIONS = $(DEFAULT_DOCKER_RUN_OPTIONS) $(VGL_OPTIONS)
else
RUN_OPTIONS = $(DEFAULT_DOCKER_RUN_OPTIONS)
endif

# Targets

.PHONY: default install restart start stop logs uninstall deploy sh status

default:
	$(DOCKER) build -f $(DOCKERFILE_DIR)/Dockerfile -t $(IMAGE_NAME):$(IMAGE_TAG) $(WORK_DIR)

tag: default
	$(DOCKER) tag $(IMAGE_NAME):$(IMAGE_TAG) $(IMAGE_FULL_NAME)

publish: tag
	$(DOCKER) push $(IMAGE_FULL_NAME)

install:
	VGL_DISPLAY_NO=$(X) SIG_SRV=$(SIG_URL) APP_NAME=$(APP_NAME) CMP_NAME=$(CMP_NAME) \
	IMG_REPO=$(IMAGE_REPO) IMG_NAME=$(IMAGE_NAME) IMG_TAG=$(IMAGE_TAG) \
	REPLICAS=$(K8S_REPLICA) envsubst < $(K8S_CONF_TMPL) | $(K8S) apply -f -

uninstall:
	VGL_DISPLAY_NO=$(X) SIG_SRV=$(SIG_URL) APP_NAME=$(APP_NAME) CMP_NAME=$(CMP_NAME) \
	IMG_REPO=$(IMAGE_REPO) IMG_NAME=$(IMAGE_NAME) IMG_TAG=$(IMAGE_TAG) \
	REPLICAS=$(K8S_REPLICA) envsubst < $(K8S_CONF_TMPL) | $(K8S) delete -f -

restart:
	$(K8S) delete pod -l 'app=$(APP_NAME)-$(CMP_NAME)'

start:
	$(K8S) scale deployment -l 'app=$(APP_NAME)-$(CMP_NAME)' --replicas=$(K8S_REPLICA)

stop:
	$(K8S) scale deployment -l 'app=$(APP_NAME)-$(CMP_NAME)' --replicas=0

logs:
	$(K8S) logs -f -l 'app=$(APP_NAME)-$(CMP_NAME)'

status:
	watch $(K8S) get pods -l 'app=$(APP_NAME)-$(CMP_NAME)'

stats:
	watch $(K8S) top pods -l 'app=$(APP_NAME)-$(CMP_NAME)'

docker-check:
	echo $(RUN_OPTIONS)

docker-install:
	$(DOCKER) run $(RUN_OPTIONS) -tid --restart=on-failure --name $(CONTAINER_NAME) \
    -e SIG_SRV=$(SIG_URL) \
	-e APP_NAME=$(APP_NAME) \
	-e CMP_NAME=$(CMP_NAME) \
    $(IMAGE_FULL_NAME)

docker-restart:
	$(DOCKER) restart $(CONTAINER_NAME)

docker-start:
	$(DOCKER) start $(CONTAINER_NAME)

docker-stop:
	$(DOCKER) stop $(CONTAINER_NAME)

docker-logs:
	$(DOCKER) logs -f $(CONTAINER_NAME)

docker-uninstall:
	$(DOCKER) rm -f $(CONTAINER_NAME)

docker-sh:
	$(DOCKER) run $(RUN_OPTIONS) -tid --restart=on-failure --name $(SH_CONTAINER_NAME) \
	--entrypoint=/bin/sh $(IMAGE_NAME):$(IMAGE_TAG)

docker-exec: sh
	$(DOCKER) attach $(SH_CONTAINER_NAME)


docker-vglinfo: sh
	$(DOCKER) exec $(SH_CONTAINER_NAME) sh /scripts/vglinfo.sh
	$(DOCKER) rm -f $(SH_CONTAINER_NAME)

docker-vgltest: sh
	$(DOCKER) exec $(SH_CONTAINER_NAME) sh /scripts/vgltest.sh
	$(DOCKER) rm -f $(SH_CONTAINER_NAME)

docker-cleansh:
	$(DOCKER) rm -f $(SH_CONTAINER_NAME)

docker-status:
	$(DOCKER) ps -a | grep $(CONTAINER_NAME)
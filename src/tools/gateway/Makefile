IMAGE_NAME = crossport-server
IMAGE_TAG = 0.7.1
DOCKERFILE_DIR = .
WORK_DIR = .
PROJECT = metaverse
REPO_NAME = 10.123.3.1:5000/$(PROJECT)
APP_NAME ?= $(IMAGE_NAME)
SH_APP_NAME = $(APP_NAME)_sh
DOCKER = docker
DEFAULT_RUN_OPTIONS = --cap-add=SYS_PTRACE --shm-size=1024m

K8S = kubectl -n $(PROJECT)
K8S_REPLICA = 1


.PHONY: default install restart start stop logs uninstall deploy sh status

default:
	$(DOCKER) build -f $(DOCKERFILE_DIR)/Dockerfile -t $(IMAGE_NAME):$(IMAGE_TAG) $(WORK_DIR)

tag: default
	$(DOCKER) tag $(IMAGE_NAME):$(IMAGE_TAG) $(REPO_NAME)/$(IMAGE_NAME):$(IMAGE_TAG)

publish: tag
	$(DOCKER) push $(REPO_NAME)/$(IMAGE_NAME):$(IMAGE_TAG)

restart:
	$(K8S) delete pod -f -l 'app=$(APP_NAME)'

start:
	$(K8S) scale deployment -l 'app=$(APP_NAME)' --replicas=$(K8S_REPLICA)

stop:
	$(K8S) scale deployment -l 'app=$(APP_NAME)' --replicas=0

logs:
	$(K8S) logs -f -l 'app=$(APP_NAME)'

uninstall:
	IMG_TAG=$(IMAGE_TAG) SVC_NAME=$(IMAGE_NAME) IMG_REPO=$(REPO_NAME) envsubst < deploy.yaml | $(K8S) delete -f -

install:
	IMG_TAG=$(IMAGE_TAG) SVC_NAME=$(IMAGE_NAME) IMG_REPO=$(REPO_NAME) envsubst < deploy.yaml | $(K8S) apply -f -

status:
	watch $(K8S) get pods -l 'app=$(APP_NAME)'

docker-install:
	$(DOCKER) run $(RUN_OPTIONS) -p 80:80 -tid --restart=on-failure --name $(APP_NAME) $(IMAGE_NAME):$(IMAGE_TAG)

docker-restart:
	$(DOCKER) restart $(APP_NAME)

docker-start:
	$(DOCKER) start $(APP_NAME)

docker-stop:
	$(DOCKER) stop $(APP_NAME)

docker-logs:
	$(DOCKER) logs -f $(APP_NAME)

docker-uninstall:
	$(DOCKER) rm -f $(APP_NAME)

docker-sh:
	$(DOCKER) run $(RUN_OPTIONS) -tid --restart=on-failure --name $(SH_APP_NAME) --entrypoint=/bin/sh $(IMAGE_NAME):$(IMAGE_TAG)

docker-runsh: sh
	$(DOCKER) attach $(SH_APP_NAME)

docker-cleansh:
	$(DOCKER) rm -f $(SH_APP_NAME)

docker-status:
	$(DOCKER) ps -a | grep $(IMAGE_NAME)
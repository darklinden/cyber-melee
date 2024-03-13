#!/usr/bin/env bash

BASEDIR=$(dirname "$0")
DOCKER_DIR="$(realpath "${BASEDIR}")"
ROOT_DIR="$(realpath "${DOCKER_DIR}/..")"
SERVER_DIR="$(realpath "${ROOT_DIR}/server")"
PROJECT_DIR="$(realpath "${SERVER_DIR}/ws-server")"

cd $PROJECT_DIR || exit

IMAGE_NAME="lockstep-server"
IMAGE_TAG="0.0.2"

echo "Will build image: $IMAGE_NAME:$IMAGE_TAG"

# rm docker containers and images if exists
CONTAINERS=$(docker ps -a -q -f name=$IMAGE_NAME)
if [ -n "$CONTAINERS" ]; then
    docker rm -f $CONTAINERS
fi
IMAGES=$(docker images -q $IMAGE_NAME)
if [ -n "$IMAGES" ]; then
    docker rmi -f $IMAGES
fi

# build image
docker build --progress=plain --no-cache -t $IMAGE_NAME:$IMAGE_TAG -f ./Dockerfile ./

# upload image
docker image tag $IMAGE_NAME:$IMAGE_TAG darklinden/$IMAGE_NAME:$IMAGE_TAG
docker push darklinden/$IMAGE_NAME:$IMAGE_TAG
docker image tag darklinden/$IMAGE_NAME:$IMAGE_TAG darklinden/$IMAGE_NAME:latest
docker push darklinden/$IMAGE_NAME:latest

# docker save $IMAGE_NAME:$IMAGE_TAG | gzip >$IMAGE_NAME.tar.gz

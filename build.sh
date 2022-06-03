#!/bin/sh
echo "$(tput setaf 2)"
echo "Building paulgilchrist/mongodb-api:arm64"
echo "$(tput setaf 7)"
docker build --rm -f "Dockerfile" --no-cache --platform linux/arm64 -t paulgilchrist/mongodb-api:arm64 .
echo "$(tput setaf 2)"
echo "Pushing paulgilchrist/mongodb-api:arm64 to https://hub.docker.com"
echo "$(tput setaf 7)"
docker push paulgilchrist/mongodb-api:arm64
echo "$(tput setaf 2)"
echo "Building paulgilchrist/mongodb-api:amd64"
echo "$(tput setaf 7)"
docker build --rm -f "Dockerfile" --no-cache --platform linux/amd64 -t paulgilchrist/mongodb-api:amd64 .
echo "$(tput setaf 2)"
echo "Pushing paulgilchrist/mongodb-api:amd64 to https://hub.docker.com"
echo "$(tput setaf 7)"
docker push paulgilchrist/mongodb-api:amd64
echo "$(tput setaf 2)"
echo "Removing paulgilchrist/mongodb-api:latest manifest"
echo "$(tput setaf 7)"
docker manifest rm paulgilchrist/mongodb-api:latest
echo "$(tput setaf 2)"
echo "Creating paulgilchrist/mongodb-api:latest manifest"
echo "$(tput setaf 7)"
docker manifest create paulgilchrist/mongodb-api:latest paulgilchrist/mongodb-api:arm64 paulgilchrist/mongodb-api:amd64
echo "$(tput setaf 2)"
echo "Pushing paulgilchrist/mongodb-api:latest manifest to https://hub.docker.com"
echo "$(tput setaf 7)"
docker manifest push paulgilchrist/mongodb-api:latest
echo "$(tput setaf 2)"
echo "Build complete"
echo "Don't forget to update Kubernetes. For example:"
echo "$(tput setaf 3)"
echo "    kubectl rollout restart deployment contacts-api -n demo"
echo "$(tput setaf 7)"

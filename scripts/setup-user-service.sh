#!/bin/bash

set -e

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

echo "Setting up user service..."

# Clean up existing user service resources
echo "Cleaning up existing user service resources..."
kubectl delete deployment user-service -n appointments-app --ignore-not-found=true
kubectl delete service user-service -n appointments-app --ignore-not-found=true

echo "Building user-service Docker image..."
eval "$(minikube docker-env --shell bash)"
cd "$SCRIPT_DIR"/../services/user-service/src/ || exit

IMAGE_NAME="user-service"
IMAGE_VERSION="0.1.$(date +%s)"

docker build -t "$IMAGE_NAME:$IMAGE_VERSION" -f ./UserService.Api/Dockerfile .
cd ../../../

echo "Deploying user-service with image version: $IMAGE_VERSION"

# Deploy user-service with the latest version of the image
DEPLOYMENT=$(cat infrastructure/kubernetes/user-service/deployment.yaml)
DEPLOYMENT=${DEPLOYMENT/$IMAGE_NAME:latest/$IMAGE_NAME:$IMAGE_VERSION}
echo "$DEPLOYMENT" | kubectl apply -f - -n appointments-app
kubectl apply -f infrastructure/kubernetes/user-service/service.yaml -n appointments-app

echo "Waiting for user service to be ready..."
kubectl wait --for=condition=ready pod -l app=user-service --timeout=120s -n appointments-app

echo ""
echo "User service ready."
echo "Image version: $IMAGE_NAME:$IMAGE_VERSION"
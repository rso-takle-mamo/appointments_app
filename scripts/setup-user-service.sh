#!/bin/bash

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

# Build User Service image
echo "Building User Service Docker image..."
eval "$(minikube docker-env --shell bash)"
cd "$SCRIPT_DIR"/../services/user-service/src/ || exit

IMAGE_NAME="user-service"
IMAGE_VERSION="0.1.$(date +%s)"

docker build -t "$IMAGE_NAME:$IMAGE_VERSION" -f ./UserService.Api/Dockerfile .
cd ../../../

# Deploy service-catalog Service with the latest version of the image
DEPLOYMENT=$(cat infrastructure/kubernetes/user-service/deployment.yaml)
DEPLOYMENT=${DEPLOYMENT/$IMAGE_NAME:latest/$IMAGE_NAME:$IMAGE_VERSION}
echo "$DEPLOYMENT" | kubectl apply -f -

kubectl apply -f infrastructure/kubernetes/user-service/service.yaml

kubectl wait --for=condition=ready pod -l app=user-service --timeout=120s

echo ""
echo "User Service URL:"
minikube service user-service --url
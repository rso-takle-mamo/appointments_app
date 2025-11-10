#!/bin/bash

set -e

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

echo "Setting up service catalog..."

# Clean up existing service catalog resources
echo "Cleaning up existing service catalog resources..."
kubectl delete deployment service-catalog-service -n appointments-app --ignore-not-found=true
kubectl delete service service-catalog-service -n appointments-app --ignore-not-found=true

echo "Building service-catalog Service Docker image..."
eval "$(minikube docker-env --shell bash)"
cd "$SCRIPT_DIR"/../services/service-catalog-service/src/ || exit

IMAGE_NAME="service-catalog-service"
IMAGE_VERSION="0.1.$(date +%s)"

docker build -t "$IMAGE_NAME:$IMAGE_VERSION" -f ./ServiceCatalogService.Api/Dockerfile .
cd ../../../

echo "Deploying service-catalog Service with image version: $IMAGE_VERSION"

# Deploy service-catalog Service with the latest version of the image
DEPLOYMENT=$(cat infrastructure/kubernetes/service-catalog-service/deployment.yaml)
DEPLOYMENT=${DEPLOYMENT/$IMAGE_NAME:latest/$IMAGE_NAME:$IMAGE_VERSION}
echo "$DEPLOYMENT" | kubectl apply -f - -n appointments-app
kubectl apply -f infrastructure/kubernetes/service-catalog-service/service.yaml -n appointments-app

echo "Waiting for catalog service to be ready..."
kubectl wait --for=condition=ready pod -l app=service-catalog-service --timeout=120s -n appointments-app

echo ""
echo "Service catalog ready."
echo "Image version: $IMAGE_NAME:$IMAGE_VERSION"
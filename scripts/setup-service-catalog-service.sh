#!/bin/bash

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

# Build service-catalog Service image
echo "Building service-catalog Service Docker image..."
eval "$(minikube docker-env --shell bash)"
cd "$SCRIPT_DIR"/../services/service-catalog-service/src/ || exit

IMAGE_NAME="service-catalog-service"
IMAGE_VERSION="0.1.$(date +%s)"

docker build -t "$IMAGE_NAME:$IMAGE_VERSION" -f ./ServiceCatalogService.Api/Dockerfile .
cd ../../../

# Deploy service-catalog Service with the latest version of the image
DEPLOYMENT=$(cat infrastructure/kubernetes/service-catalog-service/deployment.yaml)
DEPLOYMENT=${DEPLOYMENT/$IMAGE_NAME:latest/$IMAGE_NAME:$IMAGE_VERSION}
echo "$DEPLOYMENT" | kubectl apply -f -
kubectl apply -f infrastructure/kubernetes/service-catalog-service/service.yaml
kubectl wait --for=condition=ready pod -l app=service-catalog-service --timeout=120s

echo ""
echo "service-catalog Service URL:"
minikube service service-catalog-service --url
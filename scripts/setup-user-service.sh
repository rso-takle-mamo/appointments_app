#!/bin/bash

# Build User Service image
echo "Building User Service Docker image..."
eval "$(minikube docker-env --shell bash)"
cd services/user-service/src/UserService.Api
docker build -t user-service:latest .
cd ../../../../

# Deploy User Service with database connection
kubectl apply -f infrastructure/kubernetes/user-service/deployment.yaml
kubectl apply -f infrastructure/kubernetes/user-service/service.yaml

kubectl wait --for=condition=ready pod -l app=user-service --timeout=120s

echo ""
echo "User Service URL:"
minikube service user-service --url
#!/bin/bash

set -e

echo "Setting up service secrets..."

kubectl apply -f infrastructure/kubernetes/shared/secrets/user-service-secret.yaml -n appointments-app
kubectl apply -f infrastructure/kubernetes/shared/secrets/service-catalog-service-secret.yaml -n appointments-app

echo "Service secrets ready."
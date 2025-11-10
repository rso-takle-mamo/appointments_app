#!/bin/bash

set -e

echo "Setting up services..."

kubectl apply -f infrastructure/kubernetes/shared/secrets/ -n appointments-app
kubectl apply -f infrastructure/kubernetes/user-service/ -n appointments-app
kubectl apply -f infrastructure/kubernetes/service-catalog-service/ -n appointments-app

echo "Waiting for user service migration..."
kubectl wait --for=condition=complete job/migration-users-job --timeout=120s -n appointments-app

echo "Waiting for catalog service migration..."
kubectl wait --for=condition=complete job/migration-service-catalog-job --timeout=120s -n appointments-app

echo "Waiting for services to be ready..."
kubectl wait --for=condition=ready pod -l app=user-service --timeout=60s -n appointments-app
kubectl wait --for=condition=ready pod -l app=service-catalog-service --timeout=60s -n appointments-app

echo "Services ready."
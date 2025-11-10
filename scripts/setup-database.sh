#!/bin/bash

set -e

echo "Setting up database..."

kubectl apply -f infrastructure/kubernetes/shared/secrets/postgresql-secret.yaml -n appointments-app
kubectl apply -f infrastructure/kubernetes/shared/postgresql/ -n appointments-app

echo "Waiting for PostgreSQL..."
kubectl wait --for=condition=ready pod -l app=shared-postgresql --timeout=120s -n appointments-app

echo "Database ready."
#!/bin/bash

# Set Minikube Docker environment (not needed for database but good practice)
eval "$(minikube docker-env --shell bash)"

# Apply infrastructure in order
kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-pvc.yaml

kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-secret.yaml

kubectl apply -f infrastructure/kubernetes/shared/secrets/user-service-secret.yaml
kubectl apply -f infrastructure/kubernetes/shared/secrets/service-catalog-service-secret.yaml

kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-configmap.yaml

kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-service.yaml

kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-deployment.yaml

echo "Waiting for database to start up. This can take a while..."

kubectl wait --for=condition=ready pod -l app=shared-postgresql --timeout=180s

echo "Database Details:"
echo "   Service: shared-postgresql-service:5432"
echo "Running migrations for all services..."

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

bash "$SCRIPT_DIR/migrate-all.sh"

echo "All migrations complete"
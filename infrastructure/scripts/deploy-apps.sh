#!/bin/bash

# Deploy Appointments App Microservices
set -e

# Deploy the application
helm upgrade --install appointments-app ../deployments/appointments-app \
  --namespace appointments-app \
  --create-namespace \
  --values ../deployments/appointments-app/values-minikube.yaml

echo "Service deployed successfully to namespace: appointments-app"
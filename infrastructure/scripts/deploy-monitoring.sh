#!/bin/bash

# Deploy Monitoring Stack (Prometheus and Grafana)
set -e

# Add Helm repositories if not already added
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add grafana https://grafana.github.io/helm-charts
#helm repo add fluent https://fluent.github.io/helm-charts
helm repo update

# Update chart dependencies
cd ../deployments/monitoring
helm dependency update
cd - > /dev/null

# Deploy the monitoring stack
helm upgrade --install monitoring ../deployments/monitoring \
 --namespace monitoring \
 --create-namespace \
 --values ../deployments/monitoring/values-minikube.yaml

echo "Monitoring deployed successfully to namespace: monitoring"
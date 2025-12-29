#!/bin/bash

# Deploy Kafka (Event Streaming)
set -e

# Deploy Kafka
helm upgrade --install kafka ../deployments/kafka \
  --namespace kafka \
  --create-namespace \
  --values ../deployments/kafka/values-minikube.yaml

echo "Waiting for Kafka to be ready..."
kubectl wait --for=condition=ready pod -l app.kubernetes.io/name=kafka -n kafka --timeout=300s

echo "Kafka deployed successfully to namespace: kafka"

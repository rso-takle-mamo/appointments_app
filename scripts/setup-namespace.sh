#!/bin/bash

set -e

echo "Setting up namespace..."

kubectl apply -f infrastructure/kubernetes/shared/namespace/namespace.yaml

echo "Namespace ready."
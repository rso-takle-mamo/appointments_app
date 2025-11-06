#!/bin/bash

echo "CLEANING UP ALL KUBERNETES RESOURCES..."
echo "=========================================="
echo "WARNING: This will delete ALL deployments, services, PVCs, and data!"
echo ""

# Ask for confirmation
read -p "Are you sure you want to delete everything? (yes/no): " confirm
if [ "$confirm" != "yes" ]; then
    echo "Cleanup cancelled."
    exit 0
fi

echo ""
echo "Starting cleanup..."

echo "Deleting all deployments..."
kubectl delete deployment --all --ignore-not-found=true

echo "Deleting all services..."
kubectl delete service --all --ignore-not-found=true

echo "Deleting all pods..."
kubectl delete pod --all --ignore-not-found=true

echo "Deleting all PVCs (this will delete all data)..."
kubectl delete pvc --all --ignore-not-found=true

echo "Deleting all ConfigMaps..."
kubectl delete configmap --all --ignore-not-found=true

echo "Deleting all Secrets..."
kubectl delete secret --all --ignore-not-found=true

echo "Waiting for resources to be cleaned up..."
sleep 10
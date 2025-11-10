#!/bin/bash

echo "Cleaning up appointments-app resources..."
echo "======================================="
echo "WARNING: This will delete ALL deployments, services, PVCs, ingress, migrations, and data!"
echo ""

read -p "Are you sure you want to delete everything? (yes/no): " confirm
if [ "$confirm" != "yes" ]; then
    echo "Cleanup cancelled."
    exit 0
fi

echo ""
echo "Starting cleanup..."

# Delete namespace (removes EVERYTHING in appointments-app namespace)
echo "Deleting appointments-app namespace..."
kubectl delete namespace appointments-app --ignore-not-found=true

echo "Cleanup completed."
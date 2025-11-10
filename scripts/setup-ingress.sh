#!/bin/bash

set -e

minikube addons enable ingress

echo "Setting up ingress..."

kubectl apply -f infrastructure/kubernetes/shared/ingress/api-ingress.yaml -n appointments-app

echo "Waiting for ingress..."
kubectl wait --for=jsonpath='{.status.loadBalancer.ingress}' ingress/api-ingress -n appointments-app --timeout=120s

echo "Getting Minikube IP..."
MINIKUBE_IP=$(minikube ip)

echo ""
echo "Use port-forward for localhost:"
echo "  kubectl port-forward -n ingress-nginx service/ingress-nginx-controller 8080:80"
echo "  Then access: http://localhost:8080/..."
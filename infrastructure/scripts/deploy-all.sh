#!/bin/bash

# Deploy complete stack (Kafka + Apps + Monitoring)
set -e

cd "$(dirname "$0")"

./deploy-kafka.sh
./deploy-apps.sh
./deploy-monitoring.sh

echo ""
echo "Stack deployed successfully!"
echo "Namespaces: appointments-app, monitoring"
echo ""
echo "Access services:"
echo "  kubectl port-forward -n ingress-nginx svc/ingress-nginx-controller 8080:80"
echo "  kubectl port-forward -n monitoring svc/monitoring-prometheus-server 9090:80"
echo "  kubectl port-forward -n monitoring svc/monitoring-grafana 3000:80"
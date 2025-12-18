#!/bin/bash

# Deploy complete stack (Apps + Monitoring)
set -e


# Deploy applications first
echo "Deploying application services..."
cd "$(dirname "$0")"
./deploy-apps.sh
echo ""

# Deploy monitoring stack
echo "Deploying monitoring stack..."
./deploy-monitoring.sh
echo ""

echo "Deployed successfully!"
echo ""
echo "Make sure to port forward to access it"
echo "   - API: kubectl port-forward -n ingress-nginx svc/ingress-nginx-controller 8080:80 -> localhost:8080/api..."
echo "   - Prometheus: kubectl port-forward -n monitoring svc/monitoring-prometheus-server 9090:80 -> http://localhost:9090"
echo "   - Grafana: kubectl port-forward -n monitoring svc/monitoring-grafana 3000:3000 -> http://localhost:3000"
echo ""
echo "Namespaces:"
echo "   - Applications: appointments-app"
echo "   - Monitoring: monitoring"
#!/bin/bash

# Set Minikube Docker environment (not needed for database but good practice)
eval "$(minikube docker-env --shell bash)"

# Apply infrastructure in order
kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-pvc.yaml

kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-secret.yaml
kubectl apply -f infrastructure/kubernetes/shared/secrets/user-service-secret.yaml

kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-configmap.yaml

kubectl create configmap shared-postgresql-init-scripts \
  --from-file=infrastructure/kubernetes/shared/postgresql/init-scripts/ \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-service.yaml

kubectl apply -f infrastructure/kubernetes/shared/postgresql/postgresql-deployment.yaml

kubectl wait --for=condition=ready pod -l app=shared-postgresql --timeout=180s

echo "Verifying database initialization..."

kubectl exec deployment/shared-postgresql -- psql -U postgres -d postgres -c "\l userdb" | grep "userdb"

kubectl exec deployment/shared-postgresql -- psql -U postgres -d postgres -c "\du" | grep "userdb_user"

kubectl exec deployment/shared-postgresql -- psql -U userdb_user -d userdb -c "\dt" | grep -E "(users|tenants)"

echo "Database Details:"
echo "   Service: shared-postgresql-service:5432"
echo "   Admin: postgres (password in secret)"
echo "   User DB: userdb (user: userdb_user)"
echo ""
kubectl get secret user-service-db-secret -o jsonpath='{.data.connection-string}' | base64 --decode
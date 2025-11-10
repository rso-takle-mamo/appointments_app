#!/bin/bash

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

TARGET_MIGRATION=$1
export EXTERNAL_TARGET_MIGRATION="\"$TARGET_MIGRATION\""

IMAGE_NAME="service-catalog-database-migrator:latest"

cd "$SCRIPT_DIR"/../services/service-catalog-service/src/ || exit

eval "$(minikube docker-env --shell bash)"

echo "Creating user migrator docker image..."
docker build -t "$IMAGE_NAME" -f ./ServiceCatalogService.DatabaseMigrator/Dockerfile .

echo "Removing old job from kubernetes..."
kubectl delete jobs.batch/migration-service-catalog-job -n appointments-app --ignore-not-found=true

echo "Running migration job on kubernetes..."
envsubst < "$SCRIPT_DIR"/../infrastructure/kubernetes/service-catalog-service/migration-job.yaml | kubectl apply -f - -n appointments-app
JOB_POD=$(kubectl get pods --selector=batch.kubernetes.io/job-name=migration-service-catalog-job --output=jsonpath='{.items[*].metadata.name}' -n appointments-app)

echo "Waiting for job to finish (pod $JOB_POD)"
kubectl wait --for=condition=complete job/migration-service-catalog-job -n appointments-app
echo "Job completed"
echo "Logs from job:"
kubectl logs job/migration-service-catalog-job -n appointments-app

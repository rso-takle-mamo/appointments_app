#!/bin/bash

set -e

ACR_NAME="appointmentsapp"
ACR_LOGIN_SERVER="${ACR_NAME}.azurecr.io"
SERVICES=("user-service" "service-catalog-service" "availability-service" "booking-service" "notification-service")

echo "Logging in to Azure Container Registry..."
az acr login --name "$ACR_NAME"

get_migrator_name() {
  local service=$1
  echo "${service%-service}-database-migrator"
}

get_latest_tag() {
  local service=$1
  local migrator_name=$(get_migrator_name "$service")

  tags=$(az acr repository show-tags --name "$ACR_NAME" --repository "$migrator_name" --orderby time_desc --output tsv 2>/dev/null || echo "")

  if [ -z "$tags" ]; then
    echo "0.0.0"
  else
    echo "$tags" | head -n1
  fi
}

increment_version() {
  local version=$1
  local part=$2

  IFS='.' read -r major minor patch <<< "$version"

  case $part in
    major)
      major=$((major + 1))
      minor=0
      patch=0
      ;;
    minor)
      minor=$((minor + 1))
      patch=0
      ;;
    patch)
      patch=$((patch + 1))
      ;;
    *)
      echo "Invalid part to increment: $part"
      exit 1
      ;;
  esac

  echo "${major}.${minor}.${patch}"
}

echo "Select migrator services to build by typing their numbers separated by spaces:"
for i in "${!SERVICES[@]}"; do
  migrator_name=$(get_migrator_name "${SERVICES[i]}")
  echo "$((i+1))) $migrator_name"
done

read -p "Your choice: " -a choices

selected_services=()
for c in "${choices[@]}"; do
  if [[ $c =~ ^[1-9][0-9]*$ ]] && (( c >= 1 && c <= ${#SERVICES[@]} )); then
    selected_services+=("${SERVICES[c-1]}")
  else
    echo "Invalid selection: $c"
    exit 1
  fi
done

if [ ${#selected_services[@]} -eq 0 ]; then
  echo "No services selected. Exiting."
  exit 0
fi

for service in "${selected_services[@]}"; do
  echo ""
  migrator_name=$(get_migrator_name "$service")
  echo "Processing $migrator_name..."

  latest_tag=$(get_latest_tag "$service")
  echo "Latest tag for $migrator_name: $latest_tag"

  while true; do
    echo -n "Select version bump for $migrator_name (major, minor, patch): "
    read bump_part
    if [[ "$bump_part" == "major" || "$bump_part" == "minor" || "$bump_part" == "patch" ]]; then
      break
    else
      echo "Invalid input, please type major, minor, or patch."
    fi
  done

  if [ "$latest_tag" == "0.0.0" ]; then
    case $bump_part in
      major)
        new_tag="1.0.0"
        ;;
      minor)
        new_tag="0.1.0"
        ;;
      patch)
        new_tag="0.0.1"
        ;;
    esac
  else
    new_tag=$(increment_version "$latest_tag" "$bump_part")
  fi

  echo "New tag for $migrator_name: $new_tag"

  pascal_case=$(echo "$service" | sed -r 's/(^|-)([a-z])/\U\2/g' | sed 's/-//g')
  migrator_dockerfile_path="services/${service}/src/${pascal_case}.DatabaseMigrator/Dockerfile"
  migrator_build_context="services/${service}/src"

  if [ -f "$migrator_dockerfile_path" ]; then
    echo "Building migrator image ${migrator_name}:${new_tag}..."
    docker build -t "${ACR_LOGIN_SERVER}/${migrator_name}:${new_tag}" -f "$migrator_dockerfile_path" "$migrator_build_context" --no-cache

    echo "Pushing migrator image ${ACR_LOGIN_SERVER}/${migrator_name}:${new_tag}..."
    docker push "${ACR_LOGIN_SERVER}/${migrator_name}:${new_tag}"

    echo "$migrator_name done."
  else
    echo "ERROR: Migrator Dockerfile not found at $migrator_dockerfile_path"
    echo "Skipping $migrator_name..."
  fi
done

echo ""
echo "All selected migrator images built and pushed successfully."

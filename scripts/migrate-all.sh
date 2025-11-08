#!/bin/bash
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

bash "$SCRIPT_DIR/migrate-users.sh" "$1"
bash "$SCRIPT_DIR/migrate-service-catalogs.sh" "$1"
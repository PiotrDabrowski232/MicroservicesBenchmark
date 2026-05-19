#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
APP_DIR="$REPO_ROOT/tools/benchmark-results-viewer"

if [ -z "${RESULTS_ROOT:-}" ]; then
  if [ -d "$REPO_ROOT/RabbitMQResults" ] || [ -d "$REPO_ROOT/KafkaResults" ] || [ -d "$REPO_ROOT/LavinResults" ]; then
    RESULTS_ROOT="$REPO_ROOT"
  else
    RESULTS_ROOT="$HOME/benchmark-results"
  fi
fi
PORT="${PORT:-4173}"

export RESULTS_ROOT
export PORT

cd "$APP_DIR"

if ! command -v node >/dev/null 2>&1 || ! command -v npm >/dev/null 2>&1; then
  echo "Node.js and npm are required to start the benchmark results viewer."
  echo "Install Node.js 18+ and ensure both 'node' and 'npm' are available in PATH."
  exit 1
fi

if [ ! -d node_modules ]; then
  npm install
fi

npm run build
exec npm run start

#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")"/.. && pwd)"
SCRIPT_NAME="$(basename "$0")"

INFO_PREFIX="[INFO]"
WARN_PREFIX="[WARN]"
ERROR_PREFIX="[ERROR]"

usage() {
    cat <<EOF
Usage: $SCRIPT_NAME [options]

Provision or verify a local development environment for the TextEdit project.

Options:
  --verify-only       Run prerequisite checks without restoring or building
  --skip-restore      Skip dotnet restore
  --skip-build        Skip dotnet build (Debug configuration)
  --help              Show this help message

Examples:
  $SCRIPT_NAME
  $SCRIPT_NAME --verify-only
EOF
}

log_info()  { printf "%s %s\n" "$INFO_PREFIX"  "$*"; }
log_warn()  { printf "%s %s\n" "$WARN_PREFIX"  "$*" >&2; }
log_error() { printf "%s %s\n" "$ERROR_PREFIX" "$*" >&2; }

command_exists() {
    command -v "$1" >/dev/null 2>&1
}

require_command() {
    local cmd=$1
    if ! command_exists "$cmd"; then
        log_error "Missing required command: $cmd"
        exit 1
    fi
}

sdks_with_major() {
    local major=$1
    dotnet --list-sdks | awk '{print $1}' | grep -E "^${major}\." || true
}

VERIFY_ONLY=false
SKIP_RESTORE=false
SKIP_BUILD=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --verify-only) VERIFY_ONLY=true ;;
        --skip-restore) SKIP_RESTORE=true ;;
        --skip-build) SKIP_BUILD=true ;;
        --help) usage; exit 0 ;;
        *) log_error "Unknown option: $1"; usage; exit 1 ;;
    esac
    shift
done

log_info "Running from: $ROOT_DIR"

log_info "Checking prerequisite commands..."
require_command git
require_command dotnet

if command_exists node; then
    NODE_VERSION_RAW="$(node --version)"
    NODE_VERSION="${NODE_VERSION_RAW#v}"
    NODE_MAJOR="${NODE_VERSION%%.*}"
    log_info "Node.js found: $NODE_VERSION_RAW"
    if [[ "$NODE_MAJOR" =~ ^[0-9]+$ ]] && (( NODE_MAJOR < 18 )); then
        log_warn "Node.js major version ($NODE_MAJOR) is below required 18+. Upgrade recommended for extension host development."
    fi
else
    log_warn "Node.js not detected. VS Code-compatible extensions will require Node 18+."
fi

log_info "Inspecting installed .NET SDK versions..."
DOTNET_VERSION="$(dotnet --version || true)"
if [[ -z "$DOTNET_VERSION" ]]; then
    log_error "Unable to determine dotnet version."
    exit 1
fi
log_info "dotnet --version -> $DOTNET_VERSION"

if [[ -z "$(sdks_with_major 8)" ]]; then
    log_error "Missing required .NET 8 SDK. Install from https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi
log_info ".NET 8 SDK detected."

if [[ -z "$(sdks_with_major 10)" ]]; then
    log_warn "Preview .NET 10 SDK not detected. Some CI builds may target preview features."
else
    log_info ".NET 10 SDK detected."
fi

if [[ -f "$ROOT_DIR/.gitmodules" ]]; then
    log_info "Synchronizing git submodules..."
    git submodule update --init --recursive
else
    log_info "No git submodules configured."
fi

if [[ "$VERIFY_ONLY" == "true" ]]; then
    log_info "Verification complete. Skipping restore/build."
    exit 0
fi

if [[ "$SKIP_RESTORE" == "false" ]]; then
    log_info "Restoring solution dependencies..."
    dotnet restore "$ROOT_DIR/TextEdit.sln"
else
    log_info "Skipping dotnet restore per flag."
fi

if [[ "$SKIP_BUILD" == "false" ]]; then
    log_info "Building solution (Debug configuration)..."
    dotnet build "$ROOT_DIR/TextEdit.sln" --configuration Debug
else
    log_info "Skipping dotnet build per flag."
fi

log_info "Environment provisioning completed successfully."

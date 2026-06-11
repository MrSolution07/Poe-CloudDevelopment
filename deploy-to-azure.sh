#!/usr/bin/env bash
# Deploy EventEase to Azure Web App (run from repo root)
# Prerequisite: Azure CLI installed and logged in (az login)

set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
RESOURCE_GROUP="rg-WebApp"
APP_NAME="st10538419-eventease"
PUBLISH_DIR="$SCRIPT_DIR/publish"
ZIP_PATH="$SCRIPT_DIR/deploy.zip"

if ! command -v az &>/dev/null; then
  echo "Azure CLI (az) is not installed."
  echo "Install it: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-macos"
  exit 1
fi

if ! az account show &>/dev/null; then
  echo "Not logged in to Azure. Run: az login"
  exit 1
fi

# Build and publish
echo "Building app (Release)..."
dotnet publish "$SCRIPT_DIR/EventEaseApp" -c Release -o "$PUBLISH_DIR" --nologo -v q

# Repackage zip from fresh publish output
echo "Packaging deploy.zip..."
rm -f "$ZIP_PATH"
(cd "$PUBLISH_DIR" && zip -r "$ZIP_PATH" . -x "*.pdb" > /dev/null)

echo "Deploying to $APP_NAME (resource group: $RESOURCE_GROUP)..."
az webapp deploy \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_NAME" \
  --src-path "$ZIP_PATH" \
  --type zip

echo "Done. Visit: https://st10538419-eventease-ebbpdwa4dsbpg6cs.switzerlandnorth-01.azurewebsites.net/"

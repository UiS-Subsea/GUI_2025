#!/bin/bash

# Install SDL2 using Homebrew (skip if already installed)
echo "Checking if Homebrew is installed..."
if ! command -v brew &>/dev/null; then
    echo "Homebrew not found. Installing Homebrew..."
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

echo "Installing SDL2 via Homebrew..."
brew install sdl2

# Determine the platform (Intel vs Apple Silicon)
echo "Detecting platform..."
if [[ "$(uname -m)" == "arm64" ]]; then
    # Apple Silicon (M1/M2, etc.)
    echo "Detected Apple Silicon (M1/M2). Setting DYLD_LIBRARY_PATH for Apple Silicon..."
    export DYLD_LIBRARY_PATH="/opt/homebrew/lib:$DYLD_LIBRARY_PATH"
    # Add this export command to .zshrc for future sessions
    echo 'export DYLD_LIBRARY_PATH="/opt/homebrew/lib:$DYLD_LIBRARY_PATH"' >> ~/.zshrc
else
    # Intel Macs
    echo "Detected Intel Mac. Setting DYLD_LIBRARY_PATH for Intel..."
    export DYLD_LIBRARY_PATH="/usr/local/lib:$DYLD_LIBRARY_PATH"
    # Add this export command to .zshrc for future sessions
    echo 'export DYLD_LIBRARY_PATH="/usr/local/lib:$DYLD_LIBRARY_PATH"' >> ~/.zshrc
fi

# Reload the .zshrc file to apply the changes immediately
source ~/.zshrc

# Confirm SDL2 installation
echo "SDL2 has been installed, and DYLD_LIBRARY_PATH has been set."
echo "To make the DYLD_LIBRARY_PATH permanent and apply them to the current terminal run "source ~/.zshrc""

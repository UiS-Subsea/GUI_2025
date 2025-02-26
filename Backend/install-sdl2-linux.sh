#!/bin/bash

# Check if running as root
if [ "$(id -u)" -ne 0 ]; then
  echo "Please run this script as root or with sudo."
  exit 1
fi

# Determine the Linux distribution
if [ -f /etc/os-release ]; then
  . /etc/os-release
  DISTRO=$NAME
else
  echo "Unable to determine the distribution."
  exit 1
fi

echo "Detected distribution: $DISTRO"

# Install SDL2 based on the distribution
case $DISTRO in
  "Ubuntu" | "Debian" | "Linux Mint")
    echo "Installing SDL2 for Ubuntu/Debian-based distributions..."
    apt update
    apt install -y libsdl2-dev
    ;;
  "Fedora")
    echo "Installing SDL2 for Fedora..."
    dnf install -y SDL2-devel
    ;;
  "Arch Linux" | "Manjaro")
    echo "Installing SDL2 for Arch-based distributions..."
    pacman -S --noconfirm sdl2
    ;;
  *)
    echo "Distribution $DISTRO is not supported in this script. Please install SDL2 manually."
    exit 1
    ;;
esac

echo "SDL2 installation is complete!"

# Define the URL for SDL2 Windows binaries
$SDL2Url = "https://www.libsdl.org/release/SDL2-2.30.12-win32-x64.zip"
$SDL2Zip = "SDL2.zip"

# Download SDL2 for Windows
Write-Host "Downloading SDL2 for Windows..."
Invoke-WebRequest -Uri $SDL2Url -OutFile $SDL2Zip

# Extract the zip file to a temporary location
Write-Host "Extracting SDL2.zip..."
Expand-Archive -Path $SDL2Zip -DestinationPath "SDL2_Temp"

# Move SDL2.dll to the root folder
Move-Item -Path "SDL2_Temp\SDL2.dll" -Destination "." -Force

# Clean up
Remove-Item $SDL2Zip
Remove-Item -Recurse -Force "SDL2_Temp"

Write-Host "SDL2 installation for Windows is complete. You can now run the .NET project."
Write-Host "Use 'dotnet run' to start the project."

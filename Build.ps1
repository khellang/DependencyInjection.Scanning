function Install-Dotnet
{
  & where.exe dotnet 2>&1 | Out-Null

  if(($LASTEXITCODE -ne 0) -Or ((Test-Path Env:\APPVEYOR) -eq $true))
  {
    Write-Host "Dotnet CLI not found - downloading latest version"

    # Prepare the dotnet CLI folder
    $env:DOTNET_INSTALL_DIR="$(Convert-Path "$PSScriptRoot")\.dotnet\win7-x64"
    if (!(Test-Path $env:DOTNET_INSTALL_DIR))
    {
      mkdir $env:DOTNET_INSTALL_DIR | Out-Null
    }

    # Download the dotnet CLI install script
    if (!(Test-Path .\dotnet\install.ps1))
    {
      Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0-preview2/scripts/obtain/dotnet-install.ps1" -OutFile ".\.dotnet\dotnet-install.ps1"
    }

    # Run the dotnet CLI install
    & .\.dotnet\dotnet-install.ps1 -Version "1.0.0-preview2-003156"

    # Add the dotnet folder path to the process.
    Remove-PathVariable $env:DOTNET_INSTALL_DIR
    $env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"
  }
}

function Remove-PathVariable
{
  [cmdletbinding()]
  param([string] $VariableToRemove)
  $path = [Environment]::GetEnvironmentVariable("PATH", "User")
  $newItems = $path.Split(';') | Where-Object { $_.ToString() -inotlike $VariableToRemove }
  [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join(';', $newItems), "User")
  $path = [Environment]::GetEnvironmentVariable("PATH", "Process")
  $newItems = $path.Split(';') | Where-Object { $_.ToString() -inotlike $VariableToRemove }
  [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join(';', $newItems), "Process")
}

function Restore-Packages
{
    param([string] $DirectoryName)
    & dotnet restore -v Warning ("""" + $DirectoryName + """")
}

function Test-Project
{
    param([string] $DirectoryName)
    & dotnet test -c Release ("""" + $DirectoryName + """")
}

########################
# THE BUILD!
########################

Push-Location $PSScriptRoot

# Install Dotnet CLI
Install-Dotnet

# Package restore
Get-ChildItem -Path . -Filter *.xproj -Recurse | ForEach-Object { Restore-Packages $_.DirectoryName }

# Tests
Get-ChildItem -Path .\test -Filter *.xproj -Recurse | ForEach-Object { Test-Project $_.DirectoryName }

Pop-Location
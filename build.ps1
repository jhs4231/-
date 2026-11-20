param(
    [Parameter(Mandatory = $false)]
    [string]$SkulRoot = "${env:ProgramFiles(x86)}\Steam\steamapps\common\Skul"
)

$ErrorActionPreference = 'Stop'
$gameDll = Join-Path $SkulRoot 'Skul_Data\Managed\Assembly-CSharp.dll'
if (-not (Test-Path $gameDll)) {
    throw "Skul game DLL not found: $gameDll. Supply -SkulRoot pointing at the folder containing Skul.exe."
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    throw "dotnet SDK was not found. Install the .NET SDK and .NET Framework targeting packs."
}

$project = Join-Path $PSScriptRoot 'src\SkulExclusiveUnlocks\SkulExclusiveUnlocks.csproj'
Write-Host "Building against $gameDll"
& dotnet build $project -c Release "-p:SkulRoot=$SkulRoot" --no-incremental
if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE). Upload the compiler log for API compatibility fixes." }
Write-Host "Build finished. This does not verify in-game behavior."

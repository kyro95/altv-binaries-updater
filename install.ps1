$repoOwner = 'kyro95'
$repoName = 'altv-binaries-updater'
$releaseTag = 'latest'

$apiUrl = "https://api.github.com/repos/$repoOwner/$repoName/releases/$releaseTag"

$response = Invoke-RestMethod -Uri $apiUrl

$downloadUrl = $response.assets[0].browser_download_url

$downloadPath = "$env:TEMP\Release.zip"
$extractPath = "$env:ProgramFiles\altv-binaries-updater"

Invoke-WebRequest -Uri $downloadUrl -OutFile $downloadPath

if (-not (Test-Path $extractPath)) {
    New-Item -ItemType Directory -Path $extractPath | Out-Null
}

Expand-Archive -Path $downloadPath -DestinationPath $extractPath -Force

Remove-Item $downloadPath -Force

$aliasContent = @"
function altv-update {
    Set-Location "$extractPath"
    Write-Host "You are now in the altv-binaries-updater directory."
}
"@

Add-Content -Path $PROFILE.AllUsersCurrentHost -Value $aliasContent

Write-Host "altv-binaries-updater has been installed, and the global alias 'altv-update' has been added."

$repoOwner = 'kyro95'
$repoName = 'altv-binaries-updater'
$releaseTag = 'latest'
$apiUrl = "https://api.github.com/repos/$repoOwner/$repoName/releases/$releaseTag"
$response = Invoke-RestMethod -Uri $apiUrl
$downloadUrl = $response.assets[1].browser_download_url
$extractPath = "C:\Program Files\altv-binaries-updater"
$downloadPath = "$env:TEMP\AltVUpdater.zip"

Invoke-WebRequest -Uri $downloadUrl -OutFile $downloadPath
Expand-Archive -Path $downloadPath -DestinationPath $extractPath -Force
Remove-Item $downloadPath -Force

[Environment]::SetEnvironmentVariable("Path", "$($env:Path);$extractPath", [System.EnvironmentVariableTarget]::Machine)

Set-Alias -Name altv-updater -Value "$extractPath\AltV.Binaries.Updater.exe" -Scope Global
Write-Host "AltV Updater installed successfully. Please restart your terminal to use it."

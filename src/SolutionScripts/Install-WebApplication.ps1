function global:Install-WebApplication([string] $applicationPath) {
	Write-Notification "Installing web application from '$applicationPath'"

	Write-Notification "Installing web application to IIS"
	
	Write-Notification "Resetting deployment directory"
	Reset-Directory $localDeploymentSettings.DeploymentPath
	Copy-Item "$applicationPath\*" $localDeploymentSettings.DeploymentPath -Recurse
	
	Write-Notification "Setting up application pool"
	Create-AppPool $localDeploymentSettings.AppPool
	
	Write-Notification "Creating web site"
	Create-WebSite $localDeploymentSettings.WebSiteName $localDeploymentSettings.DeploymentPath $localDeploymentSettings.Port
}
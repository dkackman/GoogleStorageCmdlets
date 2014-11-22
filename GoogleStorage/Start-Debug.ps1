<#
This script will run on debug.
It will load in a PowerShell command shell and import the module developed in the project. To end debug, exit this shell.
#>

# Write a reminder on how to end debugging.
$message = "| Exit this shell to end the debug session! |"
$line = "-" * $message.Length
$color = "Cyan"
Write-Host -ForegroundColor $color $line
Write-Host -ForegroundColor $color $message
Write-Host -ForegroundColor $color $line
Write-Host 

# Load the module.
#$env:PSModulePath = ".;$env:PSModulePath"
$env:PSModulePath = (Resolve-Path .).Path + ";" + $env:PSModulePath
Import-Module GoogleStorage 
#Set-GoogleStorageConfig 930617506804-n1qur7rdr0o715k7igeivmr779smdn45.apps.googleusercontent.com (convertto-securestring -string uiJO5Zz__vQ0nDVlQCD5jn7B -asplaintext -force) poop
Get-GoogleStorageConfig
#Get-GoogleStorageBucket -bucket uspto-pair -NoAuth -ListContents
# Set-GoogleStorageProject poop
# Happy debugging :-)


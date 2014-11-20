param(
	[Parameter()] $ProjectName,
	[Parameter()] $ConfigurationName,
	[Parameter()] $TargetDir
)

Copy *.dll .\GoogleStorage -Force -Verbose

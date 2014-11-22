param(
	[Parameter()] $ProjectName,
	[Parameter()] $ConfigurationName,
	[Parameter()] $TargetDir
)

Copy GoogleStorage.dll .\GoogleStorage -Force -Verbose
Copy Newtonsoft.Json.dll .\GoogleStorage -Force -Verbose
Copy DynamicRestProxy.PortableHttpClient.dll .\GoogleStorage -Force -Verbose
Copy System.Net.Http.Primitives.dll .\GoogleStorage -Force -Verbose
Copy System.Net.Http.Extensions.dll .\GoogleStorage -Force -Verbose

Copy *.pdb .\GoogleStorage -Force -Verbose

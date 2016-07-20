param([Parameter(Mandatory=$true)][string]$version) 

# Update All AssemblyInfo file versions
$z29 = "./ExternalTools/ZeroToNine/Zero29.exe"
&$z29 -a $version
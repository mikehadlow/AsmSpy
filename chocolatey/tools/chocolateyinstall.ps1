$ErrorActionPreference = 'Stop';


$packageName = 'AsmSpy'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url = 'http://static.mikehadlow.com/AsmSpy.zip' 

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  fileType      = 'EXE' 
  url           = $url
  checksum      = '53d3e96213d8f453bc1b5cdbbfb39c78942efef4'
  checksumType  = 'sha1' 
}

Install-ChocolateyZipPackage @packageArgs

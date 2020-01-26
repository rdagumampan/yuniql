$version = '{version}'
$packageName = 'yuniql'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url = "https://github.com/rdagumampan/yuniql/releases/download/latest/yuniql-cli-linux-x64-latest.zip"
#$checksumType = 'sha256'
#$checksum = '{checksum}'

Install-ChocolateyZipPackage $packageName $url $toolsDir
Install-BinFile "yuniql" "$toolsDir\yuniql-$version\yuniql.exe"
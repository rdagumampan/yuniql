$packageName = 'yuniql'
$version = '{version}'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"    

$packageArgs = @{
    $packageName    = $packageName
    $url            = "https://github.com/rdagumampan/yuniql/releases/download/latest/yuniql-cli-linux-x64-latest.zip"
    $unzipLocation  =  $toolsDir
}

Install-ChocolateyZipPackage $packageArgs
#https://github.com/chocolatey/choco/blob/master/src/chocolatey.resources/helpers/functions/Install-ChocolateyZipPackage.ps1

$packageArgs = @{
    $name   = $packageName
    $path   = "$toolsDir\$packageName-$version\yuniql.exe"
}

Install-BinFile $packageArgs
#https://github.com/chocolatey/choco/blob/master/src/chocolatey.resources/helpers/functions/Install-BinFile.ps1
$packageName = $env:ChocolateyPackageName
$version = $env:ChocolateyPackageVersion
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"    

Write-Host "Installing $packageName v$version"
Write-Host "toolsDir: $toolsDir"

$packageArgs = @{
    packageName     = $packageName
    fileType        = 'zip'    
    url             = 'https://github.com/rdagumampan/yuniql/releases/download/latest/yuniql-cli-win-x64-latest.zip'
    checksum        = '9F8011E76CD9BBC9928E27EAD7D22DA24D1D0DC7D060083F49B5166CB583207D'
    checksumType    = 'sha256'
    unzipLocation   =  $toolsDir
}

Write-Host $packageArgs.packageName
Write-Host $packageArgs.fileType
Write-Host $packageArgs.url
Write-Host $packageArgs.checksum
Write-Host $packageArgs.checksumType
Write-Host $packageArgs.unzipLocation

Install-ChocolateyZipPackage `
  -PackageName $packageArgs.packageName `
  -Url $packageArgs.url `
  -UnzipLocation $packageArgs.unzipLocation `
  -Checksum $packageArgs.checksum `
  -ChecksumType $packageArgs.checksumType

#Install-ChocolateyZipPackage $packageArgs
#https://github.com/chocolatey/choco/blob/master/src/chocolatey.resources/helpers/functions/Install-ChocolateyZipPackage.ps1
#https://github.com/chocolatey/choco/wiki/HelpersInstallChocolateyZipPackage

$installPath = "$toolsDir\$packageName-$version\yuniql.exe"
Write-Host "installPath: $installPath"

$packageArgs = @{
    name    = $packageName
    path    = $installPath
}

Write-Host $packageArgs.name
Write-Host $packageArgs.path

Install-BinFile $packageArgs
  -Name $packageArgs.packageName `
  -Path $packageArgs.path

#Install-BinFile $packageArgs
#https://github.com/chocolatey/choco/blob/master/src/chocolatey.resources/helpers/functions/Install-BinFile.ps1
#https://github.com/chocolatey/choco/wiki/HelpersInstallBinFile

$packageName = $env:ChocolateyPackageName
$version = $env:ChocolateyPackageVersion
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"    

# Write-Host "Installing $packageName v$version"
# Write-Host "toolsDir: $toolsDir"

$packageArgs = @{
    packageName     = $packageName
    url             = 'https://github.com/rdagumampan/yuniql/releases/download/latest/yuniql-cli-win-x64-latest.zip'
    unzipLocation   =  $toolsDir
    checksum        = '49A596B99873CAE6B94852177B0D8F7F020607800A9A26FDB28C1EE3A21227B3'
    checksumType    = 'sha256'
}

# Write-Host $packageArgs.packageName
# Write-Host $packageArgs.fileType
# Write-Host $packageArgs.url
# Write-Host $packageArgs.checksum
# Write-Host $packageArgs.checksumType
# Write-Host $packageArgs.unzipLocation

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
# Write-Host "installPath: $installPath"

$packageArgs = @{
    name    = $packageName
    path    = $installPath
}

# Write-Host $packageArgs.name
# Write-Host $packageArgs.path

Install-BinFile `
  -Name $packageArgs.name `
  -Path $packageArgs.path

#Install-BinFile $packageArgs
#https://github.com/chocolatey/choco/blob/master/src/chocolatey.resources/helpers/functions/Install-BinFile.ps1
#https://github.com/chocolatey/choco/wiki/HelpersInstallBinFile

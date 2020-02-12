$packageName = $env:ChocolateyPackageName
$version = $env:ChocolateyPackageVersion
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$packageUrl = "https://github.com/rdagumampan/yuniql/releases/download/v$version/yuniql-cli-win-x64-v$version.zip"
$packageArgs = @{
    packageName     = $packageName
    url             = $packageUrl
    unzipLocation   =  $toolsDir
    checksum        = '49A596B99873CAE6B94852177B0D8F7F020607800A9A26FDB28C1EE3A21227B3'
    checksumType    = 'sha256'
}

Install-ChocolateyZipPackage `
  -PackageName $packageArgs.packageName `
  -Url $packageArgs.url `
  -UnzipLocation $packageArgs.unzipLocation `
  -Checksum $packageArgs.checksum `
  -ChecksumType $packageArgs.checksumType

$installPath = "$toolsDir\$packageName-$version\yuniql.exe"
$packageArgs = @{
    name    = $packageName
    path    = $installPath
}

Install-BinFile `
  -Name $packageArgs.name `
  -Path $packageArgs.path
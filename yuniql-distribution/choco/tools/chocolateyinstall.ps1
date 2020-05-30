$packageName = $env:ChocolateyPackageName
$version = $env:ChocolateyPackageVersion
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$packageUrl = "https://github.com/rdagumampan/yuniql/releases/download/v$version/yuniql-cli-win-x64-v$version.zip"
$packageArgs = @{
    packageName     = $packageName
    url             = $packageUrl
    unzipLocation   =  $toolsDir
    checksum        = 'DCF8E0FE8A5568311A9FE611101BE215458E4409ACF5FF340ABCC478D8EE11D6'
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
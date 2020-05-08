$packageName = $env:ChocolateyPackageName
$version = $env:ChocolateyPackageVersion
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$packageUrl = "https://github.com/rdagumampan/yuniql/releases/download/v$version/yuniql-cli-win-x64-v$version.zip"
$packageArgs = @{
    packageName     = $packageName
    url             = $packageUrl
    unzipLocation   =  $toolsDir
    checksum        = '7976CB66DEAAE62619D6319333ABEC640418E7CD2EBBD7E17B89C24A70F1BCC7'
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
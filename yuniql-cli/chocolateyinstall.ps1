$ErrorActionPreference = 'Stop'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
Install-Binfile -Name yuniql -Path "$defaultDotnetRuntimePath" -Command "$toolsDir\yuniql.exe"
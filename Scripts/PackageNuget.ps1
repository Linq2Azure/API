$nugetRelativePath = ".nuget\nuget.exe"
$parentDirectory = split-path $PSScriptRoot -parent

$coreNuspecPath = join-path $parentDirectory "Linq2Azure.nuspec"

$linq2AzureDllPath = join-path $parentDirectory "bin\Release\Linq2Azure.dll"
$linq2AzurePdbPath = join-path $parentDirectory "bin\Release\Linq2Azure.pdb"
$linq2AzureXmlPath = join-path $parentDirectory "bin\Release\Linq2Azure.xml"

$packagePath = join-path $parentDirectory -childpath "createpackage"
$outputDirectory = join-path $packagePath "output"

$corePackagePath = join-path $packagePath -childpath "core"
$coreNet45LibPath = join-path $corePackagePath -childpath "lib\net45"

if (Test-Path -Path $packagePath){
    Remove-Item -Recurse -Force $packagePath
}

New-Item -ItemType directory -Path $packagePath | Out-Null
New-Item -ItemType directory -Path $outputDirectory | Out-Null

New-Item -ItemType directory -Path $coreNet45LibPath | Out-Null
Copy-Item $coreNuspecPath $corePackagePath
Copy-Item  $linq2AzureDllPath $coreNet45LibPath
Copy-Item  $linq2AzurePdbPath $coreNet45LibPath
Copy-Item  $linq2AzureXmlPath $coreNet45LibPath

Push-Location -Path $corePackagePath

& '..\..\.nuget\nuget.exe' Pack "Linq2Azure.nuspec"
Copy-Item "*.nupkg" $outputDirectory

Pop-Location

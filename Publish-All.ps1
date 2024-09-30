$publishPath = "M:\PointlessWaymarksPublications\PiDarkPhotosProject"
if(!(test-path -PathType container $publishPath)) { New-Item -ItemType Directory -Path $publishPath }

Remove-Item -Path $publishPath\* -Recurse

dotnet publish .\PiDarkPhotos\PiDarkPhotos.csproj /p:PublishProfile=.\VibrationMonitor\Properties\PublishProfiles\FolderProfile.pubxml

$publishVersion = (Get-Date).ToString("yyyy-MM-dd-HH-mm")
$destinationZipFile = "M:\PointlessWaymarksPublications\PiDarkPhotosProject-Zip--{0}.zip" -f $publishVersion

Compress-Archive -Path M:\PointlessWaymarksPublications\PiDarkPhotosProject -DestinationPath $destinationZipFile

Write-Output "PiDarkPhotosProject zipped to '$destinationZipFile'"

if ($lastexitcode -ne 0) {throw ("Exec: " + $errorMessage) }
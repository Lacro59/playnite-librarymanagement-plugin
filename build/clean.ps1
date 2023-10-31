param(
	[string]$OutDir
)

$strarry = @(
	"CommonPluginsResources.dll", 
	"CroppingImageLibrary.dll",
	"LazZiya.ImageResize.dll",
	"LibraryManagement.dll",
	"MoreLinq.dll",
	"System.Drawing.Common.dll")

$files = get-childitem (Join-Path $OutDir "*.dll")
foreach ($file in $files) {
	if($file.Name -in $strarry) {
	}
	else {
		Remove-Item $file.Name
	}
}

$files = Get-ChildItem -Path $PSScriptRoot -Filter *.xsd;

$schemas = "xsd";
foreach ($f in $files){
    if($f.Name -notmatch 'rttiPPTSchema_v1[1234567].xsd' && $f.Name -notmatch 'rttiCTTReferenceSchema_v3.xsd')
    {
        $schemas = $schemas + " " +  $f.Name; 
    }
}
 

# Add dummy schema so output class file called schema.cs
# See https://stackoverflow.com/a/33906829/3805124
# " .\dummy\schema.xsd /n:DarwinClient.Schema"
$schemas = $schemas + " .\dummy\schema.xsd /c /n:DarwinClient.Schema";

Write-Output $schemas;
Invoke-Expression $schemas;
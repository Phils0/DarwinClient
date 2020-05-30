$files = Get-ChildItem -Path C:\Users\phils\source\DarwinClient\schemas -Filter *.xsd;

$schemas = "xsd"
foreach ($f in $files){
    if($f.Name -notmatch 'rttiPPTSchema_v1[12345].xsd')
    {
        $schemas = $schemas + " " +  $f.Name; 
    }
}
 

# Add dummy schema so output class file called schemaV16.cs
# See https://stackoverflow.com/a/33906829/3805124
$schemas = $schemas + " .\dummy\schemaV16.xsd /c /n:DarwinClient.SchemaV16";

Write-Output $schemas;
Invoke-Expression $schemas;
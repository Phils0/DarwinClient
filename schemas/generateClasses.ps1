$files = Get-ChildItem -Path C:\Users\phils\source\DarwinClient\schemas -Filter *.xsd;

$schemas = "xsd"
foreach ($f in $files){
    $schemas = $schemas + " " +  $f.Name; 
}

$schemas = $schemas + " .\dummy\schemaV16.xsd /c /n:DarwinClient.SchemaV16";

Write-Output $schemas;
Invoke-Expression $schemas;
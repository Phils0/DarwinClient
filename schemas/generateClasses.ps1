$files = Get-ChildItem -Path $PSScriptRoot -Filter *.xsd;

$schemas = "xsd";
foreach ($f in $files){
    $isOldSchema = $f.Name -notmatch 'rttiPPTSchema_v1[1234567].xsd';
    $isNotTimetableSchema = $f.Name -notmatch 'rttiCTTSchema_v\d.xsd';
    $isNotRefSchema = $f.Name -notmatch 'rttiCTTReferenceSchema_v\d.xsd';
    if( $isOldSchema -and $isNotRefSchema -and $isNotTimetableSchema)
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

# timetable reference schema
$referenceSchema = "xsd rttiCTTReferenceSchema_v4.xsd .\dummy\referenceSchema.xsd /c /n:DarwinClient.Schema.TimetableReference";
Write-Output $referenceSchema;
Invoke-Expression $referenceSchema;

# timetable schema
# Note as we generate the timetable schema separately we need to include the common types 
# this means we have duplicate classes for the common types, in DarwinClient.Schema and DarwinClient.Schema.Timetable, 
# TODO look to avoid this duplication in the future, but for now it is easier to maintain the separate timetable schema
$timetableSchema = "xsd rttiPPTCommonTypes_v1.xsd rttiCTTSchema_v8.xsd rttiCTTReferenceSchema_v4.xsd .\dummy\timetableSchema.xsd /c /n:DarwinClient.Schema.Timetable";
Write-Output $timetableSchema;
Invoke-Expression $timetableSchema;
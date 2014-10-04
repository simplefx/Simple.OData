call nuget pack -sym Simple.OData.Client.nuspec -Symbols -Version %1 -OutputDirectory Packages
call nuget pack -sym Simple.OData.V3.Client.nuspec -Symbols -Version %1 -OutputDirectory Packages
call nuget pack -sym Simple.OData.V4.Client.nuspec -Symbols -Version %1 -OutputDirectory Packages

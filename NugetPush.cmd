call nuget push build\packages\Simple.OData.Client.%1.nupkg    -Source https://api.nuget.org/v3/index.json -apikey %2
call nuget push build\packages\Simple.OData.V3.Client.%1.nupkg -Source https://api.nuget.org/v3/index.json -apikey %2
call nuget push build\packages\Simple.OData.V4.Client.%1.nupkg -Source https://api.nuget.org/v3/index.json -apikey %2
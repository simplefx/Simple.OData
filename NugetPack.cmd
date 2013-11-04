cd .\%1
call ..\.nuget\nuget pack -sym %1.csproj -Version %2 -Symbols -Properties Configuration=Release
cd ..\

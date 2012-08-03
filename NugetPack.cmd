cd .\%1
call nuget pack -sym %1.csproj -Properties Configuration=Release
cd ..\

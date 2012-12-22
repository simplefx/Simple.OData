cd .\%1
call ..\.nuget\nuget push %1.%2.nupkg %3
cd ..\

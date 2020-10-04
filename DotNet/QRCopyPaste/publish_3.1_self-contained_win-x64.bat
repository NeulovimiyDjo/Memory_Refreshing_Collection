rm -r ./bin
dotnet publish -c Release --force -f netcoreapp3.1 --self-contained true -r win-x64 -o ./bin .
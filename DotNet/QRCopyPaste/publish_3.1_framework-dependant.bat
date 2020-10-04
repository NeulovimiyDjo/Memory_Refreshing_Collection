rm -r ./bin
dotnet publish -c Release --force -f netcoreapp3.1 --self-contained false -o ./bin .
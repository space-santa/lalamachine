$projects = @("LibLala")

$projects | ForEach-Object {
    dotnet build "$($_)/$($_).csproj"
    dotnet test "$($_).Unittests/$($_).Unittests.csproj"
}
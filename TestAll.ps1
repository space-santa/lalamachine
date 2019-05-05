$projects = @("LibLala", "MusicScanner")

$projects | ForEach-Object {
    dotnet build "$($_)/$($_).csproj"
    dotnet test "$($_).Unittests/$($_).Unittests.csproj"
}
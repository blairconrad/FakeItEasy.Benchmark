if (!$env:R_HOME) {
    $env:R_HOME = 'C:\Program Files\R\R-3.5.1'
}

"$env:USERPROFILE\.nuget\packages\FakeItEasy\$($args[-1])", ".\src\FakeItEasy.Benchmark\obj", ".\src\FakeItEasy.Benchmark\bin" | 
    Where-Object { Test-Path $_ } | 
    ForEach-Object { Remove-Item -Recurse $_ }

# Remove-Item -Recurse -Force BenchmarkDotNet.Artifacts* 

dotnet run --configuration Release --project .\src\FakeItEasy.Benchmark\FakeItEasy.Benchmark.csproj -- $args

$report = (Get-ChildItem .\BenchmarkDotNet.Artifacts\results\*-report.csv | Sort-Object -Property LastWriteTime)[-1].FullName

.\plot.py $report ($args | Where-Object { ! $_.StartsWith("--")})
& ($report.Replace("-report.csv", "-report.png"))
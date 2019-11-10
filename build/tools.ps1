$OrigBgColor = $host.ui.rawui.BackgroundColor
$OrigFgColor = $host.ui.rawui.ForegroundColor

Function Reset-Colors {
    $host.ui.rawui.BackgroundColor = $OrigBgColor
    $host.ui.rawui.ForegroundColor = $OrigFgColor
}

Function Clear-Artifacts {
    [CmdletBinding()]
    param()
    if(Test-Path $Artifacts) {
        Write-Host
        Write-Host "Cleaning the artifacts folder: $Artifacts"
        Remove-Item $Artifacts\* -Recurse -Force
    }
}


Function Run-Build {
    [CmdletBinding()]
    param()

    Write-Host
    Write-Host "Building solution: $SolutionFile"
    Write-Host "MSBuild arguments: $MSBuildArguments"
    Write-Host

    MSBuild `
        $SolutionFile `
        @MSBuildArguments
}
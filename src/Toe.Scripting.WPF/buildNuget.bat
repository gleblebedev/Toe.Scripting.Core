msbuild.exe /property:Platform=AnyCPU /property:Configuration=Release  Toe.Scripting.WPF.csproj
..\packages\NuGet.CommandLine.4.7.0\tools\NuGet.exe pack Toe.Scripting.WPF.csproj.nuspec -Prop Configuration=Release -BasePath .\ -OutputDirectory ..\nuget

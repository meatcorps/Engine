rm -f ./project_export.zip
zip -r project_export.zip . -i '*.cs' '*.fx' '*.csproj' '*.sln' -x '*/bin/*' '*/obj/*'

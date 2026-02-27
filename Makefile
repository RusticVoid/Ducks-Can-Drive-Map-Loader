all:
	rm -rf bin obj
	dotnet build -c Release
	cp ./bin/Release/net472/DCDMapLoader.dll ./build
	cp ./bin/Release/net472/DCDMapLoader.dll "$(HOME)/.local/share/Steam/steamapps/common/Ducks Can Drive/Mods/"
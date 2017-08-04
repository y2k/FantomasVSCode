FROM mono:latest

ARG source=.
WORKDIR /app
COPY $source .

RUN nuget restore && msbuild /p:Configuration=Release

ENTRYPOINT [ "mono", "bin/Release/FantomasServer.exe" ]
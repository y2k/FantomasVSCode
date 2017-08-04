FROM mono:latest

EXPOSE 8080

ARG source=.
WORKDIR /app
COPY $source .

RUN nuget restore && msbuild /p:Configuration=Release

ENTRYPOINT [ "mono", "bin/Release/FantomasServer.exe" ]
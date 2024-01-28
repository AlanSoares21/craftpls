FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
COPY AlternativeMkt/Core /code/AlternativeMkt/Core
COPY AlternativeMkt.Db /code/AlternativeMkt.Db
WORKDIR /code/AlternativeMkt/Core
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /code/AlternativeMkt/Core/out .
EXPOSE 80
ENTRYPOINT [ "dotnet", "AlternativeMkt.dll" ]/

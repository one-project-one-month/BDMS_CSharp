FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BDMS.Shared/ BDMS.Shared/
COPY BDMS.Database/ BDMS.Database/
COPY BDMS.Domain/ BDMS.Domain/
COPY BDMS.Api/ BDMS.Api/
RUN dotnet restore BDMS.Api/BDMS.Api.csproj
RUN dotnet publish BDMS.Api/BDMS.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "BDMS.Api.dll"]

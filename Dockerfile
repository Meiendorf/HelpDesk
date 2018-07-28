FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 3724
EXPOSE 44326

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY HelpDesk.csproj .
RUN dotnet restore HelpDesk.csproj
COPY . .
WORKDIR /src
RUN dotnet build HelpDesk.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish HelpDesk.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HelpDesk.dll"]

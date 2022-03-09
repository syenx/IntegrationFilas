FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-bionic AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-bionic AS build
WORKDIR /src
COPY ["EDM.Infohub.Integration.csproj", ""]
RUN dotnet restore "./EDM.Infohub.Integration.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "EDM.Infohub.Integration.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EDM.Infohub.Integration.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt update
ENV TZ=America/Sao_Paulo
RUN echo $TZ > /etc/timezone
RUN apt-get install tzdata
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime

ENTRYPOINT ["dotnet", "EDM.Infohub.Integration.dll"]
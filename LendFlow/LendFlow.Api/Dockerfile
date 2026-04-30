FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy solution and project files (context is the LendFlow/ folder)
COPY LendFlow.slnx ./
COPY LendFlow.Application/LendFlow.Application.csproj ./LendFlow.Application/
COPY LendFlow.Domain/LendFlow.Domain.csproj ./LendFlow.Domain/
COPY LendFlow.Infrastructure/LendFlow.Infrastructure.csproj ./LendFlow.Infrastructure/
COPY LendFlow.Api/LendFlow.Api.csproj ./LendFlow.Api/

# restore
WORKDIR /src/LendFlow.Api
RUN dotnet restore "LendFlow.Api.csproj"

# copy source and build
WORKDIR /src
COPY . .

WORKDIR /src/LendFlow.Api
RUN dotnet build "LendFlow.Api.csproj" -c Release -o /app/build

# publish
FROM build AS publish
WORKDIR /src/LendFlow.Api
RUN dotnet publish "LendFlow.Api.csproj" -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5147
ENV ConnectionStrings__DefaultConnection="Server=sql,1433;Database=LendFlow;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True;"
ENV ConnectionStrings__Redis="redis:6379"

EXPOSE 5147
ENTRYPOINT ["dotnet", "LendFlow.Api.dll"]

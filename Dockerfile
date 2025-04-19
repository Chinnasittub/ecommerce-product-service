FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ✅ คัดลอก shared-library มาไว้ใน Docker context
COPY shared-library ./shared-library
COPY product-service/. .

RUN dotnet restore "ProductService.csproj"

# 🔥 สำคัญ: ลบไฟล์ AssemblyInfo อัตโนมัติ ที่ dotnet สร้างให้
RUN rm -rf ./obj ./shared-library/obj

RUN dotnet publish "ProductService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"]

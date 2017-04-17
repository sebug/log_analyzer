FROM microsoft/aspnetcore:1.1
ENV LogSource:Directory /mnt/logs
ENTRYPOINT ["dotnet", "log_analyzer.dll"]
ARG SOURCE=.
WORKDIR /app
EXPOSE 80
COPY $source .

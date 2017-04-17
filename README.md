### Log Analyzer
Exploration in ASP.NET core to analyze performance counters from log entries. Based on this blog entry: https://www.hanselman.com/blog/ExploringASPNETCoreWithDockerInBothLinuxAndWindowsContainers.aspx

To run:

	env "LogSource:Directory=/Users/sgfeller/log_exploration/data" dotnet run

or in Docker:

	dotnet publish
	cp Dockerfile bin/Debug/netcoreapp1.1/publish
	docker build bin/Debug/netcoreapp1.1/publish -t loganalyzer
	docker run -v yourlogdirectory:/mnt/logs -it -d -p 4999:80 loganalyzer

After that, you can poll the log entries:

	curl http://localhost:4999/api/LogEntry?start=5


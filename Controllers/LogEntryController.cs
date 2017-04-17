using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log_analyzer.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace log_analyzer.Controllers
{
	[Route("api/[controller]")]
	public class LogEntryController : Controller
	{
		private readonly IOptions<LogSource> _optionsAccessor;
		private readonly IFileProvider _fileProvider;

		public LogEntryController(IOptions<LogSource> optionsAccessor)
		{
			this._optionsAccessor = optionsAccessor;
			this._fileProvider = new PhysicalFileProvider(_optionsAccessor.Value.Directory);
		}

		public async Task<string> GetContent(Stream readStream)
		{
			string ct = await new StreamReader(readStream).ReadToEndAsync();

			readStream.Dispose();

			return ct;
		}

		public IEnumerable<Task<string>> GetLogFileContents(IDirectoryContents directoryContents)
		{
			foreach (var entry in directoryContents)
			{
				if (entry.IsDirectory)
				{
					var subItems = _fileProvider.GetDirectoryContents(entry.Name);
					IEnumerable<Task<string>> subTasks = GetLogFileContents(subItems);

					foreach (var st in subTasks)
					{
						yield return st;
					}
				}
				else
				{
					yield return GetContent(entry.CreateReadStream());
				}
			}
		}

		[HttpGet]
		public async Task<IEnumerable<string>> Get()
		{
			var tasks = GetLogFileContents(_fileProvider.GetDirectoryContents(""));

			return await Task.WhenAll(tasks.Take(10));
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log_analyzer.Configuration;
using log_analyzer.Models;
using log_analyzer.Parsing;
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

		protected IEnumerable<IFileInfo> GetLogEntries(string basePath)
		{
			var fileEntries = this._fileProvider.GetDirectoryContents(basePath);
			foreach (var fileEntry in fileEntries)
			{
				if (fileEntry.IsDirectory)
				{
					var subItems = GetLogEntries(fileEntry.Name);
					foreach (var si in subItems)
					{
						yield return si;
					}
				}
				else
				{
					yield return fileEntry;
				}
			}
		}

		public async Task<IEnumerable<LogEntryModel>> GetContent(Stream readStream)
		{
			var items = await new LogEntryParser().Parse(readStream);
			readStream.Dispose();

			return items;
		}

		[HttpGet]
		public async Task<IEnumerable<LogEntryModel>> Get(int start = 0, int size = 1)
		{
			var logItems = GetLogEntries("");

			var tasks = logItems.OrderBy(fi => fi.Name).Select(li => GetContent(li.CreateReadStream()));


			var result = await Task.WhenAll(tasks.Skip(start).Take(size));

			return result.SelectMany(items => items);
		}
	}
}
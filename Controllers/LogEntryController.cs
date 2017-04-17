using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log_analyzer.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace log_analyzer.Controllers
{
	[Route("api/[controller]")]
	public class LogEntryController : Controller
	{
		private readonly IOptions<LogSource> _optionsAccessor;

		public LogEntryController(IOptions<LogSource> optionsAccessor)
		{
			this._optionsAccessor = optionsAccessor;
		}

		[HttpGet]
		public string Get()
		{
			Console.Error.WriteLine("Options accessor: " + _optionsAccessor.Value.Directory);
			return "Oh";
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log_analyzer.Models;

namespace log_analyzer.Parsing
{
	public class LogEntryParser
	{
		public const string SEPARATOR = "----------------------------------------";
		public const string FORMAT = "M/d/yyyy h:mm:ss tt";

		internal DateTime? ParseTimeStamp(string v)
		{
			DateTime r;
			if (DateTime.TryParseExact(v, FORMAT, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out r))
			{
				return r;
			}

			return (DateTime?)null;
		}

		internal int ParsePriority(string v)
		{
			int r;
			if (int.TryParse(v, out r))
			{
				return r;
			}
			return 0;
		}

		public async Task<IEnumerable<LogEntryModel>> Parse(Stream s)
		{
			List<LogEntryModel> result = new List<LogEntryModel>();
			LogEntryModel currentEntry = new LogEntryModel();
			bool entryHasData = false;
			string lastKey = null;
			StringBuilder lastValueSb = null;
			try
			{
				using (StreamReader sr = new StreamReader(s))
				{
					string line = await sr.ReadLineAsync();
					while (line != null)
					{
						if (line == SEPARATOR)
						{
							if (entryHasData)
							{
								PopulateCurrentEntry(currentEntry, entryHasData, lastKey, lastValueSb);
								result = result.Union(Enumerable.Repeat(currentEntry, 1)).ToList();
								entryHasData = false;
								currentEntry = new LogEntryModel();
								lastKey = null;
							}
						}
						if (line.StartsWith(" ", StringComparison.Ordinal) || line.StartsWith("<", StringComparison.Ordinal))
						{
							// Line continuation
							lastValueSb.AppendLine(line.TrimStart(' '));
						}
						else
						{
							entryHasData = PopulateCurrentEntry(currentEntry, entryHasData, lastKey, lastValueSb);

							int colonIdx = line.IndexOf(':');
							if (colonIdx >= 0)
							{
								// parse key-value
								lastKey = line.Substring(0, colonIdx);
								lastValueSb = new StringBuilder();
								lastValueSb.AppendLine(line.Substring(colonIdx + 1).TrimStart(' '));
							}
						}
						line = await sr.ReadLineAsync();
					}
				}

				if (entryHasData)
				{
					result.Append(currentEntry);
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				throw;
			}

			return result;
		}

		private bool PopulateCurrentEntry(LogEntryModel currentEntry, bool entryHasData, string lastKey, StringBuilder lastValueSb)
		{
			if (!String.IsNullOrEmpty(lastKey))
			{
				switch (lastKey)
				{
					case "Timestamp":
						currentEntry.Timestamp = ParseTimeStamp(lastValueSb.ToString());
						entryHasData = true;
						break;
					case "Message":
						currentEntry.Message = lastValueSb.ToString();
						entryHasData = true;
						break;
					case "Category":
						currentEntry.Category = lastValueSb.ToString();
						entryHasData = true;
						break;
					case "Priority":
						currentEntry.Priority = ParsePriority(lastValueSb.ToString());
						entryHasData = true;
						break;
					case "EventId":
						currentEntry.EventId = lastValueSb.ToString();
						entryHasData = true;
						break;
					case "Severity":
						currentEntry.Severity = lastValueSb.ToString();
						entryHasData = true;
						break;
					case "Title":
						currentEntry.Title = lastValueSb.ToString();
						break;

				}
			}

			return entryHasData;
		}
	}
}

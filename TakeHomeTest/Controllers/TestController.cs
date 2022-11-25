using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace TakeHomeTest.Controllers
{
	public class TestController : ApiController
    {
		[Route("ConvertCsvText")]
		public async Task<string> Post()
		{
			//Read raw text from Body
			string csv = await Request.Content.ReadAsStringAsync();

			//Attemp to validate possible characters that could complicate the formatting
			if (string.IsNullOrEmpty(csv))
				throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"{nameof(csv)} is required"));
			if (csv.Count(x => x == '"') % 2 != 0 || csv.Contains("[") || csv.Contains("]"))
				throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"{nameof(csv)} has invalid format"));

			StringBuilder builder = new StringBuilder(), responseBuilder = new StringBuilder();
			List<string> list = new List<string>();
			bool createNew = true;
			bool hasQuotes = false;

			Action endFormating = () =>
			{
				builder.Append("]");
				createNew = true;
				hasQuotes = false;
				if (builder.ToString() == "[]" && list.LastOrDefault() != "[]")//Avoid empty commas
					return;
				list.Add(builder.ToString());
			};

			Action initFormating = () =>
			{
				builder.Clear();
				createNew = false;
				builder.Append("[");
			};

			foreach (string ln in csv.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				list.Clear();
				builder.Clear();
				createNew = true;
				hasQuotes = false;
				for (int i = 0; i < ln.Length; i++)
				{
					if (createNew)
						initFormating();

					if (ln[i] == '"')
					{
						hasQuotes = true;
						if (builder.Length != 1)
							endFormating();
						continue;
					}
					else
					{
						if (hasQuotes)
							builder.Append(ln[i]);
						else
						{
							if (ln[i] != ',')
								builder.Append(ln[i]);
							
							if (i + 1 == ln.Length || (ln[i] == ','))
								endFormating();
						}
					}
				}
				responseBuilder.AppendLine(string.Join(string.Empty, list));
			}

			return responseBuilder.ToString();
		}
    }
}

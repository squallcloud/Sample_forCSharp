using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sample_JsonParse
{
	class Program
	{
		static void Main(string[] args)
		{
			try {
				string outputFilePath = "./hoge.json";

				var list = new List<JObject>();
				var obj = new JObject();
				obj.Add("hoge", new JValue(100));
				obj.Add("foo", new JValue(1.11));
				obj.Add("piyo", new JValue("いやっほい"));
				list.Add(obj);
				list.Add(obj);

				var json = new JArray(list.ToArray());
				var enc = new System.Text.UTF8Encoding(false);
				using (var sw = new StreamWriter(outputFilePath, false, enc)) {
					using (var tw = new JsonTextWriter(sw)) {
						tw.Formatting = Formatting.Indented;
						json.WriteTo(tw);
					}
				}

			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			} finally {

			}
		}
	}
}

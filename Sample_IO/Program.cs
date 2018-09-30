using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_IO
{
	class Program
	{
		private static void SearchFile(string rootPath, Func<string, bool> func)
		{
			try {
				var files = Directory.GetFiles(rootPath);
				foreach (var file in files) {
					func(file);
				}
			} catch (UnauthorizedAccessException ex) {
				Console.Error.WriteLine(ex.Message);
			} finally {
			}

			try {
				var dirs = Directory.GetDirectories(rootPath);
				foreach (var dir in dirs) {
					SearchFile(dir, func);
				}
			} catch (UnauthorizedAccessException ex) {
				Console.Error.WriteLine(ex.Message);
			} finally {
			}
		}

		static void Main(string[] args)
		{
			try {
				string rootPath = "C:/";
				SearchFile(rootPath, x => {
					Console.WriteLine(x);
					return true;
				});
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			} finally {

			}
		}
	}
}

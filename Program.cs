using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Linq;

namespace NextDoc.Utilities
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Welcome to PDF Command Utilities.");

			if(args.Length == 0)
			{
				Console.WriteLine("No argument. Use [merge] or [countpdfpages] parameters.");
				Console.WriteLine("");
				return;
			}

			switch (args[0].ToLower())
			{
				case "merge":
					Console.WriteLine("Starting document merge....\r\n");
					//NextDoc.Utilities.exe merge /origem:D:\ArquivosSaida\CCBC\442017
					Merge(args);
					break;

				case "countpdfpages":
					Console.WriteLine("Counting pages of the document....\r\n");
					//NextDoc.Utilities.exe countpdfpages /origem:D:\ArquivosSaida\CCBC\442017
					CountPages(args);
					break;

				default:
					CommandNotRecognized();
					break;
			}
		}

		private static void CommandNotRecognized()
		{
			Console.WriteLine("Command not recognized.");
		}

		private static void CountPages(string[] argumentos)
		{
			string destino = argumentos[1].Split("/origem:", StringSplitOptions.RemoveEmptyEntries)[0].Replace("/origem:", "");
			string[] arquivos = Directory.GetFiles(destino, "*.pdf", SearchOption.AllDirectories);

			int totalPaginas = 0;
			PdfReader reader;
			for (int i = 0; i < arquivos.Length; i++)
			{
				try
				{
					reader = new PdfReader(arquivos[i]);
					int n = reader.NumberOfPages;
					totalPaginas += n;

					Console.WriteLine($" {n} páginas do arquivo: {arquivos[i]}.");
				}
				finally
				{
					totalPaginas += 1;
				}
			}
			Console.WriteLine($"Total de Páginas dos arquivos: {totalPaginas}");
		}

		public static void Merge(string[] argumentos)
		{
			string target = argumentos[1].Split("/origin:", StringSplitOptions.RemoveEmptyEntries)[0].Replace("/origin:", "");
			string[] folders = Directory.GetDirectories(target, "*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < folders.Length; i++)
			{
				DirectoryInfo info = new DirectoryInfo(folders[i]);
				FileInfo[] files = info.GetFiles("*.pdf").OrderBy(p => p.CreationTime).ToArray();

				MergeFiles(target + $"\\{info.Name}.pdf", files);
			}
		}

		private static void MergeFiles(string destinationFile, FileInfo[] files)
		{
			string[] sourceFiles = files.Select(m => m.FullName).ToArray();
			try
			{
				int f = 0;
				// we create a reader for a certain document
				PdfReader reader = new PdfReader(sourceFiles[f]);
				// we retrieve the total number of pages
				int n = reader.NumberOfPages;
				//Console.WriteLine("There are " + n + " pages in the original file.");

				// step 1: creation of a document-object
				Document document = new Document(reader.GetPageSizeWithRotation(1));

				// step 2: we create a writer that listens to the document
				PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(destinationFile, FileMode.Create));

				// step 3: we open the document
				document.Open();

				PdfContentByte cb = writer.DirectContent;
				PdfImportedPage page;
				int rotation;

				// step 4: we add content
				while (f < sourceFiles.Length)
				{
					int i = 0;
					while (i < n)
					{
						i++;
						document.SetPageSize(reader.GetPageSizeWithRotation(i));
						document.NewPage();
						page = writer.GetImportedPage(reader, i);
						rotation = reader.GetPageRotation(i);
						if (rotation == 90 || rotation == 270)
						{
							cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
						}
						else
						{
							cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
						}
						//Console.WriteLine("Processed page " + i);
					}
					f++;
					if (f < sourceFiles.Length)
					{
						reader = new PdfReader(sourceFiles[f]);
						// we retrieve the total number of pages
						n = reader.NumberOfPages;
						//Console.WriteLine("There are " + n + " pages in the original file.");
					}
				}

				// step 5: we close the document
				document.Close();
			}
			catch (Exception e)
			{
				string strOb = e.Message;
			}
		}
	}
}
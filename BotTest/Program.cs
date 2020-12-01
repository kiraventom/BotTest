using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace BotTest
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var botClient = new TelegramBotClient("INSERT_TOKEN_HERE");
			int offset = 0; // про оффсет и работу с update читать тут https://core.telegram.org/bots/api#update
			while (true)
			{
				var updates = await botClient.GetUpdatesAsync(offset);
				foreach (var update in updates)
				{
					offset = update.Id + 1; // помечаем update как прочитанный

					string text = update.Message.Text;
					if (string.IsNullOrWhiteSpace(text))
						continue;

					string path = CreateAesthetic(update.Message.Text);
					if (path is null)
						continue;

					using (var fs = new FileStream(path, FileMode.Open))
					{
						var inputOnlineFile = new InputOnlineFile(fs);
						await botClient.SendPhotoAsync(update.Message.Chat.Id, inputOnlineFile);
					}
				}

				await Task.Delay(1000);
			}
		}

		static string CreateAesthetic(string text)
		{
			if (text is null)
				return null;

			SizeF bmpSize = new SizeF(300f, 300f);
			var textColor = GetRandomColor();
			var backColor = GetAntiColor(textColor);

			using var bmp = new Bitmap((int)bmpSize.Width, (int)bmpSize.Height);
			
			{
				using var g = Graphics.FromImage(bmp);
				using var font = new Font(FontFamily.Families.First(ff => ff.Name == "Arial"), 30f, FontStyle.Italic | FontStyle.Bold, GraphicsUnit.Point);
				using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
				using var textBrush = new SolidBrush(textColor);
				using var backBrush = new SolidBrush(backColor);
				RectangleF rect = new RectangleF(new PointF(0, 0), bmpSize);
				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
				g.FillRectangle(backBrush, rect);
				g.DrawString(text, font, textBrush, rect, sf);
				g.Save();
			}

			string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name);
			string filePath = Path.Combine(folderPath, Guid.NewGuid().ToString()) + '.' + ImageFormat.Png;
			if (Directory.Exists(folderPath))
			{
				foreach (var file in Directory.EnumerateFiles(folderPath))
				{
					File.Delete(file);
				}
			}
			else
			{
				Directory.CreateDirectory(folderPath);
			}

			bmp.Save(filePath); 

			return filePath;
		}

		static readonly Random Rnd = new Random();

		static Color GetRandomColor()
		{
			int red = Rnd.Next(0, 255);
			int green = Rnd.Next(0, 255);
			int blue = Rnd.Next(0, 255);
			return Color.FromArgb(red, green, blue);
		}

		static Color GetAntiColor(Color color)
		{
			return Color.FromArgb(Color.White.R - color.R, Color.White.G - color.G, Color.White.B - color.B);
		}
	}
}

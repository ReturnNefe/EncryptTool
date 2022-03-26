using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace PicMaker
{
    internal class Program
    {
        static byte GetByteFromArray(byte[] data, int position = 0)
        {
            if(position < data.Length)
                return data[position];
            return 0;
        }

        static void UpdatePosition(long width, long height, ref long left, ref long top)
        {
            while (left >= width)
            {
                ++top;
                left -= width;

                if (top >= height)
                    throw new Exception("Out of images bounds");
            }
        }

        static long ReadBytesFromBitmap(Bitmap image, out byte[] data, long imagePosition = 0L, long readPixelLength = -1)
        {
            bool checkToBounds(long p, long len)
            {
                if (len < 0) return true;
                return p < len;
            }

            var fakeReadPixelLength = readPixelLength >= 0 ? readPixelLength + imagePosition : -1;

            var bytes = new List<byte>();
            var left = imagePosition;
            var top = 0L;

            while(checkToBounds(imagePosition, fakeReadPixelLength))
            {
                UpdatePosition(image.Width, image.Height, ref left, ref top);

                var pixel = image.GetPixel((int)left, (int)top);

                bytes.Add(pixel.R);
                bytes.Add(pixel.G);
                bytes.Add(pixel.B);

                ++imagePosition;
                ++left;
            }

            data = bytes.ToArray();
            return imagePosition;
        }

        static long WriteBytesToBitmap(Bitmap image, byte[] data, long imagePosition = 0, int dataPosition = 0)
        {
            var fakeDataLength = data.Length + dataPosition;
            var left = imagePosition;
            var top = 0L;

            while(dataPosition < fakeDataLength)
            {
                UpdatePosition(image.Width, image.Height, ref left, ref top);

                var r = data[dataPosition];
                var g = GetByteFromArray(data, dataPosition + 1);
                var b = GetByteFromArray(data, dataPosition + 2);
                var pixel = Color.FromArgb(r, g, b);

                image.SetPixel((int)left, (int)top, pixel);

                dataPosition += 3;
                ++imagePosition;
                ++left;
            }

            return imagePosition;
        }

        static async Task Main(string[] args)
        {
            try
            {
                switch (args[0].ToLower())
                {
                    // 帮助
                    case "--help":
                    case "-h":
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write("Pic ");
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write("Maker ");
                            Console.ResetColor();
                            Console.WriteLine(FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion);
                            Console.WriteLine("将文件/文本与图片互相转换.");

                            Console.WriteLine();
                            Console.WriteLine($"使用方法: picmaker [options] [arguments]");

                            Console.WriteLine();
                            var optionLines = new string[]
                            {
                                "-h",
                                "-e/-ef/--encode <infile> <outfile>",
                                "-d/-df/--decode <infile> <outfile>",
                                "-et/--encode-text <infile> <outfile>",
                                "-dt/--decode-text <infile> <outfile>",
                            };
                            var optionDescriptions = new string[]
                            {
                                "帮助",
                                "将文件转换为图片",
                                "将图片转换为文件",
                                "将 UTF-8 文本转换为图片",
                                "将图片转换为 UTF-8 文本"
                            };
                            Console.WriteLine("options:");
                            for (int i = 0; i < optionLines.Length; ++i)
                                Console.WriteLine($"{"",2}{optionLines[i],-40}{optionDescriptions[i]}");

                            Console.WriteLine();
                            Console.WriteLine("arguments:");
                            var argumentLines = new string[]
                            {
                                "<inFile>",
                                "<outFile>"
                            };
                            var argumentDescriptions = new string[]
                            {
                                "读入的文件",
                                "输出文件"
                            };
                            for (int i = 0; i < argumentLines.Length; ++i)
                                Console.WriteLine($"{"",2}{argumentLines[i],-25}{argumentDescriptions[i]}");

                            Console.WriteLine();
                            Console.WriteLine("example:");
                            Console.WriteLine("picmaker -e input.txt test.png");
                            Console.WriteLine("picmaker -d test.png result.txt");

                            /*
                            Console.WriteLine();
                            Console.WriteLine("运行 picmaker -h/--help [options] 获取有关命令的详细信息");
                            */
                            break;
                        }

                    // 加密文件
                    case "--encode":
                    case "-ef":
                    case "-e":
                        {
                            if (args.Length != 3)
                                throw new Exception("Arguments Error");

                            var inFile = args[1];
                            var outFile = args[2];

                            using (var inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
                            {
                                if (Math.Sqrt((int)Math.Ceiling(inStream.Length / 3d)) - int.MaxValue >= (-1 << 10))
                                    throw new Exception("Out of bounds");

                                // 每次读取 384 KB
                                const int cacheLen = 384 * 1024;
                                var bytes = new byte[cacheLen];

                                // 计算大小
                                var realLen = (int)Math.Ceiling(inStream.Length / 3d) + 3;
                                int imageWidth, imageHeight;
                                imageWidth = imageHeight = (int)Math.Ceiling(Math.Sqrt(realLen));

                                using (var image = new Bitmap(imageWidth, imageHeight))
                                {
                                    var position = WriteBytesToBitmap(image, BitConverter.GetBytes(inStream.Length));

                                    while (inStream.Position < inStream.Length)
                                    {
                                        long streamPosition = inStream.Position;
                                        var disByte = inStream.Length - (position - 3) * 3;

                                        await inStream.ReadAsync(bytes, 0, bytes.Length);
                                        position = WriteBytesToBitmap(image,
                                                                      bytes.Take(disByte > cacheLen ? cacheLen : (int)(disByte)).ToArray(),
                                                                      position);
                                    }

                                    image.Save(outFile, ImageFormat.Png);
                                }
                            }
                            break;
                        }

                    case "--decode":
                    case "-df":
                    case "-d":
                        {
                            if (args.Length != 3)
                                throw new Exception("Arguments Error");

                            var inFile = args[1];
                            var outFile = args[2];

                            using (var outStream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
                            using (var image = new Bitmap(inFile))
                            {
                                byte[] lenBytes;
                                var position = ReadBytesFromBitmap(image, out lenBytes, readPixelLength: 3);
                                var dataLen = BitConverter.ToInt64(lenBytes.Take(8).ToArray());

                                if (Math.Sqrt((int)Math.Ceiling(dataLen / 3d)) - int.MaxValue >= (-1 << 10))
                                    throw new Exception("Out of bounds");

                                // 每次读取 384 KB
                                const int cacheLen = 384 * 1024;
                                var imgCacheLen = cacheLen / 3;
                                var bytes = new byte[cacheLen];

                                while ((position - 3) * 3 < dataLen)
                                {
                                    var disByte = dataLen - (position - 3) * 3;
                                    var disPixel = (long)Math.Ceiling(dataLen / 3d) - (position - 3);
                                    position = ReadBytesFromBitmap(image, out bytes, position, disPixel > imgCacheLen ? imgCacheLen : disPixel);
                                    await outStream.WriteAsync(bytes, 0, disByte > cacheLen ? cacheLen : (int)disByte);
                                }
                            }

                            break;
                        }

                    // 加密文本
                    case "--encode-text":
                    case "-et":
                        {
                            if (args.Length != 3)
                                throw new Exception("Arguments Error");

                            var inFile = args[1];
                            var outFile = args[2];
                            byte[] inBytes;

                            using (var inStream = new StreamReader(inFile, Encoding.UTF8))
                            {
                                inBytes = Encoding.UTF8.GetBytes(await inStream.ReadToEndAsync());
                            }

                            // 计算大小
                            var realLen = (int)Math.Ceiling(inBytes.Length / 3d) + 2;
                            int imageWidth, imageHeight;
                            imageWidth = imageHeight = (int)Math.Ceiling(Math.Sqrt(realLen));

                            while (realLen - imageHeight * imageWidth > imageWidth)
                                --imageHeight;

                            using (var image = new Bitmap(imageWidth, imageHeight))
                            {
                                var tmpPosition = WriteBytesToBitmap(image, BitConverter.GetBytes(inBytes.Length));
                                WriteBytesToBitmap(image, inBytes, tmpPosition);

                                image.Save(outFile, ImageFormat.Png);
                            }

                            break;
                        }

                    // 解密文本
                    case "--decode-text":
                    case "-dt":
                        {
                            if (args.Length != 3)
                                throw new Exception("Arguments Error");

                            var inFile = args[1];
                            var outFile = args[2];
                            var dataLen = 0;
                            byte[] inBytes;
                            byte[] outBytes;

                            using (var image = new Bitmap(inFile))
                            {
                                var tmpPosition = ReadBytesFromBitmap(image, out inBytes, readPixelLength: 2);
                                dataLen = BitConverter.ToInt32(inBytes.Take(4).ToArray());
                                ReadBytesFromBitmap(image, out outBytes, tmpPosition, (int)Math.Ceiling(dataLen / 3d));
                            }

                            using (var outStream = new StreamWriter(outFile, false, Encoding.UTF8))
                                await outStream.WriteAsync(Encoding.UTF8.GetString(outBytes.Take(dataLen).ToArray()));

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
}
    }
}
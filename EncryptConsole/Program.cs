using System;
using System.Text;
using System.IO;
using System.Linq;
using RetChen.Encryption;
using System.Diagnostics;

namespace EncryptConsole
{
    internal class Program
    {
        static void Main(string[] args)
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
                            Console.Write("Encrypt ");
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write("Console ");
                            Console.ResetColor();
                            Console.WriteLine(FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion);
                            Console.WriteLine("使用 AES & RSA 加密文件.");

                            Console.WriteLine();
                            Console.WriteLine($"使用方法: EncryptConsole [options] [arguments]");

                            Console.WriteLine();
                            var optionLines = new string[]
                            {
                                "-h",
                                "-e/--encrypt <infile> <outfile>",
                                "-d/--decrypt <infile> <outfile>",
                                "-m/--make <length>"
                            };
                            var optionDescriptions = new string[]
                            {
                                "帮助",
                                "加密文件",
                                "解密文件",
                                "生成 RSA 密钥"
                            };
                            Console.WriteLine("options:");
                            for (int i = 0; i < optionLines.Length; ++i)
                                Console.WriteLine($"{"",2}{optionLines[i],-40}{optionDescriptions[i]}");

                            Console.WriteLine();
                            Console.WriteLine("arguments:");
                            var argumentLines = new string[]
                            {
                                "<inFile>",
                                "<outFile>",
                                "<length>"
                            };
                            var argumentDescriptions = new string[]
                            {
                                "读入的文件",
                                "输出文件",
                                "密钥长度, 例如 4096"
                            };
                            for (int i = 0; i < argumentLines.Length; ++i)
                                Console.WriteLine($"{"",2}{argumentLines[i],-25}{argumentDescriptions[i]}");

                            Console.WriteLine();
                            Console.WriteLine("example:");
                            Console.WriteLine("EncryptConsole -m 2048");
                            Console.WriteLine("EncryptConsole -e input.txt output.txt");
                            Console.WriteLine("EncryptConsole -d output.txt result.txt");
                            break;
                        }

                    // 加密
                    case "--encrypt":
                    case "-e":
                        {
                            if (args.Length != 3)
                                throw new ArgumentException("Parameter Error");

                            var inFile = args[1];
                            var outFile = args[2];

                            Console.WriteLine("Input the RSA Key (Public) :");
                            var rsaPubKey = Console.ReadLine();
                            var rsa = new RSAEncryption("", rsaPubKey);

                            var aesKey = AESEncryption.RandomKey();
                            var aes = new AESEncryption(aesKey);

                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write("AES Key (Encrypted)");
                            Console.ResetColor();
                            Console.WriteLine($" = {rsa.Encrypt(Convert.ToBase64String(aesKey))}");
                            Console.WriteLine();

                            Console.WriteLine("Encrypting ...");

                            using (var inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
                            using (var outStream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
                            {
                                // 每次读取 512 KB
                                const int len = 512 * 1024;
                                var bytes = new byte[len];

                                while (inStream.Position < inStream.Length)
                                {
                                    long position = inStream.Position;
                                    inStream.Read(bytes, 0, bytes.Length);
                                    outStream.Write(aes.Encrypt(bytes.Take(inStream.Length - position > len ? len : (int)(inStream.Length - position)).ToArray(), System.Security.Cryptography.CipherMode.ECB));
                                }
                            }

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Success");
                            Console.ResetColor();
                            break;
                        }

                    // 解密
                    case "--decrypt":
                    case "-d":
                        {
                            if (args.Length != 3)
                                throw new ArgumentException("Parameter Error");

                            var inFile = args[1];
                            var outFile = args[2];

                            Console.WriteLine("Input the RSA Key (Private) :");
                            var rsaPriKey = Console.ReadLine();
                            var rsa = new RSAEncryption(rsaPriKey, "");

                            Console.WriteLine("Input the AES Key (Encrypted) :");
                            var aesKey = rsa.Decrypt(Console.ReadLine());
                            var aes = new AESEncryption(Convert.FromBase64String(aesKey));

                            Console.WriteLine();
                            Console.WriteLine("Decrypting ...");

                            using (var inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
                            using (var outStream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
                            {
                                // 每次读取 512 KB
                                const int len = 512 * 1024;
                                var bytes = new byte[len];

                                while (inStream.Position < inStream.Length)
                                {
                                    long position = inStream.Position;
                                    inStream.Read(bytes, 0, bytes.Length);
                                    outStream.Write(aes.Decrypt(bytes.Take(inStream.Length - position > len ? len : (int)(inStream.Length - position)).ToArray(), System.Security.Cryptography.CipherMode.ECB));
                                }
                            }

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Success");
                            Console.ResetColor();
                            break;
                        }

                    // 生成 RSA 密钥
                    case "--make":
                    case "-m":
                        {
                            var len = args.Length >= 2 ? int.Parse(args[1]) : 4096;
                            var privateKey = "";
                            var publicKey = "";
                            
                            RSAEncryption.RandomKey(out privateKey, out publicKey, len);

                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write("PublicKey");
                            Console.ResetColor();
                            Console.WriteLine($" = {publicKey}");

                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write("PrivateKey");
                            Console.ResetColor();
                            Console.WriteLine($" = {privateKey}");

                            Console.WriteLine();
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

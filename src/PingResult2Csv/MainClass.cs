using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PingResult2Csv
{
    enum PingType
    {
        WinNT,
        Unix,
        Unknown
    }
    class MainClass
    {
        private static Encoding textEncoding;

        static void Main(string[] args)
        {
            var assembly = Assembly.GetEntryAssembly();
            var os = Environment.OSVersion;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (os.Platform == PlatformID.Win32NT)
            {
                textEncoding = Encoding.GetEncoding("Shift_JIS");
            }
            else
            {
                textEncoding = Encoding.UTF8;
            }

            string root_path = Path.GetDirectoryName(assembly.Location);
            var data_di = new DirectoryInfo(Path.Combine(root_path, "data"));

            try
            {
                if (args.Length > 0 && (!string.IsNullOrWhiteSpace(args[0])))
                {
                    data_di = new DirectoryInfo(args[0]);
                }
            }
            catch
            {
                Console.WriteLine("不正な引数が検出されました");
                Console.ReadKey();
                return;
            }

            if (!data_di.Exists)
            {
                Console.WriteLine("Data格納フォルダが存在しません");
                Console.ReadKey();
                return;
            }

            var so = SearchOption.TopDirectoryOnly;
            FileInfo[] ping_txt = data_di.GetFiles("*.txt", so);
            var res = new PingResult[ping_txt.Length];

            for (var i = 0; i < ping_txt.Length; i++)
            {
                res[i] = GetPingResultObject(ping_txt[i]);
            }

            int max = res.Max((x) => x.TimeQueue.Count);
            string[] header =
                ping_txt.Select((x) => x.Name.Replace(".txt", "")).ToArray();
            var contents = new List<string[]>(max + 2);

            for (var i = 0; i < max + 2; i++)
            {
                var row = new string[header.Length];

                for (var j = 0; j < header.Length; j++)
                {
                    if (i == 0)
                    {
                        row[j] = string.Format("{0}({1})",
                                               res[j].ToIPAddress, res[j].ToHostName);
                    }
                    else if (i == max + 1)
                    {
                        row[j] = res[j].StatisticsText;
                    }
                    else
                    {
                        if (res[j].TimeQueue.Count > 0)
                        {
                            row[j] = res[j].TimeQueue.Dequeue();
                        }
                        else
                        {
                            row[j] = string.Empty;
                        }
                    }
                }

                contents.Add(row);
            }

            var cc = new CsvContents(header, contents);
            string csv_path = Path.Combine(data_di.Parent.FullName, "result.csv");

            var cw = new CsvWriter(',', textEncoding, true);
            cw.Write(csv_path, cc);

            Console.WriteLine("完了");
            Console.ReadKey();
        }

        private static PingResult GetPingResultObject(FileInfo file)
        {
            string toHostName = string.Empty;
            string toIPAddress = string.Empty;
            var timeQueue = new Queue<string>();
            var statistics = new StringBuilder();

            var type = PingType.Unknown;
            bool exist_statistics = false;

            using var sr = new StreamReader(file.FullName, textEncoding);

            while (sr.Peek() > -1)
            {
                string line = sr.ReadLine();

                switch (type)
                {
                    case PingType.Unknown:
                        var sc = StringComparison.CurrentCulture;

                        if (line.Contains("PING", sc))
                        {
                            string[] sl = line.Split(' ');

                            toHostName = sl[1];
                            toIPAddress = sl[2].Trim('(', ')');
                            type = PingType.Unix;

                        }
                        else if (line.Contains("送信しています"))
                        {
                            var nt_fl = Regex.Match(line, @"(.+) (\[.+\])?に");

                            if (!nt_fl.Success) { continue; }
                            toIPAddress = nt_fl.Groups[1].Value;

                            if (string.IsNullOrEmpty(nt_fl.Groups[2].Value))
                            {
                                toHostName = nt_fl.Groups[1].Value;
                            }
                            else
                            {
                                toHostName = nt_fl.Groups[2].Value.Trim('[', ']');
                            }

                            type = PingType.WinNT;
                        }

                        break;

                    case PingType.WinNT:
                        if (!exist_statistics)
                        {
                            var nt_cnt = Regex.Match(line, "時間 (<|=)([0-9]+)ms");

                            if (nt_cnt.Success)
                            {
                                timeQueue.Enqueue(nt_cnt.Groups[2].Value);
                            }
                            else 
                            {
                                exist_statistics = line.Contains("統計");

                                if ((!exist_statistics) && (!string.IsNullOrEmpty(line)))
                                {
                                    timeQueue.Enqueue("error");
                                }
                            }
                        }
                        else
                        {
                            if (line.Contains("パケット数"))
                            {
                                statistics.AppendLine(line.Trim().TrimEnd('、'));
                            }
                            else
                            {
                                if (line.Contains("最小"))
                                {
                                    statistics.Append(" " + line.Trim());
                                }
                                else
                                {
                                    statistics.Append(line.Trim());
                                }
                            }
                        }

                        break;

                    case PingType.Unix:
                        if (!exist_statistics)
                        {
                            var unix_cnt = Regex.Match(line, "time=([0-9.]+) ms");

                            if (unix_cnt.Success)
                            {
                                timeQueue.Enqueue(unix_cnt.Groups[1].Value);
                            }
                            else
                            {
                                exist_statistics = line.Contains("statistics");

                                if ((!exist_statistics) && (!string.IsNullOrEmpty(line)))
                                {
                                    timeQueue.Enqueue("error");
                                }
                            }
                        }
                        else
                        {
                            if (line.Contains("transmitted"))
                            {
                                statistics.AppendLine(line);
                            }
                            else
                            {
                                statistics.Append(line);
                            }
                        }
                            
                        break;
                }
            }

            if (type == PingType.Unknown)
            {
                return new PingResult();
            }
            else
            {
                return new PingResult(toHostName, toIPAddress,
                                      timeQueue, statistics.ToString());
            }
        }
    }
}

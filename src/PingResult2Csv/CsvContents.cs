using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PingResult2Csv
{
    /// <summary>
    /// CSVの内容をListで保持するクラス
    /// </summary>
    public class CsvContents
    {
        /// <summary> ヘッダーの一覧 </summary>
        public IEnumerable<string> Headers { get; private set; }

        /// <summary> コンテンツ </summary>
        public IEnumerable<IEnumerable<string>> Contents { get; private set; }

        /// <summary>
        /// CSVの内容をIEnumerableで保持します
        /// </summary>
        public CsvContents(IEnumerable<string> Headers,
                           IEnumerable<IEnumerable<string>> Contents)
        {
            this.Headers = Headers;
            this.Contents = Contents;
        }

        /// <summary>
        /// CsvContentsの情報を文字列として表します
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            string[] header = Headers.ToArray();

            sb.AppendLine($"ヘッダー：[{string.Join("], [", Headers)}]");

            foreach (var ctn in Contents)
            {
                string line = string.Empty;
                string[] row = ctn.ToArray();

                for (int i = 0; i < header.Length; i++)
                {
                    line += $"[{header[i]}] => [{row[i]}] | ";
                }

                sb.AppendLine(line.Remove(line.Length - 3));
            }

            return sb.ToString();
        }

        /// <summary>
        /// CsvContentsの任意の列情報を文字列として表します
        /// </summary>
        /// <param name="row"> 列番号 </param>
        /// <returns></returns>
        public string ToString(int row)
        {
            string line = string.Empty;
            string[] header = Headers.ToArray();
            
            if (Contents.Count() > row)
            {
                for (int i = 0; i < header.Length; i++)
                {
                    string[] target = Contents.Skip(row).First().ToArray();
                    line += $"[{header[i]}] => [{target[i]}] | ";
                }

                line = line.Remove(line.Length - 3);
            }

            return line;
        }
    }
}

using System.Collections.Generic;

namespace PingResult2Csv
{
    /// <summary>
    /// pingの結果を格納するためのクラス
    /// </summary>
    public class PingResult
    {
        /// <summary> ping先ホスト名 </summary>
        public string ToHostName { get; }

        /// <summary> ping先IPアドレス </summary>
        public string ToIPAddress { get; }

        /// <summary> 往復時間 </summary>
        public Queue<string> TimeQueue { get; }

        /// <summary> 統計情報 </summary>
        public string StatisticsText { get; }

        /// <summary>
        /// 空データ作成用コンストラクタ
        /// </summary>
        public PingResult()
        {
            ToHostName = string.Empty;
            ToIPAddress = string.Empty;
            TimeQueue = new Queue<string>(0);
            StatisticsText = string.Empty;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        /// <param name="ToHostName"> ping先ホスト名 </param>
        /// <param name="ToIPAddress"> ping先IPアドレス </param>
        /// <param name="TimeQueue"> 往復時間 </param>
        /// <param name="StatisticsText"> 統計情報 </param>
        public PingResult(string ToHostName, string ToIPAddress,
                          Queue<string> TimeQueue, string StatisticsText)
        {
            this.ToHostName = ToHostName;
            this.ToIPAddress = ToIPAddress;
            this.TimeQueue = TimeQueue;
            this.StatisticsText = StatisticsText;
        }
    }
}

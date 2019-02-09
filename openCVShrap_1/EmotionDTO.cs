using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace openCVShrap_1
{
    [DataContract(Name = "emotion")]
    public class EmotionDTO
    {
        /// <summary>
        /// Excel出力時に使用するID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 撮影開始時刻(yyyymmddhhMMss.ttt)
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 表情認識までの経過時間(ミリ秒)
        /// </summary>
        public string ElapsedTime { get; set; }

        /// <summary>
        /// 怒り
        /// </summary>
        [DataMember(Name = "anger")]
        public float Anger { get; set; }

        /// <summary>
        /// 侮辱
        /// </summary>
        [DataMember(Name = "contempt")]
        public float Contempt { get; set; }

        /// <summary>
        /// 嫌悪
        /// </summary>
        [DataMember(Name = "disgust")]
        public float Disgust { get; set; }

        /// <summary>
        /// 恐れ
        /// </summary>
        [DataMember(Name = "fear")]
        public float Fear { get; set; }

        /// <summary>
        /// 幸せ
        /// </summary>
        [DataMember(Name = "happiness")]
        public float Happiness { get; set; }

        /// <summary>
        /// 普通
        /// </summary>
        [DataMember(Name = "neutral")]
        public float Neutral { get; set; }

        /// <summary>
        /// 悲しみ
        /// </summary>
        [DataMember(Name = "sadness")]
        public float Sadness { get; set; }

        /// <summary>
        /// 驚き
        /// </summary>
        [DataMember(Name = "surprise")]
        public float Surprise { get; set; }

        /// <summary>
        /// 話しかけてよいかの判断材料
        /// </summary>
        public int Danger { get; set; }

        /// <summary>
        /// 顔検出位置の矩形の左上のX座標
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// 顔検出位置の矩形の左上のY座標
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// 顔検出位置の矩形の右下のX座標
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// 顔検出位置の矩形の右下のY座標
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// 表情認識の元画像のパス
        /// </summary>
        public string SrcPath { get; set; }

        /// <summary>
        /// 表情認識結果を追記した画像のパス
        /// </summary>
        public string ResPath { get; set; }

        /// <summary>
        /// 0：話しかけないほうがよい、1：話しかけてよい
        /// </summary>
        public int OK_Flg { get; set; }



        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public EmotionDTO() { }

        public EmotionDTO(
            int id,
            string startTime,
            string elapsedTime,
            float anger,
            float contempt,
            float disgust,
            float fear,
            float happiness,
            float neutral,
            float sadness,
            float surprise,
            int danger,
            int left,
            int top,
            int right,
            int bottom,
            string srcPath,
            string resPath,
            int ok_flg)
        {
            this.Id = id;
            this.StartTime = StartTime;
            this.ElapsedTime = elapsedTime;
            this.Anger = anger;
            this.Contempt = contempt;
            this.Disgust = disgust;
            this.Fear = fear;
            this.Happiness = happiness;
            this.Neutral = neutral;
            this.Sadness = sadness;
            this.Surprise = surprise;
            this.Danger = danger;
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.SrcPath = srcPath;
            this.ResPath = resPath;
            this.OK_Flg = ok_flg;

        }
            
    }
}

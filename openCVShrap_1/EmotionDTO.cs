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
        /// デフォルトコンストラクタ
        /// </summary>
        public EmotionDTO() { }

        public EmotionDTO(
            float anger,
            float contempt,
            float disgust,
            float fear,
            float happiness,
            float neutral,
            float sadness,
            float surprise)
        {
            this.Anger = anger;
            this.Contempt = contempt;
            this.Disgust = disgust;
            this.Fear = fear;
            this.Happiness = happiness;
            this.Neutral = neutral;
            this.Sadness = sadness;
            this.Surprise = surprise;

        }
            
    }
}

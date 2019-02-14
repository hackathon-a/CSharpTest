using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.UserInterface;

using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json.Linq;

using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace openCVShrap_1
{
    

    public partial class Form1 : Form
    {

        bool isCanceled = false;
        string retStr = "";
        const long ONE_SEC_IN_100NANOSEC = 10000000;
        static EmotionDTO LocalEmotionDTO { get; set; }
        static int id = 0;

        private enum EmotionState
        {
            Recording_NotReady = -1,
            Feeling_Bad,                    // 0:話しかけないほうがよい
            Feeling_Good,                   // 1:話しかけてよい
            
            
        }

        // 感情記録時のユーザ判定
        private int myEmotion = (int)EmotionState.Recording_NotReady;

        // Excel出力時の1行目に表示する固定文字列
        string[] ColumnNames = {
                                "out_id",
                                "time",
                                "elapsed",
                                "anger",
                                "contempt",
                                "disgust",
                                "fear",
                                "happiness",
                                "neutral",
                                "sadness",
                                "surprise",
                                "danger",
                                "left",
                                "right",
                                "top",
                                "bottom",
                                "src_path",
                                "respath",
                                "ok_fl"
                                };


        public Form1()
        {
            InitializeComponent();

            System.Reflection.Assembly myAssembly =
                System.Reflection.Assembly.GetExecutingAssembly();

            this.pictureBox2.Image = openCVShrap_1.Properties.Resources.computer11_sleep;



        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        static ImageConverter MyByteConverter = new ImageConverter();

        static async Task<string> MakeRequest(byte[] byteData)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid key.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "84b835f0a0954842874177bc4228720d"); //

            // NOTE: You must use the same region in your REST call as you used to obtain your subscription keys.
            //   For example, if you obtained your subscription keys from westcentralus, replace "westus" in the
            //   URI below with "westcentralus".
            //string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            string uri = "https://japaneast.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceAttributes=emotion";
            HttpResponseMessage response;
            string responseContent;

            string retStr = string.Empty;

            try
            {
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);
                    responseContent = await response.Content.ReadAsStringAsync();
                }

                // A peek at the raw JSON response.
                Console.WriteLine(responseContent);

                // Processing the JSON into manageable objects.
                JToken rootToken = JArray.Parse(responseContent).First;

                // 表情認識できない場合に、responseContentが"[]"になるため、その場合rootTokenがnullになる
                // そのため、rootTokenがnullなら次の処理に回す
                if (rootToken == null)
                {
                    return string.Empty;
                }

                // First token is always the faceRectangle identified by the API.
                JToken faceRectangleToken = rootToken.First;

                // Second token is all emotion scores.
                JToken scoresToken = rootToken.Last;

                // ■顔検出位置
                Rect2f rect = new Rect2f();

                // Show all face rectangle dimensions
                //JEnumerable<JToken> faceRectangleSizeList = faceRectangleToken.First.Children();
                JEnumerable<JToken> faceRectangleSizeList = faceRectangleToken.Next.Children();
                foreach (var size in faceRectangleSizeList)
                {
                    Console.WriteLine(size);

                    // 顔検出位置をrectで保持
                    rect.Left = (float)size.ElementAt(0);
                    rect.Top = (float)size.ElementAt(1);
                    rect.Width = (float)size.ElementAt(2);
                    rect.Height = (float)size.ElementAt(3);

                }

                // Show all scores
                JEnumerable<JToken> scoreList = scoresToken.First.Children();
                foreach (var score in scoreList)
                {
                    Console.WriteLine(score);

                    // 後で扱いやすいようにデシリアライズしておく
                    // ■TODO：ローカル変数を常にインスタンス生成しなおす必要あり。
                    // ■TODO：今のままではListに格納した際に、古いものまで行進されてしまう。(Cloneメソッドを実装するのもあり)
                    LocalEmotionDTO = Deserialize(score, rect);

                    retStr += score;
                }
            }
            catch(Exception)
            {
                retStr = string.Empty;
            }

            return retStr;
        }

        delegate void setResultDelegate();
        void setResult( )
        {
            this.textBox1.Text = this.retStr;
        }

        /// <summary>
        /// EmotionAPIの取得結果のJSONをEmotionDTOへ変換する
        /// </summary>
        /// <param name="resultJson"></param>
        /// <returns></returns>
        static EmotionDTO Deserialize(JToken resultJson, Rect2f rect)
        {
            JEnumerable<JToken> emotionObj = resultJson.First.Children();

            // 本当はDataContractJsonSerializerのように、マッピングしたいのだが・・・。
            // 実現する方法がわからなかったので、決め打ちなことを利用してElementAt()で各要素を取得しEmotionDTOに設定
            EmotionDTO emotionDTO = new EmotionDTO(
                id,                                 // Excel出力時のID
                string.Empty,                       // 撮影日時
                string.Empty,                       // 表情認識完了、画像加工完了までの経過時間(ミリ秒)
                (float)emotionObj.ElementAt(0),     // EmotionAPIのanger
                (float)emotionObj.ElementAt(1),     // EmotionAPIのcontempt
                (float)emotionObj.ElementAt(2),     // EmotionAPIのdisgust
                (float)emotionObj.ElementAt(3),     // EmotionAPIのfear
                (float)emotionObj.ElementAt(4),     // EmotionAPIのhappiness
                (float)emotionObj.ElementAt(5),     // EmotionAPIのneutral
                (float)emotionObj.ElementAt(6),     // EmotionAPIのsadness
                (float)emotionObj.ElementAt(7),     // EmotionAPIのsurprise
                (int)Math.Min(500 * ((float)emotionObj.ElementAt(0) + (float)emotionObj.ElementAt(1) + (float)emotionObj.ElementAt(2)),100),                                  // 羽田さん岩見さんアルゴリズムの「話しかけてよいか」の算出結果(0.0～1.0)
                (int)rect.Left,                     // 顔検出位置の矩形の左上のX座標
                (int)rect.Top,                      // 顔検出位置の矩形の左上のY座標
                (int)rect.Right,                    // 顔検出位置の矩形の右下のX座標
                (int)rect.Bottom,                   // 顔検出位置の矩形の右下のY座標
                string.Empty,                       // 表情認識の元画像のパス
                string.Empty,                       // 画像に表情認識結果を追記した画像のパス
                0                                   // 0：話しかけないほうがよい、1：話しかけてよい
                );

            // 次のデータ保存用にIDをインクリメント

            return emotionDTO;
        }

        private void CaptureCamera()
        {
            VideoCapture videoCapture = OpenCvSharp.VideoCapture.FromCamera(0);

            int frameNumber = 0;
            videoCapture.Fps = 30;

            // Taskで別スレッド化
            var task = Task.Run(async () =>
             {
                 // 画像と撮影データ保存フォルダを作成する
                 // 現在時刻をフォルダ名として使う文字列にする
                 string nowTime = DateTime.Now.ToString("yyyyMMddhhmmss");

                 // 表情認識結果のリスト
                 List<EmotionDTO> emotionDtoList = new List<EmotionDTO>();

                 try
                 {
                     //using (var window = new Window("capture"))
                     {

                         // Frame image buffer
                         Mat image = new Mat();

                         // 初回実行時点のtickカウントを0で初期化
                         DateTime firstTime = DateTime.Now;
                         DateTime beforeTime = firstTime;

                         // 次回発火時刻は4秒後
                         DateTime nextFireTime = beforeTime.AddSeconds(4.0);

                         // ダンプ先が無ければ作る
                         if (!System.IO.Directory.Exists(nowTime))
                         {
                             System.IO.Directory.CreateDirectory(nowTime);
                         }

                         

                         // When the movie playback reaches end, Mat.data becomes NULL.
                         while (!isCanceled)
                         {
                             videoCapture.Read(image); // same as cvQueryFrame
                             if (image.Empty())
                                 break;

                             // frame数をカウントアップ
                             frameNumber++;

                             DateTime currentTime = DateTime.Now;

                             // 初回は時刻の差に関係なく処理続行
                             if(firstTime != beforeTime)
                             {
                                 
                                 if(nextFireTime > currentTime)
                                 {
                                     // while文の先頭から繰り返し
                                     continue;
                                 }
                                 else
                                 {
                                     // 現在時刻を前回実行時刻としてすぐに覚えなおし、次回発火時刻を再計算
                                     CalcNextFireTime(currentTime, ref beforeTime, ref nextFireTime);
                                 }
                             }
                             else
                             {
                                 // 初回実行時は現在時刻を前回実行時刻としてすぐに覚えなおし、次回発火時刻を再計算
                                 CalcNextFireTime(currentTime, ref beforeTime, ref nextFireTime);
                             }
                            
                             
                             Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                             // 画像も表示しておく

                             byte[] byteData = (byte[])MyByteConverter.ConvertTo(bmp, typeof(byte[]));

                             // 保存したら表情認識APIを呼ぶ
                             this.retStr = await MakeRequest(byteData);

                             if (this.pictureBox1.Image != null)
                             {
                                 this.pictureBox1.Image.Dispose();
                                 this.pictureBox1.Image = null;
                             }
                             this.pictureBox1.Image = bmp;

                             // 非同期でテキストボックスに結果を表示する
                             Invoke(new setResultDelegate(setResult));

                             // ■羽田さん岩見さんアルゴリズム　ここから
                             // OKボタンまたはNGボタンが押下されていれば、画像を保存し感情パラメータも保存する
                             if(this.myEmotion != (int)EmotionState.Recording_NotReady)
                             {
                                 // 画像とパラメータを紐づけられるようにしてデータ化することで、各個人の「話しかけて良い表情」と「話しかけないほうがよい表情」を作成する
                                 // ①まず画像を保存
                                 string imageFile = nowTime + @"\" + frameNumber.ToString() + ".jpg";
                                 Cv2.ImWrite(imageFile, image);

                                 // ②パラメータを準備する
                                 UpdateEmotionDTO(currentTime,      // 経過時間算出のための、開始時間
                                                    imageFile,      // 表情認識を実施した画像
                                                    imageFile);     // 画像そのものに表情認識結果を追記した画像(TODO：元画像へ結果を書き込みパスを変更する)

                                 // ③パラメータをリストに格納しておく
                                 emotionDtoList.Add(LocalEmotionDTO);
                              

                             }
                             
                             // ■羽田さん岩見さんアルゴリズム　ここまで

                         }
                     }
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show(ex.Message);
                     throw;
                 }
                 finally
                 {
                     // 羽田さん岩見さんアルゴリズムの撮影結果をExcelに出力する
                     OutToExcel(nowTime, emotionDtoList);

                     videoCapture.Release();
                 }
             
            });
        }

        // 次回発火時刻計算
        private void CalcNextFireTime(DateTime currentTime, ref DateTime beforeTime, ref DateTime nextFireTime)
        {
            // 現在時刻を前回実行時刻として覚えなおす
            beforeTime = currentTime;

            // 次回発火時刻を再計算
            nextFireTime = beforeTime.AddSeconds(4.0);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.isCanceled = false;

            // startボタンは無効化(連打抑止)
            this.button1.Enabled = false;

            // stopボタンは有効化
            this.button2.Enabled = true;

            // OKボタン有効化
            this.button3.Enabled = true;

            // NGボタン有効化
            this.button4.Enabled = true;

            CaptureCamera();

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.isCanceled = true;

            // startボタンは有効化
            this.button1.Enabled = true;
            // stopボタンは無効化
            this.button2.Enabled = false;

            // OKボタン有効化
            this.button3.Enabled = false;

            // NGボタン有効化
            this.button4.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // OKボタン押下時は「話しかけてよい」と判断
            this.myEmotion = (int)EmotionState.Feeling_Good;

            // 一度押されたらNGボタンと排他制御する
            this.button3.Enabled = false;
            // 色も変える
            this.button3.BackColor = Color.Gray;

            this.button4.Enabled = true;
            // 色も変える
            this.button4.BackColor = Color.Red;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // NGボタン押下時は「話しかけないほうがよい」と判断
            this.myEmotion = (int)EmotionState.Feeling_Bad;

            // OKボタンと排他する
            this.button4.Enabled = false;
            // 色も変える
            this.button4.BackColor = Color.Gray;

            this.button3.Enabled = true;
            this.button3.BackColor = Color.Blue;
        }


        private void OutToExcel(string outputFileName, List<EmotionDTO> emotionDTOList)
        {

            Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application
            {
                Visible = false
            };
            Workbook wb = ExcelApp.Workbooks.Add();

            Worksheet ws1 = wb.Sheets[1];
            ws1.Select(Type.Missing);
            try
            {
                // ■TODO：ここで引数のデータをExcelに書き込む

                // まずは1行目にカラム名に相当する文字列を出力
                int columnIndex = 1;
                foreach(string columnName in ColumnNames)
                {
                    Microsoft.Office.Interop.Excel.Range range = ws1.Cells[1, columnIndex];
                    range.Value2 = columnName;
                    columnIndex++;
                }

                // 次に集めたデータを繰り返し出力する
                // 1行目はカラム表示行なのでデータは2行目から
                int rowIndex = 2;
                foreach (EmotionDTO emotion in emotionDTOList)
                {
                    UpdateWorkSheetRow(emotion, ws1, rowIndex);
                    rowIndex++;
                }


                // ファイル保存
                wb.SaveAs(outputFileName);
                wb.Close(false);
                ExcelApp.Quit();
            }
            finally
            {
                Marshal.ReleaseComObject(ws1);
                Marshal.ReleaseComObject(wb);
                Marshal.ReleaseComObject(ExcelApp);
            }
        }

        /// <summary>
        /// EmotionDTOの内容をExcel出力用WorkSheetに書き込む
        /// </summary>
        /// <param name="emotion"></param>
        /// <param name="ws1"></param>
        /// <param name="rowIndex"></param>
        private void UpdateWorkSheetRow(EmotionDTO emotion, Worksheet ws1, int rowIndex)
        {
            // ID
            Microsoft.Office.Interop.Excel.Range rangeData_ID = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "out_id")];
            rangeData_ID.Value2 = emotion.Id;

            // startTime
            Microsoft.Office.Interop.Excel.Range rangeData_StartTime = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "time")];
            rangeData_StartTime.Value2 = emotion.StartTime;

            // elapsedTime
            Microsoft.Office.Interop.Excel.Range rangeData_ElapsedTime = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "elapsed")];
            rangeData_ElapsedTime.Value2 = emotion.ElapsedTime;

            // Anger
            Microsoft.Office.Interop.Excel.Range rangeData_Anger = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "anger")];
            rangeData_ElapsedTime.Value2 = emotion.Anger;

            // contempt
            Microsoft.Office.Interop.Excel.Range rangeData_Contempt = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "comtempt")];
            rangeData_ElapsedTime.Value2 = emotion.Contempt;

            // disgust
            Microsoft.Office.Interop.Excel.Range rangeData_Disgust = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "disgust")];
            rangeData_ElapsedTime.Value2 = emotion.Disgust;

            // fear
            Microsoft.Office.Interop.Excel.Range rangeData_fear = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "fear")];
            rangeData_ElapsedTime.Value2 = emotion.Fear;

            // happiness
            Microsoft.Office.Interop.Excel.Range rangeData_Happiness = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "happiness")];
            rangeData_ElapsedTime.Value2 = emotion.Happiness;

            // neutral
            Microsoft.Office.Interop.Excel.Range rangeData_Neutral = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "neutral")];
            rangeData_ElapsedTime.Value2 = emotion.Neutral;


            // sadness
            Microsoft.Office.Interop.Excel.Range rangeData_Sadness = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "sadness")];
            rangeData_ElapsedTime.Value2 = emotion.Sadness;

            // surprize
            Microsoft.Office.Interop.Excel.Range rangeData_Surprise = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "surprise")];
            rangeData_ElapsedTime.Value2 = emotion.Surprise;

            // danger
            Microsoft.Office.Interop.Excel.Range rangeData_Danger = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "danger")];
            rangeData_ElapsedTime.Value2 = emotion.Danger;

            // left
            Microsoft.Office.Interop.Excel.Range rangeData_Left = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "left")];
            rangeData_ElapsedTime.Value2 = emotion.Left;

            // right
            Microsoft.Office.Interop.Excel.Range rangeData_Right = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "right")];
            rangeData_ElapsedTime.Value2 = emotion.Right;

            // top
            Microsoft.Office.Interop.Excel.Range rangeData_Top = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "top")];
            rangeData_ElapsedTime.Value2 = emotion.Top;

            // bottom
            Microsoft.Office.Interop.Excel.Range rangeData_Bottom = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "bottom")];
            rangeData_ElapsedTime.Value2 = emotion.Bottom;

            // srcPath
            Microsoft.Office.Interop.Excel.Range rangeData_SrcPath = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "src_path")];
            rangeData_ElapsedTime.Value2 = emotion.SrcPath;

            // respath
            Microsoft.Office.Interop.Excel.Range rangeData_ResPath = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "respath")];
            rangeData_ElapsedTime.Value2 = emotion.ResPath;

            // ok_fl
            Microsoft.Office.Interop.Excel.Range rangeData_OK_FL = ws1.Cells[rowIndex, Array.IndexOf(ColumnNames, "ok_fl")];
            rangeData_ElapsedTime.Value2 = emotion.OK_Flg;
        }

        /// <summary>
        /// 表情認識結果をもとにEmotionDTOを更新する
        /// </summary>
        /// <param name="currentTime"></param>
        private void UpdateEmotionDTO(DateTime currentTime, string srcPath, string resPath)
        {
            // ②-1 開始時刻
            LocalEmotionDTO.StartTime = currentTime.ToString("yyyymmddhhMMss");

            // ②-2 経過時間
            DateTime endTime = DateTime.Now;
            TimeSpan elapsedTime = endTime - currentTime;                  // 現在時刻 - 開始時刻
            double elapsedMiliSec = elapsedTime.TotalSeconds * 1000;       // 秒⇒ミリ秒
            LocalEmotionDTO.ElapsedTime = elapsedMiliSec.ToString();

            // ②-3 元画像のパス
            LocalEmotionDTO.SrcPath = srcPath;

            // ②-4 加工後の画像のパス
            LocalEmotionDTO.ResPath = resPath;

            // ②-5 OK・NGフラグ
            LocalEmotionDTO.OK_Flg = this.myEmotion;


            
        }
    }
}

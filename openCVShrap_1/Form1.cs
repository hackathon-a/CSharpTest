using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenCvSharp;
using OpenCvSharp.UserInterface;

using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace openCVShrap_1
{
    public partial class Form1 : Form
    {

        bool isCanceled = false;
        string retStr = "";

        public Form1()
        {
            InitializeComponent();
            

        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        //static async void MakeRequest(string imageFilePath)
        static async Task<string> MakeRequest(string imageFilePath)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid key.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "f9eda06c97e24a5f8270824e2d4a112a"); //

            // NOTE: You must use the same region in your REST call as you used to obtain your subscription keys.
            //   For example, if you obtained your subscription keys from westcentralus, replace "westus" in the
            //   URI below with "westcentralus".
            //string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            string uri = "https://japaneast.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceAttributes=emotion";
            HttpResponseMessage response;
            string responseContent;

            // Request body. Try this sample with a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (var content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                //responseContent = response.Content.ReadAsStringAsync().Result;
                responseContent = await response.Content.ReadAsStringAsync();
            }



            // A peek at the raw JSON response.
            Console.WriteLine(responseContent);

            // Processing the JSON into manageable objects.
            JToken rootToken = JArray.Parse(responseContent).First;

            // タイミング問題なのか、responseContentが"[]"でになる場合があり、その場合rootTokenがnullになる
            // そのため、rootTokenがnullなら次の処理に回す
            if(rootToken == null)
            {
                return string.Empty;
            }

            // First token is always the faceRectangle identified by the API.
            JToken faceRectangleToken = rootToken.First;

            // Second token is all emotion scores.
            JToken scoresToken = rootToken.Last;

            // Show all face rectangle dimensions
            JEnumerable<JToken> faceRectangleSizeList = faceRectangleToken.First.Children();
            foreach (var size in faceRectangleSizeList)
            {
                Console.WriteLine(size);
            }

            string retStr = string.Empty;

            // Show all scores
            JEnumerable<JToken> scoreList = scoresToken.First.Children();
            foreach (var score in scoreList)
            {
                Console.WriteLine(score);
                retStr += score;
            }

            return retStr;
        }

        delegate void setResultDelegate();
        void setResult( )
        {
            this.textBox1.Text = this.retStr;
        }

        private void CaptureCamera()
        {
            VideoCapture videoCapture = OpenCvSharp.VideoCapture.FromCamera(0);
            
            if (videoCapture.Fps == 0)
            {
                videoCapture.Fps = 30;
            }

            // 現在時刻を取得
            string nowTime = DateTime.Now.ToString("yyyyMMddhhmmss");

            // ダンプ先が無ければ作る
            if (!System.IO.Directory.Exists(nowTime))
            {
                System.IO.Directory.CreateDirectory(nowTime);
            }
            int sleepTime = (int)Math.Round(1000 / videoCapture.Fps);

            // Taskで別スレッド化
            var task = Task.Run(async () =>
             {
                 try
                 {
                     //using (VideoCapture videoCapture = OpenCvSharp.VideoCapture.FromCamera(0))
                     //{
                     //    if (videoCapture.Fps == 0)
                     //    {
                     //        videoCapture.Fps = 30;
                     //    }

                     //    // 現在時刻を取得
                     //    string nowTime = DateTime.Now.ToString("yyyyMMddhhmmss");

                     //    // ダンプ先が無ければ作る
                     //    if (!System.IO.Directory.Exists(nowTime))
                     //    {
                     //        System.IO.Directory.CreateDirectory(nowTime);
                     //    }
                     //    int sleepTime = (int)Math.Round(1000 / videoCapture.Fps);

                     //using (var window = new Window("capture"))
                     {
                         // 4frameに1回ダンプする TODO:ダンプする頻度を外部設定化する

                         int frameCounter = 0;

                         // Frame image buffer
                         Mat image = new Mat();

                         // When the movie playback reaches end, Mat.data becomes NULL.
                         //while (true)
                         while (!isCanceled)
                         {
                             videoCapture.Read(image); // same as cvQueryFrame
                             if (image.Empty())
                                 break;

                             //window.ShowImage(image);

                             // 表示したフレームに番号を付ける。
                             frameCounter++;

                             // フレーム番号が120の倍数なら保存(4秒に1回)
                             //if (frameCounter % 2 != 0)
                             if (frameCounter % 120 == 0)
                             {
                                 if (this.pictureBox1.Image != null)
                                 {
                                     this.pictureBox1.Image.Dispose();
                                     this.pictureBox1.Image = null;
                                 }

                                 // TOOD：保存枚数は上限を持たせる。感情照合2回分程度ぐらいしかファイルに保存しない。古いものから削除する
                                 string imageFile = nowTime + @"\" + frameCounter.ToString() + ".jpg";
                                 Cv2.ImWrite(imageFile, image);

                                 // 画像も表示しておく
                                 this.pictureBox1.Image = Image.FromFile(imageFile);

                                 // 保存したら表情認識APIを呼ぶ
                                 this.retStr = await MakeRequest(imageFile);

                                 // 非同期でテキストボックスに結果を表示する
                                 Invoke(new setResultDelegate(setResult));

                                 // アプリ実行場所のフォルダ名取得
                                 string currentDir = System.Environment.CurrentDirectory;

                                 // 古いファイルを削除する
                                 // TODO:creationTimeでソートする
                                 //IEnumerable<string> files = System.IO.Directory.EnumerateFiles(currentDir + @"\" + nowTime + @"\");
                                 //string[] files = System.IO.Directory.GetFiles(currentDir + @"\" + nowTime + @"\");
                                 DirectoryInfo directoryInfo = new DirectoryInfo(currentDir + @"\" + nowTime + @"\");
                                 var fileSystemInfoArray = directoryInfo.GetFileSystemInfos().OrderBy(x => x.CreationTime);

                                 // 握りっぱなしだと削除できないので、解放してから個数チェックして削除する
                                 int fileCount = fileSystemInfoArray.Count();
                                 string deleteTarget = fileSystemInfoArray.FirstOrDefault().FullName;
                                 fileSystemInfoArray = null;

                                 //if (fileSystemInfoArray.Count() > 30)
                                 if (fileCount > 30)
                                 {
                                     // ファイル削除で失敗しても処理続行
                                     try
                                     {
                                         // 30ファイル(120秒分)より多ければ最も古いファイルを削除する
                                         System.IO.File.Delete(deleteTarget);
                                     }
                                     catch (System.IO.IOException)
                                     {
                                         Console.WriteLine("delete failed:" + deleteTarget);
                                     }
                                 }
                             }


                             Cv2.WaitKey(sleepTime);
                         }
                     }
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show(ex.Message);
                     throw;
                 }
             
                 
            });
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.isCanceled = false;

            // startボタンは無効化(連打抑止)
            this.button1.Enabled = false;

            // stopボタンは有効化
            this.button2.Enabled = true;

            CaptureCamera();

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.isCanceled = true;

            // startボタンは有効化
            this.button1.Enabled = true;
            // stopボタンは無効化
            this.button2.Enabled = false;
        }
    }
}

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

                // タイミング問題なのか、responseContentが"[]"でになる場合があり、その場合rootTokenがnullになる
                // そのため、rootTokenがnullなら次の処理に回す
                if (rootToken == null)
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

                // Show all scores
                JEnumerable<JToken> scoreList = scoresToken.First.Children();
                foreach (var score in scoreList)
                {
                    Console.WriteLine(score);
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

        private void CaptureCamera()
        {
            VideoCapture videoCapture = OpenCvSharp.VideoCapture.FromCamera(0);
            
            videoCapture.Fps = 30;

            // Taskで別スレッド化
            var task = Task.Run(async () =>
             {
                 try
                 {
                     //using (var window = new Window("capture"))
                     {

                         // Frame image buffer
                         Mat image = new Mat();

                         // When the movie playback reaches end, Mat.data becomes NULL.
                         while (!isCanceled)
                         {
                             videoCapture.Read(image); // same as cvQueryFrame
                             if (image.Empty())
                                 break;

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
                     videoCapture.Release();
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

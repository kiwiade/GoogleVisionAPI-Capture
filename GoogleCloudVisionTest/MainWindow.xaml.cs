using Google.Apis.Auth.OAuth2;
using Google.Apis.Vision.v1;
using Google.Apis.Services;
using Google.Apis.Vision.v1.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

using System.Drawing;
using System.Windows.Interop;
using System.Drawing.Imaging;
using Microsoft.Win32;

namespace GoogleCloudVisionTest
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string currentImagePath = "";

        private void LoadImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();

            if (openDialog.ShowDialog() == true)
            {
                if (File.Exists(openDialog.FileName))
                {
                    Stream imageStreamSource = new FileStream(openDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    BitmapSource bitmapSource = decoder.Frames[0];

                    // 이미지를 불러와서 띄워줌
                    image.Source = bitmapSource;
                    // 현재 이미지 경로를 저장
                    currentImagePath = openDialog.FileName;
                }
            }
        }

        // https://github.com/dang-gun/GoogleVisionAPITest0001
        private void VisionRequest(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentImagePath))
                return;

            //구글 api 자격증명
            GoogleCredential credential = null;

            //다운받은 '사용자 서비스 키'를 지정하여 자격증명을 만듭니다.
            using (var stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\Vision API test-61c6255b0fcc.json", FileMode.Open, FileAccess.Read))
            {
                string[] scopes = { VisionService.Scope.CloudPlatform };
                credential = GoogleCredential.FromStream(stream);
                credential = credential.CreateScoped(scopes);
            }

            //자격증명을 가지고 구글 비전 서비스를 생성합니다.
            var service = new VisionService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "google vision",
                GZipEnabled = true,
            });

            service.HttpClient.Timeout = new TimeSpan(0, 1, 0);
            //이미지를 읽어 들입니다.
            //byte[] file = File.ReadAllBytes(@"ccc.png");
            byte[] file = File.ReadAllBytes(currentImagePath);

            //분석 요청 생성
            BatchAnnotateImagesRequest batchRequest = new BatchAnnotateImagesRequest();
            batchRequest.Requests = new List<AnnotateImageRequest>();
            batchRequest.Requests.Add(new AnnotateImageRequest()
            {
                //"TEXT_DETECTION"로 설정하면 이미지에 텍스트만 추출 합니다.
                Features = new List<Feature>() { new Feature() { Type = "TEXT_DETECTION", MaxResults = 1 }, },
                ImageContext = new ImageContext() { LanguageHints = new List<string>() { "en", "ko" } },
                Image = new Google.Apis.Vision.v1.Data.Image() { Content = Convert.ToBase64String(file) }
            });

            Thread response_thread = new Thread(() => Response(service, batchRequest));
            response_thread.Start();

            //var annotate = service.Images.Annotate(batchRequest);
            ////요청 결과 받기
            //BatchAnnotateImagesResponse batchAnnotateImagesResponse = annotate.Execute();
            //if (batchAnnotateImagesResponse.Responses.Any())
            //{
            //    AnnotateImageResponse annotateImageResponse = batchAnnotateImagesResponse.Responses[0];
            //    if (annotateImageResponse.Error != null)
            //    {//에러
            //        if (annotateImageResponse.Error.Message != null)
            //            textBox.Text = annotateImageResponse.Error.Message;
            //    }
            //    else
            //    {//정상 처리
            //        textBox.Text = annotateImageResponse.TextAnnotations[0].Description.Replace("\n", "\r\n");
            //    }
            //}
        }

        private void Response(VisionService service, BatchAnnotateImagesRequest batchRequest)
        {
            var annotate = service.Images.Annotate(batchRequest);
            //요청 결과 받기
            BatchAnnotateImagesResponse batchAnnotateImagesResponse = annotate.Execute();
            if (batchAnnotateImagesResponse.Responses.Any())
            {
                AnnotateImageResponse annotateImageResponse = batchAnnotateImagesResponse.Responses[0];
                if (annotateImageResponse.Error != null)
                {//에러
                    if (annotateImageResponse.Error.Message != null)
                    {
                        string result = annotateImageResponse.Error.Message;
                        Dispatcher.BeginInvoke(new TextChangeDelegate(TextChange), result);
                    }
                }
                else
                {//정상 처리
                    string result = annotateImageResponse.TextAnnotations[0].Description.Replace("\n", "\r\n");
                    Dispatcher.BeginInvoke(new TextChangeDelegate(TextChange), result);

                    if (!isOpened)
                        Dispatcher.BeginInvoke(new OpenTextBoxDelegate(OpenTextBox));
                }
            }
        }

        private delegate void TextChangeDelegate(string text);
        private void TextChange(string text)
        {
            textBox.Text = text;
        }

        //////////////////////////////////////////////////////////////////////////////////////////

        private void Capture_Button_Click(object sender, RoutedEventArgs e)
        {
            CaptureWindow cwindow = new CaptureWindow();
            cwindow.Show();
        }

        private bool isOpened = false;

        private void openTextBox_button_Click(object sender, RoutedEventArgs e)
        {
            OpenTextBox();
        }

        private delegate void OpenTextBoxDelegate();
        public void OpenTextBox()
        {
            if (!isOpened)
            {
                isOpened = true;
                this.Width = 1150;
                openTextBox_buttonImage.Source = new BitmapImage(new Uri("Resources/left-arrow.png", UriKind.Relative));
            }
            else
            {
                isOpened = false;
                this.Width = 600;
                openTextBox_buttonImage.Source = new BitmapImage(new Uri("Resources/right-arrow.png", UriKind.Relative));
            }
        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ZoomImage zoom = new ZoomImage();
            zoom.Show();
            zoom.Width = image.Source.Width;
            zoom.Height = image.Source.Height;
            zoom.image.Source = image.Source;
        }

        private void image_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void image_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
    }  
}

# GoogleCloudVisionAPI & ScreenCapture

구글의 클라우드비전 서비스를 테스트하면서 스크린캡쳐 기능도 만들어보았습니다.

클라우드비전 코드는 MainWindow, 스크린캡쳐 코드는 CaptureWindow에 위치하고 있습니다.

## 1. GoogleCloudVisionAPI
구글 클라우드 비전 서비스는 아래 링크를 거의 참고했습니다.

https://github.com/dang-gun/GoogleVisionAPITest0001

구글 클라우드 플랫폼에 가입한 후 프로젝트를 생성하고 서비스 계정 키를 발급받아야합니다. <br>
(API 및 서비스 - 사용자 인증 정보)

MainWindow.xaml.cs의 72번째줄에서 자신의 계정키로 생성된 json파일의 경로를 설정해주시면 됩니다.

API 및 서비스의 대시보드에서 요청수 및 지연시간을 체크할 수 있습니다.

## 2. ScreenCapture

스크린 캡쳐의 끌어서 창맞추기 기능과 UI는 아래 프로그램을 많이 참고했습니다.

https://github.com/NickeManarin/ScreenToGif


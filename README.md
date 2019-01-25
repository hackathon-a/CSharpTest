# hanashikaketeiikana
話しかけていいかな？

# このテストプログラムはOpenCV4.0のインストールが必要です
インストール方法、パスの設定は下記のサイトを参照ください。
https://qiita.com/h-adachi/items/aad3401b8900438b2acd

上記サイトの中で、「構成」の中の各種パスの設定が重要です。
各種パスの設定後に、Visual Studio 2017を起動すれば、
テストプロをビルドし実行できると思います。

# テストプロの使用方法
①上記OpenCV4.0のインストール及びパスの設定を行う
②VS2017で起動しビルド
③カメラ接続環境で、「capture start」ボタンを押下するとカメラ初期化し撮影開始
　カメラはUSB接続でもノートPCのでもOK。
④4秒に1回、画像ファイルに保存し、AzureSDKのFaceAPIのEmotionアルゴを利用し、
　返却されたjson形式の値を画面右側のテキスト領域に表示する
⑤画像ファイルは、exeと同じフォルダに、フォルダ作成して保存する。
　画像用フォルダは「start capture」ボタン押下毎に作成する。
⑥「stop」ボタン押下で、撮影及び表情認識を停止する

★AzureSDKのFaceAPIの呼び出し先は渡部アカウント用に作成したサブスクリプションキーに
　なっているため、複数の場所から同時に実行すると、無料の上限(20回/分)を超えてしまうかも！？
 確認します。
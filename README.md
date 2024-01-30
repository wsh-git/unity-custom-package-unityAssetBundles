# 工程资源目录

1、在工程目录下创建`Res` 目录作为项目存放资源的目录；

2、在这个目录中只存放程序代码直接调用的资源；

3、其它间接引用或者使用到的资源会通过打包工具来打出对应的资源；

4、间接使用或引用的资源都放入到其它相关的目录；

5、此目录下可以存放Prefab、音频、图片等资源；

# AssetBundle打包

1、在`Res`下的资源都会逐一打包成对应的ab包，bundle名称是通过对该资源在项目中的文件路径（不包含后缀名）进行md5算法来命名的；

例如：
    Assets/Res/Prefab/Player.prefab
    只会对 Res/Prefab/Player 进行md5算法；

2、间接引用或使用的资源有可能在其它目录下，不一定是在`Res`目录下，bundle名称也是通过对该资源在项目中的文件路径（不包含后缀名）进行md5算法来命名的；

例如：
    Assets/Texture/Player001.png
    只会对 Texture/Player001 进行md5 算法；
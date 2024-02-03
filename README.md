# 工程资源目录

1、在工程目录下创建`Res` 目录作为项目存放资源的目录；

2、在这个目录中只存放程序代码直接调用的资源；

3、其它间接引用或者使用到的资源会通过打包工具来打出对应的资源；

4、间接使用或引用的资源都放入到其它相关的目录；

5、此目录下可以存放Prefab、音频、图片等资源；

# AssetBundle打包

## 打包
1、在`Res`下的资源都会逐一打包成对应的ab包，bundle名称是通过对该资源在项目中的文件路径（不包含后缀名）进行md5算法来命名的；

例如：
    Assets/Res/Prefab/Player.prefab
    只会对 Res/Prefab/Player 进行md5算法；

2、间接引用或使用的资源有可能在其它目录下，不一定是在`Res`目录下，bundle名称也是通过对该资源在项目中的文件路径（不包含后缀名）进行md5算法来命名的；

例如：
    Assets/Texture/Player001.png
    只会对 Texture/Player001 进行md5 算法；

## 加密解密

> 采用简单的在byte[]前增加指定数量的数字

1、加密

```C#
private static void EncryptAB(string filePath){
    byte[] fileData = File.ReadAllBytes(filePath);
    int fileLen = (AssetBundleDefine.ASSET_BUNDLE_OFFSET + fileData.Length);
    byte[] buffer = new byte[fileLen];
    for(int slen = 0; slen < AssetBundleDefine.ASSET_BUNDLE_OFFSET; slen++) {
        buffer[slen] = 1;
    }
    for(int slen = 0; slen < fileData.Length; slen++) {
        buffer[slen + AssetBundleDefine.ASSET_BUNDLE_OFFSET] = fileData[slen];
    }
    FileStream fs = File.OpenWrite(filePath);
    fs.Write(buffer, 0, fileLen);
    fs.Close();
}
```

2、解密

```csharp
AB.LoadFromFileAsync(abPath, 0, offset);
```

# 代码裁剪类的ID

https://docs.unity3d.com/Manual/ClassIDReference.html

查看要用到的类却被裁剪掉的 class id

可以在Assets目录下建一个`link.xml`文件把需要的类都加回来

```xml
<linker>
    <!-- 保留整个命名空间 -->
    <assembly fullname="Assembly-CSharp">
        <namespace fullname="YourNamespace" />
    </assembly>

    <!-- 保留特定的类 -->
    <assembly fullname="Assembly-CSharp">
        <type fullname="YourNamespace.YourClass" />
    </assembly>

    <!-- 保留类中的特定成员 -->
    <assembly fullname="Assembly-CSharp">
        <type fullname="YourNamespace.YourClass">
            <method signature="System.Void YourMethod()" />
            <field signature="System.String YourField" />
        </type>
    </assembly>
</linker>

```

例如：
```xml
<linker>
    <assembly fullname="UnityEngine">
        <type fullname="UnityEngine.BoxCollider" />
    </assembly>

    <assembly fullname="UnityEngine">
        <type fullname="UnityEngine.Canvas" />
    </assembly>

    <assembly fullname="UnityEngine.UI">
        <type fullname="UnityEngine.UI.CanvasScaler" />
    </assembly>
    
    <assembly fullname="UnityEngine.UI">
        <type fullname="UnityEngine.UI.Image" />
    </assembly>

    <assembly fullname="UnityEngine.UI">
        <type fullname="UnityEngine.UI.GraphicRaycaster" />
    </assembly>
</linker>
```

# 热更新下载流程

## 1、检测对比资源版本号

### a. 获取本地资源版本号

先检测PersistentDataPath目录下的版本号文件，如果不存在，则从AssetSteamingPath获取，打包时，在AssetSteamingPath 中有默认的资源版本号文件、对比文件、以及默认资源AssetBundle包；

### b. 获取服务器当前资源版本号

### c. 对比版本号

## 2、对比文件检测

### a. 获取本地的对比文件

先检测PersistentDataPath目录下茶渣，如果不存在，则从AssetSteamingPath获取；

### b. 下载获取服务器端的对比文件

### c. 对比两边的对比文件

### d. 获取需要下载的AssetBundle信息列表；

## 3、AssetBunble更新

### a. 根据更新列表下载文件

### b. 资源文件、对比文件覆盖到本地
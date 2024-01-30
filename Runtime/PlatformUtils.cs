using UnityEngine;

namespace Wsh.AssetBundles {
    public class PlatformUtils {
        
        public const string FILE_PREFIX = "file:///";

        public static string Platform {
            get {
#if UNITY_EDITOR
                return PlatformType.PC.ToString();
#elif UNITY_IOS
                return PlatformType.iOS.ToString();
#elif UNITY_ANDROID
                return PlatformType.Android.ToString();
#elif UNITY_WEBGL
                return PlatformType.Webgl.ToString();
#else
                return PlatformType.PC.ToString();
#endif
            }
        }

        /// <summary>
        /// UnityWebRequest 或 WWW读取，PC、Android、IOS 平台下访问需要加 ‘file:///’
        /// </summary>
        public static string PersistentDataPath {
            get { return FILE_PREFIX + Application.persistentDataPath; }
        }

        /// <summary>
        /// C# 文件流读取，在平台下不需要加‘file:///’
        /// </summary>
        public static string PersistentDataPathWithStream {
            get { return Application.persistentDataPath; }
        }

        /// <summary>
        /// UnityWebRequest 或 WWW读取，PC、IOS 平台下访问需要加 ‘file:///’,但在 Android 下不需要加前缀，可直接使用
        /// </summary>
        public static string StreamingAssetsPath {
            get {
#if UNITY_EDITOR
                return FILE_PREFIX + Application.streamingAssetsPath;
#elif UNITY_ANDROID
                return Application.streamingAssetsPath;
#else
                return FILE_PREFIX + Application.streamingAssetsPath;
#endif
            }
        }

        /// <summary>
        /// C# 文件流读取，在PC、 iOS下可直接使用，在 Android 下不可使用，如果要访问这个路径，一般是通过UnityWebRequest、WWW来访问的，不会通过 C# File Stream 来访问
        /// Android 通过加载assetbundle 路径验证通过； 
        /// </summary>
        public static string StreamingAssetsPathWithStream {
            get {
#if UNITY_EDITOR
                return Application.streamingAssetsPath;
#elif UNITY_ANDROID
                return "jar:file://" + Application.dataPath + "!/assets/";
#else
                return Application.streamingAssetsPath;
#endif
            }
        }
    }
}
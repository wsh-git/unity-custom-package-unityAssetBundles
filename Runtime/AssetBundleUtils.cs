using System;

namespace Wsh.AssetBundles {
    
    public class AssetBundleUtils {
        
        public static int GetBundleOffset(string md5, int defaultOffset = 0) {
            if(string.IsNullOrEmpty(md5) || md5.Length == 0) {
                return defaultOffset;
            }
            try {
                int number = int.Parse(md5[0].ToString());
                return number;
            } catch( FormatException) {
                return defaultOffset;
            }
        }
        
    }
    
}
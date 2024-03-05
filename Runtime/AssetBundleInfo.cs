namespace Wsh.AssetBundles {
    
    public class AssetBundleInfo {
        
        public string Name;
        public ulong Size;
        public string Md5;
        
        public AssetBundleInfo(string name, string size, string md5) {
            this.Name = name;
            this.Size = ulong.Parse(size);
            this.Md5 = md5;
        }
        
        public static bool operator ==(AssetBundleInfo a, AssetBundleInfo b) {
            if(a.Name != b.Name) {
                return false;
            }
            if(a.Size != b.Size) {
                return false;
            }
            if(a.Md5 != b.Md5) {
                return false;
            }
            return true;
        }

        public static bool operator !=(AssetBundleInfo a, AssetBundleInfo b) {
            return !(a == b);
        }
        
    }
    
}
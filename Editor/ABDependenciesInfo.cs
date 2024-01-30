using System.Collections.Generic;

namespace Wsh.AssetBundles.Editor {
    
    public class ABDependenciesInfo {

        public string Name;
        public List<string> Dependencies;

        public ABDependenciesInfo(string name) {
            Name = name;
            Dependencies = new List<string>();
        }

        public override string ToString() {
            string str = Name + '\n';;
            for(int i = 0; i < Dependencies.Count; i++) {
                str += "        -- ";
                str += Dependencies[i];
                str += '\n';
            }
            return str;
        }

        public void Sort() {
            Dependencies.Sort();
        }

        public void Add(string dependencyName) {
            Dependencies.Add(dependencyName);
        }
        
    }
}
using System.Collections.Concurrent;

namespace dropCoreKestrel
{
    public class FileCache
    {
        private Dictionary<string, byte[]> _cachedFiles;
        private int loadCOunt = 0;

        public FileCache() {
            _cachedFiles = new Dictionary<string, byte[]>();
        }

        public void Clear() {
            _cachedFiles.Clear();
        }

        public byte[] Load(string path) {
            if(_cachedFiles.ContainsKey(path)) {

                return _cachedFiles[path];
            } else {
                try {
                    var fileAsBytes = File.ReadAllBytes(path);

                    _cachedFiles.Add(path, fileAsBytes);

                    return fileAsBytes;
                } catch{
                }
            }

            throw new FileLoadException();
        }
    }
}
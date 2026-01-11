using System;
using System.IO;

namespace Test_Project.Services
{
    public class HighScoreService
    {
        private readonly string _path;

        public HighScoreService(string path)
        {
            _path = path;
        }

        public int Load()
        {
            try
            {
                if (!File.Exists(_path)) return 0;
                if (int.TryParse(File.ReadAllText(_path), out var v)) return v;
            }
            catch { }
            return 0;
        }

        public void Save(int score)
        {
            try { File.WriteAllText(_path, score.ToString()); }
            catch { }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katalog
{
    public class XMLimporter
    {
        Microsoft.Win32.OpenFileDialog fileLoader = new Microsoft.Win32.OpenFileDialog();
        public string filename { get; set; }

        public XMLimporter()
        {
            fileLoader.FileName = "Document";
            fileLoader.DefaultExt = "xls";
            fileLoader.Filter = "Office Spreadsheet(.xls)|*.xls";
            Nullable<bool> file = fileLoader.ShowDialog();
            if (file == true)
            {
                filename = fileLoader.FileName;
            }
        }
    }
}

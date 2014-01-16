using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAMS_ETL.HelperClasses
{
    public static class inputfiles
    {
        private static List<string> _inputFilenames = new List<string>();
        private static List<string> _templateFilenames = new List<string>();
        private static string _templateFolder;
        private static string _outputFolderPath;

        public static List<string> inputFilenames
        {
            get { return _inputFilenames; }
            set
            {
                if (_inputFilenames == null)
                {
                    _inputFilenames = new List<string>();
                }
                _inputFilenames = value; 
            }
        }

        public static string TemplateFolder
        {
            get { return _templateFolder; }
            set{ _templateFolder = value;}
               
        }

        public static List<string> templateFilenames
        {
            get { return _templateFilenames; }
            set 
            {
                if (_templateFilenames == null)
                {
                    _templateFilenames = new List<string>();
                }
                _templateFilenames = value; 
            }
        }

        public static string OutputFolderPath
        {
            get { return _outputFolderPath; }
            set { _outputFolderPath = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAMS_ETL.HelperClasses
{
    public class Template
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public List<string> Columns { get; set; }

        public Template()
        {
            Columns = new List<string>();
        }
    }
}

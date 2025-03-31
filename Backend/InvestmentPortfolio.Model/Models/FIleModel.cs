using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Model.Models
{
    public class FileModel
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public string FileType { get; set; } // Stores the MIME type (e.g., "application/pdf", "image/png")
    }

}

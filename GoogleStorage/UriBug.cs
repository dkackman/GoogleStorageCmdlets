using System;
using System.Management.Automation;
using System.Net;

namespace GoogleStorage
{
    [Cmdlet(VerbsDiagnostic.Debug, "UriFormatting")]

    public class UriBug : Cmdlet
    {        
        protected override void ProcessRecord()
        {
            try
            {
                var uri = "https://www.googleapis.com/download/storage/v1/b/uspto-pair/o/applications%2F05900002.zip?generation=1370956749027000&alt=media";

                WebClient c = new WebClient();
                var b = c.DownloadData(uri);
                WriteObject(string.Format("Got {0} bytes", b.Length));
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "UriBug", ErrorCategory.ReadError, null));
            }
        }
    }
}

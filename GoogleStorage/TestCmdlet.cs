using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.Generic;

namespace GoogleStorage
{
    [Cmdlet(VerbsDiagnostic.Debug, "Stuff", SupportsShouldProcess = true)]
    public class TestCmdlet : Cmdlet
    {
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            bool yesToAll = false;
            bool noToAll = false;
            for (int i = 0; i < 10; i++)
            {
                if (ShouldProcess("ShouldProcess?", "debug", "should process"))
                {
                    if (Force || ShouldContinue("ShouldContinue?", "you sure", ref yesToAll, ref noToAll))
                    {
                        WriteObject(i);
                    }
                }
            }
        }
    }
}
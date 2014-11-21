using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace GoogleStorage
{
    public abstract class GoogleStorageAuthenticatedCmdlet : PSCmdlet
    {
        public SwitchParameter NoAuth { get; set; }
    }
}

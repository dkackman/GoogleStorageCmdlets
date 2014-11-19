using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.ComponentModel;

namespace GoogleStorage
{
    [RunInstaller(true)]
    public class GoogleStorageCmdlets : PSSnapIn
    {
        /// <summary>The format file for the snapin. </summary>
        private string[] _formats = { "GoogleStorage.Format.ps1xml" };

        /// <summary>Creates an instance of DemoSnapin class.</summary>
        public GoogleStorageCmdlets()
            : base() 
        { 
        }

        /// <summary>
        /// The snapin name which is used for registration
        /// </summary>
        public override string Name
        {
            get { return "GoogleStorage"; }
        }

        /// <summary>Gets vendor of the snapin.</summary>
        public override string Vendor
        {
            get { return "Don Kackman"; }
        }

        /// <summary>Gets description of the snapin. </summary>
        public override string Description
        {
            get { return "Google Storage Cmdlets"; }
        }

        public override string[] Formats
        {
            get { return null; }
        }
    }
}

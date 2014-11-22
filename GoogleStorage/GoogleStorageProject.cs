using System;
using System.Dynamic;
using System.Management.Automation;

namespace GoogleStorage
{
    [Cmdlet(VerbsCommon.Set, "GoogleStorageProject")]
    public class SetGoogleStorageProject : PSCmdlet
    {
        public SetGoogleStorageProject()
        {
            Persist = true;
        }

        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string ProjectName { get; set; }

        [Parameter(Mandatory = false)]
        public bool Persist { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                dynamic project = new ExpandoObject();
                project.ProjectName = ProjectName;
                this.SetPersistedVariableValue("Project", (object)project, Persist);
                WriteObject(GetVariableValue(project.ProjectName, ""));
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(
                        e,
                        "SetGoogleStorageProject",
                        ErrorCategory.NotSpecified,
                        "ProjectName"));
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "GoogleStorageProject")]
    public class GetGoogleStorageProject : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                dynamic project = this.GetPersistedVariableValue("Project");

                if (project == null)
                {
                    WriteError(new ErrorRecord(
                            new InvalidOperationException("Google Storage project name not set. Call Set-GoogleStorageProject first"),
                            "GetGoogleStorageProject",
                            ErrorCategory.ObjectNotFound,
                            "ProjectName"));
                }
                else
                {
                    WriteObject(project.ProjectName);
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(
                        e,
                        "GetGoogleStorageProject",
                        ErrorCategory.NotSpecified,
                        "ProjectName"));
            }
        }
    }

    [Cmdlet(VerbsCommon.Clear, "GoogleStorageProject")]
    public class ClearGoogleStorageProject : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                this.ClearPersistedVariableValue("Project");
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(
                        e,
                        "ClearGoogleStorageProject",
                        ErrorCategory.NotSpecified,
                        ""));
            }
        }
    }
}

using System;
using System.Security;
using System.Runtime.InteropServices;
using System.Management.Automation.Runspaces;
using System.Linq;

namespace GoogleStorage
{
    static class Extensions
    {
        public static string ToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static string ToEncyptedString(this SecureString s)
        {
            Command command = new Command("ConvertFrom-SecureString");
            command.Parameters.Add("SecureString", s);
            return command.InvokeCommand<string>();
        }

        public static SecureString FromEncyptedString(this string s)
        {
            Command command = new Command("ConvertTo-SecureString");
            command.Parameters.Add("String", s);
            return command.InvokeCommand<SecureString>();
        }

        public static T InvokeCommand<T>(this Command command)
        {
            using (Runspace runSpace = RunspaceFactory.CreateRunspace())
            {
                runSpace.Open();
                using (Pipeline pipeline = runSpace.CreatePipeline())
                {
                    pipeline.Commands.Add(command);
                    return pipeline.Invoke().Select(o => o.BaseObject).Cast<T>().First();
                }
            }
        }
    }
}

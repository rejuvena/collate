using System;
using Microsoft.Build.Utilities;

namespace Rejuvena.Collate
{
    public abstract class BaseTask : Task
    {
        protected virtual ErrorPermissiveness AllowErrors => ErrorPermissiveness.NoErrors;

        public override bool Execute() {
            try {
                ExecuteTask();
                return AllowErrors > ErrorPermissiveness.NoErrors || !Log.HasLoggedErrors;
            }
            catch (Exception e) {
                Log.LogErrorFromException(e, true);
                return AllowErrors > ErrorPermissiveness.NonFatalErrors;
            }
        }

        protected abstract void ExecuteTask();
    }
}
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.Tasks
{
    public class VerifyTargetsTask : BaseTask
    {
        [Required]
        public string EnvVarName { get; set; }

        protected override void ExecuteTask() {
            Log.LogMessage("Test");
        }
    }
}
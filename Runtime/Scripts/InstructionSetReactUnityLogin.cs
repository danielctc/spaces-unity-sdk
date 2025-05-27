using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    [Title("Spaces React Login Handler")]
    [Description("Handles Spaces React Unity login and populates UI fields with user data.")]

    [Category("Spaces/Login/React Login Handler")]
    public class InstructionSetReactUnityLogin : Instruction
    {
        public override string Title => "Spaces React Login Handler";

        protected override async Task Run(Args args)
        {
            Debug.Log("[Game Creator] Starting React Unity Login Handler");
            
            // Step 1: Request user data from Firebase
            Debug.Log("[Game Creator] About to call RequestUserForUnity");
            ReactRaiseEvent.RequestUserForUnity();
            Debug.Log("[Game Creator] RequestUserForUnity called");
        }
    }
} 
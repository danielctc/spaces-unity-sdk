using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;


namespace GameCreator.Runtime.Common
{
    [Version(0, 0, 2)]
    [Title("Ignore Character Collision")]
    [Category("Characters/Physics/Ignore Character Collision")]
    [Description("Ignore Physics Character collision")]
    [Example("Use this to make the source object collision ignore or not ignore a specific character.")]
    [Keywords("Character", "Player", "Collision", "Collider", "Ignore", "Physics")]
    [Image(typeof(IconPhysics), ColorTheme.Type.Green)]
    [Serializable]
    public class InstructionIgnoreCharacterCollision : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Origin = GetGameObjectSelf.Create();
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetBool m_Ignore = new PropertyGetBool(true);

        public override string Title => $"{m_Origin} Ignoring {m_Character} Collider = {m_Ignore}";

        protected override Task Run(Args args)
        {
            GameObject self = m_Origin.Get(args);
            if(self == null) { return DefaultResult; }

            Character character = this.m_Character.Get<Character>(args);
            if (character == null) { return DefaultResult; }

            CharacterController controller = character.GetComponent<CharacterController>();
            Physics.IgnoreCollision(self.Get<Collider>(), controller, m_Ignore.Get(args));
            return DefaultResult;
        }


    }
}
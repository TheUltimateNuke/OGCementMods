using CementTools;
using Il2CppFemur;
using Il2CppInterop.Runtime.Injection;
using System;

namespace ChangePunchForce
{
    public class ChangePunchForce : CementMod
    {
        public ChangePunchForce(IntPtr ptr) : base(ptr) { }

        public ChangePunchForce() : base(ClassInjector.DerivedConstructorPointer<ChangePunchForce>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        public void Update()
        {
            float multiplier = modFile.GetFloat("PunchForceMultiplier");
            foreach (Actor actor in FindObjectsOfType<Actor>())
            {
                if (actor.ControlledBy == Actor.ControlledTypes.Human)
                {
                    actor._punchForceModifer = multiplier;
                }
            }
        }
    }
}
using CementTools;
using Il2CppFemur;
using System;

public class ChangePunchForce : CementMod
{
    public ChangePunchForce(IntPtr ptr) : base(ptr) { }

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
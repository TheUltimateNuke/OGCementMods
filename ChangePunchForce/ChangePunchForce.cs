using CementTools;
using UnityEngine;
using Femur;

public class ChangePunchForce : CementMod
{
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
using CementTools;
using Femur;

public class InfiniteGrip : CementMod
{
    private void Update()
    {
        foreach (Actor actor in FindObjectsOfType<Actor>())
        {
            if (!actor.controlHandeler.leftGrab && !actor.controlHandeler.rightGrab)
            {
                continue;
            }

            if (actor.bodyHandeler.leftGrabInteractable !=  null)
            {   
                actor.bodyHandeler.leftGrabInteractable.grabModifier = InteractableObject.Grab.Perminant;
            }

            if (actor.bodyHandeler.rightGrabInteractable !=  null)
            {   
                actor.bodyHandeler.rightGrabInteractable.grabModifier = InteractableObject.Grab.Perminant;
            }
        }
    }
}
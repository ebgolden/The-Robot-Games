public class FieldAction : Action
{
    public bool moveUp = false, moveRight = false, moveDown = false, moveLeft = false, rotateClockwise = false, rotateCounterClockwise = false, lookUp = false, lookDown = false, chargeAttachment = false, useAttachment = false;
    public Attachment pickAttachment = null;

    public override bool equals(Action action)
    {
        FieldAction fieldAction = (FieldAction)action;
        return moveUp == fieldAction.moveUp
            && moveUp == fieldAction.moveUp
            && moveRight == fieldAction.moveRight
            && moveDown == fieldAction.moveDown
            && moveLeft == fieldAction.moveLeft
            && rotateClockwise == fieldAction.rotateClockwise
            && rotateCounterClockwise == fieldAction.rotateCounterClockwise
            && lookUp == fieldAction.lookUp
            && lookDown == fieldAction.lookDown
            && chargeAttachment == fieldAction.chargeAttachment
            && useAttachment == fieldAction.useAttachment
            && pickAttachment == fieldAction.pickAttachment;
    }
}
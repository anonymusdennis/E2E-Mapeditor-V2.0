namespace NodeCanvas.BehaviourTrees;

public abstract class BTComposite : BTNode
{
	public sealed override int maxOutConnections => -1;

	public sealed override bool showCommentsBottom => false;
}

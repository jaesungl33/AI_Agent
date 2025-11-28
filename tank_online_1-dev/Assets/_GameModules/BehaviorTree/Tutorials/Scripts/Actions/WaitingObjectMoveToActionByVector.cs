using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using GDOLib.UI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitingObjectMoveTo (vector)", story: "Wait [Object] MoveTo [Position]", category: "Action", id: "44f7bf21805fe9d35426d41679997e42")]
public partial class WaitingObjectMoveToActionByVector : Action
{
    [SerializeReference] public BlackboardVariable<string> Object;
    [SerializeReference] public BlackboardVariable<Vector3> Position;
    [SerializeField] public BlackboardVariable<float> ThresholdDistance;
    private bool isReached = false;
    private Transform objectTransform;
    protected override Status OnStart()
    {
        isReached = false;
        // find object presenter by id
        ObjectPresenter[] allPresenters = UnityEngine.Object.FindObjectsByType<ObjectPresenter>(FindObjectsSortMode.None);
        ObjectPresenter targetObject = Array.Find(allPresenters, presenter => presenter.ID == Object.Value);
        if (targetObject == null)
        {
            LogFailure($"Object with ID '{Object.Value}' not found!");
            return Status.Failure;
        }
        objectTransform = targetObject.transform;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
         float distance = Vector3.Distance(objectTransform.position, Position.Value);
        if (distance < ThresholdDistance.Value) // threshold to consider as reached
        {
            isReached = true;
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}


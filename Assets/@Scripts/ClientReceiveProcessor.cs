using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientReceiveProcessor : MonoBehaviour
{
    public static Queue<Action> Actions = new Queue<Action>();

    public static void Enqueue(Action action)
    {
        lock (Actions)
        {
            Actions.Enqueue(action);
        }

    }

    void Update()
    {
        lock(Actions)
        {
            while (Actions.Count > 0 && !RivalBoard.s_isMoving)
            {
                Actions.Dequeue().Invoke();
            }
        }
    }
}

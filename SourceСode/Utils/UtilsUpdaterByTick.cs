using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace TalentsSystem.Utils;

/// <summary>
///     This class is a utility for resource-intensive operations that need to occur once every N ticks.
///     This class accepts the minimum and maximum delay times before performing operations.
///     It also accepts a list of delegates with operations.
///     It distributes the minimum and maximum times evenly between these operations.
///     For example: 
///     if three operations are passed to the class with a minimum latency of 60 ticks and a maximum of 90 ticks...
///     Then, the first operation will be performed at 60 ticks, the second at 75 ticks, and the third at 90 ticks. Afterward, the tick count resets.
///     Thus, each operation (except for the first pass) will be executed with a delay of 90 ticks, 
///     while resource-intensive operations will be performed in different frames to reduce the instantaneous load.
/// </summary>
public class UtilsUpdaterByTick
{
    protected int DelayMin;
    protected int DelayMax;
    protected int DelayCurrent;
    protected int DelayDiff;
    protected List<Action> Actions;

    public UtilsUpdaterByTick(int delayMin, int delayMax, List<Action> actions)
    {
        DelayMin = (int)MathHelper.Min(delayMin, delayMax);
        DelayMax = (int)MathHelper.Max(delayMin, delayMax);
        DelayCurrent = 0;
        if (actions.Count > 1)
            DelayDiff = (DelayMax - DelayMin) / (actions.Count - 1);
        else
        {
            DelayDiff = 0;
            DelayMin = DelayMax - 1;
        }
        Actions = actions;
    }
    public void Update()
    {
        DelayCurrent++;
        if (DelayCurrent < DelayMin)
            return;
        ////////////////////
        for (int i = 0; i < Actions.Count; i++)
            if (DelayCurrent == DelayMin + DelayDiff * i)
                Actions[i].Invoke();
        ////////////////////
        if (DelayCurrent >= DelayMax)
            DelayCurrent = 0;
    }
}

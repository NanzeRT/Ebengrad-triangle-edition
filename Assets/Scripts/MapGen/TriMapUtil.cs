using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

static class TriMapUtil
{
    public static (int, int) GetWorldPos ((int, int) origin, (int, int) localPos)
    {
        if (Math.Abs(origin.Item1) % 2 == 1)
            return (origin.Item1 - localPos.Item1, origin.Item2 - localPos.Item2);
        else
            return (origin.Item1 + localPos.Item1, origin.Item2 + localPos.Item2);
    }

    public static (int, int) GetWorldPosRev ((int, int) origin, (int, int) localPos)
    {
        if (Math.Abs(origin.Item1) % 2 == 1)
            return (origin.Item1 + localPos.Item1, origin.Item2 + localPos.Item2);
        else
            return (origin.Item1 - localPos.Item1, origin.Item2 - localPos.Item2);
    }

    public static (int, int) GetLocalPos ((int, int) origin, (int, int) target)
    {
        if (Math.Abs(origin.Item1) % 2 == 1)
            return (origin.Item1 - target.Item1, origin.Item2 - target.Item2);
        else
            return (target.Item1 - origin.Item1, target.Item2 - origin.Item2);
    }

    public static (int, int) Rot120 ((int, int) pos)
    {
        return (-2 * pos.Item2 - pos.Item1, (int) Math.Floor((float) pos.Item1 / 2));
    }

    public static (int, int) Reverse ((int, int) pos)
    {
        return (pos.Item2 * 2 + Math.Abs(pos.Item1) % 2, (int) Math.Floor((float) pos.Item1 / 2));
    }

    public static (int, int)[] AllRots ((int, int) pos)
    {
        (int, int)[] r = new (int, int)[6];
        r[0] = pos;
        r[1] = Rot120(pos);
        r[2] = Rot120(r[1]);
        r[3] = Reverse(pos);
        r[4] = Rot120(r[3]);
        r[5] = Rot120(r[4]);
        return r;
    }

    public static Dictionary<(int, int), ushort[]>[] AllRotsRule (Dictionary<(int, int), ushort[]> rule)
    {
        Dictionary<(int, int), ushort[]>[] r = new Dictionary<(int, int), ushort[]>[6];
        for (byte i = 0; i < 6; i++) r[i] = new Dictionary<(int, int), ushort[]>();

        foreach (var pair in rule)
        {
            var rr = AllRots(pair.Key);
            for (byte i = 0; i < 6; i++)
            {
                r[i][rr[i]] = pair.Value;
            }
        }

        return r;
    }
}
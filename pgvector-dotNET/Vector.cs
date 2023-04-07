
namespace PgVectors.NET;

public class PgVector : IEquatable<PgVector>
{
    private float[] _pgVector;

    public PgVector(float[] v)
    {
        _pgVector = v;
    }

    public PgVector(string s)
    {
        _pgVector = Array.ConvertAll(s.Substring(1, s.Length - 2).Split(","), v => float.Parse(v));
    }

    public override string ToString()
    {
        return string.Concat("[", string.Join(",", _pgVector), "]");
    }

    public float[] ToArray()
    {
        return _pgVector;
    }

    public bool Equals(PgVector? other)
    {
        return other! == this;
    }

    public static bool operator == (PgVector x, PgVector y)
    {
        var xx = x.ToArray();
        var yy = y.ToArray();

        var r = xx.Length == yy.Length;
        if (r)
        {
            r = xx.SequenceEqual(yy);
        }
        return r;
    }
    
    public static bool operator != (PgVector x, PgVector y)
    {
        return !(x == y);
    }

    //public override int GetHashCode()
    //{
    //    return ToString().GetHashCode();
    //}

    public override bool Equals(object? @object)
    {
        return Equals(@object as PgVector);
    }
}

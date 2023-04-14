using System.Numerics;
namespace Pgvector;

public partial class Vector : IEquatable<Vector>
{
    public static bool operator == (Vector x, Vector y)
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
    
    public static bool operator != (Vector x, Vector y)
    {
        return !(x == y);
    }

    public override int GetHashCode()
    {

        return ToString().GetHashCode();
        //(
        //    this
        //    -
        //    new PgVector
        //            (
        //                vec
        //                    .Select
        //                        (
        //                            (x) =>
        //                            {
        //                                return 0.0f;
        //                            }
        //                        )
        //                    .ToArray()
        //            )
        //)
        //.GetHashCode();
    }

    public override bool Equals(object? @object)
    {
        return Equals(@object as Vector);
    }


    public static double operator - (Vector x, Vector y)
    {
        return
            x.GetEuclideanDistanceWith(y);
    }


    public double GetEuclideanDistanceWith(Vector other)
    {
        var r = vec
                    .Zip
                        (
                            other
                                .vec
                            , (xi, xj) =>
                            {
                                return
                                    Math
                                        .Pow(xi - xj , 2.0)
                                    ;
                            }
                        )
                    .Sum();
        return
            Math
                .Sqrt
                    (
                        r
                    );
    }

    public double GetMagnitude()
    {
        return
            Math
                .Sqrt
                    (
                        vec
                            .Select
                                (
                                    (x) =>
                                    {
                                        return
                                            Math.Pow(x, 2.0);
                                    }
                                )
                            .Sum()
                    );
    }
    public double GetDotProductWith(Vector other)
    {
        return
            vec
                .Zip
                    (
                        other
                            .vec
                        , (xi, xj) =>
                        {
                            return
                                xi * xj;
                        }
                    )
                .Sum();
    }

    public double GetCosineDistanceWith(Vector other)
    {
        double dotProduct = GetDotProductWith(other);
        double magnitudeThis = GetMagnitude();
        double magnitudeOther = other.GetMagnitude();
        return
            dotProduct/(magnitudeThis * magnitudeOther);
    }

    public bool Equals(Vector? other)
    {
        return
            vec
                .SequenceEqual
                    (
                        other!.vec
                    );
    }
}

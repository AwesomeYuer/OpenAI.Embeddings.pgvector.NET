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

    public float GetDotProductWith(Vector other)
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

    public double GetCosineSimilarityWith(Vector other)
    {
        var query = vec
                        .Zip
                            (
                                other
                                    .vec
                                , (xThis, xOther) =>
                                {
                                    var product = xThis * xOther;
                                    var powThis = Math.Pow(xThis, 2.0);
                                    var powOther = Math.Pow(xOther, 2.0);
                                    return
                                        (
                                              product
                                            , powThis
                                            , powOther
                                        );
                                }
                            );
        double dotProduct = 0.0f;
        double sumOfPowThis = 0.0f;
        double sumOfPowOther = 0.0f;
        foreach (var x in query) 
        {
            dotProduct += x.product;
            sumOfPowThis += x.powThis;
            sumOfPowOther += x.powOther;
        }
        var magnitudeThis = Math.Sqrt(sumOfPowThis);
        var magnitudeOther = Math.Sqrt(sumOfPowOther);
        var result = 0.0d;
        if
            (
                magnitudeThis != 0.0
                &&
                magnitudeOther != 0.0
            )
        {
            result =
                    dotProduct
                    /
                    (Math.Sqrt(sumOfPowThis) * Math.Sqrt(sumOfPowOther));
        }
        return result;
    }

    public double GetCosineDistanceWith(Vector other)
    {
        return 1 - GetCosineSimilarityWith(other);
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

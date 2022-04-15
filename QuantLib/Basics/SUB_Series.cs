// SUB - subtracting TSeries-TSeries, or TSeries-double, or double-TSeries
using System;
namespace QuantLib;

public class SUB_Series : TSeries
{
    readonly TSeries _d1, _d2;
    readonly double _dd;
    readonly byte _type;

    public SUB_Series(TSeries d1, TSeries d2)
    {
        this._d1 = d1;
        this._d2 = d2;
        this._dd = 0;
        this._type = 1;
        d1.Pub += this.Sub;
        d2.Pub += this.Sub;
        if (d1.Count > 0 && d2.Count > 0)
        {
            for (int i = 0; i < Math.Min(d1.Count, d2.Count); i++)
            {
                this.Add(d1[i], d2[i], false);
            }
        }
    }

    public SUB_Series(TSeries d1, double dd)
    {
        this._d1 = d1;
        this._d2 = d1;
        this._dd = dd;
        this._type = 2;
        d1.Pub += this.Sub;
        if (d1.Count > 0)
        {
            for (int i = 0; i < d1.Count; i++)
            {
                this.Add(d1[i], dd, false);
            }
        }
    }

    public SUB_Series(double dd, TSeries d1)
    {
        this._d1 = d1;
        this._d2 = d1;
        this._dd = dd;
        this._type = 3;
        d1.Pub += this.Sub;
        if (d1.Count > 0)
        {
            for (int i = 0; i < d1.Count; i++)
            {
                this.Add(dd, d1[i], false);
            }
        }
    }

    public void Add((System.DateTime t, double v) d1,
                    (System.DateTime t, double v) d2, bool update = false)
    {
        (System.DateTime t, double v) result =
            ((d1.t > d2.t) ? d1.t : d2.t, d1.v - d2.v);
        if (update)
        {
            base[base.Count - 1] = result;
        }
        else
        {
            base.Add(result);
        }
    }

    public void Add((System.DateTime t, double v) d1, double dd,
                    bool update = false)
    {
        (System.DateTime t, double v) result = (d1.t, d1.v - dd);
        if (update)
        {
            base[base.Count - 1] = result;
        }
        else
        {
            base.Add(result);
        }
    }

    public void Add(double dd, (System.DateTime t, double v) d1,
                    bool update = false)
    {
        (System.DateTime t, double v) result = (d1.t, dd - d1.v);
        if (update)
        {
            base[base.Count - 1] = result;
        }
        else
        {
            base.Add(result);
        }
    }

    public void Add(bool update = false)
    {
        if (update || (this._d1.Count > 0 && this._d1.Count == this._d2.Count &&
                       this.Count != this._d1.Count))
        {
            if (this._type == 1)
            {
                this.Add(this._d1[this._d1.Count - 1], this._d2[this._d2.Count - 1],
                         update);
            }
            else if (this._type == 2)
            {
                this.Add(this._d1[this._d1.Count - 1], this._dd, update);
            }
            else
            {
                this.Add(this._dd, this._d1[this._d1.Count - 1], update);
            }
        }
    }

    public new void Sub(object source, TSeriesEventArgs e) { this.Add(e.update); }
}
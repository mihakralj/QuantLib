namespace QuanTAlib;
using System;

/* <summary>
MIDPOINT: Midpoint value (max+min)/2 in the given period in the series.
    If period = 0 => period = full length of the series

Sources:
    https://thefaqblog.com/what-is-the-midpoint-in-statistics/

</summary> */

public class MIDPOINT_Series : Single_TSeries_Indicator
{
	public MIDPOINT_Series(TSeries source, int period, bool useNaN = false) : base(source, period, useNaN)
	{
		if (base._data.Count > 0)
		{ base.Add(base._data); }
	}
	private readonly System.Collections.Generic.List<double> _buffer = new();

	public override void Add((DateTime t, double v) TValue, bool update)
	{
		if (update)
		{ this._buffer[this._buffer.Count - 1] = TValue.v; }
		else
		{ this._buffer.Add(TValue.v); }
		if (this._buffer.Count > this._p && this._p != 0)
		{ this._buffer.RemoveAt(0); }

		double _max = TValue.v;
		double _min = TValue.v;
		for (int i = 0; i < this._buffer.Count; i++)
		{
			_max = Math.Max(this._buffer[i], _max);
			_min = Math.Min(this._buffer[i], _min);
		}
		double _mid = (_max + _min) * 0.5;

		var result = (TValue.t, this.Count < this._p - 1 && this._NaN ? double.NaN : _mid);

		base.Add(result, update);
	}
}
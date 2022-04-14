﻿/**
PVAR: Population Variance

Population variance....

Sources:
  https://en.wikipedia.org/wiki/Variance
  Bessel's correction: https://en.wikipedia.org/wiki/Bessel%27s_correction

Remark:
  PVAR (Population Variance) is also known as a biased Sample Variance. For unbiased
  sample variance use SVAR instead.
    
**/

using System;
namespace QuantLib;

public class PVAR_Series : Single_TSeries_Indicator
{
    public PVAR_Series(TSeries source, int period, bool useNaN = false) : base(source, period, useNaN)
    {
        if (base._data.Count > 0) { base.Add(base._data); }
    }
    private readonly System.Collections.Generic.List<double> _buffer = new();

    public override void Add((System.DateTime t, double v) d, bool update)
    {
        if (update) { _buffer[_buffer.Count - 1] = d.v; }
        else { _buffer.Add(d.v); }
        if (_buffer.Count > this._p && this._p != 0) { _buffer.RemoveAt(0); }

        double _sma = 0;
        for (int i = 0; i < _buffer.Count; i++) { _sma += _buffer[i]; }
        _sma /= this._buffer.Count;

        double _pvar = 0;
        for (int i = 0; i < _buffer.Count; i++) { _pvar += (_buffer[i] - _sma) * (_buffer[i] - _sma); }
        _pvar /= this._buffer.Count;

        var result = (d.t, (this.Count < this._p - 1 && this._NaN) ? double.NaN : _pvar);
        base.Add(result, update);
    }
}
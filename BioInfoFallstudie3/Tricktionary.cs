using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioInfoFallstudie3;

/// <summary>
/// Shorthand for Dictionary<TOuterKey, Dictionary<TInnerKey, TValue>>
/// </summary>
/// <typeparam name="TOuterKey"></typeparam>
/// <typeparam name="TInnerKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class Tricktionary<TOuterKey, TInnerKey, TValue> : Dictionary<TOuterKey, Dictionary<TInnerKey, TValue>> { }

public interface ITricktionary<TOuterKey, TInnerKey, TValue> : IDictionary<TOuterKey, IDictionary<TInnerKey, TValue>> { }

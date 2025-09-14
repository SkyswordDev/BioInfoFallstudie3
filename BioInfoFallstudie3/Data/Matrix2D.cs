using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioInfoFallstudie3.Data;

public record Matrix2D<T>
{
    public readonly ImmutableArray<ImmutableArray<T>> _Data;

    public readonly int ColumnCount;
    public readonly int RowCount;

    public T this[int columnX, int rowY] => _Data[columnX][rowY];

    public Matrix2D(ImmutableArray<ImmutableArray<T>> data)
    {
        this.ColumnCount = data.Length;
        if (ColumnCount == 0)
            throw new ArgumentException($"The Matrix must not be empty but {data.Length} was 0.");

        this.RowCount = data[0].Length;
        for(int i = 1; i < ColumnCount; i++)
        {
            if (data[i].Length != RowCount)
                throw new ArgumentException($"Row count mismatch: Column at index {i} had a length of {data[i].Length} while all previous columns hat a length of {RowCount}.");
        }

        this._Data = data;
    }

}

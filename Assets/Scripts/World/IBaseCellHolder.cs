using System.Collections.Generic;

namespace World {
  public interface IBaseCellHolder {
    void SetBaseCells(List<CellData> cells);

    void ClearBaseCells();
  }
}
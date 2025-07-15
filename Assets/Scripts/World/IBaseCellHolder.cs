using System.Collections.Generic;

namespace World {
  public interface IBaseCellHolder {
    void SetBaseCells(List<CellObject> cells);

    void ClearBaseCells();
  }
}
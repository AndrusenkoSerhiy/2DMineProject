using System.Collections.Generic;

namespace World {
  public interface IBaseCellHolder {
    bool CanDestroyCellsBelow { get; set; }
    void SetBaseCells(List<CellData> cells);

    void ClearBaseCells();
  }
}
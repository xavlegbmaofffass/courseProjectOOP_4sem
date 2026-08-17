namespace FrontlineCardWarfare.Models;

/// <summary>
/// Представляет игровое поле 4×4.
/// </summary>
public class Board
{
    /// <summary>
    /// Количество рядов.
    /// </summary>
    public const int Rows = 4;

    /// <summary>
    /// Количество колонок.
    /// </summary>
    public const int Columns = 4;

    /// <summary>
    /// Клетки поля.
    /// </summary>
    public BoardCell[,] Cells { get; set; }

    /// <summary>
    /// Инициализирует новое игровое поле.
    /// </summary>
    public Board()
    {
        Cells = new BoardCell[Rows, Columns];
        InitializeBoard();
    }

    /// <summary>
    /// Инициализирует поле.
    /// </summary>
    private void InitializeBoard()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                Cells[row, col] = new BoardCell
                {
                    Row = row,
                    Column = col,
                    IsPlayerSide = row >= 2 // Первые 2 ряда - враг, последние 2 - игрок
                };
            }
        }
    }

    /// <summary>
    /// Получает клетку по координатам.
    /// </summary>
    public BoardCell? GetCell(int row, int column)
    {
        if (IsValidPosition(row, column))
        {
            return Cells[row, column];
        }
        return null;
    }

    /// <summary>
    /// Проверяет, является ли позиция допустимой.
    /// </summary>
    public bool IsValidPosition(int row, int column)
    {
        return row >= 0 && row < Rows && column >= 0 && column < Columns;
    }

    /// <summary>
    /// Размещает юнита на клетке.
    /// </summary>
    public bool PlaceUnit(Unit unit, int row, int column)
    {
        if (!IsValidPosition(row, column))
            return false;

        if (!Cells[row, column].IsEmpty)
            return false;

        Cells[row, column].Unit = unit;
        unit.Row = row;
        unit.Column = column;
        return true;
    }

    /// <summary>
    /// Удаляет юнита с клетки.
    /// </summary>
    public Unit? RemoveUnit(int row, int column)
    {
        if (!IsValidPosition(row, column))
            return null;

        var unit = Cells[row, column].Unit;
        Cells[row, column].Unit = null;
        return unit;
    }

    /// <summary>
    /// Перемещает юнита на новую клетку.
    /// </summary>
    public bool MoveUnit(int fromRow, int fromColumn, int toRow, int toColumn)
    {
        if (!IsValidPosition(fromRow, fromColumn) || !IsValidPosition(toRow, toColumn))
            return false;

        var unit = Cells[fromRow, fromColumn].Unit;
        if (unit == null)
            return false;

        if (!Cells[toRow, toColumn].IsEmpty)
            return false;

        Cells[fromRow, fromColumn].Unit = null;
        Cells[toRow, toColumn].Unit = unit;
        unit.Row = toRow;
        unit.Column = toColumn;
        return true;
    }

    /// <summary>
    /// Получает всех юнитов игрока.
    /// </summary>
    public List<Unit> GetPlayerUnits(bool isPlayer)
    {
        var units = new List<Unit>();
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var unit = Cells[row, col].Unit;
                if (unit != null && unit.IsPlayer == isPlayer)
                {
                    units.Add(unit);
                }
            }
        }
        return units;
    }

    /// <summary>
    /// Получает всех живых юнитов.
    /// </summary>
    public List<Unit> GetAllAliveUnits()
    {
        var units = new List<Unit>();
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var unit = Cells[row, col].Unit;
                if (unit != null && unit.IsAlive)
                {
                    units.Add(unit);
                }
            }
        }
        return units;
    }

    /// <summary>
    /// Создаёт копию поля.
    /// </summary>
    public Board Clone()
    {
        var board = new Board();
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                board.Cells[row, col] = Cells[row, col].Clone();
            }
        }
        return board;
    }

    /// <summary>
    /// Проверяет, есть ли юниты у игрока на поле.
    /// </summary>
    public bool HasUnits(bool isPlayer)
    {
        return GetPlayerUnits(isPlayer).Count > 0;
    }

    /// <summary>
    /// Получает следующий уникальный ID для юнита.
    /// </summary>
    public int GetNextUnitId()
    {
        int maxId = 0;
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var unit = Cells[row, col].Unit;
                if (unit != null && unit.Id > maxId)
                {
                    maxId = unit.Id;
                }
            }
        }
        return maxId + 1;
    }

    /// <summary>
    /// Получает юнита по ID.
    /// </summary>
    public Unit? GetUnitById(int unitId)
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var unit = Cells[row, col].Unit;
                if (unit != null && unit.Id == unitId)
                {
                    return unit;
                }
            }
        }
        return null;
    }
}

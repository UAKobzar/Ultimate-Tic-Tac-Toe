﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        //Tester.Train();
        string[] inputs;

        Referee.GameReferee.Reset();
        GameTree tree = null;
        // game loop
        bool firstStep = true;
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int opponentRow = int.Parse(inputs[0]);
            int opponentCol = int.Parse(inputs[1]);
            var oponentMove = new Tuple<int, int>(opponentRow, opponentCol);
            int validActionCount = int.Parse(Console.ReadLine());
            int row = 0, col = 0;
            for (int i = 0; i < validActionCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                row = int.Parse(inputs[0]);
                col = int.Parse(inputs[1]);
            }

            Referee.GameReferee.Move(opponentRow, opponentCol);

            if (firstStep)
            {
                tree = new GameTree(oponentMove, Referee.GameReferee._field, 6 - Referee.GameReferee._field.Player);
                tree.Simulate(980);
                firstStep = false;
            }
            else
            {
                tree.Update(oponentMove, Referee.GameReferee._field);
                tree.Simulate(80);
            }
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            var move = tree.GetNextStep();
            if (move == null)
            {
                move = new Tuple<int, int>(row, col);
            }

            //Console.Error.WriteLine($"{tree._startLeaf.Move.Item1}:{tree._startLeaf.Move.Item2} ");

            //foreach (var m in tree._startLeaf.SubLeafs)
            //{
            //    Console.Error.Write($"{m.Move.Item1}:{m.Move.Item2} ");
            //}

            //Console.Error.WriteLine();

            //foreach (var m in Referee.GameReferee.GetNextMoves())
            //{
            //    Console.Error.Write($"{m.Item1}:{m.Item2} ");
            //}

            var test = Referee.GameReferee.Move(move.Item1, move.Item2);

            Console.Error.WriteLine(tree._startLeaf.Simulations);

            tree.Update(move, Referee.GameReferee._field);

            Console.WriteLine($"{move.Item1} {move.Item2}");
        }
    }
}

static class BoardCells
{
    public static int Tic
    {
        get
        {
            return 2;
        }
    }

    public static int Tac
    {
        get
        {
            return 4;
        }
    }

    public static int Empty
    {
        get
        {
            return 1;
        }
    }
}

class Referee
{
    public BigGameField _field;

    private Tuple<int, int> _lastMove;

    #region Singleton

    private static Referee _simulating = null;

    public static Referee Simulating
    {
        get
        {
            if (_simulating == null)
                _simulating = new Referee();

            return _simulating;
        }
    }

    private static Referee _gameReferee = null;

    public static Referee GameReferee
    {
        get
        {
            if (_gameReferee == null)
                _gameReferee = new Referee();

            return _gameReferee;
        }
    }

    private Referee()
    {
        _field = new BigGameField();
    }
    #endregion

    public void Reset()
    {
        _lastMove = null;
        _field.Reset();
    }

    public void Reset(SmallGameField[,] newField, int player, Tuple<int, int> lastMove)
    {
        _field.Reset(newField, player);
        if (lastMove.Item1 != -1)
            _lastMove = lastMove;
    }

    public bool Move(int row, int col)
    {
        if (IsValidMove(row, col))
        {
            _field.Move(row, col);
            if (row != -1)
                _lastMove = new Tuple<int, int>(row, col);
            return true;
        }
        return false;
    }

    public List<Tuple<int, int>> GetNextMoves()
    {
        List<Tuple<int, int>> result = new List<Tuple<int, int>>();
        if (_lastMove != null && _field.Field[_lastMove.Item1 % 3, _lastMove.Item2 % 3].Status == BoardStatus.InProgress)
        {
            var playField = _field.Field[_lastMove.Item1 % 3, _lastMove.Item2 % 3];
            Tuple<int, int> playFieldCoord = new Tuple<int, int>(_lastMove.Item1 % 3, _lastMove.Item2 % 3);
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (playField.Field[row, col] == BoardCells.Empty)
                        result.Add(new Tuple<int, int>(playFieldCoord.Item1 * 3 + row, playFieldCoord.Item2 * 3 + col));
                }
            }
        }

        else
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (_field.Field[row / 3, col / 3].Status == BoardStatus.InProgress && _field.Field[row / 3, col / 3].Field[row % 3, col % 3] == BoardCells.Empty)
                        result.Add(new Tuple<int, int>(row, col));
                }
            }
        }
        return result;
    }

    private bool IsValidMove(int row, int col)
    {
        bool result = row >= 0 && row < 9 && col >= 0 && col < 9;
        if (result && _lastMove != null && _field.Field[_lastMove.Item1 % 3, _lastMove.Item2 % 3].Status == BoardStatus.InProgress)
        {
            result = result && row / 3 == _lastMove.Item1 % 3 && col / 3 == _lastMove.Item2 % 3;
        }
        result = result && _field.Field[row / 3, col / 3].Field[row % 3, col % 3] == BoardCells.Empty;
        return result;
    }

    public BoardStatus GetStatus()
    {
        return _field.Status;
    }

}

class SmallGameField
{
    private int[,] _field;

    public int[,] Field
    {
        get
        {
            return _field;
        }
    }

    private BoardStatus _status;

    public BoardStatus Status
    {
        get
        {
            _status = GetStatus();
            _statusUpdated = true;
            return _status;
        }
    }

    public int Score { get; private set; }

    private int CalculateScore(int player)
    {
        var status = GetStatus();
        if (status != BoardStatus.InProgress)
            return 0;

        return player == BoardCells.Tic ?  Heuristic.GetScore(_field) : -Heuristic.GetScore(_field);

    }

    private bool _statusUpdated = false;

    private BoardStatus GetStatus()
    {
        if (_statusUpdated)
            return _status;
        bool isDraw = true;
        int sum = 0;
        for (int row = 0; row < 3; row++)
        {
            sum = 0;
            for (int col = 0; col < 3; col++)
            {
                if (_field[row, col] == BoardCells.Empty)
                {
                    isDraw = false;
                }
                sum = sum | _field[row, col];
            }

            if (sum == BoardCells.Tic)
                return BoardStatus.TicWin;
            if (sum == BoardCells.Tac)
                return BoardStatus.TacWin;
        }

        for (int col = 0; col < 3; col++)
        {
            sum = 0;
            for (int row = 0; row < 3; row++)
            {
                if (_field[row, col] == BoardCells.Empty)
                {
                    isDraw = false;
                }
                sum = sum | _field[row, col];
            }
            if (sum == BoardCells.Tic)
                return BoardStatus.TicWin;
            if (sum == BoardCells.Tac)
                return BoardStatus.TacWin;
        }

        sum = 0;
        for (int row = 0, col = 0; row < 3; col = ++row)
        {
            if (_field[row, col] == BoardCells.Empty)
            {
                isDraw = false;
            }
            sum = sum | _field[row, col];
        }

        if (sum == BoardCells.Tic)
            return BoardStatus.TicWin;
        if (sum == BoardCells.Tac)
            return BoardStatus.TacWin;

        sum = 0;
        for (int row = 0, col = 2; row < 3; row++)
        {
            if (_field[row, col] == BoardCells.Empty)
            {
                isDraw = false;
            }
            sum = sum | _field[row, col];
            col--;
        }

        if (sum == BoardCells.Tic)
            return BoardStatus.TicWin;
        if (sum == BoardCells.Tac)
            return BoardStatus.TacWin;

        if (isDraw)
            return BoardStatus.Draw;

        return BoardStatus.InProgress;
    }

    public SmallGameField()
    {
        _field = new int[3, 3];

        Reset();
    }

    public void Reset()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                _field[row, col] = BoardCells.Empty;
            }
        }
        _statusUpdated = false;
    }

    public void Reset(int[,] newField)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                _field[row, col] = newField[row, col];
            }
        }
        _statusUpdated = false;
    }

    public void Move(int row, int col, int player)
    {
        _field[row, col] = player;
        _statusUpdated = false;
        Score = CalculateScore(player);
    }
}

class BigGameField
{
    private SmallGameField[,] _field;

    public SmallGameField[,] Field
    {
        get
        {
            return _field;
        }
    }

    private int _player;

    public int Player
    {
        get
        {
            return _player;
        }
    }

    public BoardStatus Status
    {
        get
        {
            bool isDraw = true;
            int sum = 0;
            for (int row = 0; row < 3; row++)
            {
                sum = 0;
                for (int col = 0; col < 3; col++)
                {
                    if (_field[row, col].Status == BoardStatus.InProgress)
                    {
                        isDraw = false;
                    }
                    sum = sum | (int)_field[row, col].Status;
                }

                if (sum == BoardCells.Tic)
                    return BoardStatus.TicWin;
                if (sum == BoardCells.Tac)
                    return BoardStatus.TacWin;
            }

            for (int col = 0; col < 3; col++)
            {
                sum = 0;
                for (int row = 0; row < 3; row++)
                {
                    if (_field[row, col].Status == BoardStatus.InProgress)
                    {
                        isDraw = false;
                    }
                    sum = sum | (int)_field[row, col].Status;
                }
                if (sum == BoardCells.Tic)
                    return BoardStatus.TicWin;
                if (sum == BoardCells.Tac)
                    return BoardStatus.TacWin;
            }

            sum = 0;
            for (int row = 0, col = 0; row < 3; col = ++row)
            {
                if (_field[row, col].Status == BoardStatus.InProgress)
                {
                    isDraw = false;
                }
                sum = sum | (int)_field[row, col].Status;
            }

            if (sum == BoardCells.Tic)
                return BoardStatus.TicWin;
            if (sum == BoardCells.Tac)
                return BoardStatus.TacWin;

            sum = 0;
            for (int row = 0, col = 2; row < 3; row++)
            {
                if (_field[row, col].Status == BoardStatus.InProgress)
                {
                    isDraw = false;
                }
                sum = sum | (int)_field[row, col].Status;
                col--;
            }

            if (sum == BoardCells.Tic)
                return BoardStatus.TicWin;
            if (sum == BoardCells.Tac)
                return BoardStatus.TacWin;

            if (isDraw)
            {
                int tacFields = 0;
                int ticFields = 0;
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (_field[row, col].Status == BoardStatus.TicWin)
                            ticFields++;
                        else
                            tacFields++;
                    }
                }
                if (ticFields > tacFields)
                    return BoardStatus.TicWin;
                if (tacFields > ticFields)
                    return BoardStatus.TacWin;
                return BoardStatus.Draw;
            }

            return BoardStatus.InProgress;
        }
    }

    public BigGameField()
    {
        _field = new SmallGameField[3, 3];
        Reset();
    }

    public void Reset()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (_field[row, col] == null)
                {
                    _field[row, col] = new SmallGameField();
                }
                else
                {
                    _field[row, col].Reset();
                }
            }
        }
        _player = BoardCells.Tic;
    }

    public void Reset(SmallGameField[,] field, int player)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (_field[row, col] == null)
                {
                    _field[row, col] = new SmallGameField();
                }
                _field[row, col].Reset(field[row, col].Field);
            }
        }

        _player = player;
    }

    public int Score { get; private set; }

    private int CalculateScore(int player)
    {
        var status = Status;
        int result = 0;

        if (status != BoardStatus.InProgress)
            return Heuristic.WinScore;

        int[,] bigBoard = new int[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                bigBoard[i, j] = (int)_field[i, j].Status;
                result += _field[i, j].Score;
            }
        }

        result = _player == BoardCells.Tic ? Heuristic.GetScore(bigBoard) * 23 : Heuristic.GetScore(bigBoard) * -23;

        return result;
    }

    public void Move(int row, int col)
    {
        _field[row / 3, col / 3].Move(row % 3, col % 3, _player);
        _player = 6 - _player;

        Score = CalculateScore(6 - _player);
    }

    public int GetMoveScore(int row, int col)
    {
        _field[row / 3, col / 3].Move(row % 3, col % 3, _player);
        var score = CalculateScore(_player);
        _field[row / 3, col / 3].Move(row % 3, col % 3, BoardCells.Empty);
        return score;
    }
}

class Leaf
{
    public int Player { get; set; }
    public uint Wins { get; set; }
    public uint Simulations { get; set; }
    public Tuple<int, int> Move { get; set; }
    public Tuple<int, int> OponentMove { get; set; }
    public List<Leaf> SubLeafs { get; set; }
    public Leaf Parent { get; set; }
    public bool IsEnd { get; set; }
}

class SimWraper
{
    public uint Simulations { get; set; }
    public uint Wins { get; set; }
}

class GameTree
{
    public BigGameField _field;

    private uint _simulations = 0;

    public Leaf _startLeaf;

    private const double _epsilon = 1e-100;

    private Random rand;

    private SimWraper[,] _simulationsField;

    public GameTree(Tuple<int, int> move, BigGameField field, int player)
    {
        rand = new Random();
        _startLeaf = new Leaf()
        {
            Move = move,
            Player = player
        };

        _field = new BigGameField();
        _field.Reset(field.Field, _field.Player);
        _simulationsField = new SimWraper[9, 9];
        ResetSimulationsField();
    }

    private void ResetSimulationsField()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                _simulationsField[i, j] = new SimWraper()
                {
                    Simulations = 0,
                    Wins = 0
                };
            }
        }
    }

    public void Update(Tuple<int, int> move, BigGameField field)
    {
        var leaf = _startLeaf.SubLeafs.FirstOrDefault(l => l.Move.Item1 == move.Item1 && l.Move.Item2 == move.Item2);

        if (leaf == null)
        {
            _startLeaf = new Leaf()
            {
                Move = move,
                Player = 6 - _startLeaf.Player
            };
        }
        else
        {
            _startLeaf = leaf;
        }

        _field.Reset(field.Field, field.Player);
    }

    public Tuple<int, int> GetNextStep()
    {
        var max = _startLeaf.SubLeafs.Max(l => l.Simulations);
        return _startLeaf.SubLeafs.FirstOrDefault(l => l.Simulations == max)?.Move;
    }

    public void Simulate(long time)
    {
        Stopwatch watch = new Stopwatch();

        watch.Start();

        while (watch.ElapsedMilliseconds <= time)
        {
            Simulate();
        }
    }

    private void Simulate()
    {
        Referee.Simulating.Reset(_field.Field, 6 - _startLeaf.Player, _startLeaf.Move);
        var leaf = Select(_startLeaf);
        Expand(leaf);
        leaf = leaf.SubLeafs != null && leaf.SubLeafs.Count > 0 ? leaf.SubLeafs[rand.Next() % leaf.SubLeafs.Count] : leaf;
        var status = RollOut(leaf);
        BackPropagate(leaf, status);

        _simulations = _startLeaf.Simulations;
    }

    private Leaf Select(Leaf start)
    {
        if (start.IsEnd || start.SubLeafs == null || start.SubLeafs.Count == 0)
            return start;

        var maxValue = Double.MinValue;
        Leaf nextLeaf = null;

        foreach (var subLeaf in start.SubLeafs)
        {
            var uct = CalculateUCT(subLeaf);
            if (uct > maxValue)
            {
                maxValue = uct;
                nextLeaf = subLeaf;
            }
        }
        Referee.Simulating.Move(nextLeaf.Move.Item1, nextLeaf.Move.Item2);
        return Select(nextLeaf);
    }

    private void Expand(Leaf leaf)
    {
        if (leaf.IsEnd)
            return;
        var moves = Referee.Simulating.GetNextMoves();
        if (moves.Count == 0)
        {
            leaf.IsEnd = true;
            return;
        }
        leaf.SubLeafs = new List<Leaf>();

        foreach (var move in moves)
        {
            leaf.SubLeafs.Add(new Leaf()
            {
                Move = move,
                Player = 6 - leaf.Player,
                OponentMove = leaf.Move,
                Parent = leaf
            });
        }
    }

    private BoardStatus RollOut(Leaf leaf)
    {
        var status = BoardStatus.InProgress;
        while ((status = Referee.Simulating.GetStatus()) == BoardStatus.InProgress)
        {
            var moves = Referee.Simulating.GetNextMoves();

            var nextMove = StandartMove(moves);

            Referee.Simulating.Move(nextMove.Item1, nextMove.Item2);
        }

        return status;
    }

    private void BackPropagate(Leaf leaf, BoardStatus status)
    {
        leaf.Wins += ((int)status == leaf.Player) || status == BoardStatus.Draw ? (uint)1 : 0;
        leaf.Simulations++;

        if (leaf.Move.Item1 != -1)
        {
            _simulationsField[leaf.Move.Item1, leaf.Move.Item2].Simulations++;
            _simulationsField[leaf.Move.Item1, leaf.Move.Item2].Wins += ((int)status == leaf.Player) ? (uint)1 : 0;
        }

        if (leaf.Parent != null)
            BackPropagate(leaf.Parent, status);
    }

    public double c = Math.Sqrt(2);
    public double b = Math.Sqrt(2);

    private double CalculateUCT(Leaf leaf)
    {
        var totalSimulations = _simulationsField[leaf.Move.Item1, leaf.Move.Item2];
        var parentSimulations = leaf.Parent?.Simulations ?? 0;
        var beta = Beta(leaf.Simulations, totalSimulations.Simulations);
        return (1 - beta) * (leaf.Wins / (leaf.Simulations + _epsilon)) + (beta) * (totalSimulations.Wins / (totalSimulations.Simulations + _epsilon)) + c * Math.Sqrt(Math.Log(parentSimulations) / (leaf.Simulations + _epsilon)) + rand.Next() * _epsilon;
    }

    private double Beta(uint n, uint ns)
    {
        return ns / (n + ns + 4 * b * b * n * ns + _epsilon);
    }

    private Tuple<int, int> StandartMove(List<Tuple<int, int>> moves)
    {
        List<int> scores = new List<int>();
        foreach (var move in moves)
        {
            scores.Add(Referee.Simulating._field.GetMoveScore(move.Item1,move.Item2));
        }

        return moves[scores.IndexOf(scores.Max())];
    }

}

enum BoardStatus
{
    InProgress = 1,
    TicWin = 2,
    TacWin = 4,
    Draw = 8
}

public static class Heuristic
{
    public static int WinScore = 1000000;

    private static int[,,] _winningSequences = new int[,,]
    {
        { { 0,0 }, { 0,1 }, { 0,2 } },
        { { 1,0 }, { 1,1 }, { 1,2 } },
        { { 2,0 }, { 2,1 }, { 2,2 } },
        { { 0,0 }, { 1,0 }, { 2,0 } },
        { { 0,1 }, { 1,1 }, { 2,1 } },
        { { 0,2 }, { 1,2 }, { 2,2 } },
        { { 0,0 }, { 1,1 }, { 2,2 } },
        { { 0,2 }, { 1,1 }, { 2,0 } }
    };

    private static int ApproximateWinScore = 7;

    public static int GetScore(int [,] _field)
    {
        int player1 = 0, player2 = 0;
        for (int i = 0; i < 8; i++)
        {
            List<int> filteredField = new List<int>();
            for (int j = 0; j < 3; j++)
            {
                if (_field[_winningSequences[i, j, 0], _winningSequences[i, j, 1]] != BoardCells.Empty)
                    filteredField.Add(_field[_winningSequences[i, j, 0], _winningSequences[i, j, 1]]);
            }

            if(filteredField.Contains(BoardCells.Tic))
            {
                if (filteredField.Contains(BoardCells.Tac))
                    continue;
                if (filteredField.Count > 1)
                    player1 += ApproximateWinScore;
                player1++;
            }
            else
            {
                if (filteredField.Contains(BoardCells.Tac))
                {
                    if (filteredField.Count > 1)
                        player2 += ApproximateWinScore;
                    player2++;
                }
            }
        }

        return player1 - player2;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Tester
{
    public static void Train()
    {
        var startMove = new Tuple<int, int>(-1, -1);

        double bestC = 0;
        double bestB = 0;

        for (double c = 0.0001; c <= 10; c += 0.0001)
        {
            for (double b = 0.0001; b < 10; b += 0.0001)
            {
                int firstWins = 0;
                int secondWins = 0;
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Referee.GameReferee.Reset();

                        GameTree tree1 = null, tree2 = null;

                        BoardStatus status = BoardStatus.InProgress;
                        bool firstStep = true;
                        while ((status = Referee.GameReferee.GetStatus()) == BoardStatus.InProgress)
                        {
                            if(firstStep)
                            {
                                firstStep = false;
                                tree1 = new GameTree(startMove, Referee.GameReferee._field, BoardCells.Tac);
                                tree1.c = bestC;
                                tree1.b = bestB;
                                tree1.Simulate(980);
                                var move = tree1.GetNextStep();
                                Referee.GameReferee.Move(move.Item1, move.Item2);
                                tree1.Update(move, Referee.GameReferee._field);

                                tree2 = new GameTree(move, Referee.GameReferee._field, BoardCells.Tac);
                                tree2.c = c;
                                tree2.b = b;
                            }
                            else
                            {
                                tree1.Simulate(80);
                                var move = tree1.GetNextStep();
                                Referee.GameReferee.Move(move.Item1, move.Item2);
                                tree1.Update(move, Referee.GameReferee._field);
                                tree2.Update(move, Referee.GameReferee._field);

                            }

                            if ((status = Referee.GameReferee.GetStatus()) != BoardStatus.InProgress)
                                break;
                            if(firstStep)
                            {
                                tree2.Simulate(980);
                                var move = tree2.GetNextStep();
                                Referee.GameReferee.Move(move.Item1, move.Item2);
                                tree1.Update(move, Referee.GameReferee._field);
                                tree2.Update(move, Referee.GameReferee._field);
                            }
                            else
                            {
                                tree2.Simulate(80);
                                var move = tree2.GetNextStep();
                                Referee.GameReferee.Move(move.Item1, move.Item2);
                                tree1.Update(move, Referee.GameReferee._field);
                                tree2.Update(move, Referee.GameReferee._field);
                            }
                        }

                        if (status == BoardStatus.TicWin)
                            firstWins++;
                        else if (status == BoardStatus.TacWin)
                            secondWins++;
                    }

                }

                {
                    for (int i = 0; i < 50; i++)
                    {
                        Referee.GameReferee.Reset();
                        var tree1 = new GameTree(startMove, Referee.GameReferee._field, BoardCells.Tac);
                        tree1.c = bestC;
                        tree1.b = bestB;
                        var tree2 = new GameTree(startMove, Referee.GameReferee._field, BoardCells.Tac);
                        tree1.c = c;
                        tree2.b = b;

                        BoardStatus status = BoardStatus.InProgress;
                        bool firstStep = true;
                        while ((status = Referee.GameReferee.GetStatus()) == BoardStatus.InProgress)
                        {
                            if (firstStep)
                            {
                                firstStep = false;
                                tree2 = new GameTree(startMove, Referee.GameReferee._field, BoardCells.Tac);
                                tree2.c = bestC;
                                tree2.b = bestB;
                                tree2.Simulate(980);
                                var move = tree2.GetNextStep();
                                Referee.GameReferee.Move(move.Item1, move.Item2);
                                tree2.Update(move, Referee.GameReferee._field);

                                tree1 = new GameTree(move, Referee.GameReferee._field, BoardCells.Tac);
                                tree1.c = c;
                                tree1.b = b;
                            }
                            else
                            {
                                tree2.Simulate(80);
                                var move = tree2.GetNextStep();
                                Referee.GameReferee.Move(move.Item1, move.Item2);
                                tree1.Update(move, Referee.GameReferee._field);
                                tree2.Update(move, Referee.GameReferee._field);

                            }

                            if ((status = Referee.GameReferee.GetStatus()) != BoardStatus.InProgress)
                                break;
                            if (firstStep)
                            {
                                tree1.Simulate(980);
                                var move = tree1.GetNextStep();
                                Referee.GameReferee.Move(move.Item1, move.Item2);
                                tree1.Update(move, Referee.GameReferee._field);
                                tree2.Update(move, Referee.GameReferee._field);
                            }
                            else
                            {
                                tree1.Simulate(80);
                                var move = tree1.GetNextStep();
                                Referee.GameReferee.Move(move.Item1, move.Item2);
                                tree1.Update(move, Referee.GameReferee._field);
                                tree2.Update(move, Referee.GameReferee._field);
                            }
                        }

                        if (status == BoardStatus.TicWin)
                            secondWins++;
                        else if (status == BoardStatus.TacWin)
                            firstWins++;
                    }
                }

                if (secondWins > firstWins)
                {
                    bestB = b;
                    bestC = c;
                    Console.WriteLine($"Winner c:{c} b:{b}");
                }
                else
                {
                    Console.WriteLine($"Winner c:{bestC} b:{bestB}");
                }
            }
        }

        Console.ReadLine();
    }
}


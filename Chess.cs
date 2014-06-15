using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

//*******************************************************************************************************
// Chess Move List Checker - Makes sure that all moves are valid and unambiguous if processing SAN
//*******************************************************************************************************

namespace ChessRocks
{
  //this object can be instantiated to process a move list or check for a valid move
  public class Chess
  {
    #region member variables

    protected string lastFEN = "";

    public readonly string fenStart = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    protected string pawnTarget = "";
    protected string currentMoveList = "";
    protected string sanList = "";
    protected string lanList = "";
    protected string[] fields = new string[14];

    protected readonly string[,] boardLayout;
    protected string[,] piecePositions;
    protected readonly string[,] positionColor;

    protected char[] spaceDelim = " ".ToCharArray();
    protected char[] delimiter = " /".ToCharArray();

    protected bool whitesMove = true;
    protected bool wCastleQ, wCastleK, kCastleQ, kCastleK;
    protected bool inCheck = false;
    protected bool gameOver = false;
    protected bool staleMate = false;
    protected bool insufficientMaterial = false;
    protected bool insufficientWhite = false;
    protected bool insufficientBlack = false;
    protected bool invalidMove = false;
    protected bool checkQuietly = false;
    protected bool A_good_move = false;
    protected bool A_poor_move = false;
    protected bool A_very_good_move = false;
    protected bool A_very_poor_move = false;
    protected bool A_speculative_move = false;
    protected bool A_questionable_move = false;

    public readonly Point[] allowedMovesKing = new Point[8];
    //public readonly Point[] allowedMovesQueen = new Point[56]; - equivalent to rook and bishop moves combined
    public readonly Point[] allowedMovesBishop = new Point[28];
    public readonly Point[] allowedMovesKnight = new Point[8];
    public readonly Point[] allowedMovesRook = new Point[28];

    protected int halfMove = 0;
    protected int fullMove = 1;
    protected int mainPlycount = 0;

    protected Move move;
    protected ArrayList Moves = new ArrayList();
    Chess quietList;

    #endregion

    #region constructor

    public Chess()
    {
      //all moves conditional as far as not taking you off the 8x8 board
      //condition that he not be put into check
      allowedMovesKing[0] = new Point(1, 1);
      allowedMovesKing[1] = new Point(0, 1);
      allowedMovesKing[2] = new Point(1, 0);
      allowedMovesKing[3] = new Point(1, -1);
      allowedMovesKing[4] = new Point(-1, -1);
      allowedMovesKing[5] = new Point(-1, 0);
      allowedMovesKing[6] = new Point(0, -1);
      allowedMovesKing[7] = new Point(-1, 1);

      allowedMovesKnight[0] = new Point(2, 1);
      allowedMovesKnight[1] = new Point(2, -1);
      allowedMovesKnight[2] = new Point(1, 2);
      allowedMovesKnight[3] = new Point(1, -2);
      allowedMovesKnight[4] = new Point(-1, -2);
      allowedMovesKnight[5] = new Point(-1, 2);
      allowedMovesKnight[6] = new Point(-2, -1);
      allowedMovesKnight[7] = new Point(-2, 1);

      allowedMovesBishop[0] = new Point(1, 1);
      allowedMovesBishop[1] = new Point(2, 2);
      allowedMovesBishop[2] = new Point(3, 3);
      allowedMovesBishop[3] = new Point(4, 4);
      allowedMovesBishop[4] = new Point(5, 5);
      allowedMovesBishop[5] = new Point(6, 6);
      allowedMovesBishop[6] = new Point(7, 7);
      allowedMovesBishop[7] = new Point(-1, -1);
      allowedMovesBishop[8] = new Point(-2, -2);
      allowedMovesBishop[9] = new Point(-3, -3);
      allowedMovesBishop[10] = new Point(-4, -4);
      allowedMovesBishop[11] = new Point(-5, -5);
      allowedMovesBishop[12] = new Point(-6, -6);
      allowedMovesBishop[13] = new Point(-7, -7);
      allowedMovesBishop[14] = new Point(1, -1);
      allowedMovesBishop[15] = new Point(2, -2);
      allowedMovesBishop[16] = new Point(3, -3);
      allowedMovesBishop[17] = new Point(4, -4);
      allowedMovesBishop[18] = new Point(5, -5);
      allowedMovesBishop[19] = new Point(6, -6);
      allowedMovesBishop[20] = new Point(7, -7);
      allowedMovesBishop[21] = new Point(-1, 1);
      allowedMovesBishop[22] = new Point(-2, 2);
      allowedMovesBishop[23] = new Point(-3, 3);
      allowedMovesBishop[24] = new Point(-4, 4);
      allowedMovesBishop[25] = new Point(-5, 5);
      allowedMovesBishop[26] = new Point(-6, 6);
      allowedMovesBishop[27] = new Point(-7, 7);

      allowedMovesRook[0] = new Point(1, 0);
      allowedMovesRook[1] = new Point(2, 0);
      allowedMovesRook[2] = new Point(3, 0);
      allowedMovesRook[3] = new Point(4, 0);
      allowedMovesRook[4] = new Point(5, 0);
      allowedMovesRook[5] = new Point(6, 0);
      allowedMovesRook[6] = new Point(7, 0);
      allowedMovesRook[7] = new Point(-1, 0);
      allowedMovesRook[8] = new Point(-2, 0);
      allowedMovesRook[9] = new Point(-3, 0);
      allowedMovesRook[10] = new Point(-4, 0);
      allowedMovesRook[11] = new Point(-5, 0);
      allowedMovesRook[12] = new Point(-6, 0);
      allowedMovesRook[13] = new Point(-7, 0);
      allowedMovesRook[14] = new Point(0, -1);
      allowedMovesRook[15] = new Point(0, -2);
      allowedMovesRook[16] = new Point(0, -3);
      allowedMovesRook[17] = new Point(0, -4);
      allowedMovesRook[18] = new Point(0, -5);
      allowedMovesRook[19] = new Point(0, -6);
      allowedMovesRook[20] = new Point(0, -7);
      allowedMovesRook[21] = new Point(0, 1);
      allowedMovesRook[22] = new Point(0, 2);
      allowedMovesRook[23] = new Point(0, 3);
      allowedMovesRook[24] = new Point(0, 4);
      allowedMovesRook[25] = new Point(0, 5);
      allowedMovesRook[26] = new Point(0, 6);
      allowedMovesRook[27] = new Point(0, 7);

      boardLayout = new string[8, 8];

      // X, Y
      // File, (7-Rank)
      boardLayout[0, 0] = "a8";
      boardLayout[0, 1] = "a7";
      boardLayout[0, 2] = "a6";
      boardLayout[0, 3] = "a5";
      boardLayout[0, 4] = "a4";
      boardLayout[0, 5] = "a3";
      boardLayout[0, 6] = "a2";
      boardLayout[0, 7] = "a1";

      boardLayout[1, 0] = "b8";
      boardLayout[1, 1] = "b7";
      boardLayout[1, 2] = "b6";
      boardLayout[1, 3] = "b5";
      boardLayout[1, 4] = "b4";
      boardLayout[1, 5] = "b3";
      boardLayout[1, 6] = "b2";
      boardLayout[1, 7] = "b1";

      boardLayout[2, 0] = "c8";
      boardLayout[2, 1] = "c7";
      boardLayout[2, 2] = "c6";
      boardLayout[2, 3] = "c5";
      boardLayout[2, 4] = "c4";
      boardLayout[2, 5] = "c3";
      boardLayout[2, 6] = "c2";
      boardLayout[2, 7] = "c1";

      boardLayout[3, 0] = "d8";
      boardLayout[3, 1] = "d7";
      boardLayout[3, 2] = "d6";
      boardLayout[3, 3] = "d5";
      boardLayout[3, 4] = "d4";
      boardLayout[3, 5] = "d3";
      boardLayout[3, 6] = "d2";
      boardLayout[3, 7] = "d1";

      boardLayout[4, 0] = "e8";
      boardLayout[4, 1] = "e7";
      boardLayout[4, 2] = "e6";
      boardLayout[4, 3] = "e5";
      boardLayout[4, 4] = "e4";
      boardLayout[4, 5] = "e3";
      boardLayout[4, 6] = "e2";
      boardLayout[4, 7] = "e1";

      boardLayout[5, 0] = "f8";
      boardLayout[5, 1] = "f7";
      boardLayout[5, 2] = "f6";
      boardLayout[5, 3] = "f5";
      boardLayout[5, 4] = "f4";
      boardLayout[5, 5] = "f3";
      boardLayout[5, 6] = "f2";
      boardLayout[5, 7] = "f1";

      boardLayout[6, 0] = "g8";
      boardLayout[6, 1] = "g7";
      boardLayout[6, 2] = "g6";
      boardLayout[6, 3] = "g5";
      boardLayout[6, 4] = "g4";
      boardLayout[6, 5] = "g3";
      boardLayout[6, 6] = "g2";
      boardLayout[6, 7] = "g1";

      boardLayout[7, 0] = "h8";
      boardLayout[7, 1] = "h7";
      boardLayout[7, 2] = "h6";
      boardLayout[7, 3] = "h5";
      boardLayout[7, 4] = "h4";
      boardLayout[7, 5] = "h3";
      boardLayout[7, 6] = "h2";
      boardLayout[7, 7] = "h1";

      positionColor = new string[8, 8];
      piecePositions = new string[8, 8];

      for (int X = 0; X < 8; X++)
      {
        for (int Y = 0; Y < 8; Y++)
        {
          piecePositions[X, Y] = "";

          if ((X + Y) % 2 == 0)
          {
            positionColor[X, Y] = "w";
          }
          else
          {
            positionColor[X, Y] = "k";
          }
        }
      }
    }

    #endregion

    #region external interface

    //*******************************************************************************************************
    public ArrayList LoadList(string movelist, string fen, bool san)
    {
      return LoadList(movelist, fen, san, "");
    }

    //*******************************************************************************************************
    // fen from the previous move!
    public ArrayList LoadList(string movelist, string fen, bool san, string preMove1Comment)
    {
      bool result = true;

      mainPlycount = 0;

      move = new Move(fen, 1);

      //this sets the fullMove value
      ResetPieces(fen);

      lastFEN = fen;

      Moves.Clear();

      int alternativeMoveCount = 0;

      whitesMove = !(fen.IndexOf(" b ") > 0);

      if (san)
      {
        currentMoveList = movelist;
      }
      else
      {
        if (whitesMove) currentMoveList = fullMove.ToString() + ". " + movelist;
        else currentMoveList = (fullMove - 1).ToString() + "... " + movelist;
      }

      sanList = "";
      lanList = "";

      string tmp;
      bool commentMode = false;

      int i = 0;

      foreach (string tag in movelist.Split(spaceDelim))
      {
        tmp = tag.Trim();

        if (!commentMode && tmp.Equals("..."))
        {
          //chess.coms funky puzzle format...
          continue;
        }

        if (tmp.StartsWith("..."))
        {
          //chess.coms funky puzzle format...
          tmp = tmp.Substring(3);
        }

        if (tmp.Length == 0)
        {
          continue;
        }
        else if (alternativeMoveCount > 0)
        {
          //go through all the other characters to find open and close parens - they had better be matching...
          //they also need to matching in the comments also - this should be fixed...
          foreach (char c in tmp)
          {
            if (c == '(')
            {
              alternativeMoveCount++;
            }
            else if (c == ')')
            {
              alternativeMoveCount--;
            }
          }

          move.AddRav(tmp);
        }
        else if (commentMode)
        {
          //comments may not be nested
          if (tmp.EndsWith("}"))
          {
            commentMode = false;
          }
          try
          {
            move.AddComment(tmp);
          }
          catch
          {
            //this  will throw away a beginning comment
          }
        }
        else if (tmp.StartsWith("$"))
        {
          move.nag = Convert.ToInt16(tmp.Substring(1));
        }
        else if (tmp.StartsWith("{"))
        {
          //move comment 
          try
          {
            move.AddComment(tmp);
          }
          catch
          {
            //this  will throw away a beginning comment
          }

          if (!tmp.EndsWith("}"))
          {
            commentMode = true;
          }
        }
        else if (tmp.StartsWith("("))
        {
          //optional move(s) - may include comments and nags
          //go through all the other characters to find open and close parens - they had better be matching...
          foreach (char c in tmp)
          {
            if (c == '(')
            {
              alternativeMoveCount++;
            }
            else if (c == ')')
            {
              alternativeMoveCount--;
            }
          }

          move.AddRav(tmp);
        }
        else if (tmp.StartsWith("1") ||
                tmp.StartsWith("2") ||
                tmp.StartsWith("3") ||
                tmp.StartsWith("4") ||
                tmp.StartsWith("5") ||
                tmp.StartsWith("6") ||
                tmp.StartsWith("7") ||
                tmp.StartsWith("8") ||
                tmp.StartsWith("9"))
        {
          if (tmp.IndexOf("...") > 0)
          {
            if (tmp.EndsWith("..."))
            {
              //black move number only - ignore
            }
            else
            {
              //mixed move number and move
              tmp = tmp.Substring(tmp.IndexOf("...") + 3).Trim();

              try
              {
                if (mainPlycount > 0) Moves.Add(move);
                if (!san)
                {
                  if (!ToSAN(fullMove, tmp, whitesMove))
                  {
                    MessageBox.Show("Problem making black lan move " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList);
                    result = false;
                    break;
                  }
                }
                else if (!MakeMove(tmp, whitesMove, ""))
                {
                  MessageBox.Show("Problem making black san move " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList);
                  result = false;
                  break;
                }
              }
              catch (Exception e)
              {
                MessageBox.Show("Exception making black move " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList + "\n\n" + e.ToString());
                result = false;
                break;
              }

              try
              {
                sanList += ("\t" + move.san);
                lanList += ("\t" + move.lan);
              }
              catch (Exception e)
              {
                MessageBox.Show("Exception updating move list for " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList + "\n\n" + e.ToString());
                result = false;
                break;
              }

              i++;
            }
          }
          else if (tmp.IndexOf(".") > 0)
          {
            if (tmp.EndsWith("."))
            {
              //white move number only -ignore
            }
            else
            {
              //mixed move number and move
              tmp = tmp.Substring(tmp.IndexOf(".") + 1).Trim();

              try
              {
                if (mainPlycount > 0) Moves.Add(move);
                if (!san)
                {
                  if (!ToSAN(fullMove, tmp, whitesMove))
                  {
                    MessageBox.Show("Problem making white lan move " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList);
                    result = false;
                    break;
                  }
                }
                else if (!MakeMove(tmp, whitesMove, ""))
                {
                  MessageBox.Show("Problem making white san move " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList);
                  result = false;
                  break;
                }
              }
              catch (Exception e)
              {
                MessageBox.Show("Exception making white move " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList + "\n\n" + e.ToString());
                result = false;
                break;
              }

              try
              {
                sanList += ("\t" + move.san);
                lanList += ("\t" + move.lan);
              }
              catch (Exception e)
              {
                MessageBox.Show("Exception updating move list for " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList + "\n\n" + e.ToString());
                result = false;
                break;
              }

              i++;
            }
          }
          else
          {
            //this string that start with a number will be ignored
          }
        }
        else //better be the next move...
        {
          try
          {
            if (mainPlycount > 0) Moves.Add(move);
            if (!san)
            {
              if (!ToSAN(fullMove, tmp, whitesMove))
              {
                MessageBox.Show("Problem making lan move? " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList);
                result = false;
                break;
              }
            }
            else if (!MakeMove(tmp, whitesMove, ""))
            {
              MessageBox.Show("Problem making san move? " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList);
              result = false;
              break;
            }
          }
          catch (Exception e)
          {
            MessageBox.Show("Exception making move? " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList + "\n\n" + e.ToString());
            result = false;
            break;
          }

          try
          {
            sanList += ("\t" + move.san);
            lanList += ("\t" + move.lan);
          }
          catch (Exception e)
          {
            MessageBox.Show("Exception updating move list for " + tmp + "\n\n" + sanList + "\n" + lanList + "\n" + currentMoveList + "\n\n" + e.ToString());
            result = false;
            break;
          }

          i++;
        }
      }

      //add the last move
      if (result)
      {
        Moves.Add(move);
      }

      ((Move)(Moves[0])).comment = preMove1Comment + ((Move)(Moves[0])).comment;

      return Moves;
    }

    //*******************************************************************************************************
    // fen from the previous move
    public bool CheckMoveQuietly(string mv, string fen, ref Move moveObject)
    {
      bool result = true;

      checkQuietly = true;

      ResetPieces(fen);

      lastFEN = fen;

      whitesMove = !(fen.IndexOf(" b ") > 0);

      try
      {
        if (!ToSAN(fullMove, mv, whitesMove))
        {
          //king is still in check or now in check, invalid move...
          result = false;
        }
        else
        {
          moveObject = move;
        }
      }
      catch
      {
        result = false;
      }

      return result;
    }

    //*******************************************************************************************************
    protected string FindPiece(string piece)
    {
      for (int x = 0; x < 8; x++)
      {
        for (int y = 0; y < 8; y++)
        {
          if (piecePositions[x, y].Equals(piece))
          {
            return boardLayout[x, y];
          }
        }
      }

      return ""; //bad thing
    }

    //*******************************************************************************************************
    protected virtual bool MakeMove(string pgn, bool whiteMove, string longA)
    {
      string newFEN;

      string kingsSquare;

      inCheck = gameOver = insufficientMaterial = staleMate = false;

      FilterSAN(ref pgn);

      //clean up if zeroes were used
      IsACastle(ref pgn);

      int tmpFullMove = fullMove;

      if (!whiteMove) fullMove++;

      if (ToLAN(tmpFullMove, ref pgn, ref longA, whiteMove))
      {
        newFEN = ToFEN(whiteMove);
        kingsSquare = FindPiece(whiteMove ? "wKing" : "kKing");
        if (UnderAttack(kingsSquare, !whiteMove))
        {
          //king is still in check or now in check, invalid move...
          ResetPieces(lastFEN);
          return false;
        }

        kingsSquare = FindPiece(whiteMove ? "kKing" : "wKing");
        if (UnderAttack(kingsSquare, whiteMove))
        {
          //the opponent has been put into check...
          //make sure its not also checkmate...
          inCheck = true;
          if (CheckMate(kingsSquare, whiteMove))
          {
            gameOver = true;
          }
        }
        else if (CheckMate(kingsSquare, whiteMove))
        {
          gameOver = true;
          staleMate = true;
        }
        else if (insufficientMaterial)
        {
          gameOver = true;
        }
      }
      else
      {
        ResetPieces(lastFEN);
        return false;
      }

      mainPlycount++;

      if (gameOver && inCheck)
      {
        pgn += "#";
      }
      else if (inCheck)
      {
        pgn += "+";
      }

      move = new Move(lastFEN, tmpFullMove);
      move.san = pgn;
      move.lan = longA;
      move.gameOver = gameOver;
      move.inCheck = inCheck;
      move.staleMate = staleMate;
      move.insufficientMaterial = insufficientMaterial;
      move.insufficientWhite = insufficientWhite;
      move.insufficientBlack = insufficientBlack;
      move.halfMove = halfMove;

      if (A_good_move) { move.nag = 1; A_good_move = false; }
      else if (A_poor_move) { move.nag = 2; A_poor_move = false; }
      else if (A_very_good_move) { move.nag = 3; A_very_good_move = false; }
      else if (A_very_poor_move) { move.nag = 4; A_very_poor_move = false; }
      else if (A_speculative_move) { move.nag = 5; A_speculative_move = false; }
      else if (A_questionable_move) { move.nag = 6; A_questionable_move = false; }

      lastFEN = newFEN;
      move.fen = lastFEN;
      ResetPieces(lastFEN);
      whitesMove = !whiteMove;

      return true;
    }

    //*******************************************************************************************************
    public void FilterSAN(ref string san)
    {
      //some people attach these to the end of a move...
      if (san.EndsWith("?!"))
      {
        A_questionable_move = true;
        san = san.Substring(0, san.Length - 2);
      }
      else if (san.EndsWith("!?"))
      {
        A_speculative_move = true;
        san = san.Substring(0, san.Length - 2);
      }
      else if (san.EndsWith("??"))
      {
        A_very_poor_move = true;
        san = san.Substring(0, san.Length - 2);
      }
      else if (san.EndsWith("!!"))
      {
        A_very_good_move = true;
        san = san.Substring(0, san.Length - 2);
      }
      else if (san.EndsWith("?"))
      {
        A_poor_move = true;
        san = san.Substring(0, san.Length - 1);
      }
      else if (san.EndsWith("!"))
      {
        A_good_move = true;
        san = san.Substring(0, san.Length - 1);
      }

      if (san.EndsWith("+"))
      {
        san = san.Substring(0, san.Length - 1);
      }
      else if (san.EndsWith("#"))
      {
        san = san.Substring(0, san.Length - 1);
      }

      san = san.Replace("=", "");
    }

    //*******************************************************************************************************
    protected bool ToSAN(int moveNum, string lan, bool whiteMove)
    {
      string src, src2 = "", dst, san = "";

      src = lan.Substring(0, 2);
      dst = lan.Substring(2, 2);

      Point p1 = new Point();
      Point p2 = new Point();

      p1 = FindPosition(src);
      p2 = FindPosition(dst);

      bool take = (piecePositions[p2.X, p2.Y].Length > 0);

      if (lan.Equals("e1g1") && piecePositions[p1.X, p1.Y].EndsWith("wKing"))
      {
        san = "O-O";
        pawnTarget = "";
      }
      else if (lan.Equals("e1c1") && piecePositions[p1.X, p1.Y].EndsWith("wKing"))
      {
        san = "O-O-O";
        pawnTarget = "";
      }
      else if (lan.Equals("e8g8") && piecePositions[p1.X, p1.Y].EndsWith("kKing"))
      {
        san = "O-O";
        pawnTarget = "";
      }
      else if (lan.Equals("e8c8") && piecePositions[p1.X, p1.Y].EndsWith("kKing"))
      {
        san = "O-O-O";
        pawnTarget = "";
      }
      else if (piecePositions[p1.X, p1.Y].EndsWith("Knight"))
      {
        //find all valid knight moves to the destination square
        if (FindKnightSource((whiteMove ? "wKnight" : "kKnight"), dst, ref src2, ref src, moveNum.ToString() + (whiteMove ? ". " : "... ") + lan))
        {
          san = "N" + src2 + (take ? "x" : "") + dst;
        }
        else
        {
          //invalid move
          return false;
        }
        pawnTarget = "";
      }
      else if (piecePositions[p1.X, p1.Y].EndsWith("Bishop"))
      {
        if (FindBishopSource((whiteMove ? "wBishop" : "kBishop"), dst, ref src2, ref src, moveNum.ToString() + (whiteMove ? ". " : "... ") + lan))
        {
          san = "B" + src2 + (take ? "x" : "") + dst;
        }
        else
        {
          //invalid move
          return false;
        }
        pawnTarget = "";
      }
      else if (piecePositions[p1.X, p1.Y].EndsWith("Rook"))
      {
        if (FindRookSource((whiteMove ? "wRook" : "kRook"), dst, ref src2, ref src, moveNum.ToString() + (whiteMove ? ". " : "... ") + lan))
        {
          san = "R" + src2 + (take ? "x" : "") + dst;
        }
        else
        {
          //invalid move
          return false;
        }
        pawnTarget = "";
      }
      else if (piecePositions[p1.X, p1.Y].EndsWith("Queen"))
      {
        if (FindQueenSource((whiteMove ? "wQueen" : "kQueen"), dst, ref src2, ref src, moveNum.ToString() + (whiteMove ? ". " : "... ") + lan))
        {
          san = "Q" + src2 + (take ? "x" : "") + dst;
        }
        else
        {
          //invalid move
          return false;
        }
        pawnTarget = "";
      }
      else if (piecePositions[p1.X, p1.Y].EndsWith("King"))
      {
        src2 = "";
        if (FindKingSource((whiteMove ? "wKing" : "kKing"), dst, ref src2) && src.Equals(src2))
        {
          san = "K" + (take ? "x" : "") + dst;
        }
        else
        {
          //invalid move
          return false;
        }

        pawnTarget = "";
      }
      else
      {
        //pawns are all that are left
        //need to check for enpassant...
        if (pawnTarget.Length > 0)
        {
          if (whiteMove && dst.Substring(1, 1).Equals("6"))
          {
            src2 = dst.Substring(0, 1) + "6";
          }
          else if (!whiteMove && dst.Substring(1, 1).Equals("3"))
          {
            src2 = dst.Substring(0, 1) + "3";
          }
          else
          {
            src2 = "";
          }

          if (!pawnTarget.Equals(src2))
          {
            src2 = "";
          }
          else
          {
            if (whiteMove && dst.Substring(1, 1).Equals("6"))
            {
              src2 = dst.Substring(0, 1) + "5";
            }
            else if (!whiteMove && dst.Substring(1, 1).Equals("3"))
            {
              src2 = dst.Substring(0, 1) + "4";
            }
          }
        }
        else
        {
          src2 = "";
        }
        p2 = FindPosition(src2);

        if (dst.EndsWith("8") && piecePositions[p1.X, p1.Y].Equals("wPawn"))
        {
          if (take)
          {
            san = src.Substring(0, 1) + "x" + dst + "=" + lan.Substring(4, 1).ToUpper();
          }
          else
          {
            san = dst + "=" + lan.Substring(4, 1).ToUpper();
          }
          pawnTarget = "";
        }
        else if (dst.EndsWith("1") && piecePositions[p1.X, p1.Y].Equals("kPawn"))
        {
          if (take)
          {
            san = src.Substring(0, 1) + "x" + dst + "=" + lan.Substring(4, 1).ToUpper();
          }
          else
          {
            san = dst + "=" + lan.Substring(4, 1).ToUpper();
          }
          pawnTarget = "";
        }
        else if (take)
        {
          san = src.Substring(0, 1) + (take ? "x" : "") + dst;
          pawnTarget = "";
        }
        else if (piecePositions[p1.X, p1.Y].Equals((whiteMove ? "wPawn" : "kPawn")) &&
                 (src2.Length > 0) &&
                 piecePositions[p2.X, p2.Y].Equals((whiteMove ? "kPawn" : "wPawn")))
        {
          //enpassant
          san = src.Substring(0, 1) + "x" + dst;
        }
        else if (src.Substring(0, 1).Equals(dst.Substring(0, 1)))
        {
          san = dst;

          if (src.Substring(1, 1).Equals("2") && dst.Substring(1, 1).Equals("4") && piecePositions[p1.X, p1.Y].EndsWith("wPawn"))
          {
            pawnTarget = dst.Substring(0, 1) + "3";
          }
          else if (src.Substring(1, 1).Equals("7") && dst.Substring(1, 1).Equals("5") && piecePositions[p1.X, p1.Y].EndsWith("kPawn"))
          {
            pawnTarget = dst.Substring(0, 1) + "6";
          }
          else
          {
            pawnTarget = "";
          }
        }
        else
        {
          //invalid move
          return false;
        }
      }

      return MakeMove(san, whiteMove, lan);
    }

    //*******************************************************************************************************
    protected bool ToLAN(int moveNum, ref string san, ref string lan, bool whiteMove)
    {
      string dst, src, src2, promotedTo, original;
      bool validMove = true;

      try
      {
        original = lan.Substring(0, 2);
      }
      catch
      {
        original = "";
      }

      Point p1 = new Point();
      Point p2 = new Point();
      Point p3 = new Point();

      if (IsACastle(ref san))
      {
        if (!Castle(moveNum, san, whiteMove))
        {
          InvalidMove(moveNum, whiteMove, san);
          validMove = false;
          san = san.Replace('-', '?');
          lan = san;
        }
        else
        {
          if (whiteMove && san.Equals("O-O"))
          {
            lan = "e1g1";
          }
          else if (whiteMove && san.Equals("O-O-O"))
          {
            lan = "e1c1";
          }
          else if (!whiteMove && san.Equals("O-O"))
          {
            lan = "e8g8";
          }
          else if (!whiteMove && san.Equals("O-O-O"))
          {
            lan = "e8c8";
          }
          else
          {
            validMove = false;
            san = san.Replace('-', '?');
            lan = san;
          }
        }

        pawnTarget = "";
        halfMove++;
        return validMove;
      }
      else
      {
        if (san.IndexOf('x') > 0)
        {
          //a piece was taken
          halfMove = 0;
          dst = san.Substring(san.Length - 2, 2);
          src = san.Substring(0, san.Length - 3);

          p2 = FindPosition(dst);

          if (IsAPawnPromotion(san))
          {
            dst = san.Substring(2, 2);
            src = san.Substring(0, 1) + (dst.EndsWith("1") ? "2" : "7");

            promotedTo = GetPromotion(san, whiteMove);

            if (promotedTo.Length == 0)
            {
              //promotion failed...
              lan = src + dst + "?";
              validMove = false;
            }
            else
            {
              MovePiece(src, dst);
              PromotePiece(dst, promotedTo);
              lan = src + dst + san.Substring(san.Length - 1, 1).ToLower();
            }
          }
          else if (src.Length == 1 && !src.Equals("B") && !src.Equals("N") && !src.Equals("R") && !src.Equals("Q") && !src.Equals("K"))
          {
            //this is a pawn take, the first letter indicates the source rank

            //for enPassant
            if (pawnTarget.Length > 0)
            {
              if (whiteMove && dst.Substring(1, 1).Equals("6"))
              {
                src2 = dst.Substring(0, 1) + "6";
              }
              else if (!whiteMove && dst.Substring(1, 1).Equals("3"))
              {
                src2 = dst.Substring(0, 1) + "3";
              }
              else
              {
                src2 = "";
              }

              if (!pawnTarget.Equals(src2))
              {
                src2 = "";
              }
              else
              {
                if (whiteMove && dst.Substring(1, 1).Equals("6"))
                {
                  src2 = dst.Substring(0, 1) + "5";
                }
                else if (!whiteMove && dst.Substring(1, 1).Equals("3"))
                {
                  src2 = dst.Substring(0, 1) + "4";
                }
              }
            }
            else
            {
              src2 = "";
            }

            if (whiteMove)
            {
              src += (Convert.ToInt16(dst.Substring(1, 1)) - 1).ToString();
              p1 = FindPosition(src);
              p3 = FindPosition(src2);
              //make sure there is a white pawn there
              if (piecePositions[p1.X, p1.Y].Equals("wPawn") && piecePositions[p2.X, p2.Y].StartsWith("k"))
              {
                MovePiece(src, dst);
                lan = src + dst;
              }
              else if (piecePositions[p1.X, p1.Y].Equals("wPawn") &&
                      (src2.Length > 0) &&
                       piecePositions[p3.X, p3.Y].StartsWith("k"))
              {
                //enpassant
                MovePiece(src, dst);
                piecePositions[p3.X, p3.Y] = "";
                lan = src + dst;
              }
              else
              {
                lan = src + dst + "?";
                InvalidMove(moveNum, whiteMove, lan);
                validMove = false;
              }
            }
            else
            {
              src += (Convert.ToInt16(dst.Substring(1, 1)) + 1).ToString();
              p1 = FindPosition(src);
              p3 = FindPosition(src2);
              //make sure there is a black pawn there
              if (piecePositions[p1.X, p1.Y].Equals("kPawn") && piecePositions[p2.X, p2.Y].StartsWith("w"))
              {
                MovePiece(src, dst);
                lan = src + dst;
              }
              else if (piecePositions[p1.X, p1.Y].Equals("kPawn") &&
                      (src2.Length > 0) &&
                       piecePositions[p3.X, p3.Y].StartsWith("w"))
              {
                //enpassant
                MovePiece(src, dst);
                piecePositions[p3.X, p3.Y] = "";
                lan = src + dst;
              }
              else
              {
                lan = src + dst + "?";
                InvalidMove(moveNum, whiteMove, lan);
                validMove = false;
              }
            }
          }
          else if ((piecePositions[p2.X, p2.Y].Length == 0) || piecePositions[p2.X, p2.Y].StartsWith((whiteMove ? "w" : "k")))
          {
            //no piece on destination square to kill
            lan = src + dst + "?";
            InvalidMove(moveNum, whiteMove, lan);
            validMove = false;
          }
          else if (src.IndexOf("B") == 0)
          {
            src2 = src.Substring(1, src.Length - 1);
            if (FindBishopSource((whiteMove ? "wBishop" : "kBishop"), dst, ref src2, ref original, moveNum.ToString() + (whiteMove ? ". " : "... ") + san))
            {
              MovePiece(original, dst);
              lan = original + dst;
              san = "B" + src2 + "x" + dst;
            }
            else
            {
              InvalidMove(moveNum, whiteMove, san);
              validMove = false;
            }
          }
          else if (src.IndexOf("R") == 0)
          {
            src2 = src.Substring(1, src.Length - 1);
            if (FindRookSource((whiteMove ? "wRook" : "kRook"), dst, ref src2, ref original, moveNum.ToString() + (whiteMove ? ". " : "... ") + san))
            {
              MovePiece(original, dst);
              lan = original + dst;
              san = "R" + src2 + "x" + dst;
            }
            else
            {
              InvalidMove(moveNum, whiteMove, san);
              validMove = false;
            }
          }
          else if (src.IndexOf("Q") == 0)
          {
            src2 = src.Substring(1, src.Length - 1);
            if (FindQueenSource((whiteMove ? "wQueen" : "kQueen"), dst, ref src2, ref original, moveNum.ToString() + (whiteMove ? ". " : "... ") + san))
            {
              MovePiece(original, dst);
              lan = original + dst;
              san = "Q" + src2 + "x" + dst;
            }
            else
            {
              InvalidMove(moveNum, whiteMove, san);
              validMove = false;
            }
          }
          else if (src.IndexOf("N") == 0)
          {
            src2 = src.Substring(1, src.Length - 1);
            if (FindKnightSource((whiteMove ? "wKnight" : "kKnight"), dst, ref src2, ref original, moveNum.ToString() + (whiteMove ? ". " : "... ") + san))
            {
              MovePiece(original, dst);
              lan = original + dst;
              san = "N" + src2 + "x" + dst;
            }
            else
            {
              InvalidMove(moveNum, whiteMove, san);
              validMove = false;
            }
          }
          else if (src.IndexOf("K") == 0)
          {
            if (FindKingSource((whiteMove ? "wKing" : "kKing"), dst, ref src))
            {
              MovePiece(src, dst);
              lan = src + dst;
            }
            else
            {
              lan = src + dst + "?";
              InvalidMove(moveNum, whiteMove, lan);
              validMove = false;
            }
          }
          pawnTarget = "";
        }
        else
        {
          pawnTarget = "";

          //a piece was just moved
          dst = san.Substring(san.Length - 2, 2);
          src = san.Substring(0, san.Length - 2);
          src2 = "";

          p2 = FindPosition(dst);

          if (IsAPawnPromotion(san))
          {
            dst = san.Substring(0, 2);
            src = san.Substring(0, 1) + (dst.EndsWith("1") ? "2" : "7");

            halfMove = 0;
            promotedTo = GetPromotion(san, whiteMove);

            if (promotedTo.Length == 0)
            {
              //promotion failed...
              lan = src + dst + "?";
              validMove = false;
            }
            else
            {
              MovePiece(src, dst);
              PromotePiece(dst, promotedTo);
              lan = src + dst + san.Substring(san.Length - 1, 1).ToLower();
            }
          }
          else if (src.Length == 0)
          {
            //this is a pawn move
            halfMove = 0;
            if (whiteMove)
            {
              src = dst.Replace(dst.Substring(1, 1), ((Convert.ToInt16(dst.Substring(1, 1)) - 1).ToString()));
              if (dst.Substring(1, 1).Equals("4"))
              {
                src2 = dst.Replace("4", "2");
              }

              p1 = FindPosition(src);
              p3 = FindPosition(src2);

              if (piecePositions[p2.X, p2.Y].Length > 0)
              {
                //there is already a piece sitting on the destination square
                lan = src + dst + "?";
                InvalidMove(moveNum, whiteMove, lan);
                validMove = false;
              }
              //make sure there is a white pawn there
              else if (piecePositions[p1.X, p1.Y].Equals("wPawn"))
              {
                MovePiece(src, dst);
                lan = src + dst;
              }
              else if ((piecePositions[p1.X, p1.Y].Length == 0) && (src2.Length > 0) && piecePositions[p3.X, p3.Y].Equals("wPawn"))
              {
                pawnTarget = dst.Substring(0, 1) + "3";
                MovePiece(src2, dst);
                lan = src2 + dst;
              }
              else
              {
                lan = src + dst + "?";
                InvalidMove(moveNum, whiteMove, lan);
                validMove = false;
              }
            }
            else
            {
              src = dst.Replace(dst.Substring(1, 1), ((Convert.ToInt16(dst.Substring(1, 1)) + 1).ToString()));
              if (dst.Substring(1, 1).Equals("5")) src2 = dst.Replace("5", "7");

              p1 = FindPosition(src);
              p3 = FindPosition(src2);

              if (piecePositions[p2.X, p2.Y].Length > 0)
              {
                //there is already a piece sitting on the destination square
                lan = src + dst + "?";
                InvalidMove(moveNum, whiteMove, lan);
                validMove = false;
              }
              //make sure there is a black pawn there
              else if (piecePositions[p1.X, p1.Y].Equals("kPawn"))
              {
                MovePiece(src, dst);
                lan = src + dst;
              }
              else if ((piecePositions[p1.X, p1.Y].Length == 0) && (src2.Length > 0) && piecePositions[p3.X, p3.Y].Equals("kPawn"))
              {
                pawnTarget = dst.Substring(0, 1) + "6";
                MovePiece(src2, dst);
                lan = src2 + dst;
              }
              else
              {
                lan = src + dst + "?";
                InvalidMove(moveNum, whiteMove, lan);
                validMove = false;
              }
            }
          }
          else if (piecePositions[p2.X, p2.Y].Length > 0)
          {
            //there is already a piece sitting on the destination square
            lan = src + dst + "?";
            InvalidMove(moveNum, whiteMove, lan);
            halfMove++;
            validMove = false;
          }
          else if (src.IndexOf("B") == 0)
          {
            halfMove++;
            src2 = src.Substring(1, src.Length - 1);
            if (FindBishopSource((whiteMove ? "wBishop" : "kBishop"), dst, ref src2, ref original, moveNum.ToString() + (whiteMove ? ". " : "... ") + san))
            {
              MovePiece(original, dst);
              lan = original + dst;
              san = "B" + src2 + dst;
            }
            else
            {
              InvalidMove(moveNum, whiteMove, san);
              validMove = false;
            }
          }
          else if (src.IndexOf("R") == 0)
          {
            halfMove++;
            src2 = src.Substring(1, src.Length - 1);
            if (FindRookSource((whiteMove ? "wRook" : "kRook"), dst, ref src2, ref original, moveNum.ToString() + (whiteMove ? ". " : "... ") + san))
            {
              MovePiece(original, dst);
              lan = original + dst;
              san = "R" + src2 + dst;
            }
            else
            {
              InvalidMove(moveNum, whiteMove, san);
              validMove = false;
            }
          }
          else if (src.IndexOf("Q") == 0)
          {
            halfMove++;
            src2 = src.Substring(1, src.Length - 1);
            if (FindQueenSource((whiteMove ? "wQueen" : "kQueen"), dst, ref src2, ref original, moveNum.ToString() + (whiteMove ? ". " : "... ") + san))
            {
              MovePiece(original, dst);
              lan = original + dst;
              san = "Q" + src2 + dst;
            }
            else
            {
              InvalidMove(moveNum, whiteMove, san);
              validMove = false;
            }
          }
          else if (src.IndexOf("N") == 0)
          {
            halfMove++;
            src2 = src.Substring(1, src.Length - 1);
            if (FindKnightSource((whiteMove ? "wKnight" : "kKnight"), dst, ref src2, ref original, moveNum.ToString() + (whiteMove ? ". " : "... ") + san))
            {
              MovePiece(original, dst);
              lan = original + dst;
              san = "N" + src2 + dst;
            }
            else
            {
              InvalidMove(moveNum, whiteMove, san);
              validMove = false;
            }
          }
          else if (src.IndexOf("K") >= 0)
          {
            halfMove++;
            if (FindKingSource((whiteMove ? "wKing" : "kKing"), dst, ref src))
            {
              MovePiece(src, dst);
              lan = src + dst;
            }
            else
            {
              lan = src + dst + "?";
              InvalidMove(moveNum, whiteMove, lan);
              validMove = false;
            }
          }
        }
      }

      return validMove;
    }

    //*******************************************************************************************************
    protected string ToFEN(bool whiteMoved)
    {
      string fen = "";

      //counting pieces for insufficient material...
      int kPawnCount, kRookCount, kBishopKCount, kBishopWCount, kKnightCount, kQueenCount;
      int wPawnCount, wRookCount, wBishopKCount, wBishopWCount, wKnightCount, wQueenCount;
      kPawnCount = kRookCount = kBishopKCount = kBishopWCount = kKnightCount = kQueenCount = 0;
      wPawnCount = wRookCount = wBishopKCount = wBishopWCount = wKnightCount = wQueenCount = 0;

      //look at the pieces on the board and create fen notation from it
      //go through from a8 to h1...by rank
      //a8 - h8 ... a1 - h1
      int emptySquares;

      for (int rank = 7; rank >= 0; rank--)
      {
        emptySquares = 0;

        for (int file = 0; file < 8; file++)
        {
          if (piecePositions[file, 7 - rank].Equals("kKing"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "k";
          }
          else if (piecePositions[file, 7 - rank].Equals("kRook"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "r";
            kRookCount++;
          }
          else if (piecePositions[file, 7 - rank].Equals("kKnight"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "n";
            kKnightCount++;
          }
          else if (piecePositions[file, 7 - rank].Equals("kBishop"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "b";
            if (positionColor[file, 7 - rank].Equals("w"))
            {
              kBishopWCount++;
            }
            else
            {
              kBishopKCount++;
            }
          }
          else if (piecePositions[file, 7 - rank].Equals("kQueen"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "q";
            kQueenCount++;
          }
          else if (piecePositions[file, 7 - rank].Equals("kPawn"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "p";
            kPawnCount++;
          }
          else if (piecePositions[file, 7 - rank].Equals("wKing"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "K";
          }
          else if (piecePositions[file, 7 - rank].Equals("wRook"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "R";
            wRookCount++;
          }
          else if (piecePositions[file, 7 - rank].Equals("wKnight"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "N";
            wKnightCount++;
          }
          else if (piecePositions[file, 7 - rank].Equals("wBishop"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "B";
            if (positionColor[file, 7 - rank].Equals("w"))
            {
              wBishopWCount++;
            }
            else
            {
              wBishopKCount++;
            }
          }
          else if (piecePositions[file, 7 - rank].Equals("wQueen"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "Q";
            wQueenCount++;
          }
          else if (piecePositions[file, 7 - rank].Equals("wPawn"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "P";
            wPawnCount++;
          }
          else
          {
            //this should be a number indicating a number of blank squares
            emptySquares++;
            if (file == 7)
            {
              fen += emptySquares.ToString();
            }
          }
        }
        if (rank > 0) fen += "/";
      }

      if (whiteMoved)
      {
        fen += " b";
      }
      else
      {
        fen += " w";
      }

      if (!wCastleQ && !wCastleK && !kCastleQ && !kCastleK)
      {
        fen += " -";
      }
      else
      {
        fen += " ";

        if (wCastleK)
        {
          fen += "K";
        }

        if (wCastleQ)
        {
          fen += "Q";
        }

        if (kCastleK)
        {
          fen += "k";
        }

        if (kCastleQ)
        {
          fen += "q";
        }
      }

      if (pawnTarget.Length == 0)
      {
        fen += " -";
      }
      else
      {
        fen += (" " + pawnTarget);
      }

      fen += (" " + halfMove.ToString());

      fen += (" " + fullMove.ToString());

      //The game is drawn when one of the following conditions arise: 
      //(a) king against king; 
      //(b) king against king with only bishop or knight; 
      //(c) king and bishop against king and bishop, with both bishops on diagonals of the same colour. ???

      //This immediately ends the game. 
      if ((wQueenCount == 0) && (kQueenCount == 0) && (wRookCount == 0) && (kRookCount == 0) && (wPawnCount == 0) && (kPawnCount == 0))
      {
        if ((wKnightCount == 0) && (kKnightCount == 0) && (wBishopWCount == 0) && (wBishopKCount == 0) && (kBishopWCount == 0) && (kBishopKCount == 0))
        {
          //(a)
          insufficientMaterial = true;
        }
        else if ((wKnightCount == 0) && (kKnightCount == 0) && (wBishopWCount == 1) && (wBishopKCount == 0) && (kBishopWCount == 1) && (kBishopKCount == 0))
        {
          //(c)
          insufficientMaterial = true;
        }
        else if ((wKnightCount == 0) && (kKnightCount == 0) && (wBishopWCount == 0) && (wBishopKCount == 1) && (kBishopWCount == 0) && (kBishopKCount == 1))
        {
          //(c)
          insufficientMaterial = true;
        }
        else if ((wBishopWCount == 0) && (wBishopKCount == 0) && (kBishopWCount == 0) && (kBishopKCount == 0))
        {
          if ((wKnightCount == 1) && (kKnightCount == 0))
          {
            //(b)
            insufficientMaterial = true;
          }
          else if ((wKnightCount == 0) && (kKnightCount == 1))
          {
            //(b)
            insufficientMaterial = true;
          }
          else
          {
            insufficientMaterial = false;
          }
        }
        else if ((wKnightCount == 0) && (kKnightCount == 0))
        {
          if ((wBishopWCount == 1) && (wBishopKCount == 0) && (kBishopWCount == 0) && (kBishopKCount == 0))
          {
            //(b)
            insufficientMaterial = true;
          }
          else if ((wBishopWCount == 0) && (wBishopKCount == 1) && (kBishopWCount == 0) && (kBishopKCount == 0))
          {
            //(b)
            insufficientMaterial = true;
          }
          else if ((wBishopWCount == 0) && (wBishopKCount == 0) && (kBishopWCount == 1) && (kBishopKCount == 0))
          {
            //(b)
            insufficientMaterial = true;
          }
          else if ((wBishopWCount == 0) && (wBishopKCount == 0) && (kBishopWCount == 0) && (kBishopKCount == 1))
          {
            //(b)
            insufficientMaterial = true;
          }
          else
          {
            insufficientMaterial = false;
          }
        }
      }
      else if ((wQueenCount == 0) && (kQueenCount == 0) && (wRookCount == 0) && (kRookCount == 0) && (wBishopWCount == 0) && (wBishopKCount == 0) && (kBishopWCount == 0) && (kBishopKCount == 0) && (wPawnCount == kPawnCount) && (kPawnCount > 3))
      {
        //look for an inpenetrable pawn wall with kings on the opposite sides...TBD
        if (false)
        {
        }
        else
        {
          insufficientMaterial = false;
        }
      }
      else
      {
        insufficientMaterial = false;
      }

      //see if white can even win...
      if ((wQueenCount == 0) && (wRookCount == 0) && (wPawnCount == 0))
      {
        if ((wKnightCount == 0) && (wBishopWCount == 0) && (wBishopKCount == 0))
        {
          //(a)
          insufficientWhite = true;
        }
        else if ((wKnightCount == 0) && (wBishopWCount == 1) && (wBishopKCount == 0))
        {
          //(b)
          insufficientWhite = true;
        }
        else if ((wKnightCount == 0) && (wBishopWCount == 0) && (wBishopKCount == 1))
        {
          //(b)
          insufficientWhite = true;
        }
        else if ((wKnightCount == 1) && (wBishopWCount == 0) && (wBishopKCount == 0))
        {
          //(b)
          insufficientWhite = true;
        }
        else
        {
          insufficientWhite = false;
        }
      }
      else
      {
        insufficientWhite = false;
      }

      //see if black can even win...
      if ((kQueenCount == 0) && (kRookCount == 0) && (kPawnCount == 0))
      {
        if ((kKnightCount == 0) && (kBishopWCount == 0) && (kBishopKCount == 0))
        {
          //(a)
          insufficientBlack = true;
        }
        else if ((kKnightCount == 0) && (kBishopWCount == 1) && (kBishopKCount == 0))
        {
          //(b)
          insufficientBlack = true;
        }
        else if ((kKnightCount == 0) && (kBishopWCount == 0) && (kBishopKCount == 1))
        {
          //(b)
          insufficientBlack = true;
        }
        else if ((kKnightCount == 1) && (kBishopWCount == 0) && (kBishopKCount == 0))
        {
          //(b)
          insufficientBlack = true;
        }
        else
        {
          insufficientBlack = false;
        }
      }
      else
      {
        insufficientBlack = false;
      }

      return fen;
    }

    #endregion

    #region Piece Stuff

    //*******************************************************************************************************
    protected void ResetPieces(string fen)
    {
      if (fen.Length > 0)
      {
        string tmp = fen;
        int wq, wb, wk, wr, wp, kq, kb, kk, kr, kp;
        int index, fieldIndex = 0, blankCount;

        //count the number of pieces still on the board
        wq = wb = wk = wr = wp = kq = kb = kk = kr = kp = 0;

        fields = tmp.Split(delimiter, 14);

        if (fields.Length == 11)
        {
          // hopefully only the last 2 fields are missing
          //chess.coms fen's do not always have the last 2 fields
        }
        else if (fields.Length != 13)
        {
          MessageBox.Show("Invalid number of fields in FEN notation : " + tmp);
          return;
        }

        for (int rank = 7; rank >= 0; rank--)
        {
          index = 0;

          for (int file = 0; file < 8; file++)
          {
            if (fields[fieldIndex].Substring(index, 1).Equals("k"))
            {
              piecePositions[file, 7 - rank] = "kKing";
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("r"))
            {
              piecePositions[file, 7 - rank] = "kRook";
              kr++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("n"))
            {
              piecePositions[file, 7 - rank] = "kKnight";
              kk++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("b"))
            {
              piecePositions[file, 7 - rank] = "kBishop";
              kb++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("q"))
            {
              piecePositions[file, 7 - rank] = "kQueen";
              kq++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("p"))
            {
              piecePositions[file, 7 - rank] = "kPawn";
              kp++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("K"))
            {
              piecePositions[file, 7 - rank] = "wKing";
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("R"))
            {
              piecePositions[file, 7 - rank] = "wRook";
              wr++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("N"))
            {
              piecePositions[file, 7 - rank] = "wKnight";
              wk++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("Q"))
            {
              piecePositions[file, 7 - rank] = "wQueen";
              wq++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("B"))
            {
              piecePositions[file, 7 - rank] = "wBishop";
              wb++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("P"))
            {
              piecePositions[file, 7 - rank] = "wPawn";
              wp++;
            }
            else
            {
              //this should be a number indicating a number of blank squares
              blankCount = Convert.ToInt16(fields[fieldIndex].Substring(index, 1));

              piecePositions[file, 7 - rank] = "";

              while (--blankCount > 0)
              {
                file++;

                if (file >= 8) break;

                piecePositions[file, 7 - rank] = "";
              }
            }
            index++;
          }
          fieldIndex++;
        }

        whitesMove = fields[fieldIndex].Equals("w");
        fieldIndex++;

        //default interpretation
        wCastleQ = wCastleK = kCastleQ = kCastleK = false;

        if (!fields[fieldIndex].Equals("-"))
        {
          wCastleK = (fields[fieldIndex].IndexOf("K") >= 0);
          wCastleQ = (fields[fieldIndex].IndexOf("Q") >= 0);
          kCastleK = (fields[fieldIndex].IndexOf("k") >= 0);
          kCastleQ = (fields[fieldIndex].IndexOf("q") >= 0);
        }
        fieldIndex++;

        if (fields[fieldIndex].Equals("-"))
        {
          pawnTarget = "";
        }
        else
        {
          pawnTarget = fields[fieldIndex];
        }

        if (fields.Length == 11)
        {
          //only the last 2 fields are missing, using defaults
          halfMove = 0;
          fullMove = 1;
        }
        else
        {
          fieldIndex++;

          halfMove = Convert.ToInt16(fields[fieldIndex]);

          fieldIndex++;

          fullMove = Convert.ToInt16(fields[fieldIndex]);
        }
      }
    }

    //*******************************************************************************************************
    protected void MovePiece(string src, string dst)
    {
      //the validity of the move should already have been checked
      try
      {
        Point p1 = new Point();
        Point p2 = new Point();

        p1 = FindPosition(src);
        p2 = FindPosition(dst);

        piecePositions[p2.X, p2.Y] = piecePositions[p1.X, p1.Y];
        piecePositions[p1.X, p1.Y] = "";

        if (src.Equals("h1")) wCastleK = false;
        else if (src.Equals("a1")) wCastleQ = false;
        else if (src.Equals("e1")) wCastleK = wCastleQ = false;
        else if (src.Equals("h8")) kCastleK = false;
        else if (src.Equals("a8")) kCastleQ = false;
        else if (src.Equals("e8")) kCastleK = kCastleQ = false;
        else if (dst.Equals("h1")) wCastleK = false;
        else if (dst.Equals("a1")) wCastleQ = false;
        else if (dst.Equals("h8")) kCastleK = false;
        else if (dst.Equals("a8")) kCastleQ = false;
      }
      catch
      {
        MessageBox.Show("MovePiece(src,dst) src = '" + src + "' dst = '" + dst + "'");
      }
    }

    //*******************************************************************************************************
    protected void PromotePiece(string dst, string promoteTo)
    {
      //the validity of the move should already have been checked
      Point p2 = new Point();

      p2 = FindPosition(dst);

      piecePositions[p2.X, p2.Y] = promoteTo;
    }

    #endregion

    #region Piece Moving Checks

    //*******************************************************************************************************
    protected bool IsACastle(ref string pgn)
    {
      if (pgn.Equals("O-O") || pgn.Equals("O-O-O"))
      {
        return true;
      }
      else if (pgn.Equals("0-0") || pgn.Equals("0-0-0"))
      {
        if (pgn.Equals("0-0"))
        {
          pgn = "O-O";
        }
        else
        {
          pgn = "O-O-O";
        }
        return true;
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool Castle(int moveNum, string pgn, bool whiteMove)
    {
      Point p1 = new Point();
      Point p2 = new Point();
      Point p3 = new Point();
      Point p4 = new Point();
      Point p5 = new Point();

      //need to check for checks && empty squares!
      if (whiteMove)
      {
        p1 = FindPosition("e1");
        if (pgn.Equals("O-O"))
        {
          //kingside
          p2 = FindPosition("f1");
          p3 = FindPosition("g1");
          p4 = FindPosition("h1");

          if (piecePositions[p1.X, p1.Y].Equals("wKing") &&
             (piecePositions[p2.X, p2.Y].Length == 0) &&
             (piecePositions[p3.X, p3.Y].Length == 0) &&
              piecePositions[p4.X, p4.Y].Equals("wRook") &&
             wCastleK &&
             !UnderAttack("e1", !whiteMove) &&
             !UnderAttack("f1", !whiteMove) &&
             !UnderAttack("g1", !whiteMove))
          {
            MovePiece("e1", "g1");
            MovePiece("h1", "f1");
            return true;
          }
        }
        else
        {
          //queenside
          p2 = FindPosition("d1");
          p3 = FindPosition("c1");
          p4 = FindPosition("b1");
          p5 = FindPosition("a1");

          if (piecePositions[p1.X, p1.Y].Equals("wKing") &&
             (piecePositions[p2.X, p2.Y].Length == 0) &&
             (piecePositions[p3.X, p3.Y].Length == 0) &&
             (piecePositions[p4.X, p4.Y].Length == 0) &&
              piecePositions[p5.X, p5.Y].Equals("wRook") &&
             wCastleQ &&
             !UnderAttack("e1", !whiteMove) &&
             !UnderAttack("d1", !whiteMove) &&
             !UnderAttack("c1", !whiteMove))
          {
            MovePiece("e1", "c1");
            MovePiece("a1", "d1");
            return true;
          }
        }
      }
      else
      {
        //black move
        p1 = FindPosition("e8");
        if (pgn.Equals("O-O"))
        {
          //kingside
          p2 = FindPosition("f8");
          p3 = FindPosition("g8");
          p4 = FindPosition("h8");

          if (piecePositions[p1.X, p1.Y].Equals("kKing") &&
             (piecePositions[p2.X, p2.Y].Length == 0) &&
             (piecePositions[p3.X, p3.Y].Length == 0) &&
              piecePositions[p4.X, p4.Y].Equals("kRook") &&
             kCastleK &&
             !UnderAttack("e8", !whiteMove) &&
             !UnderAttack("f8", !whiteMove) &&
             !UnderAttack("g8", !whiteMove))
          {
            MovePiece("e8", "g8");
            MovePiece("h8", "f8");
            return true;
          }
        }
        else
        {
          //queenside
          p2 = FindPosition("d8");
          p3 = FindPosition("c8");
          p4 = FindPosition("b8");
          p5 = FindPosition("a8");

          if (piecePositions[p1.X, p1.Y].Equals("kKing") &&
             (piecePositions[p2.X, p2.Y].Length == 0) &&
             (piecePositions[p3.X, p3.Y].Length == 0) &&
             (piecePositions[p4.X, p4.Y].Length == 0) &&
              piecePositions[p5.X, p5.Y].Equals("kRook") &&
             kCastleQ &&
             !UnderAttack("e8", !whiteMove) &&
             !UnderAttack("d8", !whiteMove) &&
             !UnderAttack("c8", !whiteMove))
          {
            MovePiece("e8", "c8");
            MovePiece("a8", "d8");
            return true;
          }
        }
      }
      return false;
    }

    //*******************************************************************************************************
    protected bool IsAPawnPromotion(string src, string dst)
    {
      Point p1 = new Point();
      Point p2 = new Point();

      p1 = FindPosition(src);
      p2 = FindPosition(dst);

      if (piecePositions[p1.X, p1.Y].Equals("wPawn") && src.Substring(1, 1).Equals("7") && dst.Substring(1, 1).Equals("8"))
      {
        if ((piecePositions[p2.X, p2.Y].Length == 0) && src.Substring(0, 1).Equals(dst.Substring(0, 1)))
        {
          return true;
        }
        else if (piecePositions[p2.X, p2.Y].StartsWith("k"))
        {
          if (dst.Equals("a8") && src.Equals("b7"))
          {
            return true;
          }
          else if (dst.Equals("b8") && (src.Equals("a7") || src.Equals("c7")))
          {
            return true;
          }
          else if (dst.Equals("c8") && (src.Equals("b7") || src.Equals("d7")))
          {
            return true;
          }
          else if (dst.Equals("d8") && (src.Equals("c7") || src.Equals("e7")))
          {
            return true;
          }
          else if (dst.Equals("e8") && (src.Equals("d7") || src.Equals("f7")))
          {
            return true;
          }
          else if (dst.Equals("f8") && (src.Equals("e7") || src.Equals("g7")))
          {
            return true;
          }
          else if (dst.Equals("g8") && (src.Equals("f7") || src.Equals("h7")))
          {
            return true;
          }
          else if (dst.Equals("h8") && src.Equals("g7"))
          {
            return true;
          }
        }
      }
      else if (piecePositions[p1.X, p1.Y].Equals("kPawn") && src.Substring(1, 1).Equals("2") && dst.Substring(1, 1).Equals("1"))
      {
        if ((piecePositions[p2.X, p2.Y].Length == 0) && src.Substring(0, 1).Equals(dst.Substring(0, 1)))
        {
          return true;
        }
        else if (piecePositions[p2.X, p2.Y].StartsWith("w"))
        {
          if (dst.Equals("a1") && src.Equals("b2"))
          {
            return true;
          }
          else if (dst.Equals("b1") && (src.Equals("a2") || src.Equals("c2")))
          {
            return true;
          }
          else if (dst.Equals("c1") && (src.Equals("b2") || src.Equals("d2")))
          {
            return true;
          }
          else if (dst.Equals("d1") && (src.Equals("c2") || src.Equals("e2")))
          {
            return true;
          }
          else if (dst.Equals("e1") && (src.Equals("d2") || src.Equals("f2")))
          {
            return true;
          }
          else if (dst.Equals("f1") && (src.Equals("e2") || src.Equals("g2")))
          {
            return true;
          }
          else if (dst.Equals("g1") && (src.Equals("f2") || src.Equals("h2")))
          {
            return true;
          }
          else if (dst.Equals("h1") && src.Equals("g2"))
          {
            return true;
          }
        }
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool IsAPawnPromotion(string pgn)
    {
      if (pgn.StartsWith("a8") ||
          pgn.StartsWith("b8") ||
          pgn.StartsWith("c8") ||
          pgn.StartsWith("d8") ||
          pgn.StartsWith("e8") ||
          pgn.StartsWith("f8") ||
          pgn.StartsWith("g8") ||
          pgn.StartsWith("h8") ||
          pgn.StartsWith("a1") ||
          pgn.StartsWith("b1") ||
          pgn.StartsWith("c1") ||
          pgn.StartsWith("d1") ||
          pgn.StartsWith("e1") ||
          pgn.StartsWith("f1") ||
          pgn.StartsWith("g1") ||
          pgn.StartsWith("h1"))
      {
        return true;
      }
      else if (pgn.StartsWith("bxa8") ||
               pgn.StartsWith("axb8") || pgn.StartsWith("cxb8") ||
               pgn.StartsWith("bxc8") || pgn.StartsWith("dxc8") ||
               pgn.StartsWith("cxd8") || pgn.StartsWith("exd8") ||
               pgn.StartsWith("dxe8") || pgn.StartsWith("fxe8") ||
               pgn.StartsWith("exf8") || pgn.StartsWith("gxf8") ||
               pgn.StartsWith("fxg8") || pgn.StartsWith("hxg8") ||
               pgn.StartsWith("gxh8") ||
               pgn.StartsWith("bxa1") ||
               pgn.StartsWith("axb1") || pgn.StartsWith("cxb1") ||
               pgn.StartsWith("bxc1") || pgn.StartsWith("dxc1") ||
               pgn.StartsWith("cxd1") || pgn.StartsWith("exd1") ||
               pgn.StartsWith("dxe1") || pgn.StartsWith("fxe1") ||
               pgn.StartsWith("exf1") || pgn.StartsWith("gxf1") ||
               pgn.StartsWith("fxg1") || pgn.StartsWith("hxg1") ||
               pgn.StartsWith("gxh1"))
      {
        return true;
      }

      return false;
    }

    //*******************************************************************************************************
    protected string GetPromotion(string pgn, bool whiteMove)
    {
      if (pgn.StartsWith("a8") ||
          pgn.StartsWith("b8") ||
          pgn.StartsWith("c8") ||
          pgn.StartsWith("d8") ||
          pgn.StartsWith("e8") ||
          pgn.StartsWith("f8") ||
          pgn.StartsWith("g8") ||
          pgn.StartsWith("h8") ||
          pgn.StartsWith("a1") ||
          pgn.StartsWith("b1") ||
          pgn.StartsWith("c1") ||
          pgn.StartsWith("d1") ||
          pgn.StartsWith("e1") ||
          pgn.StartsWith("f1") ||
          pgn.StartsWith("g1") ||
          pgn.StartsWith("h1"))
      {
        if (pgn.EndsWith("Q"))
        {
          if (whiteMove) return "wQueen"; else return "kQueen";
        }
        else if (pgn.EndsWith("R"))
        {
          if (whiteMove) return "wRook"; else return "kRook";
        }
        else if (pgn.EndsWith("B"))
        {
          if (whiteMove) return "wBishop"; else return "kBishop";
        }
        else if (pgn.EndsWith("N"))
        {
          if (whiteMove) return "wKnight"; else return "kKnight";
        }
        else
        {
          MessageBox.Show("Invalid pawn promotion syntax for " + (whiteMove ? "White" : "Black") + " - " + pgn);
        }
      }
      else if (pgn.StartsWith("bxa8") ||
               pgn.StartsWith("axb8") || pgn.StartsWith("cxb8") ||
               pgn.StartsWith("bxc8") || pgn.StartsWith("dxc8") ||
               pgn.StartsWith("cxd8") || pgn.StartsWith("exd8") ||
               pgn.StartsWith("dxe8") || pgn.StartsWith("fxe8") ||
               pgn.StartsWith("exf8") || pgn.StartsWith("gxf8") ||
               pgn.StartsWith("fxg8") || pgn.StartsWith("hxg8") ||
               pgn.StartsWith("gxh8") ||
               pgn.StartsWith("bxa1") ||
               pgn.StartsWith("axb1") || pgn.StartsWith("cxb1") ||
               pgn.StartsWith("bxc1") || pgn.StartsWith("dxc1") ||
               pgn.StartsWith("cxd1") || pgn.StartsWith("exd1") ||
               pgn.StartsWith("dxe1") || pgn.StartsWith("fxe1") ||
               pgn.StartsWith("exf1") || pgn.StartsWith("gxf1") ||
               pgn.StartsWith("fxg1") || pgn.StartsWith("hxg1") ||
               pgn.StartsWith("gxh1"))
      {
        if (pgn.EndsWith("Q"))
        {
          if (whiteMove) return "wQueen"; else return "kQueen";
        }
        else if (pgn.EndsWith("R"))
        {
          if (whiteMove) return "wRook"; else return "kRook";
        }
        else if (pgn.EndsWith("B"))
        {
          if (whiteMove) return "wBishop"; else return "kBishop";
        }
        else if (pgn.EndsWith("N"))
        {
          if (whiteMove) return "wKnight"; else return "kKnight";
        }
        else
        {
          MessageBox.Show("Invalid pawn promotion syntax for " + (whiteMove ? "White" : "Black") + " - " + pgn);
        }
      }
      else
      {
        MessageBox.Show("Invalid pawn promotion syntax for " + (whiteMove ? "White" : "Black") + " - " + pgn);
      }

      return "";
    }

    //*******************************************************************************************************
    protected bool FindKingSource(string piece, string dst, ref string src)
    {
      Point dest = FindPosition(dst);
      int x, y;

      foreach (Point p in allowedMovesKing)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (piecePositions[x, y].Equals(piece))
          {
            src = SquareName(x, y);
            return true;
          }
        }
      }

      src = "";
      return false;
    }

    //*******************************************************************************************************
    protected bool FindQueenSource(string piece, string dst, ref string san_src, ref string lan_src, string move)
    {
      string[] sources = new string[] { "", "", "", "", "", "", "", "", "" };

      //find all the posibilities
      if (FindQueenSources(piece, dst, ref sources))
      {
        //find out which one it should be and if unclear ask the user
        return ReturnSource(sources, ref san_src, ref lan_src, move);
      }

      //no sources found at all!
      return false;
    }

    //*******************************************************************************************************
    protected bool FindQueenSources(string piece, string dst, ref string[] src)
    {
      string[] sources2 = new string[] { "", "", "", "", "", "", "", "", "", "" };

      if (FindRookSources(piece, dst, ref src))
      {
        if (FindBishopSources(piece, dst, ref sources2))
        {
          //combine sources
          int i = 0;
          for (; i < src.Length; i++)
          {
            if (src[i].Length == 0) break;
          }

          //need to combine the moves
          foreach (string s in sources2)
          {
            if (s.Length > 0)
            {
              src[i] = s;
              i++;
            }
            else break;
          }
        }
      }
      else if (!FindBishopSources(piece, dst, ref src))
      {
        return false;
      }

      if (!checkQuietly && (src[1].Length > 0))
      {
        Move moveObj = new Move(fenStart, 1);

        if (quietList == null)
          quietList = new Chess();

        try
        {
          string fen = ToFEN(!piece.StartsWith("w"));

          for (int i = 0; i < src.Length; i++)
          {
            if (src[i].Length > 0)
            {
              if (!quietList.CheckMoveQuietly(src[i] + dst, fen, ref moveObj))
              {
                for (int j = i; j < src.Length - 1; j++)
                {
                  src[j] = src[j + 1];
                }
                src[src.Length - 1] = "";
                i--;
              }
            }
            else break;
          }
        }
        catch
        {
          return false;
        }
      }

      return (src[0].Length > 0);
    }

    //*******************************************************************************************************
    protected bool FindRookSource(string piece, string dst, ref string san_src, ref string lan_src, string move)
    {
      string[] sources = new string[] { "", "", "", "", "", "", "", "", "", "" };

      if (FindRookSources(piece, dst, ref sources))
      {
        //find out which one it should be and if unclear ask the user
        return ReturnSource(sources, ref san_src, ref lan_src, move);
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool FindRookSources(string piece, string dst)
    {
      Point dest = FindPosition(dst);
      Point source;
      int x, y;

      foreach (Point p in allowedMovesRook)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (piecePositions[x, y].Equals(piece))
          {
            source = new Point(x, y);
            if (RankIsClear(source, dest, allowedMovesRook))
            {
              return true;
            }
            else if (FileIsClear(source, dest, allowedMovesRook))
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool FindRookSources(string piece, string dst, ref string[] src)
    {
      Point dest = FindPosition(dst);
      Point source;
      int x, y;
      int index = 0;

      foreach (Point p in allowedMovesRook)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (piecePositions[x, y].Equals(piece))
          {
            source = new Point(x, y);
            if (RankIsClear(source, dest, allowedMovesRook))
            {
              src[index] = SquareName(x, y);
              index++;
            }
            else if (FileIsClear(source, dest, allowedMovesRook))
            {
              src[index] = SquareName(x, y);
              index++;
            }
          }
        }
      }

      if (!checkQuietly && (src[1].Length > 0))
      {
        Move moveObj = new Move(fenStart, 1);

        if (quietList == null)
          quietList = new Chess();

        try
        {
          string fen = ToFEN(!piece.StartsWith("w"));

          for (int i = 0; i < src.Length; i++)
          {
            if (src[i].Length > 0)
            {
              if (!quietList.CheckMoveQuietly(src[i] + dst, fen, ref moveObj))
              {
                for (int j = i; j < src.Length - 1; j++)
                {
                  src[j] = src[j + 1];
                }
                src[src.Length - 1] = "";
                i--;
              }
            }
            else break;
          }
        }
        catch
        {
          return false;
        }
      }

      return (src[0].Length > 0);
    }

    //*******************************************************************************************************
    protected bool FindBishopSource(string piece, string dst, ref string san_src, ref string lan_src, string move)
    {
      string[] sources = new string[] { "", "", "", "", "", "", "", "", "" };

      //find all the posibilities
      if (FindBishopSources(piece, dst, ref sources))
      {
        //find out which one it should be and if unclear ask the user
        return ReturnSource(sources, ref san_src, ref lan_src, move);
      }

      //no sources found at all!
      return false;
    }

    //*******************************************************************************************************
    protected bool FindBishopSources(string piece, string dst)
    {
      Point dest = FindPosition(dst);
      Point source;
      int x, y;

      foreach (Point p in allowedMovesBishop)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (piecePositions[x, y].Equals(piece))
          {
            source = new Point(x, y);
            if (DiagonalIsClear(source, dest, allowedMovesBishop))
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool FindBishopSources(string piece, string dst, ref string[] src)
    {
      Point dest = FindPosition(dst);
      Point source;
      int x, y;
      int index = 0;

      foreach (Point p in allowedMovesBishop)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (piecePositions[x, y].Equals(piece))
          {
            source = new Point(x, y);
            if (DiagonalIsClear(source, dest, allowedMovesBishop))
            {
              src[index] = SquareName(x, y);
              index++;
            }
          }
        }
      }

      if (!checkQuietly && (src[1].Length > 0))
      {
        Move moveObj = new Move(fenStart, 1);

        if (quietList == null)
          quietList = new Chess();

        try
        {
          string fen = ToFEN(!piece.StartsWith("w"));

          for (int i = 0; i < src.Length; i++)
          {
            if (src[i].Length > 0)
            {
              if (!quietList.CheckMoveQuietly(src[i] + dst, fen, ref moveObj))
              {
                for (int j = i; j < src.Length - 1; j++)
                {
                  src[j] = src[j + 1];
                }
                src[src.Length - 1] = "";
                i--;
              }
            }
            else break;
          }
        }
        catch
        {
          return false;
        }
      }

      return (src[0].Length > 0);
    }

    //*******************************************************************************************************
    protected bool FindKnightSource(string piece, string dst, ref string san_src, ref string lan_src, string move)
    {
      string[] sources = new string[] { "", "", "", "", "", "", "", "", "", "" };

      //find all the posibilities
      if (FindKnightSources(piece, dst, ref sources))
      {
        //find out which one it should be and if unclear ask the user
        return ReturnSource(sources, ref san_src, ref lan_src, move);
      }

      //no sources found at all!
      return false;
    }

    //*******************************************************************************************************
    protected bool FindKnightSources(string piece, string dst)
    {
      Point dest = FindPosition(dst);
      int x, y;

      foreach (Point p in allowedMovesKnight)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (piecePositions[x, y].Equals(piece))
          {
            return true;
          }
        }
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool FindKnightSources(string piece, string dst, ref string[] src)
    {
      Point dest = FindPosition(dst);
      int x, y, index = 0;

      foreach (Point p in allowedMovesKnight)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (piecePositions[x, y].Equals(piece))
          {
            src[index] = SquareName(x, y);
            index++;
          }
        }
      }

      //make sure all these moves are legal
      if (!checkQuietly && (src[1].Length > 0))
      {
        Move moveObj = new Move(fenStart, 1);

        if (quietList == null)
          quietList = new Chess();

        try
        {
          string fen = ToFEN(!piece.StartsWith("w"));

          for (int i = 0; i < src.Length; i++)
          {
            if (src[i].Length > 0)
            {
              if (!quietList.CheckMoveQuietly(src[i] + dst, fen, ref moveObj))
              {
                for (int j = i; j < src.Length - 1; j++)
                {
                  src[j] = src[j + 1];
                }
                src[src.Length - 1] = "";
                i--;
              }
            }
            else break;
          }
        }
        catch
        {
          return false;
        }
      }

      return (src[0].Length > 0);
    }

    //*******************************************************************************************************
    protected bool ReturnSource(string[] sources, ref string san_src, ref string lan_src, string move)
    {
      if (sources[1].Length == 0)
      {
        //there is only one option so use it
        lan_src = sources[0];
        san_src = "";
        return true;
      }
      else
      {
        //more than 1 possible source - see if they can get confused...
        bool fileUnique, rankUnique;
        fileUnique = rankUnique = true;
        bool originalFound = false;
        string original = "";

        if (lan_src.Length == 2)
        {
          //make sure the original source is an option
          foreach (string s in sources)
          {
            if (s.Length == 2)
            {
              if (s.Equals(lan_src))
              {
                originalFound = true;
                original = lan_src;
                break;
              }
            }
          }
        }
        else if (san_src.Length == 2)
        {
          //see if only one matchs the san source
          //make sure the original source is an option
          foreach (string s in sources)
          {
            if (s.Length == 2)
            {
              if (s.Equals(san_src))
              {
                originalFound = true;
                original = san_src;
                break;
              }
            }
          }
        }
        else if (san_src.Length > 0)
        {
          //see if only one matchs the san source
          //make sure the original source is an option - wa only find one match!
          foreach (string s in sources)
          {
            if (s.Length == 2)
            {
              if (s.StartsWith(san_src))
              {
                if (originalFound)
                {
                  originalFound = false;
                  break;
                }
                else
                {
                  originalFound = true;
                  original = s;
                }
              }
              else if (s.EndsWith(san_src))
              {
                if (originalFound)
                {
                  originalFound = false;
                  break;
                }
                else
                {
                  originalFound = true;
                  original = s;
                }
              }
            }
          }
        }

        if (originalFound)
        {
          //check file
          foreach (string s in sources)
          {
            if (s.Length == 2)
            {
              if (!s.Equals(original))
              {
                if (s.Substring(0, 1).Equals(original.Substring(0, 1)))
                {
                  fileUnique = false;
                  break;
                }
              }
            }
          }

          //check rank
          foreach (string s in sources)
          {
            if (s.Length == 2)
            {
              if (!s.Equals(original))
              {
                if (s.Substring(1, 1).Equals(original.Substring(1, 1)))
                {
                  rankUnique = false;
                  break;
                }
              }
            }
          }

          if (!fileUnique && !rankUnique)
          {
            lan_src = original;
            san_src = lan_src;
            return true;
          }
          else if (fileUnique)
          {
            lan_src = original;
            san_src = lan_src.Substring(0, 1);
            return true;
          }
          else if (rankUnique)
          {
            lan_src = original;
            san_src = lan_src.Substring(1, 1);
            return true;
          }
          //else - a non unique condition...
        }

        //problem
        AmbiguousSource dialog = new AmbiguousSource();

        dialog.textBox1.Text = move;

        for (int i = 0; i < sources.Length; i++)
        {
          if (sources[i].Length > 0)
          {
            if (i == 0)
            {
              dialog.checkBox1.Text = sources[i];
            }
            else if (i == 1)
            {
              dialog.checkBox2.Text = sources[i];
            }
            else if (i == 2)
            {
              dialog.checkBox3.Text = sources[i];
              dialog.checkBox3.Visible = true;
            }
            else if (i == 3)
            {
              dialog.checkBox4.Text = sources[i];
              dialog.checkBox4.Visible = true;
            }
            else if (i == 4)
            {
              dialog.checkBox5.Text = sources[i];
              dialog.checkBox5.Visible = true;
            }
            else if (i == 5)
            {
              dialog.checkBox6.Text = sources[i];
              dialog.checkBox6.Visible = true;
            }
            else if (i == 6)
            {
              dialog.checkBox7.Text = sources[i];
              dialog.checkBox7.Visible = true;
            }
            else if (i == 7)
            {
              dialog.checkBox8.Text = sources[i];
              dialog.checkBox8.Visible = true;
            }
          }
          else break;
        }

        if (dialog.ShowDialog() == DialogResult.OK)
        {
          if (dialog.checkBox1.Checked)
          {
            original = sources[0];
          }
          else if (dialog.checkBox2.Checked)
          {
            original = sources[1];
          }
          else if (dialog.checkBox3.Checked)
          {
            original = sources[2];
          }
          else if (dialog.checkBox4.Checked)
          {
            original = sources[3];
          }
          else if (dialog.checkBox5.Checked)
          {
            original = sources[4];
          }
          else if (dialog.checkBox6.Checked)
          {
            original = sources[5];
          }
          else if (dialog.checkBox7.Checked)
          {
            original = sources[6];
          }
          else if (dialog.checkBox8.Checked)
          {
            original = sources[7];
          }
          else
          {
            return false;
          }

          fileUnique = rankUnique = true;

          //check file
          foreach (string s in sources)
          {
            if (s.Length == 2)
            {
              if (!s.Equals(original))
              {
                if (s.Substring(0, 1).Equals(original.Substring(0, 1)))
                {
                  fileUnique = false;
                  break;
                }
              }
            }
          }

          //check rank
          foreach (string s in sources)
          {
            if (s.Length == 2)
            {
              if (!s.Equals(original))
              {
                if (s.Substring(1, 1).Equals(original.Substring(1, 1)))
                {
                  rankUnique = false;
                  break;
                }
              }
            }
          }

          if (!fileUnique && !rankUnique)
          {
            lan_src = original;
            san_src = lan_src;
            return true;
          }
          else if (fileUnique)
          {
            lan_src = original;
            san_src = lan_src.Substring(0, 1);
            return true;
          }
          else if (rankUnique)
          {
            lan_src = original;
            san_src = lan_src.Substring(1, 1);
            return true;
          }

          return true;
        }
      }

      return false;
    }

    //*******************************************************************************************************
    protected void InvalidMove(int MoveNum, bool whiteMove, string move)
    {
      if (!checkQuietly)
      {
        if (whiteMove) MessageBox.Show("***Invalid White Move #" + fullMove.ToString() + " " + move + "\n" + currentMoveList + "\n" + sanList + "\n" + lanList);
        else MessageBox.Show("***Invalid Black Move #" + (fullMove - 1).ToString() + " " + move + "\n" + currentMoveList + "\n" + sanList + "\n" + lanList);
      }
    }

    //*******************************************************************************************************
    protected bool DiagonalIsClear(Point src, Point dst, Point[] allowedMoves)
    {
      int incrementX = (dst.X - src.X) / Math.Abs(dst.X - src.X);
      int incrementY = (dst.Y - src.Y) / Math.Abs(dst.Y - src.Y);
      Point check = new Point(src.X + incrementX, src.Y + incrementY);

      while (check.X != dst.X)
      {
        if (piecePositions[check.X, check.Y].Length > 0)
        {
          return false;
        }
        check.X += incrementX;
        check.Y += incrementY;
      }

      return true;
    }

    //*******************************************************************************************************
    protected bool RankIsClear(Point src, Point dst, Point[] allowedMoves)
    {
      int maxX = Math.Max(src.X, dst.X);
      int minX = Math.Min(src.X, dst.X);

      if (src.Y != dst.Y) return false;

      //make sure each square between the src and dst squares are empty
      foreach (Point p in allowedMoves)
      {
        if ((minX + p.X > minX) && (minX + p.X < maxX) && (p.Y == 0))
        {
          if (piecePositions[minX + p.X, src.Y].Length > 0)
          {
            return false;
          }
        }
      }

      return true;
    }

    //*******************************************************************************************************
    protected bool FileIsClear(Point src, Point dst, Point[] allowedMoves)
    {
      int maxY = Math.Max(src.Y, dst.Y);
      int minY = Math.Min(src.Y, dst.Y);

      if (src.X != dst.X) return false;

      //make sure each square between the src and dst squares are empty
      foreach (Point p in allowedMoves)
      {
        if ((p.X == 0) && (minY + p.Y > minY) && (minY + p.Y < maxY))
        {
          //string sn = SquareName(src.X, minY + p.Y);
          string pn = piecePositions[src.X, minY + p.Y];
          if (pn.Length > 0)
          {
            return false;
          }
        }
      }

      return true;
    }

    //*******************************************************************************************************
    protected bool OnBoard(int x, int y)
    {
      return ((x >= 0) && (y >= 0) && (x <= 7) && (y <= 7));
    }

    //*******************************************************************************************************
    protected bool UnderAttack(string square, bool byWhite)
    {
      string src = "";

      if (FindKnightSources((byWhite ? "wKnight" : "kKnight"), square))
      {
        return true;
      }
      else if (FindBishopSources((byWhite ? "wBishop" : "kBishop"), square))
      {
        return true;
      }
      else if (FindRookSources((byWhite ? "wRook" : "kRook"), square))
      {
        return true;
      }
      else if (FindBishopSources((byWhite ? "wQueen" : "kQueen"), square))
      {
        return true;
      }
      else if (FindRookSources((byWhite ? "wQueen" : "kQueen"), square))
      {
        return true;
      }
      else if (FindKingSource((byWhite ? "wKing" : "kKing"), square, ref src))
      {
        return true;
      }
      else if (byWhite)
      {
        //by a pawn?
        Point sq = FindPosition(square);

        if (OnBoard(sq.X - 1, sq.Y + 1) && piecePositions[sq.X - 1, sq.Y + 1].Equals("wPawn"))
        {
          return true;
        }
        else if (OnBoard(sq.X + 1, sq.Y + 1) && piecePositions[sq.X + 1, sq.Y + 1].Equals("wPawn"))
        {
          return true;
        }
      }
      else //if(!byWhite)
      {
        //by a pawn?
        Point sq = FindPosition(square);

        if (OnBoard(sq.X - 1, sq.Y - 1) && piecePositions[sq.X - 1, sq.Y - 1].Equals("kPawn"))
        {
          return true;
        }
        else if (OnBoard(sq.X + 1, sq.Y - 1) && piecePositions[sq.X + 1, sq.Y - 1].Equals("kPawn"))
        {
          return true;
        }
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool CheckMate(string kingsSquare, bool whiteMoved)
    {
      //go through all the pieces moves to see if check can be avoided
      Point source, dest = new Point();
      dest = FindPosition(kingsSquare);
      string srcSquare, srcSquare2, dstSquare, dstPiece = "", color = piecePositions[dest.X, dest.Y].Substring(0, 1);
      bool underAttack = true;

      for (int x = 0; x < 8; x++)
      {
        for (int y = 0; y < 8; y++)
        {
          srcSquare = SquareName(x, y);
          source = new Point(x, y);

          if (piecePositions[x, y].StartsWith(color))
          {
            if (piecePositions[x, y].EndsWith("King"))
            {
              foreach (Point p in allowedMovesKing)
              {
                dest.X = x + p.X;
                dest.Y = y + p.Y;
                if (OnBoard(dest.X, dest.Y))
                {
                  dstSquare = SquareName(dest.X, dest.Y);
                  if (!piecePositions[dest.X, dest.Y].StartsWith(color))
                  {
                    //temporarily move the pieces
                    dstPiece = piecePositions[dest.X, dest.Y];
                    piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                    piecePositions[x, y] = "";

                    underAttack = UnderAttack(dstSquare, whiteMoved);

                    //move pieces back
                    piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                    piecePositions[dest.X, dest.Y] = dstPiece;

                    if (!underAttack)
                      return false;
                  }
                }
              }
            }
            else if (piecePositions[x, y].EndsWith("Queen"))
            {
              foreach (Point p in allowedMovesBishop)
              {
                dest.X = x + p.X;
                dest.Y = y + p.Y;
                if (OnBoard(dest.X, dest.Y))
                {
                  if (!piecePositions[dest.X, dest.Y].StartsWith(color))
                  {
                    if (DiagonalIsClear(source, dest, allowedMovesBishop))
                    {
                      //temporarily move the pieces
                      dstPiece = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                      piecePositions[x, y] = "";

                      underAttack = UnderAttack(kingsSquare, whiteMoved);

                      //move pieces back
                      piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = dstPiece;

                      if (!underAttack)
                        return false;
                    }
                  }
                }
              }

              foreach (Point p in allowedMovesRook)
              {
                dest.X = x + p.X;
                dest.Y = y + p.Y;
                if (OnBoard(dest.X, dest.Y))
                {
                  if (!piecePositions[dest.X, dest.Y].StartsWith(color))
                  {
                    if (RankIsClear(source, dest, allowedMovesRook))
                    {
                      //temporarily move the pieces
                      dstPiece = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                      piecePositions[x, y] = "";

                      underAttack = UnderAttack(kingsSquare, whiteMoved);

                      //move pieces back
                      piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = dstPiece;

                      if (!underAttack)
                        return false;
                    }
                    else if (FileIsClear(source, dest, allowedMovesRook))
                    {
                      //temporarily move the pieces
                      dstPiece = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                      piecePositions[x, y] = "";

                      underAttack = UnderAttack(kingsSquare, whiteMoved);

                      //move pieces back
                      piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = dstPiece;

                      if (!underAttack)
                        return false;
                    }
                  }
                }
              }
            }
            else if (piecePositions[x, y].EndsWith("Bishop"))
            {
              foreach (Point p in allowedMovesBishop)
              {
                dest.X = x + p.X;
                dest.Y = y + p.Y;
                if (OnBoard(dest.X, dest.Y))
                {
                  if (!piecePositions[dest.X, dest.Y].StartsWith(color))
                  {
                    if (DiagonalIsClear(source, dest, allowedMovesBishop))
                    {
                      //temporarily move the pieces
                      dstPiece = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                      piecePositions[x, y] = "";

                      underAttack = UnderAttack(kingsSquare, whiteMoved);

                      //move pieces back
                      piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = dstPiece;

                      if (!underAttack)
                        return false;
                    }
                  }
                }
              }
            }
            else if (piecePositions[x, y].EndsWith("Knight"))
            {
              foreach (Point p in allowedMovesKnight)
              {
                dest.X = x + p.X;
                dest.Y = y + p.Y;
                if (OnBoard(dest.X, dest.Y))
                {
                  if (!piecePositions[dest.X, dest.Y].StartsWith(color))
                  {
                    //temporarily move the pieces
                    dstPiece = piecePositions[dest.X, dest.Y];
                    piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                    piecePositions[x, y] = "";

                    underAttack = UnderAttack(kingsSquare, whiteMoved);

                    //move pieces back
                    piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                    piecePositions[dest.X, dest.Y] = dstPiece;

                    if (!underAttack)
                      return false;
                  }
                }
              }
            }
            else if (piecePositions[x, y].EndsWith("Rook"))
            {
              foreach (Point p in allowedMovesRook)
              {
                dest.X = x + p.X;
                dest.Y = y + p.Y;
                if (OnBoard(dest.X, dest.Y))
                {
                  if (!piecePositions[dest.X, dest.Y].StartsWith(color))
                  {
                    if (RankIsClear(source, dest, allowedMovesRook))
                    {
                      //temporarily move the pieces
                      dstPiece = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                      piecePositions[x, y] = "";

                      underAttack = UnderAttack(kingsSquare, whiteMoved);

                      //move pieces back
                      piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = dstPiece;

                      if (!underAttack)
                        return false;
                    }
                    else if (FileIsClear(source, dest, allowedMovesRook))
                    {
                      //temporarily move the pieces
                      dstPiece = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                      piecePositions[x, y] = "";

                      underAttack = UnderAttack(kingsSquare, whiteMoved);

                      //move pieces back
                      piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                      piecePositions[dest.X, dest.Y] = dstPiece;

                      if (!underAttack)
                        return false;
                    }
                  }
                }
              }
            }
            else if (whiteMoved)
            {
              //move black pawn
              dest.X = x;
              dest.Y = y + 1;

              if (OnBoard(dest.X, dest.Y) && (piecePositions[dest.X, dest.Y].Length == 0))
              {
                //pawn can move straight forward
                //temporarily move the pieces
                piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                piecePositions[x, y] = "";

                underAttack = UnderAttack(kingsSquare, whiteMoved);

                //move pieces back
                piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = "";

                if (!underAttack)
                  return false;
              }

              if (srcSquare.EndsWith("7") && (piecePositions[dest.X, dest.Y].Length == 0))
              {
                //2 squares in a row must be clear
                dest.X = x;
                dest.Y = y + 2;

                if (piecePositions[dest.X, dest.Y].Length == 0)
                {
                  //pawn can move straight forward
                  //temporarily move the pieces
                  piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                  piecePositions[x, y] = "";

                  underAttack = UnderAttack(kingsSquare, whiteMoved);

                  //move pieces back
                  piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                  piecePositions[dest.X, dest.Y] = "";

                  if (!underAttack)
                    return false;
                }
              }

              dest.X = x + 1;
              dest.Y = y + 1;

              if (OnBoard(dest.X, dest.Y) && piecePositions[dest.X, dest.Y].StartsWith("w"))
              {
                //pawn can take a piece
                //temporarily move the pieces
                dstPiece = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                piecePositions[x, y] = "";

                underAttack = UnderAttack(kingsSquare, whiteMoved);

                //move pieces back
                piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = dstPiece;

                if (!underAttack)
                  return false;
              }

              dest.X = x - 1;
              dest.Y = y + 1;

              if (OnBoard(dest.X, dest.Y) && piecePositions[dest.X, dest.Y].StartsWith("w"))
              {
                //pawn can take a piece
                //temporarily move the pieces
                dstPiece = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                piecePositions[x, y] = "";

                underAttack = UnderAttack(kingsSquare, whiteMoved);

                //move pieces back
                piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = dstPiece;

                if (!underAttack)
                  return false;
              }

              //enpassant check to get out of check...
              if ((pawnTarget.Length > 0) && srcSquare.EndsWith("4"))
              {
                //may have a problem with a reversed board
                if (OnBoard(x + 1, y + 1) && (piecePositions[x + 1, y + 1].Length == 0) && piecePositions[x + 1, y].Equals("wPawn"))
                {
                  srcSquare2 = SquareName(x + 1, y);

                  if (srcSquare2.Equals(pawnTarget))
                  {
                    //pawn can take a piece with enpassant
                    //temporarily move the pieces
                    piecePositions[x + 1, y + 1] = piecePositions[x, y];
                    piecePositions[x, y] = "";
                    piecePositions[x + 1, y] = "";

                    underAttack = UnderAttack(kingsSquare, whiteMoved);

                    //move pieces back
                    piecePositions[x, y] = piecePositions[x + 1, y + 1];
                    piecePositions[x + 1, y + 1] = "";
                    piecePositions[x + 1, y] = "kPawn";

                    if (!underAttack)
                      return false;
                  }
                }

                //may have a problem with a reversed board
                if (OnBoard(x - 1, y + 1) && (piecePositions[x - 1, y + 1].Length == 0) && piecePositions[x - 1, y].Equals("wPawn"))
                {
                  srcSquare2 = SquareName(x - 1, y);

                  if (srcSquare2.Equals(pawnTarget))
                  {
                    //pawn can take a piece with enpassant
                    //temporarily move the pieces
                    piecePositions[x - 1, y + 1] = piecePositions[x, y];
                    piecePositions[x, y] = "";
                    piecePositions[x - 1, y] = "";

                    underAttack = UnderAttack(kingsSquare, whiteMoved);

                    //move pieces back
                    piecePositions[x, y] = piecePositions[x - 1, y + 1];
                    piecePositions[x - 1, y + 1] = "";
                    piecePositions[x - 1, y] = "kPawn";

                    if (!underAttack)
                      return false;
                  }
                }
              }
            }
            else //if (!whiteMoved)
            {
              //move white pawn
              dest.X = x + 0;
              dest.Y = y - 1;

              if (OnBoard(dest.X, dest.Y) && (piecePositions[dest.X, dest.Y].Length == 0))
              {
                //pawn can move straight forward
                //temporarily move the pieces
                piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                piecePositions[x, y] = "";

                underAttack = UnderAttack(kingsSquare, whiteMoved);

                //move pieces back
                piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = "";

                if (!underAttack)
                  return false;
              }

              if (srcSquare.EndsWith("2") && (piecePositions[dest.X, dest.Y].Length == 0))
              {
                //2 squares in a row must be clear
                dest.X = x + 0;
                dest.Y = y - 2;
                if (piecePositions[dest.X, dest.Y].Length == 0)
                {
                  //pawn can move straight forward
                  //temporarily move the pieces
                  piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                  piecePositions[x, y] = "";

                  underAttack = UnderAttack(kingsSquare, whiteMoved);

                  //move pieces back
                  piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                  piecePositions[dest.X, dest.Y] = "";

                  if (!underAttack)
                    return false;
                }
              }

              dest.X = x + 1;
              dest.Y = y + -1;

              if (OnBoard(dest.X, dest.Y) && piecePositions[dest.X, dest.Y].StartsWith("k"))
              {
                //pawn can take a piece
                //temporarily move the pieces
                dstPiece = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                piecePositions[x, y] = "";

                underAttack = UnderAttack(kingsSquare, whiteMoved);

                //move pieces back
                piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = dstPiece;

                if (!underAttack)
                  return false;
              }

              dest.X = x - 1;
              dest.Y = y - 1;

              if (OnBoard(dest.X, dest.Y) && piecePositions[dest.X, dest.Y].StartsWith("k"))
              {
                //pawn can take a piece
                //temporarily move the pieces
                dstPiece = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = piecePositions[x, y];
                piecePositions[x, y] = "";

                underAttack = UnderAttack(kingsSquare, whiteMoved);

                //move pieces back
                piecePositions[x, y] = piecePositions[dest.X, dest.Y];
                piecePositions[dest.X, dest.Y] = dstPiece;

                if (!underAttack)
                  return false;
              }

              //enpassant check to get out of check...
              if ((pawnTarget.Length > 0) && srcSquare.EndsWith("5"))
              {
                if (OnBoard(x + 1, y - 1) && (piecePositions[x + 1, y - 1].Length == 0) && piecePositions[x + 1, y].Equals("kPawn"))
                {
                  srcSquare2 = SquareName(x + 1, y);

                  if (srcSquare2.Equals(pawnTarget))
                  {
                    //pawn can take a piece with enpassant
                    //temporarily move the pieces
                    piecePositions[x + 1, y - 1] = piecePositions[x, y];
                    piecePositions[x, y] = "";
                    piecePositions[x + 1, y] = "";

                    underAttack = UnderAttack(kingsSquare, whiteMoved);

                    //move pieces back
                    piecePositions[x, y] = piecePositions[x + 1, y - 1];
                    piecePositions[x + 1, y - 1] = "";
                    piecePositions[x + 1, y] = "kPawn";

                    if (!underAttack)
                      return false;
                  }
                }

                if (OnBoard(x - 1, y - 1) && (piecePositions[x - 1, y - 1].Length == 0) && piecePositions[x - 1, y].Equals("kPawn"))
                {
                  srcSquare2 = SquareName(x - 1, y);

                  if (srcSquare2.Equals(pawnTarget))
                  {
                    //pawn can take a piece with enpassant
                    //temporarily move the pieces
                    piecePositions[x - 1, y - 1] = piecePositions[x, y];
                    piecePositions[x, y] = "";
                    piecePositions[x - 1, y] = "";

                    underAttack = UnderAttack(kingsSquare, whiteMoved);

                    //move pieces back
                    piecePositions[x, y] = piecePositions[x - 1, y - 1];
                    piecePositions[x - 1, y - 1] = "";
                    piecePositions[x - 1, y] = "kPawn";

                    if (!underAttack)
                      return false;
                  }
                }
              }
            }
          }
        }
      }

      return true;
    }

    #endregion

    #region Board Stuff

    //*******************************************************************************************************
    protected Point FindPosition(string square)
    {
      Point p;

      for (int x = 0; x < 8; x++)
      {
        for (int y = 0; y < 8; y++)
        {
          if (boardLayout[x, y].Equals(square))
          {
            p = new Point(x, y);
            return p;
          }
        }
      }

      //this should only happen if the input is bad
      return new Point(-1, -1);
    }

    //*******************************************************************************************************
    protected string SquareName(int x, int y)
    {
      return boardLayout[x, y];
    }

    //*******************************************************************************************************
    protected string fenLocation(int rank, int file)
    {
      //always viewed from the same perspective
      rank = rank + 1;

      if ((rank < 1) || (rank > 8) || (file < 0) || (file > 7))
      {
        MessageBox.Show("Invalid Rank(" + rank.ToString() + ") or File(" + (file + 1).ToString() + ") indexes specified...");
        return "a1";
      }
      else if (file == 0)
      {
        return "a" + rank.ToString();
      }
      else if (file == 1)
      {
        return "b" + rank.ToString();
      }
      else if (file == 2)
      {
        return "c" + rank.ToString();
      }
      else if (file == 3)
      {
        return "d" + rank.ToString();
      }
      else if (file == 4)
      {
        return "e" + rank.ToString();
      }
      else if (file == 5)
      {
        return "f" + rank.ToString();
      }
      else if (file == 6)
      {
        return "g" + rank.ToString();
      }

      return "h" + rank.ToString();
    }

    #endregion
  }


  #region Move Class

  //********************************************************************************
  // make a move

  public class Move
  {
    public string comment = ""; //a post move comment
    public string fen = "";     //Forsyth-Edwards Notation resulting string after the move takes place
    public string lan = "";     //Long Algebraic Notation
    public string san = "";     //Standard Algebraic Notation

    public int nag = 0;         //Numeric Annotation Glyph
    public int fullMove;
    public int halfMove;

    public bool gameOver = false;
    public bool inCheck = false;
    public bool staleMate = false;
    public bool insufficientMaterial = false;
    public bool insufficientWhite = false;
    public bool insufficientBlack = false;

    public readonly string priorFen;     //Forsyth-Edwards Notation resulting after the previous move takes place
    public readonly bool whitesMove;
    public readonly string[] padding = new string[7] { "      ", "     ", "    ", "   ", "  ", " ", "" };

    //used for seving the rav lists while reading a movelist
    public string rav = "";     //Recursive Annotation Variation lists
    public bool ravExists;

    public TreeNode[] main;

    public Move(string PriorFEN, int FullMove)
    {
      priorFen = PriorFEN;
      whitesMove = (priorFen.IndexOf(" w ") > 0);
      fullMove = FullMove;
      fen = priorFen;
    }

    //create the rav movelists
    public bool ProcessRAVMoveLists(bool showSAN, Color whiteBK, Color blackBK)
    {
      if (ravExists)
      {
        ArrayList ravArray = new ArrayList();  //Recursive Annotation Variation lists

        //this does not work with more than one level of rav
        rav = rav.Trim();
        string tmp = "";
        int alternativeMoveCount = 0;

        foreach (char c in rav)
        {
          if ((alternativeMoveCount > 1) || ((alternativeMoveCount == 1) && (c != ')')))
          {
            tmp += c.ToString();
          }

          if (c == '(')
          {
            alternativeMoveCount++;
          }
          else if (c == ')')
          {
            alternativeMoveCount--;
          }

          if (alternativeMoveCount == 0)
          {
            tmp = tmp.Trim();
            if (tmp.Length > 0) ravArray.Add(tmp);
            tmp = "";
          }
        }

        Chess virtualChessBoard = new Chess();
        ArrayList[] Moves = new ArrayList[ravArray.Count];
        main = new TreeNode[ravArray.Count];

        for (int ravNum = 0; ravNum < ravArray.Count; ravNum++)
        {
          Moves[ravNum] = virtualChessBoard.LoadList(ravArray[ravNum].ToString(), priorFen, true);
          main[ravNum] = new TreeNode("RAV       ");
          main[ravNum].Tag = Moves[ravNum][0];

          for (int move = 0; move < Moves[ravNum].Count; move++)
          {
            ((Move)(Moves[ravNum][move])).ProcessRAVMoveLists(showSAN, whiteBK, blackBK);

            if (showSAN)
            {
              if (((Move)(Moves[ravNum][move])).whitesMove)
              {
                if (((Move)(Moves[ravNum][move])).ravExists)
                {
                  ((TreeNode)(main[ravNum])).Nodes.Add(new TreeNode(padding[((Move)(Moves[ravNum][move])).fullMove.ToString().Length + 3] + ((Move)(Moves[ravNum][move])).fullMove.ToString() + ". " + ((Move)(Moves[ravNum][move])).san + padding[((Move)(Moves[ravNum][move])).san.Length - 1], ((Move)(Moves[ravNum][move])).main));
                }
                else
                {
                  ((TreeNode)(main[ravNum])).Nodes.Add(new TreeNode(padding[((Move)(Moves[ravNum][move])).fullMove.ToString().Length + 3] + ((Move)(Moves[ravNum][move])).fullMove.ToString() + ". " + ((Move)(Moves[ravNum][move])).san + padding[((Move)(Moves[ravNum][move])).san.Length - 1]));
                }
                ((TreeNode)(main[ravNum])).Nodes[((TreeNode)(main[ravNum])).Nodes.Count - 1].BackColor = whiteBK;
                ((TreeNode)(main[ravNum])).Nodes[((TreeNode)(main[ravNum])).Nodes.Count - 1].Tag = Moves[ravNum][move];
              }
              else
              {
                if (((Move)(Moves[ravNum][move])).ravExists)
                {
                  ((TreeNode)(main[ravNum])).Nodes.Add(new TreeNode("     " + ((Move)(Moves[ravNum][move])).san + padding[((Move)(Moves[ravNum][move])).san.Length - 1], ((Move)(Moves[ravNum][move])).main));
                }
                else
                {
                  ((TreeNode)(main[ravNum])).Nodes.Add(new TreeNode("     " + ((Move)(Moves[ravNum][move])).san + padding[((Move)(Moves[ravNum][move])).san.Length - 1]));
                }
                ((TreeNode)(main[ravNum])).Nodes[((TreeNode)(main[ravNum])).Nodes.Count - 1].BackColor = blackBK;
                ((TreeNode)(main[ravNum])).Nodes[((TreeNode)(main[ravNum])).Nodes.Count - 1].Tag = Moves[ravNum][move];
              }
            }
            else
            {
              if (((Move)(Moves[ravNum][move])).whitesMove)
              {
                if (((Move)(Moves[ravNum][move])).ravExists)
                {
                  ((TreeNode)(main[ravNum])).Nodes.Add(new TreeNode(padding[((Move)(Moves[ravNum][move])).fullMove.ToString().Length + 3] + ((Move)(Moves[ravNum][move])).fullMove.ToString() + ". " + ((Move)(Moves[ravNum][move])).lan + padding[((Move)(Moves[ravNum][move])).lan.Length + 1], ((Move)(Moves[ravNum][move])).main));
                }
                else
                {
                  ((TreeNode)(main[ravNum])).Nodes.Add(new TreeNode(padding[((Move)(Moves[ravNum][move])).fullMove.ToString().Length + 3] + ((Move)(Moves[ravNum][move])).fullMove.ToString() + ". " + ((Move)(Moves[ravNum][move])).lan + padding[((Move)(Moves[ravNum][move])).lan.Length + 1]));
                }
                ((TreeNode)(main[ravNum])).Nodes[((TreeNode)(main[ravNum])).Nodes.Count - 1].BackColor = whiteBK;
                ((TreeNode)(main[ravNum])).Nodes[((TreeNode)(main[ravNum])).Nodes.Count - 1].Tag = Moves[ravNum][move];
              }
              else
              {
                if (((Move)(Moves[ravNum][move])).ravExists)
                {
                  ((TreeNode)(main[ravNum])).Nodes.Add(new TreeNode("     " + ((Move)(Moves[ravNum][move])).lan + padding[((Move)(Moves[ravNum][move])).lan.Length + 1], ((Move)(Moves[ravNum][move])).main));
                }
                else
                {
                  ((TreeNode)(main[ravNum])).Nodes.Add(new TreeNode("     " + ((Move)(Moves[ravNum][move])).lan + padding[((Move)(Moves[ravNum][move])).lan.Length + 1]));
                }
                ((TreeNode)(main[ravNum])).Nodes[((TreeNode)(main[ravNum])).Nodes.Count - 1].BackColor = blackBK;
                ((TreeNode)(main[ravNum])).Nodes[((TreeNode)(main[ravNum])).Nodes.Count - 1].Tag = Moves[ravNum][move];
              }
            }
          }
        }
      }
      return true;
    }

    //add rav text
    public void AddRav(string Rav)
    {
      if (Rav.Trim().Length > 0)
      {
        if (rav.Length > 0)
        {
          if (rav.EndsWith(")") && Rav.StartsWith("("))
          {
            rav += Rav.Trim();
          }
          else
          {
            rav += (" " + Rav.Trim());
          }
        }
        else
        {
          rav = Rav.Trim();
        }
        ravExists = true;
      }
    }

    public void AddComment(string Comment)
    {
      comment += (Comment.Trim() + " ");
    }
  }

  #endregion

}

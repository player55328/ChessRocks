using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Media;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Collections;
//needed for the dll import
using System.Runtime.InteropServices;

using ns_Invisible;
using ns_PGNHeader;
using ns_Promotion;
using ns_ini;

namespace ChessRocks
{
  public partial class ChessRocks : Form
  {
    #region Import Stuff
      
    [DllImport("user32.dll")] static extern int GetScrollPos(int hWnd, int nBar);
    [DllImport("user32.dll")] static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hWnd, int nIndex);
 
    private const int GWL_STYLE = -16;
    private const int WS_VSCROLL = 0x00200000;
    private const int WS_HSCROLL = 0x00100000;
    private const int SB_HORZ = 0x0;
    private const int SB_VERT = 0x1;

    #endregion

    #region Member Variables

    public bool constructionError = true;
      
    //these are to be replaced by controls in a parent
    protected string lastFEN = "";
    protected string puzzleFEN = "";
    protected string pawnTarget = "";
    protected string onPanel = "";
    protected string loadedFileName = "";
    protected string newPosition = "", newMove = "", lastPosition;
    protected string piecePath;
    protected string soundPath;
    protected string pgnPath = "";
    protected string lastRAV = "";
    protected string lastBestMoveRAV = "";
    protected string lastBestMoveAnalysis = "";
    protected string iniDeepTime = "";
    protected string iniQuestionable = "";
    protected string iniPoor = "";
    protected string iniVeryPoor = "";
    protected string iniCPUTime = "";
    protected string startDeepAnalysisTime = "";
    protected string lastBestDeep = "";
    protected string lastAnalysisDeep = "";
    protected string pgnPad = "                                                                                                                                       ";
    protected string resourcesPath = "";

    protected string[,] boardLayout;
    protected string[,] reversedBoardLayout;
    protected string[] padding = new string[7] { "      ", "     ", "    ", "   ", "  ", " ", "" };

    protected char[] spaceDelim = " ".ToCharArray();

    protected bool whitesMove = true;
    protected bool wCastleQ, wCastleK, kCastleQ, kCastleK;
    protected bool inCheck = false;
    protected bool staleMate = false;
    protected bool insufficientMaterial = false;
    protected bool gameOver = false;
    protected bool reversedBoard = false;
    protected bool showSquareNames = true;
    protected bool checkQuietly = false;
    protected bool disableDD = false;
    protected bool runClock = false;
    protected bool runCPUClock = false;
    protected bool showSAN;
    protected bool moveOK = false;
    protected bool goOK = true;
    protected bool uciok = false;
    protected bool uciok2 = false;
    protected bool readyok = false;
    protected bool readyok2 = false;
    protected bool nagDropped = false;
    protected bool displayReady = false;
    protected bool pgnLoaded = false;
    protected bool LastColorClockChangeWasWhite = true;
    protected bool resumingGame = false;
    protected bool findingBestMove = false;
    protected bool betterMove = false;
    protected bool gamePaused = false;
    protected bool startingMoveMade = false;
    protected bool optionInitialization = true;
    protected bool formClosing = false;
    protected bool updateComment = false;
    protected bool puzzle = false;
    protected bool puzzleComputerWhite = false;
    protected bool hasVScroll = false;
    protected bool hasHScroll = false;
    protected bool enableSound = true;

    //for counting pawn Promotions
    protected int halfMove = 0;
    protected int fullMove = 1;
    protected int mainPlycount = 0;
    protected int lastManualRow = 0;
    protected int whiteBetter, blackBetter, whiteQuestionable, blackQuestionable;
    protected int whitePoor, blackPoor, whiteVeryPoor, blackVeryPoor;
    //protected int scrollMove = 0;

    public Bitmap wRook, wPawn, wQueen, wKing, wBishop, wKnight;
    public Bitmap kRook, kPawn, kQueen, kKing, kBishop, kKnight;
    protected Bitmap blank;

    public Bitmap wRookOriginal, wPawnOriginal, wQueenOriginal, wKingOriginal, wBishopOriginal, wKnightOriginal;
    public Bitmap kRookOriginal, kPawnOriginal, kQueenOriginal, kKingOriginal, kBishopOriginal, kKnightOriginal;
    protected Bitmap blankOriginal;

    protected Image wRooksm, wPawnsm, wQueensm, wBishopsm, wKnightsm;
    protected Image kRooksm, kPawnsm, kQueensm, kBishopsm, kKnightsm;
    protected Image blanksm;

    protected Point mouseDownLocation;

    protected Color whiteBK;
    protected Color blackBK;

    protected ns_Invisible.Invisible inv = new ns_Invisible.Invisible();

    //stuff for the virtual board - used to validate / convert move lists
    protected Chess virtualChessBoard = new Chess();

    public ns_ini.ini chess;

    protected DateTime dtClock, initialWhiteClock, initialBlackClock;
    protected DateTime timeSinceLastChange;

    protected DateTimePicker dtpWhite, dtpBlack, dtpComputer, dtpGeneral;

    protected GameList gameList = new GameList();

    protected string enginePath;
    protected ProcessStartInfo engineSI = null;
    protected Process engine = null;
    protected bool engineActive = false;

    protected string enginePath2;
    protected ProcessStartInfo engineSI2 = null;
    protected Process engine2 = null;
    protected bool engineActive2 = false;

    #endregion

    #region Constructor

    public ChessRocks()
    {
      InitializeComponent();

      savePGN.BackColor = Color.LightBlue;

      InitNag();

      resourcesPath = Path.GetFullPath("chess.ini");
      chess = new ini(resourcesPath);

      resourcesPath = resourcesPath.Replace("chess.ini", "Resources\\");
      if (!Directory.Exists(resourcesPath))
      {
        resourcesPath = resourcesPath.Replace("chess.ini", "");
      }

      dtpWhite = new DateTimePicker();
      dtpBlack = new DateTimePicker();
      dtpComputer = new DateTimePicker();
      dtpGeneral = new DateTimePicker();

      pieceMoving.AccessibleName = "";
      pieceMoving.AccessibleDescription = "";

      InitBoardLayoutArrays();

      chess.GetINIValue("SETTINGS", "SOUND_PATH", ref soundPath, resourcesPath + "Sounds\\Default");
      enableSound = chess.GetINIValue("SETTINGS", "ENABLE_SOUND", true);
      //if the sound paths are good allow sounds to be turned on
      if (enableSound) enableSound = CheckSoundPaths();

      chess.GetINIValue("SETTINGS", "PIECE_PATH", ref piecePath, resourcesPath + "Pieces\\Default");
      if (!CheckPiecePaths()) return;

      //these will only be checked when they are actually used
      chess.GetINIValue("SETTINGS", "ENGINE_PATH", ref enginePath, resourcesPath + "Engines\\Houdini\\Houdini_15a_w32.exe");
      chess.GetINIValue("SETTINGS", "ENGINE_PATH2", ref enginePath2, resourcesPath + "Engines\\Houdini\\Houdini_15a_w32.exe");
        
      chess.GetINIValue("SETTINGS", "PGN_PATH", ref pgnPath, resourcesPath + "PGN");
      if (!Directory.Exists(pgnPath))
      {
        //make sure a valid path is used
        pgnPath = resourcesPath;
      }
      
      showSAN = chess.GetINIValue("SETTINGS", "SHOW_SAN", true);
      reversedBoard = chess.GetINIValue("SETTINGS", "SHOW_REVERSED_BOARD", false);
      showSquareNames = !chess.GetINIValue("SETTINGS", "SHOW_SQUARE_NAMES", true);

      ToggleLabels();
      chess.PutINIValue("SETTINGS", "SHOW_SQUARE_NAMES", showSquareNames);

      if (reversedBoard) ReverseBoard();

      int red, green, blue;

      red = green = blue = 0;
      chess.GetINIValue("SETTINGS", "WHITE_RED", ref red, 192);
      chess.GetINIValue("SETTINGS", "WHITE_GREEN", ref green, 192);
      chess.GetINIValue("SETTINGS", "WHITE_BLUE", ref blue, 192);

      whiteBK = new Color();
      whiteBK = Color.FromArgb(255, red, green, blue);

      chess.GetINIValue("SETTINGS", "BLACK_RED", ref red, 113);
      chess.GetINIValue("SETTINGS", "BLACK_GREEN", ref green, 111);
      chess.GetINIValue("SETTINGS", "BLACK_BLUE", ref blue, 100);

      chess.GetINIValue("SETTINGS", "DEEP_TIME_HOURS", ref iniDeepTime, "5.0");
      chess.GetINIValue("SETTINGS", "QUESTIONABLE_PAWNS", ref iniQuestionable, "0.2");
      chess.GetINIValue("SETTINGS", "POOR_PAWNS", ref iniPoor, "0.6");
      chess.GetINIValue("SETTINGS", "VERY_POOR_PAWNS", ref iniVeryPoor, "1.2");
      chess.GetINIValue("SETTINGS", "CPU_TIME_SECONDS", ref iniCPUTime, "5");

      double testValue;

      try
      {
        testValue = Convert.ToDouble(iniDeepTime);
        deepTime.Text = iniDeepTime;
      }
      catch
      {
        deepTime.Text = "5.0";
      }

      try
      {
        testValue = Convert.ToDouble(iniQuestionable);
        questionableMove.Text = iniQuestionable;
      }
      catch
      {
        questionableMove.Text = "0.2";
      }

      try
      {
        testValue = Convert.ToDouble(iniPoor);
        poorMove.Text = iniPoor;
      }
      catch
      {
        poorMove.Text = "0.6";
      }

      try
      {
        testValue = Convert.ToDouble(iniVeryPoor);
        veryPoorMove.Text = iniVeryPoor;
      }
      catch
      {
        veryPoorMove.Text = "1.2";
      }

      int testValueI;

      try
      {
        testValueI = Convert.ToInt32(iniCPUTime);
        computerTime.Text = iniCPUTime;
      }
      catch
      {
        computerTime.Text = "5";
      }

      blackBK = new Color();
      blackBK = Color.FromArgb(255, red, green, blue);
      UpdateSquareColor();

      LoadPieces(piecePath);

      oPGN.FileName = "newgame.pgn";
      UpdateTitle();

      whiteDraw.Text = "Abort";
      blackDraw.Text = "Start";

      gameResult.Text = "*";

      ResetClocks();
      ResetComputerClock();

      ClearPieceMoving();
      SetupForNextLiveGame();

      moveTree.Nodes[0].Tag = new Move(virtualChessBoard.fenStart, 1);
      duplicateFENs.Text = "1";
      theBoard.Size = new Size(8 * (split2.Panel1.Size.Width / 8), 8 * ((split2.Panel1.Size.Height - 40) / 8));

      masterTimer.Start();
      ResizetheBoard();

      // 
      // inv
      // 
      inv.Size = new Size(a8.Size.Width, a8.Size.Height);
      inv.BackgroundImage = blank;
      inv.Location = new Point(0, 0);
      inv.Visible = true;
      inv.Refresh();
      inv.Visible = false;

      mouseDownLocation.X = 0;
      mouseDownLocation.Y = 0;

      //for debuging
      //messageBox.Visible = true;
      constructionError = false;
    }

    #endregion

    #region Other Window Stuff

    //*******************************************************************************************************
    protected void UpdateTitle()
    {
      if (oPGN.FileName.Equals("clipboard.pgn"))
      {
        loadedFileName = "clipboard.pgn";
      }
      else if (oPGN.FileName.Equals("newgame.pgn"))
      {
        loadedFileName = "newgame.pgn";
      }
      else if (oPGN.FileName.Length > 0)
      {
        loadedFileName = Path.GetFileName(oPGN.FileName);
      }
      else
      {
        loadedFileName = "unknown.pgn";
      }

      System.Reflection.Assembly ExecutingApp = System.Reflection.Assembly.GetExecutingAssembly();
      System.Reflection.AssemblyName Name = ExecutingApp.GetName();
      int UnwantedIndex = Name.Version.ToString().LastIndexOf(".0.0");
      this.Text = "Chess Rocks! : Version " + Name.Version.ToString().Remove(UnwantedIndex, 4) + " : " + loadedFileName;
    }

    //*******************************************************************************************************
    protected Image ResizeImage(Image i, int toX, int toY)
    {
      Bitmap b = new Bitmap(toX, toY);
      Graphics g = Graphics.FromImage((Image)b);
      g.InterpolationMode = InterpolationMode.HighQualityBicubic;

      g.DrawImage(i, 0, 0, toX, toY);
      g.Dispose();

      return b;
    }

    //*******************************************************************************************************
    protected Bitmap ResizeImage(Bitmap i, int toX, int toY)
    {
      Bitmap b = new Bitmap(toX, toY);
      Graphics g = Graphics.FromImage((Image)b);
      g.InterpolationMode = InterpolationMode.HighQualityBicubic;

      g.DrawImage(i, 0, 0, toX, toY);
      g.Dispose();

      return b;
    }

    //*******************************************************************************************************
    private void ChessRocks_Load(object sender, EventArgs e)
    {
      split1.Panel2MinSize = 144;
      split2.Panel2MinSize = 400;
    }

    //*******************************************************************************************************
    private void ChessRocks_FormClosing(object sender, FormClosingEventArgs e)
    {
        formClosing = true;

        if (engine != null)
        {
            //actually stopping the engine
            StopYourEngine(1);
        }

        if (engine2 != null)
        {
            //actually stopping the engine
            StopYourEngine(2);
        }

        chess.PutINIValue("SETTINGS", "X", this.Left);
        chess.PutINIValue("SETTINGS", "Y", this.Top);
        chess.PutINIValue("SETTINGS", "WIDTH", this.Right - this.Left);
        chess.PutINIValue("SETTINGS", "HEIGHT", this.Bottom - this.Top);
        chess.PutINIValue("SETTINGS", "SPLIT1", split1.SplitterDistance);
        chess.PutINIValue("SETTINGS", "SPLIT2", split2.SplitterDistance);
    }

    //*******************************************************************************************************
    protected void ChangeButton(ref Button btn, string text, string popup)
    {
      ChangeButton(ref btn, text, popup, true);
    }

    //*******************************************************************************************************
    protected void ChangeButton(ref Button btn, string text, string popup, bool visible)
    {
      btn.Text = text;
      btn.AccessibleDescription = popup;
      this.toolTips.SetToolTip(btn, popup);
      btn.Visible = visible;
    }

    //*******************************************************************************************************
    protected bool CheckSoundPaths()
    {
        DialogResult dr;
        
        while(true)
        {
            if (!File.Exists(soundPath + "\\move.wav"))
            {
              dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + soundPath + "\\move.wav does not exist", "Invalid Sound Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(soundPath + "\\check.wav"))
            {
              dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + soundPath + "\\check.wav does not exist", "Invalid Sound Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(soundPath + "\\invalid.wav"))
            {
              dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + soundPath + "\\invalid.wav does not exist", "Invalid Sound Resource", MessageBoxButtons.RetryCancel);
            }
            else
            {
              return true;
            }

            if(dr == DialogResult.Cancel)
            {
                return false;
            }
            
            resourceFolderBrowserDialog.SelectedPath = resourcesPath;
            if (resourceFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                soundPath = resourceFolderBrowserDialog.SelectedPath;
                chess.PutINIValue("SETTINGS", "SOUND_PATH", soundPath);
            }
        }
    }

    #endregion

    #region Piece Stuff

    //*******************************************************************************************************
    protected bool CheckPiecePaths()
    {
        DialogResult dr;

        while (true)
        {
            if (!File.Exists(piecePath + "\\wRook.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\wRook.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\wKnight.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\wKnight.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\wBishop.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\wBishop.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\wQueen.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\wQueen.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\wPawn.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\wPawn.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\wKing.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\wKing.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\kRook.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\kRook.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\kKnight.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\kKnight.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\kBishop.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\kBishop.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\kQueen.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\kQueen.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\kPawn.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\kPawn.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\kKing.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\kKing.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else if (!File.Exists(piecePath + "\\blank.bmp"))
            {
                dr = MessageBox.Show("A valid folder must be selected with the all the proper files in it. " + piecePath + "\\blank.bmp does not exist", "Invalid Piece Resource", MessageBoxButtons.RetryCancel);
            }
            else
            {
                return true;
            }

            if (dr == DialogResult.Cancel)
            {
                return false;
            }

            resourceFolderBrowserDialog.SelectedPath = resourcesPath;
            if (resourceFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                piecePath = resourceFolderBrowserDialog.SelectedPath;
                chess.PutINIValue("SETTINGS", "PIECE_PATH", piecePath);
            }
        }
    }

    //*******************************************************************************************************
    protected void LoadPieces(string pieceDir)
    {
      //use pixel 1 as the transparent pixel
      int width = white1.Size.Width;
      int height = white1.Size.Height;

      wRookOriginal = new Bitmap(pieceDir + "\\wRook.bmp");
      wRookOriginal.MakeTransparent(wRookOriginal.GetPixel(0, 0));
      wRooksm = ResizeImage(wRookOriginal, width, height);
      wKnightOriginal = new Bitmap(pieceDir + "\\wKnight.bmp");
      wKnightOriginal.MakeTransparent(wKnightOriginal.GetPixel(0, 0));
      wKnightsm = ResizeImage(wKnightOriginal, width, height);
      wBishopOriginal = new Bitmap(pieceDir + "\\wBishop.bmp");
      wBishopOriginal.MakeTransparent(wBishopOriginal.GetPixel(0, 0));
      wBishopsm = ResizeImage(wBishopOriginal, width, height);
      wQueenOriginal = new Bitmap(pieceDir + "\\wQueen.bmp");
      wQueenOriginal.MakeTransparent(wQueenOriginal.GetPixel(0, 0));
      wQueensm = ResizeImage(wQueenOriginal, width, height);
      wPawnOriginal = new Bitmap(pieceDir + "\\wPawn.bmp");
      wPawnOriginal.MakeTransparent(wPawnOriginal.GetPixel(0, 0));
      wPawnsm = ResizeImage(wPawnOriginal, width, height);
      wKingOriginal = new Bitmap(pieceDir + "\\wKing.bmp");
      wKingOriginal.MakeTransparent(wKingOriginal.GetPixel(0, 0));

      kRookOriginal = new Bitmap(pieceDir + "\\kRook.bmp");
      kRookOriginal.MakeTransparent(kRookOriginal.GetPixel(0, 0));
      kRooksm = ResizeImage(kRookOriginal, width, height);
      kKnightOriginal = new Bitmap(pieceDir + "\\kKnight.bmp");
      kKnightOriginal.MakeTransparent(kKnightOriginal.GetPixel(0, 0));
      kKnightsm = ResizeImage(kKnightOriginal, width, height);
      kBishopOriginal = new Bitmap(pieceDir + "\\kBishop.bmp");
      kBishopOriginal.MakeTransparent(kBishopOriginal.GetPixel(0, 0));
      kBishopsm = ResizeImage(kBishopOriginal, width, height);
      kQueenOriginal = new Bitmap(pieceDir + "\\kQueen.bmp");
      kQueenOriginal.MakeTransparent(kQueenOriginal.GetPixel(0, 0));
      kQueensm = ResizeImage(kQueenOriginal, width, height);
      kPawnOriginal = new Bitmap(pieceDir + "\\kPawn.bmp");
      kPawnOriginal.MakeTransparent(kPawnOriginal.GetPixel(0, 0));
      kPawnsm = ResizeImage(kPawnOriginal, width, height);
      kKingOriginal = new Bitmap(pieceDir + "\\kKing.bmp");
      kKingOriginal.MakeTransparent(kKingOriginal.GetPixel(0, 0));

      blankOriginal = new Bitmap(pieceDir + "\\blank.bmp");
      blankOriginal.MakeTransparent(blankOriginal.GetPixel(0, 0));
      blanksm = ResizeImage(blankOriginal, width, height);

      ResizetheBoard();
    }

    //*******************************************************************************************************
    protected void ResizePieces(int size)
    {
      //resize from the original images loaded
      wRook = ResizeImage(wRookOriginal, size, size);
      //wRook.Save("tmp.bmp");
      wKnight = ResizeImage(wKnightOriginal, size, size);
      wBishop = ResizeImage(wBishopOriginal, size, size);
      wQueen = ResizeImage(wQueenOriginal, size, size);
      wPawn = ResizeImage(wPawnOriginal, size, size);
      wKing = ResizeImage(wKingOriginal, size, size);

      kRook = ResizeImage(kRookOriginal, size, size);
      kKnight = ResizeImage(kKnightOriginal, size, size);
      kBishop = ResizeImage(kBishopOriginal, size, size);
      kQueen = ResizeImage(kQueenOriginal, size, size);
      kPawn = ResizeImage(kPawnOriginal, size, size);
      kKing = ResizeImage(kKingOriginal, size, size);

      blank = ResizeImage(blankOriginal, size, size);
    }

    //*******************************************************************************************************
    protected void RefreshAllPieces()
    {
      string srcSquare = "";

      for (int x = 0; x < 8; x++)
      {
        for (int y = 0; y < 8; y++)
        {
          srcSquare = SquareName(x, y);
          theBoard.Controls[srcSquare].BackgroundImage = SelectPiece(theBoard.Controls[srcSquare].AccessibleName);
        }
      }

      ClearSmallImages();

      white1.BackgroundImage = SelectPiece(white1.AccessibleName, true);
      white2.BackgroundImage = SelectPiece(white2.AccessibleName, true);
      white3.BackgroundImage = SelectPiece(white3.AccessibleName, true);
      white4.BackgroundImage = SelectPiece(white4.AccessibleName, true);
      white5.BackgroundImage = SelectPiece(white5.AccessibleName, true);
      white6.BackgroundImage = SelectPiece(white6.AccessibleName, true);
      white7.BackgroundImage = SelectPiece(white7.AccessibleName, true);
      white8.BackgroundImage = SelectPiece(white8.AccessibleName, true);
      white9.BackgroundImage = SelectPiece(white9.AccessibleName, true);
      white10.BackgroundImage = SelectPiece(white10.AccessibleName, true);
      white11.BackgroundImage = SelectPiece(white11.AccessibleName, true);
      white12.BackgroundImage = SelectPiece(white12.AccessibleName, true);
      white13.BackgroundImage = SelectPiece(white13.AccessibleName, true);
      white14.BackgroundImage = SelectPiece(white14.AccessibleName, true);
      white15.BackgroundImage = SelectPiece(white15.AccessibleName, true);
      black1.BackgroundImage = SelectPiece(black1.AccessibleName, true);
      black2.BackgroundImage = SelectPiece(black2.AccessibleName, true);
      black3.BackgroundImage = SelectPiece(black3.AccessibleName, true);
      black4.BackgroundImage = SelectPiece(black4.AccessibleName, true);
      black5.BackgroundImage = SelectPiece(black5.AccessibleName, true);
      black6.BackgroundImage = SelectPiece(black6.AccessibleName, true);
      black7.BackgroundImage = SelectPiece(black7.AccessibleName, true);
      black8.BackgroundImage = SelectPiece(black8.AccessibleName, true);
      black9.BackgroundImage = SelectPiece(black9.AccessibleName, true);
      black10.BackgroundImage = SelectPiece(black10.AccessibleName, true);
      black11.BackgroundImage = SelectPiece(black11.AccessibleName, true);
      black12.BackgroundImage = SelectPiece(black12.AccessibleName, true);
      black13.BackgroundImage = SelectPiece(black13.AccessibleName, true);
      black14.BackgroundImage = SelectPiece(black14.AccessibleName, true);
      black15.BackgroundImage = SelectPiece(black15.AccessibleName, true);
    }

    //*******************************************************************************************************
    protected void ClearSmallImages()
    {
      white1.BackgroundImage = blank;
      white2.BackgroundImage = blank;
      white3.BackgroundImage = blank;
      white4.BackgroundImage = blank;
      white5.BackgroundImage = blank;
      white6.BackgroundImage = blank;
      white7.BackgroundImage = blank;
      white8.BackgroundImage = blank;
      white9.BackgroundImage = blank;
      white10.BackgroundImage = blank;
      white11.BackgroundImage = blank;
      white12.BackgroundImage = blank;
      white13.BackgroundImage = blank;
      white14.BackgroundImage = blank;
      white15.BackgroundImage = blank;
      black1.BackgroundImage = blank;
      black2.BackgroundImage = blank;
      black3.BackgroundImage = blank;
      black4.BackgroundImage = blank;
      black5.BackgroundImage = blank;
      black6.BackgroundImage = blank;
      black7.BackgroundImage = blank;
      black8.BackgroundImage = blank;
      black9.BackgroundImage = blank;
      black10.BackgroundImage = blank;
      black11.BackgroundImage = blank;
      black12.BackgroundImage = blank;
      black13.BackgroundImage = blank;
      black14.BackgroundImage = blank;
      black15.BackgroundImage = blank;
    }

    //*******************************************************************************************************
    protected void AddToWhiteTotal(int i)
    {
      int total = Convert.ToInt16(whiteTotal.Text) + i;
      whiteTotal.Text = total.ToString();
      if (total > 0) whiteTotal.Visible = true;
      else whiteTotal.Visible = false;
    }

    //*******************************************************************************************************
    protected void AddToBlackTotal(int i)
    {
      int total = Convert.ToInt16(blackTotal.Text) + i;
      blackTotal.Text = total.ToString();
      if (total > 0) blackTotal.Visible = true;
      else blackTotal.Visible = false;
    }

    //*******************************************************************************************************
    protected void AddCasualty(string piece)
    {
      Image tmp = blanksm;

      if (piece.StartsWith("w"))
      {
        if (piece.EndsWith("Pawn"))
        {
          AddToWhiteTotal(1);
          tmp = wPawnsm;
        }
        else if (piece.EndsWith("Knight"))
        {
          AddToWhiteTotal(3);
          tmp = wKnightsm;
        }
        else if (piece.EndsWith("Bishop"))
        {
          AddToWhiteTotal(3);
          tmp = wBishopsm;
        }
        else if (piece.EndsWith("Rook"))
        {
          AddToWhiteTotal(5);
          tmp = wRooksm;
        }
        else if (piece.EndsWith("Queen"))
        {
          AddToWhiteTotal(9);
          tmp = wQueensm;
        }

        //this will clrea the text if zero
        AddToWhiteTotal(0);

        if (white1.AccessibleName.Length == 0)
        {
          white1.BackgroundImage = tmp;
          white1.AccessibleName = piece;
        }
        else if (white2.AccessibleName.Length == 0)
        {
          white2.BackgroundImage = tmp;
          white2.AccessibleName = piece;
        }
        else if (white3.AccessibleName.Length == 0)
        {
          white3.BackgroundImage = tmp;
          white3.AccessibleName = piece;
        }
        else if (white4.AccessibleName.Length == 0)
        {
          white4.BackgroundImage = tmp;
          white4.AccessibleName = piece;
        }
        else if (white5.AccessibleName.Length == 0)
        {
          white5.BackgroundImage = tmp;
          white5.AccessibleName = piece;
        }
        else if (white6.AccessibleName.Length == 0)
        {
          white6.BackgroundImage = tmp;
          white6.AccessibleName = piece;
        }
        else if (white7.AccessibleName.Length == 0)
        {
          white7.BackgroundImage = tmp;
          white7.AccessibleName = piece;
        }
        else if (white8.AccessibleName.Length == 0)
        {
          white8.BackgroundImage = tmp;
          white8.AccessibleName = piece;
        }
        else if (white9.AccessibleName.Length == 0)
        {
          white9.BackgroundImage = tmp;
          white9.AccessibleName = piece;
        }
        else if (white10.AccessibleName.Length == 0)
        {
          white10.BackgroundImage = tmp;
          white10.AccessibleName = piece;
        }
        else if (white11.AccessibleName.Length == 0)
        {
          white11.BackgroundImage = tmp;
          white11.AccessibleName = piece;
        }
        else if (white12.AccessibleName.Length == 0)
        {
          white12.BackgroundImage = tmp;
          white12.AccessibleName = piece;
        }
        else if (white13.AccessibleName.Length == 0)
        {
          white13.BackgroundImage = tmp;
          white13.AccessibleName = piece;
        }
        else if (white14.AccessibleName.Length == 0)
        {
          white14.BackgroundImage = tmp;
          white14.AccessibleName = piece;
        }
        else if (white15.AccessibleName.Length == 0)
        {
          white15.BackgroundImage = tmp;
          white15.AccessibleName = piece;
        }
        //else no where eles to put anything - this should not ever happen
      }
      else
      {
        if (piece.EndsWith("Pawn"))
        {
          AddToBlackTotal(1);
          tmp = kPawnsm;
        }
        else if (piece.EndsWith("Knight"))
        {
          AddToBlackTotal(3);
          tmp = kKnightsm;
        }
        else if (piece.EndsWith("Bishop"))
        {
          AddToBlackTotal(3);
          tmp = kBishopsm;
        }
        else if (piece.EndsWith("Rook"))
        {
          AddToBlackTotal(5);
          tmp = kRooksm;
        }
        else if (piece.EndsWith("Queen"))
        {
          AddToBlackTotal(9);
          tmp = kQueensm;
        }

        //this will clrea the text if zero
        AddToBlackTotal(0);

        if (black1.AccessibleName.Length == 0)
        {
          black1.BackgroundImage = tmp;
          black1.AccessibleName = piece;
        }
        else if (black2.AccessibleName.Length == 0)
        {
          black2.BackgroundImage = tmp;
          black2.AccessibleName = piece;
        }
        else if (black3.AccessibleName.Length == 0)
        {
          black3.BackgroundImage = tmp;
          black3.AccessibleName = piece;
        }
        else if (black4.AccessibleName.Length == 0)
        {
          black4.BackgroundImage = tmp;
          black4.AccessibleName = piece;
        }
        else if (black5.AccessibleName.Length == 0)
        {
          black5.BackgroundImage = tmp;
          black5.AccessibleName = piece;
        }
        else if (black6.AccessibleName.Length == 0)
        {
          black6.BackgroundImage = tmp;
          black6.AccessibleName = piece;
        }
        else if (black7.AccessibleName.Length == 0)
        {
          black7.BackgroundImage = tmp;
          black7.AccessibleName = piece;
        }
        else if (black8.AccessibleName.Length == 0)
        {
          black8.BackgroundImage = tmp;
          black8.AccessibleName = piece;
        }
        else if (black9.AccessibleName.Length == 0)
        {
          black9.BackgroundImage = tmp;
          black9.AccessibleName = piece;
        }
        else if (black10.AccessibleName.Length == 0)
        {
          black10.BackgroundImage = tmp;
          black10.AccessibleName = piece;
        }
        else if (black11.AccessibleName.Length == 0)
        {
          black11.BackgroundImage = tmp;
          black11.AccessibleName = piece;
        }
        else if (black12.AccessibleName.Length == 0)
        {
          black12.BackgroundImage = tmp;
          black12.AccessibleName = piece;
        }
        else if (black13.AccessibleName.Length == 0)
        {
          black13.BackgroundImage = tmp;
          black13.AccessibleName = piece;
        }
        else if (black14.AccessibleName.Length == 0)
        {
          black14.BackgroundImage = tmp;
          black14.AccessibleName = piece;
        }
        else if (black15.AccessibleName.Length == 0)
        {
          black15.BackgroundImage = tmp;
          black15.AccessibleName = piece;
        }
        //else no where eles to put anything - this should not ever happen
      }
    }

    //*******************************************************************************************************
    protected void ClearCasualties()
    {
      white1.AccessibleName = "";
      white2.AccessibleName = "";
      white3.AccessibleName = "";
      white4.AccessibleName = "";
      white5.AccessibleName = "";
      white6.AccessibleName = "";
      white7.AccessibleName = "";
      white8.AccessibleName = "";
      white9.AccessibleName = "";
      white10.AccessibleName = "";
      white11.AccessibleName = "";
      white12.AccessibleName = "";
      white13.AccessibleName = "";
      white14.AccessibleName = "";
      white15.AccessibleName = "";
      black1.AccessibleName = "";
      black2.AccessibleName = "";
      black3.AccessibleName = "";
      black4.AccessibleName = "";
      black5.AccessibleName = "";
      black6.AccessibleName = "";
      black7.AccessibleName = "";
      black8.AccessibleName = "";
      black9.AccessibleName = "";
      black10.AccessibleName = "";
      black11.AccessibleName = "";
      black12.AccessibleName = "";
      black13.AccessibleName = "";
      black14.AccessibleName = "";
      black15.AccessibleName = "";

      ClearSmallImages();

      whiteTotal.Text = "0";
      blackTotal.Text = "0";

      whiteTotal.Visible = false;
      blackTotal.Visible = false;
    }

    //*******************************************************************************************************
    protected void UpdateCasualties(bool white, int queens, int rooks, int bishops, int knights, int pawns)
    {
      if (white) whiteTotal.Text = "0";
      else blackTotal.Text = "0";

      for (int i = 0; i < queens; i++)
      {
        if (white) AddCasualty("wQueen");
        else AddCasualty("kQueen");
      }

      for (int i = 0; i < rooks; i++)
      {
        if (white) AddCasualty("wRook");
        else AddCasualty("kRook");
      }

      for (int i = 0; i < bishops; i++)
      {
        if (white) AddCasualty("wBishop");
        else AddCasualty("kBishop");
      }

      for (int i = 0; i < knights; i++)
      {
        if (white) AddCasualty("wKnight");
        else AddCasualty("kKnight");
      }

      for (int i = 0; i < pawns; i++)
      {
        if (white) AddCasualty("wPawn");
        else AddCasualty("kPawn");
      }
    }

    //*******************************************************************************************************
    protected void UpdateSquare(string square)
    {
      theBoard.Controls[square].BackgroundImage = SelectPiece(theBoard.Controls[square].AccessibleName);
      theBoard.Controls[square].Refresh();
    }

    //*******************************************************************************************************
    protected Image SelectPiece(string piece)
    {
      return SelectPiece(piece, false);
    }

    //*******************************************************************************************************
    protected Image SelectPiece(string piece, bool small)
    {
      if (piece.Equals("wRook"))
      {
        return (small ? wRooksm : wRook);
      }
      else if (piece.Equals("wBishop"))
      {
        return (small ? wBishopsm : wBishop);
      }
      else if (piece.Equals("wKnight"))
      {
        return (small ? wKnightsm : wKnight);
      }
      else if (piece.Equals("wKing"))
      {
        return (small ? wKing : wKing);
      }
      else if (piece.Equals("wQueen"))
      {
        return (small ? wQueensm : wQueen);
      }
      else if (piece.Equals("wPawn"))
      {
        return (small ? wPawnsm : wPawn);
      }
      else if (piece.Equals("kRook"))
      {
        return (small ? kRooksm : kRook);
      }
      else if (piece.Equals("kBishop"))
      {
        return (small ? kBishopsm : kBishop);
      }
      else if (piece.Equals("kKnight"))
      {
        return (small ? kKnightsm : kKnight);
      }
      else if (piece.Equals("kKing"))
      {
        return (small ? kKing : kKing);
      }
      else if (piece.Equals("kQueen"))
      {
        return (small ? kQueensm : kQueen);
      }
      else if (piece.Equals("kPawn"))
      {
        return (small ? kPawnsm : kPawn);
      }
      else
      {
        return blank;
      }
    }

    //*******************************************************************************************************
    protected void changePiecesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      resourceFolderBrowserDialog.SelectedPath = piecePath;

      if (resourceFolderBrowserDialog.ShowDialog() == DialogResult.OK)
      {
        piecePath = resourceFolderBrowserDialog.SelectedPath;
        if (CheckPiecePaths())
        {
            LoadPieces(piecePath);
            RefreshAllPieces();
            chess.PutINIValue("SETTINGS", "PIECE_PATH", piecePath);
        }
        else
        {
            //exit the program, it cannot run without having pieces to display
            this.Close();
        }
      }
    }

    //*******************************************************************************************************
    protected void Chess_DragEnter(object sender, DragEventArgs e)
    {
      if (!disableDD)
      {
        // make sure they're actually dropping files (not text or anything else)
        if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
        {
          // without this, the cursor stays a "NO" symbol
          e.Effect = DragDropEffects.All;
        }
      }
    }

    //*******************************************************************************************************
    protected string FindPiece(string piece)
    {
      for (int x = 0; x < 8; x++)
      {
        for (int y = 0; y < 8; y++)
        {
          if (theBoard.Controls[SquareName(x, y)].AccessibleName.Equals(piece))
          {
            return SquareName(x, y);
          }
        }
      }

      return ""; //bad thing
    }

    //*******************************************************************************************************
    protected bool OnBoard(int x, int y)
    {
      return ((x >= 0) && (y >= 0) && (x <= 7) && (y <= 7));
    }

    //*******************************************************************************************************
    protected bool MoveMatch(string lan)
    {
      ArrayList newMove;
      newMove = virtualChessBoard.LoadList(lan, fenNotation.Text, false);

      if (newMove.Count == 1)
      {
        if (moveTree.SelectedNode.NextNode == null)
        {
          return ((Move)newMove[0]).lan.Equals(((Move)moveTree.SelectedNode.Nodes[0].Tag).lan);
        }
        else
        {
          return ((Move)newMove[0]).lan.Equals(((Move)moveTree.SelectedNode.NextNode.Tag).lan);
        }
      }

      return false;
    }

    #endregion

    #region Piece Moving Checks

    //*******************************************************************************************************
    protected bool IsAPawnPromotion(string src, string dst)
    {
      if (theBoard.Controls[src].AccessibleName.Equals("wPawn") && src.Substring(1, 1).Equals("7") && dst.Substring(1, 1).Equals("8"))
      {
        if ((theBoard.Controls[dst].AccessibleName.Length == 0) && src.Substring(0, 1).Equals(dst.Substring(0, 1)))
        {
          return true;
        }
        else if (theBoard.Controls[dst].AccessibleName.StartsWith("k"))
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
      else if (theBoard.Controls[src].AccessibleName.Equals("kPawn") && src.Substring(1, 1).Equals("2") && dst.Substring(1, 1).Equals("1"))
      {
        if ((theBoard.Controls[dst].AccessibleName.Length == 0) && src.Substring(0, 1).Equals(dst.Substring(0, 1)))
        {
          return true;
        }
        else if (theBoard.Controls[dst].AccessibleName.StartsWith("w"))
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
    protected bool FindKingSource(string dst, ref string src, string piece)
    {
      Point dest = FindPosition(dst);
      int x, y;

      foreach (Point p in virtualChessBoard.allowedMovesKing)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (theBoard.Controls[SquareName(x, y)].AccessibleName.Equals(piece))
          {
            src = SquareName(x, y);
            return true;
          }
        }
      }

      src = "";
      return false;
    }

    ////*******************************************************************************************************
    //protected void InvalidMove(int MoveNum, bool whiteMove, string move)
    //{
    //  if (!checkQuietly)
    //  {
    //    if (whiteMove) MessageBox.Show("Invalid White Move #" + fullMove.ToString() + " " + move + "\n" + currentMoveList + "\n" + sanList + "\n" + lanList);
    //    else MessageBox.Show("Invalid Black Move #" + (fullMove - 1).ToString() + " " + move + "\n" + currentMoveList + "\n" + sanList + "\n" + lanList);
    //  }
    //}

    //*******************************************************************************************************
    protected bool DiagonalIsClear(Point src, Point dst, Point[] allowedMoves)
    {
      int incrementX = (dst.X - src.X) / Math.Abs(dst.X - src.X);
      int incrementY = (dst.Y - src.Y) / Math.Abs(dst.Y - src.Y);
      Point check = new Point(src.X + incrementX, src.Y + incrementY);

      while (check.X != dst.X)
      {
        if (theBoard.Controls[SquareName(check.X, check.Y)].AccessibleName.Length > 0)
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
          if (theBoard.Controls[SquareName(minX + p.X, src.Y)].AccessibleName.Length > 0)
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
          string sn = SquareName(src.X, minY + p.Y);
          string pn = Controls[sn].AccessibleName;
          if (pn.Length > 0)
          {
            return false;
          }
        }
      }

      return true;
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

        if (!reversedBoard && OnBoard(sq.X - 1, sq.Y + 1) && theBoard.Controls[SquareName(sq.X - 1, sq.Y + 1)].AccessibleName.Equals("wPawn"))
        {
          return true;
        }
        else if (!reversedBoard && OnBoard(sq.X + 1, sq.Y + 1) && theBoard.Controls[SquareName(sq.X + 1, sq.Y + 1)].AccessibleName.Equals("wPawn"))
        {
          return true;
        }
        else if (reversedBoard && OnBoard(sq.X - 1, sq.Y - 1) && theBoard.Controls[SquareName(sq.X - 1, sq.Y - 1)].AccessibleName.Equals("wPawn"))
        {
          return true;
        }
        else if (reversedBoard && OnBoard(sq.X + 1, sq.Y - 1) && theBoard.Controls[SquareName(sq.X + 1, sq.Y - 1)].AccessibleName.Equals("wPawn"))
        {
          return true;
        }
      }
      else //if(!byWhite)
      {
        //by a pawn?
        Point sq = FindPosition(square);

        if (!reversedBoard && OnBoard(sq.X - 1, sq.Y - 1) && theBoard.Controls[SquareName(sq.X - 1, sq.Y - 1)].AccessibleName.Equals("kPawn"))
        {
          return true;
        }
        else if (!reversedBoard && OnBoard(sq.X + 1, sq.Y - 1) && theBoard.Controls[SquareName(sq.X + 1, sq.Y - 1)].AccessibleName.Equals("kPawn"))
        {
          return true;
        }
        else if (reversedBoard && OnBoard(sq.X - 1, sq.Y + 1) && theBoard.Controls[SquareName(sq.X - 1, sq.Y + 1)].AccessibleName.Equals("kPawn"))
        {
          return true;
        }
        else if (reversedBoard && OnBoard(sq.X + 1, sq.Y + 1) && theBoard.Controls[SquareName(sq.X + 1, sq.Y + 1)].AccessibleName.Equals("kPawn"))
        {
          return true;
        }
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool FindKingSource(string piece, string dst, ref string src)
    {
      Point dest = FindPosition(dst);
      int x, y;

      foreach (Point p in virtualChessBoard.allowedMovesKing)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (theBoard.Controls[SquareName(x, y)].AccessibleName.Equals(piece))
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
    protected bool FindRookSources(string piece, string dst)
    {
      Point dest = FindPosition(dst);
      Point source;
      int x, y;

      foreach (Point p in virtualChessBoard.allowedMovesRook)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (theBoard.Controls[SquareName(x, y)].AccessibleName.Equals(piece))
          {
            source = new Point(x, y);
            if (RankIsClear(source, dest, virtualChessBoard.allowedMovesRook))
            {
              return true;
            }
            else if (FileIsClear(source, dest, virtualChessBoard.allowedMovesRook))
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool FindBishopSources(string piece, string dst)
    {
      Point dest = FindPosition(dst);
      Point source;
      int x, y;

      foreach (Point p in virtualChessBoard.allowedMovesBishop)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (theBoard.Controls[SquareName(x, y)].AccessibleName.Equals(piece))
          {
            source = new Point(x, y);
            if (DiagonalIsClear(source, dest, virtualChessBoard.allowedMovesBishop))
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    //*******************************************************************************************************
    protected bool FindKnightSources(string piece, string dst)
    {
      Point dest = FindPosition(dst);
      int x, y;

      foreach (Point p in virtualChessBoard.allowedMovesKnight)
      {
        x = p.X + dest.X;
        y = p.Y + dest.Y;
        if (OnBoard(x, y))
        {
          if (theBoard.Controls[SquareName(x, y)].AccessibleName.Equals(piece))
          {
            return true;
          }
        }
      }

      return false;
    }

    #endregion

    #region File Stuff

    //drag and drop of a pgn file?
    ////*******************************************************************************************************
    //protected void Chess_DragDrop(object sender, DragEventArgs e)
    //{
    //  //if (!disableDD)
    //  //{
    //  //  DisableEverything();

    //  //  play.Checked = true;
    //  //  ChangeButton(ref pauseGame, "New", "Start a new game.");
    //  //  ChangeButton(ref tradeSides, "Resume", "Resume an unfinnished game.");
    //  //  moveStatus.Text = "New / Resume or change mode.";

    //  //  // transfer the filenames to a string array
    //  //  string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

    //  //  //if multiple files are dropped only the first one will be processed...
    //  //  if (Path.GetFileName(files[0]).ToLower().EndsWith(".pgn"))
    //  //  {
    //  //    LoadNewPGNFile(Path.GetFullPath(files[0]), false);
    //  //  }
    //  //  else if (Path.GetFileName(files[0]).ToLower().EndsWith(".lan"))
    //  //  {
    //  //    LoadNewPGNFile(Path.GetFullPath(files[0]), true);
    //  //  }
    //  //  else
    //  //  {
    //  //    MessageBox.Show("Invalid file type, only pgn files supported...");
    //  //  }
    //  //}
    //  //else
    //  //{
    //  //  MessageBox.Show("Unable to process dropped pgn file in the current program state.");
    //  //}
    //}

    //*******************************************************************************************************
    private void SaveTheGame()
    {
      //save all the info that we know...
      if (play.Checked)
      {
        if (gameInfo.Text.Length > 0)
        {
          gameInfo.Text += "\r\n[Event \"Live Play\"]";
        }
        else
        {
          gameInfo.Text += "[Event \"Live Play\"]";
        }
        gameInfo.Text += "\r\n[Site \"Chess Rocks!\"]";
        gameInfo.Text += "\r\n[Date \"" + dateTime.Text.Substring(0, 10) + "\"]";
      }

      gameInfo.Text += "\r\n[PlyCount \"" + (mainPlycount).ToString() + "\"]";

      if (useClock.Checked)
      {
        gameInfo.Text += "\r\n[TimeControl \"" + clockStart.Text + "|" + bronsteinTime.Text + "\"]";
        gameInfo.Text += "\r\n[WhiteTime \"" + whiteClock.Text + "\"]";
        gameInfo.Text += "\r\n[BlackTime \"" + blackClock.Text + "\"]";
      }
      else
      {
        gameInfo.Text += "\r\n[TimeControl \"none\"]";
      }

      if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
      {
        if (computerWhite.Checked)
        {
          gameInfo.Text += ("\r\n[White \"" + engines.Controls["whiteEngine"].Text.Replace(" - Ready", "") + "\"]");
          gameInfo.Text += "\r\n[WhiteType \"computer\"]";
        }
        else
        {
          gameInfo.Text += "\r\n[WhiteType \"human\"]";
        }

        if (computerBlack.Checked)
        {
          gameInfo.Text += ("\r\n[Black \"" + engines.Controls["blackEngine"].Text.Replace(" - Ready", "") + "\"]");
          gameInfo.Text += "\r\n[BlackType \"computer\"]";
        }
        else
        {
          gameInfo.Text += "\r\n[BlackType \"human\"]";
        }
        gameInfo.Text += "\r\n[AnalysisTime \"" + computerTime.Text + "\"]";
      }
    }

    //*******************************************************************************************************
    private bool ReadPGNFile(string fileName)
    {
      StreamReader sr;

      try
      {
        sr = new StreamReader(fileName);
      }
      catch
      {
        MessageBox.Show("The file " + fileName + " could not be read...");
        return false;
      }

      string line;

      try
      {
        if (sr.EndOfStream)
        {
          MessageBox.Show("The file " + fileName + " is empty...");
          sr.Close();
          return false;
        }

        line = sr.ReadToEnd();
        sr.Close();
      }
      catch
      {
        MessageBox.Show("The file " + fileName + " could not be read...");
        sr.Close();
        return false;
      }

      if (ProcessReadPGN(line))
      {
        return true;
      }

      return false;
    }

    //*******************************************************************************************************
    private bool LoadGameFromList(int loadRow)
    {
      bool puzzlePlayersUnknown = false;

      //mainline
      ArrayList Moves = new ArrayList();

      computerWhite.Enabled = true;
      computerBlack.Enabled = true;
      puzzle = false;
      fullMove = 1;

      gameInfo.Text = gameList.gameListDataGridView.Rows[loadRow].Cells["header"].Value.ToString();

      if (gameInfo.Text.IndexOf("[PUZZLE \"") >= 0)
      {
        puzzle = true;
        computerWhite.Checked = false;
        computerBlack.Checked = false;
        computerWhite.Enabled = false;
        computerBlack.Enabled = false;
        useClock.Checked = false;
        useClock.Enabled = false;

        if ((gameInfo.Text.IndexOf("[WhiteType \"human\"]") >= 0) && (gameInfo.Text.IndexOf("[BlackType \"computer\"]") >= 0))
        {
          puzzleComputerWhite = false;
        }
        else if ((gameInfo.Text.IndexOf("[WhiteType \"computer\"]") >= 0) && (gameInfo.Text.IndexOf("[BlackType \"human\"]") >= 0))
        {
          puzzleComputerWhite = true;
        }
        else
        {
          //have to figure out who is who - last move decides
          puzzleComputerWhite = false;
          puzzlePlayersUnknown = true;
        }
      }

      gameResult.Text = gameList.gameListDataGridView.Rows[loadRow].Cells["result"].Value.ToString();
      fenNotation.Text = gameList.gameListDataGridView.Rows[loadRow].Cells["fen"].Value.ToString();

      string lastFEN = fenNotation.Text;

      ResetPieces(fenNotation.Text);

      //in case black made the first move in this file
      if (fenNotation.Text.IndexOf(" b ") > 0) whitesMove = false;

      string moves = gameList.gameListDataGridView.Rows[loadRow].Cells["moveList"].Value.ToString();

      Moves = virtualChessBoard.LoadList(moves, fenNotation.Text, true);

      moveTree.Nodes.Clear();
      TreeNode main;

      if (puzzle)
      {
        main = new TreeNode("Puzzle");
      }
      else
      {
        main = new TreeNode("Main");
      }

      moveTree.Nodes.Add(main);
      moveTree.Nodes[0].Tag = new Move(gameList.gameListDataGridView.Rows[loadRow].Cells["fen"].Value.ToString(), 1);

      if (Moves.Count > 0)
      {
        mainPlycount = 0;

        //save moves from virtual movelist to displayed movelist
        moveTree.BeginUpdate();
        for (int row = 0; row < Moves.Count; row++)
        {
          AddMoveNode((Move)(Moves[row]), main);
        }
        moveTree.Nodes[0].Expand();
        moveTree.EndUpdate();

        if (puzzle)
        {
          if (puzzlePlayersUnknown)
          {
            //need to fix this 
            //the last move should have been made by the human?
            if (((Move)(moveTree.Nodes[0].LastNode.Tag)).whitesMove)
            {
              gameInfo.Text += "\r\n[WhiteType \"human\"]";
              gameInfo.Text += "\r\n[BlackType \"computer\"]";
              puzzleComputerWhite = true;
            }
            else
            {
              gameInfo.Text += "\r\n[WhiteType \"computer\"]";
              gameInfo.Text += "\r\n[BlackType \"human\"]";
              puzzleComputerWhite = false;
            }
          }

          if (whitesMove && !puzzleComputerWhite)
          {
            moveTree.SelectedNode = moveTree.Nodes[0];
            moveStatus.Text = "White to move...";
          }
          else if (!whitesMove && puzzleComputerWhite)
          {
            moveTree.SelectedNode = moveTree.Nodes[0];
            moveStatus.Text = "Black to move...";
          }
          else
          {
            Thread.Sleep(1000);
            moveTree.Nodes[0].FirstNode.ForeColor = Color.Black;
            moveTree.SelectedNode = moveTree.Nodes[0].FirstNode;
            if (!((Move)(moveTree.Nodes[0].FirstNode.Tag)).whitesMove)
            {
              moveStatus.Text = "White to move...";
            }
            else
            {
              moveStatus.Text = "Black to move...";
            }
          }
        }
        else
        {
          mainPlycount = Moves.Count;
          moveTree.SelectedNode = moveTree.Nodes[0];
          moveStatus.Text = "Success Loading game #" + (loadRow + 1).ToString();
        }
        moveTree.Refresh();
        moveTree.Focus();
        UpdateTitle();

        return true;
      }

      return false;
    }

    //*******************************************************************************************************
    protected void AddMoveNode(Move mv, TreeNode main)
    {
      mv.ProcessRAVMoveLists(showSAN, whiteBK, blackBK);

      if (showSAN)
      {
        if (mv.whitesMove)
        {
          if (mv.ravExists)
          {
            main.Nodes.Add(new TreeNode(GetLabel(mv), mv.main));
          }
          else
          {
            main.Nodes.Add(new TreeNode(GetLabel(mv)));
          }
          main.Nodes[main.Nodes.Count - 1].BackColor = whiteBK;
          if (puzzle) main.Nodes[main.Nodes.Count - 1].ForeColor = whiteBK;
          main.Nodes[main.Nodes.Count - 1].Tag = mv;
        }
        else
        {
          if (mv.ravExists)
          {
            main.Nodes.Add(new TreeNode(GetLabel(mv), mv.main));
          }
          else
          {
            main.Nodes.Add(new TreeNode(GetLabel(mv)));
          }
          main.Nodes[main.Nodes.Count - 1].BackColor = blackBK;
          if (puzzle) main.Nodes[main.Nodes.Count - 1].ForeColor = blackBK;
          main.Nodes[main.Nodes.Count - 1].Tag = mv;
        }
      }
      else
      {
        if (mv.whitesMove)
        {
          if (mv.ravExists)
          {
            main.Nodes.Add(new TreeNode(padding[mv.fullMove.ToString().Length + 3] + mv.fullMove.ToString() + ". " + mv.lan + padding[mv.lan.Length + 1], mv.main));
          }
          else
          {
            main.Nodes.Add(new TreeNode(padding[mv.fullMove.ToString().Length + 3] + mv.fullMove.ToString() + ". " + mv.lan + padding[mv.lan.Length + 1]));
          }
          main.Nodes[main.Nodes.Count - 1].BackColor = whiteBK;
          if (puzzle) main.Nodes[main.Nodes.Count - 1].ForeColor = whiteBK;
          main.Nodes[main.Nodes.Count - 1].Tag = mv;
        }
        else
        {
          if (mv.ravExists)
          {
            main.Nodes.Add(new TreeNode("     " + mv.lan + padding[mv.lan.Length + 1], mv.main));
          }
          else
          {
            main.Nodes.Add(new TreeNode("     " + mv.lan + padding[mv.lan.Length + 1]));
          }
          main.Nodes[main.Nodes.Count - 1].BackColor = blackBK;
          if (puzzle) main.Nodes[main.Nodes.Count - 1].ForeColor = blackBK;
          main.Nodes[main.Nodes.Count - 1].Tag = mv;
        }
      }
    }

    //*******************************************************************************************************
    protected void OnGameOver()
    {
      //This routine is called after whitesMove has been updated for the next move!!!
      if (!whitesMove)
      {
        if (gameOver)
        {
          if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
          {
            if (computerWhite.Checked) WriteSIOfEngine("stop", 1);
            if (computerBlack.Checked) WriteSIOfEngine("stop", 2);
          }

          if (inCheck)
          {
            gameResult.Text = "1-0";
            if (gameInfo.Text.Length > 0)
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "\r\n[Result \"1-0\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"White won by checkmate\"]";
            }
            else
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "[Result \"1-0\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"White won by checkmate\"]";
            }
            if (play.Checked) EnableMainSelections("Checkmate, white wins.", "Game Over", true);
          }
          else if (staleMate)
          {
            gameResult.Text = "1/2-1/2";
            if (gameInfo.Text.Length > 0)
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "[Result \"1/2-1/2\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Draw by stalemate\"]";
            }
            else
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Draw by stalemate\"]";
            }
            if (play.Checked) EnableMainSelections("Draw by stalemate.", "Game Over", true);
          }
          else if (insufficientMaterial)
          {
            gameResult.Text = "1/2-1/2";
            if (gameInfo.Text.Length > 0)
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "[Result \"1/2-1/2\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Draw for insufficient material\"]";
            }
            else
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Draw for insufficient material\"]";
            }
            if (play.Checked) EnableMainSelections("Insufficient material to win.", "Game Over", true);
          }
          else
          {
            if (play.Checked) EnableMainSelections("Bug : Game Over but I do not know why, possible error in move list file!", "Game Over", true);
          }
        }
      }
      else
      {
        if (gameOver)
        {
          if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
          {
            if (computerWhite.Checked) WriteSIOfEngine("stop", 1);
            if (computerBlack.Checked) WriteSIOfEngine("stop", 2);
          }

          if (inCheck)
          {
            gameResult.Text = "0-1";
            if (gameInfo.Text.Length > 0)
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "[Result \"0-1\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Black won by checkmate\"]";
            }
            else
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "\r\n[Result \"0-1\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Black won by checkmate\"]";
            }
            if (play.Checked) EnableMainSelections("Checkmate, black wins.", "Game Over", true);
          }
          else if (staleMate)
          {
            gameResult.Text = "1/2-1/2";
            if (gameInfo.Text.Length > 0)
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "[Result \"1/2-1/2\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Draw by stalemate\"]";
            }
            else
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Draw by stalemate\"]";
            }
            if (play.Checked) EnableMainSelections("Draw by stalemate.", "Game Over", true);
          }
          else if (insufficientMaterial)
          {
            gameResult.Text = "1/2-1/2";
            if (gameInfo.Text.Length > 0)
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "[Result \"1/2-1/2\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Draw for insufficient material\"]";
            }
            else
            {
              if (gameInfo.Text.IndexOf("[Result ") < 0) gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
              if (gameInfo.Text.IndexOf("[Termination ") < 0) gameInfo.Text += "\r\n[Termination \"Draw for insufficient material\"]";
            }
            if (play.Checked) EnableMainSelections("Insufficient material to win.", "Game Over", true);
          }
          else
          {
            if (play.Checked) EnableMainSelections("Bug : Game Over but I do not know why, possible error in move list file!", "Game Over", true);
          }
        }
      }
    }

    //*******************************************************************************************************
    private bool LoadPGNFile()
    {
      oPGN.Filter = "PGN files (*.pgn)|*.pgn";
      //MessageBox.Show(pgnPath);
      oPGN.InitialDirectory = pgnPath;
      oPGN.Multiselect = false;
      oPGN.CheckFileExists = true;

      if (oPGN.ShowDialog() == DialogResult.OK)
      {
        pgnPath = Path.GetFullPath(oPGN.FileName).Replace(Path.GetFileName(oPGN.FileName), "");
        chess.PutINIValue("SETTINGS", "PGN_PATH", pgnPath);

        moveStatus.Text = "Loading PGN File...";
        moveStatus.Refresh();
        return ReadPGNFile(oPGN.FileName);
      }

      return false;
    }

    //*******************************************************************************************************
    private bool SavePGNFile()
    {
      PGNHeader dialog = new ns_PGNHeader.PGNHeader(gameInfo.Text);

      if (dialog.ShowDialog() == DialogResult.OK)
      {
        gameInfo.Text = dialog.tags;
      }
      else
      {
        return false;
      }

      //save the current movelist to a file
      //sPGN.RestoreDirectory = true;
      sPGN.Title = "Save Move List to file...";
      sPGN.AddExtension = true;
      //MessageBox.Show(pgnPath);
      sPGN.InitialDirectory = pgnPath;
      sPGN.Filter = "PGN files (*.pgn)|*.pgn";
      sPGN.FileName = (((loadedFileName.Length > 0) && !loadedFileName.Equals("newgame.pgn")) ? Path.GetFileNameWithoutExtension(loadedFileName) : (dialog.whitePlayer + "_vs_" + dialog.blackPlayer + "_" + dialog.timeStamp));

      if (sPGN.ShowDialog() == DialogResult.OK)
      {
        pgnPath = Path.GetFullPath(sPGN.FileName).Replace(Path.GetFileName(sPGN.FileName), "");
        chess.PutINIValue("SETTINGS", "PGN_PATH", pgnPath);
          
          
        try
        {
          //using (StreamWriter sw = new StreamWriter(sPGN.FileName))
          StreamWriter sw = new StreamWriter(sPGN.FileName);

          try
          {
            string ClipBoardData = GetGame();

            if (ClipBoardData.Length > 0)
            {
              oPGN.FileName = sPGN.FileName;
              UpdateTitle();
              //sw.WriteLine(ClipBoardData.Trim());
              sw.Write(ClipBoardData.Trim());
              sw.Close();
            }
            else
            {
              MessageBox.Show("The pgn movelist was not saved...");
              return false;
            }
          }
          finally
          {
            if (sw != null)
              ((IDisposable)sw).Dispose();
          }
        }
        catch (Exception ex)
        {
          // Let the user know what went wrong.
          MessageBox.Show("The pgn movelist was not saved... " + ex.Message);
          return false;
        }
      }
      return true;
    }

    //*******************************************************************************************************
    private void PreLoadingFileChanges()
    {
      ResetBoard();
      puzzle = false;
      pgnLoaded = false;
      savePGN.Visible = false;
      UpdateTitle();
    }

    //*******************************************************************************************************
    private void loadPGN_Click(object sender, EventArgs e)
    {
      oPGN.FileName = "newgame.pgn";
      PreLoadingFileChanges();

      LoadPGNFile();
    }

    //*******************************************************************************************************
    private void ProcessLoadedFile()
    {
      UpdateTitle();
      centiPawn.Invalidate();
      centiPawn.Refresh();
      savePGN.Visible = true;

      if (positionSetup.Checked)
      {
        //load current board position - pgn Header Only
      }
      else if (moveAnalysis.Checked || gameAnalysis.Checked)
      {
        //the Load button is the only setting currently
        savePGN.Visible = pgnLoaded;

        if (gameAnalysis.Checked)
        {
          //go to the second move - do not bother analyzing the first one...
          try
          {
            if (moveTree.SelectedNode == null)
            {
              if ((moveTree.Nodes.Count > 0) &&
                  (moveTree.Nodes[0].Nodes.Count > 1))
              {
                moveTree.SelectedNode = moveTree.Nodes[0].Nodes[1];
              }
            }
            else if (moveTree.SelectedNode.Text.StartsWith("Main") && (moveTree.SelectedNode.Nodes.Count > 1))
            {
              //start with the second move
              moveTree.SelectedNode = moveTree.SelectedNode.Nodes[1];
            }
          }
          catch
          {
          }
          pauseGame.Visible = true;

          UpdateAnalysisTime();
        }
      }
      else if (loadPGN.Text.Equals("Load"))
      {
        //load adjourned game
        if (!puzzle)
        {
          moveTree.SelectedNode = moveTree.Nodes[moveTree.Nodes.Count - 1];
          moveTree.Refresh();

          lastManualRow = moveTree.Nodes.Count - 1;
          if (moveTree.Nodes[0].LastNode != null)
          {
            moveTree.SelectedNode = moveTree.Nodes[0].LastNode;
          }
        }
        else
        {
          lastManualRow = moveTree.Nodes.Count - 1;
          blackDraw.Visible = false;
          whiteDraw.Visible = false;
          return;
        }

        //interpret stuff out of the Tag list for the setup
        if (gameResult.Text.Equals("*"))
        {
          int j, i = gameInfo.Text.IndexOf("[TimeControl \"");

          if (i > 0)
          {
            i += 14;
            if (!gameInfo.Text.Substring(i).StartsWith("none"))
            {
              //there should be 2 numbers separated by a pipe
              j = gameInfo.Text.IndexOf("|", i);

              if (j > i)
              {
                clockStart.Text = gameInfo.Text.Substring(i, j - i);
                i = j + 1;
                j = gameInfo.Text.IndexOf("\"", i);
                bronsteinTime.Text = gameInfo.Text.Substring(i, j - i);

                useClock.Visible = true;
                useClock.Checked = true;
                ResetClocks();
                whiteClock.Visible = true;
                blackClock.Visible = true;
                clockStart.Visible = true;
                bronsteinTime.Visible = true;

                //thses should exist - if not the default times will be used to start
                i = gameInfo.Text.IndexOf("[WhiteTime \"");
                if (i > 0)
                {
                  i += 12;
                  whiteClock.Text = gameInfo.Text.Substring(i, 8);

                  DateTime dt = new DateTime(2012, 12, 21, 0, 0, 0, 0, DateTimeKind.Unspecified);
                  dt = dt.AddHours(Convert.ToDouble(whiteClock.Text.Substring(0, 2)));
                  dt = dt.AddMinutes(Convert.ToDouble(whiteClock.Text.Substring(3, 2)));
                  dt = dt.AddSeconds(Convert.ToDouble(whiteClock.Text.Substring(6, 2)));
                  dtpWhite.Value = dt;
                  initialWhiteClock = dt;
                }

                i = gameInfo.Text.IndexOf("[BlackTime \"");
                if (i > 0)
                {
                  i += 12;
                  blackClock.Text = gameInfo.Text.Substring(i, 8);

                  DateTime dt = new DateTime(2012, 12, 21, 0, 0, 0, 0, DateTimeKind.Unspecified);
                  dt = dt.AddHours(Convert.ToDouble(blackClock.Text.Substring(0, 2)));
                  dt = dt.AddMinutes(Convert.ToDouble(blackClock.Text.Substring(3, 2)));
                  dt = dt.AddSeconds(Convert.ToDouble(blackClock.Text.Substring(6, 2)));
                  dtpBlack.Value = dt;
                  initialBlackClock = dt;
                }
              }
            }
          }

          computerWhite.Checked = (gameInfo.Text.IndexOf("[WhiteType \"computer\"]") >= 0);
          if (computerWhite.Checked)
          {
            //white is a computer player
            string tmp = GetHeaderValue("AnalysisTime");

            if (tmp.Length > 0)
            {
              if ((i = tmp.IndexOf(".")) > 0)
              {
                tmp = tmp.Substring(0, i);
              }

              try
              {
                if (Convert.ToInt16(tmp) > 0)
                {
                  computerTime.Text = tmp;
                }
              }
              catch { }
            }
          }

          computerBlack.Checked = (gameInfo.Text.IndexOf("[BlackType \"computer\"]") >= 0);
          if (computerBlack.Checked)
          {
            //black is a computer player
            if (!computerWhite.Checked)
            {
              //white is not
              string tmp = GetHeaderValue("AnalysisTime");

              if (tmp.Length > 0)
              {
                if ((i = tmp.IndexOf(".")) > 0)
                {
                  tmp = tmp.Substring(0, i);
                }

                try
                {
                  if (Convert.ToInt16(tmp) > 0)
                  {
                    computerTime.Text = tmp;
                  }
                }
                catch { }
              }
            }
          }

          resumingGame = true;

          DisableMainSelections();

          computerWhite.Enabled = false;
          computerBlack.Enabled = false;
          useClock.Enabled = false;

          if (whitesMove)
          {
            ChangeButton(ref whiteDraw, "Abort", "Return to main mode selection.", true);
            ChangeButton(ref blackDraw, "Start", "Resume the game. Start clock if enabled.", true);
          }
          else
          {
            ChangeButton(ref blackDraw, "Abort", "Return to main mode selection.", true);
            ChangeButton(ref whiteDraw, "Start", "Resume the game. Start clock if enabled.", true);
          }
          ChangeButton(ref pauseGame, "Pause", "Pause the game clocks if enabled.", false);

          LastColorClockChangeWasWhite = whitesMove;
        }
        else
        {
          MessageBox.Show("This is not an incomplete game, changing to Move Analysis mode.");
          moveAnalysis.Checked = true;
          play.Enabled = true;
          positionSetup.Enabled = true;
          moveAnalysis.Enabled = true;
          gameAnalysis.Enabled = true;
        }
      }
    }

    //*******************************************************************************************************
    private void savePGN_Click(object sender, EventArgs e)
    {
      if (play.Checked && gameResult.Text.Equals("*"))
      {
        runClock = false;
        runCPUClock = false;
        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
        {
          if (computerWhite.Checked) WriteSIOfEngine("stop", 1);
          if (computerBlack.Checked) WriteSIOfEngine("stop", 2);
        }
      }

      if (SaveIt())
      {
        if (SavePGNFile())
        {
          savePGN.BackColor = Color.LightBlue;
          loadPGN.Visible = true;
        }
        else
        {
          savePGN.BackColor = Color.Red;
          loadPGN.Visible = false;
        }
        //changesMade.Visible = !SavePGNFile();
        //loadPGN.Visible = !changesMade.Visible;

        if (play.Checked && gameResult.Text.Equals("*"))
        {
          //game was ended early and saved
          EnableMainSelections("", "", false);
        }
      }
    }

    //*******************************************************************************************************
    private bool SaveIt()
    {
      if (play.Checked && gameResult.Text.Equals("*"))
      {
        //save current game status - including clocks if being used.
        SaveTheGame();
      }
      else if (positionSetup.Checked)
      {
        //save current board position - pgn Header Only
        if (gameInfo.Text.Length > 0)
        {
          if (makeAPuzzle.Checked)
          {
            gameInfo.Text += "\r\n[Event \"Puzzle\"]";
          }
          else
          {
            gameInfo.Text += "\r\n[Event \"Puzzle\"]";
          }
        }
        else
        {
          if (makeAPuzzle.Checked)
          {
            gameInfo.Text = "\r\n[Event \"Position Setup\"]";
          }
          else
          {
            gameInfo.Text = "\r\n[Event \"Position Setup\"]";
          }
        }

        gameInfo.Text += "\r\n[Site \"Chess Rocks!\"]";
        gameInfo.Text += "\r\n[Date \"" + dateTime.Text.Substring(0, 10) + "\"]";
        gameInfo.Text += "\r\n[Round \"-\"]";
        gameInfo.Text += "\r\n[White \"white\"]";
        gameInfo.Text += "\r\n[Black \"black\"]";
        gameInfo.Text += "\r\n[Result \"*\"]";
        gameInfo.Text += "\r\n[SetUp \"1\"]";

        if (makeAPuzzle.Checked)
        {
          gameInfo.Text += "\r\n[PUZZLE \"Chess Rocks!!!\"]";
          //the last move should have been made by the human?
          if (((Move)(moveTree.Nodes[0].LastNode.Tag)).whitesMove)
          {
            gameInfo.Text += "\r\n[WhiteType \"human\"]";
            gameInfo.Text += "\r\n[BlackType \"computer\"]";
          }
          else
          {
            gameInfo.Text += "\r\n[WhiteType \"computer\"]";
            gameInfo.Text += "\r\n[BlackType \"human\"]";
          }
          //gameInfo.Text += "\r\n[FENPlus \"" + puzzleFENPlus + "\"]";
          gameInfo.Text += "\r\n[FEN \"" + puzzleFEN + "\"]";
        }
        else
        {
          //gameInfo.Text += "\r\n[FENPlus \"" + fenNotationPlus.Text + "\"]";
          gameInfo.Text += "\r\n[FEN \"" + fenNotation.Text + "\"]";
          gameResult.Text = "*";
        }
      }

      return true;
    }

    //*******************************************************************************************************
    private void loadFromFile_Click(object sender, EventArgs e)
    {
      loadPGN_Click(sender, e);
    }

    //*******************************************************************************************************
    private void loadFromList_Click(object sender, EventArgs e)
    {
      PreLoadingFileChanges();

      //this should be modal...
      if (gameList.gameListDataGridView.Rows.Count > 0)
      {
        disableDD = true;
        if (gameList.ShowDialog() != DialogResult.OK)
        {
          disableDD = false;
          return;
        }

        moveStatus.Text = "Loading PGN From List...";
        moveStatus.Refresh();

        disableDD = false;

        int loadRow = gameList.gameListDataGridView.CurrentCell.RowIndex;

        pgnLoaded = LoadGameFromList(loadRow);
        if (!pgnLoaded)
        {
          MessageBox.Show("Game " + (loadRow + 1).ToString() + " failed to load...");
          return;
        }
      }
      else
      {
        pgnLoaded = false;
        MessageBox.Show("Game List is empty...");
        return;
      }

      ProcessLoadedFile();
    }

    //*******************************************************************************************************
    private void loadFromClipboard_Click(object sender, EventArgs e)
    {
      //Paste
      oPGN.FileName = "clipboard.pgn";
      PreLoadingFileChanges();

      // Create a new instance of the DataObject interface.
      IDataObject data = Clipboard.GetDataObject();

      if (data.GetDataPresent(DataFormats.Text))
      {
        pgnLoaded = ProcessReadPGN(data.GetData(DataFormats.Text).ToString());

        if (pgnLoaded)
        {
          moveStatus.Text = "Loading PGN From Clipboard...";
          moveStatus.Refresh();
          ProcessLoadedFile();
        }
      }

      if (!pgnLoaded)
      {
        MessageBox.Show("Failed Loading PGN From Clipboard...");
      }
    }

    //*******************************************************************************************************
    private bool ProcessReadPGN(string text)
    {
      text = text.Replace("\r\n\r\n", "\n \n");
      text = text.Replace("\n\n", "\n \n");
      text = text.Replace("\r\n", "\n");
      text = text.Replace("\r", "\n");

      char[] delimiter = "\n".ToCharArray();
      string[] list = null;

      list = text.Split(delimiter, StringSplitOptions.None);

      ArrayList Moves = new ArrayList();

      string tmp;
      string[] tags = new string[255];
      int loadRow = gameList.gameListDataGridView.Rows.Count;
      int newGamesLoaded = 0;
      int moveCount = 0;

      gameInfo.Text = "";
      fullMove = 1;

      bool commentMode = false;
      bool alternativeMode = false;
      bool bracketMode = false;
      bool blankLineFound = false;
      bool moveMode = false;
      bool loadFEN = false;
      bool loadWhite = false;
      bool loadBlack = false;
      bool result = false;

      string fullMoveList = "", fullHeader = ""; ;
      string whitePlayer = "Unknown";
      string blackPlayer = "Unknown";
      string puzzleResult = "*";
      string line;

      foreach (string lineN in list)
      {
        try
        {
          //cleanup the line we just read in case there are 2 delimeters in a row
          line = lineN.Trim();

          if (line.Length > 0)
          {
            foreach (string tag in line.Split(spaceDelim, 255))
            {
              tmp = tag.Trim();
              if (tmp.Length == 0)
              {
                continue;
              }
              else if (moveMode)
              {
                if ((tmp.Equals(".") || //chess.com puzzle delimeter
                     tmp.Equals("0-1") ||
                     tmp.Equals("1-0") ||
                     tmp.Equals("1/2-1/2") ||
                     tmp.Equals("*")) && moveMode)
                {
                  gameList.gameListDataGridView.Rows.Add();

                  if (tmp.Equals("."))
                  {
                    //chess.com puzzle
                    gameList.gameListDataGridView.Rows[loadRow].Cells["result"].Value = puzzleResult;

                    //now we will assume that the winner is the human - probably wrong for some
                    //chess.com puzzles since their format is lacking - interpreting FULL may help?
                    if (puzzleResult.Equals("1-0"))
                    {
                      fullHeader += "\r\n[WhiteType \"human\"]";
                      fullHeader += "\r\n[BlackType \"computer\"]";
                    }
                    else if (puzzleResult.Equals("0-1"))
                    {
                      fullHeader += "\r\n[WhiteType \"computer\"]";
                      fullHeader += "\r\n[BlackType \"human\"]";
                    }
                    fullHeader += "\r\n[PUZZLE \"CHESS.COM\"]";
                  }
                  else
                  {
                    gameList.gameListDataGridView.Rows[loadRow].Cells["result"].Value = tmp;
                  }

                  gameList.gameListDataGridView.Rows[loadRow].Cells["gameNum"].Value = (loadRow + 1).ToString();
                  gameList.gameListDataGridView.Rows[loadRow].Cells["moveList"].Value = fullMoveList;
                  gameList.gameListDataGridView.Rows[loadRow].Cells["header"].Value = fullHeader;
                  gameList.gameListDataGridView.Rows[loadRow].Cells["white"].Value = whitePlayer;
                  gameList.gameListDataGridView.Rows[loadRow].Cells["black"].Value = blackPlayer;
                  gameList.gameListDataGridView.Rows[loadRow].Cells["fen"].Value = fenNotation.Text;
                  newGamesLoaded++;

                  //now see if there is another game - reset variables
                  fenNotation.Text = virtualChessBoard.fenStart;
                  fullMoveList = "";
                  fullHeader = "";
                  whitePlayer = "Unknown";
                  blackPlayer = "Unknown";

                  commentMode = false;
                  alternativeMode = false;
                  bracketMode = false;
                  blankLineFound = false;
                  moveMode = false;
                  loadFEN = false;
                  loadWhite = false;
                  loadBlack = false;
                  fullMove = 1;
                  moveCount = 0;

                  loadRow++;
                }
                else
                {
                  fullMoveList += (" " + tmp);
                }
              }
              else if (bracketMode && !moveMode)
              {
                if (result)
                {
                  puzzleResult = tmp.Substring(1, tmp.Length - 3);
                  result = false;
                }
                else if (loadWhite)
                {
                  if (tmp.EndsWith("]"))
                  {
                    if (tmp.StartsWith("\""))
                    {
                      whitePlayer += (" " + tmp.Substring(1, tmp.Length - 3));
                    }
                    else
                    {
                      whitePlayer += (" " + tmp.Substring(0, tmp.Length - 2));
                    }
                    loadWhite = false;
                  }
                  else
                  {
                    if (tmp.StartsWith("\""))
                    {
                      whitePlayer += tmp.Substring(1);
                    }
                    else
                    {
                      whitePlayer += (" " + tmp);
                    }
                  }
                }

                if (loadBlack)
                {
                  if (tmp.EndsWith("]"))
                  {
                    if (tmp.StartsWith("\""))
                    {
                      blackPlayer += (" " + tmp.Substring(1, tmp.Length - 3));
                    }
                    else
                    {
                      blackPlayer += (" " + tmp.Substring(0, tmp.Length - 2));
                    }
                    loadBlack = false;
                  }
                  else
                  {
                    if (tmp.StartsWith("\""))
                    {
                      blackPlayer += tmp.Substring(1);
                    }
                    else
                    {
                      blackPlayer += (" " + tmp);
                    }
                  }
                }

                if (loadFEN)
                {
                  if (tmp.EndsWith("]"))
                  {
                    fenNotation.Text += (" " + tmp.Substring(0, tmp.Length - 2));
                    loadFEN = false;
                  }
                  else
                  {
                    if (tmp.StartsWith("\""))
                    {
                      fenNotation.Text += tmp.Substring(1);
                    }
                    else
                    {
                      fenNotation.Text += (" " + tmp);
                    }
                  }
                }

                if (tmp.EndsWith("]"))
                {
                  bracketMode = false;
                }

                fullHeader += (" " + tmp);
              }
              else if (alternativeMode)
              {
                if (tmp.EndsWith(")"))
                {
                  alternativeMode = false;
                }

                ((Move)(Moves[Moves.Count - 1])).AddRav(tmp);
              }
              else if (commentMode)
              {
                //comments may not be nested
                if (moveMode)
                {
                  if (tmp.EndsWith("}"))
                  {
                    commentMode = false;
                  }

                  ((Move)(Moves[Moves.Count - 1])).AddComment(tmp);
                }
                else
                {
                  if (tmp.EndsWith("}"))
                  {
                    commentMode = false;
                  }

                  fullHeader += (" " + tmp);
                }
              }
              else if (tmp.StartsWith("{"))
              {
                //move comment
                if (moveMode)
                {
                  ((Move)(Moves[Moves.Count - 1])).AddComment(tmp);
                }
                else
                {
                  //unknown info - turn it into a comment
                  if (fullHeader.Length == 0)
                  {
                    fullHeader = tmp;
                  }
                  else
                  {
                    fullHeader += ("\r\n" + tmp);
                  }
                }

                if (!tmp.EndsWith("}"))
                {
                  commentMode = true;
                }
              }
              else if (tmp.StartsWith("(") && moveMode)
              {
                //optional move(s) - may include comments and nags
                ((Move)(Moves[Moves.Count - 1])).AddRav(tmp);

                if (!tmp.EndsWith(")"))
                {
                  alternativeMode = true;
                }
              }
              else if (tmp.StartsWith("[") && !moveMode)
              {
                //normal info tag - these should only happen before game moves begin
                bracketMode = true;
                if (fullHeader.Length == 0)
                {
                  fullHeader = tmp;
                }
                else
                {
                  fullHeader += ("\r\n" + tmp);
                }

                //if (tmp.StartsWith("[FENPlus"))
                //{
                //  loadFENPlus = true;
                //  fenNotationPlus.Text = "";
                //}
                //else 
                if (tmp.StartsWith("[FEN"))
                {
                  loadFEN = true;
                  fenNotation.Text = "";
                }
                else if (tmp.Equals("[White"))
                {
                  loadWhite = true;
                  whitePlayer = "";
                }
                else if (tmp.Equals("[Black"))
                {
                  loadBlack = true;
                  blackPlayer = "";
                }
                else if (tmp.Equals("[Result"))
                {
                  //save this just in case
                  result = true;
                }
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
                if (blankLineFound)
                {
                  moveCount++;
                  moveMode = true;
                  fullMoveList += (" " + tmp);
                }
              }
            }
          }
          else
          {
            //in this case only the fist pgn game in the file will be read...
            blankLineFound = true;
            if (positionSetup.Checked)
            {
              //we got all we wanted
              gameResult.Text = "*";
              //              ResetPieces(fenNotation.Text, fenNotationPlus.Text);
              ResetPieces(fenNotation.Text);
              return true;
            }
          }
        }
        catch
        {
          MessageBox.Show("Exception processing clipboard text data encountered...");
          return false;
        }
      }//foreach

      loadRow = gameList.gameListDataGridView.Rows.Count - 1;

      //now load one of the games
      if ((loadRow < 0) || (newGamesLoaded == 0))
      {
        MessageBox.Show("No games were loaded or are in the game list...");
        return false;
      }
      else if ((gameList.gameListDataGridView.Rows.Count == 1) || (newGamesLoaded == 1))
      {
        //titleFileName = Path.GetFileName(fileName);
      }
      else
      {
        disableDD = true;
        //this should be modal...
        if (gameList.ShowDialog() == DialogResult.OK)
        {
          //do not use the actual file name to help avoid the mistake of saving a single game back over it...
          disableDD = false;
          loadRow = gameList.gameListDataGridView.CurrentCell.RowIndex;
        }
        else
        {
          disableDD = false;
          return false;
        }
      }

      //for testing purposes - loading many games from one file - they are all checked for legal moves
      if (gameList.loadAll && gameList.gameListDataGridView.Rows.Count > 1)
      {
        masterTimer.Stop();
        for (int row = 0; row < gameList.gameListDataGridView.Rows.Count; row++)
        {
          Moves = virtualChessBoard.LoadList(gameList.gameListDataGridView.Rows[row].Cells["moveList"].Value.ToString(),
                                          gameList.gameListDataGridView.Rows[row].Cells["fen"].Value.ToString(),
            //gameList.gameListDataGridView.Rows[row].Cells["fenPlus"].Value.ToString(),
                                          true);
          if (Moves.Count == 0)
          {
            MessageBox.Show("Game " + (row + 1).ToString() + " failed to load...");
            return false;
          }
        }
        MessageBox.Show(gameList.gameListDataGridView.Rows.Count.ToString() + " Games Loaded Successfully...");
        masterTimer.Start();
      }

      //normal code
      if (!LoadGameFromList(loadRow))
      {
        MessageBox.Show("Game " + (loadRow + 1).ToString() + " failed to load...");
        return false;
      }

      pgnLoaded = true;
      ProcessLoadedFile();
      return true;
    }

    //*******************************************************************************************************
    private void saveToFile_Click(object sender, EventArgs e)
    {
      savePGN_Click(sender, e);
    }

    //*******************************************************************************************************
    private void appendToFile_Click(object sender, EventArgs e)
    {
      try
      {
        if (SaveIt())
        {
          string ClipBoardData = GetGame();

          if (ClipBoardData.Length > 0)
          {
            PGNHeader dialog = new ns_PGNHeader.PGNHeader(gameInfo.Text);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
              gameInfo.Text = dialog.tags;
            }
            else
            {
              return;
            }

            //save the current movelist to a file
            //sPGN.RestoreDirectory = true;
            sPGN.Title = "Save Move List to file...";
            sPGN.AddExtension = true;
            //MessageBox.Show(pgnPath);
            sPGN.InitialDirectory = pgnPath;
            sPGN.Filter = "PGN files (*.pgn)|*.pgn";
            sPGN.FileName = ((loadedFileName.Length > 0) ? Path.GetFileNameWithoutExtension(loadedFileName) : (dialog.whitePlayer + "_vs_" + dialog.blackPlayer + "_" + dialog.timeStamp));
            sPGN.OverwritePrompt = false;

            if (sPGN.ShowDialog() == DialogResult.OK)
            {
              pgnPath = Path.GetFullPath(sPGN.FileName).Replace(Path.GetFileName(sPGN.FileName), "");
              chess.PutINIValue("SETTINGS", "PGN_PATH", pgnPath);
                
              File.AppendAllText(Path.GetFileName(sPGN.FileName), "\r\n\r\n" + ClipBoardData);

              oPGN.FileName = sPGN.FileName;
              UpdateTitle();
              //              changesMade.Visible = false;
              savePGN.BackColor = Color.LightBlue;
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("appendToFile() failed... " + ex.ToString());
      }
    }

    //*******************************************************************************************************
    private void saveToList_Click(object sender, EventArgs e)
    {

    }

    //*******************************************************************************************************
    private void appendToList_Click(object sender, EventArgs e)
    {

    }

    //*******************************************************************************************************
    private void saveListToFile_Click(object sender, EventArgs e)
    {

    }

    //*******************************************************************************************************
    private void saveToClipboard_Click(object sender, EventArgs e)
    {
      try
      {
        if (SaveIt())
        {

          string ClipBoardData = GetGame();

          if (ClipBoardData.Length > 0)
          {
            Clipboard.Clear();
            Clipboard.SetDataObject(ClipBoardData);
            //            changesMade.Visible = false;
            savePGN.BackColor = Color.LightBlue;
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("saveToClipboard() failed... " + ex.ToString());
      }
    }

    //*******************************************************************************************************
    private string GetGame()
    {
      string ClipBoardData = "";

      try
      {
        bool failed = false;
        char[] delimiter = "\n".ToCharArray();

        //output tags
        foreach (string s in gameInfo.Text.Split(delimiter))
        {
          if (s.Trim().Length > 0)
          {
            ClipBoardData += (s.Trim() + "\r\n");
          }
        }

        //output an empty line
        ClipBoardData += ("\r\n");
        string MoveNagComment = "";
        bool newLine = true;

        Move move;

        //output movelist and the result
        foreach (TreeNode tn in moveTree.Nodes[0].Nodes)
        {
          move = (Move)tn.Tag;

          try
          {
            MoveNagComment = ((newLine || move.whitesMove) ? ((newLine ? "" : " ") + move.fullMove.ToString() + (move.whitesMove ? ". " : "... ")) : " ") +
              move.san +
              ((move.nag > 0) ? (" $" + move.nag.ToString()) : "") +
              ((move.comment.Length > 1) ? (" " + move.comment.Trim()) : "");

            if (tn.Nodes.Count > 0)
            {
              ClipBoardData += (MoveNagComment + "\r\n");
              newLine = true;
              if (!SaveRAV(1, tn, ref ClipBoardData))
              {
                failed = true;
                break;
              }
            }
            else if (move.whitesMove)
            {
              ClipBoardData += (MoveNagComment);
              newLine = false;
            }
            else
            {
              ClipBoardData += (MoveNagComment + "\r\n");
              newLine = true;
            }
          }
          catch
          {
            failed = true;
            break;
          }
        }

        if (!newLine)
        {
          ClipBoardData += ("\r\n");
        }

        ClipBoardData += (gameResult.Text + "\r\n");

        if (failed)
        {
          MessageBox.Show("The pgn movelist was not saved correctly...");
          return "";
        }

      }
      catch (Exception ex)
      {
        MessageBox.Show("GetGame() : " + ex.ToString());
        return "";
      }

      return ClipBoardData;
    }

    //*******************************************************************************************************
    private bool SaveRAV(int gameLevel, TreeNode mn, ref string ClipBoardData)
    {
      Move move;
      bool newLine = true;
      string MoveNagComment = "";
      bool forceMoveNum = true;
      bool firstMove = true;

      foreach (TreeNode rav in mn.Nodes)
      {
        ClipBoardData += (pgnPad.Substring(0, gameLevel) + "(");
        foreach (TreeNode tn in rav.Nodes)
        {
          move = (Move)tn.Tag;

          try
          {
            MoveNagComment = ((newLine || move.whitesMove || forceMoveNum) ? ((firstMove ? " " : pgnPad.Substring(0, gameLevel)) + move.fullMove.ToString() + (move.whitesMove ? ". " : "... ")) : " ") +
              move.san +
              ((move.nag > 0) ? (" $" + move.nag.ToString()) : "") +
              ((move.comment.Length > 1) ? (" " + move.comment.Trim()) : "");

            forceMoveNum = false;
            newLine = false;
            firstMove = false;

            if (tn.Nodes.Count > 0)
            {
              ClipBoardData += (MoveNagComment + "\r\n");
              newLine = true;

              if (!SaveRAV(gameLevel + 1, tn, ref ClipBoardData))
              {
                return false;
              }
              forceMoveNum = true;
            }
            else
            {
              ClipBoardData += (MoveNagComment);
            }
          }
          catch
          {
            return false;
          }
        }
        ClipBoardData += (" )\r\n");
        newLine = true;
      }
      return true;
    }

    #endregion

    #region Board Stuff

    //*******************************************************************************************************
    private void theBoard_Resize(object sender, EventArgs e)
    {
      ResizetheBoard();
    }

    //*******************************************************************************************************
    private void ResizetheBoard()
    {
      if (masterTimer.Enabled)
      {
        int width = theBoard.Width / 8;
        int height = theBoard.Height / 8;
        int yPosition;
        string square;
        Size sz = new Size(width, height);
        for (int rank = 0; rank < 8; rank++)
        {
          yPosition = rank * height;

          for (int file = 0; file < 8; file++)
          {
            square = SquareName(file, rank);
            theBoard.Controls[square].Location = new Point(width * file, yPosition);
            theBoard.Controls[square].Size = sz;
          }
        }

        ResizePieces(Math.Min(width, height) - 10);
        RefreshAllPieces();
      }
    }

    //*******************************************************************************************************
    private void split2_Panel1_ClientSizeChanged(object sender, EventArgs e)
    {
      theBoard.Size = new Size(8 * (split2.Panel1.Size.Width / 8), 8 * ((split2.Panel1.Size.Height - 40) / 8));
    }

    //*******************************************************************************************************
    protected void InitBoardLayoutArrays()
    {
      boardLayout = new string[8, 8];
      reversedBoardLayout = new string[8, 8];

      boardLayout[0, 0] = reversedBoardLayout[7, 7] = "a8";
      boardLayout[0, 1] = reversedBoardLayout[7, 6] = "a7";
      boardLayout[0, 2] = reversedBoardLayout[7, 5] = "a6";
      boardLayout[0, 3] = reversedBoardLayout[7, 4] = "a5";
      boardLayout[0, 4] = reversedBoardLayout[7, 3] = "a4";
      boardLayout[0, 5] = reversedBoardLayout[7, 2] = "a3";
      boardLayout[0, 6] = reversedBoardLayout[7, 1] = "a2";
      boardLayout[0, 7] = reversedBoardLayout[7, 0] = "a1";

      boardLayout[1, 0] = reversedBoardLayout[6, 7] = "b8";
      boardLayout[1, 1] = reversedBoardLayout[6, 6] = "b7";
      boardLayout[1, 2] = reversedBoardLayout[6, 5] = "b6";
      boardLayout[1, 3] = reversedBoardLayout[6, 4] = "b5";
      boardLayout[1, 4] = reversedBoardLayout[6, 3] = "b4";
      boardLayout[1, 5] = reversedBoardLayout[6, 2] = "b3";
      boardLayout[1, 6] = reversedBoardLayout[6, 1] = "b2";
      boardLayout[1, 7] = reversedBoardLayout[6, 0] = "b1";

      boardLayout[2, 0] = reversedBoardLayout[5, 7] = "c8";
      boardLayout[2, 1] = reversedBoardLayout[5, 6] = "c7";
      boardLayout[2, 2] = reversedBoardLayout[5, 5] = "c6";
      boardLayout[2, 3] = reversedBoardLayout[5, 4] = "c5";
      boardLayout[2, 4] = reversedBoardLayout[5, 3] = "c4";
      boardLayout[2, 5] = reversedBoardLayout[5, 2] = "c3";
      boardLayout[2, 6] = reversedBoardLayout[5, 1] = "c2";
      boardLayout[2, 7] = reversedBoardLayout[5, 0] = "c1";

      boardLayout[3, 0] = reversedBoardLayout[4, 7] = "d8";
      boardLayout[3, 1] = reversedBoardLayout[4, 6] = "d7";
      boardLayout[3, 2] = reversedBoardLayout[4, 5] = "d6";
      boardLayout[3, 3] = reversedBoardLayout[4, 4] = "d5";
      boardLayout[3, 4] = reversedBoardLayout[4, 3] = "d4";
      boardLayout[3, 5] = reversedBoardLayout[4, 2] = "d3";
      boardLayout[3, 6] = reversedBoardLayout[4, 1] = "d2";
      boardLayout[3, 7] = reversedBoardLayout[4, 0] = "d1";

      boardLayout[4, 0] = reversedBoardLayout[3, 7] = "e8";
      boardLayout[4, 1] = reversedBoardLayout[3, 6] = "e7";
      boardLayout[4, 2] = reversedBoardLayout[3, 5] = "e6";
      boardLayout[4, 3] = reversedBoardLayout[3, 4] = "e5";
      boardLayout[4, 4] = reversedBoardLayout[3, 3] = "e4";
      boardLayout[4, 5] = reversedBoardLayout[3, 2] = "e3";
      boardLayout[4, 6] = reversedBoardLayout[3, 1] = "e2";
      boardLayout[4, 7] = reversedBoardLayout[3, 0] = "e1";

      boardLayout[5, 0] = reversedBoardLayout[2, 7] = "f8";
      boardLayout[5, 1] = reversedBoardLayout[2, 6] = "f7";
      boardLayout[5, 2] = reversedBoardLayout[2, 5] = "f6";
      boardLayout[5, 3] = reversedBoardLayout[2, 4] = "f5";
      boardLayout[5, 4] = reversedBoardLayout[2, 3] = "f4";
      boardLayout[5, 5] = reversedBoardLayout[2, 2] = "f3";
      boardLayout[5, 6] = reversedBoardLayout[2, 1] = "f2";
      boardLayout[5, 7] = reversedBoardLayout[2, 0] = "f1";

      boardLayout[6, 0] = reversedBoardLayout[1, 7] = "g8";
      boardLayout[6, 1] = reversedBoardLayout[1, 6] = "g7";
      boardLayout[6, 2] = reversedBoardLayout[1, 5] = "g6";
      boardLayout[6, 3] = reversedBoardLayout[1, 4] = "g5";
      boardLayout[6, 4] = reversedBoardLayout[1, 3] = "g4";
      boardLayout[6, 5] = reversedBoardLayout[1, 2] = "g3";
      boardLayout[6, 6] = reversedBoardLayout[1, 1] = "g2";
      boardLayout[6, 7] = reversedBoardLayout[1, 0] = "g1";

      boardLayout[7, 0] = reversedBoardLayout[0, 7] = "h8";
      boardLayout[7, 1] = reversedBoardLayout[0, 6] = "h7";
      boardLayout[7, 2] = reversedBoardLayout[0, 5] = "h6";
      boardLayout[7, 3] = reversedBoardLayout[0, 4] = "h5";
      boardLayout[7, 4] = reversedBoardLayout[0, 3] = "h4";
      boardLayout[7, 5] = reversedBoardLayout[0, 2] = "h3";
      boardLayout[7, 6] = reversedBoardLayout[0, 1] = "h2";
      boardLayout[7, 7] = reversedBoardLayout[0, 0] = "h1";
    }

    //*******************************************************************************************************
    protected Point FindPosition(string square)
    {
      Point p;

      if (reversedBoard)
      {
        for (int x = 0; x < 8; x++)
        {
          for (int y = 0; y < 8; y++)
          {
            if (reversedBoardLayout[x, y].Equals(square))
            {
              p = new Point(x, y);
              return p;
            }
          }
        }
      }
      else
      {
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
      }

      //this should only happen if the input is bad
      return new Point(-1, -1);
    }

    //*******************************************************************************************************
    protected string SquareName(int x, int y)
    {
      if (reversedBoard)
      {
        return reversedBoardLayout[x, y];
      }

      return boardLayout[x, y];
    }

    //*******************************************************************************************************
    protected virtual void ToggleLabels()
    {
      showSquareNames = !showSquareNames;

      label_a1.Visible = showSquareNames;
      label_a2.Visible = showSquareNames;
      label_a3.Visible = showSquareNames;
      label_a4.Visible = showSquareNames;
      label_a5.Visible = showSquareNames;
      label_a6.Visible = showSquareNames;
      label_a7.Visible = showSquareNames;
      label_a8.Visible = showSquareNames;

      label_b1.Visible = showSquareNames;
      label_b2.Visible = showSquareNames;
      label_b3.Visible = showSquareNames;
      label_b4.Visible = showSquareNames;
      label_b5.Visible = showSquareNames;
      label_b6.Visible = showSquareNames;
      label_b7.Visible = showSquareNames;
      label_b8.Visible = showSquareNames;

      label_c1.Visible = showSquareNames;
      label_c2.Visible = showSquareNames;
      label_c3.Visible = showSquareNames;
      label_c4.Visible = showSquareNames;
      label_c5.Visible = showSquareNames;
      label_c6.Visible = showSquareNames;
      label_c7.Visible = showSquareNames;
      label_c8.Visible = showSquareNames;

      label_d1.Visible = showSquareNames;
      label_d2.Visible = showSquareNames;
      label_d3.Visible = showSquareNames;
      label_d4.Visible = showSquareNames;
      label_d5.Visible = showSquareNames;
      label_d6.Visible = showSquareNames;
      label_d7.Visible = showSquareNames;
      label_d8.Visible = showSquareNames;

      label_e1.Visible = showSquareNames;
      label_e2.Visible = showSquareNames;
      label_e3.Visible = showSquareNames;
      label_e4.Visible = showSquareNames;
      label_e5.Visible = showSquareNames;
      label_e6.Visible = showSquareNames;
      label_e7.Visible = showSquareNames;
      label_e8.Visible = showSquareNames;

      label_f1.Visible = showSquareNames;
      label_f2.Visible = showSquareNames;
      label_f3.Visible = showSquareNames;
      label_f4.Visible = showSquareNames;
      label_f5.Visible = showSquareNames;
      label_f6.Visible = showSquareNames;
      label_f7.Visible = showSquareNames;
      label_f8.Visible = showSquareNames;

      label_g1.Visible = showSquareNames;
      label_g2.Visible = showSquareNames;
      label_g3.Visible = showSquareNames;
      label_g4.Visible = showSquareNames;
      label_g5.Visible = showSquareNames;
      label_g6.Visible = showSquareNames;
      label_g7.Visible = showSquareNames;
      label_g8.Visible = showSquareNames;

      label_h1.Visible = showSquareNames;
      label_h2.Visible = showSquareNames;
      label_h3.Visible = showSquareNames;
      label_h4.Visible = showSquareNames;
      label_h5.Visible = showSquareNames;
      label_h6.Visible = showSquareNames;
      label_h7.Visible = showSquareNames;
      label_h8.Visible = showSquareNames;
    }

    //*******************************************************************************************************
    protected void ReverseBoard()
    {
      Point p = a1.Location;
      a1.Location = h8.Location;
      h8.Location = p;

      p = a2.Location;
      a2.Location = h7.Location;
      h7.Location = p;

      p = a3.Location;
      a3.Location = h6.Location;
      h6.Location = p;

      p = a4.Location;
      a4.Location = h5.Location;
      h5.Location = p;

      p = a5.Location;
      a5.Location = h4.Location;
      h4.Location = p;

      p = a6.Location;
      a6.Location = h3.Location;
      h3.Location = p;

      p = a7.Location;
      a7.Location = h2.Location;
      h2.Location = p;

      p = a8.Location;
      a8.Location = h1.Location;
      h1.Location = p;


      p = b1.Location;
      b1.Location = g8.Location;
      g8.Location = p;

      p = b2.Location;
      b2.Location = g7.Location;
      g7.Location = p;

      p = b3.Location;
      b3.Location = g6.Location;
      g6.Location = p;

      p = b4.Location;
      b4.Location = g5.Location;
      g5.Location = p;

      p = b5.Location;
      b5.Location = g4.Location;
      g4.Location = p;

      p = b6.Location;
      b6.Location = g3.Location;
      g3.Location = p;

      p = b7.Location;
      b7.Location = g2.Location;
      g2.Location = p;

      p = b8.Location;
      b8.Location = g1.Location;
      g1.Location = p;


      p = c1.Location;
      c1.Location = f8.Location;
      f8.Location = p;

      p = c2.Location;
      c2.Location = f7.Location;
      f7.Location = p;

      p = c3.Location;
      c3.Location = f6.Location;
      f6.Location = p;

      p = c4.Location;
      c4.Location = f5.Location;
      f5.Location = p;

      p = c5.Location;
      c5.Location = f4.Location;
      f4.Location = p;

      p = c6.Location;
      c6.Location = f3.Location;
      f3.Location = p;

      p = c7.Location;
      c7.Location = f2.Location;
      f2.Location = p;

      p = c8.Location;
      c8.Location = f1.Location;
      f1.Location = p;


      p = d1.Location;
      d1.Location = e8.Location;
      e8.Location = p;

      p = d2.Location;
      d2.Location = e7.Location;
      e7.Location = p;

      p = d3.Location;
      d3.Location = e6.Location;
      e6.Location = p;

      p = d4.Location;
      d4.Location = e5.Location;
      e5.Location = p;

      p = d5.Location;
      d5.Location = e4.Location;
      e4.Location = p;

      p = d6.Location;
      d6.Location = e3.Location;
      e3.Location = p;

      p = d7.Location;
      d7.Location = e2.Location;
      e2.Location = p;

      p = d8.Location;
      d8.Location = e1.Location;
      e1.Location = p;

      RefreshAllPositions();
    }

    //*******************************************************************************************************
    protected void RefreshAllPositions()
    {
      a1.Refresh();
      a2.Refresh();
      a3.Refresh();
      a4.Refresh();
      a5.Refresh();
      a6.Refresh();
      a7.Refresh();
      a8.Refresh();
      b1.Refresh();
      b2.Refresh();
      b3.Refresh();
      b4.Refresh();
      b5.Refresh();
      b6.Refresh();
      b7.Refresh();
      b8.Refresh();
      c1.Refresh();
      c2.Refresh();
      c3.Refresh();
      c4.Refresh();
      c5.Refresh();
      c6.Refresh();
      c7.Refresh();
      c8.Refresh();
      d1.Refresh();
      d2.Refresh();
      d3.Refresh();
      d4.Refresh();
      d5.Refresh();
      d6.Refresh();
      d7.Refresh();
      d8.Refresh();
      e1.Refresh();
      e2.Refresh();
      e3.Refresh();
      e4.Refresh();
      e5.Refresh();
      e6.Refresh();
      e7.Refresh();
      e8.Refresh();
      f1.Refresh();
      f2.Refresh();
      f3.Refresh();
      f4.Refresh();
      f5.Refresh();
      f6.Refresh();
      f7.Refresh();
      f8.Refresh();
      g1.Refresh();
      g2.Refresh();
      g3.Refresh();
      g4.Refresh();
      g5.Refresh();
      g6.Refresh();
      g7.Refresh();
      g8.Refresh();
      h1.Refresh();
      h2.Refresh();
      h3.Refresh();
      h4.Refresh();
      h5.Refresh();
      h6.Refresh();
      h7.Refresh();
      h8.Refresh();
    }

    //*******************************************************************************************************
    protected void ResetBoard()
    {
      fenNotation.Text = virtualChessBoard.fenStart;
      ResetPieces(fenNotation.Text);
      lastFEN = fenNotation.Text;
      mainPlycount = 0;

      comment.Text = "";
      gameResult.Text = "*";
      gameInfo.Text = "";
      nag.SelectedIndex = 0;

      moveTree.Nodes[0].Nodes.Clear();
      moveTree.SelectedNode = moveTree.Nodes[0];
      moveTree.Focus();
      centiPawn.Invalidate();
      centiPawn.Refresh();

      if (useClock.Checked)
      {
        ResetClocks();
      }

      savePGN.BackColor = Color.LightBlue;
    }

    //*******************************************************************************************************
    protected void toggleLabelsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToggleLabels();
      chess.PutINIValue("SETTINGS", "SHOW_SQUARE_NAMES", showSquareNames);
    }

    //*******************************************************************************************************
    protected void rotateToolStripMenuItem_Click(object sender, EventArgs e)
    {
      reversedBoard = !reversedBoard;
      chess.PutINIValue("SETTINGS", "SHOW_REVERSED_BOARD", reversedBoard);

      ReverseBoard();
    }

    //*******************************************************************************************************
    protected void whitesColorToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ColorDialog MyDialog = new ColorDialog();
      MyDialog.Color = whiteBK;

      if (MyDialog.ShowDialog() == DialogResult.OK)
      {
        whiteBK = MyDialog.Color;

        chess.PutINIValue("SETTINGS", "WHITE_RED", whiteBK.R);
        chess.PutINIValue("SETTINGS", "WHITE_GREEN", whiteBK.G);
        chess.PutINIValue("SETTINGS", "WHITE_BLUE", whiteBK.B);

        UpdateSquareColor();

        if (moveTree.Nodes[0].Nodes.Count > 0)
        {
          TreeNodeCollection nodes = moveTree.Nodes;
          moveTree.BeginUpdate();
          foreach (TreeNode n in nodes)
          {
            ChangeColor(n);
          }
          moveTree.EndUpdate();
        }
      }
    }

    //*******************************************************************************************************
    protected void blacksColorToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ColorDialog MyDialog = new ColorDialog();
      MyDialog.Color = blackBK;

      if (MyDialog.ShowDialog() == DialogResult.OK)
      {
        blackBK = MyDialog.Color;

        chess.PutINIValue("SETTINGS", "BLACK_RED", blackBK.R);
        chess.PutINIValue("SETTINGS", "BLACK_GREEN", blackBK.G);
        chess.PutINIValue("SETTINGS", "BLACK_BLUE", blackBK.B);

        UpdateSquareColor();

        if (moveTree.Nodes[0].Nodes.Count > 0)
        {
          TreeNodeCollection nodes = moveTree.Nodes;
          moveTree.BeginUpdate();
          foreach (TreeNode n in nodes)
          {
            ChangeColor(n);
          }
          moveTree.EndUpdate();
        }
      }
    }

    //*******************************************************************************************************
    protected void UpdateSquareColor()
    {
      engines.TabPages["whiteEngine"].BackColor = whiteBK;
      engines.TabPages["whiteOptions"].BackColor = whiteBK;

      engines.TabPages["blackEngine"].BackColor = blackBK;
      engines.TabPages["blackOptions"].BackColor = blackBK;

      engineCommunication.BackColor = whiteBK;
      engineCommunication2.BackColor = blackBK;

      whiteMaterialLost.BackColor = whiteBK;
      blackMaterialLost.BackColor = blackBK;

      a1.BackColor = blackBK;
      a2.BackColor = whiteBK;
      a3.BackColor = blackBK;
      a4.BackColor = whiteBK;
      a5.BackColor = blackBK;
      a6.BackColor = whiteBK;
      a7.BackColor = blackBK;
      a8.BackColor = whiteBK;

      b1.BackColor = whiteBK;
      b2.BackColor = blackBK;
      b3.BackColor = whiteBK;
      b4.BackColor = blackBK;
      b5.BackColor = whiteBK;
      b6.BackColor = blackBK;
      b7.BackColor = whiteBK;
      b8.BackColor = blackBK;

      c1.BackColor = blackBK;
      c2.BackColor = whiteBK;
      c3.BackColor = blackBK;
      c4.BackColor = whiteBK;
      c5.BackColor = blackBK;
      c6.BackColor = whiteBK;
      c7.BackColor = blackBK;
      c8.BackColor = whiteBK;

      d1.BackColor = whiteBK;
      d2.BackColor = blackBK;
      d3.BackColor = whiteBK;
      d4.BackColor = blackBK;
      d5.BackColor = whiteBK;
      d6.BackColor = blackBK;
      d7.BackColor = whiteBK;
      d8.BackColor = blackBK;

      e1.BackColor = blackBK;
      e2.BackColor = whiteBK;
      e3.BackColor = blackBK;
      e4.BackColor = whiteBK;
      e5.BackColor = blackBK;
      e6.BackColor = whiteBK;
      e7.BackColor = blackBK;
      e8.BackColor = whiteBK;

      f1.BackColor = whiteBK;
      f2.BackColor = blackBK;
      f3.BackColor = whiteBK;
      f4.BackColor = blackBK;
      f5.BackColor = whiteBK;
      f6.BackColor = blackBK;
      f7.BackColor = whiteBK;
      f8.BackColor = blackBK;

      g1.BackColor = blackBK;
      g2.BackColor = whiteBK;
      g3.BackColor = blackBK;
      g4.BackColor = whiteBK;
      g5.BackColor = blackBK;
      g6.BackColor = whiteBK;
      g7.BackColor = blackBK;
      g8.BackColor = whiteBK;

      h1.BackColor = whiteBK;
      h2.BackColor = blackBK;
      h3.BackColor = whiteBK;
      h4.BackColor = blackBK;
      h5.BackColor = whiteBK;
      h6.BackColor = blackBK;
      h7.BackColor = whiteBK;
      h8.BackColor = blackBK;
    }

    #endregion

    #region Move List Stuff

    //*******************************************************************************************************
    private void moveList_KeyDown(object sender, KeyEventArgs e)
    {
        //scrollMove = 0;
        
        if (e.KeyCode.ToString().Equals("Home"))
        {
            if (moveTree.SelectedNode.Parent != null)
            {
                moveTree.SelectedNode = moveTree.SelectedNode.Parent;
                e.Handled = true;
            }
        }
        else if (e.KeyCode.ToString().Equals("End"))
        {
            if (moveTree.SelectedNode.Text.StartsWith("Main") || moveTree.SelectedNode.Text.StartsWith("Puzzle"))
            {
                if (moveTree.Nodes[0].Nodes.Count > 0)
                {
                    moveTree.SelectedNode = moveTree.Nodes[0].Nodes[moveTree.Nodes[0].Nodes.Count - 1];
                    e.Handled = true;
                }
            }
            else if (moveTree.SelectedNode.Parent.Nodes.Count > 0)
            {
                moveTree.SelectedNode = moveTree.SelectedNode.Parent.Nodes[moveTree.SelectedNode.Parent.Nodes.Count - 1];
                e.Handled = true;
            }
        }
        else if (e.KeyCode.ToString().Equals("Down"))
        {
            //scrollMove = 1;
            if ((moveTree.SelectedNode.NextNode == null) && (moveTree.SelectedNode.Nodes.Count == 0))
            {
                //moveTree.SelectedNode = moveTree.SelectedNode.LastNode;
                e.Handled = true;
            }
        }
        else if (e.KeyCode.ToString().Equals("Next")) //pageDown
        {
            if (moveTree.SelectedNode.Text.Equals("Main") || moveTree.SelectedNode.Text.Equals("Puzzle"))
            {
                if (moveTree.Nodes[0].Nodes.Count < (moveTree.VisibleCount - 1))
                {
                    moveTree.SelectedNode = moveTree.Nodes[0].LastNode;
                    e.Handled = true;
                }
                else
                {
                    moveTree.SelectedNode = moveTree.Nodes[0].Nodes[(moveTree.VisibleCount - 1)];
                    e.Handled = true;
                }
            }
            else if ((moveTree.SelectedNode.NextNode == null) && (moveTree.SelectedNode.Nodes.Count == 0))
            {
                //moveTree.SelectedNode = moveTree.SelectedNode.LastNode;
                e.Handled = true;
            }
            else if ((moveTree.SelectedNode.Index + 20) < moveTree.SelectedNode.Parent.Nodes.Count)
            {
                //scrollMove = (moveTree.VisibleCount - 1)/2;
                moveTree.SelectedNode = moveTree.SelectedNode.Parent.Nodes[moveTree.SelectedNode.Index + (moveTree.VisibleCount - 1)];
                e.Handled = true;
            }
            else
            {
                moveTree.SelectedNode = moveTree.SelectedNode.Parent.LastNode;
                e.Handled = true;
            }
        }
        else if (e.KeyCode.ToString().Equals("PageUp"))
        {
            if (moveTree.SelectedNode.Text.Equals("Main") || moveTree.SelectedNode.Text.Equals("Puzzle"))
            {
            }
            else if (moveTree.SelectedNode.Index == 0)
            {
                moveTree.SelectedNode = moveTree.SelectedNode.Parent;
                e.Handled = true;
            }
            else if (moveTree.SelectedNode.Index < moveTree.VisibleCount - 1)
            {
                moveTree.SelectedNode = moveTree.SelectedNode.Parent.FirstNode;
                e.Handled = true;
            }
            else
            {
                //scrollMove = -(moveTree.VisibleCount - 1)/2;
                moveTree.SelectedNode = moveTree.SelectedNode.Parent.Nodes[moveTree.SelectedNode.Index - (moveTree.VisibleCount - 1)];
                e.Handled = true;
            }
        }
      //future changes???
      //else if (e.KeyCode.ToString().Equals("Up"))
      //{
      //}
      //else if (e.KeyCode.ToString().Equals("Right"))
      //{
      //}
      //else if (e.KeyCode.ToString().Equals("Left"))
      //{
      //}
      //else if (e.KeyCode.ToString().Equals("Subtract"))
      //{
      //}
      //else if (e.KeyCode.ToString().Equals("Add"))
      //{
      //}
    }

    //*******************************************************************************************************
    private void moveTree_ClientSizeChanged(object sender, EventArgs e)
    {
        int style = GetWindowLong(moveTree.Handle, GWL_STYLE);
        hasHScroll = ((style & WS_HSCROLL) != 0);
        hasVScroll = ((style & WS_VSCROLL) != 0);
    }

    //*******************************************************************************************************
    private void moveTree_AfterExpand(object sender, TreeViewEventArgs e)
    {
        AdjustVScroll();

    }

    //*******************************************************************************************************
    private void moveTree_AfterCollapse(object sender, TreeViewEventArgs e)
    {
        AdjustVScroll();

    }

    //*******************************************************************************************************
    private void comment_KeyDown(object sender, KeyEventArgs e)
    {
      savePGN.BackColor = Color.Red;
      updateComment = true;
    }

    //*******************************************************************************************************
    private void gameInfo_KeyDown(object sender, KeyEventArgs e)
    {
      savePGN.BackColor = Color.Red;
    }

    //*******************************************************************************************************
    private void nag_DropDown(object sender, EventArgs e)
    {
      nagDropped = true;
    }

    //*******************************************************************************************************
    protected void InitNag()
    {
      //NAG Interpretation 
      nag.Items.Add(""); // null annotation 
      nag.Items.Add("good move !"); // (traditional "!") 
      nag.Items.Add("poor move ?"); // (traditional "?") *** MISTAKE ***
      nag.Items.Add("very good move !!"); // (traditional "!!") 
      nag.Items.Add("very poor move ??"); // (traditional "??") *** BLUNDER ***
      nag.Items.Add("speculative move !?"); // (traditional "!?") 
      nag.Items.Add("questionable move ?!"); // (traditional "?!") *** INACCURACY ***
      nag.Items.Add("forced move (all others lose quickly)");
      nag.Items.Add("singular move (no reasonable alternatives)");
      nag.Items.Add("worst move");
      nag.Items.Add("drawish position");
      nag.Items.Add("equal chances, quiet position");
      nag.Items.Add("equal chances, active position");
      nag.Items.Add("unclear position");
      nag.Items.Add("White has a slight advantage");
      nag.Items.Add("Black has a slight advantage");
      nag.Items.Add("White has a moderate advantage");
      nag.Items.Add("Black has a moderate advantage");
      nag.Items.Add("White has a decisive advantage");
      nag.Items.Add("Black has a decisive advantage");
      nag.Items.Add("White has a crushing advantage (Black should resign)");
      nag.Items.Add("Black has a crushing advantage (White should resign)");
      nag.Items.Add("White is in zugzwang");
      nag.Items.Add("Black is in zugzwang");
      nag.Items.Add("White has a slight space advantage");
      nag.Items.Add("Black has a slight space advantage");
      nag.Items.Add("White has a moderate space advantage");
      nag.Items.Add("Black has a moderate space advantage");
      nag.Items.Add("White has a decisive space advantage");
      nag.Items.Add("Black has a decisive space advantage");
      nag.Items.Add("White has a slight time (development) advantage");
      nag.Items.Add("Black has a slight time (development) advantage");
      nag.Items.Add("White has a moderate time (development) advantage");
      nag.Items.Add("Black has a moderate time (development) advantage");
      nag.Items.Add("White has a decisive time (development) advantage");
      nag.Items.Add("Black has a decisive time (development) advantage");
      nag.Items.Add("White has the initiative");
      nag.Items.Add("Black has the initiative");
      nag.Items.Add("White has a lasting initiative");
      nag.Items.Add("Black has a lasting initiative");
      nag.Items.Add("White has the attack");
      nag.Items.Add("Black has the attack");
      nag.Items.Add("White has insufficient compensation for material deficit");
      nag.Items.Add("Black has insufficient compensation for material deficit");
      nag.Items.Add("White has sufficient compensation for material deficit");
      nag.Items.Add("Black has sufficient compensation for material deficit");
      nag.Items.Add("White has more than adequate compensation for material deficit");
      nag.Items.Add("Black has more than adequate compensation for material deficit");
      nag.Items.Add("White has a slight center control advantage");
      nag.Items.Add("Black has a slight center control advantage");
      nag.Items.Add("White has a moderate center control advantage");
      nag.Items.Add("Black has a moderate center control advantage");
      nag.Items.Add("White has a decisive center control advantage");
      nag.Items.Add("Black has a decisive center control advantage");
      nag.Items.Add("White has a slight kingside control advantage");
      nag.Items.Add("Black has a slight kingside control advantage");
      nag.Items.Add("White has a moderate kingside control advantage");
      nag.Items.Add("Black has a moderate kingside control advantage");
      nag.Items.Add("White has a decisive kingside control advantage");
      nag.Items.Add("Black has a decisive kingside control advantage");
      nag.Items.Add("White has a slight queenside control advantage");
      nag.Items.Add("Black has a slight queenside control advantage");
      nag.Items.Add("White has a moderate queenside control advantage");
      nag.Items.Add("Black has a moderate queenside control advantage");
      nag.Items.Add("White has a decisive queenside control advantage");
      nag.Items.Add("Black has a decisive queenside control advantage");
      nag.Items.Add("White has a vulnerable first rank");
      nag.Items.Add("Black has a vulnerable first rank");
      nag.Items.Add("White has a well protected first rank");
      nag.Items.Add("Black has a well protected first rank");
      nag.Items.Add("White has a poorly protected king");
      nag.Items.Add("Black has a poorly protected king");
      nag.Items.Add("White has a well protected king");
      nag.Items.Add("Black has a well protected king");
      nag.Items.Add("White has a poorly placed king");
      nag.Items.Add("Black has a poorly placed king");
      nag.Items.Add("White has a well placed king");
      nag.Items.Add("Black has a well placed king");
      nag.Items.Add("White has a very weak pawn structure");
      nag.Items.Add("Black has a very weak pawn structure");
      nag.Items.Add("White has a moderately weak pawn structure");
      nag.Items.Add("Black has a moderately weak pawn structure");
      nag.Items.Add("White has a moderately strong pawn structure");
      nag.Items.Add("Black has a moderately strong pawn structure");
      nag.Items.Add("White has a very strong pawn structure");
      nag.Items.Add("Black has a very strong pawn structure");
      nag.Items.Add("White has poor knight placement");
      nag.Items.Add("Black has poor knight placement");
      nag.Items.Add("White has good knight placement");
      nag.Items.Add("Black has good knight placement");
      nag.Items.Add("White has poor bishop placement");
      nag.Items.Add("Black has poor bishop placement");
      nag.Items.Add("White has good bishop placement");
      nag.Items.Add("Black has good bishop placement");
      nag.Items.Add("White has poor rook placement");
      nag.Items.Add("Black has poor rook placement");
      nag.Items.Add("White has good rook placement");
      nag.Items.Add("Black has good rook placement");
      nag.Items.Add("White has poor queen placement");
      nag.Items.Add("Black has poor queen placement");
      nag.Items.Add("White has good queen placement");
      nag.Items.Add("Black has good queen placement");
      nag.Items.Add("White has poor piece coordination");
      nag.Items.Add("Black has poor piece coordination");
      nag.Items.Add("White has good piece coordination");
      nag.Items.Add("Black has good piece coordination");
      nag.Items.Add("White has played the opening very poorly");
      nag.Items.Add("Black has played the opening very poorly");
      nag.Items.Add("White has played the opening poorly");
      nag.Items.Add("Black has played the opening poorly");
      nag.Items.Add("White has played the opening well");
      nag.Items.Add("Black has played the opening well");
      nag.Items.Add("White has played the opening very well");
      nag.Items.Add("Black has played the opening very well");
      nag.Items.Add("White has played the middlegame very poorly");
      nag.Items.Add("Black has played the middlegame very poorly");
      nag.Items.Add("White has played the middlegame poorly");
      nag.Items.Add("Black has played the middlegame poorly");
      nag.Items.Add("White has played the middlegame well");
      nag.Items.Add("Black has played the middlegame well");
      nag.Items.Add("White has played the middlegame very well");
      nag.Items.Add("Black has played the middlegame very well");
      nag.Items.Add("White has played the ending very poorly");
      nag.Items.Add("Black has played the ending very poorly");
      nag.Items.Add("White has played the ending poorly");
      nag.Items.Add("Black has played the ending poorly");
      nag.Items.Add("White has played the ending well");
      nag.Items.Add("Black has played the ending well");
      nag.Items.Add("White has played the ending very well");
      nag.Items.Add("Black has played the ending very well");
      nag.Items.Add("White has slight counterplay");
      nag.Items.Add("Black has slight counterplay");
      nag.Items.Add("White has moderate counterplay");
      nag.Items.Add("Black has moderate counterplay");
      nag.Items.Add("White has decisive counterplay");
      nag.Items.Add("Black has decisive counterplay");
      nag.Items.Add("White has moderate time control pressure");
      nag.Items.Add("Black has moderate time control pressure");
      nag.Items.Add("White has severe time control pressure");
      nag.Items.Add("Black has severe time control pressure");
      nag.Items.Add("Non-standard NAG");

      nag.SelectedIndex = 0;
    }

    //*******************************************************************************************************
    protected void nag_SelectedIndexChanged(object sender, EventArgs e)
    {
      //need to not do this unless the user is actually changing the index through the gui
      if (nagDropped)
      {
        ((Move)(moveTree.SelectedNode.Tag)).nag = nag.SelectedIndex;
        moveTree.Refresh();
        //        changesMade.Visible = true;
        savePGN.BackColor = Color.Red;
        nagDropped = false;
      }
    }

    //*******************************************************************************************************
    private void moveTree_AfterSelect(object sender, TreeViewEventArgs e)
    {
      try
      {
        TreeView tmpTV = (TreeView)sender;

        if (!tmpTV.SelectedNode.Text.StartsWith("???"))
        {
          Move tmpMove = (Move)tmpTV.SelectedNode.Tag;
          ResetPieces(tmpMove);
          tmpTV.SelectedNode.EnsureVisible();

          if (displayReady)
          {
            try
            {
              SoundPlayer simpleSound;

              if (enableSound && play.Checked)
              {
                simpleSound = new SoundPlayer(soundPath + "\\move.wav");
                simpleSound.Play();
              }
            }
            catch (Exception ex)
            {
              MessageBox.Show(ex.ToString());
            }
          }

          if (tmpTV.SelectedNode.Text.StartsWith("RAV") && moveAnalysis.Checked)
          {
            clearRAV.Visible = true;
            analyze.Visible = false;
            best.Visible = false;
          }
          else
          {
            if (!tmpTV.SelectedNode.Text.StartsWith("Main") && moveAnalysis.Checked)
            {
              clearRAV.Visible = false;
              analyze.Visible = true;
              best.Visible = true;
            }
          }
        }
        else
        {
          if (moveAnalysis.Checked)
          {
            clearRAV.Visible = false;
            analyze.Visible = false;
            best.Visible = false;
          }
        }

        AdjustVScroll();
      }
      catch
      {
      }
    }

    //*******************************************************************************************************
    private void AdjustVScroll()
    {
      if (hasVScroll && (moveTree.SelectedNode != null))
      {
        int pos = GetScrollPos((int)moveTree.Handle, SB_VERT);
        int error = (moveTree.SelectedNode.Bounds.Y - (moveTree.SelectedNode.Bounds.Height * moveTree.VisibleCount / 2)) / moveTree.SelectedNode.Bounds.Height;

        if (error == 0) error++;

        moveTree.BeginUpdate();
        SetScrollPos((IntPtr)moveTree.Handle, SB_VERT, pos + error, true);
        moveTree.EndUpdate();
        moveTree.Refresh();
      }
    }
      
    //*******************************************************************************************************
    private string GetLabel(Move mv)
    {
      if (showSAN)
      {
        if (mv.whitesMove)
        {
          return padding[mv.fullMove.ToString().Length + 3] + mv.fullMove.ToString() + ". " + mv.san + padding[mv.san.Length - 1];
        }
        else
        {
          return "     " + mv.san + padding[mv.san.Length - 1];
        }
      }
      else
      {
        if (mv.whitesMove)
        {
          return padding[mv.fullMove.ToString().Length + 3] + mv.fullMove.ToString() + ". " + mv.lan + padding[mv.lan.Length + 1];
        }
        else
        {
          return "     " + mv.lan + padding[mv.lan.Length + 1];
        }
      }
    }

    //*******************************************************************************************************
    private Color GetColor(Move mv)
    {
      if (mv.whitesMove)
      {
        return whiteBK;
      }
      else
      {
        return blackBK;
      }
    }

    //*******************************************************************************************************
    private void ChangeLabel(TreeNode treeNode)
    {
      foreach (TreeNode tn in treeNode.Nodes)
      {
        if (tn.Tag != null)
        {
          if (!tn.Text.StartsWith("Main") && !tn.Text.StartsWith("Puzzle") && !tn.Text.StartsWith("RAV"))
          {
            try
            {
              tn.Text = GetLabel((Move)(tn.Tag));
            }
            catch
            {
            }
          }
        }

        TreeNodeCollection nodes = tn.Nodes;
        foreach (TreeNode n in nodes)
        {
          ChangeLabel(n);
        }
      }
    }

    //*******************************************************************************************************
    private void ChangeColor(TreeNode treeNode)
    {
      foreach (TreeNode tn in treeNode.Nodes)
      {
        if (tn.Tag != null)
        {
          if (!tn.Text.StartsWith("Main") && !tn.Text.StartsWith("Puzzle") && !tn.Text.StartsWith("RAV"))
          {
            try
            {
              tn.BackColor = GetColor((Move)(tn.Tag));
            }
            catch
            {
            }
          }
        }

        TreeNodeCollection nodes = tn.Nodes;
        foreach (TreeNode n in nodes)
        {
          ChangeColor(n);
        }
      }
    }

    //*******************************************************************************************************
    private void toggleSoundOnOffToolStripMenuItem_Click(object sender, EventArgs e)
    {
      enableSound = !enableSound;
      //if the sound paths are good allow sounds to be turned on
      if (enableSound && CheckSoundPaths())
      {
          chess.PutINIValue("SETTINGS", "ENABLE_SOUND", enableSound);
          return;
      }
      chess.PutINIValue("SETTINGS", "ENABLE_SOUND", false);
    }

    //*******************************************************************************************************
    private void showSANLANMoveListToolStripMenuItem_Click(object sender, EventArgs e)
    {
      //re-label all the nodes that have Moves as a tag
      showSAN = !showSAN;
      if (moveTree.Nodes[0].Nodes.Count > 0)
      {
        TreeNodeCollection nodes = moveTree.Nodes;
        moveTree.BeginUpdate();
        foreach (TreeNode n in nodes)
        {
          ChangeLabel(n);
        }
        moveTree.EndUpdate();
      }
      chess.PutINIValue("SETTINGS", "SHOW_SAN", showSAN);
    }

    //*******************************************************************************************************
    private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
      showSANLANMoveListToolStripMenuItem.Text = (showSAN ? "Show LAN Move List" : "Show SAN Move List");
      toggleSoundOnOffToolStripMenuItem.Text = (enableSound ? "Turn Sounds Off" : "Turn Sounds On");
    }

    //*******************************************************************************************************
    protected void AddComment(string comment)
    {
      comment = comment.Trim();
      if (comment.Length > 0)
      {
        ((Move)(moveTree.SelectedNode.Tag)).AddComment(comment);
        moveTree.Refresh();
        //        changesMade.Visible = true;
        savePGN.BackColor = Color.Red;
      }
    }

    //*******************************************************************************************************
    protected void comment_TextChanged(object sender, EventArgs e)
    {
      if (moveTree.SelectedNode.Tag != null)
      {
        if (updateComment) ((Move)(moveTree.SelectedNode.Tag)).comment = comment.Text;
      }
    }

    //*******************************************************************************************************
    protected void addRAV_Click(object sender, EventArgs e)
    {
      if (positionSetup.Checked)
      {
        ResetPieces(fenNotation.Text);
      }
      else
      {
        //add the last rav created by the chess engine
        newPosition = "RAV";
        WriteSIOfEngine("stop", (computerWhite.Checked ? 1 : 2));
        computerWhite.Enabled = true;
        computerBlack.Enabled = true;
        addRAV.Visible = false;
      }
      moveTree.Focus();
    }

    //*******************************************************************************************************
    protected void clearRAV_Click(object sender, EventArgs e)
    {
      if (moveTree.SelectedNode.Text.StartsWith("RAV"))
      {
        moveTree.SelectedNode.Remove();
        //        changesMade.Visible = true;
        savePGN.BackColor = Color.Red;
      }
      else
      {
        moveStatus.Text = "Select the 'RAV' node that you wish to delete...";
      }

      moveTree.Focus();
    }

    //*******************************************************************************************************
    private void AddRAVRecord(bool best, string RAVList, string result)
    {
      string fen = ((Move)(moveTree.SelectedNode.Tag)).priorFen;

      ArrayList Moves = new ArrayList();
      Moves = virtualChessBoard.LoadList(RAVList, fen, false, ("{" + (best ? (betterMove ? "*BETTER MOVE " : "*BEST MOVE ") : "*MOVE ANALYSIS ") + result + "} "));

      if (Moves.Count > 0)
      {
        moveTree.SelectedNode.Nodes.Add(new TreeNode("RAV       "));
        moveTree.SelectedNode.Nodes[moveTree.SelectedNode.Nodes.Count - 1].Tag = Moves[0];

        for (int row = 0; row < Moves.Count; row++)
        {
          AddMoveNode((Move)(Moves[row]), moveTree.SelectedNode.Nodes[moveTree.SelectedNode.Nodes.Count - 1]);
        }

        //        changesMade.Visible = true;
        savePGN.BackColor = Color.Red;

        if (evaluateRAV.Checked)
        {
          //evaluate each RAV move for its pawn value, not for another alternate RAV list...
        }

      }
      else
      {
        MessageBox.Show("Alternate movelist is invalid.");
      }
    }

    #endregion

    #region Mouse Events

    //*******************************************************************************************************
    private bool ManualMoveOK()
    {
      if (play.Checked)
      {
        if (puzzle) return true;

        if ((whitesMove && computerWhite.Checked) || (!whitesMove && computerBlack.Checked))
        {
          return false;
        }
        else if ((moveTree.SelectedNode == moveTree.Nodes[0].LastNode) || (moveTree.Nodes[0].Nodes.Count == 0))
        {
          return moveOK;
        }
        else
        {
          moveTree.SelectedNode = moveTree.Nodes[0].LastNode;
          return false;
        }
      }
      else
      {
        return positionSetup.Checked || (moveAnalysis.Checked && (goOK || (!uciok || !uciok2)));
      }
    }

    //*******************************************************************************************************
    protected bool MakeAMove(string src, string dst)
    {
      if (dst.Length > 2)
      {
        if (!IsAPawnPromotion(src, dst.Substring(0, 2)))
        {
          ClearPieceMoving();
          MessageBox.Show("Unknown move, src = " + src + ", dst = " + dst);
          return false;
        }
      }
      else if (IsAPawnPromotion(src, dst))
      {
        ClearPieceMoving();
        Promotion dialog = new ns_Promotion.Promotion(this, whitesMove);
        dialog.ShowDialog();
        dst += dialog.PromotedTo;
      }

      Move moveObj = new Move(virtualChessBoard.fenStart, 1);

      if (virtualChessBoard.CheckMoveQuietly(src + dst, fenNotation.Text, ref moveObj))
      {
        if (moveTree.SelectedNode == null)
        {
          return false;
        }
        else if (moveTree.SelectedNode.Text.StartsWith("Main") || moveTree.SelectedNode.Text.StartsWith("Puzzle"))
        {
          AddMoveNode(moveObj, moveTree.SelectedNode);
          moveTree.SelectedNode = moveTree.SelectedNode.Nodes[0];
        }
        else if (moveTree.SelectedNode.Text.StartsWith("RAV"))
        {
        }
        else if (moveTree.SelectedNode.NextNode == null)
        {
          AddMoveNode(moveObj, moveTree.SelectedNode.Parent);
          moveTree.SelectedNode = moveTree.SelectedNode.NextNode;
          if (play.Checked)
          {
            CheckForDuplicateMoves();
          }
        }
        else
        {
          if (play.Checked)
          {
            AddMoveNode(moveObj, moveTree.SelectedNode.Parent);
            moveTree.SelectedNode = moveTree.SelectedNode.LastNode;
            CheckForDuplicateMoves();
          }
          else if (moveTree.SelectedNode.NextNode != null)
          {
            //must be adding a RAV at this location
            moveTree.SelectedNode.NextNode.Nodes.Add(new TreeNode("RAV       "));
            moveTree.SelectedNode.NextNode.Nodes[moveTree.SelectedNode.NextNode.Nodes.Count - 1].Tag = moveObj;
            AddMoveNode(moveObj, moveTree.SelectedNode.NextNode.Nodes[moveTree.SelectedNode.NextNode.Nodes.Count - 1]);
            moveTree.SelectedNode = moveTree.SelectedNode.NextNode.Nodes[moveTree.SelectedNode.NextNode.Nodes.Count - 1].Nodes[0];
          }
          else
          {
            return false;
          }
        }

        fenNotation.Text = moveObj.fen;
        gameOver = moveObj.gameOver;
        inCheck = moveObj.inCheck;
        staleMate = moveObj.staleMate;
        insufficientMaterial = moveObj.insufficientMaterial;
        halfMove = moveObj.halfMove;
        fullMove = moveObj.fullMove;

        lastManualRow = moveTree.Nodes.Count - 1;

        if (gameOver)
        {
          moveStatus.Text = "Game over.";
          whitesMove = !moveObj.whitesMove;
          OnGameOver();
        }
        else
        {
          if (inCheck)
          {
            if (enableSound && play.Checked)
            {
              SoundPlayer simpleSound = new SoundPlayer(soundPath + "\\check.wav");
              simpleSound.Play();
            }
          }

          CheckForClaimingDraw();

          if (play.Checked && whiteDraw.Text.Equals("Claim") && computerWhite.Checked)
          {
            //make sure a computer based game ends if there are move violations - else the game may never end
            gameOver = true;
            if (computerWhite.Checked) WriteSIOfEngine("stop", 1);
            if (computerBlack.Checked) WriteSIOfEngine("stop", 2);
            WhiteDrawPressed();
          }
          else if (play.Checked && blackDraw.Text.Equals("Claim") && computerBlack.Checked)
          {
            //make sure a computer based game ends if there are move violations - else the game may never end
            gameOver = true;
            if (computerWhite.Checked) WriteSIOfEngine("stop", 1);
            if (computerBlack.Checked) WriteSIOfEngine("stop", 2);
            BlackDrawPressed();
          }
          else
          {
            moveStatus.Text = (whitesMove ? "Whites move." : "Blacks move.");
          }
        }

        ClearPieceMoving();
        return true;
      }

      ClearPieceMoving();
      return false;
    }

    //*******************************************************************************************************
    protected void ClearPieceMoving()
    {
      pieceMoving.AccessibleName = "";
      pieceMoving.AccessibleDescription = "";
      inv.Visible = false;
    }

    //*******************************************************************************************************
    protected void all_MouseDown(object sender, MouseEventArgs e)
    {
        if (ManualMoveOK() && (((Panel)sender).AccessibleName.Length > 0))
        {
            if ((e.X >= ((Panel)sender).Size.Width / 8) &&
               (e.X <= ((Panel)sender).Size.Width * 7 / 8) &&
               (e.Y >= ((Panel)sender).Size.Height / 8) &&
               (e.Y <= ((Panel)sender).Size.Height * 7 / 8))
            {
                //messageBox.Items.Insert(0, "Down = " + e.X.ToString() + ":" + e.Y.ToString());
                //messageBox.Items.Insert(0, "panel = " + ((Panel)sender).Size.Width.ToString() + ":" + ((Panel)sender).Size.Height.ToString());
                //messageBox.Items.Insert(0, "inv = " + inv.Size.Width.ToString() + ":" + inv.Size.Height.ToString());

                pieceMoving.AccessibleName = ((Panel)sender).AccessibleName;
                pieceMoving.AccessibleDescription = ((Panel)sender).Name;

                mouseDownLocation.X = e.X;
                mouseDownLocation.Y = e.Y;

                inv.Size = new Size(a8.Size.Width, a8.Size.Height);
                inv.BackgroundImage = blank;
                inv.BackgroundImage = SelectPiece(pieceMoving.AccessibleName);
                inv.Location = new Point(Location.X + theBoard.Location.X + ((Panel)sender).Location.X + 8,
                                         Location.Y + theBoard.Location.Y + ((Panel)sender).Location.Y + 30);

                inv.Visible = true;
                inv.Refresh();

                ((Panel)sender).BackgroundImage = blank;
                ((Panel)sender).Refresh();
            }
        }
        else
        {
            ClearPieceMoving();
        }
    }

    //*******************************************************************************************************
    protected void all_MouseMove(object sender, MouseEventArgs e)
    {
        if (pieceMoving.AccessibleName.Length > 0)
        {
            //messageBox.Items.Insert(0, "Move = " + e.X.ToString() + ":" + e.Y.ToString());

            inv.Location = new Point(Location.X + theBoard.Location.X + ((Panel)sender).Location.X + e.X - mouseDownLocation.X + 8,
                                     Location.Y + theBoard.Location.Y + ((Panel)sender).Location.Y + e.Y - mouseDownLocation.Y + 30);

            inv.Refresh();
        }
        else
        {
            try
            {
                onPanel = ((Panel)sender).Name;
            }
            catch
            {
            }
        }
    }

    //*******************************************************************************************************
    protected void all_MouseUp(object sender, MouseEventArgs e)
    {
        if (pieceMoving.AccessibleName.Length > 0)
        {
            string dst, src = pieceMoving.AccessibleDescription;

            if (ManualMoveOK())
            {
                //messageBox.Items.Insert(0, "Up = " + e.X.ToString() + ":" + e.Y.ToString());
                
                int X = (theBoard.Controls[src].Location.X + e.X) / a1.Width;
                int Y = (theBoard.Controls[src].Location.Y + e.Y) / a1.Height;

                if (OnBoard(X, Y))
                {
                    if (reversedBoard)
                    {
                        dst = theBoard.Controls[reversedBoardLayout[X, Y]].Name;
                    }
                    else
                    {
                        dst = theBoard.Controls[boardLayout[X, Y]].Name;
                    }

                    if (dst.Equals(src))
                    {
                        //piece was not moved...should implement touch move rule?
                        theBoard.Controls[dst].BackgroundImage = SelectPiece(pieceMoving.AccessibleName);
                        ClearPieceMoving();
                        InvalidMoveMade();
                    }
                    else if (!positionSetup.Checked || (positionSetup.Checked && makeAPuzzle.Checked))
                    {
                        //rules apply!
                        if ((whitesMove && pieceMoving.AccessibleName.StartsWith("w")) ||
                           (!whitesMove && pieceMoving.AccessibleName.StartsWith("k")))
                        {
                            if (puzzle)
                            {
                                //check for pawn promotion first
                                if (dst.Length > 2)
                                {
                                    if (!IsAPawnPromotion(src, dst.Substring(0, 2)))
                                    {
                                        MessageBox.Show("Unknown move, src = " + src + ", dst = " + dst);
                                        ResetPieces(fenNotation.Text);
                                        return;
                                    }
                                }
                                else if (IsAPawnPromotion(src, dst))
                                {
                                    Promotion dialog = new ns_Promotion.Promotion(this, whitesMove);
                                    dialog.ShowDialog();
                                    dst += dialog.PromotedTo;
                                }

                                if (MoveMatch(src + dst))
                                {
                                    //this is done to see if the move ends the game
                                    Move moveObj = new Move(virtualChessBoard.fenStart, 1);

                                    if (virtualChessBoard.CheckMoveQuietly(src + dst, fenNotation.Text, ref moveObj))
                                    {
                                        if (moveTree.SelectedNode.Text.Equals("Puzzle"))
                                        {
                                            moveTree.Nodes[0].FirstNode.ForeColor = Color.Black;
                                            moveTree.SelectedNode = moveTree.Nodes[0].FirstNode;
                                        }
                                        else
                                        {
                                            moveTree.SelectedNode.NextNode.ForeColor = Color.Black;
                                            moveTree.SelectedNode = moveTree.SelectedNode.NextNode;
                                        }
                                        ClearPieceMoving();

                                        if (moveObj.gameOver)
                                        {
                                            moveStatus.Text = "Puzzle Successfully Completed!!!";
                                            return;
                                        }
                                        else
                                        {
                                            //make the computers next move
                                            if (moveTree.SelectedNode.NextNode != null)
                                            {
                                                Thread.Sleep(1000);
                                                moveTree.SelectedNode.NextNode.ForeColor = Color.Black;
                                                moveTree.SelectedNode = moveTree.SelectedNode.NextNode;
                                            }
                                            else
                                            {
                                                //there are no move moves so the game must be over
                                                moveStatus.Text = "Puzzle Successfully Completed!!!";
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //this should not happen...the puzzle moves were check when loaded
                                        MessageBox.Show("Puzzle Error...");
                                    }
                                    return;
                                }
                                else
                                {
                                    InvalidMoveMade();
                                    ResetPieces(fenNotation.Text);
                                    moveStatus.Text = "Not the correct move ... try again.";
                                    ClearPieceMoving();
                                    return;
                                }
                            }
                            else
                            {
                                if (MakeAMove(src, dst))
                                {
                                    if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
                                    {
                                        WriteEngineCommands(src, dst);
                                    }

                                    if (play.Checked)
                                    {
                                        savePGN.Visible = true;
                                        pgnLoaded = true;
                                    }

                                    return;
                                }
                                else
                                {
                                    InvalidMoveMade();
                                    ResetPieces(fenNotation.Text);
                                    moveStatus.Text = "Invalid move not allowed, " + (whitesMove ? "whites move." : "blacks move.");
                                    ClearPieceMoving();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            InvalidMoveMade();
                            ResetPieces(fenNotation.Text);
                            moveStatus.Text = "Out of turn move, " + (whitesMove ? "whites move." : "blacks move.");
                            ClearPieceMoving();
                            return;
                        }
                    }
                    else
                    {
                        //move the piece, rules do not apply
                        theBoard.Controls[dst].BackgroundImage = SelectPiece(pieceMoving.AccessibleName);
                        theBoard.Controls[dst].AccessibleName = pieceMoving.AccessibleName;
                        theBoard.Controls[dst].Refresh();
                        theBoard.Controls[src].AccessibleName = "";
                        ClearPieceMoving();
                        fenNotation.Text = ToFEN(whitesMove);
                        return;
                    }
                }
                else
                {
                    InvalidMoveMade();
                    ResetPieces(fenNotation.Text);
                    moveStatus.Text = "Moves off board not allowed, " + (whitesMove ? "whites move." : "blacks move.");
                    ClearPieceMoving();
                    return;
                }
            }

            InvalidMoveMade();
            ResetPieces(fenNotation.Text);
            moveStatus.Text = "Manual moves not allowed, " + (whitesMove ? "whites move." : "blacks move.");
            ClearPieceMoving();
        }
        else
        {
            ClearPieceMoving();
            moveStatus.Text = "No piece selected to move, " + (whitesMove ? "whites move." : "blacks move.");
        }
    }

    #endregion

    #region Playing Mode

    //*******************************************************************************************************
    protected void DisableMainSelections()
    {
      play.Enabled = false;
      positionSetup.Enabled = false;
      moveAnalysis.Enabled = false;
      gameAnalysis.Enabled = false;
    }

    //*******************************************************************************************************
    protected virtual void DisableEverything()
    {
      //engine stuff
      if (uciok)
      {
          StopYourEngine(1);
      }

      if (uciok2)
      {
          StopYourEngine(2);
      }

      computerWhite.Visible = false;
      computerBlack.Visible = false;
      computerAnalysisResult.Visible = false;
      bestMove.Visible = false;
      computerClock.Visible = false;
      computerTime.Visible = false;
      computerTimeLabel.Visible = false;
      deepTime.Visible = false;
      deepTimeLabel.Visible = false;
      abortTime.Visible = false;
      questionableMove.Visible = false;
      poorMove.Visible = false;
      veryPoorMove.Visible = false;
      slightlyBetter.Visible = false;
      evaluateRAV.Visible = false;
      useAbortTime.Visible = false;
      clearEngineOutput.Visible = false;

      //buttons
      analyze.Visible = false;
      best.Visible = false;
      addRAV.Visible = false;
      clearRAV.Visible = false;
      ready.Visible = false;
      ready2.Visible = false;
      pauseGame.Visible = false;
      whiteDraw.Visible = false;
      blackDraw.Visible = false;
      whiteResign.Visible = false;
      blackResign.Visible = false;
      savePGN.Visible = false;
      loadPGN.Visible = false;

      //clocks
      useClock.Visible = false;
      useClock.Enabled = true;
      clockStart.Visible = false;
      bronsteinTime.Visible = false;
      whiteClock.Visible = false;
      blackClock.Visible = false;

      moveOK = false;
      makeAPuzzle.Visible = false;
    }

    //*******************************************************************************************************
    protected void pauseGame_Click(object sender, EventArgs e)
    {
      if (play.Checked)
      {
        if (pauseGame.Text.Equals("Pause"))
        {
          runClock = false;
          if (runCPUClock)
          {
            runCPUClock = false;
            gamePaused = true;

            if (computerWhite.Checked) WriteSIOfEngine("stop", 1);
            if (computerBlack.Checked) WriteSIOfEngine("stop", 2);

            ResetComputerClock();
          }
          pauseGame.Text = "Continue";
        }
        else if (pauseGame.Text.Equals("Continue"))
        {
          pauseGame.Text = "Pause";
          runClock = true;

          if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
          {
            if (gamePaused)
            {
              gamePaused = false;

              WriteSIOfEngine("position fen " + fenNotation.Text, (whitesMove ? 1 : 2));
              WriteSIOfEngine("go infinite", (whitesMove ? 1 : 2));
              runCPUClock = true;
            }
          }
        }
      }
      else
      {
        StartDeepAnalysis();
      }
      moveTree.Focus();
    }

    //*******************************************************************************************************
    protected string GetHeaderValue(string value)
    {
      int i = gameInfo.Text.IndexOf("[" + value + " \"");

      if (i > 0)
      {
        int length = value.Length + 3;
        return gameInfo.Text.Substring(i + length, gameInfo.Text.IndexOf("\"", i + length) - i - length);
      }
      return "";
    }

    //*******************************************************************************************************
    protected void play_CheckedChanged(object sender, EventArgs e)
    {
      if (play.Checked)
      {
        SetupForNextLiveGame();
      }
    }

    //*******************************************************************************************************
    protected void positionSetup_CheckedChanged(object sender, EventArgs e)
    {
      if (positionSetup.Checked)
      {
        DisableEverything();
        ChangeButton(ref loadPGN, "Load", "Load PGN Header Only, loading FEN starting point.", true);
        ChangeButton(ref savePGN, "Save", "Save PGN Header or a puzzle, saving current FEN starting point in the header.", true);
        ChangeButton(ref addRAV, "Update", "Update board to reflect changes to the FEN strings.", true);
        moveStatus.Text = "Change positions manually, with menu options or with FEN edits.";
        makeAPuzzle.Visible = true;
        puzzle = false;

        PSContextMenu(true);
        fenNotation.ReadOnly = false;
        duplicateFENs.ReadOnly = false;

        moveTree.Nodes.Clear();
        moveTree.Nodes.Add(new TreeNode("Main"));
        //        moveTree.Nodes[0].Tag = new Move(fenNotation.Text, fenNotationPlus.Text, 1);
        moveTree.Nodes[0].Tag = new Move(fenNotation.Text, 1);
        moveTree.SelectedNode = moveTree.Nodes[0];
        moveTree.Focus();
      }
      else
      {
        PSContextMenu(false);
        fenNotation.ReadOnly = true;
        duplicateFENs.ReadOnly = true;
      }
    }

    //*******************************************************************************************************
    protected void moveAnalysis_CheckedChanged(object sender, EventArgs e)
    {
      if (moveAnalysis.Checked)
      {
        DisableEverything();

        ChangeButton(ref loadPGN, "Load", "Load a game to Analyze");
        ChangeButton(ref savePGN, "Save", "Save the game.", pgnLoaded);
        ChangeButton(ref addRAV, "Add RAV", "Add CPU step analysis RAV move sequence.", false);

        moveStatus.Text = "Load PGN file to start a move analysis.";

        computerWhite.Visible = true;
        computerBlack.Visible = true;

        if (computerWhite.Checked)
        {
          computerBlack.Checked = false;
          if (!readyok) StartYourEngine(1);
        }
        else if (!computerBlack.Checked)
        {
          computerWhite.Checked = true;
          if (!readyok) StartYourEngine(1);
        }
        else
        {
          if (!readyok2) StartYourEngine(2);
        }

        useClock.Visible = false;
        bronsteinTime.Visible = false;
        whiteClock.Visible = false;
        blackClock.Visible = false;
      }
    }

    //*******************************************************************************************************
    protected void gameAnalysis_CheckedChanged(object sender, EventArgs e)
    {
      if (gameAnalysis.Checked)
      {
        DisableEverything();

        ChangeButton(ref loadPGN, "Load", "Load a game to Analyze");
        ChangeButton(ref savePGN, "Save", "Save the game.", pgnLoaded);
        ChangeButton(ref pauseGame, "Start", "Start Analysis of current move list.", pgnLoaded);

        moveStatus.Text = "Load PGN file to start game analysis.";
        deepTime.Visible = true;
        deepTimeLabel.Visible = true;
        abortTime.Visible = useAbortTime.Checked;

        questionableMove.Visible = true;
        poorMove.Visible = true;
        veryPoorMove.Visible = true;
        slightlyBetter.Visible = true;
        evaluateRAV.Visible = true;
        useAbortTime.Visible = true;
        abortTime.Visible = true;

        computerClock.Visible = true;
        computerTime.Visible = true;
        computerTimeLabel.Visible = true;
        bestMove.Visible = true;
        computerAnalysisResult.Visible = true;

        computerWhite.Visible = true;
        computerBlack.Visible = true;

        if (computerWhite.Checked)
        {
          computerBlack.Checked = false;
          if (!readyok) StartYourEngine(1);
        }
        else if (!computerBlack.Checked)
        {
          computerWhite.Checked = true;
          if (!readyok) StartYourEngine(1);
        }
        else
        {
          if (!readyok2) StartYourEngine(2);
        }

        try
        {
          if (moveTree.SelectedNode == null)
          {
            if ((moveTree.Nodes.Count > 0) &&
                (moveTree.Nodes[0].Nodes.Count > 1))
            {
              moveTree.SelectedNode = moveTree.Nodes[0].Nodes[1];
            }
          }
          else if ((moveTree.SelectedNode.Text.StartsWith("Main") || moveTree.SelectedNode.Text.StartsWith("Puzzle")) && (moveTree.SelectedNode.Nodes.Count > 1))
          {
            //start with the second move
            moveTree.SelectedNode = moveTree.SelectedNode.Nodes[1];
          }
        }
        catch
        {
        }
      }
    }

    //*******************************************************************************************************
    private void EnableMainSelections(string message, string title, bool offerSave)
    {
      play.Enabled = true;
      positionSetup.Enabled = true;
      moveAnalysis.Enabled = true;
      gameAnalysis.Enabled = true;

      computerWhite.Enabled = true;
      computerBlack.Enabled = true;

      if (moveAnalysis.Checked && offerSave)
      {
        MessageBox.Show(message, title);
      }
      else if (offerSave)
      {
        if (DialogResult.Yes == MessageBox.Show(message + "\r\nDo you wish to save this game?", title, MessageBoxButtons.YesNo))
        {
          SaveTheGame();
          SavePGNFile();
          loadPGN.Visible = true;
        }
      }

      if (play.Checked)
      {
        //do not reset the game allow it to be analyzed
        play.Checked = false;
        moveAnalysis.Checked = true;
      }

    }

    //*******************************************************************************************************
    protected void SetupForNextLiveGame()
    {
      DisableEverything();
      oPGN.FileName = "newgame.pgn";
      PreLoadingFileChanges();
      ChangeButton(ref pauseGame, "Pause", "Pause game.", false);
      moveStatus.Text = "Play or load a game to resume or puzzle to play.";
      ChangeButton(ref whiteDraw, "Abort", "Abort Game.", false);
      ChangeButton(ref blackDraw, "Start", "Start game now.", true);
      ChangeButton(ref whiteResign, "Resign", "White resigns game.", false);
      ChangeButton(ref blackResign, "Resign", "Black resigns game.", false);
      loadPGN.Visible = true;

      useClock.Enabled = true;
      useClock.Visible = true;

      //are clocks being used?
      if (useClock.Checked)
      {
        bronsteinTime.Visible = true;
        clockStart.Visible = true;
        whiteClock.Visible = true;
        blackClock.Visible = true;
      }

      computerWhite.Enabled = true;
      computerBlack.Enabled = true;
      computerWhite.Visible = true;
      computerBlack.Visible = true;

      //is a cpu going to play
      if (computerWhite.Checked || computerBlack.Checked)
      {
        computerTime.Text = iniCPUTime;
        clearEngineOutput.Visible = true;

        if (computerWhite.Checked)
        {
          if (!readyok) StartYourEngine(1);
        }

        if (computerBlack.Checked)
        {
          if (!readyok2) StartYourEngine(2);
        }
      }

      play.Enabled = true;
      positionSetup.Enabled = true;
      moveAnalysis.Enabled = true;
      gameAnalysis.Enabled = true;
    }

    #endregion

    #region Engine Stuff

    //*******************************************************************************************************
    private void centiPawn_Paint(object sender, PaintEventArgs e)
    {
      if (!displayReady || moveTree.Nodes[0].Nodes.Count == 0) return;

      // Get the graphics object 
      Graphics gfx = e.Graphics;

      // Create a new pen that we shall use for drawing the lines
      Pen myPen = new Pen(Color.Black);
      Pen myRedPen = new Pen(Color.Red);
      Pen myBluePen = new Pen(Color.Blue);
      Pen myGreenPen = new Pen(Color.Green);

      Font myFont = new Font("Arial", 8);

      int rows = moveTree.Nodes[0].Nodes.Count;
      int xStart = 12;
      int yStart = 4;
      int pixelsPerX = (centiPawn.Width - xStart) / rows;
      if (pixelsPerX == 0) pixelsPerX = 1;
      if (pixelsPerX > 4) pixelsPerX = 4;
      double minValue = 0, maxValue = 0, missingValue = -9999, mateValue = 9998, absMinValue = 2, absMaxValue = 10;
      double[] dataArray;
      bool realData = false;
      string data = "";
      dataArray = new double[rows];

      for (int row = 0; row < rows; row++)
      {
        //see if there are pawn values to graph...
        try
        {
          Move mv = (Move)(moveTree.Nodes[0].Nodes[row].Tag);

          dataArray[row] = missingValue;
          int index1 = mv.comment.IndexOf("(");
          int index2 = mv.comment.IndexOf(")");
          //chess.coms way which I have now adopted
          if ((index1 > 0) && (index2 > 0))
          {
            if (mv.comment.Substring(index1 + 1, index2 - index1 - 1).IndexOf("-Mat") >= 0)
            {
              dataArray[row] = -mateValue;
            }
            else if (mv.comment.Substring(index1 + 1, index2 - index1 - 1).IndexOf("Mat") >= 0)
            {
              dataArray[row] = mateValue;
            }
            else
            {
              data = mv.comment.Substring(index1 + 1, index2 - index1 - 1);
              dataArray[row] = Convert.ToSingle(data);
              realData = true;
              if (dataArray[row] > maxValue)
              {
                maxValue = dataArray[row];
              }
              else if (dataArray[row] < minValue)
              {
                minValue = dataArray[row];
              }
            }
          }
          //my old way - should go away eventually...
          else if (mv.comment.StartsWith("{"))
          {
            if (mv.comment.IndexOf("-Mat") >= 0)
            {
              dataArray[row] = -mateValue;
            }
            else if (mv.comment.IndexOf("Mat") >= 0)
            {
              dataArray[row] = mateValue;
            }
            else
            {
              data = mv.comment.Substring(1, mv.comment.Trim().Length - 2);
              dataArray[row] = Convert.ToSingle(data);
              realData = true;
              if (dataArray[row] > maxValue)
              {
                maxValue = dataArray[row];
              }
              else if (dataArray[row] < minValue)
              {
                minValue = dataArray[row];
              }
            }
          }
        }
        catch
        {
        }
      }

      //create the zero axis...
      if (realData)
      {
        if (maxValue > absMaxValue)
        {
          maxValue = absMaxValue;
        }

        if (minValue < -absMaxValue)
        {
          minValue = -absMaxValue;
        }

        if (maxValue < absMinValue)
        {
          maxValue = absMinValue;
        }

        if (minValue > -absMinValue)
        {
          minValue = -absMinValue;
        }


        int pixelsPerY = Convert.ToInt32((centiPawn.Height - 2 * yStart) / (maxValue - minValue));

        if (pixelsPerY == 0) pixelsPerY = 1;

        int center = yStart + Convert.ToInt32(pixelsPerY * maxValue);
        Rectangle rect = new Rectangle(0, 0, 2, 2);

        gfx.DrawLine(myPen, xStart, center, xStart + pixelsPerX * rows, center);
        gfx.DrawLine(myPen, xStart + pixelsPerX * rows, yStart, xStart + pixelsPerX * rows, centiPawn.Height - yStart);
        gfx.DrawString("0", myFont, Brushes.Black, 0, center - 7);
        gfx.DrawString(maxValue.ToString(), myFont, Brushes.Black, xStart + pixelsPerX * rows + 2, yStart);
        gfx.DrawString(minValue.ToString(), myFont, Brushes.Black, xStart + pixelsPerX * rows + 2, centiPawn.Height - 12 - yStart);

        for (int row = 10; row < rows; row += 10)
        {
          gfx.DrawLine(myPen, xStart + pixelsPerX * row, center + 5, xStart + pixelsPerX * row, center - 5);
        }

        for (int row = 0; row < rows; row++)
        {
          if (dataArray[row] != missingValue)
          {
            rect.X = xStart + row * pixelsPerX;
            rect.Y = yStart + Convert.ToInt32((maxValue - dataArray[row]) * pixelsPerY);

            if (dataArray[row] == mateValue)
            {
              rect.Y = yStart;
              gfx.DrawRectangle(myRedPen, rect);
            }
            else if (dataArray[row] == -mateValue)
            {
              rect.Y = centiPawn.Height - yStart;
              gfx.DrawRectangle(myRedPen, rect);
            }
            else if (dataArray[row] > absMaxValue)
            {
              rect.Y = yStart;
              gfx.DrawRectangle(myBluePen, rect);
            }
            else if (dataArray[row] < -absMaxValue)
            {
              rect.Y = centiPawn.Height - yStart;
              gfx.DrawRectangle(myBluePen, rect);
            }
            else
            {
              gfx.DrawRectangle(myGreenPen, rect);
            }
          }
        }
      }
      else
      {
        gfx.DrawLine(myPen, xStart, centiPawn.Height / 2, xStart + pixelsPerX * rows - 2, centiPawn.Height / 2);
        gfx.DrawLine(myPen, xStart + pixelsPerX * rows, 0, xStart + pixelsPerX * rows, centiPawn.Height);
        gfx.DrawString("0", myFont, Brushes.Black, 0, centiPawn.Height / 2 - 7);
      }
    }

    //*******************************************************************************************************
    protected void ready_Click(object sender, EventArgs e)
    {
      if (ready.Text.Equals("Move"))
      {
        WriteSIOfEngine("stop", 1);
        ResetComputerClock();
      }
      else if (ready.Text.Equals("Stop"))
      {
        WriteSIOfEngine("stop", 1);
        computerWhite.Enabled = true;
        computerBlack.Enabled = true;

        pauseGame.Visible = true;
        ready.Visible = false;
        loadPGN.Visible = true;
        savePGN.Visible = true;

        if (gameAnalysis.Checked)
        {
          runCPUClock = false;
          runClock = false;
        }
      }
      moveTree.Focus();
    }

    //*******************************************************************************************************
    protected void ready2_Click(object sender, EventArgs e)
    {
      if (ready2.Text.Equals("Move"))
      {
        WriteSIOfEngine("stop", 2);
        ResetComputerClock();
      }
      else if (ready2.Text.Equals("Stop"))
      {
        WriteSIOfEngine("stop", 2);
        computerWhite.Enabled = true;
        computerBlack.Enabled = true;

        pauseGame.Visible = true;
        ready2.Visible = false;
        loadPGN.Visible = true;
        savePGN.Visible = true;

        if (gameAnalysis.Checked)
        {
          runCPUClock = false;
          runClock = false;
        }
      }
      moveTree.Focus();
    }

    //*******************************************************************************************************
    protected void analyze_Click(object sender, EventArgs e)
    {
      if ((moveTree.SelectedNode != null) && !moveTree.SelectedNode.Text.StartsWith("RAV") && !moveTree.SelectedNode.Text.StartsWith("Main") && !moveTree.SelectedNode.Text.StartsWith("Puzzle"))
      {
        newPosition = ((Move)(moveTree.SelectedNode.Tag)).priorFen;
        newMove = ((Move)(moveTree.SelectedNode.Tag)).lan;

        findingBestMove = false;

        if (!goOK)
        {
          WriteSIOfEngine("stop", (computerWhite.Checked ? 1 : 2));
        }
        else
        {
          MakeNextAnalysisMove();
          addRAV.Visible = true;
        }
      }
      else
      {
        moveStatus.Text = "You must select a move to analyze...";
      }
      moveTree.Focus();
    }

    //*******************************************************************************************************
    protected void best_Click(object sender, EventArgs e)
    {
      newPosition = lastPosition;
      newMove = "";
      findingBestMove = true;

      if (!goOK)
      {
        WriteSIOfEngine("stop", (computerWhite.Checked ? 1 : 2));
      }
      else
      {
        MakeNextAnalysisMove();
        addRAV.Visible = true;
      }
      moveTree.Focus();
    }

    //*******************************************************************************************************
    protected void optionCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (!optionInitialization)
      {
        CheckBox cbx = (CheckBox)sender;
        if (cbx.Name.StartsWith("white"))
        {
          WriteSIOfEngine("setoption name " + cbx.Text + " value " + (cbx.Checked ? "true" : "false"), 1);
          WhiteNotReady();
        }
        else
        {
          WriteSIOfEngine("setoption name " + cbx.Text + " value " + (cbx.Checked ? "true" : "false"), 2);
          BlackNotReady();
        }
      }
    }

    //*******************************************************************************************************
    protected void optionButton_Click(object sender, EventArgs e)
    {
      if (!optionInitialization)
      {
        Button btn = (Button)sender;
        if (btn.Name.StartsWith("white"))
        {
          WriteSIOfEngine("setoption name " + btn.Text, 1);
          WhiteNotReady();
        }
        else
        {
          WriteSIOfEngine("setoption name " + btn.Text, 2);
          BlackNotReady();
        }
      }
    }

    //*******************************************************************************************************
    protected void optionNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
      if (!optionInitialization)
      {
        NumericUpDown nud = (NumericUpDown)sender;
        if (nud.Name.StartsWith("white"))
        {
          WriteSIOfEngine("setoption name " + nud.AccessibleName + " value " + nud.Value.ToString(), 1);
          WhiteNotReady();
        }
        else
        {
          WriteSIOfEngine("setoption name " + nud.AccessibleName + " value " + nud.Value.ToString(), 2);
          BlackNotReady();
        }
      }
    }

    //*******************************************************************************************************
    protected void optionComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!optionInitialization)
      {
        ComboBox cbx = (ComboBox)sender;

        if (cbx.Name.StartsWith("white"))
        {
          WriteSIOfEngine("setoption name " + cbx.AccessibleName + " value " + cbx.Text, 1);
          WhiteNotReady();
        }
        else
        {
          WriteSIOfEngine("setoption name " + cbx.AccessibleName + " value " + cbx.Text, 2);
          BlackNotReady();
        }
      }
    }

    //*******************************************************************************************************
    protected void optionTextBox_TextChanged(object sender, EventArgs e)
    {
      if (!optionInitialization)
      {
        TextBox txt = (TextBox)sender;

        //put the test info in the button and enable the button...
        string btnName = txt.Name.Replace("TextBox", "TXT");

        if (txt.Name.StartsWith("white"))
        {
          if (!whiteOptions.Controls[btnName].Enabled)
          {
            //only do this the first time through
            whiteOptions.Controls[btnName].Enabled = true;
            whiteOptions.Controls[btnName].Text = "Update : " + whiteOptions.Controls[btnName].Text;
          }
          whiteOptions.Controls[btnName].AccessibleName = txt.Text;
        }
        else
        {
          if (!blackOptions.Controls[btnName].Enabled)
          {
            //only do this the first time through
            blackOptions.Controls[btnName].Enabled = true;
            blackOptions.Controls[btnName].Text = "Update : " + blackOptions.Controls[btnName].Text;
          }
          blackOptions.Controls[btnName].AccessibleName = txt.Text;
        }
      }
    }

    //*******************************************************************************************************
    protected void optionTextUpdate_Click(object sender, EventArgs e)
    {
      if (!optionInitialization)
      {
        Button btn = (Button)sender;

        btn.Text = btn.Text.Replace("Update : ", "");

        if (btn.Name.StartsWith("white"))
        {
          WriteSIOfEngine("setoption name " + btn.Text + " value " + btn.AccessibleName, 1);
          WhiteNotReady();
        }
        else
        {
          WriteSIOfEngine("setoption name " + btn.Text + " value " + btn.AccessibleName, 2);
          BlackNotReady();
        }
        btn.Enabled = false;
      }
    }

    //*******************************************************************************************************
    protected void WhiteNotReady()
    {
      readyok = false;
      engines.Controls["whiteEngine"].Text = engines.Controls["whiteEngine"].Text.Replace(" - Ready", "");
      WriteSIOfEngine("isready", 1);
    }

    //*******************************************************************************************************
    protected void BlackNotReady()
    {
      readyok2 = false;
      engines.Controls["blackEngine"].Text = engines.Controls["blackEngine"].Text.Replace(" - Ready", "");
      WriteSIOfEngine("isready", 2);
    }

    //*******************************************************************************************************
    private void ShowOptions(ref ComboBox list, string color, ref TabPage tab)
    {
      optionInitialization = true;

      int checkCount = 0;
      int spinCount = 0;
      int comboCount = 0;
      int buttonCount = 0;
      int stringCount = 0;

      int maxCheckLength = 0;
      int maxSpinLength = 0;
      int maxComboLength = 0;
      int maxButtonLength = 0;
      int maxStringLength = 0;

      int deltaY = whiteCheckBox2.Location.Y - whiteCheckBox1.Location.Y;
      int index1, index2;
      int referenceX = whiteCheckBox1.Location.X;

      string item, nextName, nextNameLabel;
      bool found;

      Point refCheckBox1 = new Point(0, 0);
      Point refButton1 = new Point(0, 0);
      Point refNUD1 = new Point(0, 0), reflblNUD1 = new Point(0, 0);
      Point refComboBox1 = new Point(0, 0), reflblComboBox1 = new Point(0, 0);
      Point refTextBox1 = new Point(0, 0), reflblTextBox1 = new Point(0, 0);

      CheckBox cbx;
      Button btn;
      NumericUpDown nud;
      Label lbl_nud;
      ComboBox combo;
      Label lbl_combo;
      TextBox txt;
      Button lbl_txt;

      for (int i = 0; i < list.Items.Count; i++)
      {
        item = list.Items[i].ToString();
        //*******************************************************************************************************
        if (item.IndexOf("type check") > 0)
        {
          checkCount++;
          cbx = new CheckBox();
          found = false;
          nextName = color + "CheckBox" + checkCount.ToString();

          for (int c = 0; c < tab.Controls.Count; c++)
          {
            if (tab.Controls[c].Name.Equals(nextName))
            {
              cbx = (CheckBox)tab.Controls[c];
              found = true;
              break;
            }
          }

          if (!found)
          {
            cbx.Name = nextName;
            cbx.Location = new Point(referenceX, refCheckBox1.Y + (checkCount - 1) * deltaY);
            cbx.AutoSize = true;
            cbx.CheckedChanged += new System.EventHandler(this.optionCheckBox_CheckedChanged);
            tab.Controls.Add(cbx);
          }

          cbx.Visible = true;
          cbx.Text = item.Substring(0, item.IndexOf("type check") - 1);
          cbx.Checked = (item.IndexOf("default true") > 0);

          if (maxCheckLength < cbx.Size.Width)
          {
            maxCheckLength = cbx.Size.Width;
          }

          if (checkCount == 1) refCheckBox1 = cbx.Location;
        }
        //*******************************************************************************************************
        else if (item.IndexOf("type button") > 0)
        {
          buttonCount++;
          btn = new Button();
          found = false;
          nextName = color + "Button" + buttonCount.ToString();

          for (int c = 0; c < tab.Controls.Count; c++)
          {
            if (tab.Controls[c].Name.Equals(nextName))
            {
              btn = (Button)tab.Controls[c];
              found = true;
              break;
            }
          }

          if (!found)
          {
            btn.Name = nextName;
            btn.Location = new Point(refButton1.X, refButton1.Y + (buttonCount - 1) * deltaY);
            btn.AutoSize = true;
            btn.Click += new System.EventHandler(this.optionButton_Click);
            tab.Controls.Add(btn);
          }

          btn.Visible = true;
          btn.Text = item.Substring(0, item.IndexOf("type button") - 1);

          if (maxButtonLength < btn.Size.Width)
          {
            maxButtonLength = btn.Size.Width;
          }

          if (buttonCount == 1) refButton1 = btn.Location;
        }
        //*******************************************************************************************************
        else if (item.IndexOf("type spin") > 0)
        {
          spinCount++;
          nud = new NumericUpDown();
          lbl_nud = new Label();

          found = false;
          nextName = color + "NumericUpDown" + spinCount.ToString();
          nextNameLabel = color + "Spin" + spinCount.ToString();

          for (int c = 0; c < tab.Controls.Count; c++)
          {
            if (tab.Controls[c].Name.Equals(nextName))
            {
              nud = (NumericUpDown)tab.Controls[c];
              lbl_nud = (Label)tab.Controls[nextNameLabel];
              found = true;
              break;
            }
          }

          if (!found)
          {
            nud.Name = nextName;
            nud.Location = new Point(refNUD1.X, refNUD1.Y + (spinCount - 1) * deltaY);
            nud.Size = new Size(whiteNumericUpDown1.Width, whiteNumericUpDown1.Height);
            nud.ValueChanged += new System.EventHandler(this.optionNumericUpDown_ValueChanged);
            tab.Controls.Add(nud);

            lbl_nud.Name = nextNameLabel;
            lbl_nud.Location = new Point(reflblNUD1.X, reflblNUD1.Y + (spinCount - 1) * deltaY);
            lbl_nud.AutoSize = true;
            tab.Controls.Add(lbl_nud);
          }

          nud.Visible = true;
          index1 = item.IndexOf("max ") + 4;
          index2 = item.IndexOf(" ", index1);
          if (index2 > 0) nud.Maximum = Convert.ToDecimal(item.Substring(index1, index2 - index1));
          else nud.Maximum = Convert.ToDecimal(item.Substring(index1));
          index1 = item.IndexOf("min ") + 4;
          index2 = item.IndexOf(" ", index1);
          if (index2 > 0) nud.Minimum = Convert.ToDecimal(item.Substring(index1, index2 - index1));
          else nud.Minimum = Convert.ToDecimal(item.Substring(index1));
          index1 = item.IndexOf("default ") + 8;
          index2 = item.IndexOf(" ", index1);
          if (index2 > 0) nud.Value = Convert.ToDecimal(item.Substring(index1, index2 - index1));
          else nud.Value = Convert.ToDecimal(item.Substring(index1));
          nud.AccessibleName = item.Substring(0, item.IndexOf("type spin") - 1);

          lbl_nud.Visible = true;
          lbl_nud.Text = item.Substring(0, item.IndexOf("type spin") - 1);

          if (maxSpinLength < lbl_nud.Size.Width)
          {
            maxSpinLength = lbl_nud.Size.Width;
          }

          if (spinCount == 1)
          {
            refNUD1 = nud.Location;
            reflblNUD1 = lbl_nud.Location;
          }
        }
        //*******************************************************************************************************
        else if (item.IndexOf("type combo") > 0)
        {
          comboCount++;
          combo = new ComboBox();
          lbl_combo = new Label();

          found = false;
          nextName = color + "ComboBox" + comboCount.ToString();
          nextNameLabel = color + "CBX" + comboCount.ToString();

          for (int c = 0; c < tab.Controls.Count; c++)
          {
            if (tab.Controls[c].Name.Equals(nextName))
            {
              combo = (ComboBox)tab.Controls[c];
              lbl_combo = (Label)tab.Controls[nextNameLabel];
              found = true;
              break;
            }
          }

          if (!found)
          {
            combo.Name = nextName;
            combo.Location = new Point(refComboBox1.X, refComboBox1.Y + (comboCount - 1) * deltaY);
            //combo.AutoSize = true;
            combo.Size = new Size(whiteComboBox1.Width, whiteComboBox1.Height);
            combo.SelectedIndexChanged += new System.EventHandler(this.optionComboBox_SelectedIndexChanged);
            tab.Controls.Add(combo);
            lbl_combo.Name = nextNameLabel;
            lbl_combo.Location = new Point(reflblComboBox1.X, reflblComboBox1.Y + (comboCount - 1) * deltaY);
            lbl_combo.AutoSize = true;
            tab.Controls.Add(lbl_combo);
          }

          combo.Visible = true;
          combo.Items.Clear();
          combo.Text = "";
          index2 = 1;
          while (index2 > 0)
          {
            index1 = item.IndexOf("var ", index2 + 1) + 4;
            if (index1 > 5)
            {
              index2 = item.IndexOf("var ", index1) - 1;
              if (index2 > 0) combo.Items.Add(item.Substring(index1, index2 - index1));
              else combo.Items.Add(item.Substring(index1));
            }
            else break;
          }
          index1 = item.IndexOf("default ") + 8;
          if (index1 > 7)
          {
            index2 = item.IndexOf("var ", index1) - 1;//this will only work if the default value always shows up in the list befare the var's
            if (index2 > 0) combo.Text = item.Substring(index1, index2 - index1);
            else combo.Text = item.Substring(index1);
          }
          combo.AccessibleName = item.Substring(0, item.IndexOf("type combo") - 1);

          lbl_combo.Visible = true;
          lbl_combo.Text = item.Substring(0, item.IndexOf("type combo") - 1);

          if (maxComboLength < lbl_combo.Size.Width)
          {
            maxComboLength = lbl_combo.Size.Width;
          }

          if (comboCount == 1)
          {
            refComboBox1 = combo.Location;
            reflblComboBox1 = lbl_combo.Location;
          }
        }
        //*******************************************************************************************************
        else if (item.IndexOf("type string") > 0)
        {
          stringCount++;
          txt = new TextBox();
          lbl_txt = new Button();

          found = false;
          nextName = color + "TextBox" + stringCount.ToString();
          nextNameLabel = color + "TXT" + stringCount.ToString();

          for (int c = 0; c < tab.Controls.Count; c++)
          {
            if (tab.Controls[c].Name.Equals(nextName))
            {
              txt = (TextBox)tab.Controls[c];
              lbl_txt = (Button)tab.Controls[nextNameLabel];
              found = true;
              break;
            }
          }

          if (!found)
          {
            txt.Name = nextName;
            txt.Location = new Point(refTextBox1.X, refTextBox1.Y + (stringCount - 1) * deltaY);
            txt.Size = new Size(whiteTextBox1.Width, whiteTextBox1.Height);
            txt.TextChanged += new System.EventHandler(this.optionTextBox_TextChanged);
            tab.Controls.Add(txt);

            lbl_txt.Name = nextNameLabel;
            lbl_txt.Location = new Point(reflblTextBox1.X, reflblTextBox1.Y + (stringCount - 1) * deltaY);
            lbl_txt.AutoSize = true;
            lbl_txt.Enabled = false;
            lbl_txt.Click += new System.EventHandler(this.optionTextUpdate_Click);
            tab.Controls.Add(lbl_txt);
          }

          txt.Visible = true;
          index1 = item.IndexOf("default ") + 8;
          index2 = item.IndexOf(" ", index1);
          if (index2 > 0) txt.Text = item.Substring(index1, index2 - index1);
          else txt.Text = item.Substring(index1);
          txt.AccessibleName = item.Substring(0, item.IndexOf("type string") - 1);
          txt.AccessibleDescription = "";

          lbl_txt.BackColor = Color.LightGray;// (color.Equals("white") ? whiteBK : blackBK);
          lbl_txt.Visible = true;
          lbl_txt.Text = item.Substring(0, item.IndexOf("type string") - 1);

          if (maxStringLength < lbl_txt.Size.Width)
          {
            maxStringLength = lbl_txt.Size.Width;
          }

          if (stringCount == 1)
          {
            refTextBox1 = txt.Location;
            reflblTextBox1 = lbl_txt.Location;
          }
        }
        //else //unimplemented type...
        //{
        //}
      }

      //adjust button x locations
      int shiftValue = referenceX + maxCheckLength + 5 - refButton1.X;

      if (shiftValue != 0)
      {
        refButton1.X += shiftValue;
        nextName = color + "Button";
        buttonCount = 0;

        //shift all the buttons over
        for (int c = 0; c < tab.Controls.Count; c++)
        {
          if (tab.Controls[c].Name.StartsWith(nextName))
          {
            buttonCount = Convert.ToInt32(tab.Controls[c].Name.Replace(nextName, ""));
            tab.Controls[c].Location = new Point(refButton1.X, whiteButton1.Location.Y + (buttonCount - 1) * deltaY);
          }
        }
      }

      //adjust spin x locations
      shiftValue = refButton1.X + maxButtonLength + 5 - refNUD1.X;

      if (shiftValue != 0)
      {
        refNUD1.X += shiftValue;
        //reflblNUD1.X += shiftValue;
        reflblNUD1.X = refNUD1.X + whiteNumericUpDown1.Width + 5;
        nextName = color + "NumericUpDown";
        nextNameLabel = color + "Spin";

        //shift all the spin stuff over
        for (int c = 0; c < tab.Controls.Count; c++)
        {
          if (tab.Controls[c].Name.StartsWith(nextName))
          {
            spinCount = Convert.ToInt32(tab.Controls[c].Name.Replace(nextName, ""));
            tab.Controls[c].Location = new Point(refNUD1.X, whiteButton1.Location.Y + (spinCount - 1) * deltaY);
          }
        }

        for (int c = 0; c < tab.Controls.Count; c++)
        {
          if (tab.Controls[c].Name.StartsWith(nextNameLabel))
          {
            spinCount = Convert.ToInt32(tab.Controls[c].Name.Replace(nextNameLabel, ""));
            tab.Controls[c].Location = new Point(reflblNUD1.X, whiteButton1.Location.Y + (spinCount - 1) * deltaY);
          }
        }
      }

      //adjust combobox x locations
      shiftValue = (maxSpinLength == 0) ? (refNUD1.X + 5 - refComboBox1.X) : (reflblNUD1.X + maxSpinLength + 5 - refComboBox1.X);

      if (shiftValue != 0)
      {
        refComboBox1.X += shiftValue;
        reflblComboBox1.X = refComboBox1.X + whiteComboBox1.Width + 5;
        nextName = color + "ComboBox";
        nextNameLabel = color + "CBX";

        //shift all the spin stuff over
        for (int c = 0; c < tab.Controls.Count; c++)
        {
          if (tab.Controls[c].Name.StartsWith(nextName))
          {
            comboCount = Convert.ToInt32(tab.Controls[c].Name.Replace(nextName, ""));
            tab.Controls[c].Location = new Point(refComboBox1.X, whiteButton1.Location.Y + (comboCount - 1) * deltaY);
          }
        }

        for (int c = 0; c < tab.Controls.Count; c++)
        {
          if (tab.Controls[c].Name.StartsWith(nextNameLabel))
          {
            comboCount = Convert.ToInt32(tab.Controls[c].Name.Replace(nextNameLabel, ""));
            tab.Controls[c].Location = new Point(reflblComboBox1.X, whiteButton1.Location.Y + (comboCount - 1) * deltaY);
          }
        }
      }

      //adjust textbox x locations
      shiftValue = (maxComboLength == 0) ? (refComboBox1.X + 5 - refTextBox1.X) : (reflblComboBox1.X + maxComboLength + 5 - refTextBox1.X);

      if (shiftValue != 0)
      {
        refTextBox1.X += shiftValue;
        reflblTextBox1.X = refTextBox1.X + whiteTextBox1.Width + 5;
        nextName = color + "TextBox";
        nextNameLabel = color + "TXT";

        //shift all the spin stuff over
        for (int c = 0; c < tab.Controls.Count; c++)
        {
          if (tab.Controls[c].Name.StartsWith(nextName))
          {
            stringCount = Convert.ToInt32(tab.Controls[c].Name.Replace(nextName, ""));
            tab.Controls[c].Location = new Point(refTextBox1.X, whiteButton1.Location.Y + (stringCount - 1) * deltaY);
          }
        }

        for (int c = 0; c < tab.Controls.Count; c++)
        {
          if (tab.Controls[c].Name.StartsWith(nextNameLabel))
          {
            stringCount = Convert.ToInt32(tab.Controls[c].Name.Replace(nextNameLabel, ""));
            tab.Controls[c].Location = new Point(reflblTextBox1.X, whiteButton1.Location.Y + (stringCount - 1) * deltaY);
          }
        }
      }

      optionInitialization = false;
    }

    //*******************************************************************************************************
    private void ResetOptions(ref TabPage tab)
    {
      optionInitialization = true;

      for (int c = 0; c < tab.Controls.Count; c++)
      {
        if (tab.Controls[c].Name.StartsWith("engineOptionList"))
        {
          ComboBox cb = (ComboBox)tab.Controls[c];
          cb.Items.Clear();
          cb.Text = "";
        }
        else if (!tab.Controls[c].Name.StartsWith("engineLoaded") &&
                !tab.Controls[c].Name.EndsWith("Update"))
        {
          tab.Controls[c].Visible = false;
        }
      }
    }

    //*******************************************************************************************************
    protected void changeEngineToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ChangeWhiteEngine(true);
    }

    //*******************************************************************************************************
    protected void changeEngine2ToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ChangeBlackEngine(true);
    }

    //*******************************************************************************************************
    private void swapEnginesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (goOK)
      {
        string tmp = enginePath;

        enginePath = enginePath2;
        chess.PutINIValue("SETTINGS", "ENGINE_PATH", enginePath);

        //stop the current engine if running
        StopYourEngine(1);
        StartYourEngine(1);
                    
        enginePath2 = tmp;
        chess.PutINIValue("SETTINGS", "ENGINE_PATH2", enginePath2);

        //stop the current engine if running
        StopYourEngine(2);
        StartYourEngine(2);
      }
      else
      {
        MessageBox.Show("Engine should not be active when making this change...");
      }
    }

    //*******************************************************************************************************
    private void ChangeWhiteEngine(bool menu)
    {
        if (goOK)
        {
            oPGN.Filter = "EXE files (*.exe)|*.exe";
            oPGN.InitialDirectory = Path.GetFullPath(enginePath).Replace(Path.GetFileName(enginePath), "");

            if (oPGN.ShowDialog() == DialogResult.OK)
            {
                enginePath = oPGN.FileName;
                chess.PutINIValue("SETTINGS", "ENGINE_PATH", enginePath);

                if (menu)
                {
                    //stop the current engine if running
                    StopYourEngine(1);
                    StartYourEngine(1);
                }
            }
        }
        else
        {
            MessageBox.Show("Engine should not be active when making this change...");
        }
        
    }

    //*******************************************************************************************************
    private void ChangeBlackEngine(bool menu)
    {
        if (goOK)
        {
            oPGN.Filter = "EXE files (*.exe)|*.exe";
            oPGN.InitialDirectory = Path.GetFullPath(enginePath2).Replace(Path.GetFileName(enginePath2), "");

            if (oPGN.ShowDialog() == DialogResult.OK)
            {
                enginePath2 = oPGN.FileName;

                chess.PutINIValue("SETTINGS", "ENGINE_PATH2", enginePath2);

                if (menu)
                {
                    //stop the current engine if running
                    StopYourEngine(2);
                    StartYourEngine(2);
                }
            }
        }
        else
        {
            MessageBox.Show("Engine should not be active when making this change...");
        }
        
    }

    //*******************************************************************************************************
    protected void computerWhite_CheckedChanged(object sender, EventArgs e)
    {
      if (computerWhite.Checked)
      {
        if (moveAnalysis.Checked || gameAnalysis.Checked)
        {
          computerBlack.Checked = false;
        }
        else if (play.Checked)
        {
          clearEngineOutput.Visible = true;
          computerTime.Visible = true;
          computerTimeLabel.Visible = true;
        }

        StartYourEngine(1);
      }
      else
      {
          StopYourEngine(1);
        if (moveAnalysis.Checked || gameAnalysis.Checked)
        {
          computerBlack.Checked = true;
        }
        else if (play.Checked)
        {
          clearEngineOutput.Visible = computerBlack.Checked;
          computerTime.Visible = computerBlack.Checked;
          computerTimeLabel.Visible = computerBlack.Checked;
          computerClock.Visible = computerBlack.Checked;
        }
      }
    }

    //*******************************************************************************************************
    protected void computerBlack_CheckedChanged(object sender, EventArgs e)
    {
      if (computerBlack.Checked)
      {
        if (moveAnalysis.Checked || gameAnalysis.Checked)
        {
          computerWhite.Checked = false;
        }
        else if (play.Checked)
        {
          clearEngineOutput.Visible = true;
          computerTime.Visible = true;
          computerTimeLabel.Visible = true;
        }

        StartYourEngine(2);
      }
      else
      {
          StopYourEngine(2);
        if (moveAnalysis.Checked || gameAnalysis.Checked)
        {
          computerWhite.Checked = true;
        }
        else if (play.Checked)
        {
          clearEngineOutput.Visible = computerWhite.Checked;
          computerTime.Visible = computerWhite.Checked;
          computerTimeLabel.Visible = computerWhite.Checked;
          computerClock.Visible = computerWhite.Checked;
        }
      }
    }

    //*******************************************************************************************************
    protected void deepTime_TextChanged(object sender, EventArgs e)
    {
      double testValue;

      try
      {
        testValue = Convert.ToDouble(deepTime.Text);
        iniDeepTime = deepTime.Text;
        chess.PutINIValue("SETTINGS", "DEEP_TIME_HOURS", deepTime.Text);
        UpdateAnalysisTime();
      }
      catch
      {
        //do not save if invalid
      }
    }

    //*******************************************************************************************************
    protected void UpdateAnalysisTime()
    {
      //this rows to process value is for the processing of the current level only
      int rowsToProcess = moveTree.SelectedNode.Parent.Nodes.Count;
      //assuming 2 analysis per move ~75% of the time - best move and then original move
      //~25% of the time only the best move will be calculated - if it matches actual no additional analysis is needed
      int addPositionsToProcess = 0;
      //additional analysis requested...
      if (evaluateRAV.Checked)
      {
        addPositionsToProcess = (int)(2 * 0.25 * rowsToProcess + 0.5);
      }

      int totalMovesToProcess = (int)(0.75 * rowsToProcess + 2 * 0.25 * rowsToProcess + 0.5) + addPositionsToProcess;
      int timePerAnalysis = (int)(Convert.ToDouble(deepTime.Text) * 60 * 60 / totalMovesToProcess + 0.5);

      computerTime.Text = timePerAnalysis.ToString();
      computerTime.Visible = true;
      computerTimeLabel.Visible = true;
      computerClock.Visible = true;
      ResetComputerClock();
    }

    //*******************************************************************************************************
    protected void veryPoorMove_TextChanged(object sender, EventArgs e)
    {
      double testValue;

      try
      {
        testValue = Convert.ToDouble(veryPoorMove.Text);
        iniVeryPoor = veryPoorMove.Text;
        chess.PutINIValue("SETTINGS", "VERY_POOR_PAWNS", veryPoorMove.Text);
      }
      catch
      {
        //do not save if invalid
      }
    }

    //*******************************************************************************************************
    protected void poorMove_TextChanged(object sender, EventArgs e)
    {
      double testValue;

      try
      {
        testValue = Convert.ToDouble(poorMove.Text);
        iniVeryPoor = poorMove.Text;
        chess.PutINIValue("SETTINGS", "POOR_PAWNS", poorMove.Text);
      }
      catch
      {
        //do not save if invalid
      }
    }

    //*******************************************************************************************************
    protected void questionableMove_TextChanged(object sender, EventArgs e)
    {
      double testValue;

      try
      {
        testValue = Convert.ToDouble(questionableMove.Text);
        iniQuestionable = questionableMove.Text;
        chess.PutINIValue("SETTINGS", "QUESTIONABLE_PAWNS", questionableMove.Text);
      }
      catch
      {
        //do not save if invalid
      }
    }

    //*******************************************************************************************************
    protected void computerTime_TextChanged(object sender, EventArgs e)
    {
      if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
      {
        int testValue;

        try
        {
          testValue = Convert.ToInt32(computerTime.Text);
          iniCPUTime = computerTime.Text;
          chess.PutINIValue("SETTINGS", "CPU_TIME_SECONDS", computerTime.Text);
        }
        catch
        {
          //do not save if invalid
        }
      }
    }

    //*******************************************************************************************************
    private bool StartYourEngine(int engNum)
    {
      if (engNum == 1)
      {
        uciok = false;
        readyok = false;

        try
        {
            if (engine != null)
            {
                engine.CancelOutputRead();
                engine.CancelErrorRead();
                engine.Kill();
            }
        }
        catch
        {
        }
        engine = null;

        ResetOptions(ref whiteOptions);

        while (!File.Exists(enginePath))
        {
            DialogResult dr = MessageBox.Show("A valid white engine executable path does not exist", "Invalid Engine Resource", MessageBoxButtons.RetryCancel);

            if (dr == DialogResult.Cancel)
            {
                return false;
            }

            ChangeWhiteEngine(false);
        }

        engineLoaded.Text = "";

        // Setup the process with the ProcessStartInfo class.
        engineSI = new ProcessStartInfo();
        engineSI.FileName = enginePath; // Specify exe name.
        engineSI.UseShellExecute = false;
        engineSI.RedirectStandardOutput = true;
        engineSI.RedirectStandardInput = true;
        engineSI.RedirectStandardError = true;
        engineSI.CreateNoWindow = true;

        // Start the process.
        engine = Process.Start(engineSI);
        engine.OutputDataReceived += new DataReceivedEventHandler(EngineOutputHandler);
        engine.ErrorDataReceived += new DataReceivedEventHandler(EngineErrorHandler);

        engine.BeginOutputReadLine();
        engine.BeginErrorReadLine();

        engineActive = true;
        WriteSIOfEngine("uci", 1);
      }
      else //if (engNum == 2)
      {
        uciok2 = false;
        readyok2 = false;

        try
        {
            if (engine2 != null)
            {
                engine2.CancelOutputRead();
                engine2.CancelErrorRead();
                engine2.Kill();
            }
        }
        catch
        {
        }
        engine2 = null;

        ResetOptions(ref blackOptions);

        while (!File.Exists(enginePath2))
        {
            DialogResult dr = MessageBox.Show("A valid black engine executable path does not exist", "Invalid Engine Resource", MessageBoxButtons.RetryCancel);

            if (dr == DialogResult.Cancel)
            {
                return false;
            }

            ChangeBlackEngine(false);
        }

        engineLoaded2.Text = "";

        // Setup the process with the ProcessStartInfo class.
        engineSI2 = new ProcessStartInfo();
        engineSI2.FileName = enginePath2; // Specify exe name.
        engineSI2.UseShellExecute = false;
        engineSI2.RedirectStandardOutput = true;
        engineSI2.RedirectStandardInput = true;
        engineSI2.RedirectStandardError = true;
        engineSI2.CreateNoWindow = true;

        // Start the process.
        engine2 = Process.Start(engineSI2);
        engine2.OutputDataReceived += new DataReceivedEventHandler(EngineOutputHandler2);
        engine2.ErrorDataReceived += new DataReceivedEventHandler(EngineErrorHandler2);

        engine2.BeginOutputReadLine();
        engine2.BeginErrorReadLine();

        engineActive2 = true;
        WriteSIOfEngine("uci", 2);
      }
      return true;
    }

    //*******************************************************************************************************
    private void StopYourEngine(int engNum)
    {
        if (engNum == 1)
        {
            uciok = false;
            readyok = false;

            ResetOptions(ref whiteOptions);

            // Kill the process.
            WriteSIOfEngine("quit", 1);
            engineActive = false;
            ready.Visible = engineLoaded.Visible = engineOptionList.Visible = engineCommunication.Visible = false;
            engines.Controls["whiteEngine"].Text = "White Engine";
            engines.Controls["whiteOptions"].Text = "White Engine Options";

            try
            {
                if (engine != null)
                {
                    engine.CancelOutputRead();
                    engine.CancelErrorRead();
                    engine.Kill();
                }
            }
            catch
            {
            }
            engine = null;
        }
        else //if (engNum == 2)
        {
            uciok2 = false;
            readyok2 = false;

            ResetOptions(ref blackOptions);

            // Kill the process.
            WriteSIOfEngine("quit", 2);
            engineActive2 = false;
            ready2.Visible = engineLoaded2.Visible = engineOptionList2.Visible = engineCommunication2.Visible = false;
            engines.Controls["blackEngine"].Text = "Black Engine";
            engines.Controls["blackOptions"].Text = "Black Engine Options";

            try
            {
                if (engine2 != null)
                {
                    engine2.CancelOutputRead();
                    engine2.CancelErrorRead();
                    engine2.Kill();
                }
            }
            catch
            {
            }
            engine2 = null;
        }
    }

    //*******************************************************************************************************
    private void EngineErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (outLine.Data != null)
      {
        this.Invoke(new Action(delegate() { engineCommunication.Text += (outLine.Data + Environment.NewLine); }));
      }
    }

    //*******************************************************************************************************
    private void EngineErrorHandler2(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (outLine.Data != null)
      {
        this.Invoke(new Action(delegate() { engineCommunication2.Text += (outLine.Data + Environment.NewLine); }));
      }
    }

    //*******************************************************************************************************
    private void EngineOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (outLine.Data != null)
      {
        string thisMessage = outLine.Data;

        if (thisMessage.StartsWith("info "))
        {
          thisMessage = thisMessage.Substring(5);
          thisMessage = thisMessage.Replace("multipv", "mpv");
          thisMessage = thisMessage.Replace("seldepth", "sdp");
          thisMessage = thisMessage.Replace("depth", "dp");
          thisMessage = thisMessage.Replace("lowerbound", "lb");
          thisMessage = thisMessage.Replace("upperbound", "ub");
          thisMessage = thisMessage.Replace("hashfull", "hf");
          thisMessage = thisMessage.Replace("tbhits", "th");
          thisMessage = thisMessage.Replace("nodes", "n");

          string score = "", pv = "", m = "";
          string[] fields = new string[255];

          bool cpNext = false, cpDone = false, mDone = false;
          bool pvNext = false, pvDone = false, mNext = false;

          if (thisMessage.IndexOf(" pv ") > 0)
          {
            lastRAV = thisMessage.Substring(thisMessage.IndexOf(" pv ") + 4);
          }

          fields = thisMessage.Split(spaceDelim, 255);
          foreach (string s in fields)
          {
            if (cpNext)
            {
              if (s.Length > 0)
              {
                score = s;
                cpDone = true;
                cpNext = false;
              }
            }
            else if (mNext)
            {
              if (s.Length > 0)
              {
                m = s;
                mDone = true;
                mNext = false;
              }
            }
            else if (pvNext)
            {
              if (s.Length > 0)
              {
                pv = s;
                //pv = s.Replace("x", "");
                pvDone = true;
                pvNext = false;
              }
            }
            else if (!cpDone && s.Equals("cp"))
            {
              cpNext = true;
            }
            else if (!mDone && s.Equals("mate"))
            {
              mNext = true;
            }
            else if (!pvDone && s.Equals("pv"))
            {
              pvNext = true;
            }

            if (cpDone && pvDone)
            {
              this.Invoke(new Action(delegate() { computerAnalysisResult.Text = (Convert.ToDouble(score) / (whitesMove ? -100 : 100)).ToString(); }));
              this.Invoke(new Action(delegate() { bestMove.Text = pv; }));
              break;
            }
            else if (mDone && pvDone)
            {
              int mateInValue = Convert.ToInt32(m) * (whitesMove ? -1 : 1);
              if (mateInValue < 0)
              {
                this.Invoke(new Action(delegate() { computerAnalysisResult.Text = ("-Mate " + (-mateInValue).ToString()); }));
              }
              else
              {
                this.Invoke(new Action(delegate() { computerAnalysisResult.Text = ("Mate " + (mateInValue).ToString()); }));
              }
              this.Invoke(new Action(delegate() { bestMove.Text = pv; }));
              break;
            }
          }

          thisMessage = thisMessage.Replace("score", "s");
          this.Invoke(new Action(delegate() { engineCommunication.AppendText(Environment.NewLine + thisMessage); }));
        }
        else if (thisMessage.StartsWith("option name "))
        {
          this.Invoke(new Action(delegate() { engineOptionList.Items.Add(thisMessage.Substring(12).Trim()); engineOptionList.SelectedIndex = engineOptionList.Items.Count - 1; }));
        }
        else if (thisMessage.StartsWith("bestmove NULL"))
        {
          this.Invoke(new Action(delegate() { engineCommunication.AppendText(Environment.NewLine + thisMessage); }));
          goOK = true;
        }
        else if (thisMessage.StartsWith("bestmove "))
        {
          this.Invoke(new Action(delegate() { engineCommunication.AppendText(Environment.NewLine + thisMessage); }));

          try
          {
            thisMessage = thisMessage.Substring(9).Trim();
            int length = thisMessage.IndexOf(" ", 1);
            if (length > 0)
            {
              thisMessage = thisMessage.Substring(0, length);
            }
          }
          catch (Exception e)
          {
            MessageBox.Show(e.ToString());
          }

          this.Invoke(new Action(delegate() { RespondToBestMove(thisMessage); }));
        }
        else if (thisMessage.IndexOf("id name ") >= 0)
        {
//          this.Invoke(new Action(delegate() { engines.Controls["whiteEngine"].Text = thisMessage.Substring(8).Trim(); }));
//          this.Invoke(new Action(delegate() { engines.Controls["whiteOptions"].Text = (thisMessage.Substring(8).Trim() + " Options"); }));
            this.Invoke(new Action(delegate() { engines.Controls["whiteEngine"].Text = thisMessage.Substring(8 + thisMessage.IndexOf("id name ")).Trim(); }));
            this.Invoke(new Action(delegate() { engines.Controls["whiteOptions"].Text = (thisMessage.Substring(8 + thisMessage.IndexOf("id name ")).Trim() + " Options"); }));
        }
        else if (thisMessage.StartsWith("id author "))
        {
          this.Invoke(new Action(delegate() { engineLoaded.Text = "Author(s) : " + thisMessage.Substring(10).Trim(); }));
        }
        else if (thisMessage.StartsWith("uciok"))
        {
          if (!uciok)
          {
            uciok = true;
            this.Invoke(new Action(delegate() { RespondToUCIOK(); }));
          }
        }
        else if (thisMessage.StartsWith("readyok"))
        {
          this.Invoke(new Action(delegate()
          {
            readyok = true;
            engines.Controls["whiteEngine"].Text = engines.Controls["whiteEngine"].Text + " - Ready";
            TurnGameOptionsOn(1);
          }));
        }
        else if (thisMessage.Length == 0)
        {
        }
        else
        {
          //unknown message
          this.Invoke(new Action(delegate() { engineCommunication.Text += (Environment.NewLine + "??? " + thisMessage + " ???" + Environment.NewLine); }));
        }
      }
    }

    //*******************************************************************************************************
    private void EngineOutputHandler2(object sendingProcess, DataReceivedEventArgs outLine)
    {
      if (outLine.Data != null)
      {
        string thisMessage = outLine.Data;

        if (thisMessage.StartsWith("info "))
        {
          thisMessage = thisMessage.Substring(5);
          thisMessage = thisMessage.Replace("multipv", "mpv");
          thisMessage = thisMessage.Replace("seldepth", "sdp");
          thisMessage = thisMessage.Replace("depth", "dp");
          thisMessage = thisMessage.Replace("lowerbound", "lb");
          thisMessage = thisMessage.Replace("upperbound", "ub");
          thisMessage = thisMessage.Replace("hashfull", "hf");
          thisMessage = thisMessage.Replace("tbhits", "th");
          thisMessage = thisMessage.Replace("nodes", "n");

          string score = "", pv = "", m = "";
          string[] fields = new string[255];

          bool cpNext = false, cpDone = false, mDone = false;
          bool pvNext = false, pvDone = false, mNext = false;

          if (thisMessage.IndexOf(" pv ") > 0)
          {
            lastRAV = thisMessage.Substring(thisMessage.IndexOf(" pv ") + 4);
          }

          fields = thisMessage.Split(spaceDelim, 255);
          foreach (string s in fields)
          {
            if (cpNext)
            {
              if (s.Length > 0)
              {
                score = s;
                cpDone = true;
                cpNext = false;
              }
            }
            else if (mNext)
            {
              if (s.Length > 0)
              {
                m = s;
                mDone = true;
                mNext = false;
              }
            }
            else if (pvNext)
            {
              if (s.Length > 0)
              {
                pv = s;
                //pv = s.Replace("x", "");
                pvDone = true;
                pvNext = false;
              }
            }
            else if (!cpDone && s.Equals("cp"))
            {
              cpNext = true;
            }
            else if (!mDone && s.Equals("mate"))
            {
              mNext = true;
            }
            else if (!pvDone && s.Equals("pv"))
            {
              pvNext = true;
            }

            if (cpDone && pvDone)
            {
              this.Invoke(new Action(delegate() { computerAnalysisResult.Text = (Convert.ToDouble(score) / (whitesMove ? -100 : 100)).ToString(); }));
              this.Invoke(new Action(delegate() { bestMove.Text = pv; }));
              break;
            }
            else if (mDone && pvDone)
            {
              int mateInValue = Convert.ToInt32(m) * (whitesMove ? -1 : 1);
              if (mateInValue < 0)
              {
                this.Invoke(new Action(delegate() { computerAnalysisResult.Text = ("-Mate " + (-mateInValue).ToString()); }));
              }
              else
              {
                this.Invoke(new Action(delegate() { computerAnalysisResult.Text = ("Mate " + (mateInValue).ToString()); }));
              }
              this.Invoke(new Action(delegate() { bestMove.Text = pv; }));
              break;
            }
          }

          thisMessage = thisMessage.Replace("score", "s");
          this.Invoke(new Action(delegate() { engineCommunication2.AppendText(Environment.NewLine + thisMessage); }));
        }
        else if (thisMessage.StartsWith("option name "))
        {
          this.Invoke(new Action(delegate() { engineOptionList2.Items.Add(thisMessage.Substring(12).Trim()); engineOptionList2.SelectedIndex = engineOptionList2.Items.Count - 1; }));
        }
        else if (thisMessage.StartsWith("bestmove NULL"))
        {
          this.Invoke(new Action(delegate() { engineCommunication2.AppendText(Environment.NewLine + thisMessage); }));
          goOK = true;
        }
        else if (thisMessage.StartsWith("bestmove "))
        {
          this.Invoke(new Action(delegate() { engineCommunication2.AppendText(Environment.NewLine + thisMessage); }));

          try
          {
            thisMessage = thisMessage.Substring(9).Trim();
            int length = thisMessage.IndexOf(" ", 1);
            if (length > 0)
            {
              thisMessage = thisMessage.Substring(0, length);
            }
          }
          catch (Exception e)
          {
            MessageBox.Show(e.ToString());
          }

          this.Invoke(new Action(delegate() { RespondToBestMove(thisMessage); }));
        }
        else if (thisMessage.IndexOf("id name ") >= 0)
        {
//          this.Invoke(new Action(delegate() { engines.Controls["blackEngine"].Text = thisMessage.Substring(8).Trim(); }));
//          this.Invoke(new Action(delegate() { engines.Controls["blackOptions"].Text = (thisMessage.Substring(8).Trim() + " Options"); }));
            this.Invoke(new Action(delegate() { engines.Controls["blackEngine"].Text = thisMessage.Substring(8 + thisMessage.IndexOf("id name ")).Trim(); }));
            this.Invoke(new Action(delegate() { engines.Controls["blackOptions"].Text = (thisMessage.Substring(8 + thisMessage.IndexOf("id name ")).Trim() + " Options"); }));
        }
        else if (thisMessage.StartsWith("id author "))
        {
          this.Invoke(new Action(delegate() { engineLoaded2.Text = "Author(s) : " + thisMessage.Substring(10).Trim(); }));
        }
        else if (thisMessage.StartsWith("uciok"))
        {
          if (!uciok2)
          {
            uciok2 = true;
            this.Invoke(new Action(delegate() { RespondToUCIOK2(); }));
          }
        }
        else if (thisMessage.StartsWith("readyok"))
        {
          this.Invoke(new Action(delegate()
          {
            readyok2 = true;
            engines.Controls["blackEngine"].Text = engines.Controls["blackEngine"].Text + " - Ready";
            TurnGameOptionsOn(2);
          }));
        }
        else if (thisMessage.Length == 0)
        {
        }
        else
        {
          //unknown message
          this.Invoke(new Action(delegate() { engineCommunication2.Text += (Environment.NewLine + "??? " + thisMessage + " ???" + Environment.NewLine); }));
        }
      }
    }

    //*******************************************************************************************************
    private void RespondToUCIOK()
    {
      engineOptionList.Visible = engineLoaded.Visible = engineOptionList.Visible = engineCommunication.Visible = true;

      if (moveAnalysis.Checked)
      {
        moveStatus.Text = "Load PGN or press Ready? after selecting options.";
      }

      ShowOptions(ref engineOptionList, "white", ref whiteOptions);
      WriteSIOfEngine("isready", 1);
    }

    //*******************************************************************************************************
    private void RespondToUCIOK2()
    {
      engineOptionList2.Visible = engineLoaded2.Visible = engineOptionList2.Visible = engineCommunication2.Visible = true;

      if (moveAnalysis.Checked)
      {
        moveStatus.Text = "Load PGN or press Ready? after selecting options.";
      }

      ShowOptions(ref engineOptionList2, "black", ref blackOptions);
      WriteSIOfEngine("isready", 2);
    }

    //*******************************************************************************************************
    private void RespondToBestMove(string bestmove)
    {
      try
      {
        bestMove.Text = bestmove;
        goOK = true;

        if (gamePaused)
        {
          //do not do anything with the results...
          return;
        }

        if (moveAnalysis.Checked)
        {
          //do not do anything
          if (newPosition.Length > 0)
          {
            if (newPosition.Equals("RAV"))
            {
              newPosition = "";
              AddRAVRecord(findingBestMove, lastRAV, "(" + computerAnalysisResult.Text + ")");
            }
            else
            {
              MakeNextAnalysisMove();
            }
          }
        }
        else if (gameAnalysis.Checked)
        {
          if (runClock == true)
          {
            if (findingBestMove)
            {
              if (lastRAV.StartsWith(((Move)(moveTree.SelectedNode.Tag)).lan))
              {
                //best move matches move actually made - skip to doing best move on the next move

                //add comment of analysis value...
                AddComment("{(" + computerAnalysisResult.Text + ")}");

                //make sure there is another move to process...
                try
                {
                  if (moveTree.SelectedNode.NextNode != null)
                  {
                    newMove = ((Move)(moveTree.SelectedNode.NextNode.Tag)).lan;
                    newPosition = ((Move)(moveTree.SelectedNode.NextNode.Tag)).priorFen;
                    moveTree.SelectedNode = moveTree.SelectedNode.NextNode;
                  }
                  else
                  {
                    //we are done
                    EndDeepAnalysis();
                    return;
                  }
                }
                catch
                {
                  //we are done
                  EndDeepAnalysis();
                  return;
                }
                newMove = "";
                findingBestMove = !findingBestMove;
              }
              else
              {
                //the actual move did not match the best move so analyse the actual move to determine how bad it was...
                newMove = ((Move)(moveTree.SelectedNode.Tag)).lan;
                newPosition = lastPosition;

                //save this in case it is needed after the next analysis
                lastBestMoveRAV = lastRAV;
                lastBestMoveAnalysis = computerAnalysisResult.Text;
              }
            }
            else
            {
              double bestAnalysis, currentAnalysis, questionable, poor, veryPoor, analysisDifference;
              bool nagAdded = false;
              bool skipRAV = false;

              AddComment("{(" + computerAnalysisResult.Text + ")}");

              //make the compiler happy...
              bestAnalysis = currentAnalysis = questionable = poor = veryPoor = analysisDifference = 0;

              try
              {
                bestAnalysis = Convert.ToDouble(lastBestMoveAnalysis);

                try
                {
                  currentAnalysis = Convert.ToDouble(computerAnalysisResult.Text);
                  //check this difference against the 3 thresholds
                  analysisDifference = bestAnalysis - currentAnalysis;
                }
                catch
                {
                  //this should not happen!
                  nagAdded = true;
                }
              }
              catch
              {
                //best move must be a mating move
                try
                {
                  currentAnalysis = Convert.ToDouble(computerAnalysisResult.Text);
                  nagAdded = true;
                }
                catch
                {
                  //are the mating move counts the same?
                  if (!lastBestMoveAnalysis.Equals(computerAnalysisResult.Text))
                  {
                    //best move probably mates faster...
                    nagAdded = true;
                  }
                  else
                  {
                    //identical, treat as a 'good enough' move
                    skipRAV = true;
                  }
                }
              }

              if (!skipRAV)
              {
                if (!nagAdded)
                {
                  try
                  {
                    questionable = Convert.ToDouble(questionableMove.Text);
                  }
                  catch
                  {
                    //last known good value
                    questionable = Convert.ToDouble(iniQuestionable);
                  }

                  try
                  {
                    poor = Convert.ToDouble(poorMove.Text);
                  }
                  catch
                  {
                    //last known good value
                    poor = Convert.ToDouble(iniPoor);
                  }

                  try
                  {
                    veryPoor = Convert.ToDouble(veryPoorMove.Text);
                  }
                  catch
                  {
                    //last known good value
                    veryPoor = Convert.ToDouble(iniVeryPoor);
                  }

                  if (analysisDifference < 0)
                  {
                    analysisDifference = -analysisDifference;
                  }

                  if (analysisDifference >= veryPoor)
                  {
                    if (!whitesMove)
                    {
                      if (bestAnalysis > currentAnalysis)
                      {
                        whiteVeryPoor++;
                        nagAdded = true;
                      }
                    }
                    else
                    {
                      if (bestAnalysis < currentAnalysis)
                      {
                        blackVeryPoor++;
                        nagAdded = true;
                      }
                    }

                    if (nagAdded)
                    {
                      ((Move)(moveTree.SelectedNode.Tag)).nag = 4;
                    }
                  }
                  else if (analysisDifference >= poor)
                  {
                    if (!whitesMove)
                    {
                      if (bestAnalysis > currentAnalysis)
                      {
                        whitePoor++;
                        nagAdded = true;
                      }
                    }
                    else
                    {
                      if (bestAnalysis < currentAnalysis)
                      {
                        blackPoor++;
                        nagAdded = true;
                      }
                    }
                    if (nagAdded)
                    {
                      ((Move)(moveTree.SelectedNode.Tag)).nag = 2;
                    }
                  }
                  else if (analysisDifference >= questionable)
                  {
                    if (!whitesMove)
                    {
                      if (bestAnalysis > currentAnalysis)
                      {
                        whiteQuestionable++;
                        nagAdded = true;
                      }
                    }
                    else
                    {
                      if (bestAnalysis < currentAnalysis)
                      {
                        blackQuestionable++;
                        nagAdded = true;
                      }
                    }
                    if (nagAdded)
                    {
                      ((Move)(moveTree.SelectedNode.Tag)).nag = 6;
                    }
                  }
                }

                if (nagAdded)
                {
                  AddRAVRecord(true, lastBestMoveRAV, "(" + lastBestMoveAnalysis + ")");
                  AddRAVRecord(false, lastRAV, "(" + computerAnalysisResult.Text + ")");
                }
                else if (slightlyBetter.Checked)
                {
                  if (!whitesMove) whiteBetter++; else blackBetter++;
                  betterMove = true;
                  AddRAVRecord(true, lastBestMoveRAV, "(" + lastBestMoveAnalysis + ")");
                  betterMove = false;
                }
              }

              //make sure there is another move to process...
              try
              {
                newMove = ((Move)(moveTree.SelectedNode.NextNode.Tag)).lan;
                if (newMove.Length > 0)
                {
                  newPosition = ((Move)(moveTree.SelectedNode.NextNode.Tag)).priorFen;
                  moveTree.SelectedNode = moveTree.SelectedNode.NextNode;
                }
                else
                {
                  //we are done
                  EndDeepAnalysis();
                  return;
                }
              }
              catch
              {
                //we are done
                EndDeepAnalysis();
                return;
              }
              newMove = "";
            }

            findingBestMove = !findingBestMove;
            MakeNextAnalysisMove();
            runCPUClock = true;
          }
        }
        else if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
        {
          if (clearEngineOutput.Checked)
          {
            engineCommunication.Text = "";
            engineCommunication2.Text = "";
          }

          if ((moveTree.SelectedNode != moveTree.Nodes[0].LastNode) && (moveTree.Nodes[0].Nodes.Count != 0))
          {
            moveTree.SelectedNode = moveTree.Nodes[0].LastNode;
          }

          if (MakeAMove(bestMove.Text.Substring(0, 2), bestMove.Text.Substring(2)))
          {
            savePGN.Visible = true;
            pgnLoaded = true;

            if (!gameOver && computerWhite.Checked && whitesMove)
            {
              WriteSIOfEngine("position fen " + fenNotation.Text, 1);
              WriteSIOfEngine("go infinite", 1);
              ready.Visible = true;
              ready2.Visible = false;
            }
            else if (!gameOver && computerBlack.Checked && !whitesMove)
            {
              WriteSIOfEngine("position fen " + fenNotation.Text, 2);
              WriteSIOfEngine("go infinite", 2);
              ready2.Visible = true;
              ready.Visible = false;
            }
            else
            {
              ready.Visible = false;
              ready2.Visible = false;
            }
          }
          else
          {
            runCPUClock = false;
            MessageBox.Show((whitesMove ? "Whites move " : "Blacks move ") + bestMove.Text + " failed...");
          }
        }
      }
      catch (Exception e)
      {
        MessageBox.Show("Problem in RespondToBestMove() : " + e.ToString());
      }
    }

    //*******************************************************************************************************
    private void StartDeepAnalysis()
    {
      whiteBetter = blackBetter = whiteQuestionable = blackQuestionable = 0;
      whitePoor = blackPoor = whiteVeryPoor = blackVeryPoor = 0;
      startDeepAnalysisTime = dateTime.Text;

      if (computerWhite.Checked)
      {
        ready.Text = "Stop";
        ready.Visible = true;
      }
      else
      {
        ready2.Text = "Stop";
        ready2.Visible = true;
      }

      //should this be done?
      //if (moveTree.SelectedNode.Parent.Text.Equals("Main") || moveTree.SelectedNode.Parent.Text.Equals("Puzzle"))
      //{
      //}
      //else
      //{
      //  //recalculate the analysis time - we are processing a RAV?

      //}

      addRAV.Visible = false;
      clearRAV.Visible = false;
      best.Visible = false;
      analyze.Visible = false;
      pauseGame.Visible = false;
      loadPGN.Visible = false;
      savePGN.Visible = false;

      computerWhite.Enabled = false;
      computerBlack.Enabled = false;

      //we want to start with the best move search, if it ends up being the same as the actual move the analysis
      //of the actual move can be skipped...
      findingBestMove = true;
      newMove = "";
      newPosition = ((Move)(moveTree.SelectedNode.Tag)).priorFen;

      bestMove.Visible = true;
      computerAnalysisResult.Visible = true;

      goOK = true;

      MakeNextAnalysisMove();

      runCPUClock = true;
      runClock = true;
    }

    //*******************************************************************************************************
    private void EndDeepAnalysis()
    {
      computerWhite.Enabled = true;
      computerBlack.Enabled = true;
      moveStatus.Text = "Game analysis is complete...";
      runCPUClock = false;
      savePGN.Visible = true;
      ready.Visible = false;
      ready2.Visible = false;

      //add analysis summary to pgn header
      if (gameInfo.Text.Length > 0) gameInfo.Text += "\r\n";
      gameInfo.Text += ("{Analysis done by " + engineLoaded.Text +
                        " : Started/Completed " + startDeepAnalysisTime + " / " + dateTime.Text +
                        " : Better moves proposed - W/B = " + whiteBetter.ToString() + "/" + blackBetter.ToString() +
                        " : Questionable moves made - W/B = " + whiteQuestionable.ToString() + "/" + blackQuestionable.ToString() +
                        " : Poor moves made - W/B = " + whitePoor.ToString() + "/" + blackPoor.ToString() +
                        " : Very poor moves made - W/B = " + whiteVeryPoor.ToString() + "/" + blackVeryPoor.ToString() +
                        "}");
    }

    //*******************************************************************************************************
    private void MakeNextAnalysisMove()
    {
      goOK = false;

      if (computerWhite.Checked)
      {
        ready.Visible = true;
        engineCommunication.Text = ""; //clear previous info
      }
      else
      {
        ready2.Visible = true;
        engineCommunication2.Text = ""; //clear previous info
      }

      computerWhite.Enabled = false;
      computerBlack.Enabled = false;

      bestMove.Text = "";
      computerAnalysisResult.Text = "";
      WriteSIOfEngine("position fen " + newPosition, (computerWhite.Checked ? 1 : 2));

      if (newMove.Length == 0)
      {
        WriteSIOfEngine("go infinite", (computerWhite.Checked ? 1 : 2));
        moveStatus.Text = "Analyzing " + (whitesMove ? "Blacks best move..." : "Whites best move...");
      }
      else
      {
        WriteSIOfEngine("go searchmoves " + newMove + " infinite", (computerWhite.Checked ? 1 : 2));
        moveStatus.Text = "Analyzing " + (whitesMove ? "Blacks current move..." : "Whites current move...");
      }

      lastPosition = newPosition;
      newPosition = "";
      newMove = "";
    }

    //*******************************************************************************************************
    private void TurnGameOptionsOn(int engine)
    {
      if (moveAnalysis.Checked)
      {
        if (engine == 1)
        {
          ChangeButton(ref ready, "Stop", "Stop", false);
        }
        else
        {
          ChangeButton(ref ready2, "Stop", "Stop", false);
        }

        goOK = true;
        //computerAnalysisResult.Visible = true;
        moveStatus.Text = "Select a move to Analyze...";
      }
      else if (gameAnalysis.Checked)
      {
      }
      else if (positionSetup.Checked)
      {
        //no engine should be running here
      }
      else //play
      {
        useClock.Visible = true;

        if (useClock.Checked)
        {
          whiteClock.Visible = true;
          blackClock.Visible = true;
          clockStart.Visible = true;
          bronsteinTime.Visible = true;
        }

        computerTime.Visible = true;
        computerTimeLabel.Visible = true;
        computerClock.Visible = true;

        if (computerWhite.Checked)
        {
          if (engine == 1)
          {
            ready.Visible = false;
            //resuming a game???
            //whiteDraw.Visible = true;

            //if (!computerBlack.Checked)
            //{
            //  blackDraw.Visible = true;
            //}
          }
        }

        if (computerBlack.Checked)
        {
          if (engine == 2)
          {
            ready2.Visible = false;
            //resuming a game???
            //blackDraw.Visible = true;

            //if (!computerWhite.Checked)
            //{
            //  whiteDraw.Visible = true;
            //}
          }
        }

        if (whiteDraw.Visible && blackDraw.Visible)
        {
          goOK = true;
          moveStatus.Text = "Press 'Start' to begin game.";
        }

      }
    }

    //*******************************************************************************************************
    private void WriteSIOfEngine(string cmd, int eng)
    {
      if (eng == 1)
      {
        if (engineActive && (engine != null))
        {
          engine.StandardInput.WriteLine(cmd);
          engineCommunication.AppendText(Environment.NewLine + cmd);
        }
      }
      else if (eng == 2)
      {
        if (engineActive2 && (engine2 != null))
        {
          engine2.StandardInput.WriteLine(cmd);
          engineCommunication2.AppendText(Environment.NewLine + cmd);
        }
      }
    }

    //*******************************************************************************************************
    protected void WriteEngineCommands(string src, string dst)
    {
      WriteSIOfEngine("position fen " + fenNotation.Text, (whitesMove ? 1 : 2));
      WriteSIOfEngine("go infinite", (whitesMove ? 1 : 2));
    }

    #endregion

    #region Timers

    //*******************************************************************************************************
    private void useClock_CheckedChanged(object sender, EventArgs e)
    {
      if (useClock.Checked)
      {
        clockStart.Visible = true;
        bronsteinTime.Visible = true;
        whiteClock.Visible = true;
        blackClock.Visible = true;
      }
      else
      {
        clockStart.Visible = false;
        bronsteinTime.Visible = false;
        whiteClock.Visible = false;
        blackClock.Visible = false;
      }
    }

    //*******************************************************************************************************
    private void useAbortTime_CheckedChanged(object sender, EventArgs e)
    {
      abortTime.Visible = useAbortTime.Checked;
    }

    //*******************************************************************************************************
    protected void masterTimer_Tick(object sender, EventArgs e)
    {
      displayReady = true;

      dtpGeneral.Value = DateTime.Now;
      dateTime.Text = dtpGeneral.Value.ToString("yyyy.MM.dd HH:mm:ss");
      TimeSpan timeDelta = dtpGeneral.Value.Subtract(dtClock);

      if (runClock && !gameAnalysis.Checked)
      {
        if (whitesMove)
        {
          dtpWhite.Value -= timeDelta;
          whiteClock.Text = dtpWhite.Value.ToString("HH:mm:ss");

          if (whiteClock.Text.Equals("00:00:00") || whiteClock.Text.StartsWith("23:5"))
          {
            runClock = false;
            runCPUClock = false;

            gameOver = true;
            if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
            {
                if (computerWhite.Checked) StopYourEngine(1);
                if (computerBlack.Checked) StopYourEngine(2);
            }

            if (gameInfo.Text.Length > 0)
            {
              //make sure black has sufficient material to deserve the win...
              if (((Move)(moveTree.Nodes[0].LastNode.Tag)).insufficientBlack)
              {
                gameResult.Text = "1/2-1/2";
                //this should override any existing entries
                gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
                gameInfo.Text += "\r\n[Termination \"Draw for white running out of time and black having insufficient material to win\"]";
                EnableMainSelections("Draw, white is out of time and black has insufficient material.", "Game Over", true);
              }
              else
              {
                gameResult.Text = "0-1";
                //this should override any existing entries
                gameInfo.Text += "\r\n[Result \"0-1\"]";
                gameInfo.Text += "\r\n[Termination \"Black won on time\"]";
                EnableMainSelections("White is out of time, black wins.", "Game Over", true);
              }
            }
            else
            {
              //make sure black has sufficient material to deserve the win...
              if (((Move)(moveTree.Nodes[0].LastNode.Tag)).insufficientBlack)
              {
                gameResult.Text = "1/2-1/2";
                gameInfo.Text = "[Result \"1/2-1/2\"]";
                gameInfo.Text += "\r\n[Termination \"Draw for white running out of time and black having insufficient material to win\"]";
                EnableMainSelections("Draw, white is out of time and black has insufficient material.", "Game Over", true);
              }
              else
              {
                gameResult.Text = "0-1";
                gameInfo.Text = "[Result \"0-1\"]";
                gameInfo.Text += "\r\n[Termination \"Black won on time\"]";
                EnableMainSelections("White is out of time, black wins.", "Game Over", true);
              }
            }
          }

          if (!LastColorClockChangeWasWhite)
          {
            int bt = Convert.ToInt16(bronsteinTime.Text);
            LastColorClockChangeWasWhite = true;
            TimeSpan timeUsed = initialBlackClock.Subtract(dtpBlack.Value);
            if (timeUsed.Seconds > bt)
            {
              dtpBlack.Value = dtpBlack.Value.AddSeconds(bt);
            }
            else
            {
              dtpBlack.Value = initialBlackClock;
            }
            initialBlackClock = dtpBlack.Value;
            blackClock.Text = dtpBlack.Value.ToString("HH:mm:ss");
          }
        }
        else
        {
          dtpBlack.Value -= timeDelta;
          blackClock.Text = dtpBlack.Value.ToString("HH:mm:ss");

          if (blackClock.Text.Equals("00:00:00") || blackClock.Text.StartsWith("23:5"))
          {
            runClock = false;
            runCPUClock = false;

            gameOver = true;
            if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
            {
              if (computerWhite.Checked) WriteSIOfEngine("stop", 1);
              if (computerBlack.Checked) WriteSIOfEngine("stop", 2);
            }

            if (gameInfo.Text.Length > 0)
            {
              //make sure white has sufficient material to deserve the win...
              if (((Move)(moveTree.Nodes[0].LastNode.Tag)).insufficientWhite)
              {
                gameResult.Text = "1/2-1/2";
                //this should override any existing entries
                gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
                gameInfo.Text += "\r\n[Termination \"Draw for black running out of time and white having insufficient material to win\"]";
                EnableMainSelections("Draw, black is out of time and white has insufficient material.", "Game Over", true);
              }
              else
              {
                gameResult.Text = "1-0";
                //this should override any existing entries
                gameInfo.Text += "\r\n[Result \"1-0\"]";
                gameInfo.Text += "\r\n[Termination \"White won on time\"]";
                EnableMainSelections("Black is out of time, white wins.", "Game Over", true);
              }
            }
            else
            {
              //make sure white has sufficient material to deserve the win...
              if (((Move)(moveTree.Nodes[0].LastNode.Tag)).insufficientWhite)
              {
                gameResult.Text = "1/2-1/2";
                //this should override any existing entries
                gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
                gameInfo.Text += "\r\n[Termination \"Draw for black running out of time and white having insufficient material to win\"]";
                EnableMainSelections("Draw, black is out of time and white has insufficient material.", "Game Over", true);
              }
              else
              {
                gameResult.Text = "1-0";
                gameInfo.Text = "[Result \"1-0\"]";
                gameInfo.Text += "\r\n[Termination \"White won on time\"]";
                EnableMainSelections("Black is out of time, white wins.", "Game Over", true);
              }
            }
          }

          if (LastColorClockChangeWasWhite)
          {
            int bt = Convert.ToInt16(bronsteinTime.Text);
            LastColorClockChangeWasWhite = false;
            TimeSpan timeUsed = initialWhiteClock.Subtract(dtpWhite.Value);
            if (timeUsed.Seconds > bt)
            {
              dtpWhite.Value = dtpWhite.Value.AddSeconds(bt);
            }
            else
            {
              dtpWhite.Value = initialWhiteClock;
            }
            initialWhiteClock = dtpWhite.Value;
            whiteClock.Text = dtpWhite.Value.ToString("HH:mm:ss");

          }
        }
        dtClock = dtpGeneral.Value;
      }
      else
      {
        dtClock = dtpGeneral.Value;
      }

      if (runCPUClock)
      {
        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
        {
          gamePaused = false;

          if ((moveTree.SelectedNode != moveTree.Nodes[0].LastNode) && (moveTree.Nodes[0].Nodes.Count != 0))
          {
            moveTree.SelectedNode = moveTree.Nodes[0].LastNode;
          }

          if (whitesMove && computerWhite.Checked && (moveTree.Nodes.Count == 0) && !startingMoveMade)
          {
            startingMoveMade = true;
            WriteSIOfEngine("position fen " + fenNotation.Text, 1);
            WriteSIOfEngine("go infinite", 1);
            ResetComputerClock();
          }
          else if (whitesMove && computerWhite.Checked)
          {
            dtpComputer.Value -= timeDelta;
            computerClock.Text = dtpComputer.Value.ToString("HH:mm:ss");

            if (computerClock.Text.Equals("00:00:00") || computerClock.Text.StartsWith("23:5"))
            {
              WriteSIOfEngine("stop", 1);
              ResetComputerClock();
            }
            ready.Visible = true;
          }
          else if (!whitesMove && computerBlack.Checked)
          {
            dtpComputer.Value -= timeDelta;
            computerClock.Text = dtpComputer.Value.ToString("HH:mm:ss");

            if (computerClock.Text.Equals("00:00:00") || computerClock.Text.StartsWith("23:5"))
            {
              WriteSIOfEngine("stop", 2);
              ResetComputerClock();
            }
            ready2.Visible = true;
          }
        }
        else if (gameAnalysis.Checked)
        {
          dtpComputer.Value -= timeDelta;
          computerClock.Text = dtpComputer.Value.ToString("HH:mm:ss");


          bool endProcess = false;

          if (useAbortTime.Checked)
          {
            if (lastBestDeep.Equals(bestMove.Text) && lastAnalysisDeep.Equals(computerAnalysisResult.Text))
            {
              endProcess = (dtpComputer.Value < timeSinceLastChange);
            }
            else
            {
              lastBestDeep = bestMove.Text;
              lastAnalysisDeep = computerAnalysisResult.Text;
              ResetComputerAbortClock();
            }
          }

          if (endProcess || computerClock.Text.Equals("00:00:00") || computerClock.Text.StartsWith("23:5"))
          {
            WriteSIOfEngine("stop", (computerWhite.Checked ? 1 : 2));
            ResetComputerClock();
            runCPUClock = false;
          }
        }
      }
    }

    //*******************************************************************************************************
    protected void ResetClocks()
    {
      DateTime dt = new DateTime(2012, 12, 21, 0, 0, 0, 0, DateTimeKind.Unspecified);
      dtClock = DateTime.Now;

      dt = dt.AddMinutes(Convert.ToDouble(clockStart.Text));
      dtpWhite.Value = dt;
      dtpBlack.Value = dt;
      initialWhiteClock = dt;
      initialBlackClock = dt;
      whiteClock.Text = dt.ToString("HH:mm:ss");
      blackClock.Text = dt.ToString("HH:mm:ss");
    }

    //*******************************************************************************************************
    protected void ResetComputerClock()
    {
      DateTime dt = new DateTime(2012, 12, 21, 0, 0, 0, 0, DateTimeKind.Unspecified);
      dt = dt.AddSeconds(Convert.ToDouble(computerTime.Text));
      dtpComputer.Value = dt;
      computerClock.Text = dt.ToString("HH:mm:ss");
      lastBestDeep = "";
      lastAnalysisDeep = "";
      ResetComputerAbortClock();
    }

    //*******************************************************************************************************
    protected void ResetComputerAbortClock()
    {
      try
      {
        timeSinceLastChange = dtpComputer.Value - new TimeSpan(0, 0, Convert.ToInt16(abortTime.Text));
      }
      catch
      {
        abortTime.Text = "10";
        timeSinceLastChange = dtpComputer.Value - new TimeSpan(0, 0, 10);
      }
    }

    #endregion

    #region Draw and Resign

    //*******************************************************************************************************
    protected bool CheckForDuplicateMoves()
    {
      //we should get at least 1...
      int duplicateCount = 0;

      try
      {
        string thisFEN = ((Move)(moveTree.SelectedNode.Tag)).fen.Substring(0, ((Move)(moveTree.SelectedNode.Tag)).fen.IndexOf(" "));
        TreeNode tn = moveTree.SelectedNode;
        CheckForDuplicateMoves(thisFEN, ref duplicateCount, ref tn);
      }
      catch
      {
        duplicateCount = 1;
      }

      duplicateFENs.Text = duplicateCount.ToString();

      return (duplicateCount >= 3);
    }

    //*******************************************************************************************************
    protected void CheckForDuplicateMoves(string thisFEN, ref int duplicateCount, ref TreeNode tn)
    {
      if (tn != null)
      {
        if (!tn.Text.StartsWith("RAV"))
        {
          if (((Move)(tn.Tag)).fen.Substring(0, ((Move)(tn.Tag)).fen.IndexOf(" ")).Equals(thisFEN))
          {
            duplicateCount++;
          }

          TreeNode tnp = tn.PrevNode;
          if (tnp == null)
          {
            if (tn.Parent == null) return;
            tnp = tn.Parent;
          }

          CheckForDuplicateMoves(thisFEN, ref duplicateCount, ref tnp);
        }
        else
        {
          TreeNode tnp = tn.PrevNode;
          if (tnp == null)
          {
            if (tn.Parent == null) return;
            tnp = tn.Parent;
          }

          CheckForDuplicateMoves(thisFEN, ref duplicateCount, ref tnp);
        }
      }
    }

    //*******************************************************************************************************
    protected void CheckForClaimingDraw()
    {

      if (CheckForDuplicateMoves())
      {
        if (blackDraw.Visible && whiteDraw.Visible)
        {
          ChangeButton(ref whiteDraw, "Claim", "A Draw may be claimed for the threefold repetition rule violation.", true);
          ChangeButton(ref blackDraw, "Claim", "A Draw may be claimed for the threefold repetition rule violation.", true);
          whiteDraw.AccessibleDescription = "Threefold repetition rule violation.";
          blackDraw.AccessibleDescription = "Threefold repetition rule violation.";
        }
      }
      else if (halfMove >= 100)
      {
        if (blackDraw.Visible && whiteDraw.Visible)
        {
          ChangeButton(ref whiteDraw, "Claim", "A Draw may be claimed for the 50 move rule violation.", true);
          ChangeButton(ref blackDraw, "Claim", "A Draw may be claimed for the 50 move rule violation.", true);
          whiteDraw.AccessibleDescription = "50 move rule violation.";
          blackDraw.AccessibleDescription = "50 move rule violation.";
        }
      }
      else if (!(whiteDraw.Text.Equals("Start") || blackDraw.Text.Equals("Start") || whiteDraw.Text.Equals("Decline") || blackDraw.Text.Equals("Decline")))
      {
        if (blackDraw.Visible && whiteDraw.Visible)
        {
          ChangeButton(ref whiteDraw, "Offer", "A Draw may be requested.", true);
          ChangeButton(ref blackDraw, "Offer", "A Draw may be requested.", true);
          whiteDraw.AccessibleDescription = "";
          blackDraw.AccessibleDescription = "";
        }
      }
    }

    //*******************************************************************************************************
    protected void InvalidMoveMade()
    {
      if (displayReady)
      {
        if (enableSound && play.Checked)
        {
          SoundPlayer simpleSound = new SoundPlayer(soundPath + "\\invalid.wav");
          simpleSound.Play();
        }
      }
    }

    //*******************************************************************************************************
    protected void whiteDraw_Click(object sender, EventArgs e)
    {
      WhiteDrawPressed();
      moveTree.Focus();
    }

    //*******************************************************************************************************
    protected void blackDraw_Click(object sender, EventArgs e)
    {
      BlackDrawPressed();
      moveTree.Focus();
    }

    //*******************************************************************************************************
    protected void whiteResign_Click(object sender, EventArgs e)
    {
      if (whiteResign.Text.Equals("Resign"))
      {
        //game over...
        moveOK = false;
        gameOver = true;
        runClock = false;
        runCPUClock = false;
        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
        {
          WriteSIOfEngine("stop", 1);
        }
        gameResult.Text = "0-1";
        gameInfo.Text += "\r\n[Result \"0-1\"]";
        gameInfo.Text += "\r\n[Termination \"White Resigned\"]";
        EnableMainSelections("White has resigned, black wins.", "Game Over", true);
      }
      else if (whiteResign.Text.Equals("Accept"))
      {
        //game over...
        moveOK = false;
        gameOver = true;
        runClock = false;
        runCPUClock = false;

        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
        {
          WriteSIOfEngine("stop", 1);
        }
        gameResult.Text = "1/2-1/2";
        gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
        gameInfo.Text += "\r\n[Termination \"White accepted draw offer\"]";
        EnableMainSelections("Game has been drawn by agreement.", "Game Over", true);
      }
      moveTree.Focus();
    }

    //*******************************************************************************************************
    protected void blackResign_Click(object sender, EventArgs e)
    {
      if (blackResign.Text.Equals("Resign"))
      {
        //game over...
        moveOK = false;
        gameOver = true;
        runClock = false;
        runCPUClock = false;
        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
        {
          WriteSIOfEngine("stop", 2);
        }
        gameResult.Text = "1-0";
        gameInfo.Text += "\r\n[Result \"1-0\"]";
        gameInfo.Text += "\r\n[Termination \"Black Resigned\"]";
        EnableMainSelections("Black has resigned, white wins.", "Game Over", true);
      }
      else if (blackResign.Text.Equals("Accept"))
      {
        //game over...
        moveOK = false;
        gameOver = true;
        runClock = false;
        runCPUClock = false;
        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
        {
          WriteSIOfEngine("stop", 2);
        }
        gameResult.Text = "1/2-1/2";
        gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
        gameInfo.Text += "\r\n[Termination \"Black accepted draw offer\"]";
        EnableMainSelections("Game has been drawn by agreement.", "Game Over", true);
      }
      moveTree.Focus();
    }

    //*******************************************************************************************************
    private void WhiteDrawPressed()
    {
      if (whiteDraw.Text.Equals("Start"))
      {
        if (resumingGame)
        {
          //make sure we are at the end of the game...
          moveTree.SelectedNode = moveTree.Nodes[0].LastNode;
          whitesMove = !((Move)(moveTree.SelectedNode.Tag)).whitesMove;
        }

        bestMove.Text = "";
        moveOK = true;
        moveStatus.Text = (whitesMove ? "Whites move." : "Blacks move.");
        useClock.Enabled = false;
        loadPGN.Visible = false;

        if (useClock.Checked)
        {
          whiteClock.Visible = true;
          blackClock.Visible = true;

          if (!resumingGame) ResetClocks();

          runClock = true;
        }
        resumingGame = false;

        ChangeButton(ref whiteDraw, "Offer", "Offer Black a Draw.");
        ChangeButton(ref blackDraw, "Offer", "Offer White a Draw.");
        ChangeButton(ref whiteResign, "Resign", "White resigns game.");
        ChangeButton(ref blackResign, "Resign", "Black resigns game.");

        ChangeButton(ref pauseGame, "Pause", "Pause Timers.", useClock.Checked);
        DisableMainSelections();

        if (play.Checked)
        {
          computerWhite.Enabled = false;
          computerBlack.Enabled = false;
        }

        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
        {
          if (computerWhite.Checked)
          {
            ChangeButton(ref ready, "Move", "???", false);
          }

          if (computerBlack.Checked)
          {
            ChangeButton(ref ready2, "Move", "???", true);

            WriteSIOfEngine("position fen " + fenNotation.Text, 2);
            WriteSIOfEngine("go infinite", 2);
          }

          ResetComputerClock();
          bestMove.Visible = false;
          computerAnalysisResult.Visible = false;
          computerTime.Visible = true;
          computerTimeLabel.Visible = true;
          computerClock.Visible = true;
          runCPUClock = true;
        }
        else
        {
          runCPUClock = false;
        }
        ClearPieceMoving();
      }
      else if (whiteDraw.Text.Equals("Abort"))
      {
        DisableEverything();
        resumingGame = false;
        SetupForNextLiveGame();
      }
      else if (whiteDraw.Text.Equals("Claim"))
      {
        //game over...
        moveOK = false;
        gameOver = true;
        runClock = false;
        runCPUClock = false;
        //not sure if this is even necessary...
        if (play.Checked)
        {
          if (computerWhite.Checked) WriteSIOfEngine("stop", 1);
          if (computerBlack.Checked) WriteSIOfEngine("stop", 2);
        }
        gameResult.Text = "1/2-1/2";
        gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
        gameInfo.Text += "\r\n[Termination \"White claimed draw of " + whiteDraw.AccessibleDescription + "\"]";
        EnableMainSelections("Game has been drawn by whites claim of " + whiteDraw.AccessibleDescription + ".", "Game Over", true);
      }
      else if (whiteDraw.Text.Equals("Decline"))
      {
        moveOK = true;
        ChangeButton(ref whiteDraw, "Offer", "Offer black a draw.");
        ChangeButton(ref blackDraw, "Offer", "Offer white a draw.");
        ChangeButton(ref whiteResign, "Resign", "White resigns game.");
        blackResign.Visible = true;
        savePGN.Visible = true;
        pauseGame.Visible = useClock.Checked;

        if (useClock.Checked)
        {
          runClock = true;
        }
        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
          runCPUClock = true;
      }
      else if (whiteDraw.Text.Equals("Offer"))
      {
        moveOK = false;
        ChangeButton(ref blackDraw, "Decline", "Decline whites offer of a draw.");
        ChangeButton(ref blackResign, "Accept", "Accept whites offer of a draw.");
        whiteResign.Visible = false;
        savePGN.Visible = false;
        pauseGame.Visible = false;
        runClock = false;
        runCPUClock = false;
      }
    }

    //*******************************************************************************************************
    private void BlackDrawPressed()
    {
      if (blackDraw.Text.Equals("Start"))
      {
        if (resumingGame)
        {
          //make sure we are at the end of the game...
          moveTree.SelectedNode = moveTree.Nodes[0].LastNode;
          whitesMove = !((Move)(moveTree.SelectedNode.Tag)).whitesMove;
        }

        bestMove.Text = "";
        moveOK = true;
        moveStatus.Text = (whitesMove ? "Whites move." : "Blacks move.");
        useClock.Enabled = false;
        loadPGN.Visible = false;

        if (useClock.Checked)
        {
          whiteClock.Visible = true;
          blackClock.Visible = true;

          if (!resumingGame) ResetClocks();

          runClock = true;
        }
        resumingGame = false;

        ChangeButton(ref whiteDraw, "Offer", "Offer Black a Draw.");
        ChangeButton(ref blackDraw, "Offer", "Offer White a Draw.");
        ChangeButton(ref whiteResign, "Resign", "White resigns game.");
        ChangeButton(ref blackResign, "Resign", "Black resigns game.");

        ChangeButton(ref pauseGame, "Pause", "Pause Timers.", useClock.Checked);
        DisableMainSelections();

        if (play.Checked)
        {
          computerWhite.Enabled = false;
          computerBlack.Enabled = false;
        }

        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
        {
          startingMoveMade = false;
          ResetComputerClock();

          if (computerWhite.Checked)
          {
            ChangeButton(ref ready, "Move", "???", true);

            WriteSIOfEngine("position fen " + fenNotation.Text, 1);
            WriteSIOfEngine("go infinite", 1);
          }

          if (computerBlack.Checked)
          {
            ChangeButton(ref ready2, "Move", "???", false);
          }

          bestMove.Visible = false;
          computerAnalysisResult.Visible = false;
          computerTime.Visible = true;
          computerTimeLabel.Visible = true;
          computerClock.Visible = true;
          runCPUClock = true;
        }
        else
        {
          runCPUClock = false;
        }
        ClearPieceMoving();
      }
      else if (blackDraw.Text.Equals("Abort"))
      {
        DisableEverything();
        resumingGame = false;
        SetupForNextLiveGame();
      }
      else if (blackDraw.Text.Equals("Claim"))
      {
        //game over...
        moveOK = false;
        gameOver = true;
        runClock = false;
        runCPUClock = false;
        if (play.Checked)
        {
          if (computerWhite.Checked) WriteSIOfEngine("stop", 1);
          if (computerBlack.Checked) WriteSIOfEngine("stop", 2);
        }
        gameResult.Text = "1/2-1/2";
        gameInfo.Text += "\r\n[Result \"1/2-1/2\"]";
        gameInfo.Text += "\r\n[Termination \"Black claimed draw of " + blackDraw.AccessibleDescription + "\"]";
        EnableMainSelections("Game has been drawn by blacks claim of " + blackDraw.AccessibleDescription + ".", "Game Over", true);
      }
      else if (blackDraw.Text.Equals("Decline"))
      {
        moveOK = true;
        ChangeButton(ref whiteDraw, "Offer", "Offer to end the game in a draw.");
        ChangeButton(ref blackDraw, "Offer", "Offer to end the game in a draw.");
        ChangeButton(ref blackResign, "Resign", "Black resigns game.");
        whiteResign.Visible = true;
        savePGN.Visible = true;
        pauseGame.Visible = useClock.Checked;

        if (useClock.Checked)
        {
          runClock = true;
        }
        if (play.Checked && (computerWhite.Checked || computerBlack.Checked))
          runCPUClock = true;
      }
      else if (blackDraw.Text.Equals("Offer"))
      {
        moveOK = false;
        ChangeButton(ref whiteDraw, "Decline", "Decline blacks offer of a draw.");
        ChangeButton(ref whiteResign, "Accept", "Accept blacks offer of a draw.");
        blackResign.Visible = false;
        savePGN.Visible = false;
        pauseGame.Visible = false;

        runClock = false;
        runCPUClock = false;
      }
    }

    #endregion

    #region Position Setup Menu

    //*******************************************************************************************************
    protected void fenNotation_TextChanged(object sender, EventArgs e)
    {
      if (positionSetup.Checked)
      {
        moveTree.Nodes[0].Tag = new Move(fenNotation.Text, 1);
      }
    }

    //*******************************************************************************************************
    protected void updateFEN_Click(object sender, EventArgs e)
    {
      ResetPieces(fenNotation.Text);
    }

    //*******************************************************************************************************
    protected void removePieceToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "";
      theBoard.Controls[onPanel].BackgroundImage = blank;
      fenNotation.Text = ToFEN(true);
    }

    //*******************************************************************************************************
    protected void pawnToolStripMenuItem2_Click(object sender, EventArgs e)
    {
      try
      {
        theBoard.Controls[onPanel].AccessibleName = "wPawn";
        theBoard.Controls[onPanel].BackgroundImage = wPawn;
        fenNotation.Text = ToFEN(true);
      }
      catch
      {
      }
    }

    //*******************************************************************************************************
    protected void rookToolStripMenuItem2_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "wRook";
      theBoard.Controls[onPanel].BackgroundImage = wRook;
      fenNotation.Text = ToFEN(true);
    }

    //*******************************************************************************************************
    protected void knightToolStripMenuItem2_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "wKnight";
      theBoard.Controls[onPanel].BackgroundImage = wKnight;
      fenNotation.Text = ToFEN(true);
    }

    //*******************************************************************************************************
    protected void bishopToolStripMenuItem2_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "wBishop";
      theBoard.Controls[onPanel].BackgroundImage = wBishop;
      fenNotation.Text = ToFEN(true);
    }

    //*******************************************************************************************************
    protected void queenToolStripMenuItem2_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "wQueen";
      theBoard.Controls[onPanel].BackgroundImage = wQueen;
      fenNotation.Text = ToFEN(true);
    }

    //*******************************************************************************************************
    protected void kingToolStripMenuItem2_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "wKing";
      theBoard.Controls[onPanel].BackgroundImage = wKing;
      fenNotation.Text = ToFEN(true);
    }

    //*******************************************************************************************************
    protected void pawnToolStripMenuItem3_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "kPawn";
      theBoard.Controls[onPanel].BackgroundImage = kPawn;
      fenNotation.Text = ToFEN(false);
    }

    //*******************************************************************************************************
    protected void rookToolStripMenuItem3_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "kRook";
      theBoard.Controls[onPanel].BackgroundImage = kRook;
      fenNotation.Text = ToFEN(false);
    }

    //*******************************************************************************************************
    protected void knightToolStripMenuItem3_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "kKnight";
      theBoard.Controls[onPanel].BackgroundImage = kKnight;
      fenNotation.Text = ToFEN(false);
    }

    //*******************************************************************************************************
    protected void bishopToolStripMenuItem3_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "kBishop";
      theBoard.Controls[onPanel].BackgroundImage = kBishop;
      fenNotation.Text = ToFEN(false);
    }

    //*******************************************************************************************************
    protected void queenToolStripMenuItem3_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "kQueen";
      theBoard.Controls[onPanel].BackgroundImage = kQueen;
      fenNotation.Text = ToFEN(false);
    }

    //*******************************************************************************************************
    protected void kingToolStripMenuItem3_Click(object sender, EventArgs e)
    {
      theBoard.Controls[onPanel].AccessibleName = "kKing";
      theBoard.Controls[onPanel].BackgroundImage = kKing;
      fenNotation.Text = ToFEN(false);
    }

    //*******************************************************************************************************
    protected void clearBoardToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      fenNotation.Text = "8/8/8/8/8/8/8/8 w - - 0 1";
      duplicateFENs.Text = "1";
      ResetPieces(fenNotation.Text);
    }

    //*******************************************************************************************************
    protected void resetBoardToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      fenNotation.Text = virtualChessBoard.fenStart;
      duplicateFENs.Text = "1";
      ResetPieces(fenNotation.Text);
    }

    //*******************************************************************************************************
    private void PSContextMenu(bool add)
    {
      a1.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      a2.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      a3.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      a4.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      a5.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      a6.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      a7.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      a8.ContextMenuStrip = (add ? newPSContextMenuStrip : null);

      b1.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      b2.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      b3.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      b4.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      b5.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      b6.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      b7.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      b8.ContextMenuStrip = (add ? newPSContextMenuStrip : null);

      c1.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      c2.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      c3.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      c4.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      c5.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      c6.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      c7.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      c8.ContextMenuStrip = (add ? newPSContextMenuStrip : null);

      d1.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      d2.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      d3.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      d4.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      d5.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      d6.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      d7.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      d8.ContextMenuStrip = (add ? newPSContextMenuStrip : null);

      e1.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      e2.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      e3.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      e4.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      e5.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      e6.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      e7.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      e8.ContextMenuStrip = (add ? newPSContextMenuStrip : null);

      f1.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      f2.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      f3.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      f4.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      f5.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      f6.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      f7.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      f8.ContextMenuStrip = (add ? newPSContextMenuStrip : null);

      g1.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      g2.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      g3.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      g4.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      g5.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      g6.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      g7.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      g8.ContextMenuStrip = (add ? newPSContextMenuStrip : null);

      h1.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      h2.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      h3.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      h4.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      h5.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      h6.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      h7.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
      h8.ContextMenuStrip = (add ? newPSContextMenuStrip : null);
    }

    //*******************************************************************************************************
    protected void ResetPieces(Move mv)
    {
      fenNotation.Text = mv.fen;
      ResetPieces(mv.fen, mv.nag, mv.comment);
    }

    //*******************************************************************************************************
    protected void ResetPieces(string fen)
    {
      ResetPieces(fen, 0, "");
    }

    //*******************************************************************************************************
    protected void ResetPieces(string fen, int ng, string cmnt)
    {
      if (ng < nag.Items.Count)
      {
        nag.SelectedIndex = ng;
      }
      else
      {
        nag.SelectedIndex = nag.Items.Count - 1;
      }
      comment.Text = cmnt;

      if (fen.Length > 0)
      {
        string square, tmp = fen;
        string[] fields = new string[255];
        char[] delimiter = " /".ToCharArray();
        int wq, wb, wk, wr, wp, kq, kb, kk, kr, kp;
        int index, fieldIndex = 0, blankCount;

        //count the number of pieces still on the board
        wq = wb = wk = wr = wp = kq = kb = kk = kr = kp = 0;

        fields = tmp.Split(delimiter, 255);

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
            square = fenLocation(rank, file);

            if (fields[fieldIndex].Substring(index, 1).Equals("k"))
            {
              theBoard.Controls[square].AccessibleName = "kKing";
              UpdateSquare(square);
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("r"))
            {
              theBoard.Controls[square].AccessibleName = "kRook";
              UpdateSquare(square);
              kr++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("n"))
            {
              theBoard.Controls[square].AccessibleName = "kKnight";
              UpdateSquare(square);
              kk++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("b"))
            {
              theBoard.Controls[square].AccessibleName = "kBishop";
              UpdateSquare(square);
              kb++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("q"))
            {
              theBoard.Controls[square].AccessibleName = "kQueen";
              UpdateSquare(square);
              kq++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("p"))
            {
              theBoard.Controls[square].AccessibleName = "kPawn";
              UpdateSquare(square);
              kp++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("K"))
            {
              theBoard.Controls[square].AccessibleName = "wKing";
              UpdateSquare(square);
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("R"))
            {
              theBoard.Controls[square].AccessibleName = "wRook";
              UpdateSquare(square);
              wr++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("N"))
            {
              theBoard.Controls[square].AccessibleName = "wKnight";
              UpdateSquare(square);
              wk++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("Q"))
            {
              theBoard.Controls[square].AccessibleName = "wQueen";
              UpdateSquare(square);
              wq++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("B"))
            {
              theBoard.Controls[square].AccessibleName = "wBishop";
              UpdateSquare(square);
              wb++;
            }
            else if (fields[fieldIndex].Substring(index, 1).Equals("P"))
            {
              theBoard.Controls[square].AccessibleName = "wPawn";
              UpdateSquare(square);
              wp++;
            }
            else
            {
              //this should be a number indicating a number of blank squares
              blankCount = Convert.ToInt16(fields[fieldIndex].Substring(index, 1));

              theBoard.Controls[square].AccessibleName = "";
              UpdateSquare(square);

              while (--blankCount > 0)
              {
                file++;
                if (file >= 8) break;
                square = fenLocation(rank, file);
                theBoard.Controls[square].AccessibleName = "";
                UpdateSquare(square);
              }
            }
            index++;
          }
          fieldIndex++;
        }

        ClearCasualties();
        UpdateCasualties(true, wq, wr, wb, wk, wp);
        UpdateCasualties(false, kq, kr, kb, kk, kp);
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
          CheckForClaimingDraw();
        }
      }
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
      string square;

      for (int rank = 7; rank >= 0; rank--)
      {
        emptySquares = 0;

        for (int file = 0; file < 8; file++)
        {
          square = fenLocation(rank, file);

          if (theBoard.Controls[square].AccessibleName.Equals("kKing"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "k";
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("kRook"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "r";
            kRookCount++;
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("kKnight"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "n";
            kKnightCount++;
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("kBishop"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "b";
            if (theBoard.Controls[square].Tag.ToString().Equals("w"))
            {
              kBishopWCount++;
            }
            else
            {
              kBishopKCount++;
            }
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("kQueen"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "q";
            kQueenCount++;
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("kPawn"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "p";
            kPawnCount++;
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("wKing"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "K";
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("wRook"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "R";
            wRookCount++;
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("wKnight"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "N";
            wKnightCount++;
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("wBishop"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "B";
            if (theBoard.Controls[square].Tag.ToString().Equals("w"))
            {
              wBishopWCount++;
            }
            else
            {
              wBishopKCount++;
            }
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("wQueen"))
          {
            if (emptySquares > 0)
            {
              fen += emptySquares.ToString();
              emptySquares = 0;
            }
            fen += "Q";
            wQueenCount++;
          }
          else if (theBoard.Controls[square].AccessibleName.Equals("wPawn"))
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
      //(c) king and bishop against king and bishop, with both bishops on diagonals of the same colour. 
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

      return fen;
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

    //*******************************************************************************************************
    private void makeAPuzzle_CheckedChanged(object sender, EventArgs e)
    {
      if (makeAPuzzle.Checked)
      {
        puzzleFEN = fenNotation.Text;
        ResetPieces(fenNotation.Text);
      }
      else
      {
        puzzleFEN = "";
      }
    }

    #endregion

  }
}

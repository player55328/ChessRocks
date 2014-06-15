using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace ChessRocks
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      ChessRocks app = new ChessRocks();

      if (!app.constructionError)
      {
          int x = 0;
          int y = 0;

          app.chess.GetINIValue("SETTINGS", "X", ref x, app.Location.X);
          app.chess.GetINIValue("SETTINGS", "Y", ref y, app.Location.Y);
          app.Left = x;
          app.Top = y;

          app.chess.GetINIValue("SETTINGS", "WIDTH", ref x, app.Size.Width);
          app.chess.GetINIValue("SETTINGS", "HEIGHT", ref y, app.Size.Height);
          app.Size = new Size(x, y);

          app.chess.GetINIValue("SETTINGS", "SPLIT1", ref x, app.split1.SplitterDistance);
          app.chess.GetINIValue("SETTINGS", "SPLIT2", ref y, app.split2.SplitterDistance);

          app.split1.SplitterDistance = x;
          app.split2.SplitterDistance = y;

          Application.Run(app);
      }
    }
  }
}

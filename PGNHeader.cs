using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ns_PGNHeader
{
  public partial class PGNHeader : Form
  {
    //so that the caller can get the info back...
    public string tags;
    public string whitePlayer;
    public string blackPlayer;
    public string timeStamp;

    protected string tmp;

    public PGNHeader(string TAGsIn)
    {
      InitializeComponent();

      //fill in the known tags if any.
      tags = TAGsIn;

      string[] separator = new string[] { "\r\n" };

      foreach (string line in tags.Split(separator, StringSplitOptions.None))
      {
        if (line.Length == 0) { } //skip

        else if (GetValue(line, ref Event)) continue;
        else if (GetValue(line, ref Sitee)) continue;
        else if (GetValue(line, ref Date)) continue;
        else if (GetValue(line, ref Round)) continue;
        else if (GetValue(line, ref White)) continue;
        else if (GetValue(line, ref Black)) continue;
        else if (GetValue(line, ref Result)) continue;

        else if (GetValue(line, ref WhiteTitle)) continue;
        else if (GetValue(line, ref BlackTitle)) continue;
        else if (GetValue(line, ref WhiteELO)) continue;
        else if (GetValue(line, ref BlackELO)) continue;
        else if (GetValue(line, ref WhiteUSCF)) continue;
        else if (GetValue(line, ref BlackUSCF)) continue;
        else if (GetValue(line, ref WhiteNA)) continue;
        else if (GetValue(line, ref BlackNA)) continue;
        else if (GetValue(line, ref WhiteType)) continue;
        else if (GetValue(line, ref BlackType)) continue;

        else if (GetValue(line, ref EventDate)) continue;
        else if (GetValue(line, ref EventSponsor)) continue;
        else if (GetValue(line, ref Section)) continue;
        else if (GetValue(line, ref Stage)) continue;
        else if (GetValue(line, ref Board)) continue;

        else if (GetValue(line, ref Opening)) continue;
        else if (GetValue(line, ref Variation)) continue;
        else if (GetValue(line, ref SubVariation)) continue;
        else if (GetValue(line, ref ECO)) continue;
        else if (GetValue(line, ref NIC)) continue;

        else if (GetValue(line, ref Time)) continue;
        else if (GetValue(line, ref UTCTime)) continue;
        else if (GetValue(line, ref UTCDate)) continue;
        else if (GetValue(line, ref TimeControl)) continue;

        else if (GetValue(line, ref SetUp)) continue;
        else if (GetValue(line, ref FEN)) continue;
        else if (GetValue(line, ref Termination)) continue;
        else if (GetValue(line, ref Annotator)) continue;
        else if (GetValue(line, ref Mode)) continue;
        else if (GetValue(line, ref PlyCount)) continue;

        else if (GetValue(line, ref custom1)) continue;
        else if (GetValue(line, ref custom2)) continue;
        else if (GetValue(line, ref custom3)) continue;
        else if (GetValue(line, ref custom4)) continue;
        else if (GetValue(line, ref custom5)) continue;
        else if (GetValue(line, ref custom6)) continue;

        else if (line.StartsWith("["))
        {
          //unknown tag
          unknownTags.Text += (line + "\r\n");
        }
        else if (line.StartsWith("{"))
        {
          //comment
          comments.Text += (line + "\r\n");
        }
        else
        {
          //comment
          tmp = line.Replace("}{", " : ");
          tmp = tmp.Replace("{", null);
          tmp = tmp.Replace("}", null);
          comments.Text += ("{" + tmp.Trim() + "}\r\n");
        }
      }
    }

    private bool GetValue(string input, ref TextBox tb)
    {
      if (input.StartsWith("[" + tb.Tag.ToString() + " \""))
      {
        int tagLength = tb.Tag.ToString().Length;
        int tagValueBegin = tagLength + 3;
        int tagValueLength = input.IndexOf('\"', tagValueBegin + 1) - tagValueBegin;

        if (tagValueLength > 0)
        {
          tb.Text = input.Substring(tagValueBegin, tagValueLength);
        }
        else
        {
          //seems to be a format problem so add it to the comment section as a comment
          comments.Text += ("{" + input + "}\r\n");
        }

        //whether or not there is a valid value...
        return true;
      }

      return false;
    }

    private bool GetValue(string input, ref ComboBox cb)
    {
      if (input.StartsWith("[" + cb.Tag.ToString() + " \""))
      {
        int tagLength = cb.Tag.ToString().Length;
        int tagValueBegin = tagLength + 3;
        int tagValueLength = input.IndexOf('\"', tagValueBegin + 1) - tagValueBegin;

        if (tagValueLength > 0)
        {
          cb.Text = input.Substring(tagValueBegin, tagValueLength);
        }
        else
        {
          //seems to be a format problem so add it to the comment section as a comment
          comments.Text += ("{" + input + "}\r\n");
        }

        //whether or not there is a valid value...
        return true;
      }

      return false;
    }

    private void SetUp_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (SetUp.SelectedIndex == 1)
      {
        FEN.ReadOnly = false;
      }
    }

    private void Result_TextChanged(object sender, EventArgs e)
    {
      Termination.ReadOnly = false;
    }

    private void done_Click(object sender, EventArgs e)
    {
      //create a new tag string from the user changes
      string newTags = "";

      if (Event.Text.Length > 0) newTags += ("[Event \"" + Event.Text + "\"]\r\n"); else { MessageBox.Show("'Event' is a required field..."); return; }
      if (Sitee.Text.Length > 0) newTags += ("[Site \"" + Sitee.Text + "\"]\r\n"); else { MessageBox.Show("'Site' is a required field..."); return; }
      if (Date.Text.Length > 0) newTags += ("[Date \"" + Date.Text + "\"]\r\n"); else { MessageBox.Show("'Date' is a required field..."); return; }
      if (Round.Text.Length > 0) newTags += ("[Round \"" + Round.Text + "\"]\r\n"); else { MessageBox.Show("'Round' is a required field..."); return; }
      if (White.Text.Length > 0) newTags += ("[White \"" + White.Text + "\"]\r\n"); else { MessageBox.Show("'White' is a required field..."); return; }
      if (Black.Text.Length > 0) newTags += ("[Black \"" + Black.Text + "\"]\r\n"); else { MessageBox.Show("'Black' is a required field..."); return; }
      if (Result.Text.Length > 0) newTags += ("[Result \"" + Result.Text + "\"]\r\n"); else { MessageBox.Show("'Result' is a required field..."); return; }

      whitePlayer = White.Text.Replace('.', ' ');
      blackPlayer = Black.Text.Replace('.', ' ');
      timeStamp = Date.Text.Replace('.', '_');

      if (WhiteTitle.Text.Length > 0) newTags += ("[WhiteTitle \"" + WhiteTitle.Text + "\"]\r\n");
      if (BlackTitle.Text.Length > 0) newTags += ("[BlackTitle \"" + BlackTitle.Text + "\"]\r\n");
      if (WhiteELO.Text.Length > 0) newTags += ("[WhiteElo \"" + WhiteELO.Text + "\"]\r\n");
      if (BlackELO.Text.Length > 0) newTags += ("[BlackElo \"" + BlackELO.Text + "\"]\r\n");
      if (WhiteUSCF.Text.Length > 0) newTags += ("[WhiteUSCF \"" + WhiteUSCF.Text + "\"]\r\n");
      if (BlackUSCF.Text.Length > 0) newTags += ("[BlackUSCF \"" + BlackUSCF.Text + "\"]\r\n");
      if (WhiteNA.Text.Length > 0) newTags += ("[WhiteNA \"" + WhiteNA.Text + "\"]\r\n");
      if (BlackNA.Text.Length > 0) newTags += ("[BlackNA \"" + BlackNA.Text + "\"]\r\n");
      if (WhiteType.Text.Length > 0) newTags += ("[WhiteType \"" + WhiteType.Text + "\"]\r\n");
      if (BlackType.Text.Length > 0) newTags += ("[BlackType \"" + BlackType.Text + "\"]\r\n");

      if (EventDate.Text.Length > 0) newTags += ("[EventDate \"" + EventDate.Text + "\"]\r\n");
      if (EventSponsor.Text.Length > 0) newTags += ("[EventSponsor \"" + EventSponsor.Text + "\"]\r\n");
      if (Section.Text.Length > 0) newTags += ("[Section \"" + Section.Text + "\"]\r\n");
      if (Stage.Text.Length > 0) newTags += ("[Stage \"" + Stage.Text + "\"]\r\n");
      if (Board.Text.Length > 0) newTags += ("[Board \"" + Board.Text + "\"]\r\n");

      if (Opening.Text.Length > 0) newTags += ("[Opening \"" + Opening.Text + "\"]\r\n");
      if (Variation.Text.Length > 0) newTags += ("[Variation \"" + Variation.Text + "\"]\r\n");
      if (SubVariation.Text.Length > 0) newTags += ("[SubVariation \"" + SubVariation.Text + "\"]\r\n");
      if (ECO.Text.Length > 0) newTags += ("[ECO \"" + ECO.Text + "\"]\r\n");
      if (NIC.Text.Length > 0) newTags += ("[NIC \"" + NIC.Text + "\"]\r\n");

      if (Time.Text.Length > 0) newTags += ("[Time \"" + Time.Text + "\"]\r\n");
      if (UTCTime.Text.Length > 0) newTags += ("[UTCTime \"" + UTCTime.Text + "\"]\r\n");
      if (UTCDate.Text.Length > 0) newTags += ("[UTCDate \"" + UTCDate.Text + "\"]\r\n");
      if (TimeControl.Text.Length > 0) newTags += ("[TimeControl \"" + TimeControl.Text + "\"]\r\n");

      if (SetUp.Text.Equals("1"))
      {
        if (FEN.Text.Length == 0)
        {
          MessageBox.Show("'FEN' is a required field when the 'SetUp' field is set to 1 ...");
          return;
        }
        else
        {
          newTags += ("[SetUp \"" + SetUp.Text + "\"]\r\n");
          if ((customTag3.Text.Length > 0) && (custom3.Text.Length > 0)) newTags += ("[" + customTag3.Text + " \"" + custom3.Text + "\"]\r\n");
          newTags += ("[FEN \"" + FEN.Text + "\"]\r\n");
        }
      }

      if (Termination.Text.Length > 0) newTags += ("[Termination \"" + Termination.Text + "\"]\r\n");
      if (Annotator.Text.Length > 0) newTags += ("[Annotator \"" + Annotator.Text + "\"]\r\n");
      if (Mode.Text.Length > 0) newTags += ("[Mode \"" + Mode.Text + "\"]\r\n");
      if (PlyCount.Text.Length > 0) newTags += ("[PlyCount \"" + PlyCount.Text + "\"]\r\n");

      if ((customTag1.Text.Length > 0) && (custom1.Text.Length > 0)) newTags += ("[" + customTag1.Text + " \"" + custom1.Text + "\"]\r\n");
      if ((customTag2.Text.Length > 0) && (custom2.Text.Length > 0)) newTags += ("[" + customTag2.Text + " \"" + custom2.Text + "\"]\r\n");
      if ((customTag4.Text.Length > 0) && (custom4.Text.Length > 0)) newTags += ("[" + customTag4.Text + " \"" + custom4.Text + "\"]\r\n");
      if ((customTag5.Text.Length > 0) && (custom5.Text.Length > 0)) newTags += ("[" + customTag5.Text + " \"" + custom5.Text + "\"]\r\n");
      if ((customTag6.Text.Length > 0) && (custom6.Text.Length > 0)) newTags += ("[" + customTag6.Text + " \"" + custom6.Text + "\"]\r\n");

      newTags += unknownTags.Text;

      string[] separator = new string[] { "\r\n" };
      string tmp;

      foreach (string line in comments.Text.Split(separator, StringSplitOptions.None))
      {
        tmp = line.Replace("\n", null);
        tmp = tmp.Replace("\r", null);
        tmp = tmp.Replace("}{", " : ");
        tmp = tmp.Replace("{", null);
        tmp = tmp.Replace("}", null);
        if (tmp.Length > 0) newTags += ("{" + tmp.Trim() + "}\r\n");
      }

      tags = newTags.Trim();

      //successfully completed
      DialogResult = DialogResult.OK;
      Dispose(true);
    }

  }
}

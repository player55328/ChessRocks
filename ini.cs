using System;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

namespace ns_ini
{
  public class ini
  {
    private const int maxStringLength = 256;

    StringBuilder retrunedString = new StringBuilder(maxStringLength);
    private string iniFile;

    [DllImport("KERNEL32.dll", EntryPoint = "WritePrivateProfileString")]
    public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
    [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileString")]
    protected internal static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
    [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileSection")]
    protected internal static extern int GetPrivateProfileSection(string lpAppName, byte[] lpReturnedString, int nSize, string lpFileName);

    //*******************************************************************************************************
    public ini(string iniFileName)
    {
        iniFile = iniFileName;
    }

    //*******************************************************************************
    //read everything in the section and send it back
    //
    public int GetINISectionValues(string pszSection, ArrayList pszBuf)
    {
      int nStatus; // number of characters parsed
      byte[] byteBuffer = new byte[maxStringLength];

      // Use the INI filename specified in the constructor.
      nStatus = GetPrivateProfileSection(pszSection, byteBuffer, byteBuffer.GetUpperBound(0), iniFile);

      //int BufferIndex = 0;
      if (nStatus < maxStringLength - 2)
      {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < nStatus; i++)
        {
          if (byteBuffer[i] != 0)
          {
            sb.Append((char)byteBuffer[i]);
          }
          else
          {
            if (sb.Length > 0)
            {
              pszBuf.Add(sb.ToString());
              sb = new StringBuilder();
            }
          }
        }
      }

      return nStatus;
    }

    //*******************************************************************************************************
    public bool GetINIValue(string pszSection, string pszEntry, ref string pszBuf, string Default)
    {
      return GetINIValue(pszSection, pszEntry, ref pszBuf, Default, false);
      //int Status;

      //Status = GetPrivateProfileString(pszSection, pszEntry, Default, retrunedString, maxStringLength, iniFile);
      //pszBuf = retrunedString.ToString();

      //// If one or more characters were parsed clean it up
      //if (Status > 0)
      //{
      //  pszBuf = pszBuf.ToUpper();
      //}
      ////else --not needed because of the default...

      //return true;
    }

    //*******************************************************************************************************
    public bool GetINIValue(string pszSection, string pszEntry, ref string pszBuf, string Default, bool caseSensitive)
    {
      int Status;

      Status = GetPrivateProfileString(pszSection, pszEntry, Default, retrunedString, maxStringLength, iniFile);
      pszBuf = retrunedString.ToString();

      // If one or more characters were parsed clean it up
      if (Status > 0)
      {
        if (!caseSensitive) pszBuf = pszBuf.ToUpper();
      }
      //else --not needed because of the default...

      return true;
    }

    //*******************************************************************************************************
    public bool GetINIValue(string pszSection, string pszEntry, ref int Value, int Default)
    {
      int Status;

      do
      {
          Status = GetPrivateProfileString(pszSection, pszEntry, Default.ToString(), retrunedString, maxStringLength, iniFile);

        // If one or more characters were parsed, convert the string to a int.
        if (Status > 0)
        {
          //there was a value
          try
          {
            // If one or more characters were parsed, convert the string to a int.
              Value = Convert.ToInt32(retrunedString.ToString());
            return true;
          }
          catch
          {
            ShowInvalidReadError(pszSection, pszEntry);
            Status = 0;
          }
        }

      } while (Status <= 0);

      return true;
    }


    //*******************************************************************************************************
    public bool GetINIValue(string pszSection, string pszEntry, ref double Value, double Default)
    {
      int Status;

      do
      {
          Status = GetPrivateProfileString(pszSection, pszEntry, Default.ToString(), retrunedString, maxStringLength, iniFile);

        // If one or more characters were parsed, convert the string to a int.
        if (Status > 0)
        {
          //there was a value
          try
          {
            // If one or more characters were parsed, convert the string to a int.
              Value = Convert.ToDouble(retrunedString.ToString());
            return true;
          }
          catch
          {
            ShowInvalidReadError(pszSection, pszEntry);
            Status = 0;
          }
        }

      } while (Status <= 0);

      return true;
    }

    //*******************************************************************************************************
    public bool GetINIValue(string pszSection, string pszEntry, bool Default)
    {
      if (Default)
          GetPrivateProfileString(pszSection, pszEntry, "1", retrunedString, maxStringLength, iniFile);
      else
          GetPrivateProfileString(pszSection, pszEntry, "0", retrunedString, maxStringLength, iniFile);


      //non-zero means true
      retrunedString.Remove(1, retrunedString.Length - 1);
      if (!retrunedString.ToString().Equals("0"))
        return true;

      return false;
    }

    //*******************************************************************************************************
    public bool PutINIValue(string pszSection, string pszEntry, string pszBuf)
    {
      if (pszBuf.Length > 0)
      {
        try
        {
          if (WritePrivateProfileString(pszSection, pszEntry, pszBuf, iniFile))
            return true;
        }
        catch
        {
          ShowInvalidWriteError(pszSection, pszEntry);
          return false;
        }
      }
      else
      {
        ShowInvalidWriteError(pszSection, pszEntry);
        return false;
      }

      //write function failed
      ShowWriteError(pszSection, pszEntry);
      return false;
    }

    //*******************************************************************************************************
    public bool PutINIValue(string pszSection, string pszEntry, int Value)
    {
      try
      {
          if (WritePrivateProfileString(pszSection, pszEntry, Value.ToString(), iniFile))
          return true;
      }
      catch
      {
        ShowInvalidWriteError(pszSection, pszEntry);
        return false;
      }

      //write function failed
      ShowWriteError(pszSection, pszEntry);
      return false;
    }

    //*******************************************************************************************************
    public bool PutINIValue(string pszSection, string pszEntry, double Value)
    {
      try
      {
          if (WritePrivateProfileString(pszSection, pszEntry, Value.ToString(), iniFile))
          return true;
      }
      catch
      {
        ShowInvalidWriteError(pszSection, pszEntry);
        return false;
      }

      //write function failed
      ShowWriteError(pszSection, pszEntry);
      return false;
    }


    //*******************************************************************************************************
    public bool PutINIValue(string pszSection, string pszEntry, bool Value)
    {
      try
      {
        if (Value)
        {
            if (WritePrivateProfileString(pszSection, pszEntry, "1", iniFile))
            return true;
        }
        else
        {
            if (WritePrivateProfileString(pszSection, pszEntry, "0", iniFile))
            return true;
        }
      }
      catch
      {
        ShowInvalidWriteError(pszSection, pszEntry);
        return false;
      }

      //write function failed
      ShowWriteError(pszSection, pszEntry);
      return false;
    }

    //*******************************************************************************************************
    public bool DeleteINIValue(string pszSection, string pszEntry)
    {
        if (WritePrivateProfileString(pszSection, pszEntry, null, iniFile))
        return true;

      //write function failed
      string Message = iniFile + "::" + pszSection + ":" + pszEntry + " delete failed, press OK to continue...";
      MessageBox.Show(Message);
      return false;
    }


    //*******************************************************************************************************
    private void ShowInvalidReadError(string pszSection, string pszEntry)
    {
        string Message = iniFile + "::" + pszSection + ":" + pszEntry + " value invalid, press OK to retry getting INI value...";
      MessageBox.Show(Message);
    }

    //*******************************************************************************************************
    private void ShowInvalidWriteError(string pszSection, string pszEntry)
    {
        string Message = iniFile + "::" + pszSection + ":" + pszEntry + " is invalid to write, press OK to continue...";
      MessageBox.Show(Message);
    }

    //*******************************************************************************************************
    private void ShowWriteError(string pszSection, string pszEntry)
    {
        string Message = iniFile + "::" + pszSection + ":" + pszEntry + " write failed, press OK to continue...";
      MessageBox.Show(Message);
    }

  }
}

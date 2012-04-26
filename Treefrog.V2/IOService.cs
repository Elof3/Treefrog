﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Treefrog.V2
{
    public interface IOService
    {
        string OpenFileDialog (string defaultPath);

        string SaveFileDialog (string defaultPath);
    }

    public class DefaultIOService : IOService
    {
        public string OpenFileDialog (string defaultPath)
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                InitialDirectory = defaultPath,
                Multiselect = false,
            };

            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            else
                return null;
        }

        public string SaveFileDialog (string defaultPath)
        {
            SaveFileDialog dlg = new SaveFileDialog()
            {
                InitialDirectory = defaultPath,
            };

            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            else
                return null;
        }
    }
}

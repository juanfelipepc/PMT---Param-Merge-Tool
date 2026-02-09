using System;
using System.Collections.Generic;
using SoulsFormats;

class Program
{
    static void Main()
    {
        string paramPath = @"D:\Dark Souls Mods\DSR Mods\Mine\Cheaper Shops\param\GameParam\GameParam.parambnd.dcx";
        string defPath   = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS REMASTERED\paramdef\paramdef.paramdefbnd.dcx";

        // Load all paramdefs into a dictionary
        var defBnd = BND3.Read(defPath);
        var paramDefs = new Dictionary<string, PARAMDEF>();

        foreach (var file in defBnd.Files)
        {
            try
            {
                var def = PARAMDEF.Read(file.Bytes);
                // Use the filename (without extension) as the key
                string key = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                paramDefs[key] = def;
            }
            catch
            {
                Console.WriteLine($"Skipped {file.Name} (not a valid PARAMDEF).");
            }
        }

        // Load the GameParam bundle
        var bnd = BND3.Read(paramPath);

        foreach (var file in bnd.Files)
        {
            // Each file is a PARAM
            var param = PARAM.Read(file.Bytes);

            // Match definition by filename
            string key = System.IO.Path.GetFileNameWithoutExtension(file.Name);
            if (paramDefs.TryGetValue(key, out var def))
            {
                param.ApplyParamdef(def);

                Console.WriteLine($"Applied definition to {key}: {param.Rows.Count} rows");

                // Example: print first row fields
                if (param.Rows.Count > 0)
                {
                    var row = param.Rows[0];
                    Console.WriteLine($"Row {row.ID} - {row.Name}");
                    foreach (var cell in row.Cells)
                    {
                        Console.WriteLine($"{cell.Def.InternalName}: {cell.Value}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"No definition found for {key}");
            }
        }
    }
}

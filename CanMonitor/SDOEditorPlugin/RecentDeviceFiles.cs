using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SDOEditorPlugin
{
    internal sealed class RecentDeviceFiles
    {
        private static readonly object storageLock = new object();
        private readonly string filename;
        internal string Error { get; private set; }

        internal RecentDeviceFiles(string filename)
        {
            this.filename = filename;
        }

        internal string[] Load()
        {
            lock (storageLock)
            {
                return Read();
            }
        }

        internal string[] Add(string path)
        {
            lock (storageLock)
            {
                string fullPath = Path.GetFullPath(path);
                string[] paths = new[] { fullPath }.Concat(Read())
                    .Distinct(StringComparer.OrdinalIgnoreCase).Take(10).ToArray();
                if (Error == null)
                {
                    Save(paths);
                }
                return paths;
            }
        }

        private string[] Read()
        {
            Error = null;
            try
            {
                if (!File.Exists(filename))
                {
                    return new string[0];
                }
                var paths = new List<string>();
                foreach (string line in File.ReadAllLines(filename))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    try
                    {
                        string path = Path.GetFullPath(line);
                        string extension = Path.GetExtension(path).ToLowerInvariant();
                        if (extension == ".eds" || extension == ".dcf" || extension == ".xdd" || extension == ".xml")
                        {
                            paths.Add(path);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Ignore malformed legacy entries, preserving useful history.
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
                return paths.Distinct(StringComparer.OrdinalIgnoreCase).Take(10).ToArray();
            }
            catch (IOException error)
            {
                Error = error.Message;
            }
            catch (UnauthorizedAccessException error)
            {
                Error = error.Message;
            }
            return new string[0];
        }

        private void Save(string[] paths)
        {
            string temporary = filename + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                File.WriteAllLines(temporary, paths);
                if (File.Exists(filename))
                {
                    File.Replace(temporary, filename, null);
                }
                else
                {
                    File.Move(temporary, filename);
                }
                Error = null;
            }
            catch (IOException error)
            {
                Error = error.Message;
            }
            catch (UnauthorizedAccessException error)
            {
                Error = error.Message;
            }
            finally
            {
                try
                {
                    if (File.Exists(temporary))
                    {
                        File.Delete(temporary);
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}

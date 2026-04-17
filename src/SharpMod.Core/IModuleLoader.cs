using SharpMod.Song;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpMod;

/// <summary>
/// Interface for loading tracker modules from files or streams.
/// </summary>
public interface IModuleLoader
{
    /// <summary>
    /// Load a module from a file path.
    /// </summary>
    SongModule LoadModule(string path);

    /// <summary>
    /// Load a module from a stream.
    /// </summary>
    SongModule LoadModule(Stream stream);

    /// <summary>
    /// Load a module from a named stream.
    /// </summary>
    SongModule LoadModule(string name, Stream stream);
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplaylistAdder.Models;

public struct Playlist
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Selected { get; set; }
}

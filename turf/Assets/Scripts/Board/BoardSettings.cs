using System.Collections.Generic;
using UnityEngine;

// Configuration for a single match's board.
// Tile probabilities shift as the run progresses — early matches favor Plains,
// later matches introduce more special tiles.
public class BoardSettings
{
    public int Width  { get; private set; }
    public int Height { get; private set; }
    public Dictionary<TileType, float> TileWeights { get; private set; }

    private BoardSettings() { }

    public static BoardSettings ForMatch(int matchNumber)
    {
        // Board grows slightly each match, capped at 14x14.
        int size = Mathf.Min(6 + matchNumber, 14);

        // As the run progresses, Plains weight drops and specials increase.
        float progress  = Mathf.Clamp01((matchNumber - 1) / 10f);
        float plains    = Mathf.Lerp(0.60f, 0.25f, progress);
        float forest    = Mathf.Lerp(0.10f, 0.15f, progress);
        float tower     = Mathf.Lerp(0.05f, 0.10f, progress);
        float cave      = Mathf.Lerp(0.05f, 0.10f, progress);
        float mountain  = Mathf.Lerp(0.10f, 0.12f, progress);
        float wizard    = Mathf.Lerp(0.03f, 0.08f, progress);
        float barbarian = Mathf.Lerp(0.03f, 0.08f, progress);
        float desolate  = Mathf.Lerp(0.04f, 0.12f, progress);

        return new BoardSettings
        {
            Width  = size,
            Height = size,
            TileWeights = new Dictionary<TileType, float>
            {
                { TileType.Plains,    plains    },
                { TileType.Forest,    forest    },
                { TileType.Tower,     tower     },
                { TileType.Cave,      cave      },
                { TileType.Mountain,  mountain  },
                { TileType.Wizard,    wizard    },
                { TileType.Barbarian, barbarian },
                { TileType.Desolate,  desolate  },
            }
        };
    }
}

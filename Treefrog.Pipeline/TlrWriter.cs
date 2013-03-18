﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Pipeline.Content;

namespace Treefrog.Pipeline
{
    [ContentTypeWriter]
    public class TlrWriter : ContentTypeWriter<TileRegistryContent>
    {
        protected override void Write (ContentWriter output, TileRegistryContent value)
        {
            output.Write(value.Version);

            int id = value.Id;
            TilePool pool = value.TilePool;

            output.Write((short)id);
            output.Write((short)pool.TileWidth);
            output.Write((short)pool.TileHeight);
            output.Write(value.TextureAsset);

            WritePropertyBlock(output, pool.CustomProperties);

            output.Write((short)pool.Count);
            foreach (Tile tile in pool) {
                TileCoord coord = pool.GetTileLocation(tile.Uid);

                output.Write((short)tile.Uid);
                output.Write((short)coord.X);
                output.Write((short)coord.Y);

                WritePropertyBlock(output, tile.CustomProperties);
            }
        }

        private void WritePropertyBlock (ContentWriter output, IEnumerable<Property> properties)
        {
            short pcount = 0;
            foreach (Property p in properties) {
                pcount++;
            }

            output.Write((short)pcount);
            foreach (Property p in properties) {
                output.Write(p.Name);
                output.Write(p.ToString());
            }
        }

        public override string GetRuntimeReader (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.TilesetReader, Treefrog.Runtime";
        }

        public override string GetRuntimeType (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.Tileset, Treefrog.Runtime";
        }
    }
}

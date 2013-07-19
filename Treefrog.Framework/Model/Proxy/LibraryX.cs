﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;
using System.Xml;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("Library", Namespace = LibraryX.Namespace)]
    public class LibraryX
    {
        private const string Namespace = "http://jaquadro.com/schemas/treefrog/library";

        public class PropertyGroupX
        {
            [XmlElement]
            public Guid LibraryGuid { get; set; }

            [XmlElement]
            public string LibraryName { get; set; }

            [XmlAnyElement]
            public XmlElement[] Extra { get; set; }
        }

        public class TextureGroupX
        {
            [XmlElement("Texture")]
            public List<TextureX> Textures { get; set; }
        }

        public class ObjectGroupX
        {
            [XmlElement("ObjectPool")]
            public List<ObjectPoolX> ObjectPools { get; set; }
        }

        public class TileGroupX
        {
            [XmlElement("TilePool")]
            public List<TilePoolX> TilePools { get; set; }
        }

        public class TileBrushGroupX
        {
            [XmlElement]
            public TileBrushCollectionX<StaticTileBrushX> StaticBrushes { get; set; }

            [XmlElement]
            public TileBrushCollectionX<DynamicTileBrushX> DynamicBrushes { get; set; }
        }

        public class TextureX
        {
            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Include { get; set; }

            [XmlElement]
            public TextureResource.XmlProxy TextureData { get; set; }
        }

        public class ObjectPoolX
        {
            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlArray]
            [XmlArrayItem("ObjectClass")]
            public List<RasterObjectClassX> ObjectClasses { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public List<CommonX.PropertyX> Properties { get; set; }
        }

        //[XmlRoot("ObjectClassBase", Namespace = "http://jaquadro.com/schemas/treefrog/library")]
        public class ObjectClassX
        {
            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlElement]
            public Point Origin { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public List<CommonX.PropertyX> Properties { get; set; }
        }

        //[XmlRoot("ObjectClass", Namespace = "http://jaquadro.com/schemas/treefrog/library")]
        public class RasterObjectClassX : ObjectClassX
        {
            [XmlAttribute]
            public Guid Texture { get; set; }

            [XmlElement]
            public Rectangle ImageBounds { get; set; }

            [XmlElement]
            public Rectangle MaskBounds { get; set; }
        }

        public class TilePoolX
        {
            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public int TileWidth { get; set; }

            [XmlAttribute]
            public int TileHeight { get; set; }

            [XmlAttribute]
            public Guid Texture { get; set; }

            [XmlArray]
            [XmlArrayItem("TileDef")]
            public List<TileDefX> TileDefinitions { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public List<CommonX.PropertyX> Properties { get; set; }
        }

        public class TileDefX
        {
            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute("Loc")]
            public string Location { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public List<CommonX.PropertyX> Properties { get; set; }
        }

        public class ObjectCollectionX<TProxy>
            where TProxy : ObjectClassX
        {
            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlElement("ObjectClass")]
            public List<TProxy> ObjectClasses { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public List<CommonX.PropertyX> Properties { get; set; }
        }

        public class TileBrushCollectionX<TProxy>
            where TProxy : TileBrushX
        {
            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlElement("Brush")]
            public List<TProxy> Brushes { get; set; }
        }

        public class TileBrushX
        {
            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public int TileWidth { get; set; }

            [XmlAttribute]
            public int TileHeight { get; set; }
        }

        public class StaticTileBrushX : TileBrushX
        {
            [XmlArray]
            [XmlArrayItem("Tile")]
            public List<CommonX.TileStackX> Tiles { get; set; }
        }

        public class DynamicTileBrushX : TileBrushX
        {
            [XmlAttribute]
            public string Type { get; set; }

            [XmlElement("Entry")]
            public List<BrushEntryX> Entries { get; set; }
        }

        public class BrushEntryX
        {
            [XmlAttribute]
            public int Slot { get; set; }

            [XmlAttribute]
            public Guid TileId { get; set; }
        }

        [XmlElement]
        public PropertyGroupX PropertyGroup { get; set; }

        [XmlElement]
        public TextureGroupX TextureGroup { get; set; }

        [XmlElement]
        public ObjectGroupX ObjectGroup { get; set; }

        [XmlElement]
        public TileGroupX TileGroup { get; set; }

        [XmlElement]
        public TileBrushGroupX TileBrushGroup { get; set; }
    }
}
